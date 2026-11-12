using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public sealed class PlayerController : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 6f;

    private Rigidbody body;
    private Vector2 movementInput;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Normalizar evita que la diagonal sea mas rapida.
        movementInput = Vector2.ClampMagnitude(new Vector2(
            Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);
    }

    private void FixedUpdate()
    {
        // La gravedad sigue controlando Y; solo cambiamos la velocidad horizontal.
        body.linearVelocity = new Vector3(
            movementInput.x * speed, body.linearVelocity.y, movementInput.y * speed);
    }
}
