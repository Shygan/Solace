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
    [SerializeField] private int touchTaskGoal = 4; // 4 things to find for Touch task
    [SerializeField] private int soundTaskGoal = 3; // 3 things to hear for Sound task
    [SerializeField] private int smellTaskGoal = 2; // 2 things to smell for Smell task
    [SerializeField] private int tasteTaskGoal = 1; // 1 thing to taste for Taste task
    [SerializeField] private float delayBeforeCompletion = 1f;

    [Header("Player Reference")]
    [SerializeField] private GameObject player;

    [Header("Task Dialogues (Optional)")]
    [SerializeField] private Dialogue touchTaskDialogue; // shown after Sight complete
    [SerializeField] private Dialogue soundTaskDialogue; // shown after Touch complete
    [SerializeField] private Dialogue smellTaskDialogue; // shown after Sound complete
    [SerializeField] private Dialogue tasteTaskDialogue; // shown after Smell complete

    private int progressAmount = 0;
    private bool levelComplete = false;
    private CurrentTask currentTask = CurrentTask.Sight;
    
    private enum CurrentTask
    {
        Sight,  // 5 objects
        Touch,  // 4 objects
        Sound,  // 3 objects
        Smell,  // 2 objects
        Taste   // 1 object
    }

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

        // Get current task goal based on active task
        int currentGoal = GetCurrentTaskGoal();

        // Update task UI
        if (taskProgressUI != null)
        {
            taskProgressUI.text = $"Found: {progressAmount}/{currentGoal}";
        }

        Debug.Log($"[DoorOfGrounding] Progress: {progressAmount}/{currentGoal} ({currentTask})");

        // Check if current task is complete
        if (progressAmount >= currentGoal)
        {
            OnLevelComplete();
        }
    }
    
    int GetCurrentTaskGoal()
    {
        switch (currentTask)
        {
            case CurrentTask.Sight: return sightTaskGoal;
            case CurrentTask.Touch: return touchTaskGoal;
            case CurrentTask.Sound: return soundTaskGoal;
            case CurrentTask.Smell: return smellTaskGoal;
            case CurrentTask.Taste: return tasteTaskGoal;
            default: return sightTaskGoal;
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

        GroundingLevelManager levelManager = FindObjectOfType<GroundingLevelManager>();
        
        // Progress through all tasks in the 5-4-3-2-1 sequence
        switch (currentTask)
        {
            case CurrentTask.Sight:
                Debug.Log("[DoorOfGrounding] Sight task complete. Starting touch task (4 objects).");
                TransitionToNextTask(CurrentTask.Touch, touchTaskGoal);
                if (levelManager != null) levelManager.StartTouchTask();
                StartTaskDialogue(touchTaskDialogue);
                break;
                
            case CurrentTask.Touch:
                Debug.Log("[DoorOfGrounding] Touch task complete. Starting sound task (3 objects).");
                TransitionToNextTask(CurrentTask.Sound, soundTaskGoal);
                if (levelManager != null) levelManager.StartSoundTask();
                StartTaskDialogue(soundTaskDialogue);
                break;
                
            case CurrentTask.Sound:
                Debug.Log("[DoorOfGrounding] Sound task complete. Starting smell task (2 objects).");
                TransitionToNextTask(CurrentTask.Smell, smellTaskGoal);
                if (levelManager != null) levelManager.StartSmellTask();
                StartTaskDialogue(smellTaskDialogue);
                break;
                
            case CurrentTask.Smell:
                Debug.Log("[DoorOfGrounding] Smell task complete. Starting taste task (1 object).");
                TransitionToNextTask(CurrentTask.Taste, tasteTaskGoal);
                if (levelManager != null) levelManager.StartTasteTask();
                StartTaskDialogue(tasteTaskDialogue);
                break;
                
            case CurrentTask.Taste:
                Debug.Log("[DoorOfGrounding] All grounding tasks complete! Returning to lobby.");
                
                // Award the plant reward
                PlayerProgress.Instance.CompleteSection1();

                // Return to lobby after completing all 5 tasks
                SceneManager.LoadScene(lobbySceneName);
                break;
        }
    }
    
    void TransitionToNextTask(CurrentTask nextTask, int nextGoal)
    {
        currentTask = nextTask;
        levelComplete = false;
        progressAmount = 0;
        progressSlider.value = 0;
        progressSlider.maxValue = nextGoal;
        
        if (taskProgressUI != null)
        {
            taskProgressUI.text = $"Found: 0/{nextGoal}";
        }
    }

    void StartTaskDialogue(Dialogue dialogue)
    {
        if (dialogue == null)
            return;

        if (!dialogue.gameObject.activeInHierarchy)
            dialogue.gameObject.SetActive(true);

        dialogue.StartDialogue();
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
