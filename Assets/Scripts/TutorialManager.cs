using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("Connections")]
    public UIManager uiManager; 
    public GameObject tutorialPanel; // The main background panel

    [Header("Tutorial Pages")]
    public GameObject[] slides; // We will drop Slide 1, 2, and 3 in here!
    
    private int currentSlide = 0;

    // Call this when they click "Scan AR" on the Home Menu!
    public void OpenTutorial()
    {
        tutorialPanel.SetActive(true);
        currentSlide = 0;
        UpdateSlides();
    }

    // Call this when they click the "Next" button
    public void NextSlide()
    {
        if (currentSlide < slides.Length - 1)
        {
            currentSlide++;
            UpdateSlides();
        }
        else
        {
            // If they are on the last slide, "Next" acts like a "Start" button!
            FinishTutorial(); 
        }
    }

    // Call this if they click "Skip"
    public void SkipTutorial()
    {
        FinishTutorial();
    }

    private void UpdateSlides()
    {
        // 1. Turn ALL slides off first
        for (int i = 0; i < slides.Length; i++)
        {
            if (slides[i] != null) slides[i].SetActive(false);
        }
        
        // 2. Turn only the current slide back on
        if (slides[currentSlide] != null) slides[currentSlide].SetActive(true);
    }

    private void FinishTutorial()
    {
        // Hide the tutorial
        tutorialPanel.SetActive(false);
        
        // --- IMPORTANT: Tell your UIManager to open the camera now! ---
        if (uiManager != null)
        {
            uiManager.OnScanImageClicked(); // <-- This perfectly matches your UIManager!
        }
    }
}