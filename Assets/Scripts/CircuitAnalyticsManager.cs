using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// =========================================================================================
// LESSON 1 (ELECTRIC CIRCUITS): RESEARCH ANALYTICS MANAGER (Singleton)
// =========================================================================================
// Context: Collects research evaluation metrics during the student's Lesson 1 session:
// - AR interaction time (time spent pointing camera & wiring / stacking batteries)
// - 2D Quiz interaction time & error counts
// - Total session duration and retry attempts
// Data is uploaded to Firebase Realtime Database (/sessions) with an offline PlayerPrefs cache
// and locally backed up to a CSV file in Application.persistentDataPath.
public class CircuitAnalyticsManager : MonoBehaviour
{
    // Global singleton instance accessible by all Lesson 1 scripts
    public static CircuitAnalyticsManager Instance { get; private set; }

    [Header("Firebase Settings")]
    [Tooltip("Firebase Realtime Database REST endpoint")]
    public string firebaseDatabaseURL = "PASTE_YOUR_FIREBASE_URL_HERE";

    // --- Session Metrics ---
    private string sessionLanguage = "English";

    // 1. AR Interaction Timers
    private float arTimerStart = 0f;
    private float arTotalTime = 0f;
    private bool arTimerRunning = false;

    // 2. 2D Quiz Timers & Errors
    private float quizTimerStart = 0f;
    private float quizTotalTime = 0f;
    private bool quizTimerRunning = false;
    private int totalQuizErrors = 0;
    private int totalQuizQuestions = 0;

    // 3. Overall Session Duration & Retry Count
    private float sessionStartTime = 0f;
    private int totalRetries = 0;

    private bool sessionSubmitted = false;
    private string csvFilePath;
    private string deviceId;

    private void Awake()
    {
        // Enforce persistent singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        deviceId = SystemInfo.deviceUniqueIdentifier;

        // Initialize local CSV file header if not already created
        csvFilePath = Path.Combine(Application.persistentDataPath, "AR_CircuitLesson_analytics.csv");
        if (!File.Exists(csvFilePath))
        {
            File.WriteAllText(csvFilePath,
                "Timestamp,Device ID,Language,AR Time (s),Quiz Time (s),Interaction Time (s),Quiz Errors,Quiz Total Questions,Quiz Error Rate,Session Duration (s),Retry Count\n");
        }

        ResetSession();
    }

    private void Start()
    {
        // Flush any offline cached payloads that couldn't be sent during previous sessions
        string cached = PlayerPrefs.GetString("CircuitFirebaseOfflineCache", "");
        if (!string.IsNullOrEmpty(cached))
        {
            string[] entries = cached.Split(new string[] { "|||" }, System.StringSplitOptions.RemoveEmptyEntries);
            Debug.Log($"[CircuitAnalytics] Flushing {entries.Length} offline Firebase sessions...");

            PlayerPrefs.DeleteKey("CircuitFirebaseOfflineCache");
            PlayerPrefs.Save();

            foreach (string entry in entries)
            {
                StartCoroutine(PostToFirebase(entry));
            }
        }
    }

    // --- Public Analytics API ---

    // Called when user selects their language (English / Sinhala) at start of lesson
    public void SetLanguage(string language)
    {
        if (sessionSubmitted) ResetSession();
        
        sessionLanguage  = language;
        sessionStartTime = Time.realtimeSinceStartup;
        Debug.Log($"[CircuitAnalytics] Language: {sessionLanguage} | Session started.");
    }

    // AR phase timers (invoked during wiring and battery stacking)
    public void StartARTimer()
    {
        if (arTimerRunning) return;
        arTimerStart = Time.realtimeSinceStartup;
        arTimerRunning = true;
    }

    public void StopARTimer()
    {
        if (!arTimerRunning) return;
        float elapsed = Time.realtimeSinceStartup - arTimerStart;
        arTotalTime += elapsed;
        arTimerRunning = false;
    }

    // 2D Quiz phase timers
    public void StartQuizTimer()
    {
        if (quizTimerRunning) return;
        quizTimerStart = Time.realtimeSinceStartup;
        quizTimerRunning = true;
    }

    public void StopQuizTimer()
    {
        if (!quizTimerRunning) return;
        float elapsed = Time.realtimeSinceStartup - quizTimerStart;
        quizTotalTime += elapsed;
        quizTimerRunning = false;
    }

    // Called whenever the student picks an incorrect answer
    public void RecordQuizError()
    {
        totalQuizErrors++;
    }

    public void AddQuizQuestions(int count)
    {
        totalQuizQuestions += count;
    }

    // Called whenever student taps 'Retry' in AR wiring or battery stacking
    public void RecordRetry()
    {
        totalRetries++;
    }

