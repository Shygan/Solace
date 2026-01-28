using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controller for the Door of Grounding level.
/// Manages progress tracking, UI updates, and scene completion.
/// </summary>
public class DoorOfGroundingController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private GameObject loadCanvas;
    [SerializeField] private GameObject holdPromptWorldText;
    [SerializeField] private TextMeshProUGUI taskProgressUI;

    [Header("Level Settings")]
    [SerializeField] private string lobbySceneName = "Lobby Scene";
    [SerializeField] private int sightTaskGoal = 5; // 5 things to find for Sight task
    [SerializeField] private float delayBeforeCompletion = 1f;

    [Header("Player Reference")]
    [SerializeField] private GameObject player;

    private int progressAmount = 0;
    private bool levelComplete = false;

    void Start()
    {
        // Initialize UI
        progressAmount = 0;
        progressSlider.value = 0;
        progressSlider.maxValue = sightTaskGoal;

        if (loadCanvas != null)
            loadCanvas.SetActive(false);

        if (holdPromptWorldText != null)
            holdPromptWorldText.SetActive(false);

        // Subscribe to grounding events
        GroundingLevelManager.OnGroundingObjectFound += IncreaseProgress;
        HoldToLoadLevel.OnHoldComplete += CompleteLevel;

        Debug.Log("[DoorOfGroundingController] Scene initialized. Find 5 objects to complete.");
    }

    void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        GroundingLevelManager.OnGroundingObjectFound -= IncreaseProgress;
        HoldToLoadLevel.OnHoldComplete -= CompleteLevel;
    }

    void IncreaseProgress(int amount)
    {
        if (levelComplete) return; // Prevent progress after level completion

        progressAmount += amount;
        progressSlider.value = progressAmount;

        // Update task UI
        if (taskProgressUI != null)
        {
            taskProgressUI.text = $"Found: {progressAmount}/{sightTaskGoal}";
        }

        Debug.Log($"[DoorOfGrounding] Progress: {progressAmount}/{sightTaskGoal}");

        // Check if level is complete
        if (progressAmount >= sightTaskGoal)
        {
            OnLevelComplete();
        }
    }

    void OnLevelComplete()
    {
        levelComplete = true;
        Debug.Log("[DoorOfGrounding] All objects found! Level complete.");

        // Show completion UI
        if (loadCanvas != null)
            loadCanvas.SetActive(true);

        if (holdPromptWorldText != null)
            holdPromptWorldText.SetActive(true);
    }

    void CompleteLevel()
    {
        if (!levelComplete) return;

        if (holdPromptWorldText != null)
            holdPromptWorldText.SetActive(false);

        if (loadCanvas != null)
            loadCanvas.SetActive(false);

        Debug.Log("[DoorOfGrounding] Returning to lobby.");
        
        // Award the plant reward (same as completing a section)
        PlayerProgress.Instance.CompleteSection1();

        // Return to lobby
        SceneManager.LoadScene(lobbySceneName);
    }

    /// <summary>
    /// Optional: Reset progress without leaving scene (for testing).
    /// </summary>
    public void ResetLevel()
    {
        progressAmount = 0;
        progressSlider.value = 0;
        levelComplete = false;

        if (loadCanvas != null)
            loadCanvas.SetActive(false);

        if (holdPromptWorldText != null)
            holdPromptWorldText.SetActive(false);

        Debug.Log("[DoorOfGrounding] Level reset.");
    }
}
