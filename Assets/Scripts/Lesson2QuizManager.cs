using System.Collections;
using UnityEngine;
using TMPro;

public class Lesson2QuizManager : MonoBehaviour
{
    // --- THIS CREATES A DROPDOWN IN UNITY ---
    public enum Lesson2QuestionType 
    { 
        Question1_Ramp, 
        Question2_Pulley 
    }
    
    [Header("Select Which Question This Panel Is:")]
    public Lesson2QuestionType thisQuestionType;

    [Header("Quiz Panels")]
    public GameObject currentQuizPanel; 
    public GameObject nextQuizPanel;    

    // --- NEW: Containers to transition between AR scenes ---
    [Header("End of Quiz Transition")]
    [Tooltip("Drag the FIRST AR Scene (Push/Pull) here to hide it!")]
    public GameObject oldARSceneContainer;

    [Tooltip("Drag your Lesson2_Pulley_AR_Scene container here!")]
    public GameObject pulleyARSceneContainer;

    [Header("Feedback UI Elements")]
    public GameObject feedbackPanel;
    public TextMeshProUGUI feedbackTextEN;
    public TextMeshProUGUI feedbackTextSN;

    [Header("Audio Settings")]
    public AudioSource audioPlayer;
    
    [Header("Question Audio")]
    public AudioClip questionAudioEN;
    public AudioClip questionAudioSN;

    [Header("Feedback Audio")]
    public AudioClip correctAudioEN;
    public AudioClip correctAudioSN;
    public AudioClip incorrectAudioEN;
    public AudioClip incorrectAudioSN;

    // ==========================================
    // ALL TEXT IS NOW SAFELY STORED IN THE CODE
    // ==========================================

    // --- QUESTION 1 (RAMP) TEXT ---
    private string q1_correctEN = "Correct! A 100cm ramp is less steep. When the slope is lower, it takes less pushing force to move the crate up!";
    private string q1_correctSN = "ඉතා හොඳයි! තලයේ ආනත බව (steepness) අඩු වන විට යමක් ඉහළට තල්ලු කිරීම පහසු වේ.";
    
    private string q1_incorrectEN = "Not quite! A 150cm ramp is much steeper. Steeper slopes mean you have to use a lot more pushing force on the crate. Try the lower one!";
    private string q1_incorrectSN = "නැවත උත්සාහ කරන්න! 150cm යනු වැඩි ආනතියකි. ආනතිය වැඩි වන විට තල්ලු කිරීමට වැඩි බලයක් යෙදිය යුතුයි.";

    // --- QUESTION 2 (PULLEY) TEXT ---
    private string q2_correctEN = "Excellent! A pulley is a simple machine. It allows you to pull down to lift the crate up, making the heavy work much easier!";
    private string q2_correctSN = "ඉතා හොඳයි! කප්පියක් (Pulley) යනු බර එසවීම පහසු කරන සරල යන්ත්‍රයකි. කඹය පහළට ඇදීම මගින් පෙට්ටිය ඉහළට එසවිය හැකිය.";
    
    private string q2_incorrectEN = "Almost! A rope helps you hold it, but it does not reduce the heavy weight. You still have to pull with all your strength. We need a simple machine!";
    private string q2_incorrectSN = "මෙය නිවැරදි නොවේ! කඹයකින් ඇදීමෙන් බර අඩුවන්නේ නැත. කාර්යය පහසු කිරීමට අපට සරල යන්ත්‍රයක් අවශ්‍ය වේ!";

    private Coroutine activeCoroutine;

    // --- Automatically plays the moment the panel turns on! ---
    void OnEnable()
    {
        PlayQuestionVoiceover();
    }

    void Start()
    {
        // Ensure feedback panel is hidden when the quiz first appears
        if(feedbackPanel != null) feedbackPanel.SetActive(false);
    }

    private void PlayQuestionVoiceover()
    {
        if (audioPlayer == null) return;

        string currentLang = PlayerPrefs.GetString("Language", "English");
        AudioClip clipToPlay = (currentLang == "Sinhala") ? questionAudioSN : questionAudioEN;

        if (clipToPlay != null)
        {
            audioPlayer.Stop();
            audioPlayer.clip = clipToPlay;
            audioPlayer.Play();
        }
    }

