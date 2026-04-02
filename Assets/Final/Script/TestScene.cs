using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestScene : MonoBehaviour
{
    public TMP_Text value;
    private string currentText = "";

    void UpdateDisplay()
    {
        value.text = currentText;
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

    public void Pro()
    {
        
    }
}
