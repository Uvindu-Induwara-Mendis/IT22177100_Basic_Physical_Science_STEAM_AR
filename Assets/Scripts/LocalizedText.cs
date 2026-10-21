using UnityEngine;

// =========================================================================================
// UTILITY: DYNAMIC BILINGUAL UI TEXT TOGGLER
// =========================================================================================
// Context: Attached to UI elements containing paired English and Sinhala text components.
// Reads the current language from PlayerPrefs ("English" / "Sinhala") and enables the matching
// GameObject while hiding the other. Automatically called on startup, OnEnable, or via UIManager.
public class LocalizedText : MonoBehaviour
{
    // --- Text GameObjects to Toggle ---
    [Header("Drop your two text objects here!")]
    public GameObject englishTextObject;
    public GameObject sinhalaTextObject;

    void Start()
    {
        UpdateLanguage();
    }

    void OnEnable() 
    {
        UpdateLanguage();
    }

    // Toggles between English and Sinhala GameObjects based on PlayerPrefs setting
    public void UpdateLanguage()
    {
        string currentLang = PlayerPrefs.GetString("Language", "English");

        if (currentLang == "Sinhala")
        {
            if (englishTextObject != null) englishTextObject.SetActive(false);
            if (sinhalaTextObject != null) sinhalaTextObject.SetActive(true);
        }
        else 
        {
            if (englishTextObject != null) englishTextObject.SetActive(true);
            if (sinhalaTextObject != null) sinhalaTextObject.SetActive(false);
        }
    }
}