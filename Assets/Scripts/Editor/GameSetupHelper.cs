#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChromeDino.Core;
using ChromeDino.Player;
using ChromeDino.Interactables;
using ChromeDino.Environment;
using ChromeDino.UI;

namespace ChromeDino.EditorTools
{
    // Helper script to automatically setup scenes, prefabs, sprites, and UI
    public static class GameSetupHelper
    {
        [MenuItem("Tools/Setup Chrome Dino Game")]
        public static void SetupAll()
        {
            EnsureFolders();
            var animController = CreateAnimatorController();
            var prefabs = CreatePrefabs();
            SetupSampleScene(animController, prefabs);
            CreateMainMenuScene();
            UpdateBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Chrome Dino Runner", "All components, sprites, prefabs, animator, and scenes have been set up successfully!", "OK");
        }

        // Ensure required asset folders exist
        private static void EnsureFolders()
        {
            if (!Directory.Exists("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!Directory.Exists("Assets/Animations")) AssetDatabase.CreateFolder("Assets", "Animations");
            if (!Directory.Exists("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            if (!Directory.Exists("Assets/Sprites")) AssetDatabase.CreateFolder("Assets", "Sprites");
        }

        // Load and configure pixel sprite import settings
        private static Sprite LoadSprite(string path, float ppu = 32f)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool reimport = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    reimport = true;
                }
                if (importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    reimport = true;
                }
                if (Mathf.Abs(importer.spritePixelsPerUnit - ppu) > 0.1f)
                {
                    importer.spritePixelsPerUnit = ppu;
                    reimport = true;
                }
                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    reimport = true;
                }
                if (reimport)
                {
                    importer.SaveAndReimport();
                }
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // Create animator controller and transitions
        private static RuntimeAnimatorController CreateAnimatorController()
        {
            string path = "Assets/Animations/PlayerAnimator.controller";
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            controller.AddParameter("Run", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

            var rootStateMachine = controller.layers[0].stateMachine;

            var runState = rootStateMachine.AddState("Run");
            runState.motion = AssetDatabase.LoadAssetAtPath<Motion>("Assets/Animations/Dino_Run.anim");

            var jumpState = rootStateMachine.AddState("Jump");
            jumpState.motion = AssetDatabase.LoadAssetAtPath<Motion>("Assets/Animations/Dino_Jump.anim");

            var hitState = rootStateMachine.AddState("Hit");
            hitState.motion = AssetDatabase.LoadAssetAtPath<Motion>("Assets/Animations/Dino_Hit.anim");

            var deathState = rootStateMachine.AddState("Death");
            deathState.motion = AssetDatabase.LoadAssetAtPath<Motion>("Assets/Animations/Dino_Death.anim");

            rootStateMachine.defaultState = runState;

            // Run to Jump transitions
            var runToJump = runState.AddTransition(jumpState);
            runToJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");
            runToJump.hasExitTime = false;
            runToJump.duration = 0f;

            var runToJumpAir = runState.AddTransition(jumpState);
            runToJumpAir.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");
            runToJumpAir.hasExitTime = false;
            runToJumpAir.duration = 0f;

            // Jump to Run transition
            var jumpToRun = jumpState.AddTransition(runState);
            jumpToRun.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
            jumpToRun.hasExitTime = false;
            jumpToRun.duration = 0f;

            // AnyState to Hit transition
            var anyToHit = rootStateMachine.AddAnyStateTransition(hitState);
            anyToHit.AddCondition(AnimatorConditionMode.If, 0, "Hit");
            anyToHit.hasExitTime = false;
            anyToHit.duration = 0f;

            // Hit recovery transitions (grounded -> run, airborne -> jump)
            var hitToRun = hitState.AddTransition(runState);
            hitToRun.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
            hitToRun.hasExitTime = true;
            hitToRun.exitTime = 0.2f;
            hitToRun.duration = 0f;

            var hitToJump = hitState.AddTransition(jumpState);
            hitToJump.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");
            hitToJump.hasExitTime = true;
            hitToJump.exitTime = 0.2f;
            hitToJump.duration = 0f;

            // AnyState to Death transition
            var anyToDeath = rootStateMachine.AddAnyStateTransition(deathState);
            anyToDeath.AddCondition(AnimatorConditionMode.If, 0, "Death");
            anyToDeath.hasExitTime = false;
            anyToDeath.duration = 0f;

            return controller;
        }

        // Create obstacle and coin prefabs
        private static (GameObject ground, GameObject air, GameObject coin) CreatePrefabs()
        {
            // Ground obstacle prefab (cactus)
            GameObject goGround = new GameObject("GroundObstacle_Prefab");
            var srG = goGround.AddComponent<SpriteRenderer>();
            srG.sprite = LoadSprite("Assets/Sprites/Cactus.png", 48f);
            srG.sortingOrder = 8;
            var colG = goGround.AddComponent<BoxCollider2D>();
            colG.isTrigger = true;
            colG.size = new Vector2(0.8f, 1.8f);
            colG.offset = new Vector2(0f, 0.9f);
            goGround.AddComponent<GroundObstacle>();
            string pathG = "Assets/Prefabs/GroundObstacle.prefab";
            var prefabGround = PrefabUtility.SaveAsPrefabAsset(goGround, pathG);
            Object.DestroyImmediate(goGround);

            // Air obstacle prefab (pterodactyl)
            GameObject goAir = new GameObject("AirObstacle_Prefab");
            var srA = goAir.AddComponent<SpriteRenderer>();
            srA.sprite = LoadSprite("Assets/Sprites/Pterodactyl.png", 48f);
            srA.sortingOrder = 8;
            var colA = goAir.AddComponent<BoxCollider2D>();
            colA.isTrigger = true;
            colA.size = new Vector2(1.4f, 0.8f);
            colA.offset = new Vector2(0f, 0f);
            goAir.AddComponent<AirObstacle>();
            var animA = goAir.AddComponent<Animator>();
            animA.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/BirdAnimator.controller");
            string pathA = "Assets/Prefabs/AirObstacle.prefab";
            var prefabAir = PrefabUtility.SaveAsPrefabAsset(goAir, pathA);
            Object.DestroyImmediate(goAir);

            // Coin item prefab
            GameObject goCoin = new GameObject("CoinItem_Prefab");
            var srC = goCoin.AddComponent<SpriteRenderer>();
            srC.sprite = LoadSprite("Assets/Sprites/Coin.png", 32f);
            srC.sortingOrder = 9;
            var colC = goCoin.AddComponent<CircleCollider2D>();
            colC.isTrigger = true;
            colC.radius = 0.45f;
            var coinComp = goCoin.AddComponent<CoinItem>();
            var soCoin = new SerializedObject(coinComp);
            soCoin.FindProperty("scoreValue").intValue = 50;
            soCoin.ApplyModifiedProperties();

            string pathC = "Assets/Prefabs/CoinItem.prefab";
            var prefabCoin = PrefabUtility.SaveAsPrefabAsset(goCoin, pathC);
            Object.DestroyImmediate(goCoin);

            return (prefabGround, prefabAir, prefabCoin);
        }

        // Setup gameplay scene (SampleScene)
        private static void SetupSampleScene(RuntimeAnimatorController animController, (GameObject ground, GameObject air, GameObject coin) prefabs)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);

            // Main camera setup
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
            }
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.96f, 0.96f, 0.96f);

