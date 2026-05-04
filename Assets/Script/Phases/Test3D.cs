using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EyeClosed))]
[RequireComponent(typeof(AutoWalk))]
public class Test3D : MonoBehaviour
{
    
    [Header("References")]
    [SerializeField] private AutoWalk autoWalk;
    [SerializeField] private EyeClosed eyeClosed;

#region Unity Lifecycle
    private void Awake()
    {
        if(autoWalk == null) autoWalk = GetComponent<AutoWalk>();
        if(eyeClosed == null) eyeClosed = GetComponent<EyeClosed>();
    }

    private void OnValidate(){
        autoWalk = GetComponent<AutoWalk>();
        eyeClosed = GetComponent<EyeClosed>();
    }

    void Start()
    {
        //show ui for test 3d
        
        //if BCI mode, start the eye closed test and wait for user input
        // if eye closed detect, then assign predict start log
        // wait for LSL manager to get the prediction 
        //after confirmation then manual trigger door for dwell + Flicker
        // then move to door
        // Final UI Close Experiment.
        
        if(IsBCIMode()) 
        {
            
            
        }
        // If BCI mode, 
        else
        {
            
        }
    }
#endregion

#region Eye Closed Check
    public void StartEyeClosedTest()
    {
        eyeClosed.StartChecking();
    }
#endregion

#region Walk to Door
    public void walk(int code)
    {
        autoWalk.MoveToTarget(code);
    }
#endregion


 #region Helpers

    /// <summary>
    /// Returns true when the current experiment mode requires LSL confirmation
    /// before executing an action (Hybrid or BCI).
    /// </summary>
    private bool IsBCIMode()
    {
        if (MainControl.Instance == null) return false;
        var exp = MainControl.Instance.currentExperiment;
        return exp == MainControl.ExperimentType.BCI;
    }
    #endregion
}
