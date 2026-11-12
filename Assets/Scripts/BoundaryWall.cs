using UnityEngine;

// El enunciado pide paredes Trigger. Un trigger por si solo no detiene cuerpos:
// este componente mantiene la bola dentro sin convertirlo en un collider solido.
[RequireComponent(typeof(BoxCollider))]
public sealed class BoundaryWall : MonoBehaviour
{
    [SerializeField] private Vector3 inwardNormal = Vector3.forward;
    [SerializeField, Min(0f)] private float clearance = 0.85f;

    private void OnTriggerEnter(Collider other) => KeepInside(other);
    private void OnTriggerStay(Collider other) => KeepInside(other);

    private void KeepInside(Collider other)
    {
        if (!other.CompareTag("Player") || other.attachedRigidbody == null)
            return;

        Rigidbody body = other.attachedRigidbody;
        Vector3 normal = inwardNormal.normalized;
        float distance = Vector3.Dot(body.position - transform.position, normal);
        if (distance >= clearance)
            return;

        body.position += normal * (clearance - distance);
        float outwardSpeed = Vector3.Dot(body.linearVelocity, normal);
        if (outwardSpeed < 0f)
            body.linearVelocity -= normal * outwardSpeed;
    }
}
