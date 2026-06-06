using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.AI;
using System.IO;

public class ZombieSetupWizard : EditorWindow
{
    private string zombieFBXPath = "Assets/Zombies/zombie-3d-model-free-by-oscar-creativo/source/ZOMBIEN_3D_MODEL_FREE_BY_Oscar_Creativo.fbx";
    private string outputPrefabPath = "Assets/Zombies/ZombiePrefab.prefab";
    private string canvasPath = "";

    [MenuItem("Zombies/Setup Horde System", false, 1)]
    public static void OpenWizard()
    {
        GetWindow<ZombieSetupWizard>("Zombie Horde Setup");
    }

    [MenuItem("Zombies/Create Zombie Prefab", false, 2)]
    public static void CreateZombiePrefab()
    {
        CreateZombiePrefabStatic();
    }

    [MenuItem("Zombies/Add Spawner To Scene", false, 3)]
    public static void AddSpawnerToScene()
    {
        AddSpawner();
    }

    [MenuItem("Zombies/Add HUD Canvas", false, 4)]
    public static void AddHUDCanvas()
    {
        CreateHUDCanvas();
    }

    [MenuItem("Zombies/Full Auto Setup", false, 0)]
    public static void FullAutoSetup()
    {
        GameObject prefab = CreateZombiePrefabStatic();
        if (prefab != null)
        {
            AddSpawner();
            CreateHUDCanvas();
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("Zombie horde system fully set up!");
        }
    }

    void OnGUI()
    {
        GUILayout.Label("Zombie Horde System Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        zombieFBXPath = EditorGUILayout.TextField("Zombie FBX Path", zombieFBXPath);
        outputPrefabPath = EditorGUILayout.TextField("Output Prefab Path", outputPrefabPath);

        EditorGUILayout.Space();

        if (GUILayout.Button("1. Create Zombie Prefab", GUILayout.Height(30)))
        {
            CreateZombiePrefabStatic();
        }

        if (GUILayout.Button("2. Add Spawner To Scene", GUILayout.Height(30)))
        {
            AddSpawner();
        }

        if (GUILayout.Button("3. Add HUD Canvas", GUILayout.Height(30)))
        {
            CreateHUDCanvas();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("FULL AUTO SETUP (1+2+3)", GUILayout.Height(40)))
        {
            FullAutoSetup();
        }
    }

    static GameObject CreateZombiePrefabStatic()
    {
        string fbxPath = "Assets/Zombies/zombie-3d-model-free-by-oscar-creativo/source/ZOMBIEN_3D_MODEL_FREE_BY_Oscar_Creativo.fbx";
        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (fbx == null)
        {
            Debug.LogError($"FBX not found at: {fbxPath}");
            return null;
        }

        string prefabPath = "Assets/Zombies/ZombiePrefab.prefab";

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
        instance.name = "Zombie";

        instance.layer = LayerMask.NameToLayer("Default");

        NavMeshAgent agent = instance.GetComponent<NavMeshAgent>();
        if (agent == null) agent = instance.AddComponent<NavMeshAgent>();
        agent.speed = 2.5f;
        agent.angularSpeed = 360;
        agent.acceleration = 12f;
        agent.stoppingDistance = 1.5f;
        agent.radius = 0.3f;
        agent.height = 1.8f;
        agent.avoidancePriority = 50;

        Animator anim = instance.GetComponent<Animator>();
        if (anim == null) anim = instance.AddComponent<Animator>();
        anim.applyRootMotion = false;

        AudioSource audio = instance.GetComponent<AudioSource>();
        if (audio == null) audio = instance.AddComponent<AudioSource>();
        audio.spatialBlend = 1f;
        audio.maxDistance = 30f;
        audio.rolloffMode = AudioRolloffMode.Logarithmic;

        CapsuleCollider col = instance.GetComponent<CapsuleCollider>();
        if (col == null)
        {
            col = instance.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0, 0.9f, 0);
            col.radius = 0.3f;
            col.height = 1.8f;
        }

        Zombie zombie = instance.GetComponent<Zombie>();
        if (zombie == null) zombie = instance.AddComponent<Zombie>();
        zombie.health = 60f;
        zombie.walkSpeed = 1.8f;
        zombie.rushSpeed = 4f;
        zombie.attackDamage = 20f;
        zombie.sprayPerHit = 20;
        zombie.splatterOnDeath = 60;
        zombie.geyserOnDeath = 50;
        zombie.destroyAfterDelay = 8f;

        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        DestroyImmediate(instance);

        Debug.Log($"Zombie prefab created at: {prefabPath}");
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
    }

    static void AddSpawner()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (scene == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("FPSController");
            if (player == null)
            {
                Debug.LogWarning("No Player found. Spawner placed at origin.");
            }
        }