    public void OnCorrectAnswerClicked()
    {
        HapticFeedbackManager.TriggerSubtleHaptic(40L);
        // Check which question we are on, and send the correct text!
        if (thisQuestionType == Lesson2QuestionType.Question1_Ramp)
        {
            ShowFeedback(true, q1_correctEN, q1_correctSN, correctAudioEN, correctAudioSN);
        }
        else if (thisQuestionType == Lesson2QuestionType.Question2_Pulley)
        {
            ShowFeedback(true, q2_correctEN, q2_correctSN, correctAudioEN, correctAudioSN);
        }
    }

    public void OnIncorrectAnswerClicked()
    {
        HapticFeedbackManager.TriggerSubtleHaptic(40L);
        // Check which question we are on, and send the incorrect text!
        if (thisQuestionType == Lesson2QuestionType.Question1_Ramp)
        {
            ShowFeedback(false, q1_incorrectEN, q1_incorrectSN, incorrectAudioEN, incorrectAudioSN);
        }
        else if (thisQuestionType == Lesson2QuestionType.Question2_Pulley)
        {
            ShowFeedback(false, q2_incorrectEN, q2_incorrectSN, incorrectAudioEN, incorrectAudioSN);
        }
    }

    // --- MAIN FEEDBACK LOGIC ---
    private void ShowFeedback(bool isCorrect, string textEN, string textSN, AudioClip audioEN, AudioClip audioSN)
    {
        // If they click a button while audio is already playing, stop the old timer
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        string currentLang = PlayerPrefs.GetString("Language", "English");
        bool isEnglish = (currentLang != "Sinhala");

        // 1. Set the text dynamically
        if (isEnglish)
        {
            if (feedbackTextEN != null) 
            { 
                feedbackTextEN.text = textEN; 
                feedbackTextEN.gameObject.SetActive(true); 
            }
            if (feedbackTextSN != null) feedbackTextSN.gameObject.SetActive(false);
        }
        else
        {
            if (feedbackTextEN != null) feedbackTextEN.gameObject.SetActive(false);
            if (feedbackTextSN != null) 
            { 
                feedbackTextSN.text = textSN; 
                feedbackTextSN.gameObject.SetActive(true); 
            }
        }

        // 2. Play the corresponding audio
        AudioClip clipToPlay = isEnglish ? audioEN : audioSN;

        if (audioPlayer != null && clipToPlay != null)
        {
            audioPlayer.Stop();
            audioPlayer.clip = clipToPlay;
            audioPlayer.Play();
        }

        // 3. Show the feedback strip on screen
        if (feedbackPanel != null) feedbackPanel.SetActive(true);

        // 4. Start the automatic wait timer
        if (isCorrect)
        {
            activeCoroutine = StartCoroutine(HandleCorrectAnswerSequence(clipToPlay));
        }
        else
        {
            activeCoroutine = StartCoroutine(HandleIncorrectAnswerSequence(clipToPlay));
        }
    }

    private IEnumerator HandleCorrectAnswerSequence(AudioClip playedClip)
    {
        // Wait exactly the length of the voiceover + a half-second buffer
        float waitTime = playedClip != null ? playedClip.length + 0.5f : 3.0f;
        yield return new WaitForSeconds(waitTime);

        // Hide the feedback panel
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        
        // --- UPDATED LOGIC: Transition to Next Quiz OR to AR Pulley Scene ---
        if (nextQuizPanel != null) 
        {
            nextQuizPanel.SetActive(true);
        }
        else 
        {
            // If there is no next quiz panel, it means the quiz is over!
            
            // 1. Turn OFF the old Push/Pull models!
            if (oldARSceneContainer != null) 
            {
                oldARSceneContainer.SetActive(false);
            }

            // 2. Turn ON the new Pulley models!
            if (pulleyARSceneContainer != null) 
            {
                pulleyARSceneContainer.SetActive(true);
            }
        }

        // 3. TURN OFF CURRENT PANEL LAST (Fixes the Coroutine Bug)
        if (currentQuizPanel != null) currentQuizPanel.SetActive(false);
    }

    private IEnumerator HandleIncorrectAnswerSequence(AudioClip playedClip)
    {
        // Wait exactly the length of the voiceover
        float waitTime = playedClip != null ? playedClip.length + 0.5f : 3.0f;
        yield return new WaitForSeconds(waitTime);

        // Just hide the feedback panel so the student can try clicking the correct button
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }
}