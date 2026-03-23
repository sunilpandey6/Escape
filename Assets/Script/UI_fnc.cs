using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class UI_fnc : MonoBehaviour
{
    public TMP_Text displayText;
    private string currentText = "";

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
            string filePath = Path.Combine(Application.persistentDataPath, "input.txt");

            // Append instead of overwrite
            File.AppendAllText(filePath, currentText + "\n");

            Debug.Log("Input saved to: " + filePath);

            currentText = "";
            UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {
        displayText.text = currentText;
    }
}