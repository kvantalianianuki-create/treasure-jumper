using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public int coins = 0;

    public int maxHealth = 100;
    public int health = 100;

    public float moveSpeed = 5f;
    public float accel = 30f;
    public float decel = 25f;
    public float iceAccel = 10f;
    public float iceDecel = 3f;

    public float jumpForce = 7.5f;
    public float jumpContinuesForce = 1f;
    public float maxHoldJumpTime = 0.18f;

    public float bouncePadMultiplier = 1.6f;

    public int extraJumpsValue = 1;
    private int extraJumps;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    public float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    public int damageAmount = 10;
    public float damageCooldown = 0.35f;
    private float lastDamageTime;

    public Image healthImage;

    public AudioClip jumpClip;
    public AudioClip hurtClip;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    private bool isGrounded;
    private float holdJumpCounter;

    private bool onIce;
    private bool touchingIce;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        extraJumps = extraJumpsValue;
        lastDamageTime = -999f;

        health = Mathf.Clamp(health, 0, maxHealth);

        if (Checkpoint.IsCheckpointValidForCurrentScene())
            transform.position = Checkpoint.savedPosition;
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (rb.linearVelocity.x != 0f)
            spriteRenderer.flipX = rb.linearVelocity.x < 0f;

        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            extraJumps = extraJumpsValue;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f)
        {
            if (coyoteTimeCounter > 0f)
            {
                DoJump();
                jumpBufferCounter = 0f;
            }
            else if (extraJumps > 0)
            {
                DoJump();
                extraJumps--;
                jumpBufferCounter = 0f;
            }
        }

        if (Input.GetKey(KeyCode.Space) && holdJumpCounter > 0f && rb.linearVelocity.y > 0f)
        {
            rb.AddForce(Vector2.up * jumpContinuesForce, ForceMode2D.Force);
            holdJumpCounter -= Time.deltaTime;
        }

        if (healthImage != null)
            healthImage.fillAmount = maxHealth <= 0 ? 0f : (float)health / maxHealth;

        if (transform.position.y < -20f)
            Die();

        SetAnimation(moveInput);
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        onIce = touchingIce;
        touchingIce = false;

        float moveInput = Input.GetAxis("Horizontal");

        float accelRate = onIce ? iceAccel : accel;
        float decelRate = onIce ? iceDecel : decel;

        if (Mathf.Abs(moveInput) > 0.01f)
        {
            rb.AddForce(new Vector2(moveInput * accelRate, 0f), ForceMode2D.Force);
        }
        else
        {
            if (!onIce)
            {
                float vx = rb.linearVelocity.x;
                rb.AddForce(new Vector2(-vx * decelRate, 0f), ForceMode2D.Force);
            }
        }

        rb.linearVelocity = new Vector2(Mathf.Clamp(rb.linearVelocity.x, -moveSpeed, moveSpeed), rb.linearVelocity.y);
    }

    private void DoJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        holdJumpCounter = maxHoldJumpTime;

        if (jumpClip != null)
            PlaySFX(jumpClip, 1f);

        coyoteTimeCounter = 0f;
    }

    private void Bounce()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * (jumpForce * bouncePadMultiplier), ForceMode2D.Impulse);
        holdJumpCounter = 0f;
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
    }

    private void SetAnimation(float moveInput)
    {
        if (animator == null) return;

        animator.SetBool("Grounded", isGrounded);
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (audioSource == null || clip == null) return;

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Damage"))
            TakeDamage(damageAmount);

        if (collision.collider.CompareTag("Ice"))
            touchingIce = true;

        if (collision.collider.CompareTag("BouncePad"))
            Bounce();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Damage"))
            TakeDamage(damageAmount);

        if (collision.collider.CompareTag("Ice"))
            touchingIce = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("BouncePad"))
            Bounce();

        if (other.CompareTag("Ice"))
            touchingIce = true;

        if (other.CompareTag("Damage"))
            TakeDamage(damageAmount);

        if (other.CompareTag("Banana"))
        {
            extraJumps = 2;
            extraJumpsValue = Mathf.Max(extraJumpsValue, 2);
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Orange"))
        {
            if (health < maxHealth)
            {
                int healAmount = Mathf.CeilToInt(maxHealth * 0.25f);
                health = Mathf.Min(health + healAmount, maxHealth);
                Destroy(other.gameObject);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Damage"))
            TakeDamage(damageAmount);

        if (other.CompareTag("Ice"))
            touchingIce = true;
    }

    private void TakeDamage(int amount)
    {
        if (Time.time - lastDamageTime < damageCooldown) return;
        lastDamageTime = Time.time;

        health -= amount;

        if (hurtClip != null)
            PlaySFX(hurtClip, 1f);

        if (health <= 0)
            Die();
    }

    private void Die()
    {
        if (Checkpoint.IsCheckpointValidForCurrentScene())
        {
            rb.linearVelocity = Vector2.zero;
            transform.position = Checkpoint.savedPosition;
            health = maxHealth;
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}