            // Remove default 3D objects that cause gizmos or horizon lines
            var dirLight = GameObject.Find("Directional Light");
            if (dirLight != null) Object.DestroyImmediate(dirLight);

            var globalVolume = GameObject.Find("Global Volume");
            if (globalVolume != null) Object.DestroyImmediate(globalVolume);

            var defaultCube = GameObject.Find("Cube");
            if (defaultCube != null) Object.DestroyImmediate(defaultCube);

            // Add and configure day/night cycle
            var cycle = cam.GetComponent<DayNightCycle>();
            if (cycle == null) cycle = cam.gameObject.AddComponent<DayNightCycle>();
            var soCycle = new SerializedObject(cycle);
            soCycle.FindProperty("scoreInterval").intValue = 500;
            soCycle.FindProperty("transitionDuration").floatValue = 2.0f;
            soCycle.ApplyModifiedProperties();

            // Ensure EventSystem
            EnsureEventSystem();

            // GameManager setup
            var gmGo = GameObject.Find("GameManager");
            if (gmGo == null) gmGo = new GameObject("GameManager");
            if (!gmGo.TryGetComponent<GameManager>(out _)) gmGo.AddComponent<GameManager>();

            // Player character setup
            var playerGo = GameObject.Find("Player");
            if (playerGo == null) playerGo = new GameObject("Player");
            playerGo.transform.position = new Vector3(-5f, -2.4f, 0f);

