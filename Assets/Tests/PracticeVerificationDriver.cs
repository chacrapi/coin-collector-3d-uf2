#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Prueba de integracion en Play Mode. No se incluye en los ejecutables.
public sealed class PracticeVerificationDriver : MonoBehaviour
{
    private readonly List<string> passed = new List<string>();
    private readonly List<string> logs = new List<string>();
    private readonly List<string> errors = new List<string>();

    private IEnumerator Start()
    {
        Application.runInBackground = true;
        Application.logMessageReceived += OnLog;
        IEnumerator checks = RunChecks();
        while (true)
        {
            bool next;
            object current = null;
            try
            {
                next = checks.MoveNext();
                if (next) current = checks.Current;
            }
            catch (Exception error)
            {
                Finish(error.ToString());
                yield break;
            }
            if (!next) break;
            yield return current;
        }
        Finish(null);
    }

    private IEnumerator RunChecks()
    {
        yield return null;
        Check(SceneManager.GetActiveScene().name == "MenuPrincipal", "El juego arranca en MenuPrincipal");
        Button play = FindAnyObjectByType<Button>();
        Check(play != null && play.onClick.GetPersistentEventCount() == 1, "Play tiene su evento persistente conectado");
        Capture("menu.png");
        play.onClick.Invoke();
        yield return null;
        yield return new WaitForSeconds(0.25f);
        Check(SceneManager.GetActiveScene().name == "Level1", "El boton Play carga Level1");
        GameManager manager = GameManager.Instance;
        Check(manager != null && manager.Score == 0, "Singleton inicial y puntuacion cero");
        Check(FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length == 1, "Una instancia de GameManager");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Rigidbody body = player.GetComponent<Rigidbody>();
        Check(body != null && player.GetComponent<SphereCollider>() != null, "Player: SphereCollider y Rigidbody");
        Check(body.constraints.HasFlag(RigidbodyConstraints.FreezeRotationX) && body.constraints.HasFlag(RigidbodyConstraints.FreezeRotationZ), "Rotacion X y Z bloqueada");
        Check(GameObject.FindGameObjectWithTag("Ground").GetComponent<MeshCollider>() != null, "Plane con tag Ground y collider");
        BoundaryWall[] walls = FindObjectsByType<BoundaryWall>(FindObjectsSortMode.None);
        Check(walls.Length == 4, "Cuatro paredes invisibles");
        foreach (BoundaryWall wall in walls)
            Check(wall.GetComponent<BoxCollider>().isTrigger && !wall.GetComponent<MeshRenderer>().enabled, wall.name + ": trigger sin renderer");
        KillZone kill = FindAnyObjectByType<KillZone>();
        Check(kill.GetComponent<BoxCollider>().isTrigger && !kill.GetComponent<MeshRenderer>().enabled && kill.transform.position.y < 0f, "KillZone invisible debajo del nivel");
        Check(FindObjectsByType<Coin>(FindObjectsSortMode.None).Length == 6, "Seis instancias de moneda");
        Capture("level1.png");

        PlayerController controller = player.GetComponent<PlayerController>();
        controller.enabled = false;
        body.useGravity = false;
        body.linearVelocity = new Vector3(0f, -2.5f, 0f);
        typeof(PlayerController).GetField("movementInput", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(controller, new Vector2(1f, 0f));
        MethodInfo fixedUpdate = typeof(PlayerController).GetMethod("FixedUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
        fixedUpdate.Invoke(controller, null);
        Check(Mathf.Approximately(body.linearVelocity.y, -2.5f), "FixedUpdate conserva velocidad Y");
        Check(Mathf.Approximately(body.linearVelocity.x, 6f), "FixedUpdate aplica speed a linearVelocity");
        Vector3 before = body.position;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        Check(body.position.x > before.x, "El Rigidbody se desplaza en pasos fisicos");
        body.useGravity = true;
        body.linearVelocity = Vector3.zero;
        body.position = new Vector3(0f, 0.6f, -5.5f);
        Physics.SyncTransforms();

        Coin animated = FindAnyObjectByType<Coin>();
        float oldY = animated.transform.position.y;
        Quaternion oldRotation = animated.transform.rotation;
        yield return new WaitForSeconds(0.2f);
        Check(Quaternion.Angle(oldRotation, animated.transform.rotation) > 1f, "Spinner gira la moneda sobre Y");
        Check(Mathf.Abs(oldY - animated.transform.position.y) > 0.01f, "La moneda flota con PingPong");

        Coin[] coins = FindObjectsByType<Coin>(FindObjectsSortMode.None);
        GameObject outsider = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Rigidbody outsiderBody = outsider.AddComponent<Rigidbody>();
        outsiderBody.useGravity = false;
        outsiderBody.position = coins[0].transform.position;
        Physics.SyncTransforms();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        Check(manager.Score == 0, "Un objeto sin tag Player no recoge monedas");
        Destroy(outsider);
        yield return null;

        for (int i = 0; i < coins.Length; i++)
        {
            body.position = coins[i].transform.position;
            body.linearVelocity = Vector3.zero;
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return null;
            Check(manager.Score == i + 1, "Moneda " + (i + 1) + ": suma exactamente un punto");
            Check(coins[i] == null, "Moneda " + (i + 1) + ": Destroy elimina el objeto");
            Check(logs.Contains("Coin collected! Score: " + (i + 1)), "Log exacto de moneda " + (i + 1));
        }
        Check(FindObjectsByType<Coin>(FindObjectsSortMode.None).Length == 0, "Se pueden recoger las seis monedas");

        GameObject duplicate = new GameObject("DuplicateManager");
        duplicate.AddComponent<GameManager>();
        yield return null;
        Check(duplicate == null && GameManager.Instance == manager && manager.Score == 6, "El Singleton elimina duplicados sin perder puntuacion");

        body.position = new Vector3(9.55f, 0.7f, 0f);
        body.linearVelocity = new Vector3(6f, 0f, 0f);
        Physics.SyncTransforms();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        Check(body.position.x <= 9.25f, "La pared trigger contiene al jugador");

        body.position = new Vector3(11.5f, 2f, 0f);
        body.linearVelocity = Vector3.zero;
        Physics.SyncTransforms();
        float deadline = Time.realtimeSinceStartup + 5f;
        while (GameManager.Instance == manager && Time.realtimeSinceStartup < deadline)
            yield return null;
        yield return null;
        Check(logs.Contains("Player killed!"), "La caida real activa el log de KillZone");
        Check(SceneManager.GetActiveScene().name == "Level1" && GameManager.Instance != null && GameManager.Instance != manager, "KillZone recarga la escena activa por buildIndex");
        Check(GameManager.Instance.Score == 0 && FindObjectsByType<Coin>(FindObjectsSortMode.None).Length == 6, "El reinicio restaura puntos y seis monedas");
        Check(GameObject.FindGameObjectWithTag("Player").transform.position.y > 0f, "El jugador reaparece encima del suelo");
        Check(errors.Count == 0, "Sin errores ni excepciones en ejecucion");
    }

    private void Check(bool condition, string description)
    {
        if (!condition) throw new Exception("FAILED: " + description);
        passed.Add(description);
        Debug.Log("PASS: " + description);
    }

    private void Capture(string file)
    {
        Directory.CreateDirectory("Docs/Screenshots");
        Canvas.ForceUpdateCanvases();
        Camera camera = Camera.main;
        RenderTexture target = new RenderTexture(1280, 720, 24);
        RenderTexture previous = RenderTexture.active;
        camera.targetTexture = target;
        camera.Render();
        RenderTexture.active = target;
        Texture2D image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        image.Apply();
        File.WriteAllBytes("Docs/Screenshots/" + file, ImageConversion.EncodeToPNG(image));
        camera.targetTexture = null;
        RenderTexture.active = previous;
        target.Release();
        Destroy(target);
        Destroy(image);
    }

    private void OnLog(string message, string stack, LogType type)
    {
        logs.Add(message);
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            errors.Add(message);
    }

    private void Finish(string failure)
    {
        Application.logMessageReceived -= OnLog;
        Directory.CreateDirectory("Docs");
        string report = "# Verificacion en Unity Play Mode\n\n";
        report += "Unity: " + Application.unityVersion + "\n\n";
        report += "Resultado: " + (failure == null ? "PASS" : "FAIL") + "\n\n";
        foreach (string check in passed) report += "- PASS: " + check + "\n";
        if (failure != null) report += "\n```text\n" + failure + "\n```\n";
        foreach (string error in errors) report += "\nERROR: " + error + "\n";
        File.WriteAllText("Docs/VERIFICACION.md", report);
        Debug.Log("UF2 verification: " + (failure == null ? "PASS" : "FAIL"));
        EditorApplication.Exit(failure == null ? 0 : 1);
    }
}
#endif
