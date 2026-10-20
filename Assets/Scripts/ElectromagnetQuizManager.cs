using UnityEngine;
using TMPro;
using System.Collections;

public class ElectromagnetQuizManager : MonoBehaviour
{
    [Header("Scene Containers (The Single-Scene Flow!)")]
    public GameObject currentQuizContainer; 
    public GameObject finalARContainer;     
    public GameObject firstARContainer; 
    public GameObject part1UIContainer; 

    public ElectromagnetARManager arManager; 

    [Header("UI Connections")]
    public GameObject feedbackPanel; 
    // NEW: We will force the old panel off so it doesn't get stuck!
    public GameObject part1FeedbackPanel; 
    public TextMeshProUGUI subtitleText;

    [Header("Audio Sources")]
    public AudioSource voiceAudioSource;
    public AudioSource sfxAudioSource;

    [Header("Voiceovers (Dasun)")]
    public AudioClip introVoice;
    public AudioClip successVoice;
    public AudioClip trickMagnetVoice; 
    public AudioClip trickStringVoice; 
    public AudioClip trickBulbVoice;
    
    [Header("Correct Part Voiceovers")]
    public AudioClip correctNailVoice;
    public AudioClip correctWireVoice;
    public AudioClip correctBatteryVoice;

    [Header("Sound Effects")]
    public AudioClip correctSnapSFX;
    public AudioClip errorBoopSFX;

    [Header("Game State")]
    public int partsConnected = 0;
    public GameObject nextButton; 
    
    private Coroutine hideTextCoroutine;

    void Start()
    {
        if (nextButton != null) nextButton.SetActive(false);
        if (feedbackPanel != null) feedbackPanel.SetActive(false); 
        
        // FIX: Force the old Part 1 text off the screen instantly!
        if (part1FeedbackPanel != null) part1FeedbackPanel.SetActive(false);
        
        if (firstARContainer != null) 
        {
            firstARContainer.SetActive(false); 
        }

        if (finalARContainer != null)
        {
            // Add pinch to scale for Part 2 AR models
            if (finalARContainer.GetComponent<ARPinchToScale>() == null)
            {
                ARPinchToScale scaler = finalARContainer.AddComponent<ARPinchToScale>();
                scaler.objectToScale = finalARContainer.transform;
            }
        }

        if (part1UIContainer != null) part1UIContainer.SetActive(false); 
        
        PlayVoice(introVoice, "Let's help Dasun build an electromagnet! Drag the correct core, wire, and power source to the blueprint.");
    }

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

        if (partsConnected >= 3)
        {
            StartCoroutine(QuizCompleteSequence(voiceLength));
        }
    }

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

    private IEnumerator QuizCompleteSequence(float explanationTime)
    {
        yield return new WaitForSeconds(explanationTime + 0.2f);
        
        PlayVoice(successVoice, "Excellent work, scientist! You've put all the pieces together. Now it's time to test our new electromagnet in action!");
        
        yield return new WaitForSeconds(successVoice != null ? successVoice.length : 4.0f);
        if (nextButton != null) nextButton.SetActive(true);
    }

    public void GoToFinalARPart()
    {
        if (currentQuizContainer != null) currentQuizContainer.SetActive(false);
        if (firstARContainer != null) firstARContainer.SetActive(false); 
        if (part1UIContainer != null) part1UIContainer.SetActive(false); 
        
        if (finalARContainer != null) finalARContainer.SetActive(true);
        
        if (arManager != null) arManager.StartFinalARPhase();
    }
}