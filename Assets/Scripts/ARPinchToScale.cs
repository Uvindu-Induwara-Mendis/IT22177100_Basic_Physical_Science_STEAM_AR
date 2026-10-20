using UnityEngine;

public class ARPinchToScale : MonoBehaviour
{
    [Header("Scaling Settings")]
    [Tooltip("Drag your AR_Content_Wrapper here!")]
    public Transform objectToScale; 
    
    [Tooltip("How fast it zooms in and out")]
    public float scaleSpeed = 0.005f; 
    
    [Tooltip("The smallest it can get (e.g., 0.8 = 80% of original)")]
    public float minScale = 0.8f; 
    
    [Tooltip("The largest it can get (e.g., 1.4 = 140% of original)")]
    public float maxScale = 1.4f;

    private Vector3 initialScale;

    void Start()
    {
        if (objectToScale != null)
        {
            // Remember the starting size so we can clamp it safely
            initialScale = objectToScale.localScale;
        }
    }

    void Update()
    {
        // Check if there are exactly 2 fingers on the screen
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position of those fingers in the previous frame
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the distance between the fingers in the previous frame and current frame
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference (did they pinch in or pull out?)
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // Apply the scale
            if (objectToScale != null)
            {
                // Calculate the new scale based on finger movement
                Vector3 newScale = objectToScale.localScale - (Vector3.one * deltaMagnitudeDiff * scaleSpeed);

                // Clamp it so kids can't make the battery the size of a skyscraper or infinitely small
                newScale.x = Mathf.Clamp(newScale.x, initialScale.x * minScale, initialScale.x * maxScale);
                newScale.y = Mathf.Clamp(newScale.y, initialScale.y * minScale, initialScale.y * maxScale);
                newScale.z = Mathf.Clamp(newScale.z, initialScale.z * minScale, initialScale.z * maxScale);

                // Apply it to the wrapper!
                objectToScale.localScale = newScale;
            }
        }
    }
}