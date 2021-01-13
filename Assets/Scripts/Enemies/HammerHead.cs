using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerHead : MonoBehaviour, IDamageable, IItemSpawn
{
    [Header("Control Variables")]
    [SerializeField] bool inChaseMode = false;
    [SerializeField] bool canAttack;
    [SerializeField] bool canMove = false;
    [SerializeField] bool isMoving = false;
    [SerializeField] bool inAttackRange = false;
    [SerializeField] bool isGrounded;
    [SerializeField] bool goingToRight = true;
    [SerializeField] bool isVulnerable = true;
    [SerializeField] bool isHurt = false;
    
    [Header("Config")]
    [SerializeField] int maxHealth;
    [SerializeField] int currentHealth;
    [SerializeField] int attackForce;
    [SerializeField] float speed;
    [SerializeField] float attackRange;
    [SerializeField] float chaseRange;
    [SerializeField] float invulnerabilityTime;
    [SerializeField] GameObject deathFX;
    [SerializeField] Color baseColor;
    [SerializeField] Color blinkedColor;
    [SerializeField] int pointsWhenKilled = 250;

    [Header("Components")]
    [SerializeField] Transform player;
    [SerializeField] GameManager gameManager;
    [SerializeField] Animator anim;
    [SerializeField] Rigidbody2D rig;
    [SerializeField] SpriteRenderer rend;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Collider2D boxCollider;
    [SerializeField] AudioSource audioSource;

    [Header("Patrol")]
    [SerializeField] float[] pointToFollowOnX = new float[2];
    [SerializeField] float middlePointOnX;
    [SerializeField] float patrolRange;

    [Header("Item Config")]
    [SerializeField] GameObject itemToSpawnPrefab;
    [SerializeField] int valueToHeal;

    [Header("Audio")]
    [SerializeField] AudioClip hitAudio;
    [SerializeField] AudioClip explosionAudio;
    [SerializeField] AudioClip shootAudio;

    private void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rend = GetComponent<SpriteRenderer>();
        gameManager = FindObjectOfType<GameManager>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        FindPlayer();
        SetPatrolPath();
        currentHealth = maxHealth;
        SetColors();
    }

    void Update()
    {
        isGrounded = Physics2D.IsTouchingLayers(boxCollider, groundLayer);
        CheckDistanceToChase();
        if (canMove && !inChaseMode && isGrounded)
        {
            Patrol();
        }else if(inChaseMode)
        {
            if (canMove && isGrounded)
            {
                Move(player.position.x);
            }
            else
            {
                isMoving = false;
            }
            CheckDistanceToAttack();
            if (inAttackRange)
            {
                Attack();
            }
        }
        ProcessAnimation();
    }

    private void FindPlayer()
    {
        player = FindObjectOfType<Mover>().transform;
    }
    private void SetPatrolPath()
    {
        middlePointOnX = transform.position.x;
        pointToFollowOnX[0] = middlePointOnX - patrolRange;
        pointToFollowOnX[1] = middlePointOnX + patrolRange;
    }

    private void Move(float targetXpos)
    {
        isMoving = true;

        float direction;

        if(targetXpos < transform.position.x)
        {
            direction = -1;
            rend.flipX = false;
        }
        else
        {
            direction = 1;
            rend.flipX = true;
        }

        rig.velocity = (new Vector2(direction, 0)) * speed * Time.fixedDeltaTime;
    }

    private void Patrol()
    {
        if (goingToRight)
        {
            Move(pointToFollowOnX[1]);
            if(transform.position.x >= pointToFollowOnX[1])
            {
                goingToRight = false;
            }
        }
        else
        {
            Move(pointToFollowOnX[0]);
            if (transform.position.x <= pointToFollowOnX[0])
            {
                goingToRight = true;
            }
        }
    }

    private void CheckDistanceToChase()
    {
        if (!gameManager.GetIsPlayerAlive())
        {
            PlayerIsDead();
            return;
        }

        if (Vector2.Distance(player.position, transform.position) <= chaseRange)
        {
            inChaseMode = true;
        }
        else
        {
            inChaseMode = false;
        }
    }

    private void CheckDistanceToAttack()
    {
        if (!gameManager.GetIsPlayerAlive())
        {
            PlayerIsDead();
            return;
        }
        if (Vector2.Distance(player.position, transform.position) < attackRange)
        {
            inAttackRange = true;
        }
        else
        {
            inAttackRange = false;
            canMove = true;
        }
    }

    private void Attack()
    {
        canAttack = true;
        canMove = false;
    }

    private IEnumerator ManageVulnerability()
    {
        isVulnerable = false;
        isHurt = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isVulnerable = true;
        isHurt = false;
    }

    private void SetColors()
    {
        baseColor = rend.color;
        blinkedColor = new Color(rend.color.r, rend.color.g, rend.color.b, 0);
    }

    private void HurtAnimation()
    {
        if (!isHurt)
        {
            if (rend.color.a != 255)
            {
                rend.color = baseColor;
            }
            return;
        }
        if (rend.color.a == 0)
        {
            rend.color = baseColor;
        }
        else
        {
            rend.color = blinkedColor;
        }
    }

    private void Die()
    {
        SpawnItem();
        FindObjectOfType<GameManager>().AddPoints(pointsWhenKilled);
        Instantiate(deathFX, transform.position, Quaternion.identity);
        audioSource.PlayOneShot(explosionAudio);
        Destroy(gameObject,0.1f);
    }

    private void PlayerIsDead()
    {
        inAttackRange = false;
        canMove = true;
        inChaseMode = false;
    }

    private void ProcessAnimation()
    {
        if (!canMove)
        {
            isMoving = false;
            anim.SetBool("isWalking", false);
        }
        if (isMoving)
        {
            anim.SetBool("isWalking", true);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }

        if (canAttack)
        {
            anim.SetTrigger("attack");
            canAttack = false;
        }
        HurtAnimation();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.GetComponent<IDamageable>().GetIsVulnerable())
            {
                other.GetComponent<IDamageable>().GetHit(attackForce);
            }
        }        
    }

    public void TriggerBehaviour(bool value)
    {
        canMove = value;
    }

    public void GetHit(int damage)
    {
        currentHealth -= damage;
        audioSource.PlayOneShot(hitAudio);
        StartCoroutine(ManageVulnerability());
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public bool GetIsVulnerable()
    {
        return isVulnerable;
    }

    public bool GetIsDead()
    {
        return currentHealth > 1;
    }

    public void SpawnItem()
    {
        var item = Instantiate(itemToSpawnPrefab, transform.position, Quaternion.identity);
        HealthPickup pickup = item.GetComponent<HealthPickup>();
        if (pickup != null)
        {
            pickup.Setup(valueToHeal);
        }
    }
}
