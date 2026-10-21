using UnityEngine;
using System.Collections;
using TMPro;

// =========================================================================================
// LESSON 1 (ELECTRIC CIRCUITS): PHASE 1 - BATTERY & BULB AR WIRE CONNECTING SCENE
// =========================================================================================
// Context: First AR interactive scene in Lesson 1. The student points the camera at the storybook
// marker and taps battery terminals (+ / -) and bulb terminals (side / bottom).
// The script draws flat curved wires on the desk surface using LineRenderers. Once a valid complete
// circuit is wired, the bulb turns on, success audio plays, and the 'Take Quiz' button unlocks.
public class CircuitQuizManager : MonoBehaviour
{
    // --- Managers & State Connections ---
    [Header("Connections")]
    public StoryUIManager storyManager;      // Displays storybook dialogue & subtitles
    public UIManager uiManager;              // Coordinates scene transitions (quiz pass/fail)
    public CircuitCoreQuiz coreQuizScript;   // 2D Quiz manager (prevents audio overlaps)
    
    // --- Phase 2 Safety Lock ---
    // When Lesson 1 moves to the Battery Stacking phase, this lock prevents new taps
    // while keeping drawn wires glued between the battery and bulb!
    [Header("Phase 2 Safety")]
    [Tooltip("Check this via script or inspector to mute this script but keep wires glued!")]
    public bool isPhase2Mode = false; 

    // --- 3D Circuit Models & Visuals ---
    [Header("Circuit Models")]
    public LineRenderer wire1;               // First connecting wire
    public LineRenderer wire2;               // Second connecting wire
    public GameObject successLight;          // Light effect that turns ON when circuit is closed
    
    // --- Local Quiz & Transition UI ---
    [Header("Local Quiz UI")]
    public GameObject failTextPanel;         // Shown if invalid connections are made
    public GameObject revealStoryButton; 
    public GameObject takeQuizButton;        // Unlocked when circuit is successfully wired
    
    // --- Audio & Sound Effects ---
    [Header("Sound Effects")]
    public AudioClip wireConnectSFX;         // SFX played whenever a wire connects two nodes

    [Header("English Voice & Dialogue")]
    public AudioClip introVoice;
    public string introDialogue_EN = "Tap the Battery, then the Bulb to connect the wires!";
    public AudioClip successVoice;
    public string successDialogue_EN = "Great job! The circuit is complete and the light is on!";
    public AudioClip failVoice;
    public string failDialogue_EN = "Oops! Try checking your connections again.";

    [Header("Sinhala Voice & Dialogue")]
    public AudioClip introVoice_SN;
    public string introDialogue_SN = "hd¿fõ\" wfma mßm:h..."; 
    public AudioClip successVoice_SN;
    public string successDialogue_SN = "yßu fYdala! ඔhd...";    
    public AudioClip failVoice_SN;
    public string failDialogue_SN = "අපොයි! නැවත උත්සාහ කරන්න..."; 

    // --- Wire Visual Appearance (Curved Bezier Arc on Desk) ---
    [Header("Wire Look (Desk Arc)")]
    public int wireSmoothness = 15;          // Number of subdivisions along the curve
    public float wireArcCurve = 0.5f;        // How wide the wire curves out sideways
    public float wireThickness = 0.05f;      // Thickness of the rendered wire

    // --- Terminal Selection & Wiring State ---
    private GameObject firstTappedTerminal = null; // Stores first terminal tapped in a pair
    private bool isPlusWired = false;
    private bool isMinusWired = false;
    private bool isSideWired = false;
    private bool isBottomWired = false;
    private int wiresDrawn = 0; 

    // Terminal endpoints stored to keep wires anchored each frame
    private GameObject w1NodeA, w1NodeB;
    private GameObject w2NodeA, w2NodeB;

    private AudioSource localAudioSource;

    private void Awake()
    {
        localAudioSource = gameObject.AddComponent<AudioSource>();
        localAudioSource.playOnAwake = false;
    }

    void Start()
    {
        if (wire1 != null) wire1.positionCount = 0;
        if (wire2 != null) wire2.positionCount = 0;
        
        if (revealStoryButton != null) revealStoryButton.SetActive(false);
        if (takeQuizButton != null) takeQuizButton.SetActive(false);
    }

    // --- Mutes Input During Phase 2 ---
    // Locks terminal tapping during Battery Stacking experiment while keeping drawn wires visible
    public void EnablePhase2Mode()
    {
        isPhase2Mode = true;
        if (failTextPanel != null) failTextPanel.SetActive(false);
        if (revealStoryButton != null) revealStoryButton.SetActive(false);
        if (takeQuizButton != null) takeQuizButton.SetActive(false);
    }

    // --- Intro Voiceover Triggered by Image Target Tracking ---
    public void TriggerIntro()
    {
        if (isPhase2Mode) return; 
        if (coreQuizScript != null && coreQuizScript.isQuizActive == true) return; 

        PlayAudioAndText(introVoice, introVoice_SN, introDialogue_EN, introDialogue_SN);

        // Start tracking time spent by student in AR wiring phase
        if (CircuitAnalyticsManager.Instance != null)
            CircuitAnalyticsManager.Instance.StartARTimer();
    }

    void Update()
    {
        // 1. Redraw both wires each frame so they stay glued to moving AR markers
        if (wiresDrawn > 0 && wire1 != null && w1NodeA != null && w1NodeB != null)
            DrawFlatDeskWire(wire1, w1NodeA.transform.position, w1NodeB.transform.position, 1f);
        
        if (wiresDrawn > 1 && wire2 != null && w2NodeA != null && w2NodeB != null)
            DrawFlatDeskWire(wire2, w2NodeA.transform.position, w2NodeB.transform.position, -1f);

        // 2. Raycast touch/mouse tap to pick terminals (Battery +/- and Bulb Side/Bottom)
        if (!isPhase2Mode)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Camera.main == null) return; 

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    GameObject tappedObj = hit.collider.gameObject;
                    string tag = tappedObj.tag;

