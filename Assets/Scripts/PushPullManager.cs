using System.Collections;
using UnityEngine;
using TMPro;

public class PushPullManager : MonoBehaviour
{
    [Header("3D Models")]
    public Transform theCrate;
    public GameObject pushArrow; 
    public GameObject pullArrow; 

    [Header("Rope Physics")]
    public LineRenderer pullRope; 
    public Transform ropeAnchor; 
    public Transform crateTiePoint;    
    public int ropeSegments = 20;      
    public float maxRopeSag = 0.5f;    

    [Header("UI Elements")]
    public GameObject mainUIPanel;       
    public GameObject lesson2UIContainer; 
    public GameObject pushButton;  
    public GameObject pullButton;  
    public GameObject nextButton; 
    public GameObject retryButton; // --- NEW: Toggle the retry button ---
    
    [Tooltip("Drag any Lesson 1 UI elements here that need to be hidden")]
    public GameObject[] lesson1ElementsToHide; 

    [Header("Quiz Transition")]
    public GameObject lesson2QuizPanel_Q1; 

    [Header("Top Instructions (Simple Guide)")]
    public TextMeshProUGUI topInstruction_EN; 
    public TextMeshProUGUI topInstruction_SN; 
    
    public string topTextIntroEN = "Task: Tap 'Push' to move the crate away from Dasun!";
    public string topTextIntroSI = "[Enter Sinhala Here]";
    public string topTextPushEN = "Task: Now tap 'Pull' to bring it towards Dasun!";
    public string topTextPushSI = "[Enter Sinhala Here]";
    public string topTextPullEN = "Great job! Tap 'Next' to continue!";
    public string topTextPullSI = "[Enter Sinhala Here]";

    [Header("Subtitles & Text (Dasun's Voice)")]
    public TextMeshProUGUI subtitleText_EN; 
    public TextMeshProUGUI subtitleText_SN; 
    
    public string introTextEN = "Let's help Dasun! Tap 'Push' to push the crate away from him.";
    public string introTextSI = "[Enter Sinhala Intro Here]";
    public string pushTextEN = "Great push! Pushing is a force that moves object away from you. By pushing the crate, you moved the crate away from Dasun. Now let's tap 'Pull'! to pull it towards Dasun!";
    public string pushTextSI = "[Enter Sinhala Push Here]";
    public string pullTextEN = "Awesome! Pulling is a force that brings an object closer to you. By pulling the rope, you brought the crate closer to Dasun!";
    public string pullTextSI = "[Enter Sinhala Pull Here]";

    [Header("Movement Settings")]
    public float slideDistance = 0.5f; 
    public float slideSpeed = 3.0f; 
    public float tiltAngle = 15f; 
    public float arrowLingerTime = 0.6f; 

    [Header("Audio Settings")]
    public AudioSource audioPlayer;
    public AudioClip introInstructionEN;
    public AudioClip introInstructionSI;
    public AudioClip pushInstructionEN;
    public AudioClip pushInstructionSI;
    public AudioClip pullInstructionEN;
    public AudioClip pullInstructionSI;
    
    [Tooltip("Drag your wooden scrape/slide sound effect here!")]
    public AudioClip crateSlideSFX;

    private bool isMoving = false;
    private bool hasPushed = false;
    private bool hasPulled = false;
    private bool isCurrentlyPulling = false; 
    
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private bool isInitialized = false;

    void Awake()
    {
        InitializeState();
        HideUI(); 
    }

    void Update()
    {
        if (pullRope != null && pullRope.gameObject.activeInHierarchy)
        {
            DrawRope();
        }
    }

    private void InitializeState()
    {
        if (!isInitialized && theCrate != null)
        {
            startingPosition = theCrate.localPosition;
            startingRotation = theCrate.localRotation;
            isInitialized = true;
        }
    }

    public void ShowUI()
    {
        // Safety Lock
        if (!gameObject.activeInHierarchy || theCrate == null || !theCrate.gameObject.activeInHierarchy) return; 

        InitializeState(); 

        isMoving = false;
        hasPushed = false;
        hasPulled = false;
        isCurrentlyPulling = false;
        
        if (theCrate != null && isInitialized)
        {
            theCrate.localPosition = startingPosition;
            theCrate.localRotation = startingRotation;
        }

        if (mainUIPanel != null) mainUIPanel.SetActive(true);
        if (lesson2UIContainer != null) lesson2UIContainer.SetActive(true);
        
        if (pushButton != null) pushButton.SetActive(true);
        if (pullButton != null) pullButton.SetActive(false);
        if (nextButton != null) nextButton.SetActive(false); 
        if (retryButton != null) retryButton.SetActive(true); 

        if (pushArrow != null) pushArrow.SetActive(false);
        if (pullArrow != null) pullArrow.SetActive(false);
        
        if (pullRope != null) pullRope.gameObject.SetActive(false);

        foreach (GameObject uiElement in lesson1ElementsToHide)
        {
            if (uiElement != null) uiElement.SetActive(false);
        }

        PlayAudioAndText(introInstructionEN, introInstructionSI, introTextEN, introTextSI);
        UpdateTopInstruction(topTextIntroEN, topTextIntroSI);
    }

