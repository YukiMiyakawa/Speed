#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using Speed.Domain;
using Speed.View;
using Speed.Controllers;
using Speed.Config;

namespace Speed.Editor
{
    /// <summary>
    /// Scene setup utility. Run via Speed menu or Context Menu.
    /// </summary>
    public class SceneSetup : MonoBehaviour
    {
        // ---- Static menu items (accessible via unity_execute_menu_item) ----
        [MenuItem("Speed/Setup All Scenes")]
        public static void MenuSetupAll()
        {
            CreateOrRefreshScriptableObjects();
            SetupTitleScene();
            SetupBattleScene();
            SetupBuildSettings();
            Debug.Log("[SceneSetup] All scenes created successfully.");
        }

        [MenuItem("Speed/Setup TitleScene")]
        public static void MenuSetupTitle() => SetupTitleScene();

        [MenuItem("Speed/Setup BattleScene")]
        public static void MenuSetupBattle() => SetupBattleScene();

        [MenuItem("Speed/Create ScriptableObjects")]
        public static void MenuCreateSOs() => CreateOrRefreshScriptableObjects();

        // ---- Context menu aliases ----
        [ContextMenu("Setup All")]
        public void SetupAll() => MenuSetupAll();

        [ContextMenu("Setup TitleScene Only")]
        public void SetupTitleSceneOnly() => SetupTitleScene();

        [ContextMenu("Setup BattleScene Only")]
        public void SetupBattleSceneOnly() => SetupBattleScene();

        [ContextMenu("Create ScriptableObjects")]
        public void CreateSOs() => CreateOrRefreshScriptableObjects();

        // =====================================================================
        //  ScriptableObjects
        // =====================================================================
        private static void CreateOrRefreshScriptableObjects()
        {
            EnsureFolder("Assets/Resources");

            // CpuDifficultyConfig
            const string diffPath = "Assets/Resources/CpuDifficultyConfig.asset";
            if (!System.IO.File.Exists(System.IO.Path.GetFullPath(diffPath)))
            {
                var cfg = ScriptableObject.CreateInstance<CpuDifficultyConfig>();
                AssetDatabase.CreateAsset(cfg, diffPath);
                Debug.Log($"[SceneSetup] Created {diffPath}");
            }

            // CardPrefabRegistry
            const string regPath = "Assets/Resources/CardPrefabRegistry.asset";
            if (!System.IO.File.Exists(System.IO.Path.GetFullPath(regPath)))
            {
                var reg = ScriptableObject.CreateInstance<CardPrefabRegistry>();
                AssetDatabase.CreateAsset(reg, regPath);
                Debug.Log($"[SceneSetup] Created {regPath}");
            }

            // Auto-fill CardPrefabRegistry
            AutoFillCardRegistry(regPath);
            AssetDatabase.SaveAssets();
        }

        private static void AutoFillCardRegistry(string regPath)
        {
            var reg = AssetDatabase.LoadAssetAtPath<CardPrefabRegistry>(regPath);
            if (reg == null) return;
            const string deck = "Assets/ExternalResource/Asset_PlayingCards/Prefabs/Deck01";
            var entries = new List<CardPrefabEntry>();
            foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
            {
                for (int rank = 1; rank <= 13; rank++)
                {
                    string rs = rank == 1 ? "A" : rank == 11 ? "J" : rank == 12 ? "Q" : rank == 13 ? "K" : rank.ToString();
                    string path = $"{deck}/Deck01_{suit}_{rs}.prefab";
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    entries.Add(new CardPrefabEntry { Suit = suit, Rank = rank, Prefab = prefab });
                }
            }
            reg.Entries = entries.ToArray();
            EditorUtility.SetDirty(reg);
        }

        // =====================================================================
        //  Build Settings
        // =====================================================================
        private static void SetupBuildSettings()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/TitleScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/BattleScene.unity", true),
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log("[SceneSetup] Build settings updated.");
        }

