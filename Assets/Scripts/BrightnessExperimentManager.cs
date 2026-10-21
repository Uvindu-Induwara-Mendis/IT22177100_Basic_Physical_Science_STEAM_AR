using UnityEngine;
using TMPro; 

// =========================================================================================
// LESSON 1 (ELECTRIC CIRCUITS): PHASE 2 - BATTERY SERIES & BULB BRIGHTNESS AR EXPERIMENT
// =========================================================================================
// Context: Follows the 2D Circuit Quiz. In this AR scene, the student taps 'Add Battery'
// to stack dry cells in series. As more batteries are stacked, the 3D bulb's light intensity
// and range visibly increase, teaching the concept of voltage/current in series circuits.
public class BrightnessExperimentManager : MonoBehaviour
{
    // --- Scene & Quiz Manager References ---
    [Header("Managers")]
    public StoryUIManager storyManager;      // Storybook narrative & subtitle display
    public CircuitCoreQuiz coreQuizScript;    // Quiz manager to notify when experiment is complete

    // --- UI Controls & Text Prompts ---
    [Header("UI Connections")]
    public GameObject addBatteryButton;       // Button to stack the next battery
    public GameObject retryButton;            // Allows student to restart the battery stacking experiment
    public TextMeshProUGUI topInstructionText_EN; 
    public TextMeshProUGUI topInstructionText_SN; 
    
    // --- Audio & Sound Effects ---
    [Header("Sound Effects")]
    public AudioClip batteryStackSFX;         // Click/snap sound when a battery is added

    // --- 3D Circuit Models & Light Component ---
    [Header("3D Models & Physics")]
    public GameObject batteryPrefab;          // 3D model of the dry cell to spawn
    public Transform firstBatteryLocation;    // Starting transform where base battery sits
    public Transform batteryPlusNode;         // Positive terminal point (moves upward as batteries stack)
    public Light bulbLight;                   // Unity Light component inside the bulb that brightens
    public Transform bulbModel;               // Bulb 3D model transform (slides up to sit on top of stack)

    // --- Stacking Offsets & Lighting Gains ---
    [Header("Stacking Settings")]
    public float stackHeightOffset = 0.035f;       // Distance offset along forward/up axis per battery
    public float lightIntensityIncrease = 5.0f;    // Light intensity added per extra battery
    public float lightRangeIncrease = 1.0f;        // Light reach/range added per extra battery

    // --- Voiceover Dialogue & Subtitles ---
    [Header("Intro Audio & Text")]
    public AudioClip introVoice_EN;
    public AudioClip introVoice_SN;
    public string introText_EN = "Let's add batteries in a series connection! Tap the button and watch the light.";
    public string introText_SN = "wms negß tl fm<g ^fYa%Ks.;j& iïnkaO lruq fnd;a;u Tn,d n,aí tl m;a;= fjk yeá n,kakflda"; 

    [Header("Success Audio & Text")]
    public AudioClip successVoice_EN;
    public AudioClip successVoice_SN;
    public string successText_EN = "Great job! By connecting multiple dry cells the right way, we gave the bulb enough power to shine brightly!";
    public string successText_SN = "kshuhs mßm:hg ;j;a negß tl;= l<du f.dvla n,h ,efnkjd ta ksid n,aí tl ;j;a fyd|g t<sh fokjd";

    [Header("Top Instructions")]
    public string topInstIntro_EN = "Tap the 'Add Battery' button to see what happens!";
    [Tooltip("Paste your FM-Abhaya converted text here!")]
    public string topInstIntro_SN = "negß tl;= lrk fnd;a;u Tn,d fudlo fjkafka lsh,d n,uq"; 
    
    public string topInstOutro_EN = "Experiment Complete! Read the explanation below.";
    [Tooltip("Paste your FM-Abhaya converted text here!")]
    public string topInstOutro_SN = "mÍlaIKh bjrhs"; 

