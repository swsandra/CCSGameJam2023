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
            attackPoint.localPosition = currentMovement.normalized * attackOffset;
        }
    }

    void Attack(InputAction.CallbackContext context) {
        if (canAttack) {
            Debug.Log("Coñazo");
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
                // TODO: Do damage
            }
        }
        return;
    }

    private void OnDrawGizmosSelected() {
        if (attackPoint == null)
            return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    IEnumerator AttackCooldown() {
        yield return new WaitForSeconds(attackDelay);
        anim.SetBool("Attack", false);
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