        // =====================================================================
        //  TitleScene
        // =====================================================================
        private static void SetupTitleScene()
        {
            EnsureFolder("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Camera
            var cam = GameObject.FindObjectOfType<Camera>();
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 7f;
                cam.backgroundColor = new Color(0.13f, 0.13f, 0.2f);
            }

            // EventSystem
            CreateEventSystem();

            // ----- Canvas -----
            var canvasGO = CreateCanvas("Canvas");
            var safeAreaGO = CreateRectPanel("SafeArea", canvasGO.transform, Vector2.zero, Vector2.one);
            safeAreaGO.AddComponent<SafeArea>();

            // Background
            var bg = CreateRectPanel("Background", safeAreaGO.transform, Vector2.zero, Vector2.one);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.18f);

            // Title text
            var titleTextGO = CreateTMPText("TitleText", safeAreaGO.transform,
                "SPEED", 72, TextAlignmentOptions.Center);
            SetRectTransform(titleTextGO, new Vector2(0, 250), new Vector2(600, 120));

            // CPU Battle button
            var cpuBtn = CreateButton("CpuBattleButton", safeAreaGO.transform, "CPU BATTLE", 36);
            SetRectTransform(cpuBtn, new Vector2(0, 80), new Vector2(380, 80));

            // Online button (grayed out)
            var onlineBtn = CreateButton("OnlineButton", safeAreaGO.transform, "ONLINE (Coming Soon)", 28);
            SetRectTransform(onlineBtn, new Vector2(0, -30), new Vector2(380, 70));
            var onlineBtnComp = onlineBtn.GetComponent<Button>();
            onlineBtnComp.interactable = false;
            var onlineColors = onlineBtnComp.colors;
            onlineColors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            onlineBtnComp.colors = onlineColors;

            // Settings button
            var settingsBtn = CreateButton("SettingsButton", safeAreaGO.transform, "SETTINGS", 32);
            SetRectTransform(settingsBtn, new Vector2(0, -130), new Vector2(380, 70));

            // ----- Settings Panel (initially inactive) -----
            var settingsPanelGO = CreateRectPanel("SettingsPanel", safeAreaGO.transform, Vector2.zero, Vector2.one);
            settingsPanelGO.SetActive(false);
            var settingsBg = settingsPanelGO.AddComponent<Image>();
            settingsBg.color = new Color(0, 0, 0, 0.85f);

            // Settings inner panel
            var settingsInner = CreateRectPanel("SettingsInner", settingsPanelGO.transform, Vector2.zero, Vector2.zero);
            SetRectTransform(settingsInner, Vector2.zero, new Vector2(500, 500));
            var innerImg = settingsInner.AddComponent<Image>();
            innerImg.color = new Color(0.18f, 0.18f, 0.28f);

            // Settings title
            var stTitle = CreateTMPText("SettingsTitle", settingsInner.transform, "SETTINGS", 42, TextAlignmentOptions.Center);
            SetRectTransform(stTitle, new Vector2(0, 180), new Vector2(400, 70));

            // Difficulty label + slider
            var diffLabel = CreateTMPText("DifficultyLabel", settingsInner.transform, "Level 3", 30, TextAlignmentOptions.Center);
            SetRectTransform(diffLabel, new Vector2(0, 90), new Vector2(400, 50));

            var sliderGO = CreateSlider("DifficultySlider", settingsInner.transform);
            SetRectTransform(sliderGO, new Vector2(0, 30), new Vector2(360, 40));
            var slider = sliderGO.GetComponent<Slider>();
            slider.minValue = 1; slider.maxValue = 5; slider.wholeNumbers = true; slider.value = 3;

            // Sound toggle
            var soundToggleGO = CreateToggle("SoundToggle", settingsInner.transform, "Sound ON");
            SetRectTransform(soundToggleGO, new Vector2(0, -50), new Vector2(360, 50));

