using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject languagePanel; 
    public GameObject loginPanel;
    public GameObject homePanel;
    public GameObject tutorialPanel; 
    public GameObject arScannerPanel; 
    public GameObject allLessonsPanel;
    
    [Header("The New AR Flow Panels")]
    public GameObject arQuizUIPanel; 
    public GameObject storyRevealPanel;
    
    [Header("Duplicate Story Panels")]
    [Tooltip("Drag the story reveal panels for Lesson 2 and 3 here so they close when going Home!")]
    public GameObject[] extraStoryPanels;

    [Header("Login & Feedback Texts")]
    public TMP_InputField studentNameInput;
    public TextMeshProUGUI welcomeTextEnglish; 
    public TextMeshProUGUI welcomeTextSinhala; 
    public TextMeshProUGUI quizFeedbackText; 

    [Header("AR Camera Setup")]
    public GameObject arCamera; 

    private string temporaryLanguageChoice = "English"; 

    void Start()
    {
        ShowPanel(languagePanel);
    }

    // --- LANGUAGE SELECTION LOGIC ---
    public void OnEnglishSelected() { temporaryLanguageChoice = "English"; }
    public void OnSinhalaSelected() { temporaryLanguageChoice = "Sinhala"; }

    public void OnConfirmLanguageClicked()
    {
        ApplyLanguageSettings();
        
        // Move to the Login Screen
        ShowPanel(loginPanel);
    }

    // --- MID-GAME LANGUAGE SWITCHING (NEW) ---
    public void ForceLanguageEnglish()
    {
        temporaryLanguageChoice = "English";
        ApplyLanguageSettings();
        Debug.Log("Forced language back to English");
    }

    public void ForceLanguageSinhala()
    {
        temporaryLanguageChoice = "Sinhala";
        ApplyLanguageSettings();
        Debug.Log("Forced language to Sinhala");
    }
    
    public void ToggleLanguage()
    {
        HapticFeedbackManager.TriggerSubtleHaptic(30L);
        
        string currentLang = PlayerPrefs.GetString("Language", "English");
        
        if (currentLang == "English")
        {
            temporaryLanguageChoice = "Sinhala";
        }
        else
        {
            temporaryLanguageChoice = "English";
        }
        
        ApplyLanguageSettings();
        Debug.Log("Toggled language to: " + temporaryLanguageChoice);
    }

    private void ApplyLanguageSettings()
    {
        // 1. Save the new language choice to memory
        PlayerPrefs.SetString("Language", temporaryLanguageChoice);
        PlayerPrefs.Save();

        // 2. Find EVERY LocalizedText script in the entire game and force them to update instantly!
        LocalizedText[] allTextManagers = FindObjectsOfType<LocalizedText>(true);
        foreach (LocalizedText textManager in allTextManagers)
        {
            textManager.UpdateLanguage();
        }

        // 3. Analytics
        if (CircuitAnalyticsManager.Instance != null)
            CircuitAnalyticsManager.Instance.SetLanguage(temporaryLanguageChoice);
    }

    // --- LOGIN LOGIC ---
    public void OnGrade4LoginClicked() 
    { 
        HapticFeedbackManager.TriggerSubtleHaptic(30L);
        SaveStudentData(4); 
    }
    public void OnGrade5LoginClicked() 
    { 
        HapticFeedbackManager.TriggerSubtleHaptic(30L);
        SaveStudentData(5); 
    }

    private void SaveStudentData(int gradeLevel)
    {
        string nameToSave = studentNameInput.text;
        if (string.IsNullOrEmpty(nameToSave)) nameToSave = "Explorer"; 
        
        PlayerPrefs.SetString("StudentName", nameToSave);
        PlayerPrefs.SetInt("StudentGrade", gradeLevel);
        PlayerPrefs.Save(); 

        if (welcomeTextEnglish != null) welcomeTextEnglish.text = "Hello, " + nameToSave + "!";

        if (welcomeTextSinhala != null) 
        {
            welcomeTextSinhala.text = "wdhqfndajka <font=\"LiberationSans SDF\">" + nameToSave + "!</font>"; 
        }

        ShowPanel(homePanel);
    }

    // --- HOME MENU & TUTORIAL LOGIC ---
    public void OnHomeClicked() 
    { 
        HapticFeedbackManager.TriggerSubtleHaptic(30L);
        ShowPanel(homePanel); 
    }
    
    public void OnScanImageClicked() 
    { 
        HapticFeedbackManager.TriggerSubtleHaptic(30L);
        ShowPanel(arScannerPanel); 
    }
    
    public void OnAllLessonsClicked() 
    { 
        HapticFeedbackManager.TriggerSubtleHaptic(30L);
        ShowPanel(allLessonsPanel); 
    }
    
    public void OnQuitClicked() 
    { 
        HapticFeedbackManager.TriggerSubtleHaptic(30L);
        Debug.Log("Quit App Clicked");
        Application.Quit(); 
    }
    
    public void OnSkipTutorialClicked() 
    { 
        HapticFeedbackManager.TriggerSubtleHaptic(30L);
        ShowPanel(arScannerPanel); 
    }

    // --- AR QUIZ LOGIC ---
    public void StartARQuiz()
    {
        if (quizFeedbackText != null) quizFeedbackText.text = ""; 
        ShowPanel(arQuizUIPanel);
    }

    public void OnQuizPass()
    {
        ShowPanel(storyRevealPanel);
    }

    public void OnQuizFail()
    {
        if (quizFeedbackText != null)
        {
            quizFeedbackText.color = Color.red;
            quizFeedbackText.text = "Not quite! Check your book and try again.";
        }
    }

    public void OnNextScanClicked() { ShowPanel(arScannerPanel); }

    // --- NEW FUNCTION TO CLOSE THE PANEL ---
    public void CloseStoryPanel()
    {
        // ── Analytics: user is leaving the lesson — submit if not already done ──
        if (CircuitAnalyticsManager.Instance != null)
            CircuitAnalyticsManager.Instance.SubmitSession();

        if (storyRevealPanel != null) storyRevealPanel.SetActive(false);
        
        if (extraStoryPanels != null)
        {
            foreach (GameObject extraPanel in extraStoryPanels)
            {
                if (extraPanel != null) extraPanel.SetActive(false);
            }
        }
    }

    // --- MASTER PANEL CONTROLLER ---
    private void ShowPanel(GameObject panelToShow)
    {
        if(languagePanel != null) languagePanel.SetActive(false); 
        if(loginPanel != null) loginPanel.SetActive(false);
        if(homePanel != null) homePanel.SetActive(false);
        if(tutorialPanel != null) tutorialPanel.SetActive(false); 
        if(arScannerPanel != null) arScannerPanel.SetActive(false);
        if(allLessonsPanel != null) allLessonsPanel.SetActive(false);
        if(arQuizUIPanel != null) arQuizUIPanel.SetActive(false);
        if(storyRevealPanel != null) storyRevealPanel.SetActive(false);
        
        // Hide any extra story panels explicitly linked in the inspector
        if (extraStoryPanels != null)
        {
            foreach (GameObject extraPanel in extraStoryPanels)
            {
                if (extraPanel != null && extraPanel != panelToShow)
                {
                    extraPanel.SetActive(false);
                }
            }
        }
        
        // --- Force hide ALL duplicate story panels in the scene (Catch-all) ---
        StoryUIManager[] allStories = FindObjectsOfType<StoryUIManager>(true);
        foreach(var story in allStories) 
        {
            if (story != null && story.gameObject != panelToShow)
            {
                story.gameObject.SetActive(false);
            }
        }

        if(panelToShow != null) panelToShow.SetActive(true);

        if (arCamera != null)
        {
            // NEW FIX: Added "|| panelToShow == storyRevealPanel" right here!
            if (panelToShow == arScannerPanel || panelToShow == arQuizUIPanel || panelToShow == storyRevealPanel)
            {
                arCamera.SetActive(true);
            }
            else
            {
                arCamera.SetActive(false);
            }
        }

        // --- PREVENT PREMATURE LESSON TRIGGERS ---
        // Keep tracking ON for the scanner and quiz. Turn it OFF for the story reveal panel, 
        // so the camera feed stays active but doesn't accidentally trigger the next lesson!
        bool shouldTrack = (panelToShow == arScannerPanel || panelToShow == arQuizUIPanel);
        
        Vuforia.ObserverBehaviour[] allTrackers = FindObjectsOfType<Vuforia.ObserverBehaviour>(true);
        foreach (Vuforia.ObserverBehaviour tracker in allTrackers)
        {
            tracker.enabled = shouldTrack;
        }
    }
}