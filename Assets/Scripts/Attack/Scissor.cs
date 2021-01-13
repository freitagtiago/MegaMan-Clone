using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scissor : MonoBehaviour
{
    [Header("Movement")]
    [Range(0, 10)] [SerializeField] float rotateSpeed = 1f;
    [Range(0, 5)]  [SerializeField] float radius = 1f;
    [SerializeField] float[] yRange = new float[2];
    [SerializeField] Vector2 Velocity;
    [SerializeField] Vector2 centre;
    [SerializeField] float angle;
    [SerializeField] float initialXPos;

    [Header("Header")]
    [SerializeField] float attackRange = 10f;
    [SerializeField] float returnSpeed = 10f;
    [SerializeField] bool facingRight = false;
    [SerializeField] int attackForce;

    [Header("Components")]
    Transform emitter;
    IDamageable emitterHealth;

    [Header("Control Variables")]
    [SerializeField] bool isReturning = false;
    [SerializeField] bool canReturn = false;

    private void Start()
    {
        initialXPos = transform.position.x;
        centre = transform.position;
        StartCoroutine(SetCanReturn());
    }

    private void Update()
    {
        if (emitterHealth.GetIsDead())
        {
            Destroy(gameObject);
        }

        if (!NeedToReturn() && !isReturning)
        {
            centre += Velocity * Time.deltaTime;

            angle += rotateSpeed * Time.deltaTime;

            var offset = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * radius;

            transform.position = centre + offset;
        }
        else
        {
            float step = returnSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, emitter.position, step);
        }
    }

    private bool NeedToReturn()
    {
        if (facingRight)
        {
            if (initialXPos + attackRange < transform.position.x)
            {
                isReturning = true;
                return true;
            }
        }
        if (!facingRight)
        {
            if (initialXPos - attackRange > transform.position.x)
            {
                isReturning = true;
                return true;
            }
        }
        return false;
    }

    private IEnumerator SetCanReturn()
    {
        yield return new WaitForSeconds(1f);
        canReturn = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Cutman") && canReturn)
        {
            other.GetComponent<Cutman>().ResetScissor();
            Destroy(gameObject);
            return;
        }
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.GetComponent<IDamageable>().GetIsVulnerable())
            {
                other.GetComponent<IDamageable>().GetHit(attackForce);
            }
        }
    }

    public void Setup(Transform origin, float direction, int attackDamage)
    {
        emitter = origin;
        emitterHealth = emitter.GetComponent<IDamageable>();
        Velocity.x *= direction;
        Velocity.y = Random.Range(yRange[0], yRange[1]);
        if(direction == -1)
        {
            facingRight = false;
        }
        else
        {
            facingRight = true;
        }
        if(attackDamage > 0)
        {
            attackForce = attackDamage;
        }
    }
}
