using UnityEngine;
using System.Collections;
using TMPro;

// =========================================================================================
// SHARED UI: STORYBOOK NARRATIVE & TYPEWRITER SUBTITLE MANAGER
// =========================================================================================
// Context: Used across all AR lessons (Electric Circuits, Simple Machines, Electromagnets).
// Displays bilingual subtitles (English / Sinhala) using an animated typewriter effect,
// and plays matching narrative voiceovers based on user's language setting.
public class StoryUIManager : MonoBehaviour
{
    // --- Audio & Subtitle Components ---
    [Header("Universal Audio & Text")]
    public AudioSource audioSource;
    public TextMeshProUGUI subtitleText_EN; 
    public TextMeshProUGUI subtitleText_SN; 
    public float typingSpeed = 0.05f;        // Seconds per character in typewriter animation

    private Coroutine typingCoroutine;

    // --- Plays Localized Voiceover & Animates Subtitles ---
    // Chooses English or Sinhala clip from PlayerPrefs, then types out sentence
    public void PlayVoiceover(AudioClip clip_EN, AudioClip clip_SN, string sentence_EN, string sentence_SN)
    {
        if (audioSource != null)
        {
            string currentLang = PlayerPrefs.GetString("Language", "English");
            
            AudioClip clipToPlay = null;
            if (currentLang == "Sinhala" && clip_SN != null)
                clipToPlay = clip_SN;
            else if (clip_EN != null)
                clipToPlay = clip_EN;

            if (clipToPlay != null)
            {
                audioSource.clip = clipToPlay;
                audioSource.Play();
            }
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(sentence_EN, sentence_SN));
    }

    // Coroutine typing out characters one by one
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

    // Clears subtitle text and stops ongoing typing animation
    public void ClearText()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (subtitleText_EN != null) subtitleText_EN.text = "";
        if (subtitleText_SN != null) subtitleText_SN.text = "";
    }
}