using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BB : MonoBehaviour
{
    [Header("Button Type")]
    [SerializeField] private Button button;
    public Att attribute;
    public enum Att
    {
        None,
        Normal,
        DwellDemo,
        FlickerDemo
    }

    enum State
    {
        Idle,
        Hovering,
        Dwelling,
        Flickering,
    }
    [SerializeField] private State currentState = State.Idle;

    [Header("Button as header")]
    [SerializeField] private Image outlineImage;
    [SerializeField] private Image borderImage;
    [SerializeField] private Image buttonImage;

    [SerializeField] private RectTransform outlineRect;
    [SerializeField] private RectTransform borderRect;
    [SerializeField] private RectTransform buttonRect;

    private Material runtimeMaterial;
    private Material runtimeMaterialFlicker;

    [Header("Internal Value")]
    public bool isHovering = false;
    [SerializeField] private bool hasTriggered = false;
    [SerializeField] private float dwellTimer = 0f;

    // Removed: flickerTimes, framesPerCycle, halfCycle — no longer needed

    // Time-based flicker anchor
    private float flickerStartTime = -1f;

    [Header("Outline and Border Settings")]
    [SerializeField] private float outlineSize = 10f;
    [SerializeField] private float borderSize = 3f;

    [Header("Button Action")]
    [SerializeField] private ActionType selectedAction;

    [Header("UI Control reference")]
    public string value;
    public bool isDelete;
    public bool isNext;
    public TestUI testUI;

    public enum ActionType
    {
        None,
        Flicker,
        ResetDwell,
        TestUI
    }

    #region Unity Lifecycle

    public void Awake()
    {
        if (outlineImage)
        {
            outlineRect.sizeDelta = buttonRect.sizeDelta + new Vector2(outlineSize * 2, outlineSize * 2);
            runtimeMaterial = Instantiate(outlineImage.material);
            outlineImage.material = runtimeMaterial;
            ApplyGlobalColors();
            outlineImage.gameObject.SetActive(false);
        }

        if (borderImage)
        {
            borderRect.sizeDelta = buttonRect.sizeDelta + new Vector2(borderSize * 2, borderSize * 2);
        }

        if (buttonImage)
        {
            runtimeMaterialFlicker = Instantiate(buttonImage.material);
            buttonImage.material = runtimeMaterialFlicker;
            ApplyFlickerColors();
        }

        if (!button)
            button = GetComponent<Button>();

        // framesPerCycle / halfCycle removed — time-based approach needs nothing pre-computed
    }

    void ApplyGlobalColors()
    {
        runtimeMaterial.SetColor("_IdleColor",   GlobalInput.Instance.idleColor);
        runtimeMaterial.SetColor("_MidColor",    GlobalInput.Instance.midColor);
        runtimeMaterial.SetColor("_ActiveColor", GlobalInput.Instance.activeColor);
    }

    void ApplyFlickerColors()
    {
        runtimeMaterialFlicker.SetColor("_IdleColor",    GlobalInput.Instance.idleColor);
        runtimeMaterialFlicker.SetColor("_FlickerColor", GlobalInput.Instance.flickerOn);
    }

    public void Update()
    {
        switch (attribute)
        {
            case Att.None:
                HandleNone();
                break;
            case Att.Normal:
                if (isHovering && currentState != State.Flickering)
                    ChangeColor();
                break;
            case Att.DwellDemo:
                HandleDwell();
                break;
            case Att.FlickerDemo:
                HandleFlickerDemo();
                break;
        }

        if (currentState == State.Flickering)
            UpdateFlicker();
    }

    #endregion

    #region State Control

    void HandleNone()
    {
        if (outlineImage && !outlineImage.gameObject.activeSelf)
        {
            outlineImage.gameObject.SetActive(true);
            outlineImage.color = Color.yellow;
        }
    }

    void HandleDwell()
    {
        if (currentState == State.Hovering || currentState == State.Dwelling)
        {
            currentState = State.Dwelling;

            dwellTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(dwellTimer / GlobalInput.Instance.dwellTime);
            runtimeMaterial.SetFloat("_Progress", progress);

            if (progress >= 1f && currentState != State.Flickering && !hasTriggered)
            {
                hasTriggered = true;
                StartCoroutine(FlickerAndExecute());
            }
        }
    }

    void HandleFlickerDemo()
    {
        if (isHovering)
        {
            currentState = State.Flickering;
            flickerStartTime = -1f;
        }
        else
        {
            currentState = State.Idle;
            flickerStartTime = -1f;
            if (runtimeMaterialFlicker != null)
                runtimeMaterialFlicker.SetFloat("_FlickerState", 0f);
        }
    }

    #endregion

    #region Dwell Functions

    public void ChangeColor()
    {
        if (outlineImage && !outlineImage.gameObject.activeSelf)
            outlineImage.gameObject.SetActive(true);

        if (outlineImage)
        {
            dwellTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(dwellTimer / GlobalInput.Instance.dwellTime);
            runtimeMaterial.SetFloat("_Progress", progress);

            if (dwellTimer >= GlobalInput.Instance.dwellTime && !hasTriggered)
            {
                hasTriggered = true;
                StartCoroutine(FlickerAndExecute());
            }
        }
    }

    private IEnumerator FlickerAndExecute()
    {
        ExperimentLogger.Instance?.LogEvent("Dwell_Complete", $"Button: {gameObject.name}");
        
        currentState = State.Flickering;
        hasTriggered = true;
        
        ExperimentLogger.Instance?.LogEvent("Flicker_Start", $"Button: {gameObject.name}, Hz: {GlobalInput.Instance.flickerHz}");

        yield return new WaitForSeconds(GlobalInput.Instance.flickerDuration);

        currentState = State.Idle;
        flickerStartTime = -1f;

        if (runtimeMaterialFlicker != null)
            runtimeMaterialFlicker.SetFloat("_FlickerState", 0f);

        ExperimentLogger.Instance?.LogEvent("Flicker_End", $"Button: {gameObject.name}");

        Execution(selectedAction);
        button?.onClick.Invoke();
    }

    public void ResetColor()
    {
        dwellTimer = 0f;
        if (outlineImage)
        {
            runtimeMaterial.SetFloat("_Progress", 0f);
            outlineImage.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Action

    public void Execution(ActionType action)
    {
        ExperimentLogger.Instance?.LogEvent("Action_Executed", $"Button: {gameObject.name}, Action: {action}");
        
        switch (action)
        {
            case ActionType.None:
                break;
            case ActionType.Flicker:
                Debug.Log("Flicker action executed!");
                break;
            case ActionType.ResetDwell:
                ResetColor();
                break;
            case ActionType.TestUI:
                TestUIControl();
                break;
        }
    }

    #endregion

    #region Flicker Functions

    void UpdateFlicker()
    {
        if (currentState != State.Flickering || runtimeMaterialFlicker == null) return;

        // Anchor the phase to when flickering started so every activation is consistent
        if (flickerStartTime < 0f)
            flickerStartTime = Time.unscaledTime;

        float elapsed = Time.unscaledTime - flickerStartTime;
        float phase = (elapsed * GlobalInput.Instance.flickerHz) % 1.0f;
        bool isOn = phase < 0.5f;  // 50% duty cycle, self-correcting every frame

        runtimeMaterialFlicker.SetFloat("_FlickerState", isOn ? 1f : 0f);
    }

    #endregion

    #region TestUI Update

    public void TestUIControl()
    {
        if (isNext)
            NextPhase();
        else if (isDelete)
            DeleteValue();
        else
            AddValue();
    }

    private void AddValue()
    {
        if (testUI != null) testUI.AddDigit(value);
    }

    private void DeleteValue()
    {
        if (testUI != null) testUI.RemoveDigitLast();
    }

    private void NextPhase()
    {
        if (testUI != null) testUI.NextPhase();
    }

    #endregion

    #region Hover Events

    public void OnHoverEnter()
    {
        isHovering = true;
        ExperimentLogger.Instance?.LogEvent("Hover_Enter", $"Button: {gameObject.name}");
        
        if (currentState == State.Idle)
            currentState = State.Hovering;
    }

    public void OnHoverExit()
    {
        isHovering = false;
        ExperimentLogger.Instance?.LogEvent("Hover_Exit", $"Button: {gameObject.name}");
        
        currentState = State.Idle;
        dwellTimer = 0f;
        hasTriggered = false;
        flickerStartTime = -1f;

        ResetColor();

        if (runtimeMaterialFlicker != null)
            runtimeMaterialFlicker.SetFloat("_FlickerState", 0f);
    }

    #endregion
}