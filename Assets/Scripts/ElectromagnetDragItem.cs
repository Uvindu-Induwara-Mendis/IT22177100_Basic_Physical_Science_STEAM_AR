using UnityEngine;
using UnityEngine.EventSystems;

// =========================================================================================
// LESSON 2 (ELECTROMAGNETS): 2D BLUEPRINT ASSEMBLY - DRAGGABLE COMPONENT
// =========================================================================================
// Context: Attached to UI items on the parts shelf during the 2D Electromagnet assembly quiz.
// Students drag parts (iron nail, insulated copper wire, dry cell battery, switch, or trick items)
// into the central blueprint assembly area. Correct parts snap into place on the diagram;
// trick parts trigger error feedback and bounce back to the shelf.
public class ElectromagnetDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // --- Part Configuration ---
    [Header("Item Settings")]
    public bool isCorrectPart;               // True if required for electromagnet (Nail, Wire, Battery)
    public string itemType;                  // Identifier: "Nail", "Wire", "Battery", "Wood", etc.
    public float blueprintScale = 1.33f;     // Scale when snapped onto the blueprint
    public bool renderOnTop = false;         // Controls sibling hierarchy order when snapped
    public GameObject itemLabel;             // Text label (e.g., "Iron Nail") hidden once placed

    // --- References & Snap Target ---
    [Header("Connections")]
    public ElectromagnetQuizManager quizManager; // Notified when item is correctly or mistakenly placed
    public RectTransform assemblyArea;           // Central drop target zone on canvas
    public Transform snapTarget;                 // Exact anchor point where this part locks in

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 startingPosition;
    private bool isLocked = false;               // Once locked into blueprint, cannot be dragged again

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    // --- Drag Started ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        TriggerSubtleGrabHaptic();

        startingPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0.8f; 
        canvasGroup.blocksRaycasts = false; // Allow drop detection on assembly area beneath
        
        rectTransform.localScale = new Vector3(blueprintScale, blueprintScale, 1f);
    }

    // --- Drag In Progress ---
    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked) return;
        rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    // --- Drag Released ---
    // Checks if dropped inside assembly area, then snaps or rejects item
    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Check if cursor/finger is within the blueprint assembly area
        if (RectTransformUtility.RectangleContainsScreenPoint(assemblyArea, Input.mousePosition, eventData.pressEventCamera))
        {
            if (isCorrectPart)
            {
                // Snap into blueprint diagram
                rectTransform.SetParent(assemblyArea);
                rectTransform.position = snapTarget.position;
                rectTransform.localScale = new Vector3(blueprintScale, blueprintScale, 1f);
                
                if (renderOnTop)
                    rectTransform.SetAsLastSibling(); 
                else
                    rectTransform.SetAsFirstSibling(); 
                
                if (itemLabel != null) 
                    itemLabel.SetActive(false);
                
                isLocked = true;
                quizManager.OnCorrectPartPlaced(itemType);
            }
            else
            {
                // Trick item (e.g. wooden stick instead of iron nail)
                #if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate(); 
                #endif

                // Return to original shelf position
                rectTransform.anchoredPosition = startingPosition;
                rectTransform.localScale = Vector3.one; 
                quizManager.OnTrickPartUsed(itemType);
            }
        }
        else
        {
            // Dropped outside assembly area: return to shelf
            rectTransform.anchoredPosition = startingPosition;
            rectTransform.localScale = Vector3.one; 
        }
    }

    // Subtle 40ms vibration on Android when pickup starts
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
}