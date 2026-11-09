using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [Header("Stamina")]
    [SerializeField] private float maximumStamina = 3f;
    [SerializeField] private float dashStaminaCost = 1f;
    [SerializeField] private float staminaRegenerationRate = 0.75f;
    [SerializeField] private float staminaRegenerationDelay = 0.75f;
    [SerializeField] private Slider staminaSlider;

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

    [Header("Environmental Damage")]
    [SerializeField] private int sinkPitDamage = 30;
    [SerializeField]
    private Vector3 respawnPosition =
        new Vector3(0f, 0.81f, -20f);

    private Rigidbody body;
    private PlayerHealth playerHealth;

    private Vector2 moveInput;

    private bool isSpinning;
    private float spinRemaining;

    private bool isDashing;
    private float dashEndTime;
    private Vector3 dashDirection;
    private GameObject activeStarAura;

    private float currentStamina;
    private float staminaRegenerationStartTime;

    public bool HasMoveInput =>
        moveInput.sqrMagnitude > 0.01f;

    public bool IsSpinning =>
        isSpinning;

    public bool IsDashing =>
        isDashing;

    public float CurrentStamina =>
        currentStamina;

    public float MaximumStamina =>
        maximumStamina;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();

        playerHealth =
            GetComponent<PlayerHealth>();

        body.freezeRotation = true;

        body.interpolation =
            RigidbodyInterpolation.Interpolate;

        body.collisionDetectionMode =
            CollisionDetectionMode.Continuous;

        currentStamina = maximumStamina;

        SetupStaminaBar();
    }

    private void Update()
    {
        UpdateDashTimer();
        ReadMovementInput();
        ReadDashInput();
        ReadAttackInput();
        ReadJumpInput();
        RegenerateStamina();
    }

    private void FixedUpdate()
    {
        Vector3 desiredVelocity;

        if (isDashing)
        {
            desiredVelocity =
                dashDirection * dashSpeed;

            body.linearVelocity =
                new Vector3(
                    desiredVelocity.x,
                    body.linearVelocity.y,
                    desiredVelocity.z
                );
        }
        else
        {
            desiredVelocity =
                new Vector3(
                    moveInput.x,
                    0f,
                    moveInput.y
                ) * moveSpeed;

            Vector3 currentHorizontalVelocity =
                new Vector3(
                    body.linearVelocity.x,
                    0f,
                    body.linearVelocity.z
                );

            Vector3 velocityChange =
                Vector3.MoveTowards(
                    currentHorizontalVelocity,
                    desiredVelocity,
                    acceleration *
                    Time.fixedDeltaTime
                ) - currentHorizontalVelocity;

            body.AddForce(
                velocityChange,
                ForceMode.VelocityChange
            );
        }

        UpdateSpin(desiredVelocity);
    }

    private void ReadMovementInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed ||
            keyboard.leftArrowKey.isPressed)
        {
            horizontal -= 1f;
        }

        if (keyboard.dKey.isPressed ||
            keyboard.rightArrowKey.isPressed)
        {
            horizontal += 1f;
        }

        if (keyboard.sKey.isPressed ||
            keyboard.downArrowKey.isPressed)
        {
            vertical -= 1f;
        }

        if (keyboard.wKey.isPressed ||
            keyboard.upArrowKey.isPressed)
        {
            vertical += 1f;
        }

        moveInput =
            Vector2.ClampMagnitude(
                new Vector2(
                    horizontal,
                    vertical
                ),
                1f
            );
    }

    private void ReadDashInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        bool shiftPressed =
            keyboard.leftShiftKey.wasPressedThisFrame ||
            keyboard.rightShiftKey.wasPressedThisFrame;

        if (!shiftPressed)
        {
            return;
        }

        if (isDashing)
        {
            return;
        }

        if (currentStamina < dashStaminaCost)
        {
            return;
        }

        StartDash();
    }

    private void ReadAttackInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (keyboard.fKey.wasPressedThisFrame &&
            !isSpinning)
        {
            StartAttack();
        }
    }

    private void ReadJumpInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (keyboard.spaceKey.wasPressedThisFrame &&
            IsGrounded())
        {
            body.AddForce(
                Vector3.up * jumpForce,
                ForceMode.VelocityChange
            );
        }
    }

    private void StartDash()
    {
        currentStamina =
            Mathf.Max(
                0f,
                currentStamina -
                dashStaminaCost
            );

        staminaRegenerationStartTime =
            Time.time +
            dashDuration +
            staminaRegenerationDelay;

        UpdateStaminaBar();

        isDashing = true;

        dashEndTime =
            Time.time + dashDuration;

        dashDirection = HasMoveInput
            ? new Vector3(
                moveInput.x,
                0f,
                moveInput.y
            ).normalized
            : Vector3.ProjectOnPlane(
                transform.forward,
                Vector3.up
            ).normalized;

        SpawnStarAura();
    }

    private void UpdateDashTimer()
    {
        if (isDashing &&
            Time.time >= dashEndTime)
        {
            EndDash();
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

    private void SpawnStarAura()
    {
        if (starAuraPrefab == null)
        {
            return;
        }

        activeStarAura =
            Instantiate(
                starAuraPrefab,
                transform.position,
                transform.rotation,
                transform
            );

        activeStarAura
            .transform
            .localPosition = Vector3.zero;

        ParticleSystem[] effects =
            activeStarAura
                .GetComponentsInChildren
                    <ParticleSystem>();

        foreach (ParticleSystem effect in effects)
        {
            effect.Play();
        }
    }

    private void RegenerateStamina()
    {
        if (isDashing)
        {
            return;
        }

        if (Time.time <
            staminaRegenerationStartTime)
        {
            return;
        }

        if (currentStamina >= maximumStamina)
        {
            return;
        }

        currentStamina =
            Mathf.MoveTowards(
                currentStamina,
                maximumStamina,
                staminaRegenerationRate *
                Time.deltaTime
            );

        UpdateStaminaBar();
    }

    private void SetupStaminaBar()
    {
        if (staminaSlider == null)
        {
            return;
        }

        staminaSlider.minValue = 0f;

        staminaSlider.maxValue =
            maximumStamina;

        staminaSlider.wholeNumbers = false;

        UpdateStaminaBar();
    }

    private void UpdateStaminaBar()
    {
        if (staminaSlider != null)
        {
            staminaSlider.value =
                currentStamina;
        }
    }

    private void StartAttack()
    {
        isSpinning = true;
        spinRemaining = 360f;

        SpawnSlash();
    }

    private void UpdateSpin(
        Vector3 desiredVelocity)
    {
        if (isSpinning)
        {
            float rotation =
                Mathf.Min(
                    spinSpeed *
                    Time.fixedDeltaTime,
                    spinRemaining
                );

            transform.Rotate(
                0f,
                rotation,
                0f
            );

            spinRemaining -= rotation;

            if (spinRemaining <= 0f)
            {
                spinRemaining = 0f;
                isSpinning = false;
            }

            return;
        }

        if (desiredVelocity.sqrMagnitude <= 0.05f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(
                desiredVelocity.normalized,
                Vector3.up
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                12f *
                Time.fixedDeltaTime
            );
    }

    private void SpawnSlash()
    {
        if (slashPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition =
            transform.position +
            transform.forward *
            slashForwardOffset +
            Vector3.up *
            slashHeightOffset;

        Quaternion spawnRotation =
            transform.rotation *
            Quaternion.Euler(
                slashRotationOffset
            );

        GameObject slash =
            Instantiate(
                slashPrefab,
                spawnPosition,
                spawnRotation
            );

        Destroy(
            slash,
            slashLifetime
        );
    }

    private bool IsGrounded()
    {
        Vector3 origin =
            transform.position +
            Vector3.up * 0.1f;

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

    private void OnTriggerEnter(
        Collider other)
    {
        if (!other.CompareTag("Respawn"))
        {
            return;
        }

        if (playerHealth != null)
        {
            playerHealth
                .TakeEnvironmentalDamage(
                    sinkPitDamage
                );
        }

        if (playerHealth == null ||
            !playerHealth.IsDead)
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        EndDash();

        isSpinning = false;
        spinRemaining = 0f;

        currentStamina =
            maximumStamina;

        staminaRegenerationStartTime = 0f;

        UpdateStaminaBar();

        body.linearVelocity =
            Vector3.zero;

        body.angularVelocity =
            Vector3.zero;

        transform.position =
            respawnPosition;
    }

    private void OnDisable()
    {
        EndDash();
    }
}