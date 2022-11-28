using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float speed;                     // walk speed
    public float jumpForce;                 // vertical jump velocity (impulse)

    public float dashForce;                 // horizontal speed
    public float dashTime;                  // time in secs spent dashing
    public float dashCooldown;              // rate in secs player can dash
    private bool canDash = true;            // false if dash is on cooldown

    public float fallMultiplier = 2.5f;     // fast fall multiplier
    public float lowJumpMultiplier = 2f;    // low jump multiplier

    [SerializeField] private LayerMask jumpableGround;  
    private bool isGrounded = true;                   

    private bool facingRight = true;                   
    private bool canMove = true;                       
    private bool dead = false;                        

    /* COMPONENTS */
    private Rigidbody2D rb;         
    private SpriteRenderer sr;
    private Animator anim;
    private BoxCollider2D coll;

    /* AUDIO SOURCES */
    [SerializeField] private AudioSource jumpSound;
    [SerializeField] private AudioSource dashSound;
    [SerializeField] private AudioSource walkSound;
    [SerializeField] private AudioSource dieSound;

    /* STATE MACHINE */
    private enum State { idle, walk, jump }
    private State currentState;

    void Start()
    { 
        rb = GetComponent<Rigidbody2D>(); 
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        coll = GetComponent<BoxCollider2D>();

        currentState = State.idle;
    }

    void Update() 
    { 
        if (!canMove || dead)
            return;

        isGrounded = IsGrounded();
        float x = Move(); 
        Jump();
        JumpArc();
        Dash();
        UpdateAnimation(x);
        Flip(x);
    }

    private float Move()
    { 
        float x = Input.GetAxisRaw("Horizontal"); 
        float moveBy = x * speed; 
        rb.velocity = new Vector2(moveBy, rb.velocity.y); 

        return x;
    }

    void Jump()
    { 
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpSound.Play();
        }
    }

    public void Die()
    {
        if (dead)
            return;

        dead = true;
        dieSound.Play();
        anim.Play("die");
        Invoke("ReloadScene", 1);
    }

    public void StepSound()
    {
        walkSound.Play();
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Dash()
    {
        if (Input.GetButtonDown("Fire1") && canDash)
        {
            StartCoroutine(Dashing());
        }
    }

    private IEnumerator Dashing()
    {
        canDash = false;
        canMove = false;

        dashSound.Play();

        float gravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float x = facingRight ? 1f : -1f;
        rb.velocity = new Vector2(x * dashForce, 0f);
        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = gravity;
        canMove = true;
        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    void UpdateAnimation(float x)
    {
        if (!isGrounded)
            currentState = State.jump;
        else if (x < 0.01f && x > -0.01f)
            currentState = State.idle;
        else
            currentState = State.walk;

        switch(currentState)
        {
            case State.idle:
                anim.Play("idle");
                break;
            case State.walk:
                anim.Play("walk");
                break;
            case State.jump:
                anim.Play("fall");
                break;
            default:
                Debug.Log("Player state not found " + currentState);
                break;
        }
    }

    void JumpArc()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector2.up * Physics2D.gravity * (lowJumpMultiplier - 1) * Time.deltaTime;
        }   
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 0.1f, jumpableGround);
    }

    void Flip(float x)
    {
        if (x > 0f)
        {
            facingRight = true;
            sr.flipX = false;
        }
        else if (x < 0f)
        {
            facingRight = false;
            sr.flipX = true;
        }
    }
}
