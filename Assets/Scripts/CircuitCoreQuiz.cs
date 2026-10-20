using System.Collections; 
using UnityEngine;
using TMPro;

public class CircuitCoreQuiz : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject question1Panel; 
    public GameObject question2Panel; 
    public GameObject feedbackPanel;  
    
    [Header("Text Elements")]
    public TextMeshProUGUI feedbackText_EN; 
    public TextMeshProUGUI feedbackText_SN; 
    
    [Header("Connections")]
    public GameObject addBatteryButton; 
    public GameObject takeQuizButton;   
    public GameObject resetQuizButton; 
    public StoryUIManager storyManager;
    public GameObject finalRevealStoryButton; 
    
    [Header("Question 1 Audio (EN & SN)")]
    public AudioClip q1QuestionVoiceEN;
    public AudioClip q1QuestionVoiceSN;
    public AudioClip q1WrongWiresVoiceEN; 
    public AudioClip q1WrongWiresVoiceSN; 
    public AudioClip q1WrongBulbVoiceEN;  
    public AudioClip q1WrongBulbVoiceSN;  
    public AudioClip q1RightVoiceEN;
    public AudioClip q1RightVoiceSN;

    [Header("Question 2 Audio (EN & SN)")]
    public AudioClip q2QuestionVoiceEN;
    public AudioClip q2QuestionVoiceSN;
    public AudioClip q2WrongVoiceEN;
    public AudioClip q2WrongVoiceSN;
    public AudioClip q2RightVoiceEN;
    public AudioClip q2RightVoiceSN;

    public bool isQuizActive = false; 
    
    // Safety flag to prevent button mashing during transitions!
    private bool isTransitioning = false; 
    
    private Coroutine activeFeedbackTimer;
    private AudioSource localAudioSource; // NEW: Local audio source to prevent inactive GameObject issues

    // --- STRINGS ---
    private string q1WrongWiresEN = "Not quite! Adding more wires just makes the electricity travel further. It doesn't add any extra power to the circuit. Try again!";
    private string q1WrongWiresSN = ";j;a jh¾ tl;= l<du fjkafka úÿ,shg .uka lrkak ;sfhk ÿr jeä fjk tl ú;rhs mßm:hg wu;r n,hla tl;= fjkafka kE wdfh;a W;aidy lr,d n,uq";
    
    private string q1WrongBulbEN = "Oops! A bigger bulb actually needs MORE power to light up. If we don't add a stronger power source first, it won't work! Try again.";
    private string q1WrongBulbSN = "f,dl= n,aí tlla m;a;= fjkak ;j;a f.dvla n,h ´fka uq,skau wms ;j;a n,h tl;= lf<a ke;akï tal jev lrkafka kE wdfh;a lr,d n,uq";

    private string q1RightEN = "Excellent! Connecting more batteries gives the circuit much more power, making the bulb shine brighter!";
    private string q1RightSN = "kshuhs mßm:hg ;j;a negß tl;= l<du f.dvla n,h ,efnkjd ta ksid n,aí tl ;j;a fyd|g t<sh fokjd";

    private string q2WrongEN = "A Parallel connection makes batteries last longer, but it doesn't increase brightness! Try the other one.";
    private string q2WrongSN = "iudka;r.;j l<du negß f.dvla fj,d mdúÉÑ lrkak mq¿jka yenehs n,aí tfla t<sh jeä fjkafka kE";

    private string q2RightEN = "Correct! A Series connection adds the power of the batteries together. Point your phone back at the storybook to see your circuit, then let's add them!";
    private string q2RightSN = "yßu W;a;f¾ fYa%Ks.;j ^tl fm<g& iïnkaO l<du negßj, n,h tlg tl;= fjkjd Thdf.a mßm:h n,kak f*daka tl wdfh;a l;kaor fmd; Èydjg yrjkak Bgmiafia wms negß iïnkaO lruq";

    private void Awake()
    {
        localAudioSource = gameObject.AddComponent<AudioSource>();
        localAudioSource.playOnAwake = false;
    }

    // --- START LOGIC ---
    public void StartQuiz()
    {
        isQuizActive = true; 
        isTransitioning = false; 

        if (storyManager != null) storyManager.ClearText();

        question1Panel.SetActive(true);
        if (takeQuizButton != null) takeQuizButton.SetActive(false);
        if (resetQuizButton != null) resetQuizButton.SetActive(false);

        // ── Analytics: quiz begins; 2 questions total in this lesson ──
        if (CircuitAnalyticsManager.Instance != null)
        {
            CircuitAnalyticsManager.Instance.AddQuizQuestions(2);
            CircuitAnalyticsManager.Instance.StartQuizTimer();
        }

        PlayAudioByLanguage(q1QuestionVoiceEN, q1QuestionVoiceSN);
    }

    // --- QUESTION 1 LOGIC ---
    public void OnQ1WrongWires()
    {
        HapticFeedbackManager.TriggerSubtleHaptic(40L);
        if (isTransitioning) return; 
        // ── Analytics: wrong answer ──
        if (CircuitAnalyticsManager.Instance != null)
            CircuitAnalyticsManager.Instance.RecordQuizError();
        ShowAutoHidingFeedback(q1WrongWiresEN, q1WrongWiresSN, 4f, q1WrongWiresVoiceEN, q1WrongWiresVoiceSN);
    }

    public void OnQ1WrongBulb()
    {
        HapticFeedbackManager.TriggerSubtleHaptic(40L);
        if (isTransitioning) return;
        // ── Analytics: wrong answer ──
        if (CircuitAnalyticsManager.Instance != null)
            CircuitAnalyticsManager.Instance.RecordQuizError();
        ShowAutoHidingFeedback(q1WrongBulbEN, q1WrongBulbSN, 5f, q1WrongBulbVoiceEN, q1WrongBulbVoiceSN);
    }

    public void OnQ1CorrectAnswer()
    {
        HapticFeedbackManager.TriggerSubtleHaptic(40L);
        if (isTransitioning) return;
        isTransitioning = true; // Lock the buttons!

        bool isSinhala = PlayerPrefs.GetString("Language", "English") == "Sinhala";
        AudioClip clipToCheck = isSinhala ? q1RightVoiceSN : q1RightVoiceEN;
        float waitTime = (clipToCheck != null) ? clipToCheck.length + 0.5f : 4f;

        ShowAutoHidingFeedback(q1RightEN, q1RightSN, 4f, q1RightVoiceEN, q1RightVoiceSN);
        
        StartCoroutine(TransitionToQ2(waitTime)); 
    }

    private IEnumerator TransitionToQ2(float waitTime)
    {
        yield return new WaitForSeconds(waitTime); 
        
        question1Panel.SetActive(false); 
        question2Panel.SetActive(true); 
        
        isTransitioning = false; // Unlock the buttons for Question 2

        PlayAudioByLanguage(q2QuestionVoiceEN, q2QuestionVoiceSN);
    }

    // --- QUESTION 2 LOGIC ---
    public void OnQ2ParallelSelected() 
    {
        HapticFeedbackManager.TriggerSubtleHaptic(40L);
        if (isTransitioning) return;
        // ── Analytics: wrong answer ──
        if (CircuitAnalyticsManager.Instance != null)
            CircuitAnalyticsManager.Instance.RecordQuizError();
        ShowAutoHidingFeedback(q2WrongEN, q2WrongSN, 4f, q2WrongVoiceEN, q2WrongVoiceSN);
    }

    public void OnQ2SeriesSelected() 
    {
        HapticFeedbackManager.TriggerSubtleHaptic(40L);
        if (isTransitioning) return;
        isTransitioning = true;
        isQuizActive = false; 

        question2Panel.SetActive(false); 

        // ── Analytics: correct final answer, stop quiz timer ──
        if (CircuitAnalyticsManager.Instance != null)
            CircuitAnalyticsManager.Instance.StopQuizTimer();

        bool isSinhala = PlayerPrefs.GetString("Language", "English") == "Sinhala";
        AudioClip clipToCheck = isSinhala ? q2RightVoiceSN : q2RightVoiceEN;
        float waitTime = (clipToCheck != null) ? clipToCheck.length + 0.5f : 5f;

        ShowAutoHidingFeedback(q2RightEN, q2RightSN, 5f, q2RightVoiceEN, q2RightVoiceSN);
        
        StartCoroutine(DelayExperimentStart(waitTime));
    }

    private IEnumerator DelayExperimentStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime); 

        if(addBatteryButton != null) addBatteryButton.SetActive(true); 

        // ── Analytics: quiz is complete, AR battery experiment begins ──
        // (SubmitSession was removed from here so we can keep tracking AR time!)

        BrightnessExperimentManager expManager = FindObjectOfType<BrightnessExperimentManager>();
        if (expManager != null) 
        {
            expManager.PlayIntroVoiceover(); 
        }
        
        isTransitioning = false;
    }

    // --- FINAL EXPERIMENT LOGIC ---
    public void OnExperimentComplete()
    {
        StartCoroutine(DelayFinalButton(6f)); 
    }

    private IEnumerator DelayFinalButton(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if(finalRevealStoryButton != null) finalRevealStoryButton.SetActive(true);
    }

    // --- THE AUTO-TIMING FEEDBACK SYSTEM (NOW BILINGUAL) ---
    private void ShowAutoHidingFeedback(string messageEN, string messageSN, float fallbackSeconds, AudioClip voiceEN = null, AudioClip voiceSN = null)
    {
        if (activeFeedbackTimer != null) StopCoroutine(activeFeedbackTimer);
        activeFeedbackTimer = StartCoroutine(FeedbackTimerRoutine(messageEN, messageSN, fallbackSeconds, voiceEN, voiceSN));
    }

    private IEnumerator FeedbackTimerRoutine(string messageEN, string messageSN, float fallbackSeconds, AudioClip voiceEN, AudioClip voiceSN)
    {
        string currentLang = PlayerPrefs.GetString("Language", "English");
        bool isSinhala = (currentLang == "Sinhala");

        feedbackPanel.SetActive(true);

        // Turn on the correct text object and set the text!
        if (isSinhala)
        {
            if (feedbackText_EN != null) feedbackText_EN.gameObject.SetActive(false);
            if (feedbackText_SN != null) 
            {
                feedbackText_SN.gameObject.SetActive(true);
                feedbackText_SN.text = messageSN;
            }
        }
        else
        {
            if (feedbackText_SN != null) feedbackText_SN.gameObject.SetActive(false);
            if (feedbackText_EN != null) 
            {
                feedbackText_EN.gameObject.SetActive(true);
                feedbackText_EN.text = messageEN;
            }
        }

        float exactWaitTime = fallbackSeconds; 
        AudioClip clipToPlay = isSinhala ? voiceSN : voiceEN;

        if (localAudioSource != null)
        {
            localAudioSource.Stop(); 
            
            if (clipToPlay != null)
            {
                localAudioSource.PlayOneShot(clipToPlay);
                exactWaitTime = clipToPlay.length + 0.5f; 
            }
        }

        yield return new WaitForSeconds(exactWaitTime);
        feedbackPanel.SetActive(false);
    }

    // Helper method to play pure voiceovers
    private void PlayAudioByLanguage(AudioClip clipEN, AudioClip clipSN)
    {
        if (localAudioSource == null) return;

        string currentLang = PlayerPrefs.GetString("Language", "English");
        AudioClip clipToPlay = (currentLang == "Sinhala") ? clipSN : clipEN;

        if (clipToPlay != null)
        {
            localAudioSource.Stop();
            localAudioSource.PlayOneShot(clipToPlay);
        }
    }
}