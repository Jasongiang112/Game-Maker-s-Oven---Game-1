using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private GameObject hitEffectPrefab;   
    [SerializeField] private Vector3 hitEffectOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private float hitEffectLifetime = 1f;
    [SerializeField] private bool isBoss;


    private int hitsRemaining;
    private bool isDead;

    public bool IsBoss => isBoss;
    public bool IsDead => isDead;
    public int HitsRemaining => hitsRemaining;

    private void Awake()
    {
        // Normal enemies die in 2 hits.
        // Boss enemies die in 5 hits.
        hitsRemaining = isBoss ? 5 : 2;
    }
    
    public void TakeSwordHit()
    {
        //shows hit effect on enemy when hit by player
        ShowHitEffect();
        //enemy gets knocked back when hit
        GetComponent<EnemyNav>().ApplyKnockback();

        if (isDead)
        {
            return;
        }

        hitsRemaining--;

        Debug.Log(
            $"{gameObject.name} was hit. " +
            $"Hits remaining: {hitsRemaining}"
        );

        if (hitsRemaining <= 0)
        {
            Die();
        }
    }

    //effect shows on enemy when hit by player
    private void ShowHitEffect()
{
    if (hitEffectPrefab == null)
    {
        return;
    }

    GameObject effect = Instantiate(
        hitEffectPrefab,
        transform.position + hitEffectOffset,
        Quaternion.identity
    );

    Destroy(effect, hitEffectLifetime);
}

    private void Die()
    {
        isDead = true;

        NavMeshAgent agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        foreach (Collider enemyCollider
                 in GetComponentsInChildren<Collider>())
        {
            enemyCollider.enabled = false;
        }

        Destroy(gameObject);
    }
}