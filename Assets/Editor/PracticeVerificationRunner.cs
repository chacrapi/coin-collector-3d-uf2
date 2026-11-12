using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PracticeVerificationRunner
{
    private const string Key = "UF2.VerificationRunning";

    static PracticeVerificationRunner()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    [MenuItem("UF2/Verificar practica y cerrar editor")]
    public static void Run()
    {
        SessionState.SetBool(Key, true);
        EditorSceneManager.OpenScene("Assets/Scenes/MenuPrincipal.unity");
        EditorApplication.EnterPlaymode();
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Key, false))
            return;

        SessionState.SetBool(Key, false);
        GameObject driver = new GameObject("UF2_Verification");
        Object.DontDestroyOnLoad(driver);
        driver.AddComponent<PracticeVerificationDriver>();
    }
}
