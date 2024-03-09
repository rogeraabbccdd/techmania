﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TrackSetupPanel : MonoBehaviour
{
    public MessageDialog messageDialog;
    public ConfirmDialog confirmDialog;
    public Tabs tabs;

    #region Filename caching
    private List<string> audioFilesCache;
    private List<string> imageFilesCache;
    private List<string> videoFilesCache;

    private void RefreshFilenameCaches()
    {
        audioFilesCache = Paths.GetAllAudioFiles(
            EditorContext.trackFolder);
        imageFilesCache = Paths.GetAllImageFiles(
            EditorContext.trackFolder);
        videoFilesCache = Paths.GetAllVideoFiles(
            EditorContext.trackFolder);
    }
    #endregion

    #region Refreshing
    private void OnEnable()
    {
        if (!EditorContext.Dirty)
        {
            // Load full track from disk. If the editor is dirty,
            // however, we don't want to overwrite the in-memory
            // track with unsaved changes.
            EditorContext.track = Track.LoadFromFile(
                EditorContext.trackPath) as Track;
        }

        Tabs.tabChanged += Refresh;
        EditorContext.UndoInvoked += OnUndoOrRedo;
        EditorContext.RedoInvoked += OnUndoOrRedo;
        PatternRadioList.SelectedPatternChanged += 
            SelectedPatternChanged;

        selectedPattern = null;
        EditorContext.ClearUndoRedoStack();
        RefreshFilenameCaches();
        Refresh();

        DiscordController.SetActivity(DiscordActivityType.EditorTrack);
    }

    private void OnDisable()
    {
        Tabs.tabChanged -= Refresh;
        EditorContext.UndoInvoked -= OnUndoOrRedo;
        EditorContext.RedoInvoked -= OnUndoOrRedo;
        PatternRadioList.SelectedPatternChanged -= 
            SelectedPatternChanged;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            RefreshFilenameCaches();
            Refresh();
        }
    }

    private void OnUndoOrRedo(EditTransaction t)
    {
        Refresh();
    }

    private void Refresh()
    {
        switch (tabs.CurrentTab)
        {
            case 0:
                RefreshResourcesTab();
                break;
            case 1:
                RefreshMetadataTab();
                break;
            case 2:
                RefreshPatternsTab();
                break;
        }
    }
    #endregion

    #region Resources tab
    [Header("Resources tab")]
    public TextMeshProUGUI audioFilesDisplay;
    public TextMeshProUGUI imageFilesDisplay;
    public TextMeshProUGUI videoFilesDisplay;

    public void OnImportButtonClick()
    {
        EditorUtilities.ImportResource(new string[]
            {
                "wav", "ogg", "jpg", "png", "mp4", "wmv"
            },
            copyDestinationFolder: EditorContext.trackFolder,
            messageDialog, confirmDialog,
            completeCopyCallback: () =>
            {
                RefreshFilenameCaches();
                RefreshResourcesTab();
            });
    }

    private string CondenseFileList(List<string> fullPaths)
    {
        return EditorUtilities.CondenseFileList(fullPaths, 
            EditorContext.trackFolder);
    }

    public void RefreshResourcesTab()
    {
        audioFilesDisplay.text = CondenseFileList(
            audioFilesCache);
        imageFilesDisplay.text = CondenseFileList(
            imageFilesCache);
        videoFilesDisplay.text = CondenseFileList(
            videoFilesCache);
    }
    #endregion

    #region Metadata tab
    [Header("Metadata tab")]
    public TMP_InputField trackTitle;
    public TMP_InputField artist;
    public TMP_InputField genre;
    public TMP_InputField additionalCredits;
    public EyecatchSelfLoader eyecatchPreview;
    public TMP_Dropdown eyecatchImage;
    public TMP_Dropdown previewTrack;
    public TMP_InputField startTime;
    public TMP_InputField endTime;
    public PreviewTrackPlayer previewTrackPlayer;
    public TMP_Dropdown previewBga;

    public void RefreshMetadataTab()
    {
        TrackMetadata metadata = EditorContext.track.trackMetadata;

        trackTitle.SetTextWithoutNotify(metadata.title);
        artist.SetTextWithoutNotify(metadata.artist);
        genre.SetTextWithoutNotify(metadata.genre);
        additionalCredits.SetTextWithoutNotify(
            metadata.additionalCredits);

        UIUtils.MemoryToDropdown(eyecatchImage,
            metadata.eyecatchImage, imageFilesCache,
            EditorContext.trackFolder);
        UIUtils.MemoryToDropdown(previewTrack,
            metadata.previewTrack, audioFilesCache,
            EditorContext.trackFolder);
        startTime.SetTextWithoutNotify(metadata.previewStartTime.ToString());
        endTime.SetTextWithoutNotify(metadata.previewEndTime.ToString());
        UIUtils.MemoryToDropdown(previewBga,
            metadata.previewBga, videoFilesCache,
            EditorContext.trackFolder);

        foreach (TMP_InputField field in new List<TMP_InputField>()
        {
            trackTitle,
            artist,
            genre,
            additionalCredits,
            startTime,
            endTime
        })
        {
            field.GetComponent<MaterialTextField>().RefreshMiniLabel();
        }

        RefreshEyecatchPreview();
    }

    public void OnMetadataUpdated()
    {
        TrackMetadata metadata = EditorContext.track.trackMetadata;
        bool madeChange = false;

        UIUtils.UpdateTrackOrPatternMetadataInMemory(
            ref metadata.title, trackTitle.text, ref madeChange);
        UIUtils.UpdateTrackOrPatternMetadataInMemory(
            ref metadata.artist, artist.text, ref madeChange);
        UIUtils.UpdateTrackOrPatternMetadataInMemory(
            ref metadata.genre, genre.text, ref madeChange);
        UIUtils.UpdateTrackOrPatternMetadataInMemory(
            ref metadata.additionalCredits,
            additionalCredits.text, ref madeChange);

        UIUtils.UpdateTrackOrPatternMetadataInMemory(
            ref metadata.eyecatchImage, eyecatchImage, ref madeChange);
        UIUtils.UpdateTrackOrPatternMetadataInMemory(
            ref metadata.previewTrack, previewTrack, ref madeChange);
        UIUtils.ClampInputField(startTime, 0.0, double.MaxValue);
        UIUtils.UpdateTrackOrPatternMetadataInMemory(
            ref metadata.previewStartTime, startTime.text, ref madeChange);
        UIUtils.ClampInputField(endTime, 0.0, double.MaxValue);
        UIUtils.UpdateTrackOrPatternMetadataInMemory(
            ref metadata.previewEndTime, endTime.text, ref madeChange);
        UIUtils.UpdateTrackOrPatternMetadataInMemory(
            ref metadata.previewBga, previewBga, ref madeChange);
    }

    public void OnEyecatchUpdated()
    {
        RefreshEyecatchPreview();
    }

    public void RefreshEyecatchPreview()
    {
        eyecatchPreview.LoadImage(EditorContext.trackFolder,
            EditorContext.track.trackMetadata);
    }

    public void OnPlayPreviewButtonClick()
    {
        previewTrackPlayer.Play(
            EditorContext.trackFolder,
            EditorContext.track.trackMetadata,
            loop: false);
    }

    public void OnDeleteTrackButtonClick()
    {
        confirmDialog.Show(
            L10n.GetStringAndFormatIncludingPaths(
                "track_setup_metadata_tab_delete_track_confirmation",
                EditorContext.trackFolder),
            L10n.GetString(
                "track_setup_metadata_tab_delete_track_confirm"),
            L10n.GetString(
                "track_setup_metadata_tab_delete_track_cancel"),
            () =>
            {
                // Delete from disk
                Directory.Delete(EditorContext.trackFolder,
                    recursive: true);

                // Delete from RAM
                string parent = Path.GetDirectoryName(
                    EditorContext.trackFolder);
                GlobalResource.trackList[parent].RemoveAll(
                    (GlobalResource.TrackInFolder t) =>
                {
                    return t.folder == EditorContext.trackFolder;
                });

                GetComponentInChildren<
                    CustomTransitionFromTrackSetupPanel>().
                Transition();
            });
    }

    #endregion

    #region Patterns tab
    [Header("Patterns tab")]
    public PatternRadioList patternList;
    public Toggle autoOrderPatterns;
    public GameObject patternMetadata;
    public GameObject patternButtons;
    public GameObject noPatternSelectedNotice;
    public List<GameObject> orderControls;
    public TMP_InputField patternName;
    public TMP_InputField patternAuthor;
    public TMP_Dropdown controlScheme;
    public TMP_InputField patternLevel;
    public TMP_Dropdown playableLanes;
    public TMP_Dropdown patternBackingTrack;
    public TMP_Dropdown backgroundImage;
    public TMP_Dropdown backgroundVideo;
    public TMP_InputField bgaOffset;
    public Toggle waitForEndOfBga;
    public Toggle playBgaOnLoop;
    public TransitionToPanel transitionToPatternPanel;

    // Keep a reference to the selected pattern, so we can
    // re-select it when the pattern list refreshes.
    //
    // However this reference will be invalidated upon undo/redo.
    // Must refresh it upon undo/redo.
    private Pattern selectedPattern;

    private void RefreshPatternsTab()
    {
        if (selectedPattern != null)
        {
            // This ensures selectedPattern still points to a pattern
            // in EditorContext.track even after undo and redo.
            selectedPattern = EditorContext.track.FindPatternByGuid(
                selectedPattern.patternMetadata.guid);
        }
        RefreshPatternList();
        RefreshPatternMetadata();
    }

    private void RefreshPatternList()
    {
        patternList.InitializeAndReturnFirstPatternObject(
            EditorContext.track, records: null,
            initialSelectedPattern: selectedPattern);
        autoOrderPatterns.SetIsOnWithoutNotify(
            EditorContext.track.trackMetadata.autoOrderPatterns);
    }

    private void RefreshPatternMetadata()
    {
        foreach (GameObject o in orderControls)
        {
            o.SetActive(
                !EditorContext.track.trackMetadata.autoOrderPatterns);
        }

        patternMetadata.SetActive(selectedPattern != null);
        patternButtons.SetActive(selectedPattern != null);
        noPatternSelectedNotice.SetActive(selectedPattern == null);
        if (selectedPattern == null) return;

        PatternMetadata m = selectedPattern.patternMetadata;
        patternName.SetTextWithoutNotify(m.patternName);
        patternAuthor.SetTextWithoutNotify(m.author);
        controlScheme.options.Clear();
        controlScheme.options.Add(new TMP_Dropdown.OptionData(
            L10n.GetString("control_scheme_touch")));
        controlScheme.options.Add(new TMP_Dropdown.OptionData(
            L10n.GetString("control_scheme_keys")));
        controlScheme.options.Add(new TMP_Dropdown.OptionData(
            L10n.GetString("control_scheme_km")));
        controlScheme.SetValueWithoutNotify((int)m.controlScheme);
        controlScheme.RefreshShownValue();
        patternLevel.SetTextWithoutNotify(m.level.ToString());
        playableLanes.SetValueWithoutNotify(m.playableLanes - 2);
        playableLanes.RefreshShownValue();

        UIUtils.MemoryToDropdown(patternBackingTrack,
            m.backingTrack, audioFilesCache,
            EditorContext.trackFolder);
        UIUtils.MemoryToDropdown(backgroundImage,
            m.backImage, imageFilesCache,
            EditorContext.trackFolder);
        UIUtils.MemoryToDropdown(backgroundVideo,
            m.bga, videoFilesCache,
            EditorContext.trackFolder);
        bgaOffset.SetTextWithoutNotify(m.bgaOffset.ToString());
        waitForEndOfBga.SetIsOnWithoutNotify(m.waitForEndOfBga);
        waitForEndOfBga.interactable = !m.playBgaOnLoop;
        playBgaOnLoop.SetIsOnWithoutNotify(m.playBgaOnLoop);

        foreach (TMP_InputField field in new List<TMP_InputField>()
        {
            patternName,
            patternAuthor,
            patternLevel,
            bgaOffset
        })
        {
            field.GetComponent<MaterialTextField>().RefreshMiniLabel();
        }
    }

    private void SelectedPatternChanged(Pattern newSelection)
    {
        selectedPattern = newSelection;
        RefreshPatternMetadata();
    }

    public void OnAutoOrderPatternChanged()
    {
        EditorContext.PrepareToModifyTrackOrPatternMetadata();
        EditorContext.track.trackMetadata.autoOrderPatterns =
            !EditorContext.track.trackMetadata.autoOrderPatterns;
        if (EditorContext.track.trackMetadata.autoOrderPatterns)
        {
            EditorContext.track.SortPatterns();
        }
        RefreshPatternList();
        RefreshPatternMetadata();
    }

    public void OnPatternMetadataChanged()
    {
        PatternMetadata m = selectedPattern.patternMetadata;
        bool madeChange = false;

        UIUtils.UpdateTrackOrPatternMetadataInMemory(ref m.patternName,
            patternName.text, ref madeChange);
        UIUtils.UpdateTrackOrPatternMetadataInMemory(ref m.author,
            patternAuthor.text, ref madeChange);
        UIUtils.ClampInputField(patternLevel,
            Pattern.minLevel, int.MaxValue);
        UIUtils.UpdateTrackOrPatternMetadataInMemory(ref m.level,
            patternLevel.text, ref madeChange);

        // Special handling for control scheme
        if ((int)m.controlScheme != controlScheme.value)
        {
            if (!madeChange)
            {
                EditorContext.PrepareToModifyTrackOrPatternMetadata();
                madeChange = true;
            }
            m.controlScheme = (ControlScheme)controlScheme.value;
        }

        // Special handling for playable lanes
        if (m.playableLanes != playableLanes.value + 2)
        {
            if (!madeChange)
            {
                EditorContext.PrepareToModifyTrackOrPatternMetadata();
                madeChange = true;
            }
            m.playableLanes = playableLanes.value + 2;
        }

        UIUtils.UpdateTrackOrPatternMetadataInMemory(ref m.backingTrack,
            patternBackingTrack, ref madeChange);
        UIUtils.UpdateTrackOrPatternMetadataInMemory(
            ref m.backImage, backgroundImage, ref madeChange);
        UIUtils.UpdateTrackOrPatternMetadataInMemory(
            ref m.bga, backgroundVideo, ref madeChange);
        UIUtils.UpdateTrackOrPatternMetadataInMemory(
            ref m.bgaOffset, bgaOffset.text, ref madeChange);
        UIUtils.UpdateTrackOrPatternMetadataInMemory(
            ref m.waitForEndOfBga, waitForEndOfBga.isOn, ref madeChange);
        UIUtils.UpdateTrackOrPatternMetadataInMemory(
            ref m.playBgaOnLoop, playBgaOnLoop.isOn, ref madeChange);

        // Disable waitForEndOfBga if playBgaOnLoop is turned on.
        if (m.playBgaOnLoop)
        {
            m.waitForEndOfBga = false;
            waitForEndOfBga.SetIsOnWithoutNotify(false);
            waitForEndOfBga.interactable = false;
        }
        else
        {
            waitForEndOfBga.interactable = true;
        }

        if (madeChange)
        {
            if (EditorContext.track.trackMetadata.autoOrderPatterns)
            {
                EditorContext.track.SortPatterns();
            }
            RefreshPatternList();
        }
    }

    public void OnNewPatternButtonClick()
    {
        EditorContext.PrepareToModifyTrackOrPatternMetadata();
        EditorContext.track.patterns.Add(new Pattern());
        if (EditorContext.track.trackMetadata.autoOrderPatterns)
        {
            EditorContext.track.SortPatterns();
        }

        RefreshPatternList();
        RefreshPatternMetadata();
    }

    public void OnDeletePatternButtonClick()
    {
        // This is undoable, so no need for confirmation.
        EditorContext.PrepareToModifyTrackOrPatternMetadata();
        EditorContext.track.patterns.Remove(selectedPattern);

        selectedPattern = null;
        RefreshPatternList();
        RefreshPatternMetadata();
    }

    public void OnDuplicatePatternButtonClick()
    {
        EditorContext.PrepareToModifyTrackOrPatternMetadata();
        EditorContext.track.patterns.Add(
            selectedPattern.CloneWithDifferentGuid());
        if (EditorContext.track.trackMetadata.autoOrderPatterns)
        {
            EditorContext.track.SortPatterns();
        }

        RefreshPatternList();
        RefreshPatternMetadata();
    }

    public void OnOpenPatternButtonClick()
    {
        int index = EditorContext.track.FindPatternIndexByGuid(
            selectedPattern.patternMetadata.guid);
        if (index < 0) return;
        EditorContext.patternIndex = index;

        transitionToPatternPanel.Invoke();
    }

    private void MoveSelectedPattern(int oldIndex, int newIndex)
    {
        EditorContext.track.patterns.RemoveAt(oldIndex);
        EditorContext.track.patterns.Insert(newIndex, selectedPattern);
        RefreshPatternList();
        RefreshPatternMetadata();
    }

    public void OnMovePatternUpButtonClick()
    {
        EditorContext.PrepareToModifyTrackOrPatternMetadata();
        int index = EditorContext.track.FindPatternIndexByGuid(
            selectedPattern.patternMetadata.guid);
        int newIndex = index - 1;
        if (newIndex < 0)
        {
            newIndex = EditorContext.track.patterns.Count - 1;
        }

        MoveSelectedPattern(index, newIndex);
    }

    public void OnMovePatternDownButtonClick()
    {
        EditorContext.PrepareToModifyTrackOrPatternMetadata();
        int index = EditorContext.track.FindPatternIndexByGuid(
            selectedPattern.patternMetadata.guid);
        int newIndex = index + 1;
        if (newIndex >= EditorContext.track.patterns.Count)
        {
            newIndex = 0;
        }

        MoveSelectedPattern(index, newIndex);
    }
    #endregion
}
