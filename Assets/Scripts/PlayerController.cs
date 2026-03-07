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
        // BLOQUEO ABSOLUTO DURANTE EL TUTORIAL
        bool isTutorialActive = (GameManager.instance != null && GameManager.instance.tutorialPanel != null && GameManager.instance.tutorialPanel.activeSelf);
        if (isTutorialActive)
        {
            moveX = 0f;
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

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
            Debug.Log("[PlayerController] Tecla ESC detectada");

            // No permitir ESC si el tutorial está activo
            if (GameManager.instance != null && GameManager.instance.tutorialPanel != null && GameManager.instance.tutorialPanel.activeSelf)
            {
                Debug.Log("[PlayerController] ESC bloqueado por el Tutorial.");
                return;
            }
            
            // SI los controles están abiertos, ESC los cierra
            if (UIManager.instance != null && UIManager.instance.controlsPanel != null && UIManager.instance.controlsPanel.activeSelf)
            {
                UIManager.instance.controlsPanel.SetActive(false);
                // Si el juego NO estaba pausado (raro, pero posible si se abrió sin pausar), no hacemos nada más.
                // Pero lo normal es que estuviéramos en el Menú de Pausa.
            }
            else
            {
                TogglePause();
            }
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
        if (GameManager.instance != null && GameManager.instance.tutorialPanel != null && GameManager.instance.tutorialPanel.activeSelf) return;
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
                string objName = collision.gameObject.name.ToLower();
                
                // Si es el Vacío (nombres comunes), muerte instantánea
                if (objName.Contains("vacio") || objName.Contains("void") || objName.Contains("fall") || objName.Contains("caida"))
                {
                    Debug.Log("[PlayerController] Caída al vacío detectada.");
                    HealthManager.instance.KillPlayer();
                }
                else
                {
                    // Si es otra cosa (como pinchos), quita 2 de vida (UN CORAZÓN ENTERO)
                    HealthManager.instance.HurtPlayer(2);
                    
                    if (HealthManager.instance.currentHealth > 0)
                    {
                        // Solo respawn si no ha muerto (si aún tiene vida)
                        // AÑADIDO: Pequeño empuje hacia atrás para que el impacto se sienta real
                        Vector2 knockbackDir = (transform.position - collision.transform.position).normalized;
                        rb.linearVelocity = Vector2.zero;
                        rb.AddForce(new Vector2(knockbackDir.x * 5f, 5f), ForceMode2D.Impulse);
                        
                        // Opcional: Respawn tras un breve retardo si prefieres que vuelva al inicio del salto
                        // transform.position = playerPosition; 
                    }
                }
            }
            else if (GameManager.instance != null)
            {
                GameManager.instance.Death();
            }
        }
    }

    public void MobileMove(float value) 
    { 
        if (GameManager.instance != null && GameManager.instance.tutorialPanel != null && GameManager.instance.tutorialPanel.activeSelf) return;
        moveX = value; 
    }
    public void MobileJump() 
    { 
        if (GameManager.instance != null && GameManager.instance.tutorialPanel != null && GameManager.instance.tutorialPanel.activeSelf) return;
        HandleJump(); 
    }
    public void Shoot() { }
    public void TogglePause()
    {
        if (GameManager.instance != null && GameManager.instance.tutorialPanel != null && GameManager.instance.tutorialPanel.activeSelf) return;
        
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (UIManager.instance == null)
        {
            Debug.LogWarning("[PlayerController] UIManager.instance es null. Buscando en la escena...");
            UIManager.instance = Object.FindFirstObjectByType<UIManager>();
        }

        if (UIManager.instance != null)
        {
            UIManager.instance.TogglePauseMenu(isPaused);
        }
        else
        {
            Debug.LogError("[PlayerController] No se pudo encontrar el UIManager.");
        }
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
