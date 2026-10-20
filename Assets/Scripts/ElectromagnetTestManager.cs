using UnityEngine;
using TMPro;
using System.Collections;

public class ElectromagnetTestManager : MonoBehaviour
{
    [Header("AR Manager Reference")]
    public ElectromagnetARManager arManager;

    [Header("UI Connections")]
    public GameObject feedbackPanel; 
    public TextMeshProUGUI subtitleText;

    [Header("Game Objects")]
    public Transform[] snapTargets; 
    public float snapDistance = 0.15f; 

    [Header("Audio Sources")]
    public AudioSource voiceAudioSource;
    public AudioSource sfxAudioSource;

    [Header("Sound Effects")]
    public AudioClip snapSFX;
    public AudioClip rejectBoopSFX;

    [Header("Voiceovers")]
    public AudioClip successPaperclipVoice;
    public AudioClip successSafetyPinVoice;
    public AudioClip rejectPlasticVoice;
    public AudioClip rejectWoodVoice;
    public AudioClip rejectCopperVoice;

    private MagneticTestItem draggedItem;
    private float zDistance;
    private Vector3 dragOffset;
    private int correctlySnappedItems = 0;
    private Coroutine hideTextCoroutine;

    void Update()
    {
        HandleTouchDrag();
    }

    private void HandleTouchDrag()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    MagneticTestItem item = hit.collider.GetComponent<MagneticTestItem>();
                    if (item != null && !item.isLocked)
                    {
                        draggedItem = item;
                        draggedItem.isBeingDragged = true;
                        
                        zDistance = Camera.main.WorldToScreenPoint(draggedItem.transform.position).z;
                        Vector3 screenPos = new Vector3(touch.position.x, touch.position.y, zDistance);
                        dragOffset = draggedItem.transform.position - Camera.main.ScreenToWorldPoint(screenPos);
                        TriggerSubtleGrabHaptic();
                    }
                }
            }
            else if (touch.phase == TouchPhase.Moved && draggedItem != null)
            {
                Vector3 screenPos = new Vector3(touch.position.x, touch.position.y, zDistance);
                draggedItem.transform.position = Camera.main.ScreenToWorldPoint(screenPos) + dragOffset;
            }
            else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && draggedItem != null)
            {
                draggedItem.isBeingDragged = false;
                CheckDropZone();
                draggedItem = null;
            }
        }
    }

    private void CheckDropZone()
    {
        bool isCloseEnough = false;
        foreach (Transform target in snapTargets)
        {
            if (Vector3.Distance(draggedItem.transform.position, target.position) <= snapDistance)
            {
                isCloseEnough = true;
                break;
            }
        }

        if (isCloseEnough)
        {
            if (draggedItem.isMagnetic)
            {
                if (correctlySnappedItems < snapTargets.Length)
                {
                    draggedItem.transform.position = snapTargets[correctlySnappedItems].position;
                }
                
                draggedItem.isLocked = true;
                
                if (sfxAudioSource != null) sfxAudioSource.PlayOneShot(snapSFX);
                TriggerSubtleGrabHaptic(); 

                float voiceLength = 3.0f; 

                if (draggedItem.itemName == "Paperclip")
                {
                    PlayVoice(successPaperclipVoice, "Great! The paperclip has iron in it. Iron is magnetic, so the electromagnet pulls it right in!");
                    if (successPaperclipVoice != null) voiceLength = successPaperclipVoice.length;
                }
                else if (draggedItem.itemName == "Safety Pin")
                {
                    PlayVoice(successSafetyPinVoice, "Awesome! The safety pin contains iron, which is a magnetic metal, so our electromagnet attracts it!");
                    if (successSafetyPinVoice != null) voiceLength = successSafetyPinVoice.length;
                }
                
                correctlySnappedItems++;
                CheckForCompletion(voiceLength);
            }
            else
            {
                draggedItem.transform.localPosition = draggedItem.startingLocalPosition;
                
                if (sfxAudioSource != null) sfxAudioSource.PlayOneShot(rejectBoopSFX);
                Handheld.Vibrate(); 

                if (draggedItem.materialType == "Plastic")
                    PlayVoice(rejectPlasticVoice, "Oops! Plastic isn't magnetic. Try something else!");
                else if (draggedItem.materialType == "Wood")
                    PlayVoice(rejectWoodVoice, "Oops! Wood isn't magnetic. We need a specific type of metal.");
                else if (draggedItem.materialType == "Copper")
                    PlayVoice(rejectCopperVoice, "Tricky! Copper is a metal, but it doesn't stick to magnets. Try another metal!");
            }
        }
        else
        {
            draggedItem.transform.localPosition = draggedItem.startingLocalPosition;
        }
    }

    private void PlayVoice(AudioClip clip, string subtitle)
    {
        // Stop any instruction voice from the AR Manager so they don't overlap
        if (arManager != null) arManager.StopCurrentInstruction();

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

    public void StopFeedbackPanelCoroutine()
    {
        if (hideTextCoroutine != null)
        {
            StopCoroutine(hideTextCoroutine);
            hideTextCoroutine = null;
        }
    }

    private IEnumerator HidePanelAfterTime(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }

    private void CheckForCompletion(float explanationTime)
    {
        if (correctlySnappedItems >= 2)
        {
            // Delegate to the persistent AR Manager so the coroutine
            // survives even if Vuforia disables this AR content
            if (arManager != null)
            {
                arManager.FinishLessonFromTestManager(explanationTime);
            }
        }
    }

    private void TriggerSubtleGrabHaptic()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            vibrator.Call("vibrate", 40L); 
        }
        catch (System.Exception) {}
#endif
    }

    // --- NEW: Contextual Retry Method for the AR Part ---
    public void ResetExperiment()
    {
        // 1. Analytics
        if (CircuitAnalyticsManager.Instance != null)
        {
            CircuitAnalyticsManager.Instance.RecordRetry();
        }

        // 2. Reset internal state
        correctlySnappedItems = 0;
        draggedItem = null;

        // 3. Stop audio/coroutines
        StopFeedbackPanelCoroutine();
        if (voiceAudioSource != null && voiceAudioSource.isPlaying)
        {
            voiceAudioSource.Stop();
        }
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        if (arManager != null) arManager.StopCurrentInstruction(); // stops AR manager's voice if any

        // 4. Find all test items and reset them
        MagneticTestItem[] allItems = FindObjectsOfType<MagneticTestItem>();
        foreach (MagneticTestItem item in allItems)
        {
            item.isLocked = false;
            item.isBeingDragged = false;
            item.transform.localPosition = item.startingLocalPosition;
        }
    }
}