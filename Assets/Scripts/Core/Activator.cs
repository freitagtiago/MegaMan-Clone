using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activator : MonoBehaviour
{
    [SerializeField] GameObject objectToTrigger;
    [SerializeField] float timeToActivate;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if(objectToTrigger == null)
            {
                Debug.LogError("Object to trigger is not setted");
            }
            else
            {
                objectToTrigger.GetComponent<IActivate>().Activate(timeToActivate);
                FindObjectOfType<GameManager>().PlayBossMusic();
            }
        }

        
    }
}
