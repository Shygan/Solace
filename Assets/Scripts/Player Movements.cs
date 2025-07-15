using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;

    private void Awake()
    {
        playerMovement = new PlayerMovement();
        rb = GetComponent<Rigidbody2D>();
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
        rb.linearVelocity = new Vector2(move.x * 5f, rb.linearVelocity.y); // Set a value!

        if (playerMovement.Land.Jump.triggered)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 7f); // Set a value!
        }
    }
}