    public void HideUI()
    {
        // Safety Lock
        if (!gameObject.activeInHierarchy || theCrate == null || !theCrate.gameObject.activeInHierarchy) return;

        if (lesson2UIContainer != null) lesson2UIContainer.SetActive(false);
        if (nextButton != null) nextButton.SetActive(false);
        if (mainUIPanel != null) mainUIPanel.SetActive(false);
        if (retryButton != null) retryButton.SetActive(false);
        
        if (pushArrow != null) pushArrow.SetActive(false);
        if (pullArrow != null) pullArrow.SetActive(false);
        if (pullRope != null) pullRope.gameObject.SetActive(false);

        if (audioPlayer != null && audioPlayer.isPlaying)
        {
            audioPlayer.Stop();
        }
    }

    public void OnPushTapped()
    {
        if (isMoving || theCrate == null) return; 
        
        // --- HAPTIC VIBRATION ADDED HERE ---
        HapticFeedbackManager.TriggerSubtleHaptic(40L);
        
        hasPushed = true;

        if (pushButton != null) pushButton.SetActive(false); 

        PlayAudioAndText(pushInstructionEN, pushInstructionSI, pushTextEN, pushTextSI);
        UpdateTopInstruction(topTextPushEN, topTextPushSI);

        Vector3 targetPosition = startingPosition + (Vector3.right * slideDistance);
        StartCoroutine(JuicySlideRoutine(targetPosition, pushArrow, true)); 
    }

    public void OnPullTapped()
    {
        if (isMoving || theCrate == null) return; 
        
        // --- HAPTIC VIBRATION ADDED HERE ---
        HapticFeedbackManager.TriggerSubtleHaptic(40L);
        
        hasPulled = true;

        if (pullButton != null) pullButton.SetActive(false); 

        PlayAudioAndText(pullInstructionEN, pullInstructionSI, pullTextEN, pullTextSI);
        UpdateTopInstruction(topTextPullEN, topTextPullSI);

        Vector3 targetPosition = startingPosition; 
        StartCoroutine(JuicySlideRoutine(targetPosition, pullArrow, false));
    }

    public void OnNextButtonTapped()
    {
        // 1. Hide the UI
        if (lesson2UIContainer != null) lesson2UIContainer.SetActive(false);
        if (subtitleText_EN != null) subtitleText_EN.gameObject.SetActive(false);
        if (subtitleText_SN != null) subtitleText_SN.gameObject.SetActive(false);
        if (topInstruction_EN != null) topInstruction_EN.gameObject.SetActive(false);
        if (topInstruction_SN != null) topInstruction_SN.gameObject.SetActive(false);
        if (retryButton != null) retryButton.SetActive(false);

        if (audioPlayer != null) audioPlayer.Stop();

        // 2. Start the Quiz!
        if (lesson2QuizPanel_Q1 != null) lesson2QuizPanel_Q1.SetActive(true);

        // --- THE FIX: KILL THE ZOMBIE SCENE ---
        // 3. Completely shut down this AR interaction folder so Vuforia can't trigger it anymore!
        gameObject.SetActive(false);
    }

    private void PlayAudioAndText(AudioClip clipEN, AudioClip clipSI, string textEN, string textSI)
    {
        if (subtitleText_EN != null) subtitleText_EN.text = textEN;
        if (subtitleText_SN != null) subtitleText_SN.text = textSI;

        string currentLang = PlayerPrefs.GetString("Language", "English");
        AudioClip clipToPlay = (currentLang == "Sinhala") ? clipSI : clipEN;

        if (audioPlayer != null && clipToPlay != null)
        {
            audioPlayer.Stop(); 
            audioPlayer.clip = clipToPlay;
            audioPlayer.Play();
        }
    }

