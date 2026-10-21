using UnityEngine;

// =========================================================================================
// UTILITY: 2-FINGER TOUCH PINCH-TO-SCALE FOR AR 3D MODELS
// =========================================================================================
// Context: Attached to AR model root wrappers across lessons (e.g. circuits, electromagnet, pulley).
// Measures delta distance between two fingers on touchscreen to smoothly zoom/scale 3D models.
// Clamps scale within minScale and maxScale so the model remains at a comfortable viewing size.
public class ARPinchToScale : MonoBehaviour
{
    // --- Scale Settings & Clamping ---
    [Header("Scaling Settings")]
    [Tooltip("Transform of the AR model wrapper to scale")]
    public Transform objectToScale; 
    
    [Tooltip("Zoom speed multiplier per touch delta")]
    public float scaleSpeed = 0.005f; 
    
    [Tooltip("Minimum scale limit (0.8 = 80% of original)")]
    public float minScale = 0.8f; 
    
    [Tooltip("Maximum scale limit (1.4 = 140% of original)")]
    public float maxScale = 1.4f;

    private Vector3 initialScale;

    void Start()
    {
        if (objectToScale != null)
        {
            initialScale = objectToScale.localScale;
        }
    }

    void Update()
    {
        // Detect 2-finger pinch gesture
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Previous frame positions
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Compute distance delta between fingers
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            if (objectToScale != null)
            {
                // Compute new scale
                Vector3 newScale = objectToScale.localScale - (Vector3.one * deltaMagnitudeDiff * scaleSpeed);

                // Clamp within safe limits
                newScale.x = Mathf.Clamp(newScale.x, initialScale.x * minScale, initialScale.x * maxScale);
                newScale.y = Mathf.Clamp(newScale.y, initialScale.y * minScale, initialScale.y * maxScale);
                newScale.z = Mathf.Clamp(newScale.z, initialScale.z * minScale, initialScale.z * maxScale);

                objectToScale.localScale = newScale;
            }
        }
    }
}