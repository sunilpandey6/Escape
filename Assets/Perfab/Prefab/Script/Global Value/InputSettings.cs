using UnityEngine;

public class InputSettings : MonoBehaviour
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
    [Header("Fade Settings")]
    public float fadeDurationFactor = 0.2f;

    public float detectedRefreshRate { get; private set; } = 60f;
    public int framesPerToggle { get; private set; } = 1;
    public bool FrequencyReady { get; private set; } = false;

    [Header("Border Colors")]
    public Color idleColor = new Color32(0xFF, 0xFF, 0xFF, 0xFF); // FFFFFF
    public Color midColor = new Color32(0xFF, 0xE1, 0x35, 0xFF); // FFE135
    public Color activeColor = new Color32(0x00, 0xFF, 0x00, 0xFF); // 00FF00

    [Header("Flicker Colors")]
    public Color flickerOn = new Color32(0xFF, 0xFF, 0xFF, 0xFF);

}