using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;

public class ButtonState : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Button as header")]
    public Att attribute;
    public enum Att
    {
        Normal,
        HeadingIdle,
        Flicker

    }

    [Header("Visuals")]
    [SerializeField] private Image borderImage;
    [SerializeField] private Sprite fillImage;
    private SpriteRenderer spriteRendererFill;

    [SerializeField] private TMP_Text textVal;
    [SerializeField] public Image btnImageUI;

    [Header("Internal Value")]
    [SerializeField] public bool isHovering = false;
    [SerializeField] private bool hasTriggered = false;
    [SerializeField] private float dwellTimer = 0f;

    [SerializeField] public bool isFlickering = false;
    [SerializeField] public float flickerTimes = 1f;

    [Header("Button Action")]
    [SerializeField] private OnComplete target;
    [SerializeField] private OnComplete.ActionType selectedAction;
    [SerializeField] private OnComplete.ActionType selectedAction1;

    [Header("Key Value")]
    public string value;
    public bool isDelete;
    public bool isNext;

    [Header("Button Layout")]
    [SerializeField] private RectTransform button;
    [SerializeField] private RectTransform border;
    [SerializeField] private RectTransform outer;

    [SerializeField] private float borderPadding = 2f;
    [SerializeField] private float outerPadding = 10f;


    public void Start()
    {
        if (borderImage != null) Dwell.Invisble(borderImage);
        btnImageUI = GetComponent<Image>();

        spriteRendererFill = GetComponent<SpriteRenderer>();
        if (spriteRendererFill != null && fillImage != null)
        {
            spriteRendererFill.sprite = fillImage;
            Dwell.Invisble(spriteRendererFill);
        }
        UpdateSizes();

    }


    void Update()
    {
        switch (attribute)
        {
            case Att.Normal:
                if (isHovering)
                {
                    Dwell.DwellMain(ref dwellTimer, ref hasTriggered,
                        InputSettings.Instance.dwellTime, borderImage, InputSettings.Instance.idleColor,
                        InputSettings.Instance.midColor, InputSettings.Instance.activeColor,
                        () =>
                        {
                            target.Execution(selectedAction);
                            target.Execution(selectedAction1);
                        }
                    );
                }
                else
                {

                    Dwell.ResetDwell(ref dwellTimer, ref hasTriggered, borderImage, InputSettings.Instance.idleColor);

                }
                break;
            case Att.HeadingIdle:

                break;
            case Att.Flicker:
                if (isHovering)
                {
                    target.Execution(selectedAction1);
                }
                break;
        }

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Dwell.Visble(borderImage,InputSettings.Instance.idleColor);
        Dwell.Visble(spriteRendererFill);

        isHovering = true;
        Debug.Log("Started hovering over button");

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Dwell.Invisble(borderImage);
        Dwell.Invisble(spriteRendererFill);
        isHovering = false;
        Debug.Log("Stopped hovering over button");
    }

    void UpdateSizes()
    {
        Vector2 btnSize = button.sizeDelta;

        // Border = button + 2
        border.sizeDelta = new Vector2(
            btnSize.x + borderPadding,
            btnSize.y + borderPadding
        );

        // Outer = button + 5
        outer.sizeDelta = new Vector2(
            btnSize.x + outerPadding,
            btnSize.y + outerPadding
        );
    }


}