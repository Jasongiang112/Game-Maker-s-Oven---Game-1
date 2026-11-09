using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBar : MonoBehaviour
{
    public PlayerHealth playerHealth;

    private Slider healthBar;

    private void Awake()
    {
        healthBar = GetComponent<Slider>();
        healthBar.minValue = 0f;
    }

    private void Update()
    {
        if (playerHealth == null)
        {
            return;
        }

        healthBar.maxValue = playerHealth.MaximumHealth;
        healthBar.value = playerHealth.CurrentHealth;
    }
}