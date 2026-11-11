using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class KitchenCounterLevelBuilder
{
    private const string ScenePath = "Assets/Scenes/KitchenCounterLevel.unity";
    private const string MaterialFolder = "Assets/Art/KitchenCounter/Materials";
    private const string TextureFolder = "Assets/Art/KitchenCounter/Textures";
    private const string DocumentationFolder = "Assets/Documentation";

    private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

    [MenuItem("Tools/Burger Bash/Generate Kitchen Counter Level")]
    public static void Generate()
    {
        EnsureFolders();
        CreateMaterials();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "KitchenCounterLevel";

        ConfigureEnvironment();
        BuildArchitecture();
        BuildSetDressing();
        BuildGameplayLayout();
        BuildLightingAndCameras();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        RenderSceneImage("Main Camera", DocumentationFolder + "/KitchenCounter_Perspective.png", 1600, 900);
        RenderSceneImage("Overview Camera", DocumentationFolder + "/KitchenCounter_Overview.png", 1600, 1000);
        AssetDatabase.Refresh();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        Selection.activeGameObject = GameObject.Find("LEVEL GUIDE - Select Me");
        Debug.Log("Burger Bash kitchen counter level generated at " + ScenePath);
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets", "Editor");
        EnsureFolder("Assets", "Art");
        EnsureFolder("Assets/Art", "KitchenCounter");
        EnsureFolder("Assets/Art/KitchenCounter", "Materials");
        EnsureFolder("Assets/Art/KitchenCounter", "Textures");
        EnsureFolder("Assets", "Documentation");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
    }

    private static void CreateMaterials()
    {
        Materials.Clear();
        CreatePattern("Counter", new Color(0.77f, 0.73f, 0.64f), new Color(0.64f, 0.60f, 0.53f), 16, 0.25f, 0.25f);
        CreatePattern("Wood", new Color(0.66f, 0.34f, 0.13f), new Color(0.43f, 0.20f, 0.07f), 8, 0.18f, 0.05f);
        CreatePattern("Tile", new Color(0.91f, 0.92f, 0.86f), new Color(0.57f, 0.68f, 0.66f), 16, 0.15f, 0.25f);
        CreatePattern("Steel", new Color(0.50f, 0.56f, 0.59f), new Color(0.29f, 0.34f, 0.37f), 32, 0.85f, 0.65f);
        CreateSolid("DarkSteel", new Color(0.10f, 0.12f, 0.14f), 0.75f, 0.75f);
        CreateSolid("Plate", new Color(0.88f, 0.93f, 0.96f), 0.55f, 0.2f);
        CreateSolid("Bread", new Color(0.88f, 0.58f, 0.22f), 0.18f, 0f);
        CreateSolid("Patty", new Color(0.23f, 0.08f, 0.035f), 0.25f, 0f);
        CreateSolid("Cheese", new Color(1.0f, 0.69f, 0.08f), 0.18f, 0f);
        CreateSolid("Hazard", new Color(0.90f, 0.12f, 0.08f), 0.25f, 0f);
        CreateSolid("Enemy", new Color(0.72f, 0.05f, 0.42f), 0.25f, 0f);
        CreateSolid("Item", new Color(1.0f, 0.85f, 0.10f), 0.25f, 0.15f, true);
        CreateSolid("Checkpoint", new Color(0.15f, 0.90f, 0.45f), 0.2f, 0.1f, true);
        CreateSolid("Route", new Color(0.08f, 0.72f, 1.0f), 0.15f, 0f, true);
        CreateSolid("Shortcut", new Color(0.70f, 0.30f, 1.0f), 0.15f, 0f, true);
        CreateSolid("Water", new Color(0.05f, 0.43f, 0.75f, 0.72f), 0.65f, 0.25f, true);
        CreateSolid("Goal", new Color(1.0f, 0.38f, 0.05f), 0.2f, 0.05f, true);
        CreateSolid("Green", new Color(0.18f, 0.55f, 0.13f), 0.25f, 0f);
        CreateSolid("Red", new Color(0.75f, 0.08f, 0.06f), 0.25f, 0f);
        CreateSolid("White", Color.white, 0.3f, 0f);
    }

    private static void CreatePattern(string name, Color a, Color b, int cell, float smoothness, float metallic)
    {
        string texturePath = TextureFolder + "/" + name + "_Pattern.png";
        var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        for (int y = 0; y < texture.height; y++)
        for (int x = 0; x < texture.width; x++)
        {
            bool alternate = ((x / cell) + (y / cell)) % 2 == 0;
            Color baseColor = alternate ? a : b;
            float noise = ((x * 17 + y * 31) % 19) / 400f;
            texture.SetPixel(x, y, baseColor * (0.96f + noise));
        }
        texture.Apply();
        File.WriteAllBytes(texturePath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
        importer.wrapMode = TextureWrapMode.Repeat;
        importer.filterMode = FilterMode.Bilinear;
        importer.SaveAndReimport();
        CreateMaterialAsset(name, a, smoothness, metallic, AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath), false);
    }

    private static void CreateSolid(string name, Color color, float smoothness, float metallic, bool emission = false)
    {
        CreateMaterialAsset(name, color, smoothness, metallic, null, emission);
    }

    private static void CreateMaterialAsset(string name, Color color, float smoothness, float metallic, Texture2D texture, bool emission)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
        if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (emission && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 0.75f);
        }
        EditorUtility.SetDirty(material);
        Materials[name] = material;
    }

    private static void ConfigureEnvironment()
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.48f, 0.55f, 0.64f);
        RenderSettings.ambientEquatorColor = new Color(0.28f, 0.29f, 0.30f);
        RenderSettings.ambientGroundColor = new Color(0.12f, 0.10f, 0.09f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.67f, 0.74f, 0.78f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 28f;
        RenderSettings.fogEndDistance = 65f;
    }

    private static void BuildArchitecture()
    {
        var root = Group("01 ARCHITECTURE");
        Cube("Long_Countertop_PlaySurface", new Vector3(0, 0, 0), new Vector3(14, 1, 48), "Counter", root);
        Cube("Counter_LeftFace", new Vector3(-7.1f, -2.5f, 0), new Vector3(0.4f, 5, 48), "Wood", root);
        Cube("Counter_RightFace", new Vector3(7.1f, -2.5f, 0), new Vector3(0.4f, 5, 48), "Wood", root);
        Cube("Backsplash", new Vector3(0, 4.5f, 24.1f), new Vector3(14.5f, 9, 0.5f), "Tile", root);

        // Start plate at the near end of the long counter.
        var startPlate = Cylinder("Start_Plate_VisualOnly", new Vector3(0, 0.51f, -20f), new Vector3(5.4f, 0.02f, 5.4f), "Plate", root);
        Object.DestroyImmediate(startPlate.GetComponent<Collider>());

        // Cutting-board section.
        Cube("Cutting_Board", new Vector3(0, 0.78f, -10f), new Vector3(11.5f, 0.55f, 8f), "Wood", root);

        // Sink section with two walkable side lanes. The water is a static visual hazard.
        Cube("Sink_Rim_Front", new Vector3(0, 0.86f, -1.7f), new Vector3(12f, 0.72f, 1f), "Steel", root);
        Cube("Sink_Rim_Back", new Vector3(0, 0.86f, 6.7f), new Vector3(12f, 0.72f, 1f), "Steel", root);
        Cube("Sink_Rim_Left", new Vector3(-5.5f, 0.86f, 2.5f), new Vector3(1f, 0.72f, 9.4f), "Steel", root);
        Cube("Sink_Rim_Right", new Vector3(5.5f, 0.86f, 2.5f), new Vector3(1f, 0.72f, 9.4f), "Steel", root);
        var sinkWater = Cube("Sink_Water_RespawnTrigger", new Vector3(0, 0.58f, 2.5f), new Vector3(10f, 0.12f, 7.5f), "Water", root);
        sinkWater.tag = "Respawn";
        sinkWater.GetComponent<Collider>().isTrigger = true;

        // Serving area at the far end.
        Cube("Serving_Mat", new Vector3(0, 0.67f, 16.2f), new Vector3(12f, 0.28f, 12.5f), "Tile", root);
    }

    private static void BuildSetDressing()
    {
        var root = Group("02 KITCHEN PROPS");
        // Cutting-board obstacles: a giant knife and chopped ingredients.
        Cube("Knife_Blade", new Vector3(-1.3f, 1.45f, -10.2f), new Vector3(8f, 0.22f, 1.2f), "Steel", root, new Vector3(0, 25, 0));
        Cube("Knife_Handle", new Vector3(3.3f, 1.45f, -8.0f), new Vector3(2.8f, 0.6f, 1.3f), "DarkSteel", root, new Vector3(0, 25, 0));
        Sphere("Chopped_Tomato_A", new Vector3(-3.2f, 1.45f, -7.4f), new Vector3(1.4f, 0.45f, 1.4f), "Red", root);
        Sphere("Chopped_Tomato_B", new Vector3(3.5f, 1.45f, -12.3f), new Vector3(1.4f, 0.45f, 1.4f), "Red", root);
        Sphere("Lettuce_Pile", new Vector3(2.4f, 1.55f, -7.0f), new Vector3(1.8f, 0.75f, 1.8f), "Green", root);

        // Faucet landmark above the sink.
        Cylinder("Faucet_Base", new Vector3(0, 2.2f, 6.3f), new Vector3(1.2f, 3f, 1.2f), "Steel", root);
        Cube("Faucet_Neck", new Vector3(0, 4.4f, 5.0f), new Vector3(0.8f, 0.8f, 3.2f), "Steel", root);
        Cube("Faucet_Spout", new Vector3(0, 3.6f, 3.6f), new Vector3(0.8f, 1.8f, 0.8f), "Steel", root);

        // End-of-counter dishes and oversized food form a loose obstacle course.
        ServingPlate("Serving_Plate_Left", new Vector3(-3.3f, 0.82f, 12.2f), root);
        ServingPlate("Serving_Plate_Right", new Vector3(3.2f, 0.82f, 15.2f), root);
        ServingPlate("Dessert_Plate_End", new Vector3(-2.2f, 0.82f, 20.2f), root);
        Sphere("Apple", new Vector3(-3.3f, 2.0f, 12.2f), new Vector3(2.0f, 2.0f, 2.0f), "Red", root);
        Cylinder("Pancake_Stack_1", new Vector3(3.2f, 1.35f, 15.2f), new Vector3(2.5f, 0.28f, 2.5f), "Bread", root);
        Cylinder("Pancake_Stack_2", new Vector3(3.2f, 1.8f, 15.2f), new Vector3(2.35f, 0.25f, 2.35f), "Bread", root);
        Cylinder("Pancake_Stack_3", new Vector3(3.2f, 2.2f, 15.2f), new Vector3(2.15f, 0.22f, 2.15f), "Bread", root);
        Sphere("Dessert_Dome", new Vector3(-2.2f, 1.6f, 20.2f), new Vector3(2.2f, 1.2f, 2.2f), "Cheese", root);
    }

    private static void BuildGameplayLayout()
    {
        var root = Group("03 GAMEPLAY PLACEHOLDERS");
        var guide = new GameObject("LEVEL GUIDE - Select Me");
        guide.transform.SetParent(root.transform);
        var note = guide.AddComponent<LevelDesignNotes>();
        note.levelTitle = "The Long Counter";
        note.objective = "Move straight down the counter from the starting plate, cross the cutting board, choose a side of the sink, and reach the order bell beyond the serving dishes.";
        note.primaryRoute = "WASD or arrow keys to move; Space to jump.";
        note.optionalRoute = "The burger starts on the plate at the near end of the counter.";

        // The plate is visual-only and nearly flush with the counter, so there is no
        // raised collision rim to catch the player. The counter supports the burger.
        BuildBurgerPlayer(new Vector3(0f, 0.81f, -20f), root);

        // Static enemy placeholders positioned along the forward route.
        Enemy("Enemy_Ant_CuttingBoard", new Vector3(-3.4f, 1.65f, -5.7f), root);
        Enemy("Enemy_Cockroach_SinkExit", new Vector3(4.0f, 1.65f, 8.5f), root, new Vector3(1.3f, 1.3f, 1.3f));
        Checkpoint("Checkpoint_AfterSink", new Vector3(0f, 1.15f, 9.0f), root);
        Goal("Goal_OrderBell", new Vector3(0f, 1.35f, 22.0f), root);
    }

    private static void BuildLightingAndCameras()
    {
        var root = Group("05 LIGHTING AND CAMERAS");
        var sunObject = new GameObject("Directional Light");
        sunObject.transform.SetParent(root.transform);
        sunObject.transform.rotation = Quaternion.Euler(48, -32, 0);
        var sun = sunObject.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.35f;
        sun.color = new Color(1.0f, 0.91f, 0.78f);
        sun.shadows = LightShadows.Soft;
        RenderSettings.sun = sun;

        PointLight("Warm Fill", new Vector3(-4, 11, -9), new Color(1.0f, 0.55f, 0.30f), 650f, 30f, root);
        PointLight("Cool Fill", new Vector3(4, 10, 13), new Color(0.35f, 0.65f, 1.0f), 600f, 32f, root);

        var player = GameObject.Find("PLAYER_START_Burger");
        var main = CameraObject("Main Camera", new Vector3(0, 8.8f, -28.5f), new Vector3(0, 2.8f, -17.5f), 58f, root);
        main.gameObject.tag = "MainCamera";
        main.depth = 0;
        main.clearFlags = CameraClearFlags.Skybox;
        var follow = main.gameObject.AddComponent<BurgerCameraFollow>();
        if (player != null) follow.SetTarget(player.transform);

        var overview = CameraObject("Overview Camera", new Vector3(0, 55, 0), new Vector3(0, 0, 0), 60f, root);
        overview.orthographic = true;
        overview.orthographicSize = 26f;
        overview.aspect = 1.6f;
        overview.clearFlags = CameraClearFlags.SolidColor;
        overview.backgroundColor = new Color(0.08f, 0.10f, 0.12f);
        overview.enabled = false;
    }

    private static void BuildBurgerPlayer(Vector3 position, GameObject parent)
    {
        var burger = new GameObject("PLAYER_START_Burger");
        burger.transform.position = position;
        burger.transform.SetParent(parent.transform);
        Cylinder("Bottom_Bun", position, new Vector3(1.2f, 0.28f, 1.2f), "Bread", burger);
        Cylinder("Patty", position + Vector3.up * 0.28f, new Vector3(1.12f, 0.18f, 1.12f), "Patty", burger);
        Cube("Cheese", position + Vector3.up * 0.46f, new Vector3(1.2f, 0.09f, 1.2f), "Cheese", burger, new Vector3(0, 12, 0));
        Sphere("Top_Bun", position + Vector3.up * 0.68f, new Vector3(1.2f, 0.45f, 1.2f), "Bread", burger);
        var body = burger.AddComponent<Rigidbody>();
        body.mass = 1.2f;
        body.freezeRotation = true;
        burger.AddComponent<BurgerPlayerController>();
    }

    private static void Enemy(string name, Vector3 position, GameObject parent, Vector3? scale = null)
    {
        Cube(name, position, scale ?? Vector3.one, "Enemy", parent, new Vector3(0, 35, 0));
    }

    private static void Item(string name, Vector3 position, GameObject parent, float scale = 0.4f)
    {
        Sphere(name, position, Vector3.one * scale, "Item", parent);
    }

    private static void Checkpoint(string name, Vector3 position, GameObject parent)
    {
        Cylinder(name, position, new Vector3(1.15f, 0.12f, 1.15f), "Checkpoint", parent);
    }

    private static void Goal(string name, Vector3 position, GameObject parent)
    {
        Cylinder(name + "_Base", position, new Vector3(1.4f, 0.25f, 1.4f), "Goal", parent);
        Sphere(name + "_Bell", position + Vector3.up * 0.5f, new Vector3(1.1f, 0.55f, 1.1f), "Goal", parent);
        Cylinder(name + "_Button", position + Vector3.up * 0.93f, new Vector3(0.35f, 0.18f, 0.35f), "DarkSteel", parent);
    }

    private static void SpiceJar(string name, Vector3 position, string material, GameObject parent)
    {
        Cylinder(name, position, new Vector3(1.1f, 1.55f, 1.1f), material, parent);
        Cylinder(name + "_Lid", position + Vector3.up * 0.9f, new Vector3(1.2f, 0.2f, 1.2f), "DarkSteel", parent);
    }

    private static void ServingPlate(string name, Vector3 position, GameObject parent)
    {
        Cylinder(name, position, new Vector3(4.2f, 0.18f, 4.2f), "Plate", parent);
        Cylinder(name + "_Inner", position + Vector3.up * 0.2f, new Vector3(3.3f, 0.08f, 3.3f), "White", parent);
    }

    private static void RouteArrow(string name, Vector3 from, Vector3 to, string material, GameObject parent)
    {
        Vector3 flatFrom = from;
        Vector3 flatTo = to;
        Vector3 delta = flatTo - flatFrom;
        float length = delta.magnitude;
        Vector3 midpoint = (flatFrom + flatTo) * 0.5f;
        float yaw = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;
        Cube(name + "_Line", midpoint, new Vector3(0.16f, 0.055f, Mathf.Max(0.15f, length - 0.35f)), material, parent, new Vector3(0, yaw, 0));
        Vector3 backward = -delta.normalized;
        Vector3 left = Quaternion.Euler(0, 32, 0) * backward;
        Vector3 right = Quaternion.Euler(0, -32, 0) * backward;
        Cube(name + "_HeadL", flatTo + left * 0.32f, new Vector3(0.14f, 0.06f, 0.75f), material, parent, new Vector3(0, yaw + 32, 0));
        Cube(name + "_HeadR", flatTo + right * 0.32f, new Vector3(0.14f, 0.06f, 0.75f), material, parent, new Vector3(0, yaw - 32, 0));
    }

    private static GameObject Label(string text, Vector3 position, float size, Color color, GameObject parent)
    {
        var label = new GameObject("Label_" + text.Replace(" ", "_"));
        label.transform.SetParent(parent.transform);
        label.transform.position = position;
        label.transform.rotation = Quaternion.Euler(90, 0, 0);
        var mesh = label.AddComponent<TextMesh>();
        mesh.text = text;
        mesh.fontSize = 64;
        mesh.characterSize = size / 10f;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        mesh.color = color;
        return label;
    }

    private static GameObject Group(string name)
    {
        return new GameObject(name);
    }

    private static GameObject Cube(string name, Vector3 position, Vector3 scale, string material, GameObject parent, Vector3? rotation = null)
    {
        return Primitive(PrimitiveType.Cube, name, position, scale, material, parent, rotation ?? Vector3.zero);
    }

    private static GameObject Sphere(string name, Vector3 position, Vector3 scale, string material, GameObject parent, Vector3? rotation = null)
    {
        return Primitive(PrimitiveType.Sphere, name, position, scale, material, parent, rotation ?? Vector3.zero);
    }

    private static GameObject Cylinder(string name, Vector3 position, Vector3 scale, string material, GameObject parent, Vector3? rotation = null)
    {
        return Primitive(PrimitiveType.Cylinder, name, position, scale, material, parent, rotation ?? Vector3.zero);
    }

    private static GameObject Primitive(PrimitiveType type, string name, Vector3 position, Vector3 scale, string material, GameObject parent, Vector3 rotation)
    {
        var obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent.transform);
        obj.transform.position = position;
        obj.transform.eulerAngles = rotation;
        obj.transform.localScale = scale;
        var renderer = obj.GetComponent<Renderer>();
        renderer.sharedMaterial = Materials[material];
        return obj;
    }

    private static void PointLight(string name, Vector3 position, Color color, float intensity, float range, GameObject parent)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent.transform);
        obj.transform.position = position;
        var light = obj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.Soft;
    }

    private static Camera CameraObject(string name, Vector3 position, Vector3 lookAt, float fieldOfView, GameObject parent)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent.transform);
        obj.transform.position = position;
        obj.transform.LookAt(lookAt);
        var camera = obj.AddComponent<Camera>();
        camera.fieldOfView = fieldOfView;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 120f;
        camera.allowHDR = true;
        return camera;
    }

    private static void RenderSceneImage(string cameraName, string outputPath, int width, int height)
    {
        var cameraObject = GameObject.Find(cameraName);
        if (cameraObject == null) return;
        var camera = cameraObject.GetComponent<Camera>();
        var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        var image = new Texture2D(width, height, TextureFormat.RGB24, false);
        RenderTexture previous = RenderTexture.active;
        camera.targetTexture = renderTexture;
        camera.Render();
        RenderTexture.active = renderTexture;
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();
        File.WriteAllBytes(outputPath, image.EncodeToPNG());
        camera.targetTexture = null;
        RenderTexture.active = previous;
        Object.DestroyImmediate(renderTexture);
        Object.DestroyImmediate(image);
    }

    private static void AddSceneToBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool exists = scenes.Exists(s => s.path == ScenePath);
        if (!exists) scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}

public class LevelDesignNotes : MonoBehaviour
{
    [TextArea] public string levelTitle;
    [TextArea(3, 6)] public string objective;
    [TextArea] public string primaryRoute;
    [TextArea] public string optionalRoute;
}
