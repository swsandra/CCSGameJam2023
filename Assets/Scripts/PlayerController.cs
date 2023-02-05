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

    [Header("Health")]
    int health = 3;
    bool invulnerable = false;
    [SerializeField] float invulnerableDuration = 1f;

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
    [SerializeField] float attackOffset = 1f;
    [SerializeField] float attackRange = 0.5f;
    [SerializeField] LayerMask enemyLayers;
    [SerializeField] float attackDelay = 1f;
    [SerializeField] float moveDelay = .5f;
    bool attackBoss = false;
    bool canAttack = true;


    private void Awake() {
        input = new PlayerInput();

        // input.Player.Move.performed += Movement;

        input.Player.Fire.performed += Attack;
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
            attackPoint.localPosition = new Vector3(spriteRenderer.flipX ? -1 : 1, 0, 0).normalized * attackOffset;
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (!invulnerable && other.gameObject.tag == "Enemy") {
            shakeCamera();
            health -= 1;
            Debug.Log(health);
            canMove = false;
            canAttack = false;
            currentMovement = Vector2.zero;
            if (health <= 0) {
                return;
                //TODO: TRIGGER GAME OVER
            }
            invulnerable = true;
            StartCoroutine(InvulnerableCooldown());
            StartCoroutine(flashRoutine());
            StartCoroutine(DamageAnimationCooldown());
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        Debug.Log("TriggerEnter");
        if (other.gameObject.tag == "BossTrigger") {
            attackBoss = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        Debug.Log("TriggerExit");
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
                Debug.Log("HIT: BOSS");
                // FindObjectOfType<BossController>().TakeDamage();
            }
            else {
                anim.SetTrigger("Attack");
                // Detect enemies in range
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

                // Damage
                foreach(Collider2D enemy in hitEnemies) {
                    Debug.Log("HIT: " + enemy.name);
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
