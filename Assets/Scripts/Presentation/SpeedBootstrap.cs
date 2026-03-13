using Speed.Controllers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Speed.Presentation
{
    public static class SpeedBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreatePrototype()
        {
            if (Object.FindObjectOfType<GameController>() != null)
            {
                return;
            }

            var root = new GameObject("SpeedPrototype");
            Object.DontDestroyOnLoad(root);

            var canvas = CreateCanvas(root.transform);
            EnsureEventSystem();

            var animationController = root.AddComponent<BattleAnimationController>();
            var inputController = root.AddComponent<BattleInputController>();
            var cpuController = root.AddComponent<CpuController>();
            var gameController = root.AddComponent<GameController>();
            var battleUiController = root.AddComponent<BattleUIController>();

            var references = CreateBattleLayout(canvas.transform);
            ConfigureBattleUiController(battleUiController, references);
            ConfigureGameController(gameController, battleUiController, animationController, inputController, cpuController);
        }

        private static Canvas CreateCanvas(Transform parent)
        {
            var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;
            return canvas;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static BattleReferences CreateBattleLayout(Transform parent)
        {
            var font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            var panel = CreatePanel("Root", parent, new Color(0.14f, 0.45f, 0.22f, 1f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var cpuInfo = CreateText("CpuInfo", panel.transform, font, 34, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(700f, 120f));
            var cpuDeck = CreateText("CpuDeck", panel.transform, font, 30, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(60f, -140f), new Vector2(300f, 70f));
            var playerDeck = CreateText("PlayerDeck", panel.transform, font, 30, TextAnchor.LowerLeft, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(60f, 220f), new Vector2(300f, 70f));
            var waiting = CreateText("Waiting", panel.transform, font, 42, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(520f, 80f));
            var invalid = CreateText("Invalid", panel.transform, font, 36, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.35f), Vector2.zero, new Vector2(420f, 80f));

            var leftPile = CreatePile("LeftPile", panel.transform, font, new Vector2(0.36f, 0.55f));
            var rightPile = CreatePile("RightPile", panel.transform, font, new Vector2(0.64f, 0.55f));

            var handRootObject = new GameObject("PlayerHand", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            handRootObject.transform.SetParent(panel.transform, false);
            var handRoot = handRootObject.GetComponent<RectTransform>();
            handRoot.anchorMin = new Vector2(0.5f, 0f);
            handRoot.anchorMax = new Vector2(0.5f, 0f);
            handRoot.sizeDelta = new Vector2(920f, 260f);
            handRoot.anchoredPosition = new Vector2(0f, 110f);
            var layout = handRootObject.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            var playerInfo = CreateText("PlayerInfo", panel.transform, font, 34, TextAnchor.LowerCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 340f), new Vector2(700f, 120f));
            var cardPrefabObject = CreateCardPrefab(font);
            cardPrefabObject.SetActive(false);

            var resultPanel = CreatePanel("ResultPanel", panel.transform, new Color(0f, 0f, 0f, 0.72f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 420f));
            resultPanel.SetActive(false);
            var resultText = CreateText("ResultText", resultPanel.transform, font, 60, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(460f, 100f));
            var rematchButton = CreateButton("RematchButton", resultPanel.transform, font, "Rematch", new Vector2(0.5f, 0.28f), new Vector2(260f, 96f));

            waiting.gameObject.SetActive(false);
            invalid.gameObject.SetActive(false);

            return new BattleReferences
            {
                LeftPileView = leftPile,
                RightPileView = rightPile,
                PlayerHandRoot = handRoot,
                CardViewPrefab = cardPrefabObject.GetComponent<CardView>(),
                PlayerDeckText = playerDeck,
                CpuDeckText = cpuDeck,
                PlayerHandText = playerInfo,
                CpuHandText = cpuInfo,
                WaitingText = waiting,
                InvalidText = invalid,
                ResultPanel = resultPanel,
                ResultText = resultText,
                RematchButton = rematchButton
            };
        }

        private static TablePileView CreatePile(string name, Transform parent, Font font, Vector2 anchor)
        {
            var pileObject = CreatePanel(name, parent, Color.white, anchor, anchor, Vector2.zero, new Vector2(240f, 320f));
            var label = CreateText("Label", pileObject.transform, font, 64, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180f, 90f));
            var pileView = pileObject.AddComponent<TablePileView>();
            SetSerializedField(pileView, "label", label);
            SetSerializedField(pileView, "background", pileObject.GetComponent<Image>());
            return pileView;
        }

        private static GameObject CreateCardPrefab(Font font)
        {
            var cardObject = CreatePanel("CardPrefab", null, Color.white, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(150f, 220f));
            cardObject.AddComponent<CanvasGroup>();
            var label = CreateText("Label", cardObject.transform, font, 36, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(110f, 120f));
            var cardView = cardObject.AddComponent<CardView>();
            SetSerializedField(cardView, "label", label);
            return cardObject;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            if (parent != null)
            {
                panel.transform.SetParent(parent, false);
            }

            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static Text CreateText(string name, Transform parent, Font font, int fontSize, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            var text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, Font font, string labelText, Vector2 anchor, Vector2 size)
        {
            var buttonObject = CreatePanel(name, parent, new Color(0.95f, 0.95f, 0.95f, 1f), anchor, anchor, Vector2.zero, size);
            var button = buttonObject.AddComponent<Button>();
            var label = CreateText("Label", buttonObject.transform, font, 32, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
            label.text = labelText;
            label.color = Color.black;
            return button;
        }

        private static void ConfigureBattleUiController(BattleUIController controller, BattleReferences references)
        {
            SetSerializedField(controller, "leftPileView", references.LeftPileView);
            SetSerializedField(controller, "rightPileView", references.RightPileView);
            SetSerializedField(controller, "playerHandRoot", references.PlayerHandRoot);
            SetSerializedField(controller, "cardViewPrefab", references.CardViewPrefab);
            SetSerializedField(controller, "playerDeckCountText", references.PlayerDeckText);
            SetSerializedField(controller, "cpuDeckCountText", references.CpuDeckText);
            SetSerializedField(controller, "playerHandCountText", references.PlayerHandText);
            SetSerializedField(controller, "cpuHandCountText", references.CpuHandText);
            SetSerializedField(controller, "waitingText", references.WaitingText);
            SetSerializedField(controller, "invalidMoveText", references.InvalidText);
            SetSerializedField(controller, "resultPanel", references.ResultPanel);
            SetSerializedField(controller, "resultText", references.ResultText);
            SetSerializedField(controller, "rematchButton", references.RematchButton);
        }

        private static void ConfigureGameController(GameController controller, BattleUIController ui, BattleAnimationController animation, BattleInputController input, CpuController cpu)
        {
            SetSerializedField(controller, "battleUIController", ui);
            SetSerializedField(controller, "animationController", animation);
            SetSerializedField(controller, "inputController", input);
            SetSerializedField(controller, "cpuController", cpu);
        }

        private static void SetSerializedField(Object target, string fieldName, object value)
        {
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var field = target.GetType().GetField(fieldName, flags);
            field?.SetValue(target, value);
        }

        private sealed class BattleReferences
        {
            public TablePileView LeftPileView;
            public TablePileView RightPileView;
            public RectTransform PlayerHandRoot;
            public CardView CardViewPrefab;
            public Text PlayerDeckText;
            public Text CpuDeckText;
            public Text PlayerHandText;
            public Text CpuHandText;
            public Text WaitingText;
            public Text InvalidText;
            public GameObject ResultPanel;
            public Text ResultText;
            public Button RematchButton;
        }
    }
}
