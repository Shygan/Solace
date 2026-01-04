using UnityEngine;

/// <summary>
/// Tracks player progress across sections and unlocked rewards.
/// Uses PlayerPrefs for persistence but provides debug overrides for testing.
/// </summary>
public class PlayerProgress : MonoBehaviour
{
    private static PlayerProgress _instance;
    public static PlayerProgress Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("PlayerProgress");
                _instance = go.AddComponent<PlayerProgress>();
                DontDestroyOnLoad(go);
                _instance.LoadProgress();
            }
            return _instance;
        }
    }

    [Header("Debug Settings")]
    [Tooltip("Override: pretend all sections are completed for testing")]
    public bool debugAllSectionsComplete = false;
    [Tooltip("Clear all saved progress on next game start")]
    public bool resetProgressOnStart = false;

    [Header("Progress State (Read-Only)")]
    [SerializeField] private bool section1Completed = false;
    [SerializeField] private bool section2Completed = false;
    [SerializeField] private bool section3Completed = false;

    private const string SECTION1_KEY = "Section1_Completed";
    private const string SECTION2_KEY = "Section2_Completed";
    private const string SECTION3_KEY = "Section3_Completed";
    
    // Track if unlock dialogue has been shown
    private const string SECTION1_DIALOGUE_KEY = "Section1_DialogueShown";
    private const string SECTION2_DIALOGUE_KEY = "Section2_DialogueShown";
    private const string SECTION3_DIALOGUE_KEY = "Section3_DialogueShown";
    
    [SerializeField] private bool section1DialogueShown = false;
    [SerializeField] private bool section2DialogueShown = false;
    [SerializeField] private bool section3DialogueShown = false;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        LoadProgress();

        if (resetProgressOnStart)
        {
            ResetProgress();
            resetProgressOnStart = false;
        }
    }

    public void LoadProgress()
    {
        section1Completed = PlayerPrefs.GetInt(SECTION1_KEY, 0) == 1;
        section2Completed = PlayerPrefs.GetInt(SECTION2_KEY, 0) == 1;
        section3Completed = PlayerPrefs.GetInt(SECTION3_KEY, 0) == 1;
        
        section1DialogueShown = PlayerPrefs.GetInt(SECTION1_DIALOGUE_KEY, 0) == 1;
        section2DialogueShown = PlayerPrefs.GetInt(SECTION2_DIALOGUE_KEY, 0) == 1;
        section3DialogueShown = PlayerPrefs.GetInt(SECTION3_DIALOGUE_KEY, 0) == 1;
        
        Debug.Log($"[PlayerProgress] Loaded - Section1: {section1Completed}, Section2: {section2Completed}, Section3: {section3Completed}");
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt(SECTION1_KEY, section1Completed ? 1 : 0);
        PlayerPrefs.SetInt(SECTION2_KEY, section2Completed ? 1 : 0);
        PlayerPrefs.SetInt(SECTION3_KEY, section3Completed ? 1 : 0);
        
        PlayerPrefs.SetInt(SECTION1_DIALOGUE_KEY, section1DialogueShown ? 1 : 0);
        PlayerPrefs.SetInt(SECTION2_DIALOGUE_KEY, section2DialogueShown ? 1 : 0);
        PlayerPrefs.SetInt(SECTION3_DIALOGUE_KEY, section3DialogueShown ? 1 : 0);
        PlayerPrefs.Save();
        
        Debug.Log($"[PlayerProgress] Saved - Section1: {section1Completed}, Section2: {section2Completed}, Section3: {section3Completed}");
    }

    public bool IsSectionComplete(string sectionName)
    {
        if (debugAllSectionsComplete)
            return true;

        return sectionName switch
        {
            "Section 1" or "Section1" => section1Completed,
            "Section 2" or "Section2" => section2Completed,
            "Section 3" or "Section3" => section3Completed,
            _ => false
        };
    }

    // Legacy method for backward compatibility
    public bool IsPlantUnlocked() => section1Completed;

    public void CompleteSection(string sectionName)
    {
        switch (sectionName)
        {
            case "Section 1":
            case "Section1":
                section1Completed = true;
                break;
            case "Section 2":
            case "Section2":
                section2Completed = true;
                break;
            case "Section 3":
            case "Section3":
                section3Completed = true;
                break;
        }
        SaveProgress();
        Debug.Log($"[PlayerProgress] {sectionName} completed!");
    }

    // Legacy method for backward compatibility
    public void CompleteSection1()
    {
        CompleteSection("Section 1");
    }

    public void ResetProgress()
    {
        section1Completed = false;
        section2Completed = false;
        section3Completed = false;
        section1DialogueShown = false;
        section2DialogueShown = false;
        section3DialogueShown = false;
        PlayerPrefs.DeleteKey(SECTION1_KEY);
        PlayerPrefs.DeleteKey(SECTION2_KEY);
        PlayerPrefs.DeleteKey(SECTION3_KEY);
        PlayerPrefs.DeleteKey(SECTION1_DIALOGUE_KEY);
        PlayerPrefs.DeleteKey(SECTION2_DIALOGUE_KEY);
        PlayerPrefs.DeleteKey(SECTION3_DIALOGUE_KEY);
        PlayerPrefs.Save();
        Debug.Log("[PlayerProgress] Progress reset!");
    }

    public bool HasShownDialogue(string sectionName)
    {
        return sectionName switch
        {
            "Section 1" or "Section1" => section1DialogueShown,
            "Section 2" or "Section2" => section2DialogueShown,
            "Section 3" or "Section3" => section3DialogueShown,
            _ => false
        };
    }

    public void MarkDialogueShown(string sectionName)
    {
        switch (sectionName)
        {
            case "Section 1":
            case "Section1":
                section1DialogueShown = true;
                break;
            case "Section 2":
            case "Section2":
                section2DialogueShown = true;
                break;
            case "Section 3":
            case "Section3":
                section3DialogueShown = true;
                break;
        }
        SaveProgress();
        Debug.Log($"[PlayerProgress] Dialogue shown for {sectionName}");
    }

    // Debug helpers - call from inspector or console
    [ContextMenu("Complete All Sections (Debug)")]
    public void DebugCompleteAllSections()
    {
        CompleteSection("Section 1");
        CompleteSection("Section 2");
        CompleteSection("Section 3");
    }

    [ContextMenu("Reset All Progress (Debug)")]
    public void DebugResetProgress()
    {
        ResetProgress();
    }
}
