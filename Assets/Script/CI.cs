using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;

public class CI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public enum InputMode
    {
        Dwell,
        Flicker
    }

    [Header("Mode")]
    public InputMode inputMode = InputMode.Dwell;

    [Header("Dwell Settings")]
    public float dwellTime = 2.5f;
    private float timer = 0f;
    public bool isHovering = false;
    private bool hasTriggered = false;

    [Header("Flicker Settings")]
    public float flickerDuration = 1f;
    public float flickerHz = 15f;
    public bool isFlickering = false;
    private float flickerTimer = 0f;
    private bool flickerState = false;

    [Header("Frame-Based Flicker (Auto Calculated)")]
    private int framesPerToggle = 0;
    private int frameCounter = 0;
    private float detectedRefreshRate = 0f;

    [Header("Visuals")]
    public Image borderImage;
    public Image innerImage;

    private Color idleColor    = new Color32(0xFF, 0xFF, 0xFF, 0xFF); // FFFFFF (normal)
    private Color midColor     = new Color32(0xD3, 0xD3, 0xD3, 0xD3); // FFE135 (highlight)
    private Color activeColor  = new Color32(0x00, 0x00, 0x00, 0x00); // 00FF00 (confirmed)
    private Color flickerOn    = new Color32(0x00, 0x00, 0x00, 0x00); // FFFFFF (flash)
    private Color flickerOff;

    [Header("References")]
    public UI_fnc uiController;
    private Keyval keyval;

    // ─── INIT ─────────────────────────────────────────────────────────────────

    void Start()
    {
        Debug.Log("[CI] Start called");
        keyval = GetComponent<Keyval>();

        if (innerImage) flickerOff = innerImage.color;
        if (borderImage) borderImage.color = idleColor;

        StartCoroutine(InitializeWithDelay());
    }

    IEnumerator InitializeWithDelay()
    {
        Debug.Log("[CI] InitializeWithDelay called");
        yield return new WaitForSeconds(0.5f);
        InitializeFrequency();
    }

    void InitializeFrequency()
    {
        Debug.Log("[CI] InitializeFrequency called");
        List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(displaySubsystems);

        if (displaySubsystems.Count > 0 && displaySubsystems[0].TryGetDisplayRefreshRate(out float vrRate))
        {
            detectedRefreshRate = vrRate;
            Debug.Log($"<color=green>VR Headset Detected: {vrRate}Hz</color>");
        }
        else
        {
            detectedRefreshRate = (float)Screen.currentResolution.refreshRateRatio.value;
            Debug.Log($"<color=yellow>No VR detected. Using Monitor: {detectedRefreshRate}Hz</color>");
        }

        framesPerToggle = Mathf.RoundToInt((detectedRefreshRate / flickerHz) / 2f);
        if (framesPerToggle < 1) framesPerToggle = 1;

        Debug.Log($"Flicker: toggling every {framesPerToggle} frames for {flickerHz}Hz on {detectedRefreshRate}Hz display");
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    void Update()
    {
        Debug.Log($"[CI] Update called. inputMode={inputMode}");
        switch (inputMode)
        {
            case InputMode.Dwell:
                HandleDwell();
                break;
            case InputMode.Flicker:
                HandleDwell(); // same pipeline, flicker fires after dwell completes
                break;
        }
    }

    // ─── DWELL ────────────────────────────────────────────────────────────────

    void HandleDwell()
    {
        Debug.Log($"[CI] HandleDwell called. isFlickering={isFlickering}, isHovering={isHovering}, timer={timer}, hasTriggered={hasTriggered}");
        if (isFlickering)
        {
            UpdateFlickerVisual();
            return;
        }

        if (isHovering)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / dwellTime);

            if (borderImage)
            {
                if (progress < 0.5f)
                {
                    float t = progress / 0.5f;
                    borderImage.color = Color.Lerp(idleColor, midColor, t);
                }
                else
                {
                    float t = (progress - 0.5f) / 0.5f;
                    borderImage.color = Color.Lerp(midColor, activeColor, t);
                }
            }

            if (timer >= dwellTime && !hasTriggered)
            {
                Debug.Log("[CI] Dwell time reached, triggering action");
                TriggerAction();
                hasTriggered = true;
                StartFlicker();
            }
        }
        else
        {
            ResetState();
        }
    }

    // ─── FLICKER ──────────────────────────────────────────────────────────────

    void StartFlicker()
    {
        Debug.Log("[CI] StartFlicker called");
        isFlickering = true;
        flickerTimer = 0f;
        frameCounter = 0;
        flickerState = false;
    }

    void UpdateFlickerVisual()
    {
        Debug.Log($"[CI] UpdateFlickerVisual called. flickerTimer={flickerTimer}, frameCounter={frameCounter}, flickerState={flickerState}");
        flickerTimer += Time.deltaTime;
        frameCounter++;

        if (frameCounter >= framesPerToggle)
        {
            frameCounter = 0;
            flickerState = !flickerState;

            if (innerImage)
                innerImage.color = flickerState ? flickerOn : flickerOff;
        }

        if (flickerTimer >= flickerDuration)
            StopFlicker();
    }

    void StopFlicker()
    {
        Debug.Log("[CI] StopFlicker called");
        isFlickering = false;
        flickerState = false;
        frameCounter = 0;

        if (innerImage) innerImage.color = flickerOff;
        ResetState();
    }

    // ─── SHARED ───────────────────────────────────────────────────────────────

    void ResetState()
    {
        Debug.Log("[CI] ResetState called");
        timer = 0f;
        hasTriggered = false;
        if (borderImage) borderImage.color = idleColor;
    }

    void TriggerAction()
    {
        Debug.Log($"[CI] TriggerAction called. keyval.value={keyval?.value}, isDelete={keyval?.isDelete}, isNext={keyval?.isNext}");
        if (keyval.isDelete)
        {
            Debug.Log("[CI] TriggerAction: RemoveDigitLast");
            uiController.RemoveDigitLast();
        }
        else if (keyval.isNext)
        {
            Debug.Log("[CI] TriggerAction: OnNext");
            uiController.OnNext();
        }
        else
        {
            Debug.Log($"[CI] TriggerAction: AddDigit({keyval.value})");
            uiController.AddDigit(keyval.value);
        }
    }

    // ─── POINTER EVENTS ───────────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("[CI] OnPointerEnter called");
        if (!isFlickering) isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("[CI] OnPointerExit called");
        isHovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[CI] OnPointerClick called");
        if (!isFlickering)
        {
            TriggerAction();
            StartFlicker();
        }
    }
}
