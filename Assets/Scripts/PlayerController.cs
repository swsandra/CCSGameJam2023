using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

[RequireComponent(typeof(Rigidbody2D))]

public class PlayerController : MonoBehaviour
{

    Animator anim;
    Rigidbody2D rb;
    PlayerInput input;
    SpriteRenderer spriteRenderer;
    CinemachineImpulseSource impulse;
    [Header("Sounds")]
    [SerializeField] AudioSource walkingSource;
    [SerializeField] AudioClip swing;
    [SerializeField] AudioClip damage;
    [SerializeField] AudioClip dead;
    [Header("Health")]
    [SerializeField] float invulnerableDuration = 1f;
    public int health = 3;
    public int maxHealth = 3;
    bool invulnerable = false;

    [Header("Damage")]
    [SerializeField] float flashDuration = .09f;
    [SerializeField] Sprite hitSprite;
    [SerializeField] Material flashMaterial;
    Material originalMaterial;

    [Header("Movement")]
    [SerializeField] float speed = 1f;
    Vector2 currentMovement;
    bool canMove = true;

    [Header("Attack")]
    [SerializeField] Transform attackPoint;
    [SerializeField] float attackOffsetX = 1f;
    [SerializeField] float attackOffsetY = 1f;
    [SerializeField] float attackRange = 0.5f;
    [SerializeField] LayerMask enemyLayers;
    [SerializeField] float attackDelay = 1f;
    [SerializeField] float moveDelay = .5f;
    bool attackBoss = false;
    bool canAttack = true;


    private void Awake() {
        input = new PlayerInput();
        input.Player.Fire.performed += Attack;
        health = maxHealth;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        impulse = transform.GetComponent<CinemachineImpulseSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMaterial = spriteRenderer.material;
    }

    // Update is called once per frame
    void Update()
    {
        // Movement
        if (canMove) {
            currentMovement = input.Player.Move.ReadValue<Vector2>();
        }
    }

    void shakeCamera() {
        impulse.GenerateImpulse(0.5f);
    }

    void FixedUpdate() {
        // Movement
        rb.velocity = currentMovement * speed;

        // Move Animations
        if (currentMovement.x < 0) {
            spriteRenderer.flipX = true;
        } else if (currentMovement.x > 0){
            spriteRenderer.flipX = false;
        }
        anim.SetFloat("Vertical", currentMovement.y);
        anim.SetFloat("Speed", currentMovement.sqrMagnitude);

        // AttackPoint move and rotate
        if (currentMovement.magnitude != 0 && canAttack) {
            attackPoint.localPosition = new Vector3(spriteRenderer.flipX ? -attackOffsetX : attackOffsetX, attackOffsetY, 0);
        }

        if (currentMovement.magnitude > 0 && !walkingSource.isPlaying) {
            walkingSource.Play();
        } else if (currentMovement.magnitude == 0) {
            walkingSource.Stop();
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (!invulnerable && other.gameObject.tag == "Enemy") {
            shakeCamera();
            health = Mathf.Clamp(health-1, 0, 3);
            canMove = false;
            canAttack = false;
            currentMovement = Vector2.zero;
            UIManager.instance.UpdateLives();
            if (health <= 0) {
                StartCoroutine(flashRoutine());
                AudioSource.PlayClipAtPoint(dead, new Vector3(0,0, -10));
                anim.enabled = false;
                GetComponent<Collider2D>().enabled = false;
                spriteRenderer.sprite = hitSprite;
                walkingSource.Stop();
                GameManager.instance.GameOver();
                return;
            } else {
                AudioSource.PlayClipAtPoint(damage, new Vector3(0,0,-10));
            }
            invulnerable = true;
            StartCoroutine(InvulnerableCooldown());
            StartCoroutine(flashRoutine());
            StartCoroutine(DamageAnimationCooldown());
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "BossTrigger") {
            attackBoss = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.tag == "BossTrigger") {
            attackBoss = false;
        }
    }

    void Attack(InputAction.CallbackContext context) {
        if (canAttack) {
            // Set cooldowns
            canAttack = false;
            canMove = false;
            currentMovement = Vector2.zero;
            StartCoroutine(MoveCooldown());
            StartCoroutine(AttackCooldown());

            // Play Animation
            if (attackBoss) {
                anim.SetTrigger("AttackUp");
                shakeCamera();
                FindObjectOfType<BossController>().Damage();
            }
            else {
                anim.SetTrigger("Attack");
                // Detect enemies in range
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

                if (hitEnemies.Length == 0) { 
                    AudioSource.PlayClipAtPoint(swing, transform.position);
                }
                // Damage
                foreach(Collider2D enemy in hitEnemies) {
                    enemy.gameObject.GetComponent<Root>().Damage();
                }
            }
        }
    }

    private void OnDrawGizmosSelected() {
        if (attackPoint == null)
            return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    IEnumerator flashRoutine() {
        spriteRenderer.color = Color.red;
        spriteRenderer.material = flashMaterial;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = Color.white;
        spriteRenderer.material = originalMaterial;
    }

    IEnumerator DamageAnimationCooldown() {
        anim.enabled = false;
        spriteRenderer.sprite = hitSprite;
        yield return new WaitForSeconds(moveDelay);
        canMove = true;
        canAttack = true;
        anim.enabled = true;
    }

    IEnumerator InvulnerableCooldown() {
        yield return new WaitForSeconds(invulnerableDuration);
        invulnerable = false;
    }

    IEnumerator AttackCooldown() {
        yield return new WaitForSeconds(attackDelay);
        canAttack = true;
    }

    IEnumerator MoveCooldown() {
        yield return new  WaitForSeconds(moveDelay);
        canMove = true;
    }

    private void OnEnable() {
        input.Player.Enable();
    }

    private void OnDisable() {
        input.Player.Disable();
    }
}
