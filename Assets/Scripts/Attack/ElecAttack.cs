using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElecAttack : MonoBehaviour
{
    [Header("Components")]
    Animator anim;
    [SerializeField] Collider2D boxCollider;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] SpriteRenderer rend;

    [Header("Config")]
    [SerializeField] bool changedState = false;
    [SerializeField] bool isGrounded = false;
    [SerializeField] float speed;
    [SerializeField] float attackRange;
    [SerializeField] int attackForce;
    [SerializeField] bool facingRight = false;
    [SerializeField] Vector3 attackTarget;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rend = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.IsTouchingLayers(boxCollider, groundLayer);
        if (isGrounded)
        {
            if(!changedState)
            {
                ChangeState();
            }
            Move();
            IsAtEnd();
        }
        else
        {
            if (changedState)
            {
                Destroy(gameObject);
            }
        }
    }

    private void ChangeState()
    {
        changedState = true;
        if (facingRight)
        {
            attackTarget = new Vector3(transform.position.x + attackRange, transform.position.y, transform.position.z);
        }
        else
        {
            attackTarget = new Vector3(transform.position.x - attackRange, transform.position.y, transform.position.z);
        }
        anim.SetTrigger("onGround");
    }

    private void Move()
    {
        float step = speed * Time.fixedDeltaTime;
        transform.position = Vector3.MoveTowards(transform.position, attackTarget, step);
    }

    private void IsAtEnd()
    {
        if(transform.position == attackTarget)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.GetComponent<IDamageable>().GetIsVulnerable())
            {
                other.GetComponent<IDamageable>().GetHit(attackForce);
            }
        }
        if (!other.gameObject.CompareTag("Enemy") && !other.gameObject.CompareTag("Ground") && !other.gameObject.CompareTag("Attack"))
        {
            Destroy(gameObject);
        }
    }

    public void Setup(bool value, float range)
    {
        facingRight = value;
        attackRange = range;
        if (facingRight)
        {
            rend.flipX = true;
        }
        else
        {
            rend.flipX = false;
        }
    }
}
