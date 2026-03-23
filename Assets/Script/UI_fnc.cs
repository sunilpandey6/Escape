using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_fnc : MonoBehaviour
{    
    public void ShowPanel()
    {
        //Transform cam = ovrRig.centerEyeAnchor;
        Vector3 spawnPos = cam.position + cam.forward * panelDistance;
        Quaternion spawnRot = Quaternion.LookRotation(cam.forward, cam.up);
        panelPrefab.transform.SetPositionAndRotation(spawnPos, spawnRot);

    }

    public TMP_Text displayText;
    private string currentText = "";
    
    public void addDigit(string digit)
    {
        currentText += digit;
        UpdateDisplay();
    }

    public void removeDigitlast()
    {
        if (currentText.Length > 0)
        {
            currentText = currentText.Substring(0, currentText.Length - 1);
            UpdateDisplay();
        }
    }



}
