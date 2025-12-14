using UnityEngine;

public class OptionsController : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private GameObject targetRoot;
    [SerializeField] private GameObject[] groupsToDisable;

    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private Vector2 spawnCoordinates;

    [Header("AI-Generated Option Dialogue")]
    [SerializeField] private int optionIndex; // 0-3 for Options 1-4
    [SerializeField] private Dialogue optionDialogue; // e.g., Option1Dialogue component

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        foreach (var g in groupsToDisable)
            if (g != null) g.SetActive(false);

        if (targetRoot != null)
            targetRoot.SetActive(true);

        // Teleport player
        var rb = player.GetComponent<Rigidbody2D>();
        //player.SetActive(true); // ensure active
        player.transform.position = spawnCoordinates;
        rb.linearVelocity = Vector2.zero;

        // Force the animator into Idle immediately
        var anim = player.GetComponent<Animator>();
        anim.Play("Idle", 0, 0f); 
        anim.Update(0f);

        // Update and start AI-generated dialogue for this option
        UpdateOptionDialogue();
    }

    private void UpdateOptionDialogue()
    {
        if (optionDialogue == null || ThoughtManager.instance == null)
            return;

        var currentThought = ThoughtManager.instance.GetCurrentThought();
        if (currentThought == null || currentThought.options == null || optionIndex < 0 || optionIndex >= currentThought.options.Length)
            return;

        // Get the AI-generated explanation dialogue for this option
        string aiDialogue = currentThought.options[optionIndex].dialogue;

        // Set it into the Dialogue component and start
        optionDialogue.SetDialogueLines(new[] { aiDialogue });
        optionDialogue.gameObject.SetActive(true);
        optionDialogue.StartDialogue();

        Debug.Log($"[OptionsController] Started AI dialogue for Option {optionIndex + 1}: {aiDialogue}");
    }
}
