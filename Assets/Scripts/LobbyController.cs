using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the lobby scene: spawns rewards, handles scene transitions.
/// Supports multiple plant pedestals for multiple unlocked sections.
/// Debug-friendly with inspector toggles for testing without playing through the game.
/// </summary>
public class LobbyController : MonoBehaviour
{
    [System.Serializable]
    public class PlantSlot
    {
        public Transform pedestal;
        public GameObject plantPrefab;
        public string sectionName = "Section 1";
        [Tooltip("Returns true if this plant should be spawned")]
        public bool IsUnlocked() => PlayerProgress.Instance.IsSectionComplete(sectionName);
    }

    [Header("Plant Setup")]
    [SerializeField] private PlantSlot[] plantSlots = new PlantSlot[3];
    
    [Header("Unlock Dialogues (match slots)")]
    [SerializeField] private GameObject[] plantUnlockDialogues;
    
    [Header("Scene Names")]
    [SerializeField] private string cognitiveReframeSceneName = "Level 1 Scene";

    [Header("Debug Settings")]
    [Tooltip("Force spawn all plants (for testing)")]
    public bool debugForceSpawnPlants = false;
    [Tooltip("Skip progress check and always spawn all plants")]
    public bool debugAlwaysSpawnPlants = false;

    private GameObject[] spawnedPlants;

    void Start()
    {
        spawnedPlants = new GameObject[plantSlots.Length];
        CheckAndSpawnRewards();
    }

    void CheckAndSpawnRewards()
    {
        for (int i = 0; i < plantSlots.Length; i++)
        {
            if (plantSlots[i] == null || plantSlots[i].pedestal == null || plantSlots[i].plantPrefab == null)
                continue;

            bool shouldSpawn = debugAlwaysSpawnPlants || 
                               debugForceSpawnPlants || 
                               plantSlots[i].IsUnlocked();

            if (shouldSpawn)
            {
                SpawnPlant(i);
            }
        }
    }

    void SpawnPlant(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= plantSlots.Length)
        {
            Debug.LogError($"[LobbyController] Invalid slot index: {slotIndex}");
            return;
        }

        PlantSlot slot = plantSlots[slotIndex];
        if (slot == null || slot.pedestal == null || slot.plantPrefab == null)
        {
            Debug.LogWarning($"[LobbyController] Slot {slotIndex} is not properly configured!");
            return;
        }

        if (spawnedPlants[slotIndex] != null)
        {
            Debug.LogWarning($"[LobbyController] Plant already spawned at slot {slotIndex}!");
            return;
        }

        // Hide the placeholder sprite when plant is unlocked
        SpriteRenderer placeholderRenderer = slot.pedestal.GetComponent<SpriteRenderer>();
        if (placeholderRenderer != null)
        {
            placeholderRenderer.enabled = false;
            Debug.Log($"[LobbyController] Placeholder hidden for slot {slotIndex}");
        }

        spawnedPlants[slotIndex] = Instantiate(slot.plantPrefab, slot.pedestal.position, Quaternion.identity);
        spawnedPlants[slotIndex].transform.SetParent(slot.pedestal);
        
        Debug.Log($"[LobbyController] {slot.sectionName} plant spawned at slot {slotIndex}");

        // Wire up the plant interaction
        PlantInteractable interactable = spawnedPlants[slotIndex].GetComponent<PlantInteractable>();
        if (interactable != null)
        {
            interactable.lobbyController = this;
        }

        // Trigger unlock dialogue for this slot, if provided
        if (plantUnlockDialogues != null && slotIndex < plantUnlockDialogues.Length)
        {
            var dlgObj = plantUnlockDialogues[slotIndex];
            if (dlgObj != null)
            {
                dlgObj.SetActive(true);
                var dialogueComp = dlgObj.GetComponent<Dialogue>();
                if (dialogueComp != null)
                    dialogueComp.StartDialogue();
            }
        }
    }

    // Called by portal/trigger to enter cognitive reframe section
    public void EnterCognitiveReframeSection()
    {
        Debug.Log("[LobbyController] Loading Cognitive Reframe section...");
        SceneManager.LoadScene(cognitiveReframeSceneName);
    }

    // Debug helper
    [ContextMenu("Force Spawn All Plants (Debug)")]
    public void DebugSpawnAllPlants()
    {
        // Clear existing plants
        for (int i = 0; i < spawnedPlants.Length; i++)
        {
            if (spawnedPlants[i] != null)
                Destroy(spawnedPlants[i]);
        }
        
        // Respawn all
        CheckAndSpawnRewards();
    }
}
