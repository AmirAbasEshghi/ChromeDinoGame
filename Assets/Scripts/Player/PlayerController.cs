using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using ChromeDino.Core;

namespace ChromeDino.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        public static event Action<int> OnHealthChanged;

        [Header("Health")]
        [SerializeField] private int maxHealth = 3;
        private int currentHealth;

        [Header("Jump and Physics")]
        [SerializeField] private float jumpForce = 13f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;
        private bool isGrounded;

        [Header("Dash")]
        [SerializeField] private float dashSpeed = 14f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 1.0f;
        [SerializeField] private float returnSpeed = 6f;
        private bool isDashing;
        private float lastDashTime = -10f;
        private float startX;

        [Header("Damage Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color hitColor = Color.red;
        private Color normalColor;

        private Rigidbody2D rb;
        private Animator animator;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                normalColor = spriteRenderer.color;
            }

            startX = transform.position.x;
            currentHealth = maxHealth;
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(currentHealth);
        }

        private void Update()
        {
            if (currentHealth <= 0) return;

            // Smooth return to base position after dash
            if (!isDashing && Mathf.Abs(transform.position.x - startX) > 0.01f)
            {
                float newX = Mathf.MoveTowards(transform.position.x, startX, returnSpeed * Time.deltaTime);
                transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            }

            // Ground check
            if (groundCheck != null)
            {
                isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
            }

            if (animator != null)
            {
                animator.SetBool("IsGrounded", isGrounded);
                animator.SetBool("Run", isGrounded);
            }

            if (isDashing) return;

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Jump with Space
            if (keyboard.spaceKey.wasPressedThisFrame && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                if (animator != null) animator.SetTrigger("Jump");
            }

            // Dash with Left Shift
            if (keyboard.leftShiftKey.wasPressedThisFrame && Time.time >= lastDashTime + dashCooldown)
            {
                StartCoroutine(PerformDash());
            }
        }

        private IEnumerator PerformDash()
        {
            isDashing = true;
            lastDashTime = Time.time;

            float prevGravity = rb.gravityScale;
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(dashSpeed, 0f);

            yield return new WaitForSeconds(dashDuration);

            rb.gravityScale = prevGravity;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            isDashing = false;
        }

        private bool isInvulnerable = false;

        public void TakeDamage(int damage)
        {
            if (currentHealth <= 0 || isInvulnerable) return;

            currentHealth = Mathf.Max(0, currentHealth - damage);
            OnHealthChanged?.Invoke(currentHealth);

            if (currentHealth > 0)
            {
                if (animator != null) animator.SetTrigger("Hit");
                StartCoroutine(HitFlash());
            }
            else
            {
                Die();
            }
        }

        private IEnumerator HitFlash()
        {
            if (spriteRenderer == null) yield break;

            isInvulnerable = true;
            // Damage flash feedback
            for (int i = 0; i < 3; i++)
            {
                spriteRenderer.color = hitColor;
                yield return new WaitForSeconds(0.12f);
                spriteRenderer.color = normalColor;
                yield return new WaitForSeconds(0.12f);
            }
            isInvulnerable = false;
        }

        public void AddScore(int amount)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(amount);
            }
        }

        private void Die()
        {
            if (animator != null) animator.SetTrigger("Death");
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }
}
