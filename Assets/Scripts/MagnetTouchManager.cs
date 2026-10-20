using UnityEngine;
using TMPro;
using System.Collections; 

public class MagnetTouchManager : MonoBehaviour
{
    [Header("UI Controls")]
    public GameObject mainUICanvas; 
    public GameObject magnetUIContainer;
    public GameObject flipLeftButton;
    public GameObject flipRightButton;
    public GameObject resetButton; 
    public GameObject nextButton;
    
    public GameObject feedbackPanel; 

    [Header("UI Cleanup")]
    public GameObject[] elementsToHide;

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

    [Header("Audio Sources")]
    public AudioSource voiceAudioSource;
    public AudioSource sfxAudioSource;   

    [Header("Voiceovers (EN / SN)")]
    public AudioClip introVoiceEN; public AudioClip introVoiceSN;
    public AudioClip attractVoiceEN; public AudioClip attractVoiceSN;
    public AudioClip repelNNVoiceEN; public AudioClip repelNNVoiceSN;
    public AudioClip repelSSVoiceEN; public AudioClip repelSSVoiceSN;

    [Header("Sound Effects")]
    public AudioClip snapSFX;
    public AudioClip magneticHumSFX;
    public AudioClip uiClickSFX; 

    [Header("3D Magnets")]
    public Transform magnetLeft;  
    public Transform magnetRight; 

    [Header("Distance Settings")]
    public float dragSensitivity = 0.0005f; 
    public float maxSwipeDistance = 0.06f; 
    public float snapDistance = 0.08f; 
    public float magneticPullZone = 0.09f; 
    public float repulsionDistance = 0.085f; 

    [Header("Struggle Polish Settings")]
    public float shakeSpeed = 50f;
    public float shakeIntensity = 0.002f;

    private Vector3 leftStartPos;
    private Vector3 rightStartPos;

    private bool leftInnerIsNorth = false; 
    private bool rightInnerIsNorth = true; 

    private bool isSnapped = false;
    private bool isDraggingRight = false;
    private bool isDraggingLeft = false;
    private bool isRepelSuccessTriggered = false;

    private float repelVibrationTimer = 0f; 
    private bool isActivelyRepelling = false; 

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

    public void OnFlipLeftButtonClicked()
    {
        leftInnerIsNorth = !leftInnerIsNorth;
        magnetLeft.localRotation *= Quaternion.Euler(0, 180f, 0);
        ResetPositions();
        if (sfxAudioSource != null && uiClickSFX != null) sfxAudioSource.PlayOneShot(uiClickSFX);
    }

    public void OnFlipRightButtonClicked()
    {
        rightInnerIsNorth = !rightInnerIsNorth;
        magnetRight.localRotation *= Quaternion.Euler(0, 180f, 0);
        ResetPositions();
        if (sfxAudioSource != null && uiClickSFX != null) sfxAudioSource.PlayOneShot(uiClickSFX);
    }

    public void OnResetButtonClicked()
    {
        // 1. Reset the internal memory of the poles to default
        leftInnerIsNorth = false; 
        rightInnerIsNorth = true; 

        // 2. Reset the visual 3D rotation back to the exact default you provided!
        if (magnetLeft != null) magnetLeft.localRotation = Quaternion.Euler(90f, 180f, 0f);
        if (magnetRight != null) magnetRight.localRotation = Quaternion.Euler(90f, 180f, 0f);

        // 3. Reset the positions
        ResetPositions();
        
        if (sfxAudioSource != null && uiClickSFX != null) sfxAudioSource.PlayOneShot(uiClickSFX); 
    }

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
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                CalculateMagneticForces(false, isDraggingRight);
                isDraggingRight = false;
                isDraggingLeft = false;
            }
        }
    }

    private void CalculateMagneticForces(bool isMovingTowards, bool isRightActive)
    {
        bool isAttracting = (leftInnerIsNorth != rightInnerIsNorth);
        float distance = magnetRight.localPosition.x - magnetLeft.localPosition.x;

        if (isAttracting)
        {
            if (!isMovingTowards && distance > snapDistance + 0.002f) 
            {
                isSnapped = false; 
            }
            else if (distance <= magneticPullZone) 
            {
                if (!isSnapped)
                {
                    isSnapped = true;
                    Handheld.Vibrate(); 

                    if (sfxAudioSource != null && snapSFX != null) sfxAudioSource.PlayOneShot(snapSFX);

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
        else 
        {
            if (distance <= repulsionDistance + 0.002f) 
            {
                Vector3 targetPassivePos = Vector3.zero;

                if (isRightActive)
                    targetPassivePos = new Vector3(magnetRight.localPosition.x - repulsionDistance, leftStartPos.y, leftStartPos.z);
                else
                    targetPassivePos = new Vector3(magnetLeft.localPosition.x + repulsionDistance, rightStartPos.y, rightStartPos.z);

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

                    if (!isRepelSuccessTriggered)
                    {
                        isRepelSuccessTriggered = true;
                        Handheld.Vibrate(); 

                        if (sfxAudioSource != null && magneticHumSFX != null) sfxAudioSource.PlayOneShot(magneticHumSFX);

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

    private void CheckCompletion()
    {
        if (!isQuizComplete && (tested_NS || tested_SN) && tested_NN && tested_SS)
        {
            isQuizComplete = true; 
            StartCoroutine(WaitAndShowNextButton());
        }
    }

    private IEnumerator WaitAndShowNextButton()
    {
        while (voiceAudioSource != null && voiceAudioSource.isPlaying)
        {
            yield return null; 
        }
        
        if (nextButton != null) nextButton.SetActive(true);
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