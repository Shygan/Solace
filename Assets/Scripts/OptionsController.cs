using UnityEngine;

public class OptionsController : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private GameObject targetRoot;
    [SerializeField] private GameObject[] groupsToDisable;

    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private Vector2 spawnCoordinates;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject != player)
            return;

        foreach (var g in groupsToDisable)
            if (g != null) g.SetActive(false);

        if (targetRoot != null)
            targetRoot.SetActive(true);

        // Teleport player
        var rb = player.GetComponent<Rigidbody2D>();
        player.transform.position = spawnCoordinates;
        rb.linearVelocity = Vector2.zero;

        // Force the animator into Idle immediately
        var anim = player.GetComponent<Animator>();
        anim.Play("Idle", 0, 0f); 
        anim.Update(0f);

    }
}
