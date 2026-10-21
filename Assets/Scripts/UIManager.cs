using UnityEngine;
using TMPro;

// =========================================================================================
// APP-WIDE: MAIN UI & NAVIGATION MANAGER
// =========================================================================================
// Context: Top-level controller for 2D UI panels and overall app flow:
// - Initial Language Selection (English / Sinhala) and live language toggling
// - Student Login (Name & Grade 4/5 selection)
// - Home Dashboard, All Lessons menu, Tutorial, and AR Scanner navigation
// - Manages AR Camera state and toggles Vuforia tracking observers to prevent accidental triggers.
public class UIManager : MonoBehaviour
{
    // --- Primary Navigation Panels ---
    [Header("UI Panels")]
    public GameObject languagePanel; 
    public GameObject loginPanel;
    public GameObject homePanel;
    public GameObject tutorialPanel; 
    public GameObject arScannerPanel; 
    public GameObject allLessonsPanel;
    
    // --- AR Activity & Narrative Flow Panels ---
    [Header("The New AR Flow Panels")]
    public GameObject arQuizUIPanel; 
    public GameObject storyRevealPanel;
    
    // --- Story Panels from Other Lessons ---
    [Header("Duplicate Story Panels")]
    [Tooltip("Story reveal panels for Lesson 2 and 3 closed when returning Home")]
    public GameObject[] extraStoryPanels;

    // --- Student Login & Greeting Displays ---
    [Header("Login & Feedback Texts")]
    public TMP_InputField studentNameInput;
    public TextMeshProUGUI welcomeTextEnglish; 
    public TextMeshProUGUI welcomeTextSinhala; 
    public TextMeshProUGUI quizFeedbackText; 

    // --- Vuforia AR Camera Object ---
    [Header("AR Camera Setup")]
    public GameObject arCamera; 

    private string temporaryLanguageChoice = "English"; 

    void Start()
    {
        // Start app on Language Selection screen
        ShowPanel(languagePanel);
    }

    // --- Language Selection Callbacks ---
    public void OnEnglishSelected() { temporaryLanguageChoice = "English"; }
    public void OnSinhalaSelected() { temporaryLanguageChoice = "Sinhala"; }

    public void OnConfirmLanguageClicked()
    {
        ApplyLanguageSettings();
        ShowPanel(loginPanel);
    }

    // Mid-game language toggle button
    public void ForceLanguageEnglish()
    {
        temporaryLanguageChoice = "English";
        ApplyLanguageSettings();
    }

    public void ForceLanguageSinhala()
    {
        temporaryLanguageChoice = "Sinhala";
        ApplyLanguageSettings();
    }
    
    public void ToggleLanguage()
    {
        HapticFeedbackManager.TriggerSubtleHaptic(30L);
        string currentLang = PlayerPrefs.GetString("Language", "English");
        temporaryLanguageChoice = (currentLang == "English") ? "Sinhala" : "English";
        ApplyLanguageSettings();
    }

    // Saves language to PlayerPrefs and notifies all LocalizedText components
    private void ApplyLanguageSettings()
    {
        PlayerPrefs.SetString("Language", temporaryLanguageChoice);
        PlayerPrefs.Save();

        // Broadcast language change to all active and inactive text objects
        LocalizedText[] allTextManagers = FindObjectsOfType<LocalizedText>(true);
        foreach (LocalizedText textManager in allTextManagers)
        {
            textManager.UpdateLanguage();
        }

        if (CircuitAnalyticsManager.Instance != null)
            CircuitAnalyticsManager.Instance.SetLanguage(temporaryLanguageChoice);
    }

    // --- Student Profile Login Handlers ---
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

    // --- Dashboard & Navigation Buttons ---
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
        Application.Quit(); 
    }
    
    public void OnSkipTutorialClicked() 
    { 
        HapticFeedbackManager.TriggerSubtleHaptic(30L);
        ShowPanel(arScannerPanel); 
    }

    // --- AR Quiz Panel Transitions ---
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

    // Closes storybook panels and finalizes session analytics
    public void CloseStoryPanel()
    {
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

    // --- Master Panel Switcher ---
    // Activates target panel, hides others, and configures AR Camera & Vuforia tracking state
    private void ShowPanel(GameObject panelToShow)
    {
        if (languagePanel != null) languagePanel.SetActive(false); 
        if (loginPanel != null) loginPanel.SetActive(false);
        if (homePanel != null) homePanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false); 
        if (arScannerPanel != null) arScannerPanel.SetActive(false);
        if (allLessonsPanel != null) allLessonsPanel.SetActive(false);
        if (arQuizUIPanel != null) arQuizUIPanel.SetActive(false);
        if (storyRevealPanel != null) storyRevealPanel.SetActive(false);
        
        if (extraStoryPanels != null)
        {
            foreach (GameObject extraPanel in extraStoryPanels)
            {
                if (extraPanel != null && extraPanel != panelToShow)
                    extraPanel.SetActive(false);
            }
        }
        
        // Hide any story manager instances active in the hierarchy
        StoryUIManager[] allStories = FindObjectsOfType<StoryUIManager>(true);
        foreach (var story in allStories) 
        {
            if (story != null && story.gameObject != panelToShow)
                story.gameObject.SetActive(false);
        }

        if (panelToShow != null) panelToShow.SetActive(true);

        // Turn AR Camera on only for camera/AR-dependent panels
        if (arCamera != null)
        {
            if (panelToShow == arScannerPanel || panelToShow == arQuizUIPanel || panelToShow == storyRevealPanel)
                arCamera.SetActive(true);
            else
                arCamera.SetActive(false);
        }

        // Enable Vuforia marker tracking only during scanner and quiz phases to prevent premature triggers
        bool shouldTrack = (panelToShow == arScannerPanel || panelToShow == arQuizUIPanel);
        Vuforia.ObserverBehaviour[] allTrackers = FindObjectsOfType<Vuforia.ObserverBehaviour>(true);
        foreach (Vuforia.ObserverBehaviour tracker in allTrackers)
        {
            tracker.enabled = shouldTrack;
        }
    }
}