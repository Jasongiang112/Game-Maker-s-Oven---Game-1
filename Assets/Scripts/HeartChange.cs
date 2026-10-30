using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HeartState : MonoBehaviour
{
    public PlayerHealth playerHealth;

    public enum HeartType
    {
        Full,
        Half,
        Empty
    }

    public HeartType heartType;

    private Image heartImage;

    void Awake()
    {
        heartImage = GetComponent<Image>();
    }

    void Update()
    {
        if (playerHealth == null)
            return;

        float health = playerHealth.CurrentHealth;

        switch (heartType)
        {
            case HeartType.Full:
                heartImage.enabled = health > 50f;
                break;

            case HeartType.Half:
                heartImage.enabled = health > 0f && health <= 50f;
                break;

            case HeartType.Empty:
                heartImage.enabled = health <= 0f;
                break;
        }
    }
}