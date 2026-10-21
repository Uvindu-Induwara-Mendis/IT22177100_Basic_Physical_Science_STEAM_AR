using System.Collections;
using UnityEngine;
using TMPro;
using Vuforia;

// =========================================================================================
// SIMPLE MACHINES: PULLEY AR EXPERIMENT (Lifting Heavy Crates)
// =========================================================================================
// Context: Follows the 2D Ramp/Pulley Quiz. In this AR scene, character Dasun uses a pulley
// mechanism to hoist a heavy wooden crate off the ground.
// - Downward screen swipe (or tap) triggers Dasun's arm pulling animation and crank SFX.
// - A 4-point LineRenderer dynamically draws the tensioned rope through the pulley wheel.
// - The crate elevates step-by-step to maxHeight, reinforcing how pulleys redirect pulling force.
// - Upon completion, reveals the storybook button to conclude the lesson.
public class PulleyManager : MonoBehaviour
{
    // --- Vuforia AR Tracking Refresh ---
    [Header("Vuforia Settings")]
    [Tooltip("Lesson 2 Image Target observer toggled to refresh camera tracking")]
    public ObserverBehaviour mainImageTarget;

    // --- Main Scene & Model Containers ---
    [Header("Main Containers")]
    public GameObject pulleyUIContainer; 
    public GameObject modelsContainer;   

    // --- UI Controls & Navigation ---
    [Header("UI Buttons")]
    public GameObject pullButton;        
    public GameObject storyRevealButton; 
    public GameObject retryButton;       // Restarts the pulley lifting experiment

    // --- Dialogue & Instruction Displays ---
    [Header("Instruction UI Elements")]
    public GameObject instructionPanel;  
    public TextMeshProUGUI instructionTextEN;
    public TextMeshProUGUI instructionTextSN;

    [Header("Feedback UI Elements")]
    public GameObject feedbackPanel;     
    public TextMeshProUGUI feedbackTextEN;
    public TextMeshProUGUI feedbackTextSN;

    // --- Audio Player & Clips ---
    [Header("Audio Settings")]
    public AudioSource audioPlayer;
    public AudioClip instructionAudioEN;
    public AudioClip instructionAudioSN;
    public AudioClip feedbackAudioEN;
    public AudioClip feedbackAudioSN;
    public AudioClip pulleyCrankSFX;     // Sound of rope squeak / wheel crank on pull

    // --- Scene Transition Delay & Camera Prompts ---
    [Header("Transition Settings")]
    public float breathingRoomDelay = 1.5f; 
    public AudioClip scanPromptAudioEN; 
    public AudioClip scanPromptAudioSN;

    // --- 3D Crate Model ---
    [Header("3D Models")]
    public Transform crateToLift;
    
    // --- Dasun Character Arm Pull Animation ---
    [Header("Dasun Arm Animation")]
    public Transform leftShoulder;
    public Transform rightShoulder;
    
    [Tooltip("Rotations for Dasun's arms at the bottom of the rope pull")]
    public Vector3 leftArmTargetRotation = new Vector3(30f, 31.2f, 12.8f); 
    public Vector3 rightArmTargetRotation = new Vector3(40f, -5.3f, 7.4f);
    public float pullAnimDuration = 0.2f; 
    private bool isAnimatingArms = false;

    // --- 4-Point Dynamic Pulley Rope ---
    // Rope anchors: Crate -> Wheel Top Left -> Wheel Top Right -> Dasun's Hands
    [Header("Rope Settings (4 Points)")]
    public LineRenderer pulleyRope;
    public Transform crateTiePoint;       
    public Transform wheelTopLeftPoint;   
    public Transform wheelTopRightPoint;  
    public Transform dasunHandPoint;      

    // --- Lifting Physics Settings ---
    [Header("Lifting Mechanics")]
    public float liftAmountPerTap = 0.2f; // Distance crate rises per pull
    public float maxHeight = 1.5f;        // Target lift height to trigger success
    