            var srPlayer = playerGo.GetComponent<SpriteRenderer>();
            if (srPlayer == null) srPlayer = playerGo.AddComponent<SpriteRenderer>();
            srPlayer.sprite = LoadSprite("Assets/Sprites/Dino.png", 48f);
            srPlayer.sortingOrder = 10;

            var rb = playerGo.GetComponent<Rigidbody2D>();
            if (rb == null) rb = playerGo.AddComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 3.5f;

            var col = playerGo.GetComponent<BoxCollider2D>();
            if (col == null) col = playerGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.9f, 1.8f);
            col.offset = new Vector2(0f, 0f);

            var anim = playerGo.GetComponent<Animator>();
            if (anim == null) anim = playerGo.AddComponent<Animator>();
            anim.runtimeAnimatorController = animController;

            var playerCtrl = playerGo.GetComponent<PlayerController>();
            if (playerCtrl == null) playerCtrl = playerGo.AddComponent<PlayerController>();

            // Ground check child
            var checkTrans = playerGo.transform.Find("GroundCheck");
            if (checkTrans == null)
            {
                var checkGo = new GameObject("GroundCheck");
                checkGo.transform.SetParent(playerGo.transform);
                checkGo.transform.localPosition = new Vector3(0f, 0f, 0f);
                checkTrans = checkGo.transform;
            }

            var soPlayer = new SerializedObject(playerCtrl);
            soPlayer.FindProperty("groundCheck").objectReferenceValue = checkTrans;
            soPlayer.FindProperty("groundLayer").intValue = 1 << LayerMask.NameToLayer("Ground");
            soPlayer.FindProperty("jumpForce").floatValue = 15f;
            soPlayer.ApplyModifiedProperties();

            // Ground with continuous tiling and scrolling
            var groundGo = GameObject.Find("Ground");
            if (groundGo == null) groundGo = new GameObject("Ground");
            groundGo.transform.position = new Vector3(0f, -3.2f, 0f);
            groundGo.layer = LayerMask.NameToLayer("Ground");

            var srGround = groundGo.GetComponent<SpriteRenderer>();
            if (srGround == null) srGround = groundGo.AddComponent<SpriteRenderer>();
            srGround.sprite = LoadSprite("Assets/Sprites/Ground.png", 32f);
            srGround.drawMode = SpriteDrawMode.Tiled;
            srGround.size = new Vector2(100f, 0.75f);
            srGround.sortingOrder = 5;

            var groundCol = groundGo.GetComponent<BoxCollider2D>();
            if (groundCol == null) groundCol = groundGo.AddComponent<BoxCollider2D>();
            groundCol.size = new Vector2(100f, 1f);
            groundCol.offset = new Vector2(0f, -0.5f);

            if (!groundGo.TryGetComponent<GroundScroller>(out _)) groundGo.AddComponent<GroundScroller>();

            // Parallax cloud background
            var bgGo = GameObject.Find("ParallaxBackground");
            if (bgGo == null) bgGo = new GameObject("ParallaxBackground");
            bgGo.transform.position = new Vector3(0f, 2.5f, 0f);

            var srBg = bgGo.GetComponent<SpriteRenderer>();
            if (srBg == null) srBg = bgGo.AddComponent<SpriteRenderer>();
            srBg.sprite = LoadSprite("Assets/Sprites/Cloud.png", 32f);
            srBg.sortingOrder = 1;

            if (!bgGo.TryGetComponent<ParallaxBackground>(out _)) bgGo.AddComponent<ParallaxBackground>();

            // Obstacle spawner
            var spawnerGo = GameObject.Find("ObstacleSpawner");
            if (spawnerGo == null) spawnerGo = new GameObject("ObstacleSpawner");
            spawnerGo.transform.position = new Vector3(14f, 0f, 0f);

            var spawner = spawnerGo.GetComponent<ObstacleSpawner>();
            if (spawner == null) spawner = spawnerGo.AddComponent<ObstacleSpawner>();

            var soSpawner = new SerializedObject(spawner);
            soSpawner.FindProperty("spawnX").floatValue = 14f;
            soSpawner.FindProperty("groundY").floatValue = -3.2f;
            var propOffset = soSpawner.FindProperty("cactusYOffset");
            if (propOffset != null) propOffset.floatValue = 0.6f;

            var aArray = soSpawner.FindProperty("airYOptions");
            aArray.arraySize = 3;
            aArray.GetArrayElementAtIndex(0).floatValue = -2.1f;
            aArray.GetArrayElementAtIndex(1).floatValue = -1.0f;
            aArray.GetArrayElementAtIndex(2).floatValue = 0.5f;

