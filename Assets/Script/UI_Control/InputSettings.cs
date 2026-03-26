using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;

public class InputSettings: MonoBehaviour
{
    public static InputSettings Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("Dwell Settings")]
    public float dwellTime = 2.5f;

    [Header("Flicker Settings")]
    public float flickerDuration = 1f;
    public float flickerHz = 15f;

    [Header("Frequency Initialization")]
    public float initDelay = 0.5f;

    // Read-only after init — BtnControl reads these
    public float detectedRefreshRate { get; private set; } = 60f;
    public int framesPerToggle { get; private set; } = 1;
    public bool FrequencyReady { get; private set; } = false;

    [Header("Border Colors")]
    public Color idleColor = new Color32(0xFF, 0xFF, 0xFF, 0xFF); // FFFFFF
    public Color midColor = new Color32(0xFF, 0xE1, 0x35, 0xFF); // FFE135
    public Color activeColor = new Color32(0x00, 0xFF, 0x00, 0xFF); // 00FF00

    [Header("Flicker Colors")]
    public Color flickerOn = new Color32(0xFF, 0xFF, 0xFF, 0xFF);

    void Start()
    {
        StartCoroutine(InitFrequencyCoroutine());
    }

    IEnumerator InitFrequencyCoroutine()
    {
        yield return new WaitForSeconds(initDelay);

        List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(displaySubsystems);

        if (displaySubsystems.Count > 0 &&
            displaySubsystems[0].TryGetDisplayRefreshRate(out float vrRate))
        {
            detectedRefreshRate = vrRate;
            Debug.Log($"<color=green>[InputSettings] VR Headset: {vrRate}Hz</color>");
        }
        else
        {
            detectedRefreshRate = (float)Screen.currentResolution.refreshRateRatio.value;
            Debug.Log($"<color=yellow>[InputSettings] No VR. Monitor: {detectedRefreshRate}Hz</color>");
        }

        //formulka (ref hz / target hz) / 2 = frames per toggle (on/off)
        framesPerToggle = Mathf.RoundToInt((detectedRefreshRate / flickerHz) / 2f);
        if (framesPerToggle < 1) framesPerToggle = 1;

        FrequencyReady = true;
        Debug.Log($"[InputSettings] Flicker: every {framesPerToggle} frames " +
                  $"at {flickerHz}Hz on {detectedRefreshRate}Hz display");
    }


}
