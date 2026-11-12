using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public sealed class Coin : MonoBehaviour
{
    private bool collected;

    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player"))
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogError("Coin needs a GameManager in the scene.", this);
            return;
        }

        // Destroy se aplica al final del frame: este flag evita contar dos veces.
        collected = true;
        GameManager.Instance.AddScore(1);
        Debug.Log($"Coin collected! Score: {GameManager.Instance.Score}");
        Destroy(gameObject);
    }
}
