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

    public static OB activeObject = null;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        flicker = GetComponent<Flicker>();
    }

    private void OnValidate()
    {
        outline = GetComponent<Outline>();
        flicker = GetComponent<Flicker>();
    }

    private void OnEnable()
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
        if (MainControl.Instance != null && !MainControl.Instance.isGazeInteractionEnabled) return;

        if (activeObject != null && activeObject != this) return;
        activeObject = this;

        isHovering = true;
        ExperimentLogger.Instance?.LogEvent("Dwell_Start", $"Object: {gameObject.name}", "Dwell_Started");
    }

    public void StopGaze()
    {
        if (MainControl.Instance != null && !MainControl.Instance.isGazeInteractionEnabled) return;

        if (activeObject != this) return;

        isHovering = false;
        ExperimentLogger.Instance?.LogEvent("Gaze_Stop", $"Object: {gameObject.name}", "Hover_Exit");
        dwellTimer = 0f;
        hasTriggered = false;
        outline.ResetOutline();

        activeObject = null;
    }
    #endregion

    #region Dwell Complete
    private IEnumerator FlickerAndExecute()
    {
        ExperimentLogger.Instance?.LogEvent("Dwell_Complete", $"Object: {gameObject.name}", "Dwelling_Completed");
        
        isFlickering = true;
        flicker.StartFlicker();

        ExperimentLogger.Instance?.LogEvent("Flicker_Start", $"Object: {gameObject.name}, Hz: {GlobalInput.Instance.flickerHz}", "Flickering_Start");

        yield return new WaitForSeconds(GlobalInput.Instance.flickerDuration);

        outline.ResetOutline();
        isFlickering = false;

        ExperimentLogger.Instance?.LogEvent("Flicker_End", $"Object: {gameObject.name}", "Flickering_Completed");

        ExecuteAction(selectedAction);
    }

    private void ExecuteAction(ActionType action)
    {
        ExperimentLogger.Instance?.LogEvent("Action_Executed", $"Object: {gameObject.name}, Action: {action}", "Execution_Proceeding");
        
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

    private void OnDisable()
    {
        isHovering = false;
        isFlickering = false;
        hasTriggered = false;
        dwellTimer = 0f;

        if (activeObject == this)
            activeObject = null;

        StopAllCoroutines();
    } 
}