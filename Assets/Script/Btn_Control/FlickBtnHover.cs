using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;


public class FlickBtnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover btn")]
    public Image outterImage;

    [Header("References")]
    public InputSettings inputSettings;
    public Image targetBorderImage;
    private bool hasTriggered = false;
    private float timer = 0f;

    private bool isHovering = false;

    [Header("Progress UI")]
    public Slider progressSlider;
    public TMP_Text DwellTimeText;

    public Slider progressSlider2;
    public TMP_Text DwellTimeText2;

    [Header("Slider Settings Frequency")]
    public float minFq = 12f;
    public float maxFq = 20f;
    public float inc = 0.2f;


    [Header("Slider Settings Duration")]
    public float minDuration = 1f;
    public float maxDuration = 5f;

    // Flicker state
    private bool isFlickering = false;
    private float flickerTimer = 0f;
    private int frameCounter = 0;
    private bool flickerState = false;

    void Start()
    {
        ResetTargetBorder();

        if (progressSlider != null && progressSlider2 != null && InputSettings.Instance != null)
        {
            progressSlider.minValue = minFq;
            progressSlider.maxValue = maxFq;
            progressSlider.value = inputSettings.flickerHz;
            UDTT(progressSlider.value);
            progressSlider.onValueChanged.AddListener(OnFrequencyChanged);

            progressSlider2.minValue = minDuration;
            progressSlider2.maxValue = maxDuration;
            progressSlider2.value = inputSettings.flickerDuration;
            UDTT2(progressSlider2.value);
            progressSlider2.onValueChanged.AddListener(OnDurationChanged);

        }
    }

    void Update()
    {
        if (InputSettings.Instance == null || !InputSettings.Instance.FrequencyReady)
            return;

        InputSettings s = InputSettings.Instance;

        if (isHovering)
        {
            outterImage.color = s.idleColor;  // Set outer image to idle color when hovering
        //start flicker for target image
        if (!isFlickering)
        {
            Flicker.StartFlicker(
                ref isFlickering,
                ref flickerTimer,
                ref frameCounter,
                ref flickerState
            );
        }

        Flicker.UpdateFlickerVisual(
            ref flickerTimer,
            ref frameCounter,
            ref flickerState,
            ref isFlickering,
            targetBorderImage,
            inputSettings.flickerOn,   // ON color
            inputSettings.idleColor,   // OFF color
            inputSettings.framesPerToggle,
            inputSettings.flickerDuration,
            () =>
            {
                // Optional: reset target color when flicker ends
                ResetTargetBorder();
            }
        );

        }
        else
        {
            ResetState();
        }

    }



  void OnFrequencyChanged(float value)
    {
        float stepped = Mathf.Round(value); // whole Hz
        inputSettings.flickerHz = stepped;

        progressSlider.SetValueWithoutNotify(stepped);
        UDTT(stepped);
    }

    void OnDurationChanged(float value)
    {
        float step = 1.0f;
        float stepped = Mathf.Round(value / step) * step;

        inputSettings.flickerDuration = stepped;

        progressSlider2.SetValueWithoutNotify(stepped);
        UDTT2(stepped);
    }

    void UDTT(float value)
    {
            if (DwellTimeText != null)
                DwellTimeText.text = value.ToString("0") + " Hz";

        
    }

    void UDTT2 (float value)
    {
        {
            if (DwellTimeText2 != null)
                DwellTimeText2.text = value.ToString("0") + " Times";
               
        }
    }

    void ResetState()
    {
        isFlickering = false;
        flickerTimer = 0f;
        frameCounter = 0;
        flickerState = false;

        ResetTargetBorder();
    }

    void ResetTargetBorder()
    {
        if (targetBorderImage != null && inputSettings != null)
            targetBorderImage.color = inputSettings.idleColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }
 
}
