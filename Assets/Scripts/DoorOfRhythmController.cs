using UnityEngine;
using UnityEngine.UI;

public class DoorOfRhythmController : MonoBehaviour
{
    [Header("UI")]
    public Slider progressSlider;
    public GameObject LoadCanvas; // Optional - leave empty if not needed
    
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
        HoldToLoadLevel.OnHoldComplete += OnLevelComplete;
        
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
            // Level is complete
            if (LoadCanvas != null)
                LoadCanvas.SetActive(true);
            Debug.Log("[DoorOfRhythm] Level Complete!");
        }
    }

    void OnLevelComplete()
    {
        Debug.Log("[DoorOfRhythm] Player held E to proceed. Loading next level...");
        if (LoadCanvas != null)
            LoadCanvas.SetActive(false);
        // Add your next scene load logic here
    }
}
