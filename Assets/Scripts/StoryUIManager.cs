using UnityEngine;
using System.Collections;
using TMPro;

public class StoryUIManager : MonoBehaviour
{
    [Header("Universal Audio & Text")]
    public AudioSource audioSource;
    public TextMeshProUGUI subtitleText_EN; 
    public TextMeshProUGUI subtitleText_SN; 
    public float typingSpeed = 0.05f;

    private Coroutine typingCoroutine;

    // NOW ACCEPTS BOTH AUDIO CLIPS
    public void PlayVoiceover(AudioClip clip_EN, AudioClip clip_SN, string sentence_EN, string sentence_SN)
    {
        if (audioSource != null)
        {
            // Check what language is currently selected
            string currentLang = PlayerPrefs.GetString("Language", "English");
            
            // Pick the correct clip based on language
            AudioClip clipToPlay = null;
            if (currentLang == "Sinhala" && clip_SN != null)
            {
                clipToPlay = clip_SN;
            }
            else if (clip_EN != null)
            {
                clipToPlay = clip_EN; // Default to English
            }

            // Play it if we have one
            if (clipToPlay != null)
            {
                audioSource.clip = clipToPlay;
                audioSource.Play();
            }
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        
        typingCoroutine = StartCoroutine(TypeSentence(sentence_EN, sentence_SN));
    }

    private IEnumerator TypeSentence(string sentence_EN, string sentence_SN)
    {
        if (subtitleText_EN != null) subtitleText_EN.text = ""; 
        if (subtitleText_SN != null) subtitleText_SN.text = ""; 

        int maxLength = Mathf.Max(sentence_EN.Length, sentence_SN.Length);

        for (int i = 0; i < maxLength; i++)
        {
            if (subtitleText_EN != null && i < sentence_EN.Length) 
            {
                subtitleText_EN.text += sentence_EN[i];
            }
            
            if (subtitleText_SN != null && i < sentence_SN.Length) 
            {
                subtitleText_SN.text += sentence_SN[i];
            }

            yield return new WaitForSeconds(typingSpeed); 
        }
    }

    public void ClearText()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (subtitleText_EN != null) subtitleText_EN.text = "";
        if (subtitleText_SN != null) subtitleText_SN.text = "";
    }
}