    private Vector3 startingCratePos;
    private bool isCompleted = false;     
    
    private Quaternion initialLeftArmRot;
    private Quaternion initialRightArmRot;
    
    private bool isMarkerTracked = false; 
    private bool hasStartedScene = false; 
    private bool isBreathingRoomOver = false;
    private bool hasPromptedToScan = false;
    
    private Quaternion currentLeftArmAnimRot;
    private Quaternion currentRightArmAnimRot;

    // --- Instruction & Feedback Strings ---
    private string instEN = "Swipe DOWN on your screen to help Dasun pull the heavy crate up!";
    private string instSN = "පෙට්ටිය ඉහළට එසවීමට තිරය මත පහළට අදින්න!";
    private string feedEN = "Great job! The pulley made lifting the heavy crate so much easier!";
    private string feedSN = "ඉතා හොඳයි! කප්පිය නිසා බර පෙට්ටිය එසවීම ඉතා පහසු විය!";
    private string scanPromptEN = "Awesome! Now point your camera back at the book to continue!";
    private string scanPromptSN = "නියමයි! දැන් ඔබේ කැමරාව නැවතත් පොත වෙත යොමු කරන්න!";

    // --- AR Marker Tracking Callbacks ---
    public void VuforiaTargetFound()
    {
        isMarkerTracked = true; 

        if (hasStartedScene && modelsContainer != null && modelsContainer != this.gameObject)
        {
            modelsContainer.SetActive(true);
        }

        if (isBreathingRoomOver)
        {
            if (hasPromptedToScan && !hasStartedScene && audioPlayer != null)
            {
                audioPlayer.Stop();
            }
            CheckAndStartScene();
        }
    }

    public void VuforiaTargetLost()
    {
        isMarkerTracked = false; 

        if (modelsContainer != null && modelsContainer != this.gameObject)
        {
            modelsContainer.SetActive(false);
        }
    }

    // Toggles Vuforia target to refresh detection when this scene opens
    void OnEnable()
    {
        if (mainImageTarget != null)
        {
            mainImageTarget.enabled = false;
            mainImageTarget.enabled = true;
        }

        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in allRenderers)
        {
            r.enabled = true;
        }

        isCompleted = false;
        hasStartedScene = false; 
        isBreathingRoomOver = false;
        hasPromptedToScan = false;
        isAnimatingArms = false;
        isMarkerTracked = false; 

        if (crateToLift != null && startingCratePos != Vector3.zero) 
        {
            crateToLift.localPosition = startingCratePos;
        }
        
        if (pulleyUIContainer != null) pulleyUIContainer.SetActive(false);
        if (retryButton != null) retryButton.SetActive(false);

        if (modelsContainer != null && modelsContainer != this.gameObject) 
        {
            modelsContainer.SetActive(false); 
        }

