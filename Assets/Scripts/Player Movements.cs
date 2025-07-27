using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float Speed = 5f;
    [SerializeField] private float Jump = 7f;
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
        rb.linearVelocity = new Vector2(move.x * Speed, rb.linearVelocity.y); 

        if (playerMovement.Land.Jump.triggered)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Jump);
        }
    }
}
