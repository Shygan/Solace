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
    public string requiredLevelName = "Level 2"; // only triggers in this level

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
            //dialogue.StartDialogue(); //alr starts in Dialogue script
        }

        yield return new WaitForSeconds(0.2f); // small delay to ensure dialogue starts
        Destroy(gameObject);
    }
    
    private bool IsCorrectLevelActive()
    {
        GameObject currentLevel = GameObject.Find(requiredLevelName);
        return currentLevel != null && currentLevel.activeSelf;
    }
}