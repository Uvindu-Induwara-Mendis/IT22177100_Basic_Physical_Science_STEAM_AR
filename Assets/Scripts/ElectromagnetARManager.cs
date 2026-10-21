using UnityEngine;
using TMPro;
using System.Collections;

// =========================================================================================
// LESSON 2 (ELECTROMAGNETS): PART 2 - AR MARKER TRACKING & ELECTROMAGNET TESTING PHASE
// =========================================================================================
// Context: Follows the Lesson 2 2D Quiz. Prompts the student to aim their camera at the storybook
// marker to inspect the 3D electromagnet. Students drag everyday objects (paperclip, eraser, key, etc.)
// to the energized iron nail to see magnetic attraction vs non-magnetic materials in action.
public class ElectromagnetARManager : MonoBehaviour
{
    // --- UI Dialogue Connections ---
    [Header("UI Connections")]
    public GameObject feedbackPanel;         // Subtitle & audio instruction banner
    public TextMeshProUGUI subtitleText;

    // --- Audio Voiceovers ---
    [Header("Audio Sources")]
    public AudioSource voiceAudioSource;

    [Header("Voiceovers")]
    public AudioClip mainIntroVoice;         // Prompts user to drag household items to nail
    public AudioClip scanMarkerVoice;        // Prompts user to aim camera back at storybook marker
    public AudioClip finalCompletionVoice;   // Congratulates user upon finishing testing

    // --- Navigation & Scene Containers ---
    public GameObject nextButton;
    public GameObject nextButtonContainer;
    public GameObject storyRevealPanel;      // Storybook narrative panel shown at end of lesson
    public GameObject retryButton;           // Allows resetting items during Part 2 testing
    public GameObject finalARContainer;
    public GameObject part2UIContainer;

    private Coroutine hideTextCoroutine;
    private bool hasPlayedIntro = false;
    
    // AR tracking states (observed globally)
    [HideInInspector] public bool isTracking = false; 
    [HideInInspector] public bool isPart2Active = false;

    // --- Starts Part 2 AR Testing Phase ---
    // Invoked by ElectromagnetQuizManager when 2D quiz is completed
    public void StartFinalARPhase()
    {
        isPart2Active = true;
        if (retryButton != null) retryButton.SetActive(true);
        
        // If marker is already in view, play the drag prompt immediately
        if (isTracking)
        {
            PlayInstruction(mainIntroVoice, "Let's test our new electromagnet! Drag these everyday household items to the nail to find out which ones are magnetic.");
            hasPlayedIntro = true;
        }
        else
        {
            PlayInstruction(scanMarkerVoice, "Point your phone back at the storybook marker to test it out!");
        }
    }

    // --- AR Image Target Found Callback ---
    // Attached to Vuforia / AR Foundation DefaultObserverEventHandler
    public void OnMarkerFound()
    {
        isTracking = true;

        if (isPart2Active && !hasPlayedIntro)
        {
            PlayInstruction(mainIntroVoice, "Let's test our new electromagnet! Drag these everyday household items to the nail to find out which ones are magnetic.");
            hasPlayedIntro = true;
        }
    }

    // --- AR Image Target Lost Callback ---
    public void OnMarkerLost()
    {
        isTracking = false;

        if (isPart2Active)
        {
            PlayInstruction(scanMarkerVoice, "Point your phone back at the storybook marker to keep testing!");
        }
    }

    // --- Completion Flow Triggered from ElectromagnetTestManager ---
    // Runs on this persistent GameObject so tracking loss doesn't cancel coroutines
    public void FinishLessonFromTestManager(float explanationTime)
    {
        StartCoroutine(FinishLessonSequence(explanationTime));
    }

    private IEnumerator FinishLessonSequence(float explanationTime)
    {
        yield return new WaitForSeconds(explanationTime + 0.2f);
        
        // Silence any ongoing item feedback from the test manager
        ElectromagnetTestManager testManager = FindObjectOfType<ElectromagnetTestManager>();
        if (testManager != null) testManager.StopFeedbackPanelCoroutine();

        PlayInstruction(finalCompletionVoice, "Amazing job, scientist! You've successfully built and tested an electromagnet. Lesson complete!");

        float finalWait = finalCompletionVoice != null ? finalCompletionVoice.length : 4.5f;
        yield return new WaitForSeconds(finalWait + 0.5f);

        if (hideTextCoroutine != null)
        {
            StopCoroutine(hideTextCoroutine);
            hideTextCoroutine = null;
        }

        isPart2Active = false;
        if (retryButton != null) retryButton.SetActive(false);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);

        // Transition directly to the storybook conclusion
        OpenStoryReveal();
    }

    // --- Opens Story Reveal Panel ---
    public void OpenStoryReveal()
    {
        if (storyRevealPanel != null) storyRevealPanel.SetActive(true);
        if (finalARContainer != null) finalARContainer.SetActive(false);
        if (part2UIContainer != null) part2UIContainer.SetActive(false);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }

    // Stops current narration before playing specific item feedback
    public void StopCurrentInstruction()
    {
        if (voiceAudioSource != null && voiceAudioSource.isPlaying)
            voiceAudioSource.Stop();

        if (hideTextCoroutine != null)
        {
            StopCoroutine(hideTextCoroutine);
            hideTextCoroutine = null;
        }

        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }

    // Plays instruction audio and displays matching subtitle banner
    private void PlayInstruction(AudioClip clip, string subtitle)
    {
        if (feedbackPanel != null) feedbackPanel.SetActive(true);
        if (subtitleText != null) subtitleText.text = subtitle;
        
        float audioLength = 4.0f; 

        if (voiceAudioSource != null && clip != null)
        {
            voiceAudioSource.Stop();
            voiceAudioSource.clip = clip;
            voiceAudioSource.Play();
            audioLength = clip.length; 
        }

        if (hideTextCoroutine != null) StopCoroutine(hideTextCoroutine);
        hideTextCoroutine = StartCoroutine(HidePanelAfterTime(audioLength + 0.5f)); 
    }

    private IEnumerator HidePanelAfterTime(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }
}