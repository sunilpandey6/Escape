using System.Collections;
using System.Collections.Generic;
using System.Security.Authentication;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameObject3D : MonoBehaviour
{
    [SerializeField] private bool isHovering = false;
    [SerializeField] private bool hasTriggered = false;
    [SerializeField] private float dwellTimer = 0f;
    [SerializeField] private float flickerTimer = 0f;
    [SerializeField] private Outline outline;


    public UIControl uiControl;
    void Awake()
    {
        outline = GetComponent<Outline>();

        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        outline.enabled = false;
    }

    public void Enter()
    {
        isHovering = true;
        outline.enabled = true;
    }

    public void Exit()
    {
        isHovering = false;
        outline.enabled = false;
        ResetColor();
    }

    private void Update()
    {
        if (isHovering)
        {
            if (outline.enabled)                       
                ChangeColor();

        }
    }

    #region Dell functions

    public void ChangeColor()
    {
        dwellTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(dwellTimer / GlobalInput.Instance.dwellTime);
        if (progress < 0.5f)
        {
            float t = progress / 0.5f;
            outline.OutlineColor = Color.Lerp(GlobalInput.Instance.idleColor, GlobalInput.Instance.midColor, t);
        }
        else
        {
            float t = (progress - 0.5f) / 0.5f;
            outline.OutlineColor = Color.Lerp(GlobalInput.Instance.midColor, GlobalInput.Instance.activeColor, t);
        }
        if (dwellTimer >= GlobalInput.Instance.dwellTime && !hasTriggered)
        {
            hasTriggered = true;
            OnDwellComplete();

        }
    }
    public void OnDwellComplete()
    {
        if (hasTriggered)
        {
            // add functions here
            uiControl.Toggle();

        }
    }

    public void ResetColor()
    {
        dwellTimer = 0f;
        hasTriggered = false;
        outline.OutlineColor = GlobalInput.Instance.idleColor;
    }

    #endregion


}
