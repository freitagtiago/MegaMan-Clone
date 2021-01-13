using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [Header("Components")]
    Animator anim;
    Rigidbody2D rig;
    SpriteRenderer rend;
    [SerializeField] Collider2D boxCollider;
    [SerializeField] AudioSource audioSource;

    [Header("Control Variables")]
    [SerializeField] bool canMove = false;
    [SerializeField] float speed = 2000f;
    [SerializeField] float jumpForce = 350f;
    [SerializeField] float dashFactor = 1.2f;
    [SerializeField] float spawnSpeed;
    [SerializeField] bool canDash = true;
    [SerializeField] bool faceRight = false;
    [SerializeField] bool isDashing = false;
    [SerializeField] Vector3 bottomLeftLimit;
    [SerializeField] Vector3 topRightLimit;

    [Header("Input Variables")]
    [SerializeField] float verticalInput;
    [SerializeField] float horizontalInput;

    [Header("Ground Control")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] bool isGrounded = true;

    [Header("Climb Config")]
    [SerializeField] bool onLadder = false;
    [SerializeField] float climbSpeed = 3f;
    [SerializeField] LayerMask ladderLayer;
    [SerializeField] float checkRadius = 0.3f;
    [SerializeField] float distanceFromMiddleCollider = 1.2f;

    [Header("Audio")]
    [SerializeField] AudioClip jumpAudio;


    private void Awake()
    {
        anim = GetComponent<Animator>();
        rig = GetComponent<Rigidbody2D>();
        rend = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (boxCollider == null)
        {
            boxCollider = GetComponent<Collider2D>();
        }
    }

    private void Start()
    {
        
    }
    void FixedUpdate()
    {
        isGrounded = Physics2D.IsTouchingLayers(boxCollider, groundLayer);
        CheckInputs();

        if (canMove)
        {
            ProcessMovement();
            ProcessAnimation();
        }
    }

    private void CheckInputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void ProcessMovement()
    {
        ClimbLadder();
        GroundMovement();
    }

    private void ProcessAnimation()
    {
        if (Input.GetKey(KeyCode.LeftArrow) && !onLadder)
        {
            anim.SetBool("isMoving", true);
            rend.flipX = true;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && !onLadder)
        {
            anim.SetBool("isMoving", true);
            rend.flipX = false;
        }
        else
        {
            anim.SetBool("isMoving", false);
        }

        if (Input.GetKey(KeyCode.Space) && !onLadder)
        {
            anim.SetBool("isJumping", true);
            isGrounded = false;
        }
        else if (isGrounded && !onLadder)
        {
            anim.SetBool("isJumping", false);
        }
        if (Input.GetKey(KeyCode.N) && isDashing && isGrounded)
        {
            anim.SetBool("isDashing", true);
        }
        else
        {
            anim.SetBool("isDashing", false);
        }
        faceRight = rend.flipX;
    }

    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(1f);
        isDashing = false;
        yield return new WaitForSeconds(2f);
        canDash = true;
    }


    public void Spawn(Vector3 spawnPos)
    {
        transform.position = spawnPos;
        rig.AddForce(Vector2.down * spawnSpeed);
    }

    public void SetCanMove(bool value)
    {
        this.canMove = value;
    }

    public bool GetIsGrounded()
    {
        return isGrounded;
    }

    public bool GetOnLadder()
    {
        return onLadder;
    }

    public bool GetFaceRight()
    {
        return faceRight;
    }

    public void SetBounds(Vector3 bottomLeft, Vector3 topRight)
    {
        bottomLeftLimit = bottomLeft + new Vector3(1f, 1f, 0);
        topRightLimit = topRight + new Vector3(-1f, -1f, 0);
    }

    private void GroundMovement()
    {
        if (onLadder) return;

        float xVelocity = horizontalInput * speed * Time.fixedDeltaTime;
        float yVelocity = rig.velocity.y;

        if (isGrounded)
        {
            rig.velocity = (new Vector2(xVelocity, yVelocity));
        }
        else
        {
            rig.velocity = (new Vector2(xVelocity * 0.8f, rig.velocity.y));
        }
        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            rig.velocity = (new Vector2(xVelocity, jumpForce));

            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(jumpAudio);
            }
            
            isGrounded = false;
        }
        if (Input.GetKey(KeyCode.N) && canDash && isGrounded)
        {
            isDashing = true;
            canDash = false;
            rig.velocity = (new Vector2(horizontalInput, rig.velocity.y)) * (speed * dashFactor) * Time.fixedDeltaTime;
            StartCoroutine(DashCooldown());
        }
    }

    private void ClimbLadder()
    {
        bool ladderTop = Physics2D.OverlapCircle(transform.position + new Vector3(0, distanceFromMiddleCollider, 0), checkRadius,ladderLayer);
        bool ladderBottom = Physics2D.OverlapCircle(transform.position + new Vector3(0,-distanceFromMiddleCollider, 0),checkRadius,ladderLayer);


        if(verticalInput != 0 && TouchingLadder() && isGrounded)
        {
            onLadder = true;
            rig.isKinematic = true;
        }

        if (onLadder)
        {
            if (!ladderTop && verticalInput > 0f)
            {
                FinishClimb();
                return;
            }

            if(!ladderBottom && verticalInput < 0f)
            {
                FinishClimb();
                return;
            }
            rig.velocity = new Vector2(0, verticalInput * climbSpeed);
            anim.SetBool("isJumping", false);
            anim.SetBool("isClimbing", true);
            anim.SetFloat("climbingSpeed", verticalInput);
        }
    }

    private void FinishClimb()
    {
        onLadder = false;
        rig.isKinematic = false;
        anim.SetBool("isClimbing", false);
        anim.SetFloat("climbingSpeed", 0);
    }

    private bool TouchingLadder()
    {
        return boxCollider.IsTouchingLayers(ladderLayer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, distanceFromMiddleCollider, 0), checkRadius);
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, -distanceFromMiddleCollider, 0), checkRadius);
    }
}
