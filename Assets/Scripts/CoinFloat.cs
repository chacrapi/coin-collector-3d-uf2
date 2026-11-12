using UnityEngine;

public sealed class CoinFloat : MonoBehaviour
{
    [SerializeField, Min(0f)] private float height = 0.3f;
    [SerializeField, Min(0f)] private float floatSpeed = 0.5f;

    private Vector3 initialPosition;

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        float offset = Mathf.PingPong(Time.timeSinceLevelLoad * floatSpeed, height);
        transform.position = initialPosition + Vector3.up * offset;
    }
}
