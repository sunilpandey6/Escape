using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class DwellSetting : MonoBehaviour
{
    // Start is called before the first frame update
public TMP_Text valueText;

    private void Start()
    {
        updateValue();
    }
    public void addDwellTIme()
    {
        if (GlobalInput.Instance.dwellTime < 5f)
        {
            GlobalInput.Instance.dwellTime += 0.5f;
            updateValue();
        }
    }
    public void subDwellTIme()
    {
        if (GlobalInput.Instance.dwellTime > 0.5f)
        {
            GlobalInput.Instance.dwellTime -= 0.5f;
            updateValue();
        }
    }


    public void updateValue()
    {
        if (valueText != null)
            valueText.text = GlobalInput.Instance.dwellTime.ToString("0.0") + " Sec";
    }
}