        StartCoroutine(BreathingRoomRoutine());
    }

    // Gives camera time to stabilize before prompting user to scan
    private IEnumerator BreathingRoomRoutine()
    {
        yield return new WaitForSeconds(breathingRoomDelay);
        isBreathingRoomOver = true;

        if (isMarkerTracked) 
            CheckAndStartScene();
        else 
        { 
            hasPromptedToScan = true; 
            PlayScanPromptAudio(); 
        }
    }

    private void PlayScanPromptAudio()
    {
        string currentLang = PlayerPrefs.GetString("Language", "English");
        bool isEnglish = (currentLang != "Sinhala");
        AudioClip clipToPlay = isEnglish ? scanPromptAudioEN : scanPromptAudioSN;

        if (pulleyUIContainer != null) pulleyUIContainer.SetActive(true);
        if (instructionPanel != null) instructionPanel.SetActive(true);
        if (pullButton != null) pullButton.SetActive(false); 

        if (isEnglish)
        {
            if (instructionTextEN != null) { instructionTextEN.text = scanPromptEN; instructionTextEN.gameObject.SetActive(true); }
            if (instructionTextSN != null) instructionTextSN.gameObject.SetActive(false);
        }
        else
        {
            if (instructionTextEN != null) instructionTextEN.gameObject.SetActive(false);
            if (instructionTextSN != null) { instructionTextSN.text = scanPromptSN; instructionTextSN.gameObject.SetActive(true); }
        }

        if (audioPlayer != null && clipToPlay != null)
        {
            audioPlayer.Stop();
            audioPlayer.clip = clipToPlay;
            audioPlayer.Play();
        }
    }

    private void CheckAndStartScene()
    {
        if (gameObject.activeInHierarchy && isMarkerTracked && !hasStartedScene && !isCompleted)
        {
            hasStartedScene = true;
            ShowInstructions();
        }
    }

    void Start()
    {
        if (storyRevealButton != null) storyRevealButton.SetActive(false);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        if (crateToLift != null) startingCratePos = crateToLift.localPosition;
        
        if (leftShoulder != null) initialLeftArmRot = leftShoulder.localRotation;
        if (rightShoulder != null) initialRightArmRot = rightShoulder.localRotation;
    }

    void Update()
    {
        DrawPulleyRope();
    }
    
    // LateUpdate overrides Unity Animator rotations so procedural arm pull poses apply
    void LateUpdate()
    {
        if (isAnimatingArms)
        {
            if (leftShoulder != null) leftShoulder.localRotation = currentLeftArmAnimRot;
            if (rightShoulder != null) rightShoulder.localRotation = currentRightArmAnimRot;
        }
    }

    // Connects 4 line segments between crate, top pulley wheel, and Dasun's hands
    private void DrawPulleyRope()
    {
        if (pulleyRope == null || crateTiePoint == null || wheelTopLeftPoint == null || wheelTopRightPoint == null || dasunHandPoint == null)
            return;

        pulleyRope.positionCount = 4; 
        pulleyRope.SetPosition(0, crateTiePoint.position);      
        pulleyRope.SetPosition(1, wheelTopLeftPoint.position);  
        pulleyRope.SetPosition(2, wheelTopRightPoint.position); 
        pulleyRope.SetPosition(3, dasunHandPoint.position);     
    }

    // --- Core Action: Lifts Crate Upward ---
    // Triggered by downward screen swipe via PulleySwipeManager (or pull button tap)
    public void LiftCrate()
    {
        if (crateToLift == null || isCompleted) return;

        float currentHeight = crateToLift.localPosition.y - startingCratePos.y;

        if (currentHeight < maxHeight)
        {
            if (audioPlayer != null && pulleyCrankSFX != null)
            {
                audioPlayer.PlayOneShot(pulleyCrankSFX);
            }

            if (!isAnimatingArms)
            {
                StartCoroutine(AnimateDasunArms());
            }

            // Raise crate
            crateToLift.localPosition += new Vector3(0, liftAmountPerTap, 0);

            // Reached target elevation: complete activity
            if ((crateToLift.localPosition.y - startingCratePos.y) >= maxHeight)
            {
                StartCoroutine(HandleCompletionSequence());
            }
        }
    }

    // Pulls Dasun's arms down and returns them up smoothly
    private IEnumerator AnimateDasunArms()
    {
        if (leftShoulder == null || rightShoulder == null) yield break;

        isAnimatingArms = true;
        
        Quaternion targetL = Quaternion.Euler(leftArmTargetRotation);
        Quaternion targetR = Quaternion.Euler(rightArmTargetRotation);

        float elapsed = 0;
        float halfDuration = pullAnimDuration / 2f;

        // Arm motion Phase 1: Pull Down
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            
            currentLeftArmAnimRot = Quaternion.Lerp(initialLeftArmRot, targetL, t);
            currentRightArmAnimRot = Quaternion.Lerp(initialRightArmRot, targetR, t);
            yield return null;
        }

        // Arm motion Phase 2: Return Up
        elapsed = 0;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            
            currentLeftArmAnimRot = Quaternion.Lerp(targetL, initialLeftArmRot, t);
            currentRightArmAnimRot = Quaternion.Lerp(targetR, initialRightArmRot, t);
            yield return null;
        }

        currentLeftArmAnimRot = initialLeftArmRot;
        currentRightArmAnimRot = initialRightArmRot;
        isAnimatingArms = false;
    }

    // Displays initial prompt to swipe down and lift crate
    private void ShowInstructions()
    {
        string currentLang = PlayerPrefs.GetString("Language", "English");
        bool isEnglish = (currentLang != "Sinhala");

        if (pulleyUIContainer != null) pulleyUIContainer.SetActive(true);
        if (modelsContainer != null && modelsContainer != this.gameObject) 
            modelsContainer.SetActive(true);

        if (instructionPanel != null) instructionPanel.SetActive(true);
        if (pullButton != null) pullButton.SetActive(true);
        if (storyRevealButton != null) storyRevealButton.SetActive(false);
        if (retryButton != null) retryButton.SetActive(true);

        if (isEnglish)
        {
            if (instructionTextEN != null) { instructionTextEN.text = instEN; instructionTextEN.gameObject.SetActive(true); }
            if (instructionTextSN != null) instructionTextSN.gameObject.SetActive(false);
            PlayAudio(instructionAudioEN);
        }
        else
        {
            if (instructionTextEN != null) instructionTextEN.gameObject.SetActive(false);
            if (instructionTextSN != null) { instructionTextSN.text = instSN; instructionTextSN.gameObject.SetActive(true); }
            PlayAudio(instructionAudioSN);
        }
    }

    // Handles success voiceover and reveals the story conclusion button
    private IEnumerator HandleCompletionSequence()
    {
        isCompleted = true; 

        if (instructionPanel != null) instructionPanel.SetActive(false);
        if (pullButton != null) pullButton.SetActive(false);
        if (retryButton != null) retryButton.SetActive(false);

        string currentLang = PlayerPrefs.GetString("Language", "English");
        bool isEnglish = (currentLang != "Sinhala");

        if (feedbackPanel != null) feedbackPanel.SetActive(true);

        AudioClip clipToPlay = null;

        if (isEnglish)
        {
            if (feedbackTextEN != null) { feedbackTextEN.text = feedEN; feedbackTextEN.gameObject.SetActive(true); }
            if (feedbackTextSN != null) feedbackTextSN.gameObject.SetActive(false);
            clipToPlay = feedbackAudioEN;
        }
        else
        {
            if (feedbackTextEN != null) feedbackTextEN.gameObject.SetActive(false);
            if (feedbackTextSN != null) { feedbackTextSN.text = feedSN; feedbackTextSN.gameObject.SetActive(true); }
            clipToPlay = feedbackAudioSN;
        }

        PlayAudio(clipToPlay);

        float waitTime = clipToPlay != null ? clipToPlay.length + 0.5f : 3.0f;
        yield return new WaitForSeconds(waitTime);

        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        if (storyRevealButton != null) storyRevealButton.SetActive(true);
    }

    private void PlayAudio(AudioClip clip)
    {
        if (audioPlayer != null && clip != null)
        {
            audioPlayer.Stop();
            audioPlayer.clip = clip;
            audioPlayer.Play();
        }
    }

    // --- Retry / Reset Pulley Experiment ---
    // Resets crate position to ground, restores arm poses, and re-enables instructions
    public void ResetExperiment()
    {
        if (CircuitAnalyticsManager.Instance != null)
        {
            CircuitAnalyticsManager.Instance.RecordRetry();
        }

        StopAllCoroutines();
        isAnimatingArms = false;
        
        currentLeftArmAnimRot = initialLeftArmRot;
        currentRightArmAnimRot = initialRightArmRot;
        
        isCompleted = false;
        if (crateToLift != null && startingCratePos != Vector3.zero) 
        {
            crateToLift.localPosition = startingCratePos;
        }

        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        if (storyRevealButton != null) storyRevealButton.SetActive(false);
        
        ShowInstructions();
    }
}