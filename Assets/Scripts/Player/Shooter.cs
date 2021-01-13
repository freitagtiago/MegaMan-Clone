using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [Header("Control Variables")]
    [SerializeField] bool canShoot;
    [SerializeField] float cooldownTime = 0.5f;

    [Header("Components")]
    [SerializeField] Animator anim;
    [SerializeField] GameObject bullet;
    [SerializeField] Mover mover;
    [SerializeField] AudioSource audioSource;

    [Header("Audio")]
    [SerializeField] AudioClip shootAudio;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        mover = GetComponent<Mover>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (canShoot)
        {
            if (Input.GetKeyDown(KeyCode.B) && !mover.GetOnLadder())
            {
                ProcessShoot();
                ProcessAnimation();
            }
        }
        
    }

    private IEnumerator CoolDown()
    {
        SetCanShoot(false);
        yield return new WaitForSeconds(cooldownTime);
        SetCanShoot(true);
    }

    private void ProcessShoot()
    {
        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        if (mover.GetFaceRight())
        {
            spawnPos.x -= 1f;
        }
        else
        {
            spawnPos.x += 1f;
        }
        Instantiate(bullet, spawnPos, Quaternion.identity);
        audioSource.PlayOneShot(shootAudio);
    }

    private void ProcessAnimation()
    {
        anim.SetTrigger("shooting");
    }

    public void SetCanShoot(bool value)
    {
        canShoot = value;
    }
}
