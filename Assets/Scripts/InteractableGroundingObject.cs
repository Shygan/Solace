using UnityEngine;

/// <summary>
/// Makes objects interactable for grounding tasks.
/// When clicked, checks if it matches the current task requirement.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class InteractableGroundingObject : MonoBehaviour
{
    [Header("Object Properties")]
    [Tooltip("What type of grounding task this object belongs to")]
    public GroundingTaskType taskType = GroundingTaskType.Sight;

    [Header("Feedback")]
    [Tooltip("Visual feedback when found")]
    public GameObject foundEffect; // Optional particle effect or glow

    [Tooltip("Audio feedback when found")]
    public AudioClip foundSound;

    private SpriteRenderer spriteRenderer;
    private GroundingLevelManager levelManager;
    private bool hasBeenFound = false;
    private Color objectColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        objectColor = spriteRenderer.color;

        // Find the level manager
        levelManager = FindObjectOfType<GroundingLevelManager>();
        if (levelManager == null)
        {
            Debug.LogError("GroundingLevelManager not found in scene!");
        }

        // Ensure collider is set as trigger
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenFound) return;

        // Check if player touched this object
        if (other.CompareTag("Player"))
        {
            // Only process sight objects for now
            if (taskType == GroundingTaskType.Sight)
            {
                CheckIfMatchesTargetColor();
            }
        }
    }

    void CheckIfMatchesTargetColor()
    {
        if (levelManager.DoesColorMatch(objectColor))
        {
            MarkAsFound();
            levelManager.OnSightObjectFound();
        }
        else
        {
            // Wrong color - give feedback
            Debug.Log($"<color=red>That's not the right color. Keep looking!</color>");
            StartCoroutine(ShakeObject());
        }
    }

    void MarkAsFound()
    {
        hasBeenFound = true;

        // Visual feedback
        if (foundEffect != null)
        {
            Instantiate(foundEffect, transform.position, Quaternion.identity);
        }

        // Audio feedback
        if (foundSound != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(foundSound, Camera.main.transform.position);
        }

        // Add a gentle glow or sparkle effect
        StartCoroutine(PulseObject());
    }

    private System.Collections.IEnumerator ShakeObject()
    {
        Vector3 originalPos = transform.localPosition;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-0.1f, 0.1f);
            float y = Random.Range(-0.1f, 0.1f);
            transform.localPosition = originalPos + new Vector3(x, y, 0f);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }

    private System.Collections.IEnumerator PulseObject()
    {
        float duration = 1f;
        float elapsed = 0f;
        Vector3 originalScale = transform.localScale;

        while (elapsed < duration)
        {
            float scale = 1f + Mathf.Sin(elapsed * Mathf.PI * 2f) * 0.2f;
            transform.localScale = originalScale * scale;
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        
        // Optional: Hide or disable the object after being found
        // gameObject.SetActive(false);
    }
}

/// <summary>
/// Types of grounding tasks based on the 5-4-3-2-1 technique.
/// </summary>
public enum GroundingTaskType
{
    Sight,   // 5 things you can see
    Touch,   // 4 things you can touch
    Sound,   // 3 things you can hear
    Smell,   // 2 things you can smell
    Taste    // 1 thing you can taste
}
