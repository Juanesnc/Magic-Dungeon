using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public LayerMask whatIsGround, whatIsPlayer;
    public int health;
    Material material;

    private NavMeshAgent agent;
    private Vector3 lastValidPosition;
    private Coroutine currentCoroutine;
    private PlayerMovement pm;
    private BoxCollider boxCollider;
    private Transform player;

    [Header("Patroling")]
    public int rutina = 1;
    public float cronometro;
    public Quaternion angulo;
    public float grado;

    private float stuckTimer = 0f;
    private float stuckThreshold = 2f;
    private Vector3 lastPosition;

    [Header("Attacking")]
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;
    public bool trowWeb;

    [Header("States")]
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private Vector3 enemiLocation;
    private bool attacked = false;
    private bool isFollow = false;
    private AttackState state;
    private Coroutine attackCoroutine;
    private Coroutine chargeAttackCoroutine;
    private Coroutine resetAttackCoroutine;
    private bool launchWeb;
    private Animator anim;
    private bool live;

    public enum AttackState
    {
        weapon,
        meele
    }

    private void Awake()
    {
        player = GameObject.Find("PlayerObj").transform;
        pm = GameObject.Find("Player").GetComponent<PlayerMovement>();
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        state = attackRange >= 8f ? AttackState.weapon : AttackState.meele;
        boxCollider = GetComponent<BoxCollider>();
        launchWeb = false;

        lastValidPosition = player.position;

        live = true;

        if (state == AttackState.meele)
            transform.Rotate(0, 180, 0);
    }

    private void Update()
    {
        if (state == AttackState.weapon && playerInSightRange && playerInAttackRange)
            transform.LookAt(player);
        if (live)
        {
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

            enemiLocation = transform.position;

            if (!playerInSightRange && !playerInAttackRange)
            {
                Patroling();
            }
            if (playerInSightRange && !playerInAttackRange)
            {
                attacked = false;
                ChasePlayer();
            }
            if (playerInSightRange && playerInAttackRange)
            {
                Attack(4f);
            }
        }
        else
        {
            agent.SetDestination(enemiLocation);
        }
    }

    private void Patroling()
    {
        cronometro += 1 * Time.deltaTime;
        if (IsNearNavMeshEdge())
        {
            TurnAwayFromEdge();
        }
        else
        {
            if (cronometro >= 4)
            {
                rutina = Random.Range(0, 2);
                cronometro = 0;
            }

            switch (rutina)
            {
                case 0:
                    if (state == AttackState.meele)
                        anim.SetBool("Move", false);
                    break;

                case 1:
                    grado = Random.Range(0, 360);
                    angulo = Quaternion.Euler(0, grado, 0);
                    rutina++;
                    break;

                case 2:
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, angulo, 0.5f);
                    transform.Translate(Vector3.forward * agent.speed * Time.deltaTime);
                    if (state == AttackState.meele)
                        anim.SetBool("Move", true);
                    break;
            }
        }
    }

    private void ChasePlayer()
    {
        if (state != AttackState.meele)
            transform.LookAt(player);
        else
            anim.SetBool("Move", true);
        agent.isStopped = false;

        NavMeshHit hit;
        bool playerOnNavMesh = NavMesh.SamplePosition(player.position, out hit, 5f, NavMesh.AllAreas);
        bool npcHasPathToPlayer = NavMesh.CalculatePath(agent.transform.position, player.position, NavMesh.AllAreas, new NavMeshPath());

        if (playerOnNavMesh)
        {
            if (npcHasPathToPlayer)
            {
                lastValidPosition = player.position;
                if (state == AttackState.meele && !alreadyAttacked && !launchWeb && trowWeb)
                {
                    float intervale = Random.Range(0f, 1f);
                    if (intervale <= 1f)
                    {
                        alreadyAttacked = true;
                        launchWeb = true;
                        Invoke(nameof(LaunchWeb), timeBetweenAttacks);
                    }
                }
            }
            lastValidPosition = new Vector3(lastValidPosition.x, transform.position.y, lastValidPosition.z);
            agent.SetDestination(lastValidPosition);
        }
        else
        {
            lastValidPosition = new Vector3(lastValidPosition.x, transform.position.y, lastValidPosition.z);
            agent.SetDestination(lastValidPosition);
        }
    }

    private void TurnAwayFromEdge()
    {
        Vector3 directionAwayFromEdge = GetNearestNavMeshEdgePoint() - transform.position;

        float turnAngle = 90f;
        Quaternion targetRotation;

        if (Vector3.Dot(directionAwayFromEdge, transform.right) > 0)
            targetRotation = Quaternion.Euler(0, -turnAngle, 0) * transform.rotation;
        else
            targetRotation = Quaternion.Euler(0, turnAngle, 0) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
        transform.Translate(Vector3.forward * agent.speed * Time.deltaTime);
    }

    private Vector3 GetNearestNavMeshEdgePoint()
    {
        NavMeshHit hit;

        if (NavMesh.FindClosestEdge(transform.position, out hit, NavMesh.AllAreas))
            return hit.position;
        return transform.position;
    }

    private bool IsNearNavMeshEdge()
    {
        NavMeshHit hit;

        if (NavMesh.SamplePosition(transform.position, out hit, 1f, NavMesh.AllAreas))
        {
            return !NavMesh.FindClosestEdge(hit.position, out hit, NavMesh.AllAreas) || hit.distance < 2f;
        }

        return true;
    }

    private void LaunchWeb()
    {
        anim.SetBool("LaunchWeb", true);
        StartCoroutine(FreezeEnemyWeb());
    }

    private void Attack(float force)
    {
        if (state == AttackState.weapon)
            transform.LookAt(player);
        if (!alreadyAttacked && state != AttackState.meele)
        {
            alreadyAttacked = true;
            attackCoroutine = StartCoroutine(ChangeAttack(force));
        }
        else if (!alreadyAttacked)
        {
            alreadyAttacked = true;

            anim.Play("Spider|Attack", 0, 0.1f);
            anim.SetBool("Attack", true);
            attackCoroutine = StartCoroutine(FreezeEnemy());
        }
    }

    private void LaunchItem(Rigidbody rb, float force)
    {
        float launchForce = 10f;
        if (state == AttackState.weapon)
        {
            launchForce = 10f;
        }
        Vector3 forwardXZ = new Vector3(transform.forward.x, transform.forward.y, transform.forward.z);
        if (!pm.grounded && !launchWeb)
            force = -force;
        rb.AddForce(forwardXZ * launchForce, ForceMode.Impulse);
        rb.AddForce(transform.up * force, ForceMode.Impulse);
    }

    public void TakeDamage(int damage)
    {
        attacked = true;
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
        if (chargeAttackCoroutine != null)
            StopCoroutine(chargeAttackCoroutine);
        if (resetAttackCoroutine != null)
            StopCoroutine(resetAttackCoroutine);

        anim.Play("Hit", 0, 0.1f);
        anim.SetBool("Hit", true);
        health -= damage;

        chargeAttackCoroutine = StartCoroutine(WaitAnimationHit());

        if (health <= 0)
        {
            pm.points += 500;
            StartCoroutine(DestroyEnemy());
        }
    }

    private void ResetWeb()
    {
        launchWeb = false;
    }

    public int GetHealth()
    {
        return health;
    }

    public void SetHealth(int enemyHealth)
    {
        health = enemyHealth;
    }

    // private void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(transform.position, attackRange);
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawWireSphere(transform.position, sightRange);
    // }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerMovement pm = other.gameObject.GetComponent<PlayerMovement>();
            if (!pm.gameOver)
            {
                Rigidbody rbPlayer = other.gameObject.GetComponentInChildren<Rigidbody>();
                LaunchItem(rbPlayer, 10f);

                HealthPlayer healthPlayer = other.gameObject.GetComponentInChildren<HealthPlayer>();
                healthPlayer.TakeDamage(100);
                StartCoroutine(DesFreezePlayer());
            } else {
                whatIsPlayer &= ~(1 << LayerMask.NameToLayer("whatIsPlayer"));
            }
        }
    }

    private IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(timeBetweenAttacks);
        alreadyAttacked = false;
    }

    private IEnumerator ChangeAttack(float force)
    {
        if (attacked) yield return new WaitForSeconds(0f);
        if (state == AttackState.weapon && !attacked)
            anim.SetBool("Ataque", true);
        yield return new WaitForSecondsRealtime(0.25f);
        if (state == AttackState.weapon)
            anim.SetBool("Ataque", false);

        Vector3 launchPosition = transform.position + transform.forward * 2f;

        Vector3 launchDirection = (player.position - launchPosition).normalized;

        Quaternion launchRotation = Quaternion.LookRotation(launchDirection);

        launchRotation *= Quaternion.Euler(0, 90, 0);

        Rigidbody rb = Instantiate(projectile, launchPosition, launchRotation).GetComponent<Rigidbody>();
        LaunchItem(rb, force);

        resetAttackCoroutine = StartCoroutine(ResetAttack());
    }

    private IEnumerator FreezeEnemy()
    {
        yield return new WaitForSeconds(0.25f);

        boxCollider.size = new Vector3(7f, 4f, 7f);
        agent.stoppingDistance = 5f;

        yield return new WaitForSeconds(1f);

        alreadyAttacked = false;
        anim.Play("spider|walk", 0, 0f);
        boxCollider.size = new Vector3(3f, 2f, 3f);
        anim.SetBool("Attack", false);
        agent.stoppingDistance = 0.5f;
    }

    private IEnumerator FreezeEnemyWeb()
    {
        agent.stoppingDistance = sightRange * 2;
        StartCoroutine(ChangeAttack(5f));
        yield return new WaitForSeconds(1f);
        agent.stoppingDistance = 0.5f;
        anim.SetBool("LaunchWeb", false);
        Invoke(nameof(ResetWeb), timeBetweenAttacks * 5);
    }

    private IEnumerator DesFreezePlayer()
    {
        yield return new WaitForSeconds(1f);
        pm.restricted = false;
    }

    private IEnumerator WaitAnimationHit()
    {
        agent.stoppingDistance = 20f;
        yield return new WaitForSecondsRealtime(0.5f);
        anim.SetBool("Hit", false);
        agent.stoppingDistance = 0.5f;
        yield return new WaitForSecondsRealtime(timeBetweenAttacks);
        attacked = false;
        alreadyAttacked = false;

        //resetAttackCoroutine = StartCoroutine(ResetAttack());
    }

    private IEnumerator DestroyEnemy()
    {
        if (state == AttackState.weapon)
            agent.baseOffset = 0;
        live = false;
        anim.Play("die", 0, 0f);
        boxCollider.enabled = false;
        yield return new WaitForSeconds(2f);
        enemiLocation.y = enemiLocation.y - 2f;
        Destroy(gameObject);
    }
}
