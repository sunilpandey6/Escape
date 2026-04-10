using System;
using System.IO;
using UnityEngine;

public class ExperimentLogger : MonoBehaviour
{
    public static ExperimentLogger Instance { get; private set; }

    private string filePath;
    private StreamWriter writer;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeLogger();
    }

    private void InitializeLogger()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path.Combine(Application.dataPath, $"ExperimentLog_{timestamp}.csv");

        try
        {
            writer = new StreamWriter(filePath, true);
            // Write CSV Header
            writer.WriteLine("Timestamp,ExperimentType,Phase,EventName,Details");
            writer.Flush();
            Debug.Log($"[ExperimentLogger] CSV initialized at: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ExperimentLogger] Failed to create log file: {e.Message}");
        }
    }

    /// <summary>
    /// Logs an event to the CSV file.
    /// Event examples: "Dwell_Start", "Flicker_End", "Phase_Transition".
    /// </summary>
    public void LogEvent(string eventName, string details = "")
    {
        if (writer == null) return;

        string time = DateTime.Now.ToString("HH:mm:ss.fff");
        string expType = "Unknown";
        string phase = "Unknown";

        // Pull current state metadata if MainControl is active
        if (MainControl.Instance != null)
        {
            expType = MainControl.Instance.currentExperiment.ToString();
            phase = MainControl.Instance.currentPhase.ToString();
        }

        // Clean details string to prevent breaking CSV formatting
        string safeDetails = details.Replace(",", ";").Replace("\n", " ");

        string line = $"{time},{expType},{phase},{eventName},{safeDetails}";
        
        try
        {
            writer.WriteLine(line);
            writer.Flush();
        }
        catch (Exception e)
        {
            Debug.LogError($"[ExperimentLogger] Failed to write to log file: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        CloseLogger();
    }

    private void OnApplicationQuit()
    {
        CloseLogger();
    }

    private void CloseLogger()
    {
        if (writer != null)
        {
            LogEvent("System_Stop", "Application Quit or Logger Destroyed");
            writer.Close();
            writer = null;
        }
    }
}
