﻿using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class EditOperation
{
    public enum Type
    {
        // This includes:
        // - modifying track metadata
        // - any pattern-level operation (add, delete, duplicate)
        // - modifying pattern metadata
        TrackOrPatternMetadata,
        // Add, modify or delete.
        TimeEvent,
        AddNote,
        DeleteNote,
        // Any field other than pulse and lane.
        ModifyNote,

        // Anything in setlists
        Setlist,
    }
    public Type type;

    // Metadata

    public string trackSnapsnotBeforeOp;  // Serialized
    public void TakeTrackSnapshot()
    {
        trackSnapsnotBeforeOp = EditorContext.track.Serialize(
            formatForFile: false);
    }
    // It's up to EditorContext to take appropriate snapshot
    // upon undo/redo.

    // TimeEvent

    public List<BpmEvent> bpmEventsBeforeOp;
    public List<TimeStop> timeStopsBeforeOp;
    public void TakeTimeEventSnapshot()
    {
        bpmEventsBeforeOp = new List<BpmEvent>();
        timeStopsBeforeOp = new List<TimeStop>();
        foreach (BpmEvent e in EditorContext.Pattern.bpmEvents)
        {
            bpmEventsBeforeOp.Add(e.Clone());
        }
        foreach (TimeStop t in EditorContext.Pattern.timeStops)
        {
            timeStopsBeforeOp.Add(t.Clone());
        }
    }
    // It's up to EditorContext to clone BPM events upon undo/redo.

    // AddNote

    public Note addedNote;

    // DeleteNote

    public Note deletedNote;

    // ModifyNote

    public Note noteBeforeOp;
    public Note noteAfterOp;

    // Setlist

    public string setlistSnapshotBeforeOp;
    public void TakeSetlistSnapshot()
    {
        setlistSnapshotBeforeOp = EditorContext.setlist.Serialize(
            formatForFile: false);
    }
}

// The unit of undo/redo.
public class EditTransaction
{
    public List<EditOperation> ops;
}

public class EditorContext
{
    // Parameter: starting scan
    public static System.Action previewCallback;
    public static System.Action exitCallback;
    public static bool inPreview;
    public static int previewStartingScan;

    #region Editing track
    public static Track track;
    // Full path of the track.tech file.
    public static string trackPath;
    public static string trackFolder
    {
        get
        {
            return Path.GetDirectoryName(trackPath);
        }
    }

    public static int patternIndex;
    public static Pattern Pattern
    {
        get { return track.patterns[patternIndex]; }
    }
    #endregion

    #region Editing setlist
    public static Setlist setlist;
    // Full path of the setlist.tech file.
    public static string setlistPath;
    public static string setlistFolder
    {
        get
        {
            return Path.GetDirectoryName(setlistPath);
        }
    }
    #endregion

    public static bool Dirty { get; private set; }
    private static LimitedStack<EditTransaction> undoStack;
    private static LimitedStack<EditTransaction> redoStack;
    private static EditTransaction currentTransaction;

    public static event UnityAction<bool> DirtynessUpdated;
    public static event UnityAction UndoRedoStackUpdated;
    
    // For types Metadata and BpmEvent, EditorContext has already
    // updated track/pattern before firing these events, listener
    // only needs to update UI.
    //
    // For other types, it's up to the listener to modify pattern
    // and update UI. UndoInvoked listener should reverse any
    // operation they receive; RedoInvoked listener should repeat
    // any operation they receive.
    public static event UnityAction<EditTransaction> UndoInvoked;
    public static event UnityAction<EditTransaction> RedoInvoked;

    public static void Reset()
    {
        Dirty = false;
        undoStack = new LimitedStack<EditTransaction>(20);
        redoStack = new LimitedStack<EditTransaction>(20);
    }

    #region Transaction and operation APIs
    // Modify tracks in the following pattern to enable undo and redo.
    //
    // To modify metadata:
    //   EditorContext.PrepareToModifyTrackOrPatternMetadata();
    //   (then modify metadata)
    //
    // To modify BPM events:
    //   EditorContext.PrepareToModifyBpmEvents();
    //   (then modify BPM events)
    //
    // To add, delete and modify notes:
    //   EditorContext.BeginTransaction();
    //
    //   Note newNote = (add note)
    //   EditorContext.RecordAddedNote(newNote);
    //
    //   EditorContext.RecordDeletedNote((note to delete));
    //   (delte note)
    //
    //   EditOperation op = EditorContext.BeginModifyNoteOperation();
    //   op.noteBeforeOp = (note before modification).Clone();
    //   (modify note)
    //   op.noteAfterOp = (note after modification).Clone();
    //
    //   EditorContext.EndTransaction();
    //
    // For anything in setlists:
    //
    //   EditorContext.PrepareToModifySetlist();
    //   (then modify setlist)

    public static void BeginTransaction()
    {
        currentTransaction = new EditTransaction();
        currentTransaction.ops = new List<EditOperation>();
    }

    public static void EndTransaction()
    {
        undoStack.Push(currentTransaction);
        currentTransaction = null;
        redoStack.Clear();

        Dirty = true;
        DirtynessUpdated?.Invoke(Dirty);
        UndoRedoStackUpdated?.Invoke();
    }

