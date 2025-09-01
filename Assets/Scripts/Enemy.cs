using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Enemy : MonoBehaviour
{
    public Transform player;
    public float chaseSpeed = 2f;
    public float jumpForce = 2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool shouldJump;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Is grounded? (Current Pos. of enemy, Cast ray straight down, raycast distance, Check against ground objects)
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer); // Therefore, if there is a ground 1 unit below, isGrounded = true

        // Player Direction (Distance btwn enemy and player)
        float direction = Mathf.Sign(player.position.x - transform.position.x);

        //Player above detection (Is player above me?)
        bool isPlayerAbove = Physics2D.Raycast(transform.position, Vector2.up, 5f, 1 << player.gameObject.layer); //Last param is a bitmask

        if (isGrounded)
        {
            //Chase Player (Just horizontal movement)
            rb.linearVelocity = new UnityEngine.Vector2(direction * chaseSpeed, rb.linearVelocity.y); //Moves left/right chasing player

            // Jump if gap ahead and no ground in front
            // Else if player above and platform above

            // If Ground
            RaycastHit2D groundInFront = Physics2D.Raycast(transform.position, new Vector2(direction, 0), 2f, groundLayer);

            // If Gap
            RaycastHit2D gapAhead = Physics2D.Raycast(transform.position + new Vector3(direction, 0, 0), Vector2.down, 2f, groundLayer);

            // If Platform
            RaycastHit2D platformAbove = Physics2D.Raycast(transform.position, Vector2.up, 5f, groundLayer);

            if (!groundInFront.collider && !gapAhead.collider)
            {
                shouldJump = true;
            }
            else if (isPlayerAbove && platformAbove.collider)
            {
                shouldJump = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isGrounded && shouldJump)
        {
            shouldJump = false;
            Vector2 direction = (player.position - transform.position).normalized; // So it doesnt go faster on diagonals

            Vector2 jumpDirection = direction * jumpForce;

            rb.AddForce(new Vector2(jumpDirection.x, jumpForce), ForceMode2D.Impulse);
        }
    }
}
