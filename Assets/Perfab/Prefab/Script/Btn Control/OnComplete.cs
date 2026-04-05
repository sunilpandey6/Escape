using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class OnComplete : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text displayText;
    private string currentText = "";
    private bool isHovering;
    [Header("Button State")]
    public ButtonState buttonState;
    [Header("Input Settings")]
    // Removed public InputSettings inputSettings;
    [Header("UI Control")]
    public UIControl uiControl;
    //private Coroutine FlickerCoroutine;

    public enum ActionType
    {
        None,
        Flicker,
        TestAction,
        ResetDwell,

    }


    public void Execution(ActionType action, string a = null)
    {
        switch (action)
        {
            case ActionType.None:
                //Debug.Log("No action selected.");
                break;
            case ActionType.Flicker:
                Debug.Log("Flicker action executed!"); // Implement your flicker logic here
                FF();
                break;
            case ActionType.TestAction:
                Action(buttonState);
                break;
            case ActionType.ResetDwell:
                buttonState.isHovering = false;
                break;
        }
    }

    #region Flicker
    public void FF()
    {
        StartCoroutine(Flicker());
    }

    private IEnumerator Flicker()
    {
        buttonState.isFlickering = true;
        for (int i = 0; i < buttonState.flickerTimes; i++)
        {
            float elapsed = 0f;
            while (elapsed < InputSettings.Instance.flickerDuration)
            {
                if ((elapsed * InputSettings.Instance.flickerHz) % 1f < 0.5f)
                {
                    ChangeColor(InputSettings.Instance.flickerOn);
                }
                else
                {
                    ChangeColor(InputSettings.Instance.idleColor);
                }

                yield return null;
                elapsed += Time.deltaTime;
            }

            ChangeColor(InputSettings.Instance.idleColor);

            if (i < buttonState.flickerTimes - 1)
            {
                yield return new WaitForSeconds(0.1f);
            }

        }
        buttonState.isFlickering = false;
    }

    public void ChangeColor(Color color)
    {
        if (buttonState.btnImageUI != null)
        {
            buttonState.btnImageUI.color = color;
        }
        else
            Debug.LogWarning("btnImageUI is not assigned in the inspector.");
    }
    #endregion

    #region Action On Test
    public void Action(ButtonState bs)
    {
        if (bs.isNext) OnNext();
        else if (bs.isDelete) RemoveDigitLast();
        else AddDigit(bs.value);

    }
    public void AddDigit(string digit)
    {
        currentText += digit;
        UpdateDisplay();
    }

    public void RemoveDigitLast()
    {
        if (currentText.Length > 0)
        {
            currentText = currentText.Substring(0, currentText.Length - 1);
            UpdateDisplay();
        }
    }

    public void OnNext()
    {
        if (!string.IsNullOrEmpty(currentText))
        {
            currentText = "";
            UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {
        displayText.text = currentText;
    }

    #endregion
}
