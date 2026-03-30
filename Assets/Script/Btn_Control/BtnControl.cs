using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/// <summary>
/// Attach to every button prefab alongside Keyval.cs.
/// 
/// Responsibilities:
///   - Holds all per-button state (isHovering, isFlickering, timers)
///   - Reads all global settings from InputSettings.Instance
///   - Delegates dwell logic to Dwell.HandleDwell()
///   - Delegates flicker logic to Flicker static methods
///   - Exposes IsHovering / IsFlickering for OutlineController
/// 
/// Inspector assignments needed per button:
///   - borderImage  → the 120x120 outer Image
///   - innerImage   → the 100x100 inner Image
///   - uiController → the UI_fnc GameObject (shared)
/// </summary>
public class BtnControl : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Visuals")]
    public Image borderImage;
    public Image innerImage;

    [Header("References")]
    public UI_fnc uiController;

    // ── Per-button state ──────────────────────────────────────────────────────

    private bool  isHovering   = false;
    private bool  isFlickering = false;
    private bool  hasTriggered = false;
    private float timer        = 0f;

    // Flicker state
    private float flickerTimer = 0f;
    private int   frameCounter = 0;
    private bool  flickerState = false;

    // Original inner image color — captured on Start, used as flickerOff
    private Color flickerOff;

    // ── Public read-only for OutlineController ────────────────────────────────

    public bool IsHovering   => isHovering;
    public bool IsFlickering => isFlickering;

    // ── Component refs ────────────────────────────────────────────────────────

    private Keyval keyval;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        keyval = GetComponent<Keyval>();

        // Store original inner image color before any changes
        if (innerImage != null)
            flickerOff = innerImage.color;

        // Set border to idle color from global settings
        if (borderImage != null && InputSettings.Instance != null)
            borderImage.color = InputSettings.Instance.idleColor;
    }

    // ── Update ────────────────────────────────────────────────────────────────

    void Update()
    {
        // Safety: wait until InputSettings frequency is ready
        if (InputSettings.Instance == null || !InputSettings.Instance.FrequencyReady)
            return;

        InputSettings s = InputSettings.Instance;

        Dwell.HandleDwell(
            isHovering      : isHovering,
            isFlickering    : isFlickering,
            timer           : ref timer,
            hasTriggered    : ref hasTriggered,
            flickerTimer    : ref flickerTimer,
            frameCounter    : ref frameCounter,
            flickerState    : ref flickerState,
            isFlickeringRef : ref isFlickering,
            dwellTime       : s.dwellTime,
            borderImage     : borderImage,
            innerImage      : innerImage,
            idleColor       : s.idleColor,
            midColor        : s.midColor,
            activeColor     : s.activeColor,
            flickerOn       : s.flickerOn,
            flickerOff      : flickerOff,
            framesPerToggle : s.framesPerToggle,
            flickerDuration : s.flickerDuration,
            onDwellComplete : OnDwellComplete,
            onFlickerEnd    : OnFlickerEnd
        );
    }

    // ── Dwell callbacks ───────────────────────────────────────────────────────

    public void OnDwellComplete()
    {
        TriggerAction();

        // Start flicker immediately after dwell
        Flicker.StartFlicker(
            ref isFlickering,
            ref flickerTimer,
            ref frameCounter,
            ref flickerState);
    }

    void OnFlickerEnd()
    {
        // Reset border back to idle after flicker finishes
        if (borderImage != null && InputSettings.Instance != null)
            borderImage.color = InputSettings.Instance.idleColor;

        timer        = 0f;
        hasTriggered = false;
    }

    // ── Action ────────────────────────────────────────────────────────────────

    void TriggerAction()
    {
        if (keyval == null || uiController == null) return;

        if (keyval.isDelete)
            uiController.RemoveDigitLast();
        else if (keyval.isNext)
            uiController.OnNext();
        else
            uiController.AddDigit(keyval.value);
    }

    // ── Pointer Events ────────────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isFlickering)
            isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isFlickering)
        {
            TriggerAction();
            Flicker.StartFlicker(
                ref isFlickering,
                ref flickerTimer,
                ref frameCounter,
                ref flickerState);
        }
    }
}
