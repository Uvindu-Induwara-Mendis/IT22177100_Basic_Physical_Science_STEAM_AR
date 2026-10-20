using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    [Header("Drop your two text objects here!")]
    public GameObject englishTextObject;
    public GameObject sinhalaTextObject;

    void Start()
    {
        UpdateLanguage();
    }

    // This makes sure it updates even if the panel is turned on later!
    void OnEnable() 
    {
        UpdateLanguage();
    }

    public void UpdateLanguage()
    {
        // Check what language they picked on the Home Screen
        // (Assuming you saved it as "Language" in PlayerPrefs. Change "English" if your default is different!)
        string currentLang = PlayerPrefs.GetString("Language", "English");

        if (currentLang == "Sinhala")
        {
            if (englishTextObject != null) englishTextObject.SetActive(false);
            if (sinhalaTextObject != null) sinhalaTextObject.SetActive(true);
        }
        else 
        {
            // Default to English
            if (englishTextObject != null) englishTextObject.SetActive(true);
            if (sinhalaTextObject != null) sinhalaTextObject.SetActive(false);
        }
    }
}