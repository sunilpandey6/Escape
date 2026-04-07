using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Demo3D : MonoBehaviour
{
    [Header("Door Reference")]
    [SerializeField] private GameObject Door1;
    [SerializeField] private GameObject Door2;

    [Header("Dwell Settings")]
    [SerializeField] private float dwellTimer;
    [SerializeField] private bool isHovering = false;
    [SerializeField] private bool hasTriggered = false;

    [Header("Outline Reference")]
    [SerializeField] private Outline outline;

    #region Unity Lifecycle

    private void OnEnable()
    {
        PositionCanvasFront();
        Door1Active();
    }

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

    #region Dwell
    private void StartDwell()
    {
        if(isHovering)
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
        
    }

    private void OnDwellComplete()
    {

        //Door2Active();

    }

    #endregion

    #region Flicker

    #endregion

    #region Door Switching
    public void Door1Active()
    {
        if (Door1 != null) Door1.SetActive(true);
        if (Door2 != null) Door2.SetActive(false);
    }
    public void Door2Active()
    {
        if (Door1 != null) Door1.SetActive(false);
        if (Door2 != null) Door2.SetActive(true);
    }
    #endregion

    #region Hover Events
    public void OnHoverEnter()
    {
        outline.enabled = true;
        isHovering = true;
        StartDwell();
    }

    public void OnHoverExit()
    {
        outline.enabled = false;
        isHovering = false;
        hasTriggered = false;
        outline.OutlineColor = GlobalInput.Instance.idleColor;
        outline.enabled = false;
    }
    #endregion

}
