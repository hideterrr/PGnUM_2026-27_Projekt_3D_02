using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

public class SetupEgyptianGameScene
{
    [MenuItem("Tools/Setup Egyptian Game Scene")]
    public static void SetupScene()
    {
        Debug.Log("Starting setup of Egyptian Game Scene...");

        // Ensure newly generated PNGs are imported as 2D Sprites
        ConfigureTextureAsSprite("Assets/Sprites/Nazwa_gra2.png");
        ConfigureTextureAsSprite("Assets/Sprites/Rozpocznij_gra2.png");
        ConfigureTextureAsSprite("Assets/Sprites/Koniec_gra2.png");
        ConfigureTextureAsSprite("Assets/Sprites/Skarabeuszek_gra2.png");
        ConfigureTextureAsSprite("Assets/Sprites/Tło_gra2.jpg");
        AssetDatabase.Refresh();

        string scenePath = "Assets/Scenes/SampleScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        // Find sprites
        Sprite tombSprite = LoadSpriteByPath("Assets/Sprites/Tło_gra2.jpg");
        if (tombSprite == null) tombSprite = LoadSpriteByFilter("Tło_gra2");
        if (tombSprite == null) tombSprite = LoadSpriteByFilter("To_gra2");

        Sprite scarabSprite = LoadSpriteByPath("Assets/Sprites/Skarabeuszek_gra2.png");
        if (scarabSprite == null) scarabSprite = LoadSpriteByFilter("Skarabeuszek_gra2");

        Sprite titleSprite = LoadSpriteByPath("Assets/Sprites/Nazwa_gra2.png");
        if (titleSprite == null) titleSprite = LoadSpriteByFilter("Nazwa_gra2");

        Sprite startBtnSprite = LoadSpriteByPath("Assets/Sprites/Rozpocznij_gra2.png");
        if (startBtnSprite == null) startBtnSprite = LoadSpriteByFilter("Rozpocznij_gra2");

        Sprite gameOverSprite = LoadSpriteByPath("Assets/Sprites/Koniec_gra2.png");
        if (gameOverSprite == null) gameOverSprite = LoadSpriteByFilter("Koniec_gra2");

        Debug.Log($"Loaded Sprites: Tomb={tombSprite != null}, Scarab={scarabSprite != null}, Title={titleSprite != null}, StartBtn={startBtnSprite != null}, GameOver={gameOverSprite != null}");

        // 1. Camera Setup
        Camera cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
        }
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.transform.position = new Vector3(0f, 0f, -10f);

        // 2. Tomb Background
        GameObject tombGo = GameObject.Find("TombBackground");
        if (tombGo == null) tombGo = new GameObject("TombBackground");

        var tombSr = tombGo.GetComponent<SpriteRenderer>();
        if (tombSr == null) tombSr = tombGo.AddComponent<SpriteRenderer>();
        tombSr.sprite = tombSprite;
        tombSr.sortingOrder = -10;

        var tombScaler = tombGo.GetComponent<TombBackgroundScaler>();
        if (tombScaler == null) tombScaler = tombGo.AddComponent<TombBackgroundScaler>();
        tombScaler.FitToScreen();

        // 3. EventSystem with modern InputSystemUIInputModule
        var eventSystem = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            var esGo = new GameObject("EventSystem");
            eventSystem = esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
        }

        // Remove legacy StandaloneInputModule to eliminate InvalidOperationException
        var legacyModule = eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        if (legacyModule != null)
        {
            Object.DestroyImmediate(legacyModule);
        }

        var inputModule = eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        if (inputModule == null)
        {
            eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // 4. GameController (GameManager, ScarabSpawner, UIManager)
        GameObject controllerGo = GameObject.Find("GameController");
        if (controllerGo == null) controllerGo = new GameObject("GameController");

        var gm = controllerGo.GetComponent<GameManager>();
        if (gm == null) gm = controllerGo.AddComponent<GameManager>();
        gm.gameDuration = 45f;

        var spawner = controllerGo.GetComponent<ScarabSpawner>();
        if (spawner == null) spawner = controllerGo.AddComponent<ScarabSpawner>();
        spawner.scarabSprite = scarabSprite;
        spawner.spawnY = 5.8f;
        spawner.minX = -2.1f;
        spawner.maxX = 2.1f;

        var uiMgr = controllerGo.GetComponent<UIManager>();
        if (uiMgr == null) uiMgr = controllerGo.AddComponent<UIManager>();
        uiMgr.titleSprite = titleSprite;
        uiMgr.startButtonSprite = startBtnSprite;
        uiMgr.gameOverSprite = gameOverSprite;

        // Clean up any existing Canvas to recreate cleanly
        var existingCanvas = Object.FindFirstObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            Object.DestroyImmediate(existingCanvas.gameObject);
        }

        // Trigger UI Hierarchy creation with new transparent sprites
        uiMgr.EnsureUIHierarchyExists();

        // Save scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        // Set Project Settings
        PlayerSettings.productName = "Klątwa Skarbnika";
        PlayerSettings.companyName = "UJD";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

        AssetDatabase.SaveAssets();
        Debug.Log("Egyptian Game Scene setup completed successfully!");
    }

    private static void ConfigureTextureAsSprite(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            bool modified = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                modified = true;
            }
            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                modified = true;
            }
            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                modified = true;
            }
            if (modified)
            {
                importer.SaveAndReimport();
            }
        }
    }

    private static Sprite LoadSpriteByPath(string assetPath)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        if (assets != null)
        {
            foreach (var a in assets)
            {
                if (a is Sprite s) return s;
            }
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static Sprite LoadSpriteByFilter(string nameFilter)
    {
        string[] guids = AssetDatabase.FindAssets(nameFilter + " t:Sprite");
        if (guids != null && guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a is Sprite s) return s;
            }
        }

        guids = AssetDatabase.FindAssets(nameFilter + " t:Texture2D");
        if (guids != null && guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a is Sprite s) return s;
            }
        }

        return null;
    }
}
