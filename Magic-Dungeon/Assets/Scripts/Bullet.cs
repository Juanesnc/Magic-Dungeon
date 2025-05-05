using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    public LayerMask whatIsLight;
    public LayerMask whatIsAir;
    public int bulletDamage = 20;
    private float maxDistance;
    private Vector3 startPosition;
    private Rigidbody rb;
    private float bounceDuration = 5f;

    private void Start()
    {
        maxDistance = 80f;
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Vector3.Distance(startPosition, transform.position) > maxDistance) DestroyBullet();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (whatIsLight == (whatIsLight | (1 << other.gameObject.layer)) || whatIsAir == (whatIsAir | (1 << other.gameObject.layer)))
        {
            return;
        }
        if (other.CompareTag("Enemy"))
        {
            EnemyAI enemyAI = other.GetComponent<EnemyAI>();
            enemyAI.TakeDamage(bulletDamage);
        }
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMovement = other.GetComponentInParent<PlayerMovement>();

            HealthPlayer healthPlayer = other.GetComponent<HealthPlayer>();
            if (healthPlayer)
                healthPlayer.TakeDamage(bulletDamage);
        }
        //DestroyBullet();
    }

    private void OnCollisionEnter(Collision other)
    {
        Vector3 bounceDirection = Vector3.Reflect(rb.velocity.normalized, other.contacts[0].normal);
        rb.velocity = bounceDirection * rb.velocity.magnitude;

        StartCoroutine(DestroyAfterBounce());

        if (other.gameObject.CompareTag("Player"))
        {
            PlayerMovement playerMovement = other.gameObject.GetComponentInParent<PlayerMovement>();

            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();

            if (!playerMovement.gameOver)
            {
                Vector3 forwardXZ = new Vector3(transform.forward.x, transform.forward.y, transform.forward.z);

                rb.AddForce(transform.up * -32f, ForceMode.Impulse);
                rb.AddForce(forwardXZ * 10f, ForceMode.Impulse);

                if (!playerMovement.restricted)
                {
                    HealthPlayer healthPlayer = other.gameObject.GetComponentInChildren<HealthPlayer>();

                    if (healthPlayer)
                        healthPlayer.TakeDamage(bulletDamage);

                    DestroyBullet();
                }
            }
        }
    }

    private void DestroyBullet()
    {
        Destroy(gameObject);
    }

    private IEnumerator DestroyAfterBounce()
    {
        yield return new WaitForSecondsRealtime(bounceDuration);
        DestroyBullet();
    }
}