            // Vibrate toggle
            var vibrateToggleGO = CreateToggle("VibrateToggle", settingsInner.transform, "Vibrate ON");
            SetRectTransform(vibrateToggleGO, new Vector2(0, -120), new Vector2(360, 50));

            // Close button
            var closeBtn = CreateButton("CloseButton", settingsInner.transform, "CLOSE", 28);
            SetRectTransform(closeBtn, new Vector2(0, -200), new Vector2(240, 60));

            // ----- TitleManager -----
            var mgr = new GameObject("TitleManager");
            var titleView = mgr.AddComponent<TitleView>();
            var settingsView = settingsPanelGO.AddComponent<SettingsView>();

            // Wire TitleView
            SetSerializedField(titleView, "_cpuBattleButton",  cpuBtn.GetComponent<Button>());
            SetSerializedField(titleView, "_onlineButton",     onlineBtn.GetComponent<Button>());
            SetSerializedField(titleView, "_settingsButton",   settingsBtn.GetComponent<Button>());
            SetSerializedField(titleView, "_settingsView",     settingsView);

            // Wire SettingsView
            SetSerializedField(settingsView, "_difficultySlider", slider);
            SetSerializedField(settingsView, "_difficultyLabel",  diffLabel.GetComponent<TextMeshProUGUI>());
            SetSerializedField(settingsView, "_soundToggle",      soundToggleGO.GetComponent<Toggle>());
            SetSerializedField(settingsView, "_vibrateToggle",    vibrateToggleGO.GetComponent<Toggle>());
            SetSerializedField(settingsView, "_closeButton",      closeBtn.GetComponent<Button>());

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/TitleScene.unity");
            Debug.Log("[SceneSetup] TitleScene saved.");
        }

        // =====================================================================
        //  BattleScene
        // =====================================================================
        private static void SetupBattleScene()
        {
            EnsureFolder("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Camera
            var cam = GameObject.FindObjectOfType<Camera>();
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 7f;
                cam.backgroundColor = new Color(0.1f, 0.14f, 0.1f);
                cam.transform.position = new Vector3(0, 0, -10);
            }

            CreateEventSystem();

            // ----- BattleLayout -----
            var layoutGO = new GameObject("BattleLayout");
            var layout   = layoutGO.AddComponent<BattleLayout>();

            var leftAnchor   = CreateAnchor("LeftPileAnchor",   layoutGO.transform, -1.3f, 0f);
            var rightAnchor  = CreateAnchor("RightPileAnchor",  layoutGO.transform,  1.3f, 0f);
            var playerDeck   = CreateAnchor("PlayerDeckAnchor", layoutGO.transform,  3.0f, -4.5f);
            var playerHand   = CreateAnchor("PlayerHandRoot",   layoutGO.transform,  0f,   -4.5f);
            var cpuDeck      = CreateAnchor("CpuDeckAnchor",    layoutGO.transform, -3.0f,  4.5f);
            var cpuHand      = CreateAnchor("CpuHandRoot",      layoutGO.transform,  0f,    4.5f);

            SetSerializedField(layout, "LeftPileAnchor",   leftAnchor);
            SetSerializedField(layout, "RightPileAnchor",  rightAnchor);
            SetSerializedField(layout, "PlayerDeckAnchor", playerDeck);
            SetSerializedField(layout, "PlayerHandRoot",   playerHand);
            SetSerializedField(layout, "CpuDeckAnchor",    cpuDeck);
            SetSerializedField(layout, "CpuHandRoot",      cpuHand);

            // Deck Visuals (simple colored quads as placeholders)
            var playerDeckVis = CreateWorldQuad("PlayerDeckVisual", new Vector3(3.0f, -4.5f, 0), new Color(0.2f, 0.4f, 0.2f));
            var cpuDeckVis    = CreateWorldQuad("CpuDeckVisual",    new Vector3(-3.0f, 4.5f, 0), new Color(0.4f, 0.2f, 0.2f));

            // ----- BattleManager -----
            var mgrGO = new GameObject("BattleManager");
            var gc    = mgrGO.AddComponent<GameController>();
            var bic   = mgrGO.AddComponent<BattleInputController>();
            var cpu   = mgrGO.AddComponent<CpuController>();
            var bv    = mgrGO.AddComponent<BattleView>();

            // Load config from Resources
            var diffCfg  = AssetDatabase.LoadAssetAtPath<CpuDifficultyConfig>("Assets/Resources/CpuDifficultyConfig.asset");
            var cardReg  = AssetDatabase.LoadAssetAtPath<CardPrefabRegistry>("Assets/Resources/CardPrefabRegistry.asset");

            SetSerializedField(gc, "DifficultyConfig", diffCfg);

            SetSerializedField(bic, "BattleView",  bv);
            SetSerializedField(bic, "GameCamera",  cam);

            SetSerializedField(cpu, "BattleView",  bv);

            SetSerializedField(bv,  "GameController",   gc);
            SetSerializedField(bv,  "Layout",           layout);
            SetSerializedField(bv,  "CardRegistry",     cardReg);
            SetSerializedField(bv,  "PlayerDeckVisual", playerDeckVis);
            SetSerializedField(bv,  "CpuDeckVisual",    cpuDeckVis);

            // ----- Canvas (UI) -----
            var canvasGO  = CreateCanvas("Canvas");
            var safeAreaGO = CreateRectPanel("SafeArea", canvasGO.transform, Vector2.zero, Vector2.one);
            safeAreaGO.AddComponent<SafeArea>();

            // Player deck count label
            var playerCountGO = CreateTMPText("PlayerDeckCount", safeAreaGO.transform,
                "20", 28, TextAlignmentOptions.Center);
            SetRectAnchorPreset(playerCountGO, new Vector2(1, 0), new Vector2(1, 0));
            SetRectTransform(playerCountGO, new Vector2(-80, 60), new Vector2(80, 40));

            // CPU deck count label
            var cpuCountGO = CreateTMPText("CpuDeckCount", safeAreaGO.transform,
                "20", 28, TextAlignmentOptions.Center);
            SetRectAnchorPreset(cpuCountGO, new Vector2(0, 1), new Vector2(0, 1));
            SetRectTransform(cpuCountGO, new Vector2(80, -60), new Vector2(80, 40));

            // Result Dialog
            var resultDialogGO = CreateResultDialog(safeAreaGO.transform);
            var resultDialog   = resultDialogGO.AddComponent<ResultDialogView>();

            var rdResultText  = resultDialogGO.transform.Find("Panel/ResultText")?.GetComponent<TextMeshProUGUI>();
            var rdRematchBtn  = resultDialogGO.transform.Find("Panel/RematchButton")?.GetComponent<Button>();
            var rdTitleBtn    = resultDialogGO.transform.Find("Panel/TitleButton")?.GetComponent<Button>();
            SetSerializedField(resultDialog, "_resultText",    rdResultText);
            SetSerializedField(resultDialog, "_rematchButton", rdRematchBtn);
            SetSerializedField(resultDialog, "_titleButton",   rdTitleBtn);

            // Wire BattleView UI refs
            SetSerializedField(bv, "PlayerDeckCountText", playerCountGO.GetComponent<TextMeshProUGUI>());
            SetSerializedField(bv, "CpuDeckCountText",    cpuCountGO.GetComponent<TextMeshProUGUI>());
            SetSerializedField(bv, "ResultDialog",        resultDialog);

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/BattleScene.unity");
            Debug.Log("[SceneSetup] BattleScene saved.");
        }

        // =====================================================================
        //  Helpers – UI
        // =====================================================================
        private static GameObject CreateCanvas(string name)
        {
            var go     = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static void CreateEventSystem()
        {
            if (GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null) return;
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        private static GameObject CreateRectPanel(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        private static GameObject CreateTMPText(string name, Transform parent, string text,
            float size, TextAlignmentOptions align)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = size;
            tmp.alignment = align;
            tmp.color     = Color.white;
            return go;
        }

        private static GameObject CreateButton(string name, Transform parent, string label, float fontSize)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = new Color(0.25f, 0.45f, 0.85f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var textRt = textGO.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;
            return go;
        }

        private static GameObject CreateSlider(string name, Transform parent)
        {
            // Minimal slider using DefaultControls
            var res = new DefaultControls.Resources();
            res.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            res.standard   = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var sliderGO = DefaultControls.CreateSlider(res);
            sliderGO.name = name;
            sliderGO.transform.SetParent(parent, false);
            return sliderGO;
        }

        private static GameObject CreateToggle(string name, Transform parent, string label)
        {
            var res = new DefaultControls.Resources();
            res.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            res.standard   = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var toggleGO = DefaultControls.CreateToggle(res);
            toggleGO.name = name;
            toggleGO.transform.SetParent(parent, false);

            // Replace built-in Text with TMP
            var legacyText = toggleGO.GetComponentInChildren<Text>();
            if (legacyText != null)
            {
                var textGO = legacyText.gameObject;
                Object.DestroyImmediate(legacyText);
                var tmp = textGO.AddComponent<TextMeshProUGUI>();
                tmp.text      = label;
                tmp.fontSize  = 26;
                tmp.color     = Color.white;
                tmp.alignment = TextAlignmentOptions.Left;
            }
            return toggleGO;
        }

        private static GameObject CreateResultDialog(Transform parent)
        {
            var dialogGO = CreateRectPanel("ResultDialog", parent, Vector2.zero, Vector2.one);
            var bg = dialogGO.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);

            var panelGO = CreateRectPanel("Panel", dialogGO.transform, Vector2.zero, Vector2.zero);
            SetRectTransform(panelGO, Vector2.zero, new Vector2(500, 420));
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.15f, 0.15f, 0.25f);

            var resultText = CreateTMPText("ResultText", panelGO.transform, "YOU WIN!", 64, TextAlignmentOptions.Center);
            SetRectTransform(resultText, new Vector2(0, 120), new Vector2(440, 100));

            var rematchBtn = CreateButton("RematchButton", panelGO.transform, "REMATCH", 34);
            SetRectTransform(rematchBtn, new Vector2(0, 0), new Vector2(360, 80));

            var titleBtn = CreateButton("TitleButton", panelGO.transform, "TITLE", 30);
            SetRectTransform(titleBtn, new Vector2(0, -100), new Vector2(360, 70));

            dialogGO.SetActive(false);
            return dialogGO;
        }

        // =====================================================================
        //  Helpers – World
        // =====================================================================
        private static Transform CreateAnchor(string name, Transform parent, float x, float y)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = new Vector3(x, y, 0);
            return go.transform;
        }

        private static GameObject CreateWorldQuad(string name, Vector3 pos, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.75f, 1.05f, 1f);
            Object.DestroyImmediate(go.GetComponent<MeshCollider>());
            var mr = go.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = color;
            mr.material = mat;
            return go;
        }

        // =====================================================================
        //  Helpers – RectTransform
        // =====================================================================
        private static void SetRectTransform(GameObject go, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = sizeDelta;
        }

        private static void SetRectAnchorPreset(GameObject go, Vector2 anchorMin, Vector2 anchorMax)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) return;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot     = anchorMin;
        }

        // =====================================================================
        //  Helpers – Serialized Fields
        // =====================================================================
        private static void SetSerializedField(Object target, string fieldName, Object value)
        {
            var so   = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning($"[SceneSetup] Field not found: {target.GetType().Name}.{fieldName}");
            }
        }

        // =====================================================================
        //  Helpers – Misc
        // =====================================================================
        private static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
