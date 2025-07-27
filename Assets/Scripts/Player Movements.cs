using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float Speed = 5f; // [SerializeField] so we can edit in Unity itself
    [SerializeField] private float JumpHeight = 7f;
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private Vector3 originalScale;
    private Animator animator; // For animations!!!
    private bool grounded; // For jumping (Keeps track of when player is on ground or not)
    private void Awake()
    {
        // Grabs references for rigidbody, animator, etc.
        playerMovement = new PlayerMovement();
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale; // Stores original scale from Unity
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        playerMovement.Enable();
    }

    private void OnDisable()
    {
        playerMovement.Disable();
    }

    private void Update()
    {
        Vector2 move = playerMovement.Land.Move.ReadValue<Vector2>();
        float horizontalInput = move.x;

        rb.linearVelocity = new Vector2(horizontalInput * Speed, rb.linearVelocity.y); // Vector2 as rb is a 2D property

        // To flip player model when moving left or right
        if (horizontalInput > 0.01f)
            transform.localScale = originalScale;
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z); // Has to be Vector3 as even tho its 2D, transform.localScale is a 3D property

        if (playerMovement.Land.Jump.triggered && grounded)
        {
            Jump();
        }

        // Set animator parameters
        animator.SetBool("run", horizontalInput != 0);
        animator.SetBool("grounded", grounded);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpHeight);
        animator.SetTrigger("jump");
        grounded = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
            grounded = true;
    }
}
