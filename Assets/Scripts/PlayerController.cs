using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Controls { mobile, pc, AI }

public class PlayerController : MonoBehaviour
{
    public Chromosome InputChromosome { get; set; }
    public Controls controlmode;
    public bool isPaused = false;

    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public LayerMask groundLayer;
    public Transform groundCheck;

    //player variables
    public bool isGroundedBool = false;
    internal bool isDead = false;
    internal float gameTime = 0;
    private float moveX;
    private bool wasonGround;

    public Animator playeranim;

    public Vector3 startPosition;
    private Rigidbody2D rb;
    private GameManager gm;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void Start()
    {
        gm = GameManager.instance;

        
        rb = GetComponent<Rigidbody2D>();

        if (controlmode == Controls.mobile)
        {
            UIManager.instance.EnableMobileControls();
        }
    }

    public void Update()
    {
        gameTime += Time.deltaTime;

        // Update animations and sprite flipping
        SetAnimations();
        if (moveX != 0)
        {
            FlipSprite(moveX);
        }
    }

    private void FixedUpdate()
    {
        if (controlmode == Controls.AI && InputChromosome != null)
        {
            moveX = InputChromosome.UpdateX(gameTime);

            // Check if the chromosome says to jump
            if (InputChromosome.ShouldJump(gameTime) && isGroundedBool)
            {
                Jump(jumpForce);
            }
        }

        else if (controlmode == Controls.pc)
        {
            // Get movement input from the player (keyboard/controller)
            moveX = Input.GetAxis("Horizontal");

            if (Input.GetButtonDown("Jump") && isGroundedBool)
            {
                Jump(jumpForce);
            }
        }

        wasonGround = isGroundedBool;
        isGroundedBool = IsGrounded();

        // Apply movement to the Rigidbody2D
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);
    }

    private void Jump(float jumpForce)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0); // Zero out vertical velocity
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        playeranim.SetTrigger("jump");
    }

    private bool IsGrounded()
    {
        float rayLength = 0.25f;
        Vector2 rayOrigin = new Vector2(groundCheck.transform.position.x, groundCheck.transform.position.y - 0.1f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, groundLayer);
        return hit.collider != null;
    }

    private void SetAnimations()
    {
        if (moveX != 0 && isGroundedBool)
        {
            playeranim.SetBool("run", true);
        }
        else
        {
            playeranim.SetBool("run", false);
        }

        playeranim.SetBool("isGrounded", isGroundedBool);
    }

    private void FlipSprite(float direction)
    {
        if (direction > 0)
        {
            // Moving right, flip sprite to the right
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (direction < 0)
        {
            // Moving left, flip sprite to the left
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("killzone"))
        {
            DieAndReset();
        }
    }

    private void DieAndReset()
    {
        Debug.Log("Death");
        isDead = true;
        gm.wasDeadThisRun = true;  // Mark death for this simulation
        ResetPlayer();
        gm.ResetGameState();  // Reset state, but don't erase wasDeadThisRun
    }



    public void ResetPlayer()
    {
        // Reset position and velocity
        transform.position = startPosition;
        if (rb != null)
        {
            rb.velocity = Vector2.zero; // Reset velocity
            rb.angularVelocity = 0f; // Reset angular velocity (if applicable)
        }

        // Reset state variables
        gameTime = 0f;
        isGroundedBool = false;
        moveX = 0f;
        wasonGround = false;
        isDead = false;

        // Reset chromosome (if applicable)
        InputChromosome = null;

        Debug.Log("Player reset");
    }
}