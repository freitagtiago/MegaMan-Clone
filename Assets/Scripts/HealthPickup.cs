using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Heal Info")]
    [SerializeField] int amountToHeal;

    public void Setup(int value)
    {
        amountToHeal = value;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<Health>().Heal(amountToHeal);
            Destroy(gameObject);
        }
    }
}
