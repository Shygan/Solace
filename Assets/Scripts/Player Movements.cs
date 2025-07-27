using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float Speed = 5f; // [SerializeField] so we can edit in Unity itself
    [SerializeField] private float Jump = 7f;
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private Vector3 originalScale;

    private void Awake()
    {
        playerMovement = new PlayerMovement();
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale; // Stores original scale from Unity
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

        if (horizontalInput > 0.01f)
            transform.localScale = originalScale;
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z); // Has to be Vector3 as even tho its 2D, transform.localScale is a 3D property

        if (playerMovement.Land.Jump.triggered)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, Jump);
            }
    }
}
