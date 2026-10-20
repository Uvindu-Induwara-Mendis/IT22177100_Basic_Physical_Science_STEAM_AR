using UnityEngine;
using System.Collections;
using TMPro;

public class CircuitQuizManager : MonoBehaviour
{
    [Header("Connections")]
    public StoryUIManager storyManager; 
    public UIManager uiManager; 
    
    [Header("State Management")]
    public CircuitCoreQuiz coreQuizScript; 
    
    // --- NEW: THE PHASE 2 SAFETY LOCK ---
    [Header("Phase 2 Safety")]
    [Tooltip("Check this via script or inspector to mute this script but keep wires glued!")]
    public bool isPhase2Mode = false; 

    [Header("Circuit Models")]
    public LineRenderer wire1; 
    public LineRenderer wire2;
    public GameObject successLight; 
    
    [Header("Local Quiz UI")]
    public GameObject failTextPanel; 
    public GameObject revealStoryButton; 
    public GameObject takeQuizButton;    
    
    [Header("Sound Effects")]
    public AudioClip wireConnectSFX;

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

    [Header("Wire Look (Desk Arc)")]
    public int wireSmoothness = 15; 
    public float wireArcCurve = 0.5f; 
    public float wireThickness = 0.05f; 

    private GameObject firstTappedTerminal = null;
    
    private bool isPlusWired = false;
    private bool isMinusWired = false;
    private bool isSideWired = false;
    private bool isBottomWired = false;

    private int wiresDrawn = 0; 

    private GameObject w1NodeA, w1NodeB;
    private GameObject w2NodeA, w2NodeB;

    private AudioSource localAudioSource; // NEW: Local audio source for when story manager is hidden

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

    // --- NEW: Call this from your button to mute the script! ---
    public void EnablePhase2Mode()
    {
        isPhase2Mode = true;
        
        // Force hide any leftover UI just to be safe
        if (failTextPanel != null) failTextPanel.SetActive(false);
        if (revealStoryButton != null) revealStoryButton.SetActive(false);
        if (takeQuizButton != null) takeQuizButton.SetActive(false);
    }

    public void TriggerIntro()
    {
        // SAFETY LOCK: If in Phase 2, stay completely quiet!
        if (isPhase2Mode) return; 

        if (coreQuizScript != null && coreQuizScript.isQuizActive == true) return; 
        PlayAudioAndText(introVoice, introVoice_SN, introDialogue_EN, introDialogue_SN);

        // ── Analytics: AR marker detected — start tracking AR time NOW ──
        if (CircuitAnalyticsManager.Instance != null)
            CircuitAnalyticsManager.Instance.StartARTimer();
    }

    void Update()
    {
        // 1. ALWAYS DRAW THE WIRES (The Glue stays active even in Phase 2!)
        if (wiresDrawn > 0 && wire1 != null && w1NodeA != null && w1NodeB != null)
            DrawFlatDeskWire(wire1, w1NodeA.transform.position, w1NodeB.transform.position, 1f);
        
        if (wiresDrawn > 1 && wire2 != null && w2NodeA != null && w2NodeB != null)
            DrawFlatDeskWire(wire2, w2NodeA.transform.position, w2NodeB.transform.position, -1f);

        // 2. ONLY ALLOW TAPPING IF NOT IN PHASE 2
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

                    if (tag == "BatteryPlus" || tag == "BatteryMinus" || tag == "BulbSide" || tag == "BulbBottom")
                    {
                        if (firstTappedTerminal == null)
                        {
                            firstTappedTerminal = tappedObj;
                            HapticFeedbackManager.TriggerSubtleHaptic(40L);
                        }
                        else
                        {
                            HapticFeedbackManager.TriggerSubtleHaptic(40L);
                            AttemptConnection(firstTappedTerminal, tappedObj);
                            firstTappedTerminal = null; 
                        }
                    }
                }
            }
        }
    }

    void AttemptConnection(GameObject nodeA, GameObject nodeB)
    {
        if (wiresDrawn >= 2) return; 
        if (nodeA == nodeB) return; 

        string tagA = nodeA.tag;
        string tagB = nodeB.tag;

        if (tagA.Contains("Battery") == tagB.Contains("Battery")) return; 

        if (failTextPanel != null) failTextPanel.SetActive(false); 

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

        if (wiresDrawn == 2) ValidateCircuit();
    }

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

    void RecordConnection(string tag)
    {
        if (tag == "BatteryPlus") isPlusWired = true;
        if (tag == "BatteryMinus") isMinusWired = true;
        if (tag == "BulbSide") isSideWired = true;
        if (tag == "BulbBottom") isBottomWired = true;
    }

    public void ValidateCircuit()
    {
        if (isPlusWired && isMinusWired && isSideWired && isBottomWired && wiresDrawn == 2)
        {
            // --- HAPTIC VIBRATION ADDED HERE ---
            Handheld.Vibrate();
            
            if(successLight != null) successLight.SetActive(true);
            PlayAudioAndText(successVoice, successVoice_SN, successDialogue_EN, successDialogue_SN);
            if (takeQuizButton != null) takeQuizButton.SetActive(true);

            // ── Analytics: wiring complete, stop AR timer ──
            if (CircuitAnalyticsManager.Instance != null)
                CircuitAnalyticsManager.Instance.StopARTimer();
        }
        else
        {
            PlayAudioAndText(failVoice, failVoice_SN, failDialogue_EN, failDialogue_SN);
            if(uiManager != null) uiManager.OnQuizFail();
            ResetQuiz(); 
        }
    }

    public void TransitionToStoryReveal()
    {
        if (storyManager != null) storyManager.ClearText();
        if (revealStoryButton != null) revealStoryButton.SetActive(false);
        if (uiManager != null) uiManager.OnQuizPass();
    }

    public void ResetQuiz()
    {
        isPhase2Mode = false; // --- NEW: Reset safety lock when completely resetting ---
        
        isPlusWired = false;
        isMinusWired = false;
        isSideWired = false;
        isBottomWired = false;
        wiresDrawn = 0;
        firstTappedTerminal = null;
        
        w1NodeA = null; w1NodeB = null;
        w2NodeA = null; w2NodeB = null;

        if(wire1 != null) wire1.positionCount = 0;
        if(wire2 != null) wire2.positionCount = 0;
        if(successLight != null) successLight.SetActive(false);
        if(failTextPanel != null) failTextPanel.SetActive(false);
        
        if(revealStoryButton != null) revealStoryButton.SetActive(false);
        if(takeQuizButton != null) takeQuizButton.SetActive(false);

        // ── Analytics: any quiz reset = a retry (covers all reset paths,
        //    including direct retry buttons wired in the Inspector) ──
        if (CircuitAnalyticsManager.Instance != null)
            CircuitAnalyticsManager.Instance.RecordRetry();
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