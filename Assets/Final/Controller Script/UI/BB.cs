using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;


// Button Behavior
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
    private State currentState = State.Idle;

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
    public bool isFlickering = false;
    public float flickerTimes = 1f;

    [Header("Outline and Border Settings")]
    [SerializeField] private float outlineSize = 10f;
    [SerializeField] private float borderSize = 3f;

    [Header("Button Action")]
    [SerializeField] private ActionType selectedAction;

    [Header("UI Control refernce ")]
    public string value;
    public bool isDelete;
    public bool isNext;
    public TestUI testUI;
    // Button Actions
    public enum ActionType
    {
        None, //Headings
        Flicker,  // Flicker
        ResetDwell // Dwell Setting
    }

    #region Unity Lifecycle
    public void Awake()
    {

        if(outlineImage)
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

        if(buttonImage)
        {
            runtimeMaterialFlicker = Instantiate(buttonImage.material);
            buttonImage.material = runtimeMaterialFlicker;
            ApplyFlickerColors();

        }  
        if (!button)
            button = GetComponent<Button>();

    }
    void ApplyGlobalColors()
    {
        runtimeMaterial.SetColor("_IdleColor", GlobalInput.Instance.idleColor);
        runtimeMaterial.SetColor("_MidColor", GlobalInput.Instance.midColor);
        runtimeMaterial.SetColor("_ActiveColor", GlobalInput.Instance.activeColor);
    }

    void ApplyFlickerColors()
    {
        runtimeMaterialFlicker.SetFloat("_FlickerStartTime", -1f);
        runtimeMaterialFlicker.SetColor("_IdleColor", GlobalInput.Instance.idleColor);
        runtimeMaterialFlicker.SetColor("_FlickerColor", GlobalInput.Instance.flickerOn);
        runtimeMaterialFlicker.SetFloat("_FlickerHz", GlobalInput.Instance.flickerHz);
        runtimeMaterialFlicker.SetFloat("_FlickerDuration", GlobalInput.Instance.flickerDuration);
    }

    public void Update()
    {
        switch (attribute)
        {
            case Att.None:
                HandleNone();
                break;
            case Att.Normal:
                if (isHovering && !isFlickering)
                {
                    ChangeColor();
                }
                break;
            case Att.DwellDemo:
                HandleDwell();
                break;
            case Att.FlickerDemo:
                HandleFlickerDemo();
                break;
           
        }
    }

    #endregion

    #region state control
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

            if (progress >= 1f && currentState != State.Flickering)
            {
                currentState = State.Flickering;

                FF(); // start flicker ONCE
                Execution(selectedAction);
            }
        }
    }

    void HandleFlickerDemo()
    {
        if (currentState == State.Hovering)
        {
            currentState = State.Flickering;
            FF(); // ONLY once
        }
    }
    
    #endregion

    #region Dell functions

    public void ChangeColor()
    {
        if (outlineImage && !outlineImage.gameObject.activeSelf)
        {
            outlineImage.gameObject.SetActive(true);
        }

        if (outlineImage)
        {
            dwellTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(dwellTimer / GlobalInput.Instance.dwellTime);
            // if (progress < 0.5f)
            // {
            //     float t = progress / 0.5f;
            //     //outlineImage.color = Color.Lerp(GlobalInput.Instance.idleColor, GlobalInput.Instance.midColor, t);
            //     material.SetFloat("_Progress", progress);
            // }
            // else
            // {
            //     float t = (progress - 0.5f) / 0.5f;
            //     //outlineImage.color = Color.Lerp(GlobalInput.Instance.midColor, GlobalInput.Instance.activeColor, t);
            //     material.SetFloat("_Progress", progress);
            // }

            // Send progress to shader
            runtimeMaterial.SetFloat("_Progress", progress);

            if (dwellTimer >= GlobalInput.Instance.dwellTime && !hasTriggered)
            {
                hasTriggered = true;
                OnDwellComplete();

            }
        }
    }
    public void OnDwellComplete()
    {
        if (hasTriggered)
        {
            Execution(selectedAction);
            //WaitForSeconds(GlobalInput.Instance.flickerDuration);
        }

        button?.onClick.Invoke();
    }

    public void ResetColor()
    {
        dwellTimer = 0f;
        if (outlineImage)
        {
            //outlineImage.color = GlobalInput.Instance.idleColor;
            runtimeMaterial.SetFloat("_Progress", 0f);
            outlineImage.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Action
    public void Execution(ActionType action)
    {
        switch (action)
        {
            case ActionType.None:
                //Debug.Log("No action selected.");
                break;
            case ActionType.Flicker:
                Debug.Log("Flicker action executed!");
                FF();
                break;
            case ActionType.ResetDwell:
                ResetColor();
                break;
        }
    }
    #endregion
    
    #region Flicker functions

    public void FF()
    {
        //StartCoroutine(Flicker());
        if (runtimeMaterialFlicker == null) return;

        runtimeMaterialFlicker.SetFloat("_FlickerStartTime", Time.time);
        isFlickering = true;

        StartCoroutine(StopFlicker());
    }

    private IEnumerator StopFlicker()
    {
        yield return new WaitForSeconds(GlobalInput.Instance.flickerDuration);

        isFlickering = false;

        // Allow system to reset properly
        currentState = State.Idle;
    }

    private IEnumerator Flicker()
    {
        isFlickering = true;
        for (int i = 0; i < flickerTimes; i++)
        {
            float elapsed = 0f;
            while (elapsed < GlobalInput.Instance.flickerDuration)
            {
                if ((elapsed * GlobalInput.Instance.flickerHz) % 1f < 0.5f)
                {
                    CFColor(GlobalInput.Instance.flickerOn);
                }
                else
                {
                    CFColor(GlobalInput.Instance.idleColor);
                }

                yield return null;
                elapsed += Time.deltaTime;
            }

            CFColor(GlobalInput.Instance.idleColor);

            if (i < flickerTimes - 1)
            {
                yield return new WaitForSeconds(0.1f);
            }

        }
        isFlickering = false;
    }

    public void CFColor(Color color)
    {
        if (buttonImage != null)
        {
           buttonImage.color = color;
        }
        else
            Debug.LogWarning("btnImageUI is not assigned in the inspector.");
    }

    #endregion

    #region TestUI update

    public void TestUIControl()
    {
        if (isNext)
        {
            NextPhase();
        }
        
        else if (isDelete)
        {
            DeleteValue();
        }

        else
        {
            AddValue();
        }
    }
    private void AddValue()
    {
        if (testUI != null)
        {
            testUI.AddDigit(value);
        }
    }

    private void DeleteValue()
    {
        if (testUI != null)
        {
            testUI.RemoveDigitLast  ();
        }
    }

    private void NextPhase()
    {
        if (testUI != null)
        {
            testUI.NextPhase();
        }
    }

    #endregion

    #region Hover Events
    public void OnHoverEnter()
    {
        isHovering = true;

        if (currentState == State.Idle)
            currentState = State.Hovering;
    }

    public void OnHoverExit()
    {
        isHovering = false;

        currentState = State.Idle;
        dwellTimer = 0f;
        hasTriggered = false;

        ResetColor();
    }
    #endregion

}
