using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BurgerPlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6.5f;
    [SerializeField] private float acceleration = 35f;
    [SerializeField] private float jumpForce = 7.5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private GameObject starAuraPrefab;

    [Header("Spin Attack")]
    [SerializeField] private float spinSpeed = 720f;

    [Header("Slash Effect")]
    [SerializeField] private GameObject slashPrefab;
    [SerializeField] private float slashForwardOffset = 1f;
    [SerializeField] private float slashHeightOffset = 0f;
    [SerializeField] private float slashLifetime = 0.5f;
    [SerializeField] private Vector3 slashRotationOffset;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.75f;
    [SerializeField] private LayerMask groundLayers = ~0;

    private Rigidbody body;
    private Vector2 moveInput;

    public bool HasMoveInput => moveInput.sqrMagnitude > 0.01f;

    private bool isSpinning;
    public bool IsSpinning => isSpinning;
    private float spinRemaining;

    private bool isDashing;
    public bool IsDashing => isDashing;

    private float dashEndTime;
    private Vector3 dashDirection;
    private GameObject activeStarAura;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void Update()
    {
        if (isDashing && Time.time >= dashEndTime)
        {
            EndDash();
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            horizontal -= 1f;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            horizontal += 1f;

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            vertical -= 1f;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            vertical += 1f;

        moveInput = Vector2.ClampMagnitude(
            new Vector2(horizontal, vertical),
            1f
        );

        //press shift to dash 
        bool shiftPressed =
            keyboard.leftShiftKey.wasPressedThisFrame ||
            keyboard.rightShiftKey.wasPressedThisFrame;

        if (shiftPressed && !isDashing)
        {
            StartDash();
        }

        //f to spin + spawns 
        if (keyboard.fKey.wasPressedThisFrame && !isSpinning)
        {
            isSpinning = true;
            spinRemaining = 360f;

            SpawnSlash();
        }

        // press space to jump
        if (keyboard.spaceKey.wasPressedThisFrame && IsGrounded())
        {
            body.AddForce(
                Vector3.up * jumpForce,
                ForceMode.VelocityChange
            );
        }
    }

    //dashing 
    private void StartDash()
    {
        isDashing = true;
        dashEndTime = Time.time + dashDuration;

        // use movement input/ face forward if standing still.
        dashDirection = HasMoveInput
            ? new Vector3(moveInput.x, 0f, moveInput.y).normalized
            : Vector3.ProjectOnPlane(
                transform.forward,
                Vector3.up
            ).normalized;

        if (starAuraPrefab != null)
        {
            //make star aura follow burger 
            activeStarAura = Instantiate(
                starAuraPrefab,
                transform.position,
                transform.rotation,
                transform
            );

            activeStarAura.transform.localPosition = Vector3.zero;

            foreach (ParticleSystem effect in
                activeStarAura.GetComponentsInChildren<ParticleSystem>())
            {
                effect.Play();
            }
        }
    }

    private void EndDash()
    {
        isDashing = false;

        if (activeStarAura != null)
        {
            Destroy(activeStarAura);
            activeStarAura = null;
        }
    }

    private void SpawnSlash()
    {
        if (slashPrefab == null) return;

        Vector3 spawnPosition =
            transform.position +
            transform.forward * slashForwardOffset +
            Vector3.up * slashHeightOffset;

        Quaternion spawnRotation =
            transform.rotation *
            Quaternion.Euler(slashRotationOffset);

        GameObject slash = Instantiate(
            slashPrefab,
            spawnPosition,
            spawnRotation
        );

        Destroy(slash, slashLifetime);
    }

    private void FixedUpdate()
    {
        Vector3 desiredVelocity;

        if (isDashing)
        {
            desiredVelocity = dashDirection * dashSpeed;

            // apply dash speed immediately + preserving vertical velocity.
            body.linearVelocity = new Vector3(
                desiredVelocity.x,
                body.linearVelocity.y,
                desiredVelocity.z
            );
        }
        else
        {
            desiredVelocity =
                new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;

            Vector3 currentHorizontal = new Vector3(
                body.linearVelocity.x,
                0f,
                body.linearVelocity.z
            );

            Vector3 velocityChange = Vector3.MoveTowards(
                currentHorizontal,
                desiredVelocity,
                acceleration * Time.fixedDeltaTime
            ) - currentHorizontal;

            body.AddForce(
                velocityChange,
                ForceMode.VelocityChange
            );
        }

        if (isSpinning)
        {
            float rotation = Mathf.Min(
                spinSpeed * Time.fixedDeltaTime,
                spinRemaining
            );

            transform.Rotate(0f, rotation, 0f);
            spinRemaining -= rotation;

            if (spinRemaining <= 0f)
            {
                isSpinning = false;
            }
        }
        else if (desiredVelocity.sqrMagnitude > 0.05f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(
                desiredVelocity.normalized,
                Vector3.up
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                12f * Time.fixedDeltaTime
            );
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Respawn"))
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        EndDash();

        isSpinning = false;
        spinRemaining = 0f;

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        transform.position = new Vector3(0f, 0.81f, -20f);
    }

    private void OnDisable()
    {
        EndDash();
    }
}