    // --- Runtime Battery Tracking ---
    private int currentBatteries = 1;         // Starts with 1 base battery
    public int maxBatteries = 3;              // Target total: 3 batteries
    private System.Collections.Generic.List<GameObject> spawnedBatteries = new System.Collections.Generic.List<GameObject>();
    private float initialScaleZ = 1f; 
    private AudioSource localAudioSource;

    private void Awake()
    {
        // Add a local audio source in case StoryUIManager is temporarily inactive
        localAudioSource = gameObject.AddComponent<AudioSource>();
        localAudioSource.playOnAwake = false;
    }

    void Start()
    {
        if (firstBatteryLocation != null)
        {
            initialScaleZ = firstBatteryLocation.lossyScale.z;
        }
    }

    // --- Starts the Battery Stacking Phase ---
    // Called when user transitions from Circuit Quiz to the Brightness Experiment in AR
    public void PlayIntroVoiceover()
    {
        // Resume analytics AR timer
        if (CircuitAnalyticsManager.Instance != null)
            CircuitAnalyticsManager.Instance.StartARTimer();

        PlayAudioAndText(introVoice_EN, introVoice_SN, introText_EN, introText_SN);

        if (retryButton != null) retryButton.SetActive(true);

        // Set instruction text based on language preference
        string currentLang = PlayerPrefs.GetString("Language", "English");
        if (currentLang == "Sinhala")
        {
            if (topInstructionText_EN != null) topInstructionText_EN.gameObject.SetActive(false);
            if (topInstructionText_SN != null) 
            {
                topInstructionText_SN.gameObject.SetActive(true);
                topInstructionText_SN.text = topInstIntro_SN;
            }
        }
        else
        {
            if (topInstructionText_SN != null) topInstructionText_SN.gameObject.SetActive(false);
            if (topInstructionText_EN != null) 
            {
                topInstructionText_EN.gameObject.SetActive(true);
                topInstructionText_EN.text = topInstIntro_EN;
            }
        }
    }

    // --- On 'Add Battery' Button Tap ---
    // Spawns another dry cell on top, shifts the bulb up, and boosts bulb light brightness
    public void OnAddBatteryClicked()
    {
        HapticFeedbackManager.TriggerSubtleHaptic(40L);
        if (currentBatteries < maxBatteries)
        {
            // Adjust offset dynamically if AR model was scaled by user pinch-zoom
            float currentScaleZ = firstBatteryLocation.lossyScale.z;
            float zoomRatio = (initialScaleZ > 0) ? (currentScaleZ / initialScaleZ) : 1f;
            float dynamicOffset = stackHeightOffset * zoomRatio;

            // Compute position for new battery on top of stack
            Vector3 localUpwardOffset = firstBatteryLocation.forward * (dynamicOffset * currentBatteries);
            Vector3 newPosition = firstBatteryLocation.position + localUpwardOffset;
            
            // Spawn battery and parent it to the AR target
            GameObject newBattery = Instantiate(batteryPrefab, newPosition, firstBatteryLocation.rotation, firstBatteryLocation.parent);
            newBattery.transform.localScale = firstBatteryLocation.localScale;
            spawnedBatteries.Add(newBattery);

            // Play stacking sound effect
            if (batteryStackSFX != null && Camera.main != null)
            {
                AudioSource.PlayClipAtPoint(batteryStackSFX, Camera.main.transform.position);
            }

            // Move the positive terminal point upward
            if (batteryPlusNode != null)
            {
                batteryPlusNode.position += (firstBatteryLocation.forward * dynamicOffset);
            }

            // Move the 3D bulb model so it rests on top of the newest battery
            if (bulbModel != null)
            {
                bulbModel.position += (firstBatteryLocation.forward * dynamicOffset);
            }

            // Increase bulb illumination (demonstrates series voltage increase)
            if (bulbLight != null)
            {
                bulbLight.intensity += lightIntensityIncrease;
                bulbLight.range += lightRangeIncrease;
            }

            currentBatteries++;

            // If reached maximum batteries (3), complete the experiment
            if (currentBatteries >= maxBatteries) FinishExperiment();
        }
    }

