using UnityEngine;
using TMPro;
using System.Collections; 

// =========================================================================================
// LESSON: MAGNETISM - INTERACTIVE 3D BAR MAGNET TOUCH EXPERIMENT
// =========================================================================================
// Context: Screen-based interactive activity. Students drag two bar magnets toward each other,
// test opposite poles (attract and snap together) vs like poles (repel with jitter), and tap
// flip buttons to rotate poles. Once all combinations are tested, the Next button appears.
public class MagnetTouchManager : MonoBehaviour
{
    // --- UI Controls & Navigation ---
    [Header("UI Controls")]
    public GameObject mainUICanvas; 
    public GameObject magnetUIContainer;
    public GameObject flipLeftButton;       // Flips left magnet (North <-> South)
    public GameObject flipRightButton;      // Flips right magnet (North <-> South)
    public GameObject resetButton;          // Resets positions & poles to default
    public GameObject nextButton;           // Revealed after all pole combinations tested
    public GameObject feedbackPanel;        // Dialogue banner for feedback text

    // --- Cleanup: Panels to hide when this scene opens ---
    [Header("UI Cleanup")]
    public GameObject[] elementsToHide;

    // --- Localized Text & Dialogue ---
    [Header("Text UI")]
    public TextMeshProUGUI topTextEN;
    public TextMeshProUGUI topTextSN;
    public TextMeshProUGUI subtitleEN;
    public TextMeshProUGUI subtitleSN;

    [Header("Strings - English")]
    public string introTopEN = "Drag the magnets with your finger!";
    public string introSubEN = "Welcome back, scientist! Let's test these magnets. Use your finger to drag one magnet toward the other. You can also tap the flip buttons to change their poles. Test the different poles to see which ones pull together, and which ones push apart!";
    public string attractSuccessEN = "Snap! Great job! The North and South poles are opposites, so they attract and pull together.";
    public string repelNNSuccessEN = "Whoa, look at them push away! Two North poles are exactly the same, so they repel each other.";
    public string repelSSSuccessEN = "Did you feel that force? Two South poles are also the same, so they repel and push away too!";

    [Header("Strings - Sinhala")]
    public string introTopSN = "[FM Intro Top]";
    public string introSubSN = "[FM Intro Sub]";
    public string attractSuccessSN = "[FM Attract Success]";
    public string repelNNSuccessSN = "[FM NN Repel Success]";
    public string repelSSSuccessSN = "[FM SS Repel Success]";

    // --- Audio Sources & Voiceovers ---
    [Header("Audio Sources")]
    public AudioSource voiceAudioSource;
    public AudioSource sfxAudioSource;   

    [Header("Voiceovers (EN / SN)")]
    public AudioClip introVoiceEN; public AudioClip introVoiceSN;
    public AudioClip attractVoiceEN; public AudioClip attractVoiceSN;
    public AudioClip repelNNVoiceEN; public AudioClip repelNNVoiceSN;
    public AudioClip repelSSVoiceEN; public AudioClip repelSSVoiceSN;

    [Header("Sound Effects")]
    public AudioClip snapSFX;               // Magnet snapping sound on attraction
    public AudioClip magneticHumSFX;        // Magnetic resistance hum on repulsion
    public AudioClip uiClickSFX; 

    // --- 3D Magnet Models & Movement Limits ---
    [Header("3D Magnets")]
    public Transform magnetLeft;  
    public Transform magnetRight; 

    [Header("Distance Settings")]
    public float dragSensitivity = 0.0005f; 
    public float maxSwipeDistance = 0.06f;   // Maximum travel along X from start
    public float snapDistance = 0.08f;       // Distance between magnets when snapped
    public float magneticPullZone = 0.09f;   // Distance at which attraction pulls them together
    public float repulsionDistance = 0.085f; // Minimum distance maintained when repelling

    [Header("Struggle Polish Settings")]
    public float shakeSpeed = 50f;           // Speed of resistance jitter
    public float shakeIntensity = 0.002f;    // Intensity of resistance jitter

    // --- Internal State & Progress Tracking ---
    private Vector3 leftStartPos;
    private Vector3 rightStartPos;

    // Pole tracking: true = North facing inward, false = South facing inward
    private bool leftInnerIsNorth = false; 
    private bool rightInnerIsNorth = true; 

