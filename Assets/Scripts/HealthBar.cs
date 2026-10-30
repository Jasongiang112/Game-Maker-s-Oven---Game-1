using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    public Slider healthBar;
    public float maxHealth = 100f;

    private float currentHealth;
    
    //keeps track of health for hearth symbol change
    public float CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;

        healthBar.minValue = 0f;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }

    void Update()
    {
        // Press H to test taking damage.
        if (Keyboard.current != null &&
            Keyboard.current.hKey.wasPressedThisFrame)
        {
            TakeDamage(10f);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Clamp(
            currentHealth - damage, 0f, maxHealth
        );

        healthBar.value = currentHealth;

        if (currentHealth <= 0f)
        {
            Debug.Log("Player died!");
        }
    }
}