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

    public enum ActionType
    {
        None,
        ButtonAction,
        TestUI
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

    public static BB activeButton = null;

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

    #region Unity Lifecycle
    void Awake()
    {
        if (!button)
            button = GetComponent<Button>();

        if (outlineImage)
            outlineRect.sizeDelta = buttonRect.sizeDelta + new Vector2(outlineSize * 2, outlineSize * 2);

        if (borderImage)
            borderRect.sizeDelta = buttonRect.sizeDelta + new Vector2(borderSize * 2, borderSize * 2);
    }

    void OnEnable()
    {
        if (outlineImage)
        {
            runtimeMaterial = new Material(outlineImage.material);
            outlineImage.material = runtimeMaterial;
            ApplyGlobalColors();
            outlineImage.gameObject.SetActive(false);
        }

        if (buttonImage)
        {
            runtimeMaterialFlicker = new Material(buttonImage.material);
            buttonImage.material = runtimeMaterialFlicker;
            ApplyFlickerColors();
        }

        // reset runtime state (important!)
        dwellTimer = 0f;
        hasTriggered = false;
        flickerStartTime = -1f;
    }

    void OnDisable()
    {
        runtimeMaterial = null;
        runtimeMaterialFlicker = null;

        if (activeButton == this)
            activeButton = null;
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
                if (isHovering && currentState != State.Flickering) ChangeColor();
                break;
            case Att.DwellDemo:
                if (isHovering && currentState != State.Flickering) HandleDwell();
                break;
            case Att.FlickerDemo:
                if (isHovering) HandleFlickerDemo();
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
        if (outlineImage && !outlineImage.gameObject.activeSelf) outlineImage.gameObject.SetActive(true);


        currentState = State.Dwelling;

        dwellTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(dwellTimer / GlobalInput.Instance.dwellTime);
        runtimeMaterial.SetFloat("_Progress", progress);

        if (progress >= 1f && currentState != State.Flickering && !hasTriggered)
        {
            hasTriggered = true;
            Execution(selectedAction);
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
        ExperimentLogger.Instance?.LogEvent("Dwell_Complete", $"Button: {gameObject.name}", "Classify_Dwelling");
        
        currentState = State.Flickering;
        hasTriggered = true;
        
        ExperimentLogger.Instance?.LogEvent("Flicker_Start", $"Button: {gameObject.name}, Hz: {GlobalInput.Instance.flickerHz}", "Flickering");

        yield return new WaitForSeconds(GlobalInput.Instance.flickerDuration);

        currentState = State.Idle;
        flickerStartTime = -1f;

        if (runtimeMaterialFlicker != null)
            runtimeMaterialFlicker.SetFloat("_FlickerState", 0f);

        ExperimentLogger.Instance?.LogEvent("Flicker_End", $"Button: {gameObject.name}", "Wait_For_Classify_Flickering");

        Execution(selectedAction);
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
        ExperimentLogger.Instance?.LogEvent("Action_Executed", $"Button: {gameObject.name}, Action: {action}", "Execution_Proceeding");
        
        switch (action)
        {
            case ActionType.None:
                break;
            case ActionType.ButtonAction:
                button?.onClick.Invoke();
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
        if (activeButton != null && activeButton != this) return;
        activeButton = this;

        isHovering = true;
        ExperimentLogger.Instance?.LogEvent("Hover_Enter", $"Button: {gameObject.name}", "Hovering");
        
        if (currentState == State.Idle)
            currentState = State.Hovering;
    }

    public void OnHoverExit()
    {
        if (activeButton != this) return;

        isHovering = false;
        ExperimentLogger.Instance?.LogEvent("Hover_Exit", $"Button: {gameObject.name}", "Hover_Exit");
        
        currentState = State.Idle;
        dwellTimer = 0f;
        hasTriggered = false;
        flickerStartTime = -1f;

        ResetColor();

        if (runtimeMaterialFlicker != null)
            runtimeMaterialFlicker.SetFloat("_FlickerState", 0f);

        activeButton = null;
    }

    #endregion
}