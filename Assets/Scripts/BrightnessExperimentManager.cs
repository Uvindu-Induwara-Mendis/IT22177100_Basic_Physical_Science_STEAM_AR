using UnityEngine;
using TMPro; 

public class BrightnessExperimentManager : MonoBehaviour
{
    [Header("Managers")]
    public StoryUIManager storyManager;
    public CircuitCoreQuiz coreQuizScript;

    [Header("UI Connections")]
    public GameObject addBatteryButton;
    public GameObject retryButton; // --- NEW: Toggle the retry button ---
    public TextMeshProUGUI topInstructionText_EN; 
    public TextMeshProUGUI topInstructionText_SN; 
    
    [Header("Sound Effects")]
    public AudioClip batteryStackSFX;

    [Header("3D Models & Physics")]
    public GameObject batteryPrefab; 
    public Transform firstBatteryLocation; 
    public Transform batteryPlusNode; 
    public Light bulbLight; 
    
    public Transform bulbModel; 

    [Header("Stacking Settings")]
    public float stackHeightOffset = 0.035f; 
    public float lightIntensityIncrease = 5.0f; 
    public float lightRangeIncrease = 1.0f;

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

    // --- NEW: Moved Top Instructions here so you can use your FM Converter! ---
    [Header("Top Instructions")]
    public string topInstIntro_EN = "Tap the 'Add Battery' button to see what happens!";
    [Tooltip("Paste your FM-Abhaya converted text here!")]
    public string topInstIntro_SN = "negß tl;= lrk fnd;a;u Tn,d fudlo fjkafka lsh,d n,uq"; 
    
    public string topInstOutro_EN = "Experiment Complete! Read the explanation below.";
    [Tooltip("Paste your FM-Abhaya converted text here!")]
    public string topInstOutro_SN = "mÍlaIKh bjrhs"; 

    private int currentBatteries = 1;
    public int maxBatteries = 3;
    
    // --- NEW: Track spawned batteries for the Retry button ---
    private System.Collections.Generic.List<GameObject> spawnedBatteries = new System.Collections.Generic.List<GameObject>();
    
    private float initialScaleZ = 1f; 
    private AudioSource localAudioSource; // NEW: Local audio source for when story manager is hidden

    private void Awake()
    {
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

    public void PlayIntroVoiceover()
    {
        // ── Analytics: battery experiment (AR) begins, resume AR timer ──
        if (CircuitAnalyticsManager.Instance != null)
            CircuitAnalyticsManager.Instance.StartARTimer();

        PlayAudioAndText(introVoice_EN, introVoice_SN, introText_EN, introText_SN);

        // Turn ON the retry button when the AR Part 2 starts!
        if (retryButton != null) retryButton.SetActive(true);

        // --- NEW: Language Toggle Logic ---
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

    public void OnAddBatteryClicked()
    {
        HapticFeedbackManager.TriggerSubtleHaptic(40L);
        if (currentBatteries < maxBatteries)
        {
            float currentScaleZ = firstBatteryLocation.lossyScale.z;
            float zoomRatio = (initialScaleZ > 0) ? (currentScaleZ / initialScaleZ) : 1f;
            float dynamicOffset = stackHeightOffset * zoomRatio;

            Vector3 localUpwardOffset = firstBatteryLocation.forward * (dynamicOffset * currentBatteries);
            Vector3 newPosition = firstBatteryLocation.position + localUpwardOffset;
            
            GameObject newBattery = Instantiate(batteryPrefab, newPosition, firstBatteryLocation.rotation, firstBatteryLocation.parent);
            newBattery.transform.localScale = firstBatteryLocation.localScale;
            
            // Track it so we can delete it on Retry
            spawnedBatteries.Add(newBattery);

            if (batteryStackSFX != null && Camera.main != null)
            {
                AudioSource.PlayClipAtPoint(batteryStackSFX, Camera.main.transform.position);
            }

            if (batteryPlusNode != null)
            {
                batteryPlusNode.position += (firstBatteryLocation.forward * dynamicOffset);
            }

            if (bulbModel != null)
            {
                bulbModel.position += (firstBatteryLocation.forward * dynamicOffset);
            }

            if (bulbLight != null)
            {
                bulbLight.intensity += lightIntensityIncrease;
                bulbLight.range += lightRangeIncrease;
            }

            currentBatteries++;

            if (currentBatteries >= maxBatteries) FinishExperiment();
        }
    }

    private void FinishExperiment()
    {
        Handheld.Vibrate();

        if (addBatteryButton != null) addBatteryButton.SetActive(false);
        if (retryButton != null) retryButton.SetActive(false); // Hide the retry button when finished
        PlayAudioAndText(successVoice_EN, successVoice_SN, successText_EN, successText_SN);

        // --- NEW: Language Toggle Logic for Outro ---
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

        // ── Analytics: entire lesson complete, submit session data ──
        if (CircuitAnalyticsManager.Instance != null)
            CircuitAnalyticsManager.Instance.SubmitSession();
    }

    // --- NEW: Contextual Retry Method for the AR Part ---
    public void ResetExperiment()
    {
        // 1. Analytics: Record that the user retried this AR section
        if (CircuitAnalyticsManager.Instance != null)
            CircuitAnalyticsManager.Instance.RecordRetry();

        // 2. Destroy all spawned batteries
        foreach (GameObject bat in spawnedBatteries)
        {
            if (bat != null) Destroy(bat);
        }
        spawnedBatteries.Clear();

        // 3. Reset positions and lights back to 1 battery worth
        if (currentBatteries > 1)
        {
            float currentScaleZ = firstBatteryLocation.lossyScale.z;
            float zoomRatio = (initialScaleZ > 0) ? (currentScaleZ / initialScaleZ) : 1f;
            float dynamicOffset = stackHeightOffset * zoomRatio;
            
            // Subtract the offset * (currentBatteries - 1)
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

        // 4. Reset state variables
        currentBatteries = 1;

        // 5. Reset UI (turn the Add Battery button back on)
        if (addBatteryButton != null) addBatteryButton.SetActive(true);

        // 6. Play the Intro voiceover again to prompt them
        PlayIntroVoiceover();
    }

    private void PlayAudioAndText(AudioClip clipEN, AudioClip clipSN, string textEN, string textSN)
    {
        if (storyManager != null)
        {
            storyManager.gameObject.SetActive(true); // RE-ENABLE THE SUBTITLE PANEL!
            storyManager.PlayVoiceover(clipEN, clipSN, textEN, textSN);
        }
        else
        {
            if (localAudioSource != null)
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
}