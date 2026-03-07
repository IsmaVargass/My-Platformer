using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Controls { mobile, pc }

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float doubleJumpForce = 8f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.4f;

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

    [Header("Audio")]
    public AudioSource levelBGM;
    public AudioSource jumpSound;
    public AudioSource landSound;
    public AudioSource walkSound;
    public AudioSource coinSound;
    public AudioSource hurtSound;

    private Vector3 baseScale = new Vector3(3.0f, 3.0f, 3.0f);
    private int jumpCount = 0;
    public int maxJumps = 2;
    private float lastJumpTime;
    private float jumpCooldown = 0.15f;
    private Vector3 playerPosition;

    [Header("Ajuste Visual")]
    public float visualOffsetY = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        transform.localScale = baseScale;
        playerPosition = transform.position;

        if (controlmode == Controls.mobile)
        {
            if (UIManager.instance != null) UIManager.instance.EnableMobileControls();
        }
    }

    private void Update()
    {
        isGroundedBool = IsGrounded();

        if (isGroundedBool && Time.time > lastJumpTime + jumpCooldown)
        {
            if (jumpCount > 0)
            {
                jumpCount = 0;
                if (playeranim != null) playeranim.ResetTrigger("jump");
            }
        }

        if (UIManager.instance != null && UIManager.instance.jumpSlider != null)
        {
            UIManager.instance.jumpSlider.maxValue = maxJumps;
            UIManager.instance.jumpSlider.value = maxJumps - jumpCount;
        }

        foreach (Transform child in transform)
        {
            if (child.name != "Ground Check" && child.GetComponent<SpriteRenderer>() != null)
            {
                child.localPosition = new Vector3(0, visualOffsetY, 0);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
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
            if (landSound != null) landSound.Play();

            if (ImpactEffect != null && groundCheck != null)
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
        if (jumpCount < maxJumps)
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
        playeranim.SetBool("run", isMoving);
        
        if (footsteps != null)
        {
            var emission = footsteps.emission;
            bool shouldWalk = isMoving && isGroundedBool;
            emission.rateOverTime = shouldWalk ? 35f : 0f;
            footsteps.transform.position = groundCheck.position;

            if (walkSound != null)
            {
                if (shouldWalk && !walkSound.isPlaying) walkSound.Play();
                else if (!shouldWalk && walkSound.isPlaying) walkSound.Stop();
            }
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
        if (playeranim != null) 
        {
            playeranim.ResetTrigger("jump");
            playeranim.SetTrigger("jump");
        }
        if (jumpSound != null) jumpSound.Play();
        lastJumpTime = Time.time;
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
        foreach (var col in colliders)
        {
            if (!col.isTrigger && !col.transform.IsChildOf(transform) && col.gameObject != gameObject)
            {
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(groundCheck.position, groundCheckRadius);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "killzone")
        {
            if (HealthManager.instance != null)
            {
                HealthManager.instance.HurtPlayer();
                if (HealthManager.instance.currentHealth > 0)
                {
                    transform.position = playerPosition;
                    rb.linearVelocity = Vector2.zero;
                }
            }
            else if (GameManager.instance != null)
            {
                GameManager.instance.Death();
            }
        }
    }

    public void MobileMove(float value) { moveX = value; }
    public void MobileJump() { HandleJump(); }
    public void Shoot() { }
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (UIManager.instance != null) UIManager.instance.TogglePauseMenu(isPaused);
    }
    public void MobileShoot()
    {
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }
}
