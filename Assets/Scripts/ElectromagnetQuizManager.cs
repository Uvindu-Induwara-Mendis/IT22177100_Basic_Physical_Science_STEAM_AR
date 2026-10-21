using UnityEngine;
using TMPro;
using System.Collections;

// =========================================================================================
// LESSON 2 (ELECTROMAGNETS): 2D BLUEPRINT ASSEMBLY QUIZ
// =========================================================================================
// Context: Intermediate 2D quiz between Part 1 and Part 2. The student helps character Dasun
// assemble an electromagnet by dragging 3 essential components onto a blueprint diagram:
// - Iron Nail (magnetic core)
// - Insulated Copper Wire (conductive coil)
// - Dry Cell Battery (current source)
// Rejects distractor/trick items (permanent magnet, yarn string, lightbulb) with educational voiceovers.
// Upon connecting all 3 correct parts, unlocks the Next button to enter the final AR testing phase.
public class ElectromagnetQuizManager : MonoBehaviour
{
    // --- Scene UI & AR Containers (Single-Scene Architecture) ---
    [Header("Scene Containers")]
    public GameObject currentQuizContainer;  // The 2D blueprint quiz canvas
    public GameObject finalARContainer;      // Part 2 AR object container (electromagnet + draggable test items)
    public GameObject firstARContainer;      // Part 1 AR object container
    public GameObject part1UIContainer; 

    public ElectromagnetARManager arManager; // Part 2 AR manager launched after this quiz

    // --- UI Dialogue Connections ---
    [Header("UI Connections")]
    public GameObject feedbackPanel;         // Subtitle dialogue box for this quiz
    public GameObject part1FeedbackPanel;    // Previous panel forced off to prevent UI overlap
    public TextMeshProUGUI subtitleText;

    // --- Audio Sources & Voiceovers ---
    [Header("Audio Sources")]
    public AudioSource voiceAudioSource;
    public AudioSource sfxAudioSource;

    [Header("Voiceovers")]
    public AudioClip introVoice;             // "Let's help Dasun build an electromagnet..."
    public AudioClip successVoice;           // "Excellent work! Now let's test our electromagnet..."
    
    // Distractor / trick item feedback
    public AudioClip trickMagnetVoice;       // Rejects permanent magnet
    public AudioClip trickStringVoice;       // Rejects yarn/string
    public AudioClip trickBulbVoice;         // Rejects lightbulb
    
    // Correct part placement feedback
    [Header("Correct Part Voiceovers")]
    public AudioClip correctNailVoice;       // Explains iron nail core
    public AudioClip correctWireVoice;       // Explains conductive copper wire
    public AudioClip correctBatteryVoice;    // Explains battery electrical current

    [Header("Sound Effects")]
    public AudioClip correctSnapSFX;
    public AudioClip errorBoopSFX;

    // --- Assembly State ---
    [Header("Game State")]
    public int partsConnected = 0;           // Increments to 3 as correct parts are placed
    public GameObject nextButton;            // Revealed when all 3 parts are connected
    
    private Coroutine hideTextCoroutine;

    void Start()
    {
        if (nextButton != null) nextButton.SetActive(false);
        if (feedbackPanel != null) feedbackPanel.SetActive(false); 
        
        // Hide leftover Part 1 panels
        if (part1FeedbackPanel != null) part1FeedbackPanel.SetActive(false);
        if (firstARContainer != null) firstARContainer.SetActive(false); 
        if (part1UIContainer != null) part1UIContainer.SetActive(false); 

        // Ensure pinch-to-scale is attached to Part 2 AR models
        if (finalARContainer != null)
        {
            if (finalARContainer.GetComponent<ARPinchToScale>() == null)
            {
                ARPinchToScale scaler = finalARContainer.AddComponent<ARPinchToScale>();
                scaler.objectToScale = finalARContainer.transform;
            }
        }
        
        PlayVoice(introVoice, "Let's help Dasun build an electromagnet! Drag the correct core, wire, and power source to the blueprint.");
    }

    // --- Called by ElectromagnetDragItem when a Correct Part is Placed ---
    public void OnCorrectPartPlaced(string itemType)
    {
        HapticFeedbackManager.TriggerSubtleHaptic(40L);
        partsConnected++;
        if (sfxAudioSource != null && correctSnapSFX != null) sfxAudioSource.PlayOneShot(correctSnapSFX);

        float voiceLength = 3.0f; 

        if (itemType == "Nail")
        {
            PlayVoice(correctNailVoice, "Perfect! An iron nail makes the perfect magnetic core.");
            if (correctNailVoice != null) voiceLength = correctNailVoice.length;
        }
        else if (itemType == "Wire")
        {
            PlayVoice(correctWireVoice, "Great job! Copper wire is highly conductive and will carry the electricity perfectly.");
            if (correctWireVoice != null) voiceLength = correctWireVoice.length;
        }
        else if (itemType == "Battery")
        {
            PlayVoice(correctBatteryVoice, "Spot on! The battery will provide the electrical current we need to create the magnetic field.");
            if (correctBatteryVoice != null) voiceLength = correctBatteryVoice.length;
        }

        // When all 3 parts (Nail, Wire, Battery) are locked in, finish quiz
        if (partsConnected >= 3)
        {
            StartCoroutine(QuizCompleteSequence(voiceLength));
        }
    }

    // --- Called by ElectromagnetDragItem when a Trick / Distractor Part is Dropped ---
    public void OnTrickPartUsed(string trickType)
    {
        HapticFeedbackManager.TriggerSubtleHaptic(40L);
        if (sfxAudioSource != null && errorBoopSFX != null) sfxAudioSource.PlayOneShot(errorBoopSFX);

        if (trickType == "PermanentMagnet")
            PlayVoice(trickMagnetVoice, "Oops! That's a permanent magnet. We need a temporary metal core.");
        else if (trickType == "String")
            PlayVoice(trickStringVoice, "Oops! Yarn can't carry electricity. We need a conductive wire!");
        else if (trickType == "Lightbulb")
            PlayVoice(trickBulbVoice, "Oops! A bulb lights up, but it doesn't create a magnetic field!");
    }

    // Displays subtitle and plays voiceover clip
    private void PlayVoice(AudioClip clip, string subtitle)
    {
        if (feedbackPanel != null) feedbackPanel.SetActive(true);
        if (subtitleText != null) subtitleText.text = subtitle;
        
        float audioLength = 3.0f; 

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

    // Plays congratulatory audio once blueprint is complete, then unlocks next button
    private IEnumerator QuizCompleteSequence(float explanationTime)
    {
        yield return new WaitForSeconds(explanationTime + 0.2f);
        
        PlayVoice(successVoice, "Excellent work, scientist! You've put all the pieces together. Now it's time to test our new electromagnet in action!");
        
        yield return new WaitForSeconds(successVoice != null ? successVoice.length : 4.0f);
        if (nextButton != null) nextButton.SetActive(true);
    }

    // --- Button Tap: Advances to Part 2 AR Testing Phase ---
    public void GoToFinalARPart()
    {
        if (currentQuizContainer != null) currentQuizContainer.SetActive(false);
        if (firstARContainer != null) firstARContainer.SetActive(false); 
        if (part1UIContainer != null) part1UIContainer.SetActive(false); 
        
        if (finalARContainer != null) finalARContainer.SetActive(true);
        if (arManager != null) arManager.StartFinalARPhase();
    }
}