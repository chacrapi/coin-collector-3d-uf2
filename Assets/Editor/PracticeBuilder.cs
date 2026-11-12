using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

// Genera assets reales y escenas editables. No se genera el nivel al ejecutar el juego.
public static class PracticeBuilder
{
    private const string Root = "Assets/";
    private static Font font;

    [MenuItem("UF2/Regenerar practica (sobrescribe escenas)")]
    public static void Generate()
    {
        foreach (string folder in new[] { "Scenes", "Scripts", "Prefabs", "Materials" })
            Directory.CreateDirectory(Root + folder);
        AssetDatabase.Refresh();

        EditorSettings.serializationMode = SerializationMode.ForceText;
        VersionControlSettings.mode = "Visible Meta Files";
        PlayerSettings.companyName = "UF2";
        PlayerSettings.productName = "Coin Collector 3D";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
        QualitySettings.vSyncCount = 1;
        QualitySettings.shadowDistance = 60f;
        SetLegacyInput();
        AddGroundTag();
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Material ground = Material("Ground", new Color(0.065f, 0.15f, 0.22f), 0.15f, 0.3f);
        Material baseMat = Material("PlatformBase", new Color(0.025f, 0.065f, 0.11f), 0.4f, 0.45f);
        Material grid = Material("Grid", new Color(0.09f, 0.24f, 0.31f), 0f, 0.2f);
        Material trim = Material("TurquoiseTrim", new Color(0.05f, 0.8f, 0.73f), 0.3f, 0.65f);
        Material gold = Material("CoinGold", new Color(1f, 0.68f, 0.08f), 0.65f, 0.8f);
        Material goldFace = Material("CoinFace", new Color(1f, 0.88f, 0.38f), 0.45f, 0.7f);
        Material playerMat = Material("Player", new Color(0.16f, 0.86f, 0.95f), 0.45f, 0.85f);
        Material white = Material("PlayerRing", new Color(0.85f, 1f, 1f), 0.2f, 0.5f);
        GameObject coinPrefab = MakeCoin(gold, goldFace);

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SetupEnvironment();
        GameObject manager = new GameObject("GameManager", typeof(GameManager));
        GameObject level = new GameObject("LevelGeometry");
        Primitive("Ground", PrimitiveType.Plane, Vector3.zero, new Vector3(2f, 1f, 1.6f), ground, level.transform).tag = "Ground";
        Decoration("PlatformBase", PrimitiveType.Cube, new Vector3(0f, -0.32f, 0f), new Vector3(20f, 0.6f, 16f), baseMat, level.transform);
        for (int x = -8; x <= 8; x += 2)
            Decoration("GridX_" + x, PrimitiveType.Cube, new Vector3(x, 0.006f, 0f), new Vector3(0.018f, 0.008f, 15.5f), grid, level.transform);
        for (int z = -6; z <= 6; z += 2)
            Decoration("GridZ_" + z, PrimitiveType.Cube, new Vector3(0f, 0.006f, z), new Vector3(19.5f, 0.008f, 0.018f), grid, level.transform);
        Decoration("NorthTrim", PrimitiveType.Cube, new Vector3(0f, 0.035f, 7.8f), new Vector3(19.6f, 0.06f, 0.06f), trim, level.transform);
        Decoration("SouthTrim", PrimitiveType.Cube, new Vector3(0f, 0.035f, -7.8f), new Vector3(19.6f, 0.06f, 0.06f), trim, level.transform);
        Decoration("EastTrim", PrimitiveType.Cube, new Vector3(9.8f, 0.035f, 0f), new Vector3(0.06f, 0.06f, 15.6f), trim, level.transform);
        Decoration("WestTrim", PrimitiveType.Cube, new Vector3(-9.8f, 0.035f, 0f), new Vector3(0.06f, 0.06f, 15.6f), trim, level.transform);

        GameObject walls = new GameObject("InvisibleWalls_Triggers");
        Wall("WallNorth", new Vector3(0f, 1.5f, 8f), new Vector3(20.6f, 4f, 0.6f), Vector3.back, walls.transform);
        Wall("WallSouth", new Vector3(0f, 1.5f, -8f), new Vector3(20.6f, 4f, 0.6f), Vector3.forward, walls.transform);
        Wall("WallEast", new Vector3(10f, 1.5f, 0f), new Vector3(0.6f, 4f, 16.6f), Vector3.left, walls.transform);
        Wall("WallWest", new Vector3(-10f, 1.5f, 0f), new Vector3(0.6f, 4f, 16.6f), Vector3.right, walls.transform);

        GameObject kill = Primitive("KillZone", PrimitiveType.Cube, new Vector3(0f, -5f, 0f), new Vector3(80f, 2f, 80f), null);
        kill.GetComponent<MeshRenderer>().enabled = false;
        kill.GetComponent<BoxCollider>().isTrigger = true;
        kill.AddComponent<KillZone>();

        GameObject player = Primitive("Player", PrimitiveType.Sphere, new Vector3(0f, 0.6f, -5.5f), Vector3.one, playerMat);
        player.tag = "Player";
        Rigidbody body = player.AddComponent<Rigidbody>();
        body.mass = 1f;
        body.useGravity = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        player.AddComponent<PlayerController>();
        GameObject marker = Decoration("PlayerMarker", PrimitiveType.Cube, new Vector3(0f, 1.05f, -5.5f), new Vector3(0.08f, 0.035f, 0.38f), white);
        marker.transform.SetParent(player.transform, true);
        PracticeShortcuts shortcuts = manager.AddComponent<PracticeShortcuts>();
        SerializedObject shortcutData = new SerializedObject(shortcuts);
        shortcutData.FindProperty("playerBody").objectReferenceValue = body;
        shortcutData.ApplyModifiedPropertiesWithoutUndo();

        GameObject coins = new GameObject("Coins");
        Vector3[] positions = {
            new Vector3(-6f, 0.85f, -4f), new Vector3(0f, 0.85f, -2.5f),
            new Vector3(6f, 0.85f, -4f), new Vector3(-6f, 0.85f, 3.5f),
            new Vector3(0f, 0.85f, 4.5f), new Vector3(6f, 0.85f, 3.5f)
        };
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject coin = (GameObject)PrefabUtility.InstantiatePrefab(coinPrefab);
            coin.name = "Coin_" + (i + 1).ToString("00");
            coin.transform.SetParent(coins.transform);
            coin.transform.position = positions[i];
        }
        MakeLevelCamera();
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Root + "Scenes/Level1.unity");
        MakeMenu();
        EditorBuildSettings.scenes = new[] {
            new EditorBuildSettingsScene(Root + "Scenes/MenuPrincipal.unity", true),
            new EditorBuildSettingsScene(Root + "Scenes/Level1.unity", true)
        };
        AssetDatabase.SaveAssets();
        Debug.Log("UF2: scenes, prefab, materials and build settings generated successfully.");
    }

    private static void SetLegacyInput()
    {
        UnityEngine.Object settings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0];
        SerializedObject data = new SerializedObject(settings);
        SerializedProperty input = data.FindProperty("activeInputHandler");
        if (input != null) input.intValue = 0;
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddGroundTag()
    {
        SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = manager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == "Ground") return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = "Ground";
        manager.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Material Material(string name, Color color, float metallic, float smoothness)
    {
        string path = Root + "Materials/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = color;
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Glossiness", smoothness);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static GameObject Primitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material, Transform parent = null)
    {
        GameObject item = GameObject.CreatePrimitive(type);
        item.name = name;
        item.transform.SetParent(parent);
        item.transform.position = position;
        item.transform.localScale = scale;
        if (material != null) item.GetComponent<Renderer>().sharedMaterial = material;
        return item;
    }

    private static GameObject Decoration(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material, Transform parent = null)
    {
        GameObject item = Primitive(name, type, position, scale, material, parent);
        UnityEngine.Object.DestroyImmediate(item.GetComponent<Collider>());
        return item;
    }

    private static void Wall(string name, Vector3 position, Vector3 scale, Vector3 normal, Transform parent)
    {
        GameObject wall = Primitive(name, PrimitiveType.Cube, position, scale, null, parent);
        wall.GetComponent<MeshRenderer>().enabled = false;
        wall.GetComponent<BoxCollider>().isTrigger = true;
        BoundaryWall boundary = wall.AddComponent<BoundaryWall>();
        SerializedObject data = new SerializedObject(boundary);
        data.FindProperty("inwardNormal").vector3Value = normal;
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject MakeCoin(Material gold, Material face)
    {
        GameObject root = new GameObject("Coin");
        SphereCollider trigger = root.AddComponent<SphereCollider>();
        trigger.radius = 0.63f;
        trigger.isTrigger = true;
        root.AddComponent<Coin>();
        root.AddComponent<Spinner>();
        root.AddComponent<CoinFloat>();
        GameObject visual = Decoration("GoldDisc", PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.9f, 0.11f, 0.9f), gold, root.transform);
        visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        foreach (float side in new[] { -1f, 1f })
        {
            GameObject inset = Decoration("CoinFace", PrimitiveType.Cylinder, new Vector3(0f, 0f, side * 0.115f), new Vector3(0.68f, 0.012f, 0.68f), face, root.transform);
            inset.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Decoration("CoinSymbol", PrimitiveType.Cube, new Vector3(0f, 0f, side * 0.137f), new Vector3(0.1f, 0.38f, 0.018f), gold, root.transform);
        }
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, Root + "Prefabs/Coin.prefab");
        UnityEngine.Object.DestroyImmediate(root);
        return prefab;
    }

    private static void SetupEnvironment()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.55f, 0.65f, 0.78f);
        RenderSettings.skybox = null;
        RenderSettings.fog = false;
        GameObject lightObject = new GameObject("Directional Light", typeof(Light));
        Light light = lightObject.GetComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.5f;
        light.color = new Color(1f, 0.94f, 0.82f);
        light.shadows = LightShadows.Soft;
        lightObject.transform.rotation = Quaternion.Euler(52f, -30f, 0f);
    }

    private static void MakeLevelCamera()
    {
        GameObject go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        go.tag = "MainCamera";
        go.transform.position = new Vector3(0f, 19f, -18f);
        go.transform.LookAt(Vector3.zero);
        Camera camera = go.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 11.4f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.018f, 0.033f, 0.065f);
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 100f;
    }

    private static void MakeMenu()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraGo.tag = "MainCamera";
        Camera camera = cameraGo.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.018f, 0.033f, 0.065f);
        camera.orthographic = true;
        GameObject menu = new GameObject("MenuPrincipal", typeof(MenuPrincipal));
        GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1f;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        Panel("Accent", canvasGo.transform, new Vector2(0f, 189f), new Vector2(46f, 5f), new Color(0.1f, 0.85f, 0.75f));
        Label("Course", "UF2  /  NUCLI DEL JOC", canvasGo.transform, new Vector2(0f, 151f), new Vector2(900f, 40f), 17, new Color(0.2f, 0.85f, 0.8f));
        Label("Title", "COIN COLLECTOR", canvasGo.transform, new Vector2(0f, 78f), new Vector2(1100f, 90f), 61, Color.white, FontStyle.Bold);
        Label("Subtitle", "3D  /  PHYSICS PLAYGROUND", canvasGo.transform, new Vector2(0f, 15f), new Vector2(1000f, 45f), 18, new Color(0.55f, 0.65f, 0.78f));
        GameObject buttonGo = Panel("Play", canvasGo.transform, new Vector2(0f, -84f), new Vector2(260f, 64f), new Color(1f, 0.71f, 0.18f));
        Button button = buttonGo.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.88f, 0.6f);
        colors.pressedColor = new Color(0.8f, 0.65f, 0.3f);
        button.colors = colors;
        UnityEventTools.AddPersistentListener(button.onClick, menu.GetComponent<MenuPrincipal>().Play);
        Label("Label", "Play", buttonGo.transform, Vector2.zero, new Vector2(240f, 60f), 27, new Color(0.03f, 0.06f, 0.1f), FontStyle.Bold);
        Label("Controls", "WASD / FLETXES     Mou la bola i recull les 6 monedes", canvasGo.transform, new Vector2(0f, -179f), new Vector2(1100f, 40f), 18, new Color(0.7f, 0.77f, 0.85f));
        Label("Footer", "R  reinicia     /     F  prova la caiguda     /     ESC  menu", canvasGo.transform, new Vector2(0f, -280f), new Vector2(1100f, 36f), 15, new Color(0.4f, 0.5f, 0.63f));
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Root + "Scenes/MenuPrincipal.unity");
    }

    private static GameObject Panel(string name, Transform parent, Vector2 position, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        go.GetComponent<Image>().color = color;
        return go;
    }

    private static void Label(string name, string text, Transform parent, Vector2 position, Vector2 size, int fontSize, Color color, FontStyle style = FontStyle.Normal)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Text label = go.GetComponent<Text>();
        label.text = text;
        label.font = font;
        label.fontStyle = style;
        label.fontSize = fontSize;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = color;
        label.raycastTarget = false;
    }

    [MenuItem("UF2/Compilar Windows")]
    public static void BuildWindows()
    {
        Directory.CreateDirectory("Builds/Windows");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { Root + "Scenes/MenuPrincipal.unity", Root + "Scenes/Level1.unity" },
            locationPathName = "Builds/Windows/CoinCollector3D.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new Exception("Windows build failed: " + report.summary.result);
        Debug.Log("UF2: Windows build succeeded.");
    }
}
