using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(Flicker))]
public class OB : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Demo3D demoScene; // Scene controller to call door actions

    public enum ActionType
    {
        None,
        DoorSwitch,
        NextScene,
        FinalScreen
    }

    [Header("Door Operations")]
    [SerializeField] private ActionType selectedAction = ActionType.None;

    [Header("Outline")]
    [SerializeField] private Outline outline;
    [SerializeField] private Flicker flicker;
    
    [Header("Dwell")]
    [SerializeField] private float dwellTimer = 0f;
    [SerializeField] private bool isHovering = false;
    [SerializeField] private bool hasTriggered = false;
    [SerializeField] private bool isFlickering = false;

    private void Start()
    {
        outline.ApplyGlobalColors();
        flicker.enabled = false;
    }

    private void Update()
    {
        if (!isHovering || hasTriggered || isFlickering) return;

        dwellTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(dwellTimer / GlobalInput.Instance.dwellTime);

        outline.SetProgress(progress);

        if (!hasTriggered && progress >= 1f)
        {
            hasTriggered = true;
            StartCoroutine(FlickerAndExecute());
        }
    }

    #region Pointer Event
    public void StartGaze()
    {
        isHovering = true;
        ExperimentLogger.Instance?.LogEvent("Gaze_Start", $"Object: {gameObject.name}");
    }

    public void StopGaze()
    {
        isHovering = false;
        ExperimentLogger.Instance?.LogEvent("Gaze_Stop", $"Object: {gameObject.name}");
        dwellTimer = 0f;
        hasTriggered = false;
        outline.ResetOutline();
    }
    #endregion

    #region Dwell Complete
    private IEnumerator FlickerAndExecute()
    {
        ExperimentLogger.Instance?.LogEvent("Dwell_Complete", $"Object: {gameObject.name}");
        
        isFlickering = true;
        flicker.StartFlicker();

        ExperimentLogger.Instance?.LogEvent("Flicker_Start", $"Object: {gameObject.name}, Hz: {GlobalInput.Instance.flickerHz}");

        yield return new WaitForSeconds(GlobalInput.Instance.flickerDuration);

        outline.ResetOutline();
        isFlickering = false;

        ExperimentLogger.Instance?.LogEvent("Flicker_End", $"Object: {gameObject.name}");

        ExecuteAction(selectedAction);
    }

    private void ExecuteAction(ActionType action)
    {
        ExperimentLogger.Instance?.LogEvent("Action_Executed", $"Object: {gameObject.name}, Action: {action}");
        
        switch (action)
        {
            case ActionType.None: break;
            case ActionType.DoorSwitch:
                demoScene?.Door2Active();
                break;
            case ActionType.NextScene:
                MainControl.Instance.GoToNextPhase();
                break;
            case ActionType.FinalScreen:
                Debug.Log("Final Screen Action Triggered!");
                break;
        }
    }
    #endregion
}