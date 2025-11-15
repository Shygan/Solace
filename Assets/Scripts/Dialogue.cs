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
    public string levelToReturnTo;          // The name of the level to go back to (e.g. "Level 2")
    public Transform levelsParent;          // Drag the parent GameObject that contains all levels
    public GameObject player;               // Reference to player object for repositioning


    // Start is called before the first frame update
    void Start()
    {
        //textComponent.text = string.Empty;
        StartDialogue();
    }

    // Update is called once per frame
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
        //StopAllCoroutines();
        StartCoroutine(TypeLine());
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
            playerMovement.isMovementLocked = false;
            //gameObject.SetActive(false);
            // if (platformToReveal != null)
            // {
            //     platformToReveal.SetActive(true);
            // }

            if (!string.IsNullOrEmpty(levelToReturnTo))
            {
                StartCoroutine(ReturnToLevelAfterDelay(levelToReturnTo, 0.5f));
                Debug.Log("[Dialogue] Reached end of dialogue, attempting to return to level...");
            }

            StartCoroutine(DisableAfterDelay(0.2f));
            
            if (platformToReveal != null)
            {
                platformToReveal.SetActive(true);
            }
        }
    }

    private IEnumerator ReturnToLevelAfterDelay(string levelName, float delay)
    {
        Debug.Log($"[Dialogue] Attempting to return to {levelName} after {delay}s...");
        yield return new WaitForSeconds(delay);

        if (levelsParent == null)
        {
            Debug.LogWarning("[Dialogue] Levels Parent not assigned in Inspector!");
            yield break;
        }

        // Search recursively for target level BEFORE deactivating anything
        Transform targetLevel = FindChildRecursive(levelsParent, levelName);

        if (targetLevel == null)
        {
            Debug.LogWarning($"[Dialogue] ❌ Could not find '{levelName}' anywhere under '{levelsParent.name}'!");
            yield break;
        }

        // Move player FIRST (before deactivating current level)
        if (player != null)
        {
            player.transform.SetParent(targetLevel); // move player into target level hierarchy
            player.transform.position = new Vector3(-9f, -2f, 0f);
            Debug.Log("[Dialogue] Player repositioned to start point.");
        }

        // Now deactivate all levels EXCEPT the target
        foreach (Transform child in levelsParent)
        {
            child.gameObject.SetActive(child == targetLevel);
        }

        Debug.Log($"[Dialogue] ✅ Activated {targetLevel.name}");
    }

    // Helper method to search all descendants
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