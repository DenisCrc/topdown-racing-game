using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Rebuilds the authored UI and vehicle assets. Runtime scripts only bind data and navigate.
public static class RacingProjectSetup
{
    static PastelTheme theme;
    static GameObject buttonPrefab;
    static GameObject panelPrefab;
    static GameObject screenPrefab;
    const string Prefabs = "Assets/Prefabs/";

    [MenuItem("Tools/Racing/Rebuild Pastel Assets and Scenes")]
    public static void Run()
    {
        Directory.CreateDirectory("Assets/Resources/Cars");
        AssetDatabase.Refresh();
        theme = AssetDatabase.LoadAssetAtPath<PastelTheme>("Assets/Resources/PastelTheme.asset");
        if (theme == null)
        {
            theme = ScriptableObject.CreateInstance<PastelTheme>();
            theme.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            AssetDatabase.CreateAsset(theme, "Assets/Resources/PastelTheme.asset");
        }
        Material carMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/CarMaterial.mat");
        if (carMaterial == null)
        {
            carMaterial = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(carMaterial, "Assets/Resources/CarMaterial.mat");
        }
        BuildCars();
        BuildSmoke(carMaterial);
        BuildSharedPrefabs();
        BuildMainMenu();
        BuildGarage();
        BuildSettings();
        EnsureNewCircuits();
        foreach (string level in LevelCatalog.Scenes) UpdateRace(level, carMaterial);
        EditorBuildSettings.scenes = new[] { "MainMenu", "Garage", "Settings" }.Concat(LevelCatalog.Scenes)
            .Select(s => new EditorBuildSettingsScene("Assets/Scenes/" + s + ".unity", true)).ToArray();
        foreach (string name in new[] { "Masina", "Masina_Left", "Masina_Right" })
            AssetDatabase.DeleteAsset("Assets/Car/" + name + ".png");
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        Debug.Log("PASTEL PROJECT SETUP COMPLETE");
    }

    static void BuildCars()
    {
        string[] ids = { "racer", "sports", "rally" };
        string[] names = { "Open-wheel racer", "Compact sports car", "Rally hatchback" };
        string[] descriptions = { "Planted at speed. Strong grip for fast, precise lines.",
            "A playful all-rounder. Quick acceleration with a little extra slide.",
            "Short, nimble and sure-footed. Strong grip for the tightest bends." };
        float[] speed = { 14, 12, 10 }, acceleration = { 11, 14, 16 }, steering = { 2.8f, 3.2f, 3.6f }, drift = { .78f, .92f, .86f };
        for (int i = 0; i < ids.Length; i++)
        {
            string path = "Assets/Art/Cars/" + ids[i] + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.isReadable = true;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            string assetPath = "Assets/Resources/Cars/" + ids[i] + ".asset";
            var car = AssetDatabase.LoadAssetAtPath<CarDefinition>(assetPath);
            if (car == null)
            {
                car = ScriptableObject.CreateInstance<CarDefinition>();
                AssetDatabase.CreateAsset(car, assetPath);
            }
            car.id = ids[i]; car.displayName = names[i]; car.description = descriptions[i]; car.order = i;
            car.maxSpeed = speed[i]; car.acceleration = acceleration[i]; car.steering = steering[i]; car.driftFactor = drift[i]; car.oversteerFactor = i == 0 ? .2f : .5f;
            car.colliderSize = new Vector2(i == 0 ? .9f : .72f, 1.35f);
            car.leftTire = new Vector2(i == 0 ? -.39f : -.32f, -.5f);
            car.rightTire = new Vector2(-car.leftTire.x, car.leftTire.y);
            // Keep existing sprite subassets stable when rerunning the setup tool.
            if (car.straight == null)
            {
                var pixels = texture.GetPixels32();
                Sprite[] sprites = new Sprite[3];
                int cell = texture.width / 3;
                for (int frame = 0; frame < 3; frame++)
                {
                    int minX = (frame + 1) * cell, maxX = frame * cell, minY = texture.height, maxY = 0;
                    for (int y = 0; y < texture.height; y++)
                        for (int x = frame * cell; x < (frame + 1) * cell; x++)
                            if (pixels[y * texture.width + x].a > 32)
                            { minX = Math.Min(minX, x); maxX = Math.Max(maxX, x); minY = Math.Min(minY, y); maxY = Math.Max(maxY, y); }
                    var rect = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
                    sprites[frame] = Sprite.Create(texture, rect, new Vector2(.5f, .5f), rect.height / 1.5f, 0, SpriteMeshType.FullRect);
                    sprites[frame].name = ids[i] + new[] { "-straight", "-left", "-right" }[frame];
                    AssetDatabase.AddObjectToAsset(sprites[frame], car);
                }
                car.straight = sprites[0]; car.left = sprites[1]; car.right = sprites[2];
            }
            EditorUtility.SetDirty(car);
        }
        AssetDatabase.SaveAssets();
    }

