using UnityEngine;
using Vuforia;

// =========================================================================================
// UTILITY: AR LESSON TRANSITION & VUFORIA MEMORY FLUSH
// =========================================================================================
// Context: Used when transitioning between lessons or resetting an AR activity.
// - Hides all active objects/canvases from the previous lesson
// - Temporarily disables and re-enables the Vuforia ObserverBehaviour to flush tracking cache
// - Activates the next lesson container for a clean scanning experience.
public class ARMemoryReset : MonoBehaviour
{
    // --- Target Observer & Cleanup Lists ---
    [Header("Vuforia Target to Reset")]
    [Tooltip("ImageTarget observer toggled to force a fresh scan")]
    public ObserverBehaviour mainImageTarget;

    [Header("Old Elements to Hide")]
    public GameObject currentStoryPanel; 
    
    [Tooltip("All previous lesson folders, canvases, and models to hide")]
    public GameObject[] oldElementsToTurnOff;

    [Header("New Lesson to Show")]
    public GameObject nextLessonFolder; 

    // --- Action: Flushes Memory & Launches Next Lesson ---
    public void TriggerFreshScan()
    {
        // 1. Hide the story narrative panel
        if (currentStoryPanel != null) currentStoryPanel.SetActive(false);

        // 2. Shut off all old lesson folders and UI elements
        if (oldElementsToTurnOff != null)
        {
            foreach (GameObject item in oldElementsToTurnOff)
            {
                if (item != null) item.SetActive(false);
            }
        }

        // 3. Flush Vuforia's tracking memory
        if (mainImageTarget != null)
        {
            mainImageTarget.enabled = false;
            mainImageTarget.enabled = true;
        }

        // 4. Activate new lesson container
        if (nextLessonFolder != null) nextLessonFolder.SetActive(true);
    }
}