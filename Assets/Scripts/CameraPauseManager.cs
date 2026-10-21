using UnityEngine;
using Vuforia;

// =========================================================================================
// UTILITY: 2D PANEL AR CAMERA & TRACKING PAUSE CONTROLLER
// =========================================================================================
// Context: Attached to 2D UI overlay panels (e.g. Quizzes, Story Dialogue, Options).
// Disables Vuforia camera tracking while the 2D panel is open to save battery and prevent
// accidental background AR marker detection. Re-enables Vuforia tracking when panel closes.
public class CameraPauseManager : MonoBehaviour
{
    // Pauses Vuforia camera and tracking when 2D panel opens
    void OnEnable()
    {
        if (VuforiaBehaviour.Instance != null)
        {
            VuforiaBehaviour.Instance.enabled = false;
        }
    }

    // Resumes Vuforia camera and tracking when 2D panel closes
    void OnDisable()
    {
        if (VuforiaBehaviour.Instance != null)
        {
            VuforiaBehaviour.Instance.enabled = true;
        }
    }
}