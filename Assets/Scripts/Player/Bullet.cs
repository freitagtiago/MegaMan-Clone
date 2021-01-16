using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Control Variables")]
    [SerializeField] int hitDamage = 1;
    [SerializeField] float speed = .1f;
    [SerializeField] float maxRange = 50f;
    [SerializeField] Vector3 direction;

    [Header("Components")]
    IDamageable damageable;
    [SerializeField] Mover mover;

    private void Awake()
    {
        
    }

    private void Start()
    {
        Destroy(gameObject, .6f);
        mover = FindObjectOfType<Mover>();
        if (mover.GetFaceRight())
        {
            maxRange *= -1;
        }
       
        direction = new Vector3(transform.position.x + maxRange
                                , transform.position.y
                                , transform.position.z);
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, direction, speed);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Attack"))
        {
            damageable = other.GetComponent<IDamageable>();

            if(damageable != null)
            {
                if (damageable.GetIsVulnerable())
                {
                    damageable.GetHit(hitDamage);
                }
            }
            Destroy(gameObject);
        }
    }
}
