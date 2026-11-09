using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyNav : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Chasing")]
    [SerializeField] private float detectionRange = 30f;
    [SerializeField] private float attackRange = 1.7f;
    [SerializeField] private float destinationUpdateRate = 0.1f;

    [Header("Attacking")]
    [SerializeField] private int normalDamage = 15;
    [SerializeField] private int bossDamage = 40;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Knockback")]
    [SerializeField] private float knockbackStrength = 8f;
    [SerializeField] private float knockbackDuration = 0.2f;
    private float knockbackEndTime;
    private Vector3 knockbackDirection;

    private NavMeshAgent agent;
    private EnemyHealth enemyHealth;
    private PlayerHealth playerHealth;

    private float nextAttackTime;
    private float nextDestinationUpdateTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyHealth = GetComponent<EnemyHealth>();

        agent.stoppingDistance = attackRange * 0.85f;
        agent.updateRotation = true;
    }
    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (enemyHealth.IsDead ||
            player == null ||
            playerHealth == null ||
            playerHealth.IsDead)
        {
            StopMoving();
            return;
        }

        if (!agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }
        if (Time.time < knockbackEndTime)
        {
            agent.Move(knockbackDirection * knockbackStrength * Time.deltaTime);
            
            return;
        }

        Vector3 difference =
            player.position - transform.position;

        difference.y = 0f;

        float distanceToPlayer = difference.magnitude;

        if (distanceToPlayer > detectionRange)
        {
            StopMoving();
            return;
        }

        if (distanceToPlayer <= attackRange)
        {
            StopMoving();
            FacePlayer(difference);
            TryAttack();
        }
        else
        {
            ChasePlayer();
        }
    }

    private void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (player == null)
        {
            Debug.LogWarning(
                $"{gameObject.name} could not find an object " +
                "with the Player tag."
            );

            return;
        }

        playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            playerHealth =
                player.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth == null)
        {
            Debug.LogWarning(
                $"{gameObject.name} found the player, but the " +
                "player does not have a PlayerHealth component."
            );
        }
    }

    private void ChasePlayer()
    {
        agent.isStopped = false;

        if (Time.time < nextDestinationUpdateTime)
        {
            return;
        }

        agent.SetDestination(player.position);

        nextDestinationUpdateTime =
            Time.time + destinationUpdateRate;
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        int damage = enemyHealth.IsBoss
            ? bossDamage
            : normalDamage;

        playerHealth.TakeDamage(damage);

        // The cooldown still begins if the player avoids damage by dashing.
        nextAttackTime = Time.time + attackCooldown;
    }

    private void FacePlayer(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction.normalized,
                Vector3.up
            );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            12f * Time.deltaTime
        );
    }

    private void StopMoving()
    {
        if (agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = true;
        agent.ResetPath();
    }

    //knockback whenever attacked by player
    public void ApplyKnockback()
    {
    if (enemyHealth.IsDead ||
        player == null ||
        !agent.enabled ||
        !agent.isOnNavMesh)
    {
        return;
    }

    //moves away from player's direction
    knockbackDirection = transform.position - player.position;
    knockbackDirection.y = 0f;
    knockbackDirection = knockbackDirection.normalized;

    knockbackEndTime = Time.time + knockbackDuration;

    StopMoving();
    }

    private void OnDisable()
    {
        StopMoving();
    }
}