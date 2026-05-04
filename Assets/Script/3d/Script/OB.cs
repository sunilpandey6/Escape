using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(Flicker))]
public class OB : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Demo3D demoScene; // Scene controller to call door actions
    [SerializeField] private Test3D test3D;

    public enum MoveCode
    {
        Default = 0,
        Door_Single = 300,
        Door_Double = 301
    }
    
    [Header("Movement Code")]
    [Tooltip("Unique code that identifies this object for movement.")]
    [SerializeField] private MoveCode moveCode ;

    public enum ActionType
    {
        None,
        DoorSwitch,
        NextScene,
        MoveToSelectedDoor,
        FinalScreen
    }

    [Header("Door Operations")]
    [SerializeField] private ActionType selectedAction = ActionType.None;

    [Header("Outline")]
    [SerializeField] private Outline outline;
    [SerializeField] private Flicker flicker;

    [Header("Dwell")]
    [SerializeField] private float dwellTimer   = 0f;
    [SerializeField] private bool  isHovering   = false;
    [SerializeField] private bool  hasTriggered = false;
    [SerializeField] private bool  isFlickering = false;

    // WaitingForLSL: flicker has finished; waiting for Python to confirm/reject
    [SerializeField] private bool isWaitingForLSL = false;

    // Ownership: mirrors BB.waitingButton — only one OB waits at a time
    public static OB activeObject  = null;
    public static OB waitingObject = null;

    // BCI Identity — auto-set in Awake; echoed back by Python in BCIMessage.Detail
    [Header("BCI Identity")]
    [Tooltip("Set automatically from InstanceID. Must match what Python echoes back.")]
    [SerializeField] private string objectId;

    // Stored when the flicker marker is sent — used to validate the Python echo
    private string lastEvent;
    private string lastDetail;

    #region Unity_Functions

    private void Awake()
    {
        outline  = GetComponent<Outline>();
        flicker  = GetComponent<Flicker>();
        objectId = gameObject.name;
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

        // Subscribe to LSL flicker event — unsubscribed in OnDisable
        if (LSLCommunicationManager.Instance != null)
            LSLCommunicationManager.Instance.OnFlickerStateChanged += HandleFlickerLSL;
    }

    private void OnDisable()
    {
        // Unsubscribe first to prevent null-ref after scene unload
        if (LSLCommunicationManager.Instance != null)
            LSLCommunicationManager.Instance.OnFlickerStateChanged -= HandleFlickerLSL;

        isHovering      = false;
        isFlickering    = false;
        isWaitingForLSL = false;
        hasTriggered    = false;
        dwellTimer      = 0f;

        if (activeObject  == this) activeObject  = null;
        if (waitingObject == this) waitingObject = null;

        StopAllCoroutines();
    }

    private void Update()
    {
        // Block dwell progression while flickering or waiting for LSL response
        if (!isHovering || hasTriggered || isFlickering || isWaitingForLSL) return;

        dwellTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(dwellTimer / GlobalInput.Instance.dwellTime);

        outline.SetProgress(progress);

        if (!hasTriggered && progress >= 1f)
        {
            hasTriggered = true;
            StartCoroutine(FlickerAndExecute());
        }
    }

    #endregion

    #region Pointer Event
    public void StartGaze()
    {
        if (MainControl.Instance != null && !MainControl.Instance.isGazeInteractionEnabled) return;

        if (activeObject != null && activeObject != this) return;
        activeObject = this;

        isHovering = true;
        outline.SetOutlineEnabled(true);
        
        ExperimentLogger.Instance?.LogEvent("Dwell_Start", $"Object: {gameObject.name}", "Dwell_Started");
        LSL_Logger.Instance?.LogEvent("Dwell_Start", $"Object: {gameObject.name}", "Dwell_Started");
    }

    public void StopGaze()
    {
        if (MainControl.Instance != null && !MainControl.Instance.isGazeInteractionEnabled) return;

        if (activeObject != this) return;

        isHovering = false;
        ExperimentLogger.Instance?.LogEvent("Gaze_Stop", $"Object: {gameObject.name}", "Hover_Exit");
        LSL_Logger.Instance?.LogEvent("Gaze_Stop", $"Object: {gameObject.name}", "Hover_Exit");
        dwellTimer   = 0f;
        hasTriggered = false;
        outline.ResetOutline();

        activeObject = null;
    }
    #endregion

