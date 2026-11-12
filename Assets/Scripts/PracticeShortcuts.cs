using UnityEngine;
using UnityEngine.SceneManagement;

// Atajos para comprobar el reinicio y volver al menu durante la defensa.
public sealed class PracticeShortcuts : MonoBehaviour
{
    [SerializeField] private Rigidbody playerBody = null;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            GameManager.Instance.RestartLevel();

        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("MenuPrincipal");

        // F deja caer al jugador desde fuera del Plane hacia la KillZone.
        if (Input.GetKeyDown(KeyCode.F) && playerBody != null)
        {
            playerBody.position = new Vector3(11.5f, 2f, 0f);
            playerBody.linearVelocity = Vector3.zero;
        }
    }
}
