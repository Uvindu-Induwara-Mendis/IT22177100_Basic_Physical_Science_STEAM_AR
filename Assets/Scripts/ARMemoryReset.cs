using UnityEngine;
using Vuforia; 

public class ARMemoryReset : MonoBehaviour
{
    [Header("Vuforia Target to Reset")]
    [Tooltip("Drag your new ImageTarget in here to flush its memory!")]
    public ObserverBehaviour mainImageTarget;

    [Header("Old Elements to Hide")]
    public GameObject currentStoryPanel; 
    
    [Tooltip("Drag ALL old lesson folders, UI canvases, or loose models here to hide them!")]
    public GameObject[] oldElementsToTurnOff; // --- UPDATED TO A LIST! ---

    [Header("New Lesson to Show")]
    public GameObject nextLessonFolder; 

    public void TriggerFreshScan()
    {
        // 1. Hide the story panel
        if (currentStoryPanel != null) currentStoryPanel.SetActive(false);

        // 2. Loop through everything in your list and shut it off!
        if (oldElementsToTurnOff != null)
        {
            foreach (GameObject item in oldElementsToTurnOff)
            {
                if (item != null) item.SetActive(false);
            }
        }

        // 3. Wipe Vuforia's Memory
        if (mainImageTarget != null)
        {
            mainImageTarget.enabled = false;
            mainImageTarget.enabled = true;
        }

        // 4. Turn on the new lesson! 
        if (nextLessonFolder != null) nextLessonFolder.SetActive(true);
    }
}