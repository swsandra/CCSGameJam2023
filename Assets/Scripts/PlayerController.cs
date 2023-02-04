using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]

public class PlayerController : MonoBehaviour
{

    Animator anim;
    Rigidbody2D rb;
    PlayerInput input;
    SpriteRenderer spriteRenderer;

    [Header("Health")]
    int health = 3;
    bool invulnerable = false;
    [SerializeField] float invulnerableDuration = 1f;

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
    bool canAttack = true;
    float bossAttack = 0f;


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
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Movement
        if (canMove) {
            currentMovement = input.Player.Move.ReadValue<Vector2>();
        }
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
            attackPoint.localPosition = new Vector3(spriteRenderer.flipX ? -1 : 1, bossAttack, 0).normalized * attackOffset;
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (!invulnerable && other.gameObject.tag == "Enemy") {
            health -= 1;
            Debug.Log(health);
            canMove = false;
            canAttack = false;
            currentMovement = Vector2.zero;
            if (health <= 0) {
                return;
                //TODO: TRIGGER GAME OVER
            }
            // TODO: TRIGGER ANIMATION/EFFECT
            invulnerable = true;
            StartCoroutine(InvulnerableCooldown());
            StartCoroutine(DamageAnimationCooldown());
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        Debug.Log("TriggerEnter");
        if (other.gameObject.tag == "BossTrigger") {
            bossAttack = 1f;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        Debug.Log("TriggerExit");
        if (other.gameObject.tag == "BossTrigger") {
            bossAttack = 0f;
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
            anim.SetTrigger("Attack");

            // Detect enemies in range
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

            // Damage
            foreach(Collider2D enemy in hitEnemies) {
                Debug.Log("HIT: " + enemy.name);
                // if (enemy.name == "Boss")
                //     enemy.gameObject.GetComponent<BossController>().Damage();
                // else
                    enemy.gameObject.GetComponent<Root>().Damage();
            }
        }
    }

    private void OnDrawGizmosSelected() {
        if (attackPoint == null)
            return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    IEnumerator DamageAnimationCooldown() {
        yield return new WaitForSeconds(moveDelay);
        canMove = true;
        canAttack = true;
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
