using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalInput : MonoBehaviour
{
    public static GlobalInput Instance { get; private set; }
    // Start is called before the first frame update
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
    public float flickerDuration = 3f;
    public float flickerHz = 15f;


    [Header("Frequency Initialization")]
    public float initDelay = 0.5f;
    [Header("Fade Settings")]
    public float fadeDurationFactor = 0.2f;

    [Header("Border Colors")]
    public Color idleColor = new Color32(0xFF, 0xFF, 0xFF, 0xFF); // FFFFFF
    public Color midColor = new Color32(0xFF, 0xE1, 0x35, 0xFF); // FFE135
    public Color activeColor = new Color32(0x00, 0xFF, 0x00, 0xFF); // 00FF00

    [Header("Flicker Colors")]
    public Color flickerOn = new Color32(0x00, 0x00, 0x00, 0x00);


}
