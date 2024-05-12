﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MaterialButton : MonoBehaviour,
    ISelectHandler, IDeselectHandler, ISubmitHandler,
    IPointerEnterHandler, IPointerDownHandler, IPointerClickHandler
{
    public Color textColor;
    public Color disabledTextColor;
    public Color buttonColor;
    public Color disabledButtonColor;

    public GameObject selectedOutline;
    public bool useClickSoundOverride;
    public AudioClip clickSoundOverride;
    public bool noSelectSound;
    public bool noClickSound;
    private FmodSoundWrap clickSoundOverrideFmod;

    private Button button;
    private Image buttonImage;
    private Graphic buttonContent;
    private RectTransform rippleRect;
    private RectTransform rippleParentRect;
    private Animator rippleAnimator;
    private bool interactable;
    private bool selected;
    private bool isBackButton;

    // Start is called before the first frame update
    void Start()
    {
        if (useClickSoundOverride && clickSoundOverride != null)
        {
            clickSoundOverrideFmod = FmodManager
                .CreateSoundFromAudioClip(
                    clickSoundOverride);
        }

        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        foreach (Graphic g in GetComponentsInChildren<Graphic>())
        {
            if (g.transform != transform)
            {
                buttonContent = g;
                break;
            }
        }
        buttonContent.color = textColor;
        rippleAnimator = GetComponentInChildren<Animator>();
        rippleRect = rippleAnimator.GetComponent<RectTransform>();
        rippleParentRect = rippleRect.parent
            .GetComponent<RectTransform>();
        interactable = true;
        isBackButton = GetComponent<BackButton>() != null;
    }

    // Update is called once per frame
    void Update()
    {
        bool newInteractable = button.IsInteractable();
        if (newInteractable != interactable)
        {
            buttonContent.color = newInteractable ?
                textColor : disabledTextColor;
            buttonImage.color = newInteractable ?
                buttonColor : disabledButtonColor;
        }
        interactable = newInteractable;
    }

    private void StartRippleAt(Vector2 startPosition)
    {
        rippleRect.anchoredPosition = startPosition;
        rippleAnimator.SetTrigger("Activate");
    }

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;
        selectedOutline.SetActive(selected);

        if (eventData is AxisEventData && !noSelectSound)
        {
            // Only play sound if selected with keyboard navigation.
            MenuSfx.instance.PlaySelectSound();
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
        selectedOutline.SetActive(selected);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        StartRippleAt(Vector2.zero);

        if (useClickSoundOverride && !noClickSound)
        {
            MenuSfx.instance.PlaySound(clickSoundOverrideFmod);
            return;
        }

        if (!noClickSound)
        {
            if (isBackButton)
            {
                MenuSfx.instance.PlayBackSound();
            }
            else
            {
                MenuSfx.instance.PlayClickSound();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable)
        {
            return;
        }

        if (useClickSoundOverride && !noClickSound)
        {
            MenuSfx.instance.PlaySound(clickSoundOverrideFmod);
            return;
        }

        if (!noClickSound)
        {
            if (isBackButton)
            {
                MenuSfx.instance.PlayBackSound();
            }
            else
            {
                MenuSfx.instance.PlayClickSound();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TouchInducedPointer.EventIsFromActualMouse(eventData))
        {
            if (interactable && !noSelectSound)
            {
                MenuSfx.instance.PlaySelectSound();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable)
        {
            return;
        }

        Vector2 pointerPosition = eventData.position;
        Vector2 rippleStartPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rippleParentRect, pointerPosition, null, out rippleStartPosition))
        {
            StartRippleAt(rippleStartPosition);
        }
    }
}
