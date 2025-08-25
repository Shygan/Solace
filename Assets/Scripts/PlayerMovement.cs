using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator animator;
    bool isFacingRight = true;
    [Header("Movement")]
    public float moveSpeed = 7f;
    float horizontalMovement;

    [Header("Jumping")]
    public float jumpPower = 8f;
    public int maxJumps = 2;
    int jumpsRemaining;

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;
    bool isGrounded;

    [Header("CheckForThoughtBubble")]
    private bool hasTriggeredThoughtBubble = false;

    [Header("Coyote Time")]
    public float coyoteTime = 5f;
    private float coyoteTimeCounter;

    [Header("WallCheck")]
    public Transform wallCheckPos;
    public Vector2 wallCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask wallLayer;

    [Header("WallMovement")]
    public float wallSlideSpeed = 2;
    bool isWallSliding;
    // Wall Jump
    bool isWallJumping;
    float wallJumpDirection;
    float wallJumpTime = 0.5f;
    float wallJumpTimer;
    public Vector2 wallJumpPower = new Vector2(5f, 10f);


    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallspeedMultiplier = 2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        GroundCheck();
        CheckForThoughtBubble();
        Gravity();
        ProcessWallSlide();
        ProcessWallJump();

        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);
            Flip();
        }

        animator.SetFloat("yVelocity", rb.linearVelocity.y);
        animator.SetFloat("magnitude", rb.linearVelocity.magnitude);
        animator.SetBool("isWallSliding", isWallSliding);
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (jumpsRemaining > 0 || coyoteTimeCounter > 0f)
        {
            if (context.performed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
                if (coyoteTimeCounter <= 0f) // only consume mid-air jump
                    jumpsRemaining--;

                coyoteTimeCounter = 0f;

                animator.SetTrigger("jump");
            }
            else if (context.canceled)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
                jumpsRemaining--;
                animator.SetTrigger("jump");
            }
        }

        //Wall Jump
        if (context.performed && wallJumpTimer > 0f)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y); // Jump away frm wall
            wallJumpTimer = 0;

            //force flip
            if (transform.localScale.x != wallJumpDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 ls = transform.localScale;
                ls.x *= -1f;
                transform.localScale = ls;
            }

            Invoke(nameof(CancelWallJump), wallJumpTime + 0.1f); // Wall jump = 0.5f -- Jump again 0.6f
        }
    }

    private void GroundCheck()
    {
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            jumpsRemaining = maxJumps;
            coyoteTimeCounter = coyoteTime;
            isGrounded = true;
        }
        else
        {
            coyoteTimeCounter = coyoteTimeCounter - Time.deltaTime;
            isGrounded = false;
        }
    }

private void CheckForThoughtBubble()
{
    if (isGrounded)
    {
        Collider2D hit = Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer);
            if (hit != null && hit.CompareTag("ThoughtBubble") && !hasTriggeredThoughtBubble)
            {
                Debug.Log("Dialogue Pops Up");
                hasTriggeredThoughtBubble = true;
            }
    }
}

    private bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, wallLayer);
    }

    private void Flip()
    {
        if (isFacingRight && horizontalMovement < 0 || !isFacingRight && horizontalMovement > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;
        }
    }
    private void Gravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallspeedMultiplier; //Falls increasingly faster
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    private void ProcessWallSlide()
    {
        // Not grounded & on a wall & movement != 0
        if (!isGrounded & WallCheck() & horizontalMovement != 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void ProcessWallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpTimer = wallJumpTime;

            CancelInvoke(nameof(CancelWallJump));
        }
        else if (wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        }
    }

    private void CancelWallJump()
    {
        isWallJumping = false;
    }

    private void OnDrawGizmosSelected()
    {
        //Ground
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        //Wall
        Gizmos.color = Color.brown;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    }
}
