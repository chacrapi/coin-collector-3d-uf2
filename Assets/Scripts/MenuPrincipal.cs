using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class MenuPrincipal : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("Level1");
    }
}
