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


    private void Awake() {
        input = new PlayerInput();

        // input.Player.Move.performed += ctx => {
        //    currentMovement = ctx.ReadValue<Vector2>();
        // };
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
        currentMovement = input.Player.Move.ReadValue<Vector2>();

        // Animations
        if (currentMovement.x < 0) {
            spriteRenderer.flipX = true;
        } else if (currentMovement.x > 0){
            spriteRenderer.flipX = false;
        }
        anim.SetFloat("Vertical", currentMovement.y);
        anim.SetFloat("Speed", currentMovement.sqrMagnitude);
    }

    void FixedUpdate() {
        rb.velocity = currentMovement * speed;
    }

    private void OnEnable() {
        input.Player.Enable();
    }

    private void OnDisable() {
        input.Player.Disable();
    }
}