    static void BuildSmoke(Material carMaterial)
    {
        const string texturePath = "Assets/Art/Smoke.png";
        if (!File.Exists(texturePath))
        {
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            {
                float r = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) / 31.5f;
                texture.SetPixel(x, y, new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - r * r), 2)));
            }
            texture.Apply(); File.WriteAllBytes(texturePath, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(texturePath);
        }
        const string finishPath = "Assets/Resources/FinishStripe.png";
        if (!File.Exists(finishPath))
        {
            var stripe = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
                stripe.SetPixel(x, y, (x / 16 + y / 4) % 2 == 0 ? theme.panel : theme.coral);
            stripe.Apply(); File.WriteAllBytes(finishPath, stripe.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(stripe);
            AssetDatabase.ImportAsset(finishPath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(finishPath);
            importer.textureType = TextureImporterType.Sprite; importer.spritePixelsPerUnit = 32;
            importer.mipmapEnabled = false; importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }
        var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/TireSmoke.mat");
        if (material == null)
        {
            material = new Material(carMaterial);
            material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            AssetDatabase.CreateAsset(material, "Assets/Resources/TireSmoke.mat");
        }
        var profile = AssetDatabase.LoadAssetAtPath<TireSmokeProfile>("Assets/Resources/TireSmoke.asset");
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<TireSmokeProfile>();
            AssetDatabase.CreateAsset(profile, "Assets/Resources/TireSmoke.asset");
        }
        profile.material = material; EditorUtility.SetDirty(profile);
    }

    static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        if (parent != null) rect.SetParent(parent, false);
        rect.sizeDelta = size; rect.anchoredPosition = position;
        return rect;
    }
    static Text Label(Transform parent, string value, float x, float y, float width, float height, int size, TextAnchor alignment = TextAnchor.MiddleLeft)
    {
        var rect = Rect(value.Replace("\n", " "), parent, new Vector2(width, height), new Vector2(x, y));
        var text = rect.gameObject.AddComponent<Text>();
        text.text = value; text.font = theme.font; text.fontSize = size; text.color = theme.ink;
        text.alignment = alignment; text.raycastTarget = false;
        return text;
    }
    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
    static GameObject SavePrefab(GameObject root, string name)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, Prefabs + name + ".prefab");
        UnityEngine.Object.DestroyImmediate(root);
        return prefab;
    }
    static void BuildSharedPrefabs()
    {
        var button = Rect("Pastel Button", null, new Vector2(320, 60), Vector2.zero);
        button.gameObject.AddComponent<Image>().color = theme.accent;
        var selectable = button.gameObject.AddComponent<Button>();
        var colors = selectable.colors;
        colors.normalColor = Color.white; colors.highlightedColor = new Color(1.07f, 1.07f, 1.02f);
        colors.selectedColor = new Color(.87f, .88f, .75f); colors.pressedColor = new Color(.78f, .8f, .7f);
        selectable.colors = colors;
        var text = Label(button, "Button", 0, 0, 320, 60, 23, TextAnchor.MiddleCenter);
        Stretch(text.rectTransform);
        buttonPrefab = SavePrefab(button.gameObject, "PastelButton");
        var panel = Rect("Pastel Panel", null, new Vector2(1100, 430), Vector2.zero);
        panel.gameObject.AddComponent<Image>().color = theme.panel;
        var shadow = panel.gameObject.AddComponent<Shadow>(); shadow.effectColor = new Color(.28f, .2f, .14f, .12f); shadow.effectDistance = new Vector2(0, -6);
        panelPrefab = SavePrefab(panel.gameObject, "PastelPanel");
        var screen = new GameObject("Pastel Screen", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        screen.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = screen.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = .5f;
        var backdrop = Rect("Tan Background", screen.transform, Vector2.zero, Vector2.zero);
        backdrop.gameObject.AddComponent<Image>().color = theme.background; Stretch(backdrop);
        var stage = Rect("Stage", screen.transform, new Vector2(1160, 660), Vector2.zero);
        screenPrefab = SavePrefab(screen, "PastelScreen");
    }
    static Button Button(Transform parent, string title, float x, float y, UnityEngine.Events.UnityAction action, float width = 320)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(buttonPrefab, parent);
        go.name = title;
        var rect = go.GetComponent<RectTransform>(); rect.sizeDelta = new Vector2(width, 58); rect.anchoredPosition = new Vector2(x, y);
        go.GetComponentInChildren<Text>().text = title;
        var button = go.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(button.onClick, action);
        return button;
    }
    static RectTransform Panel(Transform parent, string name, float x, float y, float width, float height)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(panelPrefab, parent); go.name = name;
        var rect = go.GetComponent<RectTransform>(); rect.sizeDelta = new Vector2(width, height); rect.anchoredPosition = new Vector2(x, y);
        return rect;
    }
    static Image Preview(Transform parent, Sprite sprite, float x, float y, float width, float height)
    {
        var rect = Rect("Car Preview", parent, new Vector2(width, height), new Vector2(x, y));
        var image = rect.gameObject.AddComponent<Image>(); image.sprite = sprite; image.preserveAspect = true; image.raycastTarget = false;
        return image;
    }
    static Transform NewScreen(string name)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camera.tag = "MainCamera"; camera.transform.position = new Vector3(0, 0, -10);
        camera.GetComponent<Camera>().orthographic = true; camera.GetComponent<Camera>().backgroundColor = theme.background;
        var events = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        var root = (GameObject)PrefabUtility.InstantiatePrefab(screenPrefab); root.name = name;
        return root.transform.Find("Stage");
    }
    static void Header(Transform stage, string title, string subtitle)
    {
        Label(stage, "SUNDAY CIRCUIT  /  TIME TRIAL CLUB", 0, 302, 1100, 25, 15);
        Label(stage, title, 0, 250, 1100, 64, 44).fontStyle = FontStyle.Bold;
        Label(stage, subtitle, 0, 198, 1100, 35, 19);
        Label(stage, "WASD / ARROWS  Drive      R  Restart      ESC  Back", 0, -328, 1100, 26, 15);
    }
    static void SaveScreen(string scene)
    {
        var canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        string path = Prefabs + scene + "View.prefab";
        var view = PrefabUtility.SaveAsPrefabAsset(canvas.gameObject, path);
        UnityEngine.Object.DestroyImmediate(canvas.gameObject);
        PrefabUtility.InstantiatePrefab(view);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Scenes/" + scene + ".unity");
    }
    static void BuildMainMenu()
    {
        var stage = NewScreen("Main Menu");
        var menu = stage.gameObject.AddComponent<Menu>();
        var home = Rect("Home", stage, new Vector2(1160, 660), Vector2.zero); menu.home = home.gameObject;
        Header(home, "Sunday Circuit", "A small car. An open road. One more lap.");
        var card = Panel(home, "Welcome", 0, -38, 1100, 420);
        Label(card, "A little drift.\nA better lap.", -235, 65, 540, 160, 49).fontStyle = FontStyle.Bold;
        Label(card, "Three cars. Five circuits. Find your rhythm.", -235, -45, 540, 45, 21);
        menu.selectedCar = Label(card, "YOUR CAR", -235, -105, 540, 45, 17);
        int i = 0;
        foreach (var car in CarSelection.All) Preview(card, car.straight, -445 + i++ * 115, -165, 85, 85);
        menu.homeFirst = Button(card, "Choose a circuit", 325, 120, menu.Play);
        Button(card, "Garage", 325, 40, menu.LoadGarage);
        Button(card, "Settings", 325, -40, menu.LoadSettings);
        Button(card, "Quit", 325, -120, menu.Quit);
        var levels = Rect("Circuit Selection", stage, new Vector2(1160, 660), Vector2.zero); menu.levels = levels.gameObject;
        Header(levels, "Choose your circuit", "The road is yours. Race against your own best time.");
        int pageCount = (LevelCatalog.Scenes.Length + 2) / 3;
        menu.circuitPages = new GameObject[pageCount];
        menu.pageFirst = new Button[pageCount];
        for (int page = 0; page < pageCount; page++)
        {
            var pageRoot = Rect("Circuits Page " + (page + 1), levels, new Vector2(1160, 660), Vector2.zero);
            menu.circuitPages[page] = pageRoot.gameObject;
            for (int column = 0; column < 3; column++)
            {
                i = page * 3 + column;
                if (i >= LevelCatalog.Scenes.Length) break;
                var p = Panel(pageRoot, LevelCatalog.Names[i], -370 + column * 370, -35, 350, 390);
                var item = p.gameObject.AddComponent<LevelCard>(); item.index = i;
                item.title = Label(p, (i + 1).ToString("00") + "  " + LevelCatalog.Names[i], 0, 152, 305, 42, 21);
                item.description = Label(p, LevelCatalog.Descriptions[i], 0, 98, 305, 58, 17);
                item.preview = Map(p, LevelCatalog.Layouts[i], new Vector2(260, 134), new Vector2(0, -4), 6);
                item.record = Label(p, "SET YOUR FIRST LAP", 0, -102, 305, 50, 15, TextAnchor.MiddleCenter);
                var button = Button(p, "Race", 0, -158, item.Race, 300);
                if (column == 0) menu.pageFirst[page] = button;
                if (i == 0) menu.levelsFirst = button;
            }
            pageRoot.gameObject.SetActive(page == 0);
        }
        Button(levels, "Back", -405, -275, menu.ShowMainMenu, 290);
        Button(levels, "< Previous", -50, -275, menu.PreviousCircuitPage, 240);
        menu.pageLabel = Label(levels, "1 / " + pageCount, 155, -275, 140, 40, 20, TextAnchor.MiddleCenter);
        Button(levels, "Next >", 405, -275, menu.NextCircuitPage, 290);
        levels.gameObject.SetActive(false);
        SaveScreen("MainMenu");
    }
    static void BuildGarage()
    {
        var stage = NewScreen("Garage"); Header(stage, "The garage", "Three different characters. Pick the one that feels like you.");
        var garage = stage.gameObject.AddComponent<Garage>();
        var panel = Panel(stage, "Garage Card", 0, -35, 1100, 390);
        var display = Panel(panel, "Preview Plinth", -300, 0, 430, 338); display.GetComponent<Image>().color = theme.blue;
        garage.preview = Preview(display, CarSelection.Current.straight, 0, 10, 220, 270);
        garage.counter = Label(display, "01 / 03", 0, -142, 330, 30, 17, TextAnchor.MiddleCenter);
        garage.carName = Label(panel, "Compact sports car", 228, 132, 495, 50, 32);
        garage.description = Label(panel, "Description", 228, 72, 495, 70, 19);
        garage.stats = Label(panel, "Handling", 228, -39, 495, 136, 22);
        garage.selection = Label(panel, "SELECTED FOR EVERY CIRCUIT", 228, -146, 495, 32, 16);
        garage.previousButton = Button(stage, "< Previous", -405, -275, garage.Previous, 290);
        Button(stage, "Next >", -95, -275, garage.Next, 290);
        garage.selectButton = Button(stage, "Select car", 215, -275, garage.Select, 290);
        Button(stage, "Back", 465, -275, garage.Back, 170);
        SaveScreen("Garage");
    }
    static Slider Slider(Transform parent, string label, float y)
    {
        Label(parent, label, -270, y, 400, 42, 22);
        var rect = Rect(label + " Slider", parent, new Vector2(400, 35), new Vector2(240, y));
        var slider = rect.gameObject.AddComponent<Slider>();
        var track = Rect("Track", rect, new Vector2(400, 9), Vector2.zero); track.gameObject.AddComponent<Image>().color = theme.background;
        var fill = Rect("Fill", track, Vector2.zero, Vector2.zero); fill.gameObject.AddComponent<Image>().color = theme.accent; Stretch(fill);
        var handle = Rect("Handle", rect, new Vector2(23, 0), Vector2.zero); var image = handle.gameObject.AddComponent<Image>(); image.color = theme.ink;
        slider.fillRect = fill; slider.handleRect = handle; slider.targetGraphic = image; slider.minValue = 0; slider.maxValue = 1; slider.value = .5f;
        return slider;
    }
    static Toggle Toggle(Transform parent, string label, float y)
    {
        Label(parent, label, -270, y, 400, 42, 22);
        var rect = Rect(label + " Toggle", parent, new Vector2(38, 38), new Vector2(420, y));
        var toggle = rect.gameObject.AddComponent<Toggle>();
        var box = rect.gameObject.AddComponent<Image>(); box.color = theme.background;
        var mark = Rect("Checkmark", rect, new Vector2(24, 24), Vector2.zero); var graphic = mark.gameObject.AddComponent<Image>(); graphic.color = theme.ink;
        toggle.targetGraphic = box; toggle.graphic = graphic; toggle.isOn = true;
        return toggle;
    }
    static void BuildSettings()
    {
        var stage = NewScreen("Settings"); Header(stage, "Make yourself comfortable", "Your preferences stay with you between races.");
        var settings = stage.gameObject.AddComponent<Settings>();
        var panel = Panel(stage, "Settings Card", 0, -35, 1100, 390);
        settings.MasterVolumeSlider = Slider(panel, "Master volume", 128);
        settings.MusicVolumeSlider = Slider(panel, "Music volume", 62);
        settings.SFXVolumeSlider = Slider(panel, "Effects volume", -4);
        settings.FullscreenToggle = Toggle(panel, "Fullscreen", -76);
        settings.CameraShakeToggle = Toggle(panel, "Camera shake", -142);
        settings.backButton = Button(stage, "Back", -405, -275, settings.BackToMenu, 290);
        SaveScreen("Settings");
    }

    static void EnsureNewCircuits()
    {
        // Use the common circuit scene as a template, preserving each target scene's GUID.
        foreach (string scene in LevelCatalog.Scenes.Where(s => s != "Level2" && s != "Level3"))
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Level2.unity");
            var circuit = UnityEngine.Object.FindAnyObjectByType<CircuitTrack>();
            circuit.layout = LevelCatalog.Layouts[Array.IndexOf(LevelCatalog.Scenes, scene)];
            EditorUtility.SetDirty(circuit);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Scenes/" + scene + ".unity");
        }
    }

    static void UpdateRace(string scene, Material carMaterial)
    {
        EditorSceneManager.OpenScene("Assets/Scenes/" + scene + ".unity");
        var car = UnityEngine.Object.FindAnyObjectByType<CarScript>();
        var logic = UnityEngine.Object.FindAnyObjectByType<Logic>();
        var oldCanvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        UnityEngine.Object.DestroyImmediate(oldCanvas.gameObject);
        var oldEvents = UnityEngine.Object.FindAnyObjectByType<EventSystem>();
        if (oldEvents != null) UnityEngine.Object.DestroyImmediate(oldEvents.gameObject);
        var events = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        UnityEngine.Object.FindAnyObjectByType<CircuitTrack>().layout = LevelCatalog.Layouts[Array.IndexOf(LevelCatalog.Scenes, scene)];
        Camera.main.backgroundColor = theme.background;
        Camera.main.orthographicSize = 9;
        car.transform.localScale = Vector3.one;
        car.smokeProfile = Resources.Load<TireSmokeProfile>("TireSmoke");
        car.definition = CarSelection.Current;
        car.GetComponent<SpriteRenderer>().sharedMaterial = carMaterial;
        car.GetComponent<SpriteRenderer>().sprite = car.definition.straight;
        car.GetComponent<SpriteRenderer>().sortingOrder = 10;
        car.GetComponent<BoxCollider2D>().size = car.definition.colliderSize;
        foreach (Transform child in car.transform.Cast<Transform>().ToArray()) UnityEngine.Object.DestroyImmediate(child.gameObject);
        car.tireSmokeParticles1 = NewSmoke(car.transform, "Left tire smoke");
        car.tireSmokeParticles2 = NewSmoke(car.transform, "Right tire smoke");
        // A common vehicle prefab keeps all levels on the same implementation.
        PrefabUtility.SaveAsPrefabAssetAndConnect(car.gameObject, Prefabs + "RaceCar.prefab", InteractionMode.AutomatedAction);
        EditorUtility.SetDirty(car);
        BuildHud(logic, scene);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }
    static ParticleSystem NewSmoke(Transform parent, string name)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var ps = go.AddComponent<ParticleSystem>(); Resources.Load<TireSmokeProfile>("TireSmoke").Apply(ps); return ps;
    }
    static CircuitMapGraphic Map(Transform parent, int layout, Vector2 size, Vector2 position, float width)
    {
        var rect = Rect("Circuit Layout", parent, size, position);
        var map = rect.gameObject.AddComponent<CircuitMapGraphic>();
        map.theme = theme; map.lineWidth = width; map.raycastTarget = false; map.SetLayout(layout);
        return map;
    }
    static void BuildHud(Logic logic, string scene)
    {
        var root = new GameObject("Race HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = .5f;
        var hud = root.AddComponent<RaceHud>();
        var bar = Panel(root.transform, "Timing Card", 0, 315, 920, 64);
        hud.circuit = Label(bar, "Circuit", -265, 13, 340, 27, 19);
        hud.carName = Label(bar, "Car", -265, -13, 340, 23, 14);
        var time = Label(bar, "0.00 s", 100, 0, 240, 50, 30, TextAnchor.MiddleCenter);
        var best = Label(bar, "BEST --", 345, 0, 190, 40, 18, TextAnchor.MiddleCenter);
        var status = Label(root.transform, "Cross the finish stripe to start", 0, -325, 900, 28, 18, TextAnchor.MiddleCenter);
        var mapPanel = Panel(root.transform, "Minimap", 0, 0, 160, 140);
        mapPanel.anchorMin = mapPanel.anchorMax = new Vector2(1, 0);
        mapPanel.pivot = new Vector2(1, 0);
        mapPanel.anchoredPosition = new Vector2(-18, 18);
        mapPanel.GetComponent<Image>().raycastTarget = false;
        hud.minimap = mapPanel.gameObject.AddComponent<RacingMinimap>();
        hud.minimap.route = Map(mapPanel, 0, new Vector2(140, 120), Vector2.zero, 4);
        var marker = Rect("Player Marker", hud.minimap.route.transform, new Vector2(8, 11), Vector2.zero);
        var markerGraphic = marker.gameObject.AddComponent<PlayerMapMarker>();
        markerGraphic.theme = theme; markerGraphic.raycastTarget = false;
        hud.minimap.marker = marker;
        var over = Rect("Game Over", root.transform, Vector2.zero, Vector2.zero); Stretch(over);
        over.gameObject.AddComponent<Image>().color = new Color(theme.background.r, theme.background.g, theme.background.b, .84f);
        var card = Panel(over, "Run Finished Card", 0, 0, 660, 355);
        Label(card, "A bump in the road.", 0, 100, 580, 70, 38, TextAnchor.MiddleCenter);
        Label(card, "Find your line and give it another go.", 0, 37, 580, 45, 21, TextAnchor.MiddleCenter);
        // Bind scene references after instantiating the shared HUD prefab below.
        var restart = Button(card, "Restart", 0, -40, hud.Restart, 380);
        Button(card, "Main menu", 0, -120, hud.MainMenu, 380);
        over.gameObject.SetActive(false);
        // Save a reusable HUD, then bind the scene manager to its timing widgets.
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, Prefabs + "RaceHud.prefab");
        UnityEngine.Object.DestroyImmediate(root);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.GetComponent<RaceHud>().minimap.route.SetLayout(LevelCatalog.Layouts[Array.IndexOf(LevelCatalog.Scenes, scene)]);
        var texts = instance.GetComponentsInChildren<Text>(true);
        logic.scoreText = texts.First(t => t.gameObject.name == "0.00 s");
        logic.highScoreText = texts.First(t => t.gameObject.name == "BEST --");
        logic.statusText = texts.First(t => t.gameObject.name == "Cross the finish stripe to start");
        logic.gameOverPanel = instance.transform.Find("Game Over").gameObject;
        var buttons = instance.GetComponentsInChildren<Button>(true);
        logic.restartButton = buttons.First(b => b.name == "Restart");
        EditorUtility.SetDirty(logic);
    }
}
