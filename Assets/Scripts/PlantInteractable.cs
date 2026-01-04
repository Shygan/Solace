using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Handles player interaction with the plant reward.
/// When interacted with, opens the AI quiz practice flow.
/// </summary>
public class PlantInteractable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private GameObject interactPrompt; // Optional UI prompt ("Press E")

    [Header("Quiz Settings")]
    [SerializeField] private string quizSceneName = "Level 1 Scene"; // Or dedicated quiz scene
    [Tooltip("If true, load quiz scene. If false, trigger in-lobby quiz UI")]
    [SerializeField] private bool loadQuizScene = true;
    [Header("Practice Settings")]
    [SerializeField] private int practiceStartLevelIndex = 2; // Level 3 (0-indexed)
    [SerializeField] private int practiceRuns = 2;
    [SerializeField] private string practiceReturnScene = "Lobby Scene";

    [Header("Debug")]
    [SerializeField] private bool debugShowInteractionRange = true;

    [HideInInspector] public LobbyController lobbyController;
    
    private Transform player;
    private bool playerInRange = false;
    private TextMeshPro labelText;

    void Start()
    {
        // Find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        // Find label text on this plant prefab (not the placeholder)
        labelText = GetComponentInChildren<TextMeshPro>();
        if (labelText != null)
            labelText.enabled = false;
    }

    void Update()
    {
        if (player == null) return;

        // Check if player is in range
        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;

        // Show/hide prompt
        if (interactPrompt != null)
            interactPrompt.SetActive(playerInRange);

        // Show/hide label text
        if (labelText != null)
            labelText.enabled = playerInRange;

        // Handle interaction
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            OnInteract();
        }
    }

    void OnInteract()
    {
        Debug.Log("[PlantInteractable] Player interacted with plant - opening quiz practice...");

        if (loadQuizScene)
        {
            // Configure a practice session before loading the scene
            PracticeSession.StartPractice(practiceStartLevelIndex, practiceRuns, practiceReturnScene);
            // Load the AI quiz scene/level
            SceneManager.LoadScene(quizSceneName);
        }
        else
        {
            // TODO: Trigger in-lobby quiz UI panel
            Debug.Log("[PlantInteractable] In-lobby quiz UI not yet implemented");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (debugShowInteractionRange)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
