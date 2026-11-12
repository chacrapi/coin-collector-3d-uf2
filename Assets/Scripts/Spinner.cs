using UnityEngine;

public sealed class Spinner : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f;

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}
