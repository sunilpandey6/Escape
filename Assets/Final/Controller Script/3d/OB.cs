using System.Collections;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Outline))]
public class OB : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Demo3D demoScene; // Scene controller to call door actions

    public enum ActionType
    {
        None,
        DoorSwitch,
        FinalScreen
    }

    [Header("Door Operations")]
    [SerializeField] private ActionType selectedAction = ActionType.None;
    [SerializeField] private ActionType selectedAction1 = ActionType.None;

    [SerializeField] private Outline outline;
    [SerializeField] private float dwellTimer = 0f;
    [SerializeField] private bool isHovering = false;
    [SerializeField] private bool hasTriggered = false;
    [SerializeField] private bool isFlickering = false;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        outline.ResetOutline(); // Reset shader progress
    }

    private void OnEnable()
    {
        outline.ApplyGlobalColors();
    }

    private void Update()
    {
        if (!isHovering || hasTriggered || isFlickering) return;

        // Increment dwell timer
        dwellTimer += Time.deltaTime;

        // Update shader progress
        float progress = Mathf.Clamp01(dwellTimer / GlobalInput.Instance.dwellTime);
        outline.dwellTimer = dwellTimer;
        
        outline.UpdateMaterialProperties();

        // Trigger action when dwell completes
        if (!hasTriggered && progress >= 1f)
        {
            hasTriggered = true;
            OnDwellComplete();
        }
    }

#region Pointer Event
    /// <summary>
    /// Call this when user starts gazing at the door
    /// </summary>
    public void StartGaze()
    {
        isHovering = true;
    }

    /// <summary>
    /// Call this when user stops gazing
    /// </summary>
    public void StopGaze()
    {
        isHovering = false;
        dwellTimer = 0f;
        outline.ResetOutline();
    }
 #endregion

#region Dwell Complete
    private void OnDwellComplete()
    {
        Debug.Log($"{gameObject.name} dwell complete! Initiating flicker...");
        StartCoroutine(FlickerAndExecute());
    }

    private IEnumerator FlickerAndExecute()
    {
        isFlickering = true;
        
        float elapsed = 0f;
        float duration = GlobalInput.Instance.flickerDuration;
        float hz = GlobalInput.Instance.flickerHz;

        while (elapsed < duration)
        {
            // Flash outline by enabling/disabling
            if ((elapsed * hz) % 1f < 0.5f)
            {
                outline.enabled = false;
            }
            else
            {
                outline.enabled = true;
            }

            yield return null;
            elapsed += Time.deltaTime;
        }

        // Ensure outline is restored and reset cleanly
        outline.enabled = true;
        outline.ResetOutline();
        isFlickering = false;

        // Now execute the actions after flickering is done
        ExecuteAction(selectedAction);
        ExecuteAction(selectedAction1);
    }

    private void ExecuteAction(ActionType action)
    {
        switch (action)
        {
            case ActionType.None:
                break;
            case ActionType.DoorSwitch:
                if (demoScene != null) demoScene.Door2Active();
                break;
            case ActionType.FinalScreen:
                Debug.Log("Final Screen Action Triggered!");
                // e.g. MainControl.Instance.GoToNextPhase();
                break;
        }
    }
 #endregion


    

}
