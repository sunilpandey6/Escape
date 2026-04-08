using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Demo3D : MonoBehaviour
{
    [Header("Door Reference")]
    [SerializeField] private GameObject Door1;
    [SerializeField] private GameObject Door2;

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

    #region Next Phase
    public void NextPhase()
    {
        MainControl.Instance.GoToNextPhase();
    }
    #endregion
}