            soSpawner.FindProperty("minSpawnTime").floatValue = 1.6f;
            soSpawner.FindProperty("maxSpawnTime").floatValue = 3.0f;

            var gPrefabs = soSpawner.FindProperty("groundObstaclePrefabs");
            gPrefabs.arraySize = 1;
            gPrefabs.GetArrayElementAtIndex(0).objectReferenceValue = prefabs.ground;

            var aPrefabs = soSpawner.FindProperty("airObstaclePrefabs");
            aPrefabs.arraySize = 1;
            aPrefabs.GetArrayElementAtIndex(0).objectReferenceValue = prefabs.air;

            soSpawner.FindProperty("coinPrefab").objectReferenceValue = prefabs.coin;
            soSpawner.ApplyModifiedProperties();

            // Game Canvas
            SetupGameCanvas();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // Setup game canvas and UI cards
        private static void SetupGameCanvas()
        {
            var canvasGo = GameObject.Find("Canvas");
            if (canvasGo == null)
            {
                canvasGo = new GameObject("Canvas");
                var canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // Clear old canvas children to prevent duplicates
            for (int i = canvasGo.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(canvasGo.transform.GetChild(i).gameObject);
            }

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var gameUI = canvasGo.GetComponent<GameUIController>();
            if (gameUI == null) gameUI = canvasGo.AddComponent<GameUIController>();

            var healthUI = canvasGo.GetComponent<HealthBarUI>();
            if (healthUI == null) healthUI = canvasGo.AddComponent<HealthBarUI>();

            // Current score display (top-right)
            var scoreGo = CreateTMPText("ScoreText", canvasGo.transform, new Vector2(-60f, -40f), new Vector2(1f, 1f), "Score: 0", 28, TextAlignmentOptions.Right);
            // High score display
            var highScoreGo = CreateTMPText("HighScoreText", canvasGo.transform, new Vector2(-300f, -40f), new Vector2(1f, 1f), "HI: 0", 28, TextAlignmentOptions.Right);
            highScoreGo.GetComponent<TMP_Text>().color = new Color(0.5f, 0.5f, 0.5f);

            // Health bar slider (top-left)
            var sliderGo = CreateHealthSlider("HealthSlider", canvasGo.transform, new Vector2(60f, -40f), new Vector2(0f, 1f));

            // Pause menu panel
            var (pausePanel, resumeBtn, restartBtn, mainMenuBtn) = CreateModalCard("PauseMenuPanel", canvasGo.transform, "PAUSED", "RESUME", "RESTART", "MAIN MENU");
            pausePanel.SetActive(false);

            // Game over panel
            var (gameOverPanel, retryBtn, goMainMenuBtn, finalScoreTxt, finalHighScoreTxt) = CreateGameOverModalCard("GameOverPanel", canvasGo.transform);
            gameOverPanel.SetActive(false);

            var soUI = new SerializedObject(gameUI);
            soUI.FindProperty("currentScoreText").objectReferenceValue = scoreGo.GetComponent<TMP_Text>();
            soUI.FindProperty("highScoreText").objectReferenceValue = highScoreGo.GetComponent<TMP_Text>();
            soUI.FindProperty("pauseMenuPanel").objectReferenceValue = pausePanel;
            soUI.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
            soUI.FindProperty("finalScoreText").objectReferenceValue = finalScoreTxt;
            soUI.FindProperty("finalHighScoreText").objectReferenceValue = finalHighScoreTxt;
            soUI.FindProperty("resumeButton").objectReferenceValue = resumeBtn;
            soUI.FindProperty("restartButton").objectReferenceValue = restartBtn;
            soUI.FindProperty("mainMenuButton").objectReferenceValue = mainMenuBtn;
            soUI.FindProperty("retryButton").objectReferenceValue = retryBtn;
            soUI.FindProperty("gameOverMainMenuButton").objectReferenceValue = goMainMenuBtn;
            soUI.ApplyModifiedProperties();

            var soHealth = new SerializedObject(healthUI);
            soHealth.FindProperty("healthSlider").objectReferenceValue = sliderGo.GetComponent<Slider>();
            soHealth.ApplyModifiedProperties();
        }

        // Setup main menu scene
        private static void CreateMainMenuScene()
        {
            string scenePath = "Assets/Scenes/MainMenu.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.96f, 0.96f, 0.96f);

            EnsureEventSystem();

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            var menuCtrl = canvasGo.AddComponent<MainMenuController>();

            // Dino icon above title
            var dinoIconGo = new GameObject("DinoIcon");
            dinoIconGo.transform.SetParent(canvasGo.transform, false);
            var dinoImg = dinoIconGo.AddComponent<Image>();
            dinoImg.sprite = LoadSprite("Assets/Sprites/Dino.png", 48f);
            var dRt = dinoIconGo.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0.5f, 0.5f);
            dRt.anchorMax = new Vector2(0.5f, 0.5f);
            dRt.pivot = new Vector2(0.5f, 0.5f);
            dRt.anchoredPosition = new Vector2(0f, 210f);
            dRt.sizeDelta = new Vector2(96f, 104f);

            // Game title
            CreateTMPText("TitleText", canvasGo.transform, new Vector2(0f, 110f), new Vector2(0.5f, 0.5f), "CHROME DINO RUNNER", 44, TextAlignmentOptions.Center, new Vector2(800f, 65f));

            // High score text
            var hiText = CreateTMPText("HighScoreText", canvasGo.transform, new Vector2(0f, 40f), new Vector2(0.5f, 0.5f), "High Score: 0", 24, TextAlignmentOptions.Center, new Vector2(400f, 40f));
            hiText.GetComponent<TMP_Text>().color = new Color(0.45f, 0.45f, 0.48f);

            // Menu buttons
            var playBtnGo = CreateButton("PlayButton", canvasGo.transform, new Vector2(0f, -50f), "PLAY GAME", new Color(0.22f, 0.58f, 0.34f));
            var quitBtnGo = CreateButton("QuitButton", canvasGo.transform, new Vector2(0f, -125f), "QUIT", new Color(0.3f, 0.32f, 0.36f));

            var soMenu = new SerializedObject(menuCtrl);
            soMenu.FindProperty("highScoreText").objectReferenceValue = hiText.GetComponent<TMP_Text>();
            soMenu.FindProperty("gameSceneName").stringValue = "SampleScene";
            soMenu.FindProperty("playButton").objectReferenceValue = playBtnGo.GetComponent<Button>();
            soMenu.FindProperty("quitButton").objectReferenceValue = quitBtnGo.GetComponent<Button>();
            soMenu.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene, scenePath);
        }

        // Configure scenes in Build Settings
        private static void UpdateBuildSettings()
        {
            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true)
            };
            EditorBuildSettings.scenes = scenes;
        }

        #region UI and EventSystem Helpers

        private static void EnsureEventSystem()
        {
            var es = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
        }

        private static GameObject CreateTMPText(string name, Transform parent, Vector2 anchoredPos, Vector2 anchor, string defaultText, float fontSize = 24, TextAlignmentOptions align = TextAlignmentOptions.Center, Vector2? sizeDelta = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta ?? new Vector2(280f, 45f);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(0.2f, 0.2f, 0.22f);
            tmp.alignment = align;

            return go;
        }

        private static GameObject CreateHealthSlider(string name, Transform parent, Vector2 anchoredPos, Vector2 anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(160f, 22f);

            var slider = go.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 3f;
            slider.wholeNumbers = true;
            slider.value = 3f;

            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(go.transform, false);
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.color = new Color(0.85f, 0.85f, 0.85f);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero;
            faRt.anchorMax = Vector2.one;
            faRt.sizeDelta = Vector2.zero;

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(fillArea.transform, false);
            var fillImage = fillGo.AddComponent<Image>();
            fillImage.color = new Color(0.92f, 0.25f, 0.25f);
            var fRt = fillGo.GetComponent<RectTransform>();
            fRt.anchorMin = Vector2.zero;
            fRt.anchorMax = Vector2.one;
            fRt.sizeDelta = Vector2.zero;

            slider.fillRect = fRt;
            slider.targetGraphic = bgImage;

            return go;
        }

        private static GameObject CreateButton(string name, Transform parent, Vector2 anchoredPos, string label, Color? bgColor = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(220f, 48f);

            var img = go.AddComponent<Image>();
            img.color = bgColor ?? new Color(0.2f, 0.2f, 0.24f);

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.35f, 0.35f, 0.4f);
            colors.pressedColor = new Color(0.12f, 0.12f, 0.15f);
            btn.colors = colors;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var tRt = textGo.AddComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.sizeDelta = Vector2.zero;

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 20;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return go;
        }

        // Create dialog modal card
        private static (GameObject panel, Button btn1, Button btn2, Button btn3) CreateModalCard(string name, Transform parent, string title, string btn1Text, string btn2Text, string btn3Text)
        {
            // Dim background overlay
            var modalRoot = new GameObject(name);
            modalRoot.transform.SetParent(parent, false);

            var mRt = modalRoot.AddComponent<RectTransform>();
            mRt.anchorMin = Vector2.zero;
            mRt.anchorMax = Vector2.one;
            mRt.sizeDelta = Vector2.zero;

            var bgDim = modalRoot.AddComponent<Image>();
            bgDim.color = new Color(0f, 0f, 0f, 0.6f);

            // Center dialog card
            var cardGo = new GameObject("Card");
            cardGo.transform.SetParent(modalRoot.transform, false);
            var cRt = cardGo.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.pivot = new Vector2(0.5f, 0.5f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(420f, 320f);

            var cardImg = cardGo.AddComponent<Image>();
            cardImg.color = new Color(0.13f, 0.14f, 0.16f, 0.98f);

            // Card title
            CreateTMPText("Title", cardGo.transform, new Vector2(0f, 95f), new Vector2(0.5f, 0.5f), title, 34);

            Button b1 = null, b2 = null, b3 = null;
            if (!string.IsNullOrEmpty(btn1Text)) b1 = CreateButton("Btn1", cardGo.transform, new Vector2(0f, 25f), btn1Text, new Color(0.24f, 0.6f, 0.35f)).GetComponent<Button>();
            if (!string.IsNullOrEmpty(btn2Text)) b2 = CreateButton("Btn2", cardGo.transform, new Vector2(0f, -40f), btn2Text, new Color(0.28f, 0.3f, 0.35f)).GetComponent<Button>();
            if (!string.IsNullOrEmpty(btn3Text)) b3 = CreateButton("Btn3", cardGo.transform, new Vector2(0f, -105f), btn3Text, new Color(0.45f, 0.25f, 0.25f)).GetComponent<Button>();

            return (modalRoot, b1, b2, b3);
        }

        private static (GameObject panel, Button retryBtn, Button menuBtn, TMP_Text scoreText, TMP_Text hiText) CreateGameOverModalCard(string name, Transform parent)
        {
            var modalRoot = new GameObject(name);
            modalRoot.transform.SetParent(parent, false);

            var mRt = modalRoot.AddComponent<RectTransform>();
            mRt.anchorMin = Vector2.zero;
            mRt.anchorMax = Vector2.one;
            mRt.sizeDelta = Vector2.zero;

            var bgDim = modalRoot.AddComponent<Image>();
            bgDim.color = new Color(0f, 0f, 0f, 0.65f);

            var cardGo = new GameObject("Card");
            cardGo.transform.SetParent(modalRoot.transform, false);
            var cRt = cardGo.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.pivot = new Vector2(0.5f, 0.5f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(440f, 370f);

            var cardImg = cardGo.AddComponent<Image>();
            cardImg.color = new Color(0.13f, 0.14f, 0.16f, 0.98f);

            CreateTMPText("Title", cardGo.transform, new Vector2(0f, 125f), new Vector2(0.5f, 0.5f), "GAME OVER", 32);
            var scoreObj = CreateTMPText("FinalScore", cardGo.transform, new Vector2(0f, 75f), new Vector2(0.5f, 0.5f), "Final Score: 0", 22);
            scoreObj.GetComponent<TMP_Text>().color = new Color(0.85f, 0.85f, 0.85f);
            var hiObj = CreateTMPText("FinalHighScore", cardGo.transform, new Vector2(0f, 40f), new Vector2(0.5f, 0.5f), "Best Score: 0", 20);
            hiObj.GetComponent<TMP_Text>().color = new Color(0.6f, 0.6f, 0.6f);

            var retryBtn = CreateButton("RetryBtn", cardGo.transform, new Vector2(0f, -25f), "RETRY", new Color(0.24f, 0.6f, 0.35f)).GetComponent<Button>();
            var menuBtn = CreateButton("MainMenuBtn", cardGo.transform, new Vector2(0f, -95f), "MAIN MENU", new Color(0.28f, 0.3f, 0.35f)).GetComponent<Button>();

            return (modalRoot, retryBtn, menuBtn, scoreObj.GetComponent<TMP_Text>(), hiObj.GetComponent<TMP_Text>());
        }

        #endregion
    }
}
#endif