        GameObject spawnerObj = new GameObject("ZombieSpawner");
        if (player != null) spawnerObj.transform.position = player.transform.position;

        ZombieSpawner spawner = spawnerObj.AddComponent<ZombieSpawner>();

        string prefabPath = "Assets/Zombies/ZombiePrefab.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            spawner.zombiePrefab = prefab;
        }
        else
        {
            Debug.LogWarning("Zombie prefab not found at " + prefabPath + ". Please assign it manually.");
        }

        spawner.minSpawnDist = 30f;
        spawner.maxSpawnDist = 60f;
        spawner.maxAlive = 50;
        spawner.spawnBatchSize = 5;
        spawner.baseZombiesPerWave = 5;
        spawner.baseHealth = 50f;
        spawner.enableRush = true;

        HordeController horde = spawnerObj.AddComponent<HordeController>();
        horde.spawner = spawner;
        horde.playerCamera = Camera.main;

        Selection.activeGameObject = spawnerObj;
        Debug.Log("ZombieSpawner added to scene.");
    }

    static void CreateHUDCanvas()
    {
        Scene scene = EditorSceneManager.GetActiveScene();

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("HordeHUD");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            Canvas existing = FindObjectOfType<Canvas>();
            if (existing != null && existing != canvas)
            {
                DestroyImmediate(canvasObj);
                canvas = existing;
            }
        }

        
        ZombieSpawner spawner = FindObjectOfType<ZombieSpawner>();
        HordeController horde = FindObjectOfType<HordeController>();

        CreateUIText(canvas, "WaveText", "WAVE 1", new Vector2(0.5f, 0.9f), new Vector2(0.5f, 0.5f), 36, Color.white, out Text waveText);
        CreateUIText(canvas, "AliveText", "0", new Vector2(0.9f, 0.9f), new Vector2(0.5f, 0.5f), 28, Color.red, out Text aliveText);
        CreateUIText(canvas, "KillsText", "KILLS: 0", new Vector2(0.1f, 0.9f), new Vector2(0.5f, 0.5f), 24, Color.white, out Text killsText);
        CreateUIText(canvas, "CountdownText", "", new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), 24, Color.yellow, out Text countdownText);

        GameObject announceObj = new GameObject("WaveAnnouncement");
        announceObj.transform.SetParent(canvas.transform, false);
        RectTransform announceRect = announceObj.AddComponent<RectTransform>();
        announceRect.anchorMin = new Vector2(0.5f, 0.5f);
        announceRect.anchorMax = new Vector2(0.5f, 0.5f);
        announceRect.pivot = new Vector2(0.5f, 0.5f);
        announceRect.sizeDelta = new Vector2(600, 100);
        announceRect.anchoredPosition = Vector2.zero;
        Text announceText = announceObj.AddComponent<Text>();
        announceText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        announceText.fontSize = 48;
        announceText.alignment = TextAnchor.MiddleCenter;
        announceText.color = Color.red;
        announceText.fontStyle = FontStyle.Bold;
        announceObj.SetActive(false);

        Image flashImg = CreateScreenFlash(canvas);

        if (spawner != null)
        {
            spawner.waveText = waveText;
            spawner.aliveText = aliveText;
            spawner.killsText = killsText;
            spawner.countdownText = countdownText;
            spawner.announceObj = announceObj;
            spawner.announceText = announceText;
            spawner.screenFlash = flashImg;
            EditorUtility.SetDirty(spawner);
        }

        if (horde != null)
        {
            horde.streakText = CreateStreakText(canvas);
            EditorUtility.SetDirty(horde);
        }

        Debug.Log("Horde HUD created.");
    }

    static void CreateUIText(Canvas canvas, string name, string text, Vector2 anchor, Vector2 pivot, int fontSize, Color color, out Text textComp)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(canvas.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.sizeDelta = new Vector2(300, 50);
        rt.anchoredPosition = Vector2.zero;

        textComp = go.AddComponent<Text>();
        textComp.text = text;
        textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComp.fontSize = fontSize;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = color;
        textComp.fontStyle = FontStyle.Bold;

        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, 2);
    }

    static Image CreateScreenFlash(Canvas canvas)
    {
        GameObject go = new GameObject("ScreenFlash");
        go.transform.SetParent(canvas.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        Image img = go.AddComponent<Image>();
        img.color = Color.clear;
        img.raycastTarget = false;
        return img;
    }

    static Text CreateStreakText(Canvas canvas)
    {
        GameObject go = new GameObject("StreakText");
        go.transform.SetParent(canvas.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.4f);
        rt.anchorMax = new Vector2(0.5f, 0.4f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(500, 80);
        rt.anchoredPosition = Vector2.zero;

        Text text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 48;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.red;
        text.fontStyle = FontStyle.Bold;

        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(3, 3);

        go.SetActive(false);
        return text;
    }
}
