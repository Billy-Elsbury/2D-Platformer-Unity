using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Controls { mobile, pc, AI }

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float doubleJumpForce = 8f;
    public LayerMask groundLayer;
    public Transform groundCheck;

    private Rigidbody2D rb;
    public bool isGroundedBool = false;
    public Chromosome InputChromosome { get; set; } // Chromosome for AI control
    public Animator playeranim;
    float gameTime = 0;
    public Controls controlmode;

    private float moveX;
    public bool isPaused = false;

    public ParticleSystem footsteps;
    private ParticleSystem.EmissionModule footEmissions;

    public ParticleSystem ImpactEffect;
    private bool wasonGround;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        footEmissions = footsteps.emission;

        if (controlmode == Controls.mobile)
        {
            UIManager.instance.EnableMobileControls();
        }

        Chromosome chromosome = new Chromosome();
        chromosome.LeftTime = new List<float> { 0.5f, 2.0f, 3.0f }; // Example left movement timings
        chromosome.RightTime = new List<float> { 15.5f, 25.5f, 40.0f }; // Example right movement timings
        chromosome.JumpTime = new List<float> { 1.2f, 2.2f }; // Example jump timings

        PlayerController player = FindObjectOfType<PlayerController>();
        player.InputChromosome = chromosome;
        //player.controlmode = Controls.AI;
    }

    private void Update()
    {
        gameTime += Time.deltaTime;

        // Get movement input from the chromosome (AI control)
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

        // Update animations and sprite flipping
        SetAnimations();
        if (moveX != 0)
        {
            FlipSprite(moveX);
        }

        wasonGround = isGroundedBool;
        isGroundedBool = IsGrounded();
    }

    private void FixedUpdate()
    {
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
        if (collision.gameObject.tag == "killzone")
        {
            GameManager.instance.Death();
        }
    }
}