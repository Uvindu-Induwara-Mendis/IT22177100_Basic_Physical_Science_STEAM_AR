using UnityEngine;
using TMPro;
using System.Collections;

public class ElectromagnetARManager : MonoBehaviour
{
    [Header("UI Connections")]
    public GameObject feedbackPanel; 
    public TextMeshProUGUI subtitleText;

    [Header("Audio Sources")]
    public AudioSource voiceAudioSource;

    [Header("Voiceovers")]
    public AudioClip mainIntroVoice; 
    public AudioClip scanMarkerVoice; 

    public GameObject nextButton;
    public GameObject nextButtonContainer;
    public GameObject storyRevealPanel;
    public GameObject retryButton; // --- NEW: Toggle the retry button ---
    public GameObject finalARContainer;
    public GameObject part2UIContainer;
    public AudioClip finalCompletionVoice;

    private Coroutine hideTextCoroutine;
    private bool hasPlayedIntro = false;
    
    // Passively listens to the camera the entire game!
    [HideInInspector] public bool isTracking = false; 
    [HideInInspector] public bool isPart2Active = false;

    // NEW: Called manually by the Quiz Manager when the 2D quiz finishes
    public void StartFinalARPhase()
    {
        isPart2Active = true;
        if (retryButton != null) retryButton.SetActive(true);
        
        // Instantly check the current camera state without waiting!
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

    // AR EVENT: Hook this back up to your Image Target!
    public void OnMarkerFound()
    {
        isTracking = true;

        // Only play the voice if we are actually in Part 2!
        if (isPart2Active && !hasPlayedIntro)
        {
            PlayInstruction(mainIntroVoice, "Let's test our new electromagnet! Drag these everyday household items to the nail to find out which ones are magnetic.");
            hasPlayedIntro = true;
        }
    }

    // AR EVENT: Hook this back up to your Image Target!
    public void OnMarkerLost()
    {
        isTracking = false;

        // Only play the warning if we are actually in Part 2!
        if (isPart2Active)
        {
            PlayInstruction(scanMarkerVoice, "Point your phone back at the storybook marker to keep testing!");
        }
    }

    // ──────────────────────────────────────────────
    //  COMPLETION FLOW — runs on this persistent object
    //  so Vuforia marker-loss can never kill the coroutine
    // ──────────────────────────────────────────────
    public void FinishLessonFromTestManager(float explanationTime)
    {
        StartCoroutine(FinishLessonSequence(explanationTime));
    }

    private IEnumerator FinishLessonSequence(float explanationTime)
    {
        yield return new WaitForSeconds(explanationTime + 0.2f);
        
        // Stop the test manager from hiding the shared feedback panel 
        // since its HidePanelAfterTime coroutine might still be running!
        ElectromagnetTestManager testManager = FindObjectOfType<ElectromagnetTestManager>();
        if (testManager != null) testManager.StopFeedbackPanelCoroutine();

        PlayInstruction(finalCompletionVoice, "Amazing job, scientist! You've successfully built and tested an electromagnet. Lesson complete!");

        float finalWait = finalCompletionVoice != null ? finalCompletionVoice.length : 4.5f;
        yield return new WaitForSeconds(finalWait + 0.5f);

        // Stop any lingering hide-panel coroutine
        if (hideTextCoroutine != null)
        {
            StopCoroutine(hideTextCoroutine);
            hideTextCoroutine = null;
        }

        // Part 2 is done — stop reacting to marker events
        isPart2Active = false;
        if (retryButton != null) retryButton.SetActive(false);

        // Clean up the feedback UI
        if (feedbackPanel != null) feedbackPanel.SetActive(false);

        // Go straight to the story reveal — no intermediate button needed!
        OpenStoryReveal();
    }

    // Called by the next button to transition to the story panel
    public void OpenStoryReveal()
    {
        if (storyRevealPanel != null) storyRevealPanel.SetActive(true);
        if (finalARContainer != null) finalARContainer.SetActive(false);
        if (part2UIContainer != null) part2UIContainer.SetActive(false);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }

    // Public so the test manager can silence us before playing item feedback
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