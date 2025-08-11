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
    public Transform player;
    public float moveSpeed = 5f;
    public float followDistance = 1.5f;

    private Rigidbody2D rb;
    private BoxCollider2D petCollider;
    private BoxCollider2D playerCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        petCollider = GetComponent<BoxCollider2D>();

        if (player != null)
            playerCollider = player.GetComponent<BoxCollider2D>();

        // Ignore collisions between pet and player
        if (petCollider != null && playerCollider != null)
            Physics2D.IgnoreCollision(petCollider, playerCollider);
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(rb.position, player.position);

        if (distanceToPlayer > followDistance)
        {
            Vector2 targetPosition = Vector2.MoveTowards(rb.position, player.position, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(targetPosition);
        }
    }
}
