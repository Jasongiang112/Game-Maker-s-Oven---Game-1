using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BurgerPlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6.5f;
    [SerializeField] private float acceleration = 35f;
    [SerializeField] private float jumpForce = 7.5f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.75f;
    [SerializeField] private LayerMask groundLayers = ~0;

    private Rigidbody body;
    private Vector2 moveInput;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;
        moveInput = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);

        if (keyboard.spaceKey.wasPressedThisFrame && IsGrounded())
            body.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

    private void FixedUpdate()
    {
        Vector3 desiredVelocity = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
        Vector3 currentHorizontal = new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);
        Vector3 velocityChange = Vector3.MoveTowards(currentHorizontal, desiredVelocity, acceleration * Time.fixedDeltaTime) - currentHorizontal;
        body.AddForce(velocityChange, ForceMode.VelocityChange);

        if (desiredVelocity.sqrMagnitude > 0.05f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredVelocity.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 12f * Time.fixedDeltaTime);
        }
    }

    private bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.SphereCast(origin, 0.42f, Vector3.down, out _, groundCheckDistance, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Respawn")) Respawn();
    }

    private void Respawn()
    {
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        transform.position = new Vector3(0f, 0.81f, -20f);
    }
}
