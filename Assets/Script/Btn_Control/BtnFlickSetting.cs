using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;


public class BtnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler  
{
    [Header("Hover btn")]
    public Image outterImage;

    [Header("References")]
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
    public float step = 1f;

    [Header("Slider Settings Duration")]
    public float minDuration = 1f;
    public float maxDuration = 5f;
    public float step = 1f;

    void Start()
    {
        ResetTargetBorder();

        if (progressSlider != null && InputSettings.Instance != null)
        {
            progressSlider.minValue = minDuration;
            progressSlider.maxValue = maxDuration;
            progressSlider2.minValue = minDuration;
            progressSlider2.maxValue = maxDuration;

            float initialValue = Mathf.Round(InputSettings.Instance.dwellTime / step) * step;
            progressSlider.value = initialValue;
            UDTT(initialValue);

        }
    }

    void Update()
        { 
             if (InputSettings.Instance == null || !InputSettings.Instance.FrequencyReady)
            return;

        InputSettings s = InputSettings.Instance;

        if (progressSlider != null)
        {
            progressSlider.value = s.dwellTime;  // Set max value to dwell time
        }
        if (DwellTimeText != null)
        {
            float currentMs = timer * 1000f;
            DwellTimeText.text = Mathf.RoundToInt(currentMs) + " ms";
        }
                
        if (isHovering)
        {
            outterImage.color = s.idleColor;  // Set outer image to idle color when hovering
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / s.dwellTime);

           

            // now check if we should trigger the button action
            if (timer >= s.dwellTime && !hasTriggered)
            {
                hasTriggered = true;
                ResetState();
            }
        }
        else
        {
            ResetState();
        }

    }



    public void DwellTimeSliderChanged(float value)
    {
        // Snap to nearest step
        float steppedValue = Mathf.Round(value / step) * step;

        // Update InputSettings
        if (InputSettings.Instance != null)
            InputSettings.Instance.dwellTime = steppedValue;

        // Update slider & text
        progressSlider.value = steppedValue;
        UDTT(steppedValue);
    }

    void UDTT(float value)
    {
        if (DwellTimeText != null)
            DwellTimeText.text = Mathf.RoundToInt(value * 1000f) + " ms"; // show in ms
    }

    void ResetState()
    {
        timer = 0f;
        hasTriggered = false;
        isHovering = false;

        timer = 0f;
        hasTriggered = false;
        isHovering = false;

        if (progressSlider != null && InputSettings.Instance != null)
            progressSlider.value = InputSettings.Instance.dwellTime;

        if (DwellTimeText != null && InputSettings.Instance != null)
            DwellTimeText.text = Mathf.RoundToInt(InputSettings.Instance.dwellTime * 1000f) + " ms";

        ResetTargetBorder();

    }

    void ResetTargetBorder()
    {
        if (targetBorderImage != null && InputSettings.Instance != null)
            targetBorderImage.color = InputSettings.Instance.idleColor;
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
