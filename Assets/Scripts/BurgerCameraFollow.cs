using UnityEngine;

public class BurgerCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 6.5f, -8.5f);
    [SerializeField] private float followSharpness = 8f;
    [SerializeField] private Vector3 lookOffset = new Vector3(0f, 0.7f, 2.5f);

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target.position + lookOffset);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;
        float blend = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, target.position + offset, blend);
        Quaternion desiredRotation = Quaternion.LookRotation(target.position + lookOffset - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, blend);
    }
}