    // For types Metadata and BpmEvent, EditorContext will
    // automatically record the current track / list of BPM events.
    //
    // For other types, it's up to the caller to record the
    // before/after state of things.
    private static EditOperation BeginOperation(
        EditOperation.Type type)
    {
        EditOperation op = new EditOperation();
        op.type = type;
        currentTransaction.ops.Add(op);
        switch (type)
        {
            case EditOperation.Type.TrackOrPatternMetadata:
                op.TakeTrackSnapshot();
                break;
            case EditOperation.Type.TimeEvent:
                op.TakeTimeEventSnapshot();
                break;
            case EditOperation.Type.Setlist:
                op.TakeSetlistSnapshot();
                break;
        }
        return op;
    }

    // Shortcuts

    // Call this shortcut before making any change to track
    // or pattern metadata. Afterwards there's no need to call
    // anything else.
    public static void PrepareToModifyTrackOrPatternMetadata()
    {
        BeginTransaction();
        BeginOperation(EditOperation.Type.TrackOrPatternMetadata);
        EndTransaction();
    }

    // Call this shortcut before making any change to BPM events.
    // Afterwards there's no need to call anything else.
    public static void PrepareToModifyTimeEvent()
    {
        BeginTransaction();
        BeginOperation(EditOperation.Type.TimeEvent);
        EndTransaction();
    }

    // Call this shortcut between BeginTransaction and EndTransaction.
    public static void RecordAddedNote(Note n)
    {
        EditOperation op = BeginOperation(EditOperation.Type.AddNote);
        op.addedNote = n.Clone();
    }

    // Call this shortcut between BeginTransaction and EndTransaction.
    public static void RecordDeletedNote(Note n)
    {
        EditOperation op = BeginOperation(
            EditOperation.Type.DeleteNote);
        op.deletedNote = n.Clone();
    }

    public static EditOperation BeginModifyNoteOperation()
    {
        return BeginOperation(EditOperation.Type.ModifyNote);
    }

    public static void PrepareToModifySetlist()
    {
        BeginTransaction();
        BeginOperation(EditOperation.Type.Setlist);
        EndTransaction();
    }
    #endregion

    #region Saving
    // May throw exceptions.
    public static void SaveTrack()
    {
        track.SaveToFile(trackPath);
        Dirty = false;
        DirtynessUpdated?.Invoke(Dirty);
    }

    public static void SaveSetlist()
    {
        setlist.SaveToFile(setlistPath);
        Dirty = false;
        DirtynessUpdated?.Invoke(Dirty);
    }
    #endregion

    #region Undo and Redo
    public static bool CanUndo()
    {
        return !undoStack.Empty();
    }

    public static bool CanRedo()
    {
        return !redoStack.Empty();
    }

    public static void Undo()
    {
        if (undoStack.Empty()) return;

        EditTransaction transaction = undoStack.Pop();
        EditTransaction transactionToRedo = 
            ProcessTransactionAndConvertForOtherStack(transaction);
        redoStack.Push(transactionToRedo);
        UndoRedoStackUpdated?.Invoke();

        UndoInvoked?.Invoke(transaction);

        Dirty = true;
        DirtynessUpdated?.Invoke(Dirty);
    }

    public static void Redo()
    {
        if (redoStack.Empty()) return;

        EditTransaction transaction = redoStack.Pop();
        EditTransaction transactionToUndo =
            ProcessTransactionAndConvertForOtherStack(transaction);
        undoStack.Push(transactionToUndo);
        UndoRedoStackUpdated?.Invoke();

        RedoInvoked?.Invoke(transaction);

        Dirty = true;
        DirtynessUpdated?.Invoke(Dirty);
    }

    private static EditTransaction 
        ProcessTransactionAndConvertForOtherStack(
        EditTransaction input)
    {
        EditTransaction output = new EditTransaction();
        output.ops = new List<EditOperation>();
        foreach (EditOperation op in input.ops)
        {
            switch (op.type)
            {
                case EditOperation.Type.TrackOrPatternMetadata:
                    {
                        EditOperation convertedOp =
                            new EditOperation();
                        convertedOp.type = op.type;
                        convertedOp.TakeTrackSnapshot();
                        output.ops.Add(convertedOp);

                        track = TrackBase.Deserialize(
                            op.trackSnapsnotBeforeOp) as Track;
                    }
                    break;
                case EditOperation.Type.TimeEvent:
                    {
                        EditOperation convertedOp =
                            new EditOperation();
                        convertedOp.type = op.type;
                        convertedOp.TakeTimeEventSnapshot();
                        output.ops.Add(convertedOp);

                        Pattern.bpmEvents.Clear();
                        Pattern.timeStops.Clear();
                        foreach (BpmEvent e in op.bpmEventsBeforeOp)
                        {
                            Pattern.bpmEvents.Add(e.Clone());
                        }
                        foreach (TimeStop t in op.timeStopsBeforeOp)
                        {
                            Pattern.timeStops.Add(t.Clone());
                        }
                    }
                    break;
                case EditOperation.Type.Setlist:
                    {
                        EditOperation convertedOp = new EditOperation();
                        convertedOp.type = op.type;
                        convertedOp.TakeSetlistSnapshot();
                        output.ops.Add(convertedOp);

                        setlist = SetlistBase.Deserialize(
                            op.setlistSnapshotBeforeOp) as Setlist;
                    }
                    break;
                default:
                    output.ops.Add(op);
                    break;
            }
        }
        return output;
    }

    public static void ClearUndoRedoStack()
    {
        undoStack.Clear();
        redoStack.Clear();
        UndoRedoStackUpdated?.Invoke();
    }
    #endregion
}
