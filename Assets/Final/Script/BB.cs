using System.Collections;
using System.Collections.Generic;
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
    [Header("Button as header")]
    [SerializeField] private Image outlineImage;
    [SerializeField] private readonly Image borderImage;
    [SerializeField] private Image buttonImage;

    [SerializeField] private RectTransform outlineRect;
    [SerializeField] private RectTransform borderRect;
    [SerializeField] private RectTransform buttonRect;

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
    [SerializeField] private ActionType selectedAction1;
    [SerializeField] private ActionType selectedAction2;
    [SerializeField] private ActionType selectedAction3;

    [Header("UI Control refernce ")]
    public string value;
    public bool isDelete;
    public bool isNext;

    public TestScene testScene;

    // Button Actions
    public enum ActionType
    {
        None, //Headings
        Flicker,  // Flicker
        ResetDwell, // Dwell Setting
        TestUI // UI Control


    }

    #region Awake / Start / Update
    public void Awake()
    {
        if(outlineImage)
        {
            outlineRect.sizeDelta = buttonRect.sizeDelta + new Vector2(outlineSize * 2, outlineSize * 2);
            outlineImage.gameObject.SetActive(false);
        }
        if (borderImage)
        {
            borderRect.sizeDelta = buttonRect.sizeDelta + new Vector2(borderSize * 2, borderSize * 2);
        }
        if (!button)
            button = GetComponent<Button>();

    }

    public void Update()
    {
        switch (attribute)
        {
            case Att.None:
                if (outlineImage && !outlineImage.gameObject.activeSelf)
                {
                    outlineImage.gameObject.SetActive(true);
                }
                break;
            case Att.Normal:
                if (isHovering && !isFlickering)
                {
                    ChangeColor();
                }
                break;
            case Att.DwellDemo:
                if (isHovering && !isFlickering)
                {
                    ChangeColor();
                }
                break;
            case Att.FlickerDemo:
                if (isHovering && !isFlickering)
                {
                    FF();
                }
                break;
           
        }
    }

    #endregion
    public void OnHoverEnter()
    {
        isHovering = true;
    }

    public void OnHoverExit()
    {
        isHovering = false;
        hasTriggered = false;
        ResetColor();
    }

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
            if (progress < 0.5f)
            {
                float t = progress / 0.5f;
                outlineImage.color = Color.Lerp(GlobalInput.Instance.idleColor, GlobalInput.Instance.midColor, t);
            }
            else
            {
                float t = (progress - 0.5f) / 0.5f;
                outlineImage.color = Color.Lerp(GlobalInput.Instance.midColor, GlobalInput.Instance.activeColor, t);
            }
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
            Execution(selectedAction1);
            Execution(selectedAction2);
            Execution(selectedAction3);
        }

        button?.onClick.Invoke();
    }

    public void ResetColor()
    {
        dwellTimer = 0f;
        if (outlineImage)
        {
            outlineImage.color = GlobalInput.Instance.idleColor;
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
            case ActionType.TestUI:
                if (testScene != null)
                {
                    if (isNext)
                    {
                        testScene.Pro();
                    }
                    else if (isDelete)
                    {
                        testScene.RemoveDigitLast();
                    }
                    else
                    {
                        testScene.AddDigit(value);
                    }
                }
                else
                {
                    Debug.LogWarning("TestScene reference is not assigned in the inspector.");
                }
                break;
        }
    }
    #endregion
    
    #region Flicker functions

    public void FF()
    {
        StartCoroutine(Flicker());
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


}
