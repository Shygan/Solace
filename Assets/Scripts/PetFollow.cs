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
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;

    private Rigidbody2D rb;
    private BoxCollider2D petCollider;
    private BoxCollider2D playerCollider;
    private Animator animator;

    private static readonly int MagnitudeHash = Animator.StringToHash("magnitude");
    private static readonly int YVelocityHash = Animator.StringToHash("yVelocity");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        petCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();

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
        
        // Determine if we should be moving based on distance
        bool shouldMove = distanceToPlayer > followDistance;

        if (shouldMove)
        {
            Vector2 targetPosition = Vector2.MoveTowards(rb.position, player.position, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(targetPosition);
            
            // Flip sprite based on direction to player
            FlipTowardPlayer();
        }

        UpdateAnimator(shouldMove);
    }

    private void FlipTowardPlayer()
    {
        float directionToPlayer = player.position.x - transform.position.x;
        
        if (directionToPlayer > 0.1f)
        {
            // Player is to the right, face right
            if (transform.localScale.x < 0)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (directionToPlayer < -0.1f)
        {
            // Player is to the left, face left
            if (transform.localScale.x > 0)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    private void UpdateAnimator(bool isMoving)
    {
        if (animator == null)
            return;

        // Animate based on intention to move, not actual velocity
        float magnitude = isMoving ? moveSpeed : 0f;
        
        animator.SetFloat(MagnitudeHash, magnitude);
        animator.SetFloat(YVelocityHash, rb.linearVelocity.y);
        animator.SetBool(IsGroundedHash, IsGrounded());
    }

    private bool IsGrounded()
    {
        if (rb == null)
            return true; // Default to grounded if no rigidbody

        // Only consider airborne if falling downward with significant speed
        return rb.linearVelocity.y >= -0.5f;
    }
}
