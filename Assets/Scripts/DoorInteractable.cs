using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Makes a door/entrance interactable.
/// When player presses E, loads the specified scene.
/// </summary>
public class DoorInteractable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private GameObject interactPrompt; // Optional UI prompt ("Press E")

    [Header("Scene to Load")]
    [SerializeField] private string targetSceneName = "Level 1 Scene";
    [Tooltip("Optional: Start at specific level index. Leave at -1 to use default.")]
    [SerializeField] private int startLevelIndex = 0; // Level 1 (0-indexed)

    [Header("Debug")]
    [SerializeField] private bool debugShowInteractionRange = true;

    private Transform player;
    private bool playerInRange = false;

    void Start()
    {
        // Find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
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

        // Handle interaction
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            OnInteract();
        }
    }

    void OnInteract()
    {
        Debug.Log("[DoorInteractable] Player opened the door - loading " + targetSceneName);

        // Set starting level if specified
        if (startLevelIndex >= 0)
        {
            // Create a simple flag or use a static helper to track starting level
            PlayerPrefs.SetInt("StartLevelIndex", startLevelIndex);
            PlayerPrefs.Save();
        }

        SceneManager.LoadScene(targetSceneName);
    }

    void OnDrawGizmosSelected()
    {
        if (debugShowInteractionRange)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
