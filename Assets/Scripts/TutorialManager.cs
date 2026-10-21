using UnityEngine;

// =========================================================================================
// ONBOARDING: AR SCANNING TUTORIAL SLIDESHOW
// =========================================================================================
// Context: Displays a multi-slide guide when the student chooses to view the tutorial from the Home menu.
// Teaches the student how to point the camera at storybook markers, interact with 3D items,
// and navigate lessons. Once the final slide is completed (or skipped), opens the AR Scanner.
public class TutorialManager : MonoBehaviour
{
    // --- Connections & Slide References ---
    [Header("Connections")]
    public UIManager uiManager;              // Used to launch the AR Scanner when tutorial ends
    public GameObject tutorialPanel;         // Root UI panel for the tutorial overlay

    [Header("Tutorial Pages")]
    public GameObject[] slides;              // Array of individual tutorial slide pages (Slide 1, 2, 3)
    
    private int currentSlide = 0;

    // --- Opens Tutorial from Home Menu ---
    public void OpenTutorial()
    {
        tutorialPanel.SetActive(true);
        currentSlide = 0;
        UpdateSlides();
    }

    // --- Next Button Tap ---
    // Advances to next slide, or finishes tutorial if on last slide
    public void NextSlide()
    {
        if (currentSlide < slides.Length - 1)
        {
            currentSlide++;
            UpdateSlides();
        }
        else
        {
            FinishTutorial(); 
        }
    }

    // --- Skip Button Tap ---
    public void SkipTutorial()
    {
        FinishTutorial();
    }

    // Activates only the current slide index
    private void UpdateSlides()
    {
        for (int i = 0; i < slides.Length; i++)
        {
            if (slides[i] != null) slides[i].SetActive(false);
        }
        
        if (slides[currentSlide] != null) slides[currentSlide].SetActive(true);
    }

    // Closes tutorial overlay and transitions to the AR camera scanner
    private void FinishTutorial()
    {
        tutorialPanel.SetActive(false);
        
        if (uiManager != null)
        {
            uiManager.OnScanImageClicked();
        }
    }
}