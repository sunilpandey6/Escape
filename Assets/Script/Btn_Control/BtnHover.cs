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
    public Text progressText;

    void Start()
    {
        ResetTargetBorder();
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
        if (progressText != null)
        {
            float currentMs = timer * 1000f;
            progressText.text = Mathf.RoundToInt(currentMs) + " ms";
        }

        if (isHovering)
        {
            
            outterImage.color = s.idleColor;  // Set outer image to idle color when hovering
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / s.dwellTime);

            // update border color based on progress
            if (targetBorderImage != null)
            {
                if (progress < 0.5f)
                {
                    float t = progress / 0.5f;
                    targetBorderImage.color = Color.Lerp(s.idleColor, s.midColor, t);
                }
                else
                {
                    float t = (progress - 0.5f) / 0.5f;
                    targetBorderImage.color = Color.Lerp(s.midColor, s.activeColor, t);
                }
            }

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

    void ResetState()
    {
        timer = 0f;
        hasTriggered = false;
        isHovering = false;

        if (progressSlider != null)
            progressSlider.value = s.dwellTime;  // Set to max to show full progress when reset

        if (progressText != null)
            progressText.text = s.dwellTime * 1000f + " ms";  // Show full dwell time in ms when reset

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
