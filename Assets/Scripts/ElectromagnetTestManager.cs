using UnityEngine;
using TMPro;
using System.Collections;

// =========================================================================================
// LESSON 2 (ELECTROMAGNETS): PART 2 - 3D AR ITEM TESTING INTERACTION
// =========================================================================================
// Context: Operates in the Lesson 2 AR scene once the electromagnet is active.
// Students test various 3D everyday items by dragging them toward the iron nail tip:
// - Magnetic items (Paperclip, Safety Pin): snap to the nail with magnetic pull sound and explanation.
// - Non-magnetic items (Plastic, Wood, Copper): bounce back to original position with rejection sound.
// Once both magnetic items are stuck to the nail, completes the lesson via ElectromagnetARManager.
public class ElectromagnetTestManager : MonoBehaviour
{
    // --- Managers & UI Dialogues ---
    [Header("AR Manager Reference")]
    public ElectromagnetARManager arManager;  // Coordinates marker tracking & completion flow

    [Header("UI Connections")]
    public GameObject feedbackPanel;          // Displays feedback subtitles
    public TextMeshProUGUI subtitleText;

    // --- 3D Snap Anchors on the Iron Nail ---
    [Header("Game Objects")]
    public Transform[] snapTargets;           // Target positions on nail tip where items stick
    public float snapDistance = 0.15f;        // Proximity threshold required to trigger snap

    // --- Audio Sources & SFX ---
    [Header("Audio Sources")]
    public AudioSource voiceAudioSource;
    public AudioSource sfxAudioSource;

    [Header("Sound Effects")]
    public AudioClip snapSFX;                 // Snap sound when magnetic item attracts to nail
    public AudioClip rejectBoopSFX;           // Error boop when non-magnetic item is released

    // --- Explanatory Voiceovers ---
    [Header("Voiceovers")]
    public AudioClip successPaperclipVoice;   // Explains iron in paperclip
    public AudioClip successSafetyPinVoice;   // Explains iron in safety pin
    public AudioClip rejectPlasticVoice;      // Explains plastic is non-magnetic
    public AudioClip rejectWoodVoice;         // Explains wood is non-magnetic
    public AudioClip rejectCopperVoice;       // Explains copper is a non-magnetic metal

    // --- Runtime Touch Dragging State ---
    private MagneticTestItem draggedItem;
    private float zDistance;
    private Vector3 dragOffset;
    private int correctlySnappedItems = 0;
    private Coroutine hideTextCoroutine;

    void Update()
    {
        HandleTouchDrag();
    }

    // --- Touch Dragging 3D Items in AR ---
    // Raycasts from screen touch to pick up a MagneticTestItem, maintains depth, and follows finger
    private void HandleTouchDrag()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Phase 1: Tap down - pick up item if not already locked to nail
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
            // Phase 2: Dragging item across AR screen space
            else if (touch.phase == TouchPhase.Moved && draggedItem != null)
            {
                Vector3 screenPos = new Vector3(touch.position.x, touch.position.y, zDistance);
                draggedItem.transform.position = Camera.main.ScreenToWorldPoint(screenPos) + dragOffset;
            }
            // Phase 3: Released item - test if close to electromagnet nail
            else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && draggedItem != null)
            {
                draggedItem.isBeingDragged = false;
                CheckDropZone();
                draggedItem = null;
            }
        }
    }

    // --- Evaluates Item Release Near Electromagnet ---
    private void CheckDropZone()
    {
        // Check proximity to any of the nail snap targets
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
            // CASE A: Magnetic Item (Attracted to nail)
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
            // CASE B: Non-Magnetic Item (Rejected, returns to desk)
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
            // Released away from nail: return to starting desk position
            draggedItem.transform.localPosition = draggedItem.startingLocalPosition;
        }
    }

    // Plays educational voiceover and displays subtitle
    private void PlayVoice(AudioClip clip, string subtitle)
    {
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

    // When 2 magnetic items (paperclip & safety pin) are stuck to the nail, trigger completion
    private void CheckForCompletion(float explanationTime)
    {
        if (correctlySnappedItems >= 2)
        {
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

    // --- Retry / Reset AR Testing ---
    // Resets snapped item count, restores all test items to initial desk positions
    public void ResetExperiment()
    {
        if (CircuitAnalyticsManager.Instance != null)
        {
            CircuitAnalyticsManager.Instance.RecordRetry();
        }

        correctlySnappedItems = 0;
        draggedItem = null;

        StopFeedbackPanelCoroutine();
        if (voiceAudioSource != null && voiceAudioSource.isPlaying)
        {
            voiceAudioSource.Stop();
        }
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        if (arManager != null) arManager.StopCurrentInstruction();

        MagneticTestItem[] allItems = FindObjectsOfType<MagneticTestItem>();
        foreach (MagneticTestItem item in allItems)
        {
            item.isLocked = false;
            item.isBeingDragged = false;
            item.transform.localPosition = item.startingLocalPosition;
        }
    }
}