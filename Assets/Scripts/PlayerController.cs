
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Controls { mobile,pc}

public class PlayerController : MonoBehaviour
{


    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float doubleJumpForce = 8f;
    public LayerMask groundLayer;
    public Transform groundCheck;

    private Rigidbody2D rb;
    private bool isGroundedBool = false;
    public Animator playeranim;
    public Controls controlmode;
    private float moveX;
    public bool isPaused = false;

    public ParticleSystem footsteps;
    public ParticleSystem ImpactEffect;
    private bool wasonGround;

    public float fireRate = 0.5f;
    private float nextFireTime = 0f;

    private Vector3 baseScale = new Vector3(5.0f, 5.0f, 5.0f);
    private int jumpCount = 0;
    public int maxJumps = 2;

    [Header("Ajuste Visual")]
    public float visualOffsetY = 0f; // Si el muñeco vuela, pon aquí un número negativo (ej: -0.5)

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        transform.localScale = baseScale;

        if (controlmode == Controls.mobile)
        {
            if (UIManager.instance != null) UIManager.instance.EnableMobileControls();
        }
    }

    private void Update()
    {
        isGroundedBool = IsGrounded();

        if (isGroundedBool)
        {
            jumpCount = 0; 
        }

        // Ajuste Visual Dinámico: Mueve el dibujo respecto al colisionador
        // Esto permite bajar el dibujo si el colisionador es muy grande
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            sr.transform.localPosition = new Vector3(0, visualOffsetY, 0);
        }

        if (controlmode == Controls.pc && !isPaused)
        {
            moveX = Input.GetAxis("Horizontal");

            if (Input.GetButtonDown("Jump"))
            {
                HandleJump();
            }

            if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + 1f / fireRate;
            }
        }

        SetAnimations();

        if (moveX != 0)
        {
            FlipSprite(moveX);
        }

        if (!wasonGround && isGroundedBool)
        {
            if (ImpactEffect != null)
            {
                ImpactEffect.gameObject.SetActive(true);
                ImpactEffect.Stop();
                ImpactEffect.transform.position = groundCheck.position;
                ImpactEffect.Play();
            }
        }

        wasonGround = isGroundedBool;
    }

    private void HandleJump()
    {
        if (isGroundedBool || jumpCount < maxJumps)
        {
            float force = (jumpCount == 0) ? jumpForce : doubleJumpForce;
            Jump(force);
            jumpCount++;
        }
    }

    public void SetAnimations()
    {
        if (playeranim == null) return;
        
        bool isMoving = Mathf.Abs(moveX) > 0.1f;
        playeranim.SetBool("run", isMoving && isGroundedBool);

        if (footsteps != null)
        {
            var emission = footsteps.emission;
            emission.rateOverTime = (isMoving && isGroundedBool) ? 35f : 0f;
            footsteps.transform.position = groundCheck.position;
        }
    }

    private void FlipSprite(float direction)
    {
        if (direction > 0)
            transform.localScale = baseScale;
        else if (direction < 0)
            transform.localScale = new Vector3(-baseScale.x, baseScale.y, baseScale.z);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
    }

    private void Jump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        if (playeranim != null) playeranim.SetTrigger("jump");
    }

    private bool IsGrounded()
    {
        // Rayo MUCHO más largo para detectar el suelo incluso si el muñeco vuela
        float rayLength = 1.0f; 
        Vector2 rayOrigin = groundCheck.position;
        
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, groundLayer);
        Debug.DrawRay(rayOrigin, Vector2.down * rayLength, hit.collider != null ? Color.green : Color.red);
        
        return hit.collider != null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "killzone")
            if (GameManager.instance != null) GameManager.instance.Death();
    }

    public void MobileMove(float value)
    {
        moveX = value;
    }

    public void MobileJump()
    {
        HandleJump();
    }

    public void Shoot() { }

    public void MobileShoot()
    {
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }
}