    // --- Completes Phase 2 Experiment ---
    // Plays celebration voiceover, wraps up Lesson 1, and submits session analytics
    private void FinishExperiment()
    {
        Handheld.Vibrate();

        if (addBatteryButton != null) addBatteryButton.SetActive(false);
        if (retryButton != null) retryButton.SetActive(false);
        PlayAudioAndText(successVoice_EN, successVoice_SN, successText_EN, successText_SN);

        // Update top instructions to completion message
        string currentLang = PlayerPrefs.GetString("Language", "English");
        if (currentLang == "Sinhala")
        {
            if (topInstructionText_EN != null) topInstructionText_EN.gameObject.SetActive(false);
            if (topInstructionText_SN != null) 
            {
                topInstructionText_SN.gameObject.SetActive(true);
                topInstructionText_SN.text = topInstOutro_SN;
            }
        }
        else
        {
            if (topInstructionText_SN != null) topInstructionText_SN.gameObject.SetActive(false);
            if (topInstructionText_EN != null) 
            {
                topInstructionText_EN.gameObject.SetActive(true);
                topInstructionText_EN.text = topInstOutro_EN;
            }
        }

        if (coreQuizScript != null) coreQuizScript.OnExperimentComplete();

        // Submit final analytics session for Lesson 1
        if (CircuitAnalyticsManager.Instance != null)
            CircuitAnalyticsManager.Instance.SubmitSession();
    }

    // --- Retry / Reset Experiment ---
    // Cleans up spawned batteries, resets bulb position/light back to 1 battery, and restarts prompt
    public void ResetExperiment()
    {
        if (CircuitAnalyticsManager.Instance != null)
            CircuitAnalyticsManager.Instance.RecordRetry();

        // Delete all extra batteries that were added
        foreach (GameObject bat in spawnedBatteries)
        {
            if (bat != null) Destroy(bat);
        }
        spawnedBatteries.Clear();

        // Restore original positions of bulb model & light intensity
        if (currentBatteries > 1)
        {
            float currentScaleZ = firstBatteryLocation.lossyScale.z;
            float zoomRatio = (initialScaleZ > 0) ? (currentScaleZ / initialScaleZ) : 1f;
            float dynamicOffset = stackHeightOffset * zoomRatio;
            float totalOffsetToRemove = dynamicOffset * (currentBatteries - 1);
            
            if (batteryPlusNode != null)
                batteryPlusNode.position -= (firstBatteryLocation.forward * totalOffsetToRemove);
                
            if (bulbModel != null)
                bulbModel.position -= (firstBatteryLocation.forward * totalOffsetToRemove);
                
            if (bulbLight != null)
            {
                bulbLight.intensity -= (lightIntensityIncrease * (currentBatteries - 1));
                bulbLight.range -= (lightRangeIncrease * (currentBatteries - 1));
            }
        }

        currentBatteries = 1;

        if (addBatteryButton != null) addBatteryButton.SetActive(true);

        PlayIntroVoiceover();
    }

    // --- Audio & Subtitle Helper ---
    // Plays narration via StoryUIManager (or fallback local AudioSource) in English or Sinhala
    private void PlayAudioAndText(AudioClip clipEN, AudioClip clipSN, string textEN, string textSN)
    {
        if (storyManager != null)
        {
            storyManager.gameObject.SetActive(true);
            storyManager.PlayVoiceover(clipEN, clipSN, textEN, textSN);
        }
        else if (localAudioSource != null)
        {
            string currentLang = PlayerPrefs.GetString("Language", "English");
            AudioClip clipToPlay = (currentLang == "Sinhala") ? clipSN : clipEN;
            if (clipToPlay != null)
            {
                localAudioSource.Stop();
                localAudioSource.PlayOneShot(clipToPlay);
            }
        }
    }
}