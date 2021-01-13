using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cutman : MonoBehaviour, IActivate, IDamageable, IItemSpawn
{
    private enum state{
            Iddle
            , Walking
            , Attacking
            , Jumping
            , Dead
        }
    [SerializeField] state currentState = state.Iddle;

    [Header("Components")]
    Transform targetPos;
    SpriteRenderer rend;
    Animator anim;
    Rigidbody2D rig;
    
    [Header("Collision")]
    [SerializeField] Collider2D wallCollider;
    [SerializeField] Collider2D groundCollider;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] LayerMask groundLayer;

    [Header("Config")]
    [SerializeField] int direction = -1;
    [SerializeField] float speed;
    [SerializeField] float jumpForce;
    [SerializeField] float iddleTime = 0.4f;
    [SerializeField] float targetRange = 2f;
    [SerializeField] float jumpChance = 50f;
    [SerializeField] float timeBetweenJumps = 1.5f;
    [SerializeField] int pointsWhenKilled = 2000;

    [Header("Attack Config")]
    [SerializeField] float attackMargin = 0.3f;
    [SerializeField] float timeToTriggerAttack = 2f;
    [SerializeField] float timeBetweenAttacks;
    [SerializeField] int attackForce;
    [SerializeField] GameObject scissorPrefab;
    GameObject scissor;

    [Header("Control Variable")]
    [SerializeField] bool inBattle = false;
    [SerializeField] bool canAttack = false;
    [SerializeField] bool canMove = false;
    [SerializeField] bool isGrounded = true;
    [SerializeField] bool isScissorless = false;
    [SerializeField] bool canJump = true;

    [Header("Health Config")]
    [SerializeField] int maxHealth;
    [SerializeField] int currentHealth;
    [SerializeField] bool isVulnerable;
    [SerializeField] bool isHurt;
    [SerializeField] float invulnerabilityTime;
    [SerializeField] GameObject deathFX;
    [SerializeField] Color baseColor;
    [SerializeField] Color blinkedColor;
    [SerializeField] bool isDead = false;
    [SerializeField] UIHealth healthBar;
    [SerializeField] float minBarHeight;

    [Header("Item Config")]
    [SerializeField] GameObject itemToSpawnPrefab;
    [SerializeField] int valueToHeal;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip hitAudio;
    [SerializeField] AudioClip shootAudio;
    [SerializeField] AudioClip jumpAudio;
    [SerializeField] AudioClip explosionAudio;

    private void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rig = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        FindPlayer();
        FindHealthBar();
        SetupState();
        currentHealth = maxHealth;
        StartCoroutine(TriggerAttack());
    }

    void FixedUpdate()
    {
        if (!inBattle) return;

        isGrounded = Physics2D.IsTouchingLayers(groundCollider, groundLayer);
        LookPlayer();

        if(currentHealth <= 0)
        {
            currentState = state.Dead;
        }

        if (isGrounded && !isDead)
        {
            if (canMove)
            {
                if (!CheckDistanceFromTarget())
                {
                    if (Random.Range(0, 100) < jumpChance && canJump)
                    {
                        currentState = state.Jumping;
                    }
                    else
                    {
                        currentState = state.Iddle;
                    }
                }
                else
                {
                    currentState = state.Walking;
                }
            }

            if (currentState == state.Attacking && !canAttack)
            {
                currentState = state.Iddle;
            }
            else
            {
                if (!isScissorless && canAttack && currentState != state.Jumping)
                {
                    currentState = state.Attacking;
                }
            }

        }
        HandleState();
    }

    private void SetupState()
    {
        anim.SetBool("isScissorless", false);
        anim.SetBool("isIddle", true);
        currentState = state.Iddle;
    }

    private void FindHealthBar()
    {
        foreach (UIHealth ui in FindObjectsOfType<UIHealth>())
        {
            if (!ui.GetIsPlayerBar())
            {
                healthBar = ui;
                break;
            }
        }
        healthBar.GetComponent<LayoutElement>().minHeight = minBarHeight;
        healthBar.UpdateHealthBar(maxHealth);
        healthBar.gameObject.SetActive(false);
    }

    private void HandleState()
    {
        if (currentState == state.Iddle)
        {
            anim.SetBool("isIddle", true);
            anim.SetBool("isWalking", false);
        }
        if (currentState == state.Walking)
        {
            anim.SetBool("isWalking", true);
            anim.SetBool("isIddle", false);
            Move();
        }

        if (currentState == state.Jumping)
        {
            if (canJump)
            {
                Jump();
                anim.SetBool("isJumping", true);
                anim.SetBool("isWalking", false);
                anim.SetBool("isIddle", false);
            }
        }
        if (currentState == state.Attacking)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isIddle", false);
            anim.SetTrigger("attack");  
            Attack();
            StartCoroutine(SetScissorless());
        }
        if (currentState == state.Dead && !isDead)
        {
            isDead = true;
            anim.SetTrigger("isDead");
            Die();
        }
    }

    private IEnumerator SetScissorless()
    {
        yield return new WaitForSeconds(.3f);
        anim.SetBool("isScissorless", true);
    }
    private void FindPlayer()
    {
        targetPos = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Move()
    {
        float moveDirection = direction * targetRange;
        float step = speed * Time.fixedDeltaTime;
        Vector3 target = new Vector3(targetPos.position.x + moveDirection, transform.position.y, transform.position.z); 
        transform.position = Vector3.MoveTowards(transform.position, target, step);
    }

    private void LookPlayer()
    {
        if(targetPos.position.x < transform.position.x)
        {
            rend.flipX = true;
            direction = -1;
        }
        if (targetPos.position.x > transform.position.x)
        {
            rend.flipX = false;
            direction = 1;
        }
    }

    private bool CheckDistanceFromTarget()
    {
        float distanceFromPlayer = Vector3.Distance(targetPos.position, transform.position);
        if (distanceFromPlayer > targetRange + attackMargin)
        {
            return true;
        }
        return false;
    }

    private void Attack()
    {
        if (isScissorless || !canAttack) return;

        canMove = false;
        canAttack = false;
        isScissorless = true;

        GameObject scissor = Instantiate(scissorPrefab, transform.position, Quaternion.identity);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(shootAudio);
        }
        scissor.GetComponent<Scissor>().Setup(transform,direction, attackForce);
        StartCoroutine(ReturnToMove());
    }

    private IEnumerator TriggerAttack()
    {
        yield return new WaitForSeconds(timeToTriggerAttack);
        canAttack = true;
    }
    private IEnumerator ReturnToMove()
    {
        canMove = false;
        yield return new WaitForSeconds(iddleTime);
        canMove = true;
    }

    private IEnumerator TriggerJump()
    {
        canJump = false;
        yield return new WaitForSeconds(timeBetweenJumps);
        canJump = true;
    }

    private void Jump()
    {
        StartCoroutine(TriggerJump());
        rig.velocity = (new Vector2(rig.velocity.x, jumpForce));
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(jumpAudio);
        }
    }

    private void PlayerIsDead()
    {
        inBattle = false; ;
    }
    private IEnumerator ActivationRoutine(float value)
    {
        yield return new WaitForSeconds(value);
        inBattle = true;
        healthBar.gameObject.SetActive(true);
    }

    private IEnumerator ManageVulnerability()
    {
        isVulnerable = false;
        isHurt = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isVulnerable = true;
        isHurt = false;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            anim.SetBool("isJumping", false);
            currentState = state.Iddle;
        }
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
        Instantiate(deathFX, transform.position, Quaternion.identity);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(explosionAudio);
        }
        GameManager gm = FindObjectOfType<GameManager>();
        gm.AddPoints(pointsWhenKilled);
        gm.PlayVictoryMusic();
        if(scissor != null)
        {
            Destroy(scissor.gameObject);
        }
        Destroy(gameObject, 0.1f);
    }

    public void ResetScissor()
    {
        isScissorless = false;
        anim.SetBool("isScissorless", false);
        canAttack = true;
    }

    public void Activate(float value)
    {
        StartCoroutine(ActivationRoutine(value));
    }

    public void GetHit(int damage)
    {
        currentHealth -= damage;
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(hitAudio);
        }
        healthBar.UpdateHealthBar(currentHealth);
        StartCoroutine(ManageVulnerability());
        if (currentHealth <= 0)
        {
            currentState = state.Dead;
        }
    }

    public bool GetIsVulnerable()
    {
        return isVulnerable;
    }

    public bool GetIsDead()
    {
        return currentHealth < 1;
    }

    public void SpawnItem()
    {
        if(itemToSpawnPrefab != null)
        {
            var item = Instantiate(itemToSpawnPrefab, transform.position, Quaternion.identity);
            HealthPickup pickup = item.GetComponent<HealthPickup>();
            if(pickup != null)
            {
                pickup.Setup(valueToHeal);
            }
        }
    }
}
