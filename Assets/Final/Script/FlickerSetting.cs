using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FlickerSetting : MonoBehaviour
{
    public TMP_Text valueTextHz;
    public TMP_Text valueTextTime;

    private void Awake()
    {
        updateValue();
    }
    
    public void updateValue()
    {
        if (valueTextTime != null)
            valueTextTime.text = GlobalInput.Instance.flickerDuration.ToString("0.0") + " Sec";
        if (valueTextHz != null)
            valueTextHz.text = GlobalInput.Instance.flickerHz.ToString("0.0") + " Hz";
    }

    public void addhz()
    {
        if (GlobalInput.Instance.flickerHz < 30f)
        {
            GlobalInput.Instance.flickerHz += 1.0f;
            updateValue();
        }
    }
    public void subhz()
    {
        if (GlobalInput.Instance.flickerHz > 12f)
        {
            GlobalInput.Instance.flickerHz -= 0.5f;
            updateValue();
        }
    }

    public void addTime()
    {
        if (GlobalInput.Instance.flickerDuration < 5f)
        {
            GlobalInput.Instance.flickerDuration += 0.5f;
            updateValue();
        }
    }

    public void subTime()
    {
        if (GlobalInput.Instance.flickerDuration > 0.5f)
        {
            GlobalInput.Instance.flickerDuration -= 0.5f;
            updateValue();
        }
    }
}
