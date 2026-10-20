using UnityEngine;
using UnityEngine.EventSystems;

public class ElectromagnetDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item Settings")]
    public bool isCorrectPart;
    // CHANGED: Renamed to itemType so it works for everything!
    public string itemType; 
    public float blueprintScale = 1.33f;
    public bool renderOnTop = false; 
    public GameObject itemLabel; 

    [Header("Connections")]
    public ElectromagnetQuizManager quizManager;
    public RectTransform assemblyArea; 
    public Transform snapTarget;       

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 startingPosition;
    private bool isLocked = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        TriggerSubtleGrabHaptic();

        startingPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0.8f; 
        canvasGroup.blocksRaycasts = false; 
        
        rectTransform.localScale = new Vector3(blueprintScale, blueprintScale, 1f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked) return;
        rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (RectTransformUtility.RectangleContainsScreenPoint(assemblyArea, Input.mousePosition, eventData.pressEventCamera))
        {
            if (isCorrectPart)
            {
                rectTransform.SetParent(assemblyArea);
                rectTransform.position = snapTarget.position;
                rectTransform.localScale = new Vector3(blueprintScale, blueprintScale, 1f);
                
                if (renderOnTop)
                {
                    rectTransform.SetAsLastSibling(); 
                }
                else
                {
                    rectTransform.SetAsFirstSibling(); 
                }
                
                if (itemLabel != null) 
                {
                    itemLabel.SetActive(false);
                }
                
                isLocked = true;
                // CHANGED: Now passes the itemType to the manager!
                quizManager.OnCorrectPartPlaced(itemType);
            }
            else
            {
                #if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate(); 
                #endif

                rectTransform.anchoredPosition = startingPosition;
                rectTransform.localScale = Vector3.one; 
                // CHANGED: Updated to use the new variable name
                quizManager.OnTrickPartUsed(itemType);
            }
        }
        else
        {
            rectTransform.anchoredPosition = startingPosition;
            rectTransform.localScale = Vector3.one; 
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
}