    // --- Submits Completed Session ---
    // Finalizes timers, writes to local CSV, and posts JSON to Firebase
    public void SubmitSession(System.Action onComplete = null)
    {
        if (sessionSubmitted)
        {
            onComplete?.Invoke();
            return;
        }
        if (arTimerRunning) StopARTimer();
        if (quizTimerRunning) StopQuizTimer();
        sessionSubmitted = true;

        SaveToCSV();

        string json = BuildJSON();
        StartCoroutine(PostToFirebase(json, onComplete));

        Debug.Log("[CircuitAnalytics] Session submitted. Locked until next lesson start.");
    }

    // Clears metrics for a fresh lesson attempt
    public void ResetSession()
    {
        sessionLanguage = "English";
        arTimerStart = 0f;
        arTotalTime = 0f;
        arTimerRunning = false;
        quizTimerStart = 0f;
        quizTotalTime = 0f;
        quizTimerRunning = false;
        totalQuizErrors = 0;
        totalQuizQuestions = 0;
        totalRetries = 0;
        sessionStartTime = Time.realtimeSinceStartup;
        sessionSubmitted = false;
    }

    // --- JSON Serialization for Firebase ---
    private string BuildJSON()
    {
        float sessionDuration = Time.realtimeSinceStartup - sessionStartTime;
        float interactionTime = arTotalTime + quizTotalTime;
        float errorRate = (totalQuizQuestions > 0)
            ? (float)totalQuizErrors / totalQuizQuestions
            : 0f;
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        return "{"
            + $"\"timestamp\":\"{timestamp}\","
            + $"\"lesson_name\":\"Circuit Lesson\","
            + $"\"device_id\":\"{deviceId}\","
            + $"\"language\":\"{sessionLanguage}\","
            + $"\"ar_time\":{arTotalTime:F1},"
            + $"\"quiz_time\":{quizTotalTime:F1},"
            + $"\"interaction_time\":{interactionTime:F1},"
            + $"\"quiz_errors\":{totalQuizErrors},"
            + $"\"quiz_total_questions\":{totalQuizQuestions},"
            + $"\"quiz_error_rate\":{errorRate:F2},"
            + $"\"session_duration\":{sessionDuration:F1},"
            + $"\"retry_count\":{totalRetries}"
            + "}";
    }

    // --- Firebase Web Request ---
    private IEnumerator PostToFirebase(string json, System.Action onComplete = null)
    {
        Debug.Log($"[CircuitAnalytics] Sending to Firebase:\n{json}");

        if (string.IsNullOrEmpty(firebaseDatabaseURL)
            || firebaseDatabaseURL == "PASTE_YOUR_FIREBASE_URL_HERE")
        {
            Debug.LogWarning("[CircuitAnalytics] Firebase URL not set!");
            onComplete?.Invoke();
            yield break;
        }

        string url     = firebaseDatabaseURL.TrimEnd('/') + "/sessions.json";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[CircuitAnalytics] Firebase OK: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogWarning($"[CircuitAnalytics] Firebase failed: {request.error}. Saving offline.");
                SaveFirebaseOffline(json);
            }
        }

        onComplete?.Invoke();
    }

    // Caches data in PlayerPrefs if offline / network error
    private void SaveFirebaseOffline(string json)
    {
        string existing = PlayerPrefs.GetString("CircuitFirebaseOfflineCache", "");
        if (!string.IsNullOrEmpty(existing)) existing += "|||";
        existing += json;
        PlayerPrefs.SetString("CircuitFirebaseOfflineCache", existing);
        PlayerPrefs.Save();
        Debug.Log("[CircuitAnalytics] Firebase payload saved offline.");
    }

    // Appends metrics to local CSV file
    private void SaveToCSV()
    {
        float sessionDuration = Time.realtimeSinceStartup - sessionStartTime;
        float interactionTime = arTotalTime + quizTotalTime;
        float errorRate = (totalQuizQuestions > 0)
            ? (float)totalQuizErrors / totalQuizQuestions
            : 0f;
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        string row = $"{timestamp},{deviceId},{sessionLanguage},{arTotalTime:F1},{quizTotalTime:F1},{interactionTime:F1},{totalQuizErrors},{totalQuizQuestions},{errorRate:F2},{sessionDuration:F1},{totalRetries}\n";

        try { File.AppendAllText(csvFilePath, row); }
        catch (System.Exception ex) { Debug.LogError($"[CircuitAnalytics] CSV error: {ex.Message}"); }
    }

    // Save session automatically if app is closed or minimized
    private void OnApplicationQuit()
    {
        if (!sessionSubmitted)
        {
            if (arTimerRunning) StopARTimer();
            if (quizTimerRunning) StopQuizTimer();
            sessionSubmitted = true;
            SaveToCSV();
            string json = BuildJSON();
            SaveFirebaseOffline(json);
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && !sessionSubmitted)
        {
            if (arTimerRunning) StopARTimer();
            if (quizTimerRunning) StopQuizTimer();
            sessionSubmitted = true;
            SaveToCSV();
            string json = BuildJSON();
            SaveFirebaseOffline(json);
        }
    }
}
