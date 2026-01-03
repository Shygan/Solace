using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the lobby scene: spawns rewards, handles scene transitions.
/// Debug-friendly with inspector toggles for testing without playing through the game.
/// </summary>
public class LobbyController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform plantPedestal;
    [SerializeField] private GameObject plantPrefab;
    
    [Header("Scene Names")]
    [SerializeField] private string cognitiveReframeSceneName = "Level 1 Scene";

    [Header("Debug Settings")]
    [Tooltip("Force spawn plant even if not unlocked (for testing)")]
    public bool debugForceSpawnPlant = false;
    [Tooltip("Skip progress check and always spawn plant")]
    public bool debugAlwaysSpawnPlant = false;

    private GameObject spawnedPlant;

    void Start()
    {
        CheckAndSpawnRewards();
    }

    void CheckAndSpawnRewards()
    {
        // Check if player has completed section 1 and earned the plant
        bool shouldSpawnPlant = debugAlwaysSpawnPlant || 
                                debugForceSpawnPlant || 
                                PlayerProgress.Instance.IsPlantUnlocked();

        if (shouldSpawnPlant && plantPrefab != null && plantPedestal != null)
        {
            SpawnPlant();
        }
        else
        {
            Debug.Log("[LobbyController] Plant not unlocked yet or missing references.");
        }
    }

    void SpawnPlant()
    {
        if (spawnedPlant != null)
        {
            Debug.LogWarning("[LobbyController] Plant already spawned!");
            return;
        }

        spawnedPlant = Instantiate(plantPrefab, plantPedestal.position, Quaternion.identity);
        spawnedPlant.transform.SetParent(plantPedestal);
        
        Debug.Log($"[LobbyController] Plant spawned at {plantPedestal.position}");

        // Wire up the plant interaction if needed
        PlantInteractable interactable = spawnedPlant.GetComponent<PlantInteractable>();
        if (interactable != null)
        {
            interactable.lobbyController = this;
        }
    }

    // Called by portal/trigger to enter cognitive reframe section
    public void EnterCognitiveReframeSection()
    {
        Debug.Log("[LobbyController] Loading Cognitive Reframe section...");
        SceneManager.LoadScene(cognitiveReframeSceneName);
    }

    // Debug helper
    [ContextMenu("Force Spawn Plant (Debug)")]
    public void DebugSpawnPlant()
    {
        if (spawnedPlant != null)
            Destroy(spawnedPlant);
        
        SpawnPlant();
    }
}
