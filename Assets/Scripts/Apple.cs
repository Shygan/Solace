using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Apple : MonoBehaviour, IItem
{
    public static event Action<int> OnAppleCollect;
    public int worth = 5;

    private bool collected = false;
    private Collider2D _col;   // cache to disable instantly

    [Header("Dialogue Settings (Optional)")]
    public GameObject dialogueObject;       // assign in Inspector if needed
    public float dialogueDelay = 3f;        // delay before dialogue starts

    [Tooltip("Which option does this apple belong to? (0=Option1, 1=Option2, 2=Option3, 3=Option4). Used for AI-generated explanations.")]
    public int optionIndex = 0;

    [Tooltip("Optional: assign the level root this apple belongs to. Dialogue only triggers when this level is active.")]
    public GameObject levelRoot;

    [Tooltip("Fallback name check if levelRoot is not set.")]
    public string requiredLevelName = "";

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
    }

    public void Collect()
    {
        if (collected) return;          // guard: ignore second call
        collected = true;

        if (_col) _col.enabled = false;  // stop further trigger events this frame
        OnAppleCollect?.Invoke(worth);   // safe invoke (null-checked)

        if (dialogueObject != null && IsCorrectLevelActive())
        {
            StartCoroutine(StartDialogueAfterDelay());
            GetComponent<SpriteRenderer>().enabled = false; // hide apple
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator StartDialogueAfterDelay()
    {
        yield return new WaitForSeconds(dialogueDelay);

        dialogueObject.SetActive(true);
        var dialogue = dialogueObject.GetComponent<Dialogue>();
        if (dialogue != null)
        {
            // Set the option index so dialogue knows which explanation to show
            dialogue.SetOptionIndex(optionIndex);
            //dialogue.StartDialogue(); //alr starts in Dialogue script
        }

        yield return new WaitForSeconds(0.2f); // small delay to ensure dialogue starts
        Destroy(gameObject);
    }
    
    private bool IsCorrectLevelActive()
    {
        // Preferred: explicit level root reference
        if (levelRoot != null)
            return levelRoot.activeInHierarchy;

        // Fallback: name-based lookup if provided
        if (!string.IsNullOrWhiteSpace(requiredLevelName))
        {
            GameObject currentLevel = GameObject.Find(requiredLevelName);
            if (currentLevel != null)
                return currentLevel.activeInHierarchy;

            // If not found, fail open so dialogue can still trigger instead of blocking progression
            Debug.LogWarning($"[Apple] Level named '{requiredLevelName}' not found; allowing dialogue to trigger.");
            return true;
        }

        // If nothing is specified, allow dialogue
        return true;
    }
}