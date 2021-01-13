using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElecEnemy : MonoBehaviour, IDamageable, IItemSpawn
{
    [Header("Components")]
    Animator anim;
    SpriteRenderer rend;
    Transform player;
    AudioSource audioSource;

    [Header("Attack Config")]
    [SerializeField] GameObject attackPrefab;
    [SerializeField] int attackForce;
    [SerializeField] bool canAttack;
    [SerializeField] bool isAttacking;
    [SerializeField] float timeBetweenAttacks;
    [SerializeField] float chaseRange;
    [SerializeField] float attackRange;

    [Header("Health Config")]
    [SerializeField] int currentHealth;
    [SerializeField] bool isVulnerable;
    [SerializeField] bool isDead = false;
    [SerializeField] float invulnerabilityTime;
    [SerializeField] GameObject deathFX;

    [Header("Config")]
    [SerializeField] bool facingRight = false;
    [SerializeField] bool isHurt;
    [SerializeField] Color baseColor;
    [SerializeField] Color blinkedColor;
    [SerializeField] int pointsWhenKilled = 500;

    [Header("Item Config")]
    [SerializeField] GameObject itemToSpawnPrefab;
    [SerializeField] int valueToHeal;

    [Header("Audio")]
    [SerializeField] AudioClip hitAudio;
    [SerializeField] AudioClip shootAudio;
    [SerializeField] AudioClip explosionAudio;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        rend = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        FindPlayer();
        SetColors();
    }

    void Update()
    {
        IsAlive();
        FacePlayer();
        CheckDistanceFromPlayer();
        if (canAttack && !isAttacking)
        {
            Attack();
        }
        HurtAnimation();
    }

    private void FindPlayer()
    {
        player = FindObjectOfType<Mover>().transform;
    }

    private void FacePlayer()
    {
        if (player.position.x >= transform.position.x)
        {
            facingRight = true;
            rend.flipX = true;
        }
        if (player.position.x <= transform.position.x)
        {
            facingRight = false;
            rend.flipX = false;
        }
    }

    private void CheckDistanceFromPlayer()
    {
        if(Vector3.Distance(player.position, transform.position) <= chaseRange)
        {
            canAttack = true;
        }
        else
        {
            canAttack = false;
        }
    }

    private void Attack()
    {
        isAttacking = true;
        anim.SetTrigger("attack");
        StartCoroutine(IsAttacking());
        if (facingRight)
        {
            GameObject attack = Instantiate(attackPrefab, new Vector3(transform.position.x + 0.8f, transform.position.y, 0), Quaternion.identity);
            attack.GetComponent<ElecAttack>().Setup(facingRight, attackRange);
        }
        else
        {
            Instantiate(attackPrefab, new Vector3(transform.position.x - 0.8f, transform.position.y, 0), Quaternion.identity);
        }
        audioSource.PlayOneShot(shootAudio);
    }

    private IEnumerator IsAttacking()
    {
        yield return new WaitForSeconds(timeBetweenAttacks);
        isAttacking = false;
    }

    private void IsAlive()
    {
        if(currentHealth <= 0 && !isDead)
        {
            isDead = true;
            Die();
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

    public void GetHit(int damage)
    {
        currentHealth -= damage;
        audioSource.PlayOneShot(hitAudio);
        StartCoroutine(ManageVulnerability());
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