                    // Check if tapped object is one of the 4 valid circuit terminals
                    if (tag == "BatteryPlus" || tag == "BatteryMinus" || tag == "BulbSide" || tag == "BulbBottom")
                    {
                        if (firstTappedTerminal == null)
                        {
                            // First terminal selected (e.g., BatteryPlus)
                            firstTappedTerminal = tappedObj;
                            HapticFeedbackManager.TriggerSubtleHaptic(40L);
                        }
                        else
                        {
                            // Second terminal selected: attempt to connect wire
                            HapticFeedbackManager.TriggerSubtleHaptic(40L);
                            AttemptConnection(firstTappedTerminal, tappedObj);
                            firstTappedTerminal = null; 
                        }
                    }
                }
            }
        }
    }

    // --- Connects Two Tapped Terminals ---
    // Validates that one terminal is on the battery and the other is on the bulb
    void AttemptConnection(GameObject nodeA, GameObject nodeB)
    {
        if (wiresDrawn >= 2) return; 
        if (nodeA == nodeB) return; 

        string tagA = nodeA.tag;
        string tagB = nodeB.tag;

        // Disallow connecting battery-to-battery or bulb-to-bulb
        if (tagA.Contains("Battery") == tagB.Contains("Battery")) return; 

        if (failTextPanel != null) failTextPanel.SetActive(false); 

        // Assign endpoints for the active wire
        if (wiresDrawn == 0)
        {
            w1NodeA = nodeA;
            w1NodeB = nodeB;
        }
        else
        {
            w2NodeA = nodeA;
            w2NodeB = nodeB;
        }

        RecordConnection(tagA);
        RecordConnection(tagB);
        wiresDrawn++;

        if (wireConnectSFX != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(wireConnectSFX, Camera.main.transform.position);
        }

        // Once both wires are drawn, check if circuit is scientifically valid
        if (wiresDrawn == 2) ValidateCircuit();
    }

    // --- Draws Curved Bezier Wire Along Desk Surface ---
    void DrawFlatDeskWire(LineRenderer wire, Vector3 start, Vector3 end, float arcDirection)
    {
        wire.positionCount = wireSmoothness + 1;
        wire.startWidth = wireThickness;
        wire.endWidth = wireThickness;

        Vector3 midPoint = Vector3.Lerp(start, end, 0.5f);
        float distance = Vector3.Distance(start, end);
        Vector3 direction = (end - start).normalized;
        Vector3 deskNormal = Vector3.Cross(direction, Vector3.up).normalized;
        Vector3 controlPoint = midPoint + (deskNormal * distance * wireArcCurve * arcDirection);

        for (int i = 0; i <= wireSmoothness; i++)
        {
            float t = i / (float)wireSmoothness;
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            Vector3 position = (uu * start) + (2 * u * t * controlPoint) + (tt * end);
            wire.SetPosition(i, position);
        }
    }

    // Records which terminals have a wire attached
    void RecordConnection(string tag)
    {
        if (tag == "BatteryPlus") isPlusWired = true;
        if (tag == "BatteryMinus") isMinusWired = true;
        if (tag == "BulbSide") isSideWired = true;
        if (tag == "BulbBottom") isBottomWired = true;
    }

    // --- Validates Complete Circuit ---
    // A complete circuit requires Battery+, Battery-, BulbSide, and BulbBottom to all be wired
    public void ValidateCircuit()
    {
        if (isPlusWired && isMinusWired && isSideWired && isBottomWired && wiresDrawn == 2)
        {
            Handheld.Vibrate();
            
            // Turn on bulb light and show success prompt
            if (successLight != null) successLight.SetActive(true);
            PlayAudioAndText(successVoice, successVoice_SN, successDialogue_EN, successDialogue_SN);
            if (takeQuizButton != null) takeQuizButton.SetActive(true);

            // Stop AR timer on successful wiring
            if (CircuitAnalyticsManager.Instance != null)
                CircuitAnalyticsManager.Instance.StopARTimer();
        }
        else
        {
            // Invalid wiring: play error prompt and reset
            PlayAudioAndText(failVoice, failVoice_SN, failDialogue_EN, failDialogue_SN);
            if (uiManager != null) uiManager.OnQuizFail();
            ResetQuiz(); 
        }
    }

    public void TransitionToStoryReveal()
    {
        if (storyManager != null) storyManager.ClearText();
        if (revealStoryButton != null) revealStoryButton.SetActive(false);
        if (uiManager != null) uiManager.OnQuizPass();
    }

    // --- Resets Wiring State ---
    // Clears LineRenderers, resets flags, and logs retry analytics
    public void ResetQuiz()
    {
        isPhase2Mode = false;
        isPlusWired = false;
        isMinusWired = false;
        isSideWired = false;
        isBottomWired = false;
        wiresDrawn = 0;
        firstTappedTerminal = null;
        
        w1NodeA = null; w1NodeB = null;
        w2NodeA = null; w2NodeB = null;

        if (wire1 != null) wire1.positionCount = 0;
        if (wire2 != null) wire2.positionCount = 0;
        if (successLight != null) successLight.SetActive(false);
        if (failTextPanel != null) failTextPanel.SetActive(false);
        
        if (revealStoryButton != null) revealStoryButton.SetActive(false);
        if (takeQuizButton != null) takeQuizButton.SetActive(false);

        if (CircuitAnalyticsManager.Instance != null)
            CircuitAnalyticsManager.Instance.RecordRetry();
    }

    // --- Dialogue & Audio Helper ---
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