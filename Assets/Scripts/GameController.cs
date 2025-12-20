using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    int progressAmount;
    public Slider progressSlider;
    public GameObject player;
    public GameObject LoadCanvas;
    public GameObject holdPromptWorldText;

    public List<GameObject> levels;
    private int currentLevelIndex = 0;

    // Make progress goal configurable
    [SerializeField] int progressGoal = 100;

    // Optional per-level spawn points (match levels.Count)
    public List<Transform> levelSpawnPoints;

    // Optional per-level intro dialogues (match levels.Count)
    public List<GameObject> levelIntroDialogues;

    [SerializeField] int debugStartLevelIndex = 0;

    // AI Thought Generation for Level 2
    [Header("AI Thought Generation")]
    [SerializeField] private AIThoughtGenerator aiThoughtGenerator;
    [SerializeField] private bool generateAIThoughtForLevel2 = true;
    [SerializeField] private int aiThoughtLevelIndex = 1; // Level 2 (0-indexed)


    void Start()
    {
        currentLevelIndex = debugStartLevelIndex;
        
        progressAmount = 0;
        progressSlider.value = 0;
        progressSlider.maxValue = progressGoal;

        Apple.OnAppleCollect += IncreaseProgressAmount;
        HoldToLoadLevel.OnHoldComplete += LoadNextLevel;
        LoadCanvas.SetActive(false);

        // Ensure only current level is active at start
        for (int i = 0; i < levels.Count; i++)
            levels[i].SetActive(i == currentLevelIndex);

        // If we start directly on the AI-designated level, generate the AI thought now.
        if (generateAIThoughtForLevel2 && currentLevelIndex == aiThoughtLevelIndex && aiThoughtGenerator != null)
        {
            StartCoroutine(LoadLevelWithAIThought(currentLevelIndex));
        }
        else
        {
            // Normal dialogue flow for non-AI levels
            StartDialogueForLevel(currentLevelIndex);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks / null callbacks
        Apple.OnAppleCollect -= IncreaseProgressAmount;
        HoldToLoadLevel.OnHoldComplete -= LoadNextLevel;
    }

    void Update()
    {
        
    }

    void IncreaseProgressAmount(int amount)
    {
        progressAmount += amount;
        progressSlider.value = progressAmount;
        Debug.Log($"Added {amount}. Total now {progressAmount}. Max is {progressSlider.maxValue}");

        if (progressAmount >= progressGoal)
        {
            // Level is complete
            LoadCanvas.SetActive(true);

            if (holdPromptWorldText != null)
            {
                holdPromptWorldText.SetActive(true);
            }

            Debug.Log("Level Complete");
        }
    }

    void LoadNextLevel()
    {
        if (holdPromptWorldText != null)
        {
            holdPromptWorldText.SetActive(false);
        }
        
        if (levels == null || levels.Count == 0) return;

        int nextLevelIndex = (currentLevelIndex == levels.Count - 1) ? 0 : currentLevelIndex + 1;
        LoadCanvas.SetActive(false);

        levels[currentLevelIndex].SetActive(false);
        levels[nextLevelIndex].SetActive(true);

        // Move player to the spawn point for the next level if provided
        if (levelSpawnPoints != null && nextLevelIndex < levelSpawnPoints.Count && levelSpawnPoints[nextLevelIndex] != null)
            player.transform.position = levelSpawnPoints[nextLevelIndex].position;
        else
            player.transform.position = new Vector3(-9, -2, 0); // fallback

        currentLevelIndex = nextLevelIndex;
        progressAmount = 0;
        progressSlider.value = 0;

        // Generate AI thought if this is the designated AI level
        if (generateAIThoughtForLevel2 && nextLevelIndex == aiThoughtLevelIndex && aiThoughtGenerator != null)
        {
            StartCoroutine(LoadLevelWithAIThought(nextLevelIndex));
        }
        else
        {
            // Normal dialogue flow for non-AI levels
            StartDialogueForLevel(nextLevelIndex);
        }
    }

    private IEnumerator LoadLevelWithAIThought(int levelIndex)
    {
        // Show loading UI while generating thought
        LoadCanvas.SetActive(true);
        
        bool generationComplete = false;
        AnxietyThoughtData thoughtData = null;

        // Call AI to generate thought
        aiThoughtGenerator.GenerateThought(
            (data) =>
            {
                thoughtData = data;
                generationComplete = true;
                LoadCanvas.SetActive(false);
            },
            (error) =>
            {
                Debug.LogError($"[GameController] AI Generation Error: {error}");
                generationComplete = true;
                LoadCanvas.SetActive(false);
                // Fall back to default dialogue if AI fails
                StartDialogueForLevel(levelIndex);
            }
        );

        // Wait for AI generation to complete
        yield return new WaitUntil(() => generationComplete);

        if (thoughtData != null)
        {
            // Store the thought in ThoughtManager
            if (ThoughtManager.instance != null)
                ThoughtManager.instance.SetCurrentThought(thoughtData);

            // Update dialogue with the generated intro dialogue
            if (levelIntroDialogues != null && levelIndex < levelIntroDialogues.Count)
            {
                var dlgObj = levelIntroDialogues[levelIndex];
                if (dlgObj != null)
                {
                    dlgObj.SetActive(true);
                    var dialogueComp = dlgObj.GetComponent<Dialogue>();
                    if (dialogueComp != null)
                    {
                        // Override dialogue lines with AI-generated intro
                        dialogueComp.SetDialogueLines(new[] { thoughtData.introDialogue });
                        dialogueComp.StartDialogue();
                    }
                }
            }
        }
    }

    private void StartDialogueForLevel(int levelIndex)
    {
        // Handle per-level intro dialogues
        if (levelIntroDialogues != null && levelIndex < levelIntroDialogues.Count)
        {
            // deactivate others
            foreach (var dlg in levelIntroDialogues)
                if (dlg != null) dlg.SetActive(false);

            var dlgObj = levelIntroDialogues[levelIndex];
            if (dlgObj != null)
            {
                dlgObj.SetActive(true);
                var dialogueComp = dlgObj.GetComponent<Dialogue>();
                if (dialogueComp != null) dialogueComp.StartDialogue();
            }
        }
    }

    // Optional helper to set the starting level at runtime
    public void SetCurrentLevel(int index)
    {
        if (index < 0 || index >= levels.Count) return;
        levels[currentLevelIndex].SetActive(false);
        currentLevelIndex = index;
        levels[currentLevelIndex].SetActive(true);
    }
}

// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class GameController : MonoBehaviour
// {
//     int progressAmount;
//     public Slider progressSlider;
//     public GameObject player;
//     public GameObject LoadCanvas;
//     public List<GameObject> levels;
//     private int currentLevelIndex = 0;
//     public GameObject level2IntroDialogue;

//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         progressAmount = 0;
//         progressSlider.value = 0;
//         Apple.OnAppleCollect += IncreaseProgressAmount;
//         HoldToLoadLevel.OnHoldComplete += LoadNextLevel;
//         LoadCanvas.SetActive(false);
//         // if (levelIntroDialogue != null)
//         // {
//         //     levelIntroDialogue.gameObject.SetActive(true);
//         //     levelIntroDialogue.StartDialogue();
//         // }
//         //Debug.Log("Start");
//         }

//     // Update is called once per frame
//     void Update()
//     {

//     }

//     void IncreaseProgressAmount(int amount)
//     {
//         progressAmount += amount;
//         progressSlider.value = progressAmount;
//         Debug.Log($"Added {amount}. Total now {progressAmount}. Max is {progressSlider.maxValue}");

//         if (progressAmount >= 100)
//         {
//             // Level is complete
//             LoadCanvas.SetActive(true);
//             Debug.Log("Level Complete");
//         }
//     }

//     void LoadNextLevel()
//     {
//         int nextLevelIndex = (currentLevelIndex == levels.Count - 1) ? 0 : currentLevelIndex + 1;
//         LoadCanvas.SetActive(false);

//         levels[currentLevelIndex].SetActive(false);
//         levels[nextLevelIndex].SetActive(true);

//         levels[nextLevelIndex].SetActive(true);

//         player.transform.position = new Vector3(-9, -2, 0);

//         currentLevelIndex = nextLevelIndex;
//         progressAmount = 0;
//         progressSlider.value = 0;

//         if (nextLevelIndex == 1) // Level 2
//         {
//             level2IntroDialogue.SetActive(true); // Show Dialogue
//             level2IntroDialogue.GetComponent<Dialogue>().StartDialogue(); // Start typing
//         }
//     }
// }