#region Manual Trigger

    public void TriggerInteraction()
    {
        if(activeObject != null && activeObject != this) return;
        activeObject = this;
        isHovering = true;
        dwellTimer = 0f;
        hasTriggered = false;
        ExperimentLogger.Instance?.LogEvent("Dwell_Start_External", $"Object: {gameObject.name}", "Dwell_Started");
        LSL_Logger.Instance?.LogEvent("Dwell_Start_External", $"Object: {gameObject.name}", "Dwell_Started");
    }

    #endregion

    #region Dwell Complete
    private IEnumerator FlickerAndExecute()
    {
        ExperimentLogger.Instance?.LogEvent("Dwell_Complete", $"Object: {gameObject.name}", "Dwelling_Completed");
        LSL_Logger.Instance?.LogEvent("Dwell_Complete", $"Object: {gameObject.name}", "Dwelling_Completed");

        isFlickering = true;
        flicker.StartFlicker();

        // Store the event/detail pair so HandleFlickerLSL can validate the echo
        lastEvent  = "Flicker_Start";
        lastDetail = objectId;

        ExperimentLogger.Instance?.LogEvent(lastEvent,
            $"Object: {gameObject.name}, Hz: {GlobalInput.Instance.flickerHz}", "Flickering_Start");
        LSL_Logger.Instance?.LogEvent(lastEvent, lastDetail, "Flickering_Start");

        yield return new WaitForSecondsRealtime(GlobalInput.Instance.flickerDuration);

        outline.ResetOutline();
        isFlickering = false;

        ExperimentLogger.Instance?.LogEvent("Flicker_End", $"Object: {gameObject.name}", "Flickering_Completed");
        LSL_Logger.Instance?.LogEvent("Flicker_End", $"Object: {gameObject.name}", "Flickering_Completed");

        // ── Route by experiment mode ─────────────────────────────────────────
        if (!IsBCIMode())
        {
            // EyeTracking: execute immediately, no LSL wait
            ExecuteAction(selectedAction);
            yield break;
        }

        // Hybrid / BCI: park here and wait for HandleFlickerLSL to fire
        isWaitingForLSL = true;
        waitingObject   = this;
        Debug.Log($"[OB] {gameObject.name} ({objectId}) is WaitingForLSL.");
    }
    #endregion
    
    #region Execute Action
    private void ExecuteAction(ActionType action)
    {
        ExperimentLogger.Instance?.LogEvent("Action_Executed",
            $"Object: {gameObject.name}, Action: {action}", "Execution_Proceeding");
        LSL_Logger.Instance?.LogEvent("Action_Executed",
            $"Object: {gameObject.name}, Action: {action}", "Execution_Proceeding");

        switch (action)
        {
            case ActionType.None: break;
            case ActionType.DoorSwitch:
                demoScene?.Door2Active();
                break;
            case ActionType.NextScene:
                MainControl.Instance.GoToNextPhase();
                break;
            case ActionType.MoveToSelectedDoor:
                if(test3D != null)
                {
                    test3D.walk((int)moveCode);
                }
                break;
            case ActionType.FinalScreen:
                Debug.Log("Final Screen Action Triggered!");
                break;
        }
    }
#endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region LSL Response Handler

    /// <summary>
    /// Called by LSLCommunicationManager whenever a Flicker code (100/101) arrives.
    /// Validates 4 conditions before acting:
    ///   1. Ownership  — this object is the one waiting
    ///   2. State      — isWaitingForLSL is true
    ///   3. Event match — Python echoed back "Flicker_Start"
    ///   4. Detail match — Python echoed back our objectId
    /// </summary>
    private void HandleFlickerLSL(bool detected, BCIMessage msg)
    {
        // 1. Ownership
        if (waitingObject != this) return;

        // 2. State guard
        if (!isWaitingForLSL) return;

        // 3. Event match
        if (msg.Event != lastEvent) return;

        // 4. Detail match (objectId)
        if (msg.Detail != lastDetail) return;

        Debug.Log($"[OB] Valid LSL response for '{gameObject.name}': detected={detected}");

        if (detected)
        {
            // Clean up ownership and execute the configured action
            waitingObject   = null;
            isWaitingForLSL = false;
            ExecuteAction(selectedAction);
        }
        else
        {
            // Python reported no detection — retry the flicker sequence
            StartCoroutine(RetryFlicker());
        }
    }

    /// <summary>
    /// Brief pause, then re-runs the flicker window before re-entering the wait state.
    /// Does NOT re-log Dwell events — only the flicker portion is repeated.
    /// </summary>
    private IEnumerator RetryFlicker()
    {
        Debug.Log($"[OB] Retrying flicker for '{gameObject.name}'.");

        isWaitingForLSL = false;

        // Short gap so the EEG epoch window is clean
        yield return new WaitForSecondsRealtime(0.3f);

        isFlickering = true;
        flicker.StartFlicker();

        // Re-send the LSL marker with the same event/detail so Python knows
        LSL_Logger.Instance?.LogEvent(lastEvent, lastDetail, "Flickering_Retry");

        yield return new WaitForSecondsRealtime(GlobalInput.Instance.flickerDuration);

        outline.ResetOutline();
        isFlickering = false;

        // Back to waiting — HandleFlickerLSL will fire again when Python responds
        isWaitingForLSL = true;
        waitingObject   = this;
        Debug.Log($"[OB] '{gameObject.name}' re-entered WaitingForLSL after retry.");
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Helpers

    /// <summary>
    /// Returns true when the current experiment mode requires LSL confirmation
    /// before executing an action (Hybrid or BCI).
    /// </summary>
    private bool IsBCIMode()
    {
        if (MainControl.Instance == null) return false;
        var exp = MainControl.Instance.currentExperiment;
        return exp == MainControl.ExperimentType.BCI ||
               exp == MainControl.ExperimentType.Hybrid;
    }
    #endregion
}