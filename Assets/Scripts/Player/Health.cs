using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] int currentHealth;
    [SerializeField] int maxHealth = 20;
    [SerializeField] Color baseColor;
    [SerializeField] Color blinkedColor;

    [Header("Control Variables")]
    [SerializeField] bool isVulnerable = true;
    [SerializeField] bool isHurt = false;
    [SerializeField] float timeBetweenHits;
    [SerializeField] bool isDead = false;

    [Header("Components")]
    [SerializeField] UIHealth healthBar;
    [SerializeField] ParticleSystem deathFX;
    [SerializeField] SpriteRenderer rend;
    [SerializeField] AudioSource audioSource;

    [Header("Audio")]
    [SerializeField] AudioClip healAudio;
    [SerializeField] AudioClip hitAudio;
    [SerializeField] AudioClip explosionAudio;

    private void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        FindPlayerHealthBar();
    }
    
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.UpdateHealthBar(currentHealth);
        SetColors();
    }

    void Update()
    {
        if(currentHealth <= 0)
        {
            GameOver();
        }
        HurtAnimation();
    }

    private void SetColors()
    {
        baseColor = rend.color;
        blinkedColor = new Color(rend.color.r, rend.color.g, rend.color.b, 0);
    }

    private void FindPlayerHealthBar()
    {
        foreach(UIHealth ui in FindObjectsOfType<UIHealth>())
        {
            if (ui.GetIsPlayerBar())
            {
                healthBar = ui;
                break;
            }
        }
    }

    private void GameOver()
    {
        if (!isDead)
        {
            isDead = true;
            rend.enabled = false;
            GameObject particles = Instantiate(deathFX, transform.position, Quaternion.identity).gameObject;
            AudioSource.PlayClipAtPoint(explosionAudio, transform.position);
            Destroy(particles, 1f);
        }  
    }

    private IEnumerator ManageVulnerability()
    {
        isVulnerable = false;
        isHurt = true;
        yield return new WaitForSecondsRealtime(timeBetweenHits);
        isVulnerable = true;
        isHurt = false;
    }

    private void HurtAnimation()
    {
        if (!isHurt)
        {
            if(rend.color.a != 255)
            {
                rend.color = baseColor;
            }
            return;
        }    
        if(rend.color.a == 0)
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
        if (isVulnerable)
        {
            StartCoroutine(ManageVulnerability());
        }
        currentHealth -= damage;
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(hitAudio);
        }
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        healthBar.UpdateHealthBar(currentHealth);
    }

    public bool GetIsVulnerable()
    {
        return isVulnerable;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public bool GetIsDead()
    {
        return currentHealth > 1;
    }

    public void Heal(int value)
    {
        currentHealth = Mathf.Clamp(currentHealth + value, currentHealth, maxHealth);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(healAudio);
        }
        healthBar.UpdateHealthBar(currentHealth);
    }
}
