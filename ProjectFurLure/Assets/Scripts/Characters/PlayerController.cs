using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    float horizontalInput;
    float moveSpeed =  10f;
    bool isFacingRight = false;
    float jumpPower = 6f;
    bool isJumping = false;

    Rigidbody2D rb;

    void Start()

    {

        rb = GetComponent<Rigidbody2D>();

    }
    
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        FlipSprite();
        if(Input.GetButtonDown("Jump") && !isJumping)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
            isJumping = true;
        }
    }

    private void FixedUpdate()
    {
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
        
    
}

