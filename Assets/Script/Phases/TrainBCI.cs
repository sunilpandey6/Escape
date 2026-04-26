using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainBCI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The GameObject representing Door 1")]
    public GameObject Door1;
    
    [Tooltip("The GameObject representing Door 2")]
    public GameObject Door2;
    
    [Tooltip("The UI Canvas shown before and after training")]
    public GameObject IntroductionCanvas;
    [Header("Intro Buttons")]
    public GameObject IntroButton;
    public GameObject IntroNextButton;

    [Header("Training Parameters")]
    [Tooltip("Number of trials (m times) per door")]
    public int m_times = 5;
    
    [Tooltip("Duration in seconds (n sec) to show each door")]
    public float showDuration = 4f;
    
    [Tooltip("Duration in seconds for the blank screen between trials")]
    public float blankDuration = 2f;



    public void Start()
    {
        ShowIntro();
        
    }
    private void OnEnable()
    {
        ShowIntro();
        
    }

    private void OnDisable()
    {
        if (IntroductionCanvas != null) IntroductionCanvas.SetActive(false);
    }

    /// <summary>
    /// Displays the Introduction Canvas and hides the doors.
    /// Also disables the "Next" button so the user must proceed through the training.
    /// </summary>
    public void ShowIntro()
    {
        if (IntroductionCanvas != null) IntroductionCanvas.SetActive(true);
        if (Door1 != null) Door1.SetActive(false);
        if (Door2 != null) Door2.SetActive(false);

        StartIntroButtonUI();
    }

    /// <summary>
    /// Function to disable the introduction canvas and begin the training routine.
    /// Should be linked to the "Start" button on the IntroductionCanvas.
    /// </summary>
    public void StartTraining()
    {
        if (IntroductionCanvas != null) 
        {
            BB[] buttons = IntroductionCanvas.GetComponentsInChildren<BB>(true); 
            foreach (var b in buttons) 
            {
                if (b.gameObject.name == "Intro-UI") 
                {
                    b.gameObject.SetActive(false); 
                }
            } 
            IntroductionCanvas.SetActive(false);
        }
        StartCoroutine(TrainingRoutine());
    }

    private IEnumerator TrainingRoutine()
    {
        // ----------------------------------------------------
        // Phase 1: Train Door 1
        // ----------------------------------------------------
        for (int i = 0; i < m_times; i++)
        {
            if (Door1 != null) Door1.SetActive(true);
            // Show door for n seconds
            yield return new WaitForSeconds(showDuration);
            ExperimentLogger.Instance?.LogEvent("Train_Start", "Door1", "Training_Door1");
            Door1.SetActive(false);
            // Blank screen for blankDuration
            yield return new WaitForSeconds(blankDuration);
            ExperimentLogger.Instance?.LogEvent("Train_End", "Door1", "Training_Blank");
        }

        // ----------------------------------------------------
        // Phase 2: Train Door 2
        // ----------------------------------------------------
        for (int i = 0; i < m_times; i++)
        {
            if (Door2 != null) Door2.SetActive(true);                
            // Show door for n seconds
            yield return new WaitForSeconds(showDuration);
            ExperimentLogger.Instance?.LogEvent("Train_Start", "Door2", "Training_Door2");
            Door2.SetActive(false);
            // Blank screen for blankDuration
            yield return new WaitForSeconds(blankDuration);
            ExperimentLogger.Instance?.LogEvent("Train_End", "Door2", "Training_Blank");   
        }

        // ----------------------------------------------------
        // Training complete: show Introduction Canvas with "End" button
        // ----------------------------------------------------
        if (IntroductionCanvas != null) IntroductionCanvas.SetActive(true);
        IntroNextButtonUI();
    }

    /// <summary>
    /// Hides the "Intro-Next-UI" button at the start of the scene.
    /// </summary>
    public void StartIntroButtonUI() 
    { 
        if (IntroductionCanvas == null) return;
        IntroButton.SetActive(true);
        IntroNextButton.SetActive(false);
    }

    /// <summary>
    /// Shows the "Intro-Next-UI" button at the end of the training routine.
    /// </summary>
    public void IntroNextButtonUI() 
    {
        if (IntroductionCanvas == null) return;
        IntroButton.SetActive(false);
        IntroNextButton.SetActive(true);
    }

    /// <summary>
    /// Proceeds to the next phase of the experiment.
    /// Should be linked to the "Intro-Next-UI" button.
    /// </summary>
    public void NextPhase()
    {
        if (MainControl.Instance != null)
        {
            MainControl.Instance.GoToNextPhase();
        }
    }
}
