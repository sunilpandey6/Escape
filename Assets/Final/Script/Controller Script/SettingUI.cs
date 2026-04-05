using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingUI : MonoBehaviour
{
    [Header("Main Canvas Positioning")]
    public Camera cam;
    public float distance = 2f;
    public float horizontalOffset = 0f;
    public float verticalOffset = 0f;
    
    [Header("UI Panels")]
    public GameObject DwellPanel;
    public GameObject FlickerPanel;
    
    [Header("Dwell Setting References")]
    public TMP_Text dwellTimeText;

    [Header("Flicker Setting References")]
    public TMP_Text flickerHzText;
    public TMP_Text flickerTimeText;

    #region Unity Lifecycle
    private void Awake()
    {
        DwellPanelOn();
        UpdateAllValues();
    }

    private void OnEnable()
    {
        PositionCanvasFront();
    }

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

    #region Panel Switching
    public void DwellPanelOn()
    {
        if (DwellPanel != null) DwellPanel.SetActive(true);
        if (FlickerPanel != null) FlickerPanel.SetActive(false);
    }

    public void FlickerPanelOn()
    {
        if (DwellPanel != null) DwellPanel.SetActive(false);
        if (FlickerPanel != null) FlickerPanel.SetActive(true);
    }

    public void NextPhase()
    {
        MainControl.Instance.GoToNextPhase();
    }
    #endregion

    #region Value Updates
    public void UpdateAllValues()
    {
        UpdateDwellValue();
        UpdateFlickerValues();
    }

    private void UpdateDwellValue()
    {
        if (dwellTimeText != null)
            dwellTimeText.text = GlobalInput.Instance.dwellTime.ToString("0.0") + " Sec";
    }

    private void UpdateFlickerValues()
    {
        if (flickerTimeText != null)
            flickerTimeText.text = GlobalInput.Instance.flickerDuration.ToString("0.0") + " Sec";
        if (flickerHzText != null)
            flickerHzText.text = GlobalInput.Instance.flickerHz.ToString("0.0") + " Hz";
    }
    #endregion

    #region Dwell Settings Controls
    public void addDwellTIme()
    {
        if (GlobalInput.Instance.dwellTime < 5f)
        {
            GlobalInput.Instance.dwellTime += 0.5f;
            UpdateDwellValue();
        }
    }
    public void subDwellTIme()
    {
        if (GlobalInput.Instance.dwellTime > 0.5f)
        {
            GlobalInput.Instance.dwellTime -= 0.5f;
            UpdateDwellValue();
        }
    }
    #endregion

    #region Flicker Settings Controls
    public void addhz()
    {
        if (GlobalInput.Instance.flickerHz < 30f)
        {
            GlobalInput.Instance.flickerHz += 1.0f;
            UpdateFlickerValues();
        }
    }
    public void subhz()
    {
        if (GlobalInput.Instance.flickerHz > 12f)
        {
            GlobalInput.Instance.flickerHz -= 0.5f;
            UpdateFlickerValues();
        }
    }

    public void addTime()
    {
        if (GlobalInput.Instance.flickerDuration < 5f)
        {
            GlobalInput.Instance.flickerDuration += 0.5f;
            UpdateFlickerValues();
        }
    }

    public void subTime()
    {
        if (GlobalInput.Instance.flickerDuration > 0.5f)
        {
            GlobalInput.Instance.flickerDuration -= 0.5f;
            UpdateFlickerValues();
        }
    }
    #endregion
}
