using UnityEngine;

public class PlayerCustomController : MonoBehaviour
{
    public float moveSpeed = 500f;       // Speed of horizontal movement
    public float jumpForce = 1000f;     // Force applied when jumping
    private Rigidbody2D rb;           // Reference to the Rigidbody2D component
    private bool isGrounded;          // Whether the player is on the ground

    // Custom input flags
    private bool moveLeft;
    private bool moveRight;
    private bool jump;

    void Start()
    {
        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Handle horizontal movement
        float moveX = 0f;
        if (moveLeft)
        {
            moveX = -1f;
        }
        else if (moveRight)
        {
            moveX = 1f;
        }

        // Apply horizontal velocity
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);

        // Handle jumping
        if (jump && isGrounded)
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            isGrounded = false; // Player is no longer grounded
            jump = false;       // Reset jump flag
        }
    }

    // Public methods to trigger inputs
    public void MoveLeft()
    {
        moveLeft = true;
        moveRight = false;
    }

    public void MoveRight()
    {
        moveRight = true;
        moveLeft = false;
    }

    public void StopMoving()
    {
        moveLeft = false;
        moveRight = false;
    }

    public void Jump()
    {
        if (isGrounded)
        {
            jump = true;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the player is grounded
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}