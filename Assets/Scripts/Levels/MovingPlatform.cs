using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Config")]
    [SerializeField] bool movementHorizontal;
    [SerializeField] bool movingToFinalPos = true;
    [SerializeField] float speed;
    [SerializeField] Vector3 moveRange;
    [SerializeField] Vector3 initialPosition;
    [SerializeField] Vector3 finalPosition;
    

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position - moveRange;
        finalPosition = transform.position + moveRange;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Move();
        CheckDirection();
    }

    private void CheckDirection()
    {
        if (movementHorizontal)
        {
            if (transform.position.x <= initialPosition.x)
            {
                movingToFinalPos = true;
            }
            if (transform.position.x >= finalPosition.x)
            {
                movingToFinalPos = false;
            }
        }
        else
        {
            if (transform.position.y <= initialPosition.y)
            {
                movingToFinalPos = true;
            }
            if (transform.position.y >= finalPosition.y)
            {
                movingToFinalPos = false;
            }
        }
    }

    private void Move()
    {
        float step = speed * Time.fixedDeltaTime;
        if (movingToFinalPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, finalPosition, step);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, initialPosition, step);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        other.transform.parent = transform;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        other.transform.parent = null;
    }
}
