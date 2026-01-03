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
    [Tooltip("Override: pretend section 1 is completed for testing")]
    public bool debugSection1Complete = false;
    [Tooltip("Override: pretend plant is already unlocked for testing")]
    public bool debugPlantUnlocked = false;
    [Tooltip("Clear all saved progress on next game start")]
    public bool resetProgressOnStart = false;

    [Header("Progress State (Read-Only)")]
    [SerializeField] private bool section1Completed = false;
    [SerializeField] private bool plantUnlocked = false;

    private const string SECTION1_KEY = "Section1_Completed";
    private const string PLANT_KEY = "Plant_Unlocked";

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
        plantUnlocked = PlayerPrefs.GetInt(PLANT_KEY, 0) == 1;
        
        Debug.Log($"[PlayerProgress] Loaded - Section1: {section1Completed}, Plant: {plantUnlocked}");
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt(SECTION1_KEY, section1Completed ? 1 : 0);
        PlayerPrefs.SetInt(PLANT_KEY, plantUnlocked ? 1 : 0);
        PlayerPrefs.Save();
        
        Debug.Log($"[PlayerProgress] Saved - Section1: {section1Completed}, Plant: {plantUnlocked}");
    }

    public bool IsSection1Complete()
    {
        return debugSection1Complete || section1Completed;
    }

    public bool IsPlantUnlocked()
    {
        return debugPlantUnlocked || plantUnlocked;
    }

    public void CompleteSection1()
    {
        section1Completed = true;
        plantUnlocked = true;
        SaveProgress();
        Debug.Log("[PlayerProgress] Section 1 completed! Plant unlocked.");
    }

    public void ResetProgress()
    {
        section1Completed = false;
        plantUnlocked = false;
        PlayerPrefs.DeleteKey(SECTION1_KEY);
        PlayerPrefs.DeleteKey(PLANT_KEY);
        PlayerPrefs.Save();
        Debug.Log("[PlayerProgress] Progress reset!");
    }

    // Debug helper - call from inspector or console
    [ContextMenu("Complete Section 1 (Debug)")]
    public void DebugCompleteSection1()
    {
        CompleteSection1();
    }

    [ContextMenu("Reset All Progress (Debug)")]
    public void DebugResetProgress()
    {
        ResetProgress();
    }
}
