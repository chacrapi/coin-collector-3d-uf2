using System.IO;
using UnityEditor;

// Marcador local para automatizar la primera apertura; no se distribuye con el proyecto.
public static class PracticeBootstrap
{
    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        EditorApplication.delayCall += GenerateIfRequested;
    }

    private static void GenerateIfRequested()
    {
        if (File.Exists(".uf2-generate"))
        {
            File.Delete(".uf2-generate");
            PracticeBuilder.Generate();
        }
        if (File.Exists(".uf2-build"))
        {
            File.Delete(".uf2-build");
            PracticeBuilder.BuildWindows();
        }
        if (File.Exists(".uf2-verify"))
        {
            File.Delete(".uf2-verify");
            PracticeVerificationRunner.Run();
        }
    }
}