    private void UpdateTopInstruction(string textEN, string textSI)
    {
        if (topInstruction_EN != null) topInstruction_EN.text = textEN;
        if (topInstruction_SN != null) topInstruction_SN.text = textSI;
    }

    private IEnumerator JuicySlideRoutine(Vector3 targetPos, GameObject activeArrow, bool isPushing)
    {
        isMoving = true;
        isCurrentlyPulling = !isPushing; 

        if (activeArrow != null) activeArrow.SetActive(true);

        if (audioPlayer != null && crateSlideSFX != null)
        {
            audioPlayer.PlayOneShot(crateSlideSFX);
        }

        Vector3 startLerpPos = theCrate.localPosition; 
        float targetTilt = isPushing ? -tiltAngle : tiltAngle;
        Quaternion tiltRotation = startingRotation * Quaternion.Euler(0, 0, targetTilt);

        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * slideSpeed;
            theCrate.localPosition = Vector3.Lerp(startLerpPos, targetPos, progress); 
            float currentTilt = Mathf.PingPong(progress * 2f, 1f); 
            theCrate.localRotation = Quaternion.Lerp(startingRotation, tiltRotation, currentTilt);
            yield return null; 
        }

        theCrate.localPosition = targetPos;
        theCrate.localRotation = startingRotation; 

        yield return new WaitForSeconds(arrowLingerTime);
        
        if (activeArrow != null) activeArrow.SetActive(false);

        isMoving = false;
        isCurrentlyPulling = false; 

        if (isPushing)
        {
            if (pullButton != null) pullButton.SetActive(true);
            if (pullRope != null) pullRope.gameObject.SetActive(true);
        }
        else
        {
            if (pushButton != null) pushButton.SetActive(true);
            if (pullRope != null) pullRope.gameObject.SetActive(false);
        }

        CheckProgress();
    }

    private void DrawRope()
    {
        if (pullRope == null || ropeAnchor == null || theCrate == null) return;

        pullRope.useWorldSpace = false;
        pullRope.positionCount = ropeSegments;
        
        Vector3 endWorldPos = (crateTiePoint != null) ? crateTiePoint.position : theCrate.position;
        Vector3 endPoint = pullRope.transform.InverseTransformPoint(endWorldPos);
        Vector3 startPoint = pullRope.transform.InverseTransformPoint(ropeAnchor.position);

        Vector3 crateLocal = pullRope.transform.InverseTransformPoint(theCrate.position);
        float floorHeight = crateLocal.y; 

        if (!isCurrentlyPulling)
        {
            startPoint.y = floorHeight; 
        }

        float finalSag = isCurrentlyPulling ? 0f : maxRopeSag;

        for (int i = 0; i < ropeSegments; i++)
        {
            float t = i / (float)(ropeSegments - 1);
            Vector3 pointPos = Vector3.Lerp(startPoint, endPoint, t);

            float sagCurve = Mathf.Sin(t * Mathf.PI) * finalSag;
            pointPos.y -= sagCurve;
            
            if (pointPos.y < floorHeight) pointPos.y = floorHeight; 

            pullRope.SetPosition(i, pointPos);
        }
    }

    private void CheckProgress()
    {
        if (hasPushed && hasPulled && nextButton != null)
        {
            nextButton.SetActive(true);
        }
    }

    // --- NEW: Contextual Retry Method for the AR Part ---
    public void ResetExperiment()
    {
        // 1. Analytics
        if (CircuitAnalyticsManager.Instance != null)
        {
            CircuitAnalyticsManager.Instance.RecordRetry();
        }

        // 2. Stop everything
        StopAllCoroutines();
        if (audioPlayer != null) audioPlayer.Stop();

        // 3. Reset internal state
        isMoving = false;
        hasPushed = false;
        hasPulled = false;
        isCurrentlyPulling = false;

        // 4. Reset Crate
        if (theCrate != null && isInitialized)
        {
            theCrate.localPosition = startingPosition;
            theCrate.localRotation = startingRotation;
        }

        // 5. Reset UI
        if (pushButton != null) pushButton.SetActive(true);
        if (pullButton != null) pullButton.SetActive(false);
        if (nextButton != null) nextButton.SetActive(false);
        if (pushArrow != null) pushArrow.SetActive(false);
        if (pullArrow != null) pullArrow.SetActive(false);
        if (pullRope != null) pullRope.gameObject.SetActive(false);

        // 6. Play instructions again
        PlayAudioAndText(introInstructionEN, introInstructionSI, introTextEN, introTextSI);
        UpdateTopInstruction(topTextIntroEN, topTextIntroSI);
    }
}