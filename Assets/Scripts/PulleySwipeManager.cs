using UnityEngine;
using UnityEngine.Events;

public class PulleySwipeManager : MonoBehaviour
{
    [Header("UI Affordance")]
    [Tooltip("Drag your animated SwipeIndicatorArrow UI image here!")]
    public GameObject swipeArrowUI;

    [Header("Swipe Settings")]
    [Tooltip("How far the finger must drag to count as a swipe")]
    public float minSwipeDistance = 50f; 

    [Header("What happens on swipe?")]
    public UnityEvent OnSwipeDown;

    private Vector2 touchStartPos;
    private Vector2 touchEndPos;

    void Update()
    {
        // Only look for single finger touches
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

    void CheckSwipe()
    {
        // Calculate vertical distance (start Y minus end Y is positive if swiping DOWN)
        float verticalDistance = touchStartPos.y - touchEndPos.y; 
        float horizontalDistance = Mathf.Abs(touchStartPos.x - touchEndPos.x);

        // Make sure it's a downward swipe, it's long enough, and it's mostly vertical (not horizontal)
        if (verticalDistance > minSwipeDistance && verticalDistance > horizontalDistance)
        {
            // 1. Hide the helpful arrow since the child figured it out!
            if (swipeArrowUI != null)
            {
                swipeArrowUI.SetActive(false);
            }

            // 2. Vibrate the phone!
            Handheld.Vibrate();

            // 3. Trigger the Pulley Lift function!
            OnSwipeDown.Invoke();
        }
    }
}