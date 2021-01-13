using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestruct : MonoBehaviour
{
    [SerializeField] float timeToDestroy = 1.5f;
    void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }
}
