using UnityEngine;
using Vuforia; // We need this to talk to the AR Camera

public class CameraPauseManager : MonoBehaviour
{
    // This runs the exact millisecond this 2D Panel turns ON
    void OnEnable()
    {
        if (VuforiaBehaviour.Instance != null)
        {
            // Close Vuforia's eyes (Pauses the camera and tracking)
            VuforiaBehaviour.Instance.enabled = false;
        }
    }

    // This runs the exact millisecond this 2D Panel turns OFF
    void OnDisable()
    {
        if (VuforiaBehaviour.Instance != null)
        {
            // Open Vuforia's eyes (Resumes the camera and tracking)
            VuforiaBehaviour.Instance.enabled = true;
        }
    }
}