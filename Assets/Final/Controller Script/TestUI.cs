using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestUI : MonoBehaviour
{
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
        if (GlobalInput.Instance.cam == null) return;

        if (GlobalInput.Instance.cam != null)
        {
            // Position in front of camera once
            transform.position = GlobalInput.Instance.cam.transform.position 
                + GlobalInput.Instance.cam.transform.right * GlobalInput.Instance.horizontalOffset 
                + GlobalInput.Instance.cam.transform.up * GlobalInput.Instance.verticalOffset 
                + GlobalInput.Instance.cam.transform.forward * GlobalInput.Instance.distance;

            // Make UI face the camera once
            transform.rotation = GlobalInput.Instance.cam.transform.rotation;
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
