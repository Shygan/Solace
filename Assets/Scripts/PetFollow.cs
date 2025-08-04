// using UnityEngine;

// public class PetFollow : MonoBehaviour
// {
//     [SerializeField] private Transform player;
//     //[SerializeField] private float followSpeed;
//     [SerializeField] private Vector3 offset = new Vector3(-2f, 0, 0);
//     [SerializeField] private float smoothTime = 0.2f;
//     private Vector3 velocity = Vector3.zero;
//     void Update()
//     {
//         Vector3 PetPosition = player.position + offset;
//         transform.position = Vector3.SmoothDamp(transform.position, PetPosition, ref velocity, smoothTime);
//     }
// }
using UnityEngine;

public class PetFollow : MonoBehaviour
{
    public Transform player; // Assign your player's Transform in the Inspector
    public float moveSpeed = 5f;
    public float followDistance = 1.5f; // Distance at which the pet stops following
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(rb.position, player.position);

            if (distanceToPlayer > followDistance)
            {
                // Smoothly move toward the player's position (both X and Y axes)
                Vector2 newPosition = Vector2.MoveTowards(rb.position, player.position, moveSpeed * Time.fixedDeltaTime);

                // Move the Rigidbody to the new position
                rb.MovePosition(newPosition);
            }
        }
    }
}