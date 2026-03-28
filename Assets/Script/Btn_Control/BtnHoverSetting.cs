using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
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
        targetBorderImage = outterImage; // Use outterImage as the target border for dwell feedback
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
            progressSlider.value,
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
            outterImage.color = inputSettings.idleColor; // Set outer image to idle color when hovering
        }
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
        ResetTargetBorder();
    }
    private IEnumerator FadeIn()
    {
        // Correct reference to dwellTime and fadeDurationFactor
        float fadeDuration = Mathf.Min(inputSettings.dwellTime * inputSettings.fadeDurationFactor, 1f); // Make the fade duration faster than the dwell time
        float timeElapsed = 0f;
        Color currentColor = outterImage.color; // Use targetBorderImage here (or targetImage if needed)
        while (timeElapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, timeElapsed / fadeDuration);
            outterImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        outterImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1f); // Ensure it ends at full opacity
    }

    private IEnumerator FadeOut()

    {

        // Correct reference to dwellTime and fadeDurationFactor

        float fadeDuration = Mathf.Min(inputSettings.dwellTime * inputSettings.fadeDurationFactor, 1f); // Make the fade duration faster than the dwell time

        float timeElapsed = 0f;

        Color currentColor = outterImage.color; // Use targetBorderImage here (or targetImage if needed)



        while (timeElapsed < fadeDuration)

        {

            float alpha = Mathf.Lerp(1f, 0f, timeElapsed / fadeDuration);

            outterImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);

            timeElapsed += Time.deltaTime;

            yield return null;

        }



        outterImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0f); // Ensure it ends at fully transparent

    }



    void ResetTargetBorder()

    {

        if (targetBorderImage != null && InputSettings.Instance != null)

            targetBorderImage.color = InputSettings.Instance.idleColor;

    }



    public void OnPointerEnter(PointerEventData eventData)

    {

        isHovering = true;

        StartCoroutine(FadeIn());

    }



    public void OnPointerExit(PointerEventData eventData)

    {

        isHovering = false;

        StartCoroutine(FadeOut());

    }

}