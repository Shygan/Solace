using UnityEngine;
using TMPro;

/// <summary>
/// Shows unlock text when player is near a locked plant placeholder.
/// Automatically disabled when plant is unlocked.
/// </summary>
public class PlaceholderInteractable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool debugShowInteractionRange = true;
    
    private Transform player;
    private TextMeshPro labelText;
    private SpriteRenderer placeholderSprite;

    void Start()
    {
        // Find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Find label text on this GameObject or children
        labelText = GetComponentInChildren<TextMeshPro>();
        if (labelText != null)
            labelText.enabled = false;

        // Get the placeholder sprite renderer
        placeholderSprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null || labelText == null) return;

        // Only show text if placeholder is still visible (not unlocked yet)
        if (placeholderSprite != null && !placeholderSprite.enabled)
        {
            labelText.enabled = false;
            return;
        }

        // Check if player is in range
        float distance = Vector3.Distance(transform.position, player.position);
        bool playerInRange = distance <= interactionRange;

        // Show/hide label text based on distance
        labelText.enabled = playerInRange;
    }

    void OnDrawGizmosSelected()
    {
        if (debugShowInteractionRange)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
