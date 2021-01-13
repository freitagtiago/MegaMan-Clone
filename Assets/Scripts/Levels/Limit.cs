using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Limit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable health = other.GetComponent<IDamageable>();

        if (health != null)
        {
            health.GetHit(100);
        }
    }
}
