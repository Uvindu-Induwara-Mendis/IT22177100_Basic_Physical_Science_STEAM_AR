using UnityEngine;
using UnityEngine.Events;

// =========================================================================================
// SIMPLE MACHINES: PULLEY AR SCENE - DOWNWARD SWIPE GESTURE DETECTOR
// =========================================================================================
// Context: Detects when the student swipes downward on the touchscreen to pull the pulley rope.
// Hides the instructional swipe arrow UI affordance, triggers tactile haptic feedback,
// and invokes PulleyManager.LiftCrate() via the OnSwipeDown UnityEvent.
public class PulleySwipeManager : MonoBehaviour
{
    // --- UI Affordance & Swipe Tuning ---
    [Header("UI Affordance")]
    [Tooltip("Animated arrow prompt indicating user should swipe down")]
    public GameObject swipeArrowUI;

    [Header("Swipe Settings")]
    [Tooltip("Minimum pixel distance required to register as an intentional swipe")]
    public float minSwipeDistance = 50f; 

    // --- Action Event (Linked to PulleyManager.LiftCrate) ---
    [Header("What happens on swipe?")]
    public UnityEvent OnSwipeDown;

    private Vector2 touchStartPos;
    private Vector2 touchEndPos;

    void Update()
    {
        // Track single-finger touch gesture
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                touchEndPos = touch.position;
                CheckSwipe();
            }
        }
    }

    // Evaluates touch delta to verify it was a predominantly downward swipe
    void CheckSwipe()
    {
        float verticalDistance = touchStartPos.y - touchEndPos.y; 
        float horizontalDistance = Mathf.Abs(touchStartPos.x - touchEndPos.x);

        // Verify downward direction and vertical alignment
        if (verticalDistance > minSwipeDistance && verticalDistance > horizontalDistance)
        {
            if (swipeArrowUI != null)
            {
                swipeArrowUI.SetActive(false);
            }

            Handheld.Vibrate();
            OnSwipeDown.Invoke();
        }
    }
}