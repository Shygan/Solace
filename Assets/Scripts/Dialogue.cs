using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed;
    private int index;
    public PlayerMovement playerMovement; // Assign in Inspector
    public GameObject platformToReveal;

    [Header("Level Return (Optional)")]
    public string levelToReturnTo;          // Must match level name exactly (e.g., "Level 2")
    public Transform levelsParent;          // Drag the parent GameObject "Levels"
    public GameObject player;               // Reference to player for repositioning


    void Start()
    {
        StartDialogue();
        //textComponent.text = string.Empty;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (textComponent.text == lines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }

    public void StartDialogue()
    {
        playerMovement.isMovementLocked = true;
        index = 0;
        textComponent.text = string.Empty;
        StartCoroutine(TypeLine());
    }

    /// <summary>
    /// Set dialogue lines at runtime (used for AI-generated dialogue)
    /// </summary>
    public void SetDialogueLines(string[] newLines)
    {
        lines = newLines;
        Debug.Log($"[Dialogue] Dialogue lines updated to {newLines.Length} line(s)");
    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            // End of dialogue
            playerMovement.isMovementLocked = false;

            // Reveal the platform immediately
            if (platformToReveal != null)
                platformToReveal.SetActive(true);

            // If returning to a level, handle that FIRST
            if (!string.IsNullOrEmpty(levelToReturnTo))
            {
                StartCoroutine(ReturnToLevelThenDisable(levelToReturnTo, 0.5f));
            }
            else
            {
                // Otherwise just hide dialogue normally
                StartCoroutine(DisableAfterDelay(0.2f));
            }
        }
    }

    // NEW: Proper sequence — return to level THEN disable dialogue
    private IEnumerator ReturnToLevelThenDisable(string levelName, float delay)
    {
        yield return ReturnToLevelAfterDelay(levelName, delay);
        yield return new WaitForSeconds(0.1f); 
        gameObject.SetActive(false);
    }

    private IEnumerator ReturnToLevelAfterDelay(string levelName, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (levelsParent == null)
        {
            Debug.LogWarning("[Dialogue] Levels Parent not assigned!");
            yield break;
        }

        // Find the level by name
        Transform targetLevel = FindChildRecursive(levelsParent, levelName);

        if (targetLevel == null)
        {
            Debug.LogWarning($"[Dialogue] ❌ Could not find '{levelName}' under '{levelsParent.name}'!");
            yield break;
        }

        // Move player to the target level BEFORE disabling anything
        if (player != null)
        {
            player.transform.SetParent(null);
            player.transform.position = new Vector3(-9f, -2f, 0f);
            //player.transform.SetParent(targetLevel);
            //player.transform.position = new Vector3(-9f, -2f, 0f);
        }

        // Deactivate all levels except the target one
        foreach (Transform child in levelsParent)
            child.gameObject.SetActive(child == targetLevel);

        Debug.Log($"[Dialogue] ✅ Returned to {targetLevel.name}");
    }

    // Recursive search for level by name
    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Replace(" ", "").ToLower() == name.Replace(" ", "").ToLower())
                return child;

            Transform result = FindChildRecursive(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}