using UnityEngine;

[RequireComponent(typeof(BurgerPlayerController))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maximumHealth = 100;

    [Header("Knockback")]
    [SerializeField] private float knockbackStrength = 8f;
    [SerializeField] private float knockbackDuration = 0.2f;

private float knockbackEndTime;

public bool IsKnockedBack => Time.time < knockbackEndTime;
    private BurgerPlayerController playerController;
    private Rigidbody body;

    private int currentHealth;
    private bool isDead;

    public int CurrentHealth =>
        currentHealth;

    public int MaximumHealth =>
        maximumHealth;

    public bool IsDead =>
        isDead;

    private void ApplyKnockback(Transform attacker)
{
    //only pushing backwards
    body.linearVelocity =
        -transform.forward * knockbackStrength +
        Vector3.up * body.linearVelocity.y;

    //pauses normal movement
    knockbackEndTime = Time.time + knockbackDuration;
}
    private void Awake()
    {
        playerController =
            GetComponent<BurgerPlayerController>();

        body = GetComponent<Rigidbody>();

        currentHealth = maximumHealth;
    }

    public bool TakeDamage(int damage)
    {
        //player flashes when hit
        GetComponent<HitFlash>().Flash();
        // Enemy attacks respect dash invulnerability.
        return ApplyDamage(
            damage,
            true
        );
    }

    public bool TakeEnvironmentalDamage(int damage)
    {
        // Environmental hazards ignore dash invulnerability.
        return ApplyDamage(
            damage,
            false
        );
    }

    private bool ApplyDamage(
        int damage,
        bool respectDashInvulnerability)
    {
        if (isDead || damage <= 0)
        {
            return false;
        }

        if (respectDashInvulnerability &&
            playerController.IsDashing)
        {
            return false;
        }

        currentHealth = Mathf.Max(
            0,
            currentHealth - damage
        );

        if (currentHealth > 0 && body != null)
{
    //only push backwards
        body.linearVelocity =
        -transform.forward * knockbackStrength +
        Vector3.up * body.linearVelocity.y;

    //pauses movement normal movement while hit
        knockbackEndTime = Time.time + knockbackDuration;
}

        Debug.Log(
            $"Player took {damage} damage. " +
            $"Health: {currentHealth}/{maximumHealth}"
        );
        if (currentHealth == 0)
        {
            Die();
        }

        return true;
    }


    public void RestoreHealth(int amount)
    {
        if (isDead || amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Min(
            maximumHealth,
            currentHealth + amount
        );
    }

    public void RestoreFullHealth()
    {
        if (isDead)
        {
            return;
        }

        currentHealth = maximumHealth;
    }

    private void Die()
    {
        isDead = true;

        if (body != null)
        {
            body.linearVelocity =
                Vector3.zero;

            body.angularVelocity =
                Vector3.zero;
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        Debug.Log("Player died.");
    }
}