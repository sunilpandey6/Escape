using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestUI : MonoBehaviour
{
    [Header("Main Canvas Positioning")]
    public Camera cam;
    public float distance = 2f;
    public float horizontalOffset = 0f;
    public float verticalOffset = 0f;

    [Header("UI Reference")]
    public TMP_Text value;
    
    private string currentText = "";


#region Unity Lifecycle
    private void OnEnable()
    {
        PositionCanvasFront();
        UpdateDisplay();
    }
#endregion

#region UI Positioning
    public void PositionCanvasFront()
    {
        if (cam == null) cam = Camera.main;

        if (cam != null)
        {
            // Position in front of camera once
            transform.position = cam.transform.position 
                + cam.transform.right * horizontalOffset 
                + cam.transform.up * verticalOffset 
                + cam.transform.forward * distance;

            // Make UI face the camera once
            transform.rotation = cam.transform.rotation;
        }
    }
#endregion

#region UI Update
    void UpdateDisplay()
    {
        if (value != null)
            value.text = currentText;
    }
#endregion

#region UI Input
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

    public void NextPhase()
    {
        MainControl.Instance.GoToNextPhase();
    }
#endregion
}
