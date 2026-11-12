using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public sealed class KillZone : MonoBehaviour
{
    private bool restarting;

    private void OnTriggerEnter(Collider other)
    {
        if (restarting || !other.CompareTag("Player"))
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogError("KillZone needs a GameManager in the scene.", this);
            return;
        }

        restarting = true;
        Debug.Log("Player killed!");
        GameManager.Instance.RestartLevel();
    }
}
