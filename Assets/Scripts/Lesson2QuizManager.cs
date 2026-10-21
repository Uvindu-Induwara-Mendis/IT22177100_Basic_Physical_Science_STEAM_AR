using System.Collections;
using UnityEngine;
using TMPro;

// =========================================================================================
// SIMPLE MACHINES / FORCES: 2D MULTIPLE-CHOICE QUIZ (Ramp & Pulley)
// =========================================================================================
// Context: Assesses understanding of simple machines:
// - Question 1 (Ramp): Explains that a gentler inclined plane (100cm) requires less pushing force than a steep ramp (150cm).
// - Question 2 (Pulley): Explains that a pulley lets you pull downward to lift a crate upward, making heavy work easier.
// When Question 2 is answered correctly, hides the old Push/Pull AR scene and activates the Pulley AR scene.
public class Lesson2QuizManager : MonoBehaviour
{
    // Dropdown to configure whether this script instance manages Question 1 or Question 2
    public enum Lesson2QuestionType 
    { 
        Question1_Ramp, 
        Question2_Pulley 
    }
    
    [Header("Select Which Question This Panel Is:")]
    public Lesson2QuestionType thisQuestionType;

    // --- Quiz Panels & Flow ---
    [Header("Quiz Panels")]
    public GameObject currentQuizPanel; 
    public GameObject nextQuizPanel;    

    // --- Scene Transition Containers ---
    [Header("End of Quiz Transition")]
    [Tooltip("Push/Pull AR scene container hidden when quiz completes")]
    public GameObject oldARSceneContainer;

    [Tooltip("Pulley AR scene container activated when quiz completes")]
    public GameObject pulleyARSceneContainer;

    // --- Feedback UI Elements ---
    [Header("Feedback UI Elements")]
    public GameObject feedbackPanel;
    public TextMeshProUGUI feedbackTextEN;
    public TextMeshProUGUI feedbackTextSN;

    // --- Audio Settings & Clips ---
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

    // --- Question 1 (Ramp) Explanatory Dialogue ---
    private string q1_correctEN = "Correct! A 100cm ramp is less steep. When the slope is lower, it takes less pushing force to move the crate up!";
    private string q1_correctSN = "ඉතා හොඳයි! තලයේ ආනත බව (steepness) අඩු වන විට යමක් ඉහළට තල්ලු කිරීම පහසු වේ.";
    
    private string q1_incorrectEN = "Not quite! A 150cm ramp is much steeper. Steeper slopes mean you have to use a lot more pushing force on the crate. Try the lower one!";
    private string q1_incorrectSN = "නැවත උත්සාහ කරන්න! 150cm යනු වැඩි ආනතියකි. ආනතිය වැඩි වන විට තල්ලු කිරීමට වැඩි බලයක් යෙදිය යුතුයි.";

    // --- Question 2 (Pulley) Explanatory Dialogue ---
    private string q2_correctEN = "Excellent! A pulley is a simple machine. It allows you to pull down to lift the crate up, making the heavy work much easier!";
    private string q2_correctSN = "ඉතා හොඳයි! කප්පියක් (Pulley) යනු බර එසවීම පහසු කරන සරල යන්ත්‍රයකි. කඹය පහළට ඇදීම මගින් පෙට්ටිය ඉහළට එසවිය හැකිය.";
    
    private string q2_incorrectEN = "Almost! A rope helps you hold it, but it does not reduce the heavy weight. You still have to pull with all your strength. We need a simple machine!";
    private string q2_incorrectSN = "මෙය නිවැරදි නොවේ! කඹයකින් ඇදීමෙන් බර අඩුවන්නේ නැත. කාර්යය පහසු කිරීමට අපට සරල යන්ත්‍රයක් අවශ්‍ය වේ!";

    private Coroutine activeCoroutine;

    // Plays the question audio as soon as this panel becomes active
    void OnEnable()
    {
        PlayQuestionVoiceover();
    }

    void Start()
    {
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }

    // Plays question narration in user's selected language
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

    // --- Button Tap: Correct Answer ---
    public void OnCorrectAnswerClicked()
    {
        HapticFeedbackManager.TriggerSubtleHaptic(40L);
        if (thisQuestionType == Lesson2QuestionType.Question1_Ramp)
        {
            ShowFeedback(true, q1_correctEN, q1_correctSN, correctAudioEN, correctAudioSN);
        }
        else if (thisQuestionType == Lesson2QuestionType.Question2_Pulley)
        {
            ShowFeedback(true, q2_correctEN, q2_correctSN, correctAudioEN, correctAudioSN);
        }
    }

    // --- Button Tap: Incorrect Answer ---
    public void OnIncorrectAnswerClicked()
    {
        HapticFeedbackManager.TriggerSubtleHaptic(40L);
        if (thisQuestionType == Lesson2QuestionType.Question1_Ramp)
        {
            ShowFeedback(false, q1_incorrectEN, q1_incorrectSN, incorrectAudioEN, incorrectAudioSN);
        }
        else if (thisQuestionType == Lesson2QuestionType.Question2_Pulley)
        {
            ShowFeedback(false, q2_incorrectEN, q2_incorrectSN, incorrectAudioEN, incorrectAudioSN);
        }
    }

    // --- Displays Localized Feedback Strip & Starts Transition Timer ---
    private void ShowFeedback(bool isCorrect, string textEN, string textSN, AudioClip audioEN, AudioClip audioSN)
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        string currentLang = PlayerPrefs.GetString("Language", "English");
        bool isEnglish = (currentLang != "Sinhala");

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

        AudioClip clipToPlay = isEnglish ? audioEN : audioSN;
        if (audioPlayer != null && clipToPlay != null)
        {
            audioPlayer.Stop();
            audioPlayer.clip = clipToPlay;
            audioPlayer.Play();
        }

        if (feedbackPanel != null) feedbackPanel.SetActive(true);

        if (isCorrect)
            activeCoroutine = StartCoroutine(HandleCorrectAnswerSequence(clipToPlay));
        else
            activeCoroutine = StartCoroutine(HandleIncorrectAnswerSequence(clipToPlay));
    }

    // Waits for feedback audio, then opens Question 2 or activates Pulley AR scene
    private IEnumerator HandleCorrectAnswerSequence(AudioClip playedClip)
    {
        float waitTime = playedClip != null ? playedClip.length + 0.5f : 3.0f;
        yield return new WaitForSeconds(waitTime);

        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        
        if (nextQuizPanel != null) 
        {
            // Advance to Question 2
            nextQuizPanel.SetActive(true);
        }
        else 
        {
            // Quiz complete: Switch from Push/Pull AR to Pulley AR
            if (oldARSceneContainer != null) 
                oldARSceneContainer.SetActive(false);

            if (pulleyARSceneContainer != null) 
                pulleyARSceneContainer.SetActive(true);
        }

        if (currentQuizPanel != null) currentQuizPanel.SetActive(false);
    }

    // Waits for error audio, then hides feedback strip so student can retry
    private IEnumerator HandleIncorrectAnswerSequence(AudioClip playedClip)
    {
        float waitTime = playedClip != null ? playedClip.length + 0.5f : 3.0f;
        yield return new WaitForSeconds(waitTime);

        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }
}