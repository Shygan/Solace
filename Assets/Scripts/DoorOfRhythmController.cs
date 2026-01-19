using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DoorOfRhythmController : MonoBehaviour
{
    [Header("UI")]
    public Slider progressSlider;
    public GameObject LoadCanvas; // Canvas with HoldToLoadLevel UI
    public GameObject PromptCanvas; // Canvas with hold prompt text
    
    [Header("Fog")]
    public FogEventManager fogEventManager;
    
    [Header("Progress")]
    [SerializeField] int progressGoal = 100;
    private int progressAmount = 0;

    void Start()
    {
        progressAmount = 0;
        progressSlider.value = 0;
        progressSlider.maxValue = progressGoal;
        
        // Subscribe to apple collection event
        Apple.OnAppleCollect += IncreaseProgressAmount;
        // Do NOT subscribe to HoldToLoadLevel yet - wait until progress is full
        
        if (LoadCanvas != null)
            LoadCanvas.SetActive(false);
    }

    void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        Apple.OnAppleCollect -= IncreaseProgressAmount;
        HoldToLoadLevel.OnHoldComplete -= OnLevelComplete;
    }

    void IncreaseProgressAmount(int amount)
    {
        progressAmount += amount;
        progressSlider.value = progressAmount;
        Debug.Log($"[DoorOfRhythm] Added {amount}. Total now {progressAmount}. Max is {progressSlider.maxValue}");

        if (progressAmount >= progressGoal)
        {
            // Level is complete - show hold to load UI and disable fog
            if (LoadCanvas != null)
            {
                LoadCanvas.SetActive(true);
                Debug.Log("[DoorOfRhythm] LoadCanvas activated");
            }
            else
            {
                Debug.LogError("[DoorOfRhythm] LoadCanvas is not assigned!");
            }
            
            if (PromptCanvas != null)
            {
                PromptCanvas.SetActive(true);
                Debug.Log("[DoorOfRhythm] PromptCanvas activated");
            }
            
            // Stop fog from triggering
            if (fogEventManager != null)
            {
                fogEventManager.StopLoop();
                Debug.Log("[DoorOfRhythm] Fog stopped");
            }
            
            // NOW subscribe to HoldToLoadLevel so we can return to lobby
            HoldToLoadLevel.OnHoldComplete += OnLevelComplete;
            
            Debug.Log("[DoorOfRhythm] Level Complete! Hold E to proceed.");
        }
    }

    void OnLevelComplete()
    {
        Debug.Log("[DoorOfRhythm] Player held E to proceed. Loading Lobby Scene...");
        if (LoadCanvas != null)
            LoadCanvas.SetActive(false);
        
        // Load Lobby Scene
        SceneManager.LoadScene("Lobby Scene");
    }
}
