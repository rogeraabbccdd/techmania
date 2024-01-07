﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Shared by notes and markers, this component positions
// the GameObject at the appropriate position in the workspace.
public class SelfPositionerInEditor : MonoBehaviour
{
    private static int pulsesPerScan => Pattern.pulsesPerBeat *
            EditorContext.Pattern.patternMetadata.bps;

    private void OnEnable()
    {
        PatternPanelWorkspace.RepositionNeeded += Reposition;
    }

    private void OnDisable()
    {
        PatternPanelWorkspace.RepositionNeeded -= Reposition;
    }

    public void Reposition()
    {
        Marker marker = GetComponent<Marker>();
        ScanlineInEditor scanline = GetComponent<ScanlineInEditor>();
        NoteObject noteObject = GetComponent<NoteObject>();

        float pulse;
        if (marker != null)
        {
            pulse = marker.pulse;
        }
        else if (scanline != null)
        {
            pulse = scanline.floatPulse;
        }
        else
        {
            pulse = noteObject.note.pulse;
        }
        float x = PulseToX(pulse);

        float y;
        if (marker != null)
        {
            // Don't change y.
            y = GetComponent<RectTransform>().anchoredPosition.y;
        }
        else if (scanline != null)
        {
            y = 0f;
        }
        else
        {
            y = LaneToY(noteObject.note.lane);
        }

        RectTransform rect = GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(x, y);
        if (noteObject != null)
        {
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(
                PatternPanelWorkspace.LaneHeight, 
                PatternPanelWorkspace.LaneHeight);
        }
    }

    public static Vector2 PositionOf(Note n)
    {
        return new Vector2(PulseToX(n.pulse), LaneToY(n.lane));
    }

    private static float PulseToX(float pulse)
    {
        float scan = pulse / pulsesPerScan;
        return PatternPanelWorkspace.ScanWidth * scan;
    }

    private static float LaneToY(int lane)
    {
        return -PatternPanelWorkspace.LaneHeight * (lane + 0.5f);
    }
}
