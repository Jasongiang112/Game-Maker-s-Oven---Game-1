using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class DamageCheck : MonoBehaviour
{
    private BoxCollider hitbox;
    private Rigidbody body;

    // Prevents one attack from damaging the same enemy multiple times.
    private readonly HashSet<EnemyHealth> enemiesHit =
        new HashSet<EnemyHealth>();

    private void Awake()
    {
        hitbox = GetComponent<BoxCollider>();
        body = GetComponent<Rigidbody>();

        hitbox.isTrigger = true;

        body.isKinematic = true;
        body.useGravity = false;
        body.collisionDetectionMode =
            CollisionDetectionMode.ContinuousSpeculative;
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyHealth enemy =
            other.GetComponentInParent<EnemyHealth>();

        if (enemy == null || enemy.IsDead)
        {
            return;
        }

        // Each spawned slash can hit each enemy only once.
        if (enemiesHit.Add(enemy))
        {
            enemy.TakeSwordHit();
        }
    }
}