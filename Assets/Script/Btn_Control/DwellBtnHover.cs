using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;


public class DwellBtnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover btn")]
    public Image outterImage;

    [Header("References")]
    public Image targetBorderImage;
    private bool hasTriggered = false;
    private float timer = 0f;

    public InputSettings inputSettings;

    private bool isHovering = false;

    [Header("Progress UI")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text DwellTimeText;

    [Header("Slider Settings")]
    public float minDwellTime = 1f; 
    public float maxDwellTime = 5f;
    public float inc = 0.2f;

    void Start()
    {
        ResetTargetBorder();


        if (progressSlider != null && InputSettings.Instance != null)
        {
            progressSlider.minValue = minDwellTime;
            progressSlider.maxValue = maxDwellTime;

            progressSlider.value = inputSettings.dwellTime;
            progressSlider.onValueChanged.AddListener(OnDwellTimeSliderChanged);


            // console log dwell time
            UnityEngine.Debug.Log("Initial Dwell Time: " + InputSettings.Instance.dwellTime);
            UDTT(progressSlider.value);

            //progressSlider.onValueChanged.AddListener((v)=> {OnDwellTimeSliderChanged(v);});
        }
    }

    void Update()
    {
        if (inputSettings == null || !InputSettings.Instance.FrequencyReady)
            return;

        if (isHovering)
        {
            timer += Time.deltaTime;

            Dwell.DwellMain(
                ref timer,
                ref hasTriggered,
                progressSlider.value, // convert ms to seconds
                targetBorderImage,
                inputSettings.idleColor,
                inputSettings.midColor,
                inputSettings.activeColor,
                () =>
                {
                    // Action on dwell complete
                    ResetState();
                }
            );
            
            outterImage.color = inputSettings.idleColor;  // Set outer image to idle color when hovering
        }       
        // if (isHovering)
        // {
        //     outterImage.color = inputSettings.idleColor;  // Set outer image to idle color when hovering
        //     timer += Time.deltaTime;
        //     float progress = Mathf.Clamp01(timer / progressSlider.value);

        //     // update border color based on progress
        //     if (targetBorderImage != null)
        //     {
        //         if (progress < 0.5f)
        //         {
        //             float t = progress / 0.5f;
        //             targetBorderImage.color = Color.Lerp(inputSettings.idleColor, inputSettings.midColor, t);
        //         }
        //         else
        //         {
        //             float t = (progress - 0.5f) / 0.5f;
        //             targetBorderImage.color = Color.Lerp(inputSettings.midColor, inputSettings.activeColor, t);
        //         }
        //     }

        //     // now check if we should trigger the button action
        //     if (timer >= progressSlider.value && !hasTriggered)
        //     {
        //         hasTriggered = true;
        //         ResetState();
        //     }
        // }
        // else
        // {
        //     ResetState();
        // }

    }

    void OnDwellTimeSliderChanged(float value)
    {
        float step = 0.5f;
        float steppedValue = Mathf.Round(value / step) * step;

        inputSettings.dwellTime = steppedValue;

        progressSlider.SetValueWithoutNotify(steppedValue);

        UDTT(steppedValue);
    }


    void UDTT(float value)
    {
        if (DwellTimeText != null)
        DwellTimeText.text = value.ToString("0.0") + " s";
    }

    void ResetState()
    {
        timer = 0f;
        hasTriggered = false;
        isHovering = false;

        timer = 0f;
        hasTriggered = false;
        isHovering = false;

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
