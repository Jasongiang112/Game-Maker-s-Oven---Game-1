using UnityEngine;

[RequireComponent(typeof(BurgerPlayerController))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maximumHealth = 100;

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

    private void Awake()
    {
        playerController =
            GetComponent<BurgerPlayerController>();

        body = GetComponent<Rigidbody>();

        currentHealth = maximumHealth;
    }

    public bool TakeDamage(int damage)
    {
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