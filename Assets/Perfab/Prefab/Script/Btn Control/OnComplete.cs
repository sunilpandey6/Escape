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
    public ButtonState buttonState;
    public UIControl uiControl;
    public enum ActionType
    {
        None,
        Flicker,
        TestAction,
        ResetDwell,
        ToogleUI

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

                break;
            case ActionType.TestAction:
                Action(buttonState);
                break;
            case ActionType.ResetDwell:
                buttonState.isHovering = false;
                break;
            case ActionType.ToogleUI:
                // Implement your UI toggle logic here
                uiControl.Toggle();
                break;

        }
    }

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
}
