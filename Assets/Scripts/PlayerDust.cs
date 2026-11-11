using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerDust : MonoBehaviour
{
    [SerializeField] private ParticleSystem dust;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.75f;
    [SerializeField] private LayerMask groundLayers = ~0;

    private Rigidbody body;
    private BurgerPlayerController player;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        player = GetComponent<BurgerPlayerController>();

        if (dust != null)
        {
            dust.Stop(true,
                ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void LateUpdate()
    {
        if (dust == null) return;

        Vector3 velocity = body.linearVelocity;

        bool isMoving =
            new Vector2(velocity.x, velocity.z).sqrMagnitude > 0.01f;

        bool isSpinning = player != null && player.IsSpinning;

        bool shouldEmit =
            isMoving &&
            velocity.y <= 0.1f &&
            IsGrounded() &&
            !isSpinning;

        if (shouldEmit && !dust.isEmitting)
        {
            dust.Play(true);
        }
        else if (!shouldEmit && dust.isEmitting)
        {
            dust.Stop(true,
                ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        return Physics.SphereCast(
            origin,
            0.42f,
            Vector3.down,
            out _,
            groundCheckDistance,
            groundLayers,
            QueryTriggerInteraction.Ignore
        );
    }
}