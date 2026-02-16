using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private float horizontalInput;
    private float moveSpeed =  10f;
    private bool isFacingRight = true;
    private float jumpPower = 6f;
    private bool isJumping;



    private bool canDash = true;
    private bool isDashing;
    private float dashSpeed = 20f;
    private float dashDuration = 0.2f;
    private float dashCooldown = 0.5f;

    private bool doubleJump;
    private float doubleJumpPower = 8f;

    private Rigidbody2D rb;
    [SerializeField] private TrailRenderer dashTrail;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask stairsLayer;
    [SerializeField] private LayerMask ladderLayer;


    void Start()

    {

        rb = GetComponent<Rigidbody2D>();

    }
    
    void Update()
    {
        if (isDashing)
        {
            return;
        }

        horizontalInput = Input.GetAxis("Horizontal");
        FlipSprite();

        if (IsGrounded() && !Input.GetButton("Jump")&& !isJumping)
        {
            doubleJump = false;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (IsGrounded() || doubleJump)
            {
                rb.velocity = new Vector2(rb.velocity.x, doubleJump ? doubleJumpPower: jumpPower);

                isJumping = true;

                doubleJump = !doubleJump;
            }
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) && canDash)
        {
            StartCoroutine(Dash());
        }

        if (IsStairs())
        {
            rb.gravityScale = 7f;
              
        }
        else
        {
            rb.gravityScale = 1f;
        }

        if(Input.GetKeyDown(KeyCode.W) && IsLadder())
        {
            rb.gravityScale = 10f;
            rb.velocity = new Vector2(rb.velocity.x, moveSpeed);
        }
        else if(Input.GetKeyUp(KeyCode.W) && IsLadder())
        {
            rb.gravityScale = 0f;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }

    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }

        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }


    void FlipSprite()
    {
        if (isFacingRight && horizontalInput < 0f || !isFacingRight && horizontalInput > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

        private void OnCollisionEnter2D(Collision2D collision)
            {
                isJumping = false;
            }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private bool IsStairs()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.7f, stairsLayer);
    }

    private bool IsLadder()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.9f, ladderLayer);
    }





    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0f);
        dashTrail.emitting = true;
        yield return new WaitForSeconds(dashDuration);
        dashTrail.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}