    private bool isSnapped = false;
    private bool isDraggingRight = false;
    private bool isDraggingLeft = false;
    private bool isRepelSuccessTriggered = false;

    private float repelVibrationTimer = 0f; 
    private bool isActivelyRepelling = false; 

    // Progress flags to ensure student tests all combinations
    private bool tested_NS = false;
    private bool tested_SN = false;
    private bool tested_NN = false;
    private bool tested_SS = false;
    private bool isQuizComplete = false; 

    void Start()
    {
        if (magnetLeft != null) leftStartPos = magnetLeft.localPosition;
        if (magnetRight != null) rightStartPos = magnetRight.localPosition;
        if (nextButton != null) nextButton.SetActive(false);
    }

    // --- Initializes the Magnet Activity ---
    // Activates the magnet canvas, sets up buttons, and plays the intro voiceover
    public void StartScene()
    {
        if (elementsToHide != null && elementsToHide.Length > 0)
        {
            foreach (GameObject element in elementsToHide)
            {
                if (element != null) element.SetActive(false);
            }
        }

        if (mainUICanvas != null) mainUICanvas.SetActive(true);
        if (magnetUIContainer != null) magnetUIContainer.SetActive(true);
        if (flipLeftButton != null) flipLeftButton.SetActive(true);
        if (flipRightButton != null) flipRightButton.SetActive(true);
        if (resetButton != null) resetButton.SetActive(true); 

        PlayAudioAndText(introVoiceEN, introVoiceSN, introTopEN, introTopSN, introSubEN, introSubSN);
    }

    // --- UI Button: Flip Left Magnet Pole ---
    // Inverts pole orientation, rotates 3D model 180°, and resets positions
    public void OnFlipLeftButtonClicked()
    {
        leftInnerIsNorth = !leftInnerIsNorth;
        magnetLeft.localRotation *= Quaternion.Euler(0, 180f, 0);
        ResetPositions();
        if (sfxAudioSource != null && uiClickSFX != null) sfxAudioSource.PlayOneShot(uiClickSFX);
    }

    // --- UI Button: Flip Right Magnet Pole ---
    // Inverts pole orientation, rotates 3D model 180°, and resets positions
    public void OnFlipRightButtonClicked()
    {
        rightInnerIsNorth = !rightInnerIsNorth;
        magnetRight.localRotation *= Quaternion.Euler(0, 180f, 0);
        ResetPositions();
        if (sfxAudioSource != null && uiClickSFX != null) sfxAudioSource.PlayOneShot(uiClickSFX);
    }

    // --- UI Button: Reset Experiment ---
    // Restores default poles (Left: South, Right: North) and initial rotations/positions
    public void OnResetButtonClicked()
    {
        leftInnerIsNorth = false; 
        rightInnerIsNorth = true; 

        if (magnetLeft != null) magnetLeft.localRotation = Quaternion.Euler(90f, 180f, 0f);
        if (magnetRight != null) magnetRight.localRotation = Quaternion.Euler(90f, 180f, 0f);

        ResetPositions();
        if (sfxAudioSource != null && uiClickSFX != null) sfxAudioSource.PlayOneShot(uiClickSFX); 
    }

    // Restores magnets to start positions and clears interaction states
    private void ResetPositions()
    {
        isSnapped = false;
        isRepelSuccessTriggered = false;
        isActivelyRepelling = false;
        repelVibrationTimer = 0f;
        magnetLeft.localPosition = leftStartPos;
        magnetRight.localPosition = rightStartPos;
    }

    void Update()
    {
        if (magnetLeft != null && !magnetLeft.gameObject.activeInHierarchy) return;

        isActivelyRepelling = false; 
        HandleTouchDrag();

        // Pulsing haptic vibration when like poles actively push against each other
        if (isActivelyRepelling)
        {
            repelVibrationTimer -= Time.deltaTime;
            if (repelVibrationTimer <= 0f)
            {
                Handheld.Vibrate();
                repelVibrationTimer = 0.2f; 
            }
        }
        else
        {
            repelVibrationTimer = 0f;
        }
    }

    // --- Touch Dragging & Raycast Selection ---
    // Detects finger touches, picks a magnet via raycast, and slides it along the X-axis
    private void HandleTouchDrag()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Phase 1: Tap down - raycast to select magnet
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.transform == magnetRight) 
                    {
                        isDraggingRight = true;
                        TriggerSubtleGrabHaptic(); 
                    }
                    else if (hit.transform == magnetLeft) 
                    {
                        isDraggingLeft = true;
                        TriggerSubtleGrabHaptic(); 
                    }
                }
            }
            // Phase 2: Dragging - update position along X within bounds
            else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && (isDraggingRight || isDraggingLeft))
            {
                bool isMovingTowards = false;

                if (touch.phase == TouchPhase.Moved)
                {
                    float moveX = touch.deltaPosition.x * dragSensitivity;

                    if (isDraggingRight)
                    {
                        Vector3 newPos = magnetRight.localPosition + new Vector3(moveX, 0, 0);
                        newPos.x = Mathf.Clamp(newPos.x, rightStartPos.x - maxSwipeDistance, rightStartPos.x + maxSwipeDistance);
                        magnetRight.localPosition = newPos;
                        if (moveX < 0) isMovingTowards = true; 
                    }
                    else if (isDraggingLeft)
                    {
                        Vector3 newPos = magnetLeft.localPosition + new Vector3(moveX, 0, 0);
                        newPos.x = Mathf.Clamp(newPos.x, leftStartPos.x - maxSwipeDistance, leftStartPos.x + maxSwipeDistance);
                        magnetLeft.localPosition = newPos;
                        if (moveX > 0) isMovingTowards = true; 
                    }
                }

                CalculateMagneticForces(isMovingTowards, isDraggingRight);
            }
            // Phase 3: Touch released
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                CalculateMagneticForces(false, isDraggingRight);
                isDraggingRight = false;
                isDraggingLeft = false;
            }
        }
    }

    // --- Magnetic Attraction & Repulsion Simulation ---
    // If opposite poles: magnets snap together when close enough (attract).
    // If like poles: opposite magnet is pushed away; shaking jitter applies at boundary (repel).
    private void CalculateMagneticForces(bool isMovingTowards, bool isRightActive)
    {
        bool isAttracting = (leftInnerIsNorth != rightInnerIsNorth);
        float distance = magnetRight.localPosition.x - magnetLeft.localPosition.x;

        // --- OPPOSITE POLES: ATTRACTION ---
        if (isAttracting)
        {
            if (!isMovingTowards && distance > snapDistance + 0.002f) 
            {
                isSnapped = false; 
            }
            else if (distance <= magneticPullZone) 
            {
                // Magnet entered pull zone: snap together
                if (!isSnapped)
                {
                    isSnapped = true;
                    Handheld.Vibrate(); 

                    if (sfxAudioSource != null && snapSFX != null) sfxAudioSource.PlayOneShot(snapSFX);

                    // Track test combination (N-S or S-N) and explain attraction
                    if (leftInnerIsNorth) 
                    {
                        if (!tested_NS) { tested_NS = true; PlayAudioAndText(attractVoiceEN, attractVoiceSN, introTopEN, introTopSN, attractSuccessEN, attractSuccessSN); }
                    } 
                    else 
                    {
                        if (!tested_SN) { tested_SN = true; PlayAudioAndText(attractVoiceEN, attractVoiceSN, introTopEN, introTopSN, attractSuccessEN, attractSuccessSN); }
                    }

                    CheckCompletion();
                }

                if (isRightActive)
                    magnetRight.localPosition = new Vector3(magnetLeft.localPosition.x + snapDistance, rightStartPos.y, rightStartPos.z);
                else
                    magnetLeft.localPosition = new Vector3(magnetRight.localPosition.x - snapDistance, leftStartPos.y, leftStartPos.z);
            }
            else
            {
                isSnapped = false;
            }
        }
        // --- LIKE POLES: REPULSION ---
        else 
        {
            if (distance <= repulsionDistance + 0.002f) 
            {
                Vector3 targetPassivePos = Vector3.zero;

                // Push passive magnet away
                if (isRightActive)
                    targetPassivePos = new Vector3(magnetRight.localPosition.x - repulsionDistance, leftStartPos.y, leftStartPos.z);
                else
                    targetPassivePos = new Vector3(magnetLeft.localPosition.x + repulsionDistance, rightStartPos.y, rightStartPos.z);

                // Add struggle jitter if pushed to travel boundary
                bool isAtBoundary = false;
                if (isRightActive)
                    isAtBoundary = (magnetRight.localPosition.x <= rightStartPos.x - maxSwipeDistance + 0.005f);
                else
                    isAtBoundary = (magnetLeft.localPosition.x >= leftStartPos.x + maxSwipeDistance - 0.005f);

                if (isAtBoundary)
                {
                    isActivelyRepelling = true; 
                    float shakeOffset = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
                    targetPassivePos.x += shakeOffset;
                    targetPassivePos.z += Mathf.Cos(Time.time * shakeSpeed * 0.8f) * (shakeIntensity * 0.5f);
                }

                if (isRightActive)
                    magnetLeft.localPosition = targetPassivePos;
                else
                    magnetRight.localPosition = targetPassivePos;

                float displacement = isRightActive ? (leftStartPos.x - magnetLeft.localPosition.x) : (magnetRight.localPosition.x - rightStartPos.x);

                if (displacement > 0.002f) 
                {
                    if (!isAtBoundary) isActivelyRepelling = true; 

                    // Play repulsion voiceover & SFX
                    if (!isRepelSuccessTriggered)
                    {
                        isRepelSuccessTriggered = true;
                        Handheld.Vibrate(); 

                        if (sfxAudioSource != null && magneticHumSFX != null) sfxAudioSource.PlayOneShot(magneticHumSFX);

                        // Track test combination (N-N or S-S) and explain repulsion
                        if (leftInnerIsNorth) 
                        {
                            if (!tested_NN) { tested_NN = true; PlayAudioAndText(repelNNVoiceEN, repelNNVoiceSN, introTopEN, introTopSN, repelNNSuccessEN, repelNNSuccessSN); }
                        } 
                        else 
                        {
                            if (!tested_SS) { tested_SS = true; PlayAudioAndText(repelSSVoiceEN, repelSSVoiceSN, introTopEN, introTopSN, repelSSSuccessEN, repelSSSuccessSN); }
                        }

                        CheckCompletion();
                    }
                }
            }
            else
            {
                isRepelSuccessTriggered = false; 
            }
        }
    }

    // --- Progression Check ---
    // If student has tested at least one attraction and both N-N and S-S repulsions, reveal Next button
    private void CheckCompletion()
    {
        if (!isQuizComplete && (tested_NS || tested_SN) && tested_NN && tested_SS)
        {
            isQuizComplete = true; 
            StartCoroutine(WaitAndShowNextButton());
        }
    }

    // Waits until voiceover ends before showing Next button to prevent audio cutoffs
    private IEnumerator WaitAndShowNextButton()
    {
        while (voiceAudioSource != null && voiceAudioSource.isPlaying)
        {
            yield return null; 
        }
        
        if (nextButton != null) nextButton.SetActive(true);
    }

    // Subtle 40ms pickup vibration on Android
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

    // --- Audio & Subtitle Helper ---
    // Displays localized dialogue and plays voiceover clip in English or Sinhala
    private void PlayAudioAndText(AudioClip clipEN, AudioClip clipSN, string topEN, string topSN, string subEN, string subSN)
    {
        bool isSinhala = PlayerPrefs.GetString("Language", "English") == "Sinhala";

        if (feedbackPanel != null) feedbackPanel.SetActive(true);

        if (isSinhala)
        {
            if (topTextEN != null) topTextEN.gameObject.SetActive(false);
            if (subtitleEN != null) subtitleEN.gameObject.SetActive(false);
            if (topTextSN != null) { topTextSN.gameObject.SetActive(true); topTextSN.text = topSN; }
            if (subtitleSN != null) { subtitleSN.gameObject.SetActive(true); subtitleSN.text = subSN; }
            if (voiceAudioSource != null && clipSN != null) { voiceAudioSource.Stop(); voiceAudioSource.PlayOneShot(clipSN); }
        }
        else
        {
            if (topTextSN != null) topTextSN.gameObject.SetActive(false);
            if (subtitleSN != null) subtitleSN.gameObject.SetActive(false);
            if (topTextEN != null) { topTextEN.gameObject.SetActive(true); topTextEN.text = topEN; }
            if (subtitleEN != null) { subtitleEN.gameObject.SetActive(true); subtitleEN.text = subEN; }
            if (voiceAudioSource != null && clipEN != null) { voiceAudioSource.Stop(); voiceAudioSource.PlayOneShot(clipEN); }
        }
    }
}