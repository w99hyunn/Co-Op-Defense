using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace OpenFrame.Editor
{
    /// <summary>Explicit editor authoring command. The authored Canvas is saved in Ingame.</summary>
    public static class DefenseSceneAuthoring
    {
        private static TMP_FontAsset font;
        private static readonly Color Ink = new Color(0.87f, 0.94f, 0.98f);
        private static readonly Color Muted = new Color(0.52f, 0.66f, 0.74f);
        private static readonly Color Panel = new Color(0.075f, 0.12f, 0.17f);

        [MenuItem("OpenFrame/Author Defense Prototype")]
        public static void Create()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != "Assets/00_Co-Op/01_Scenes/Ingame.unity")
                throw new System.InvalidOperationException("Open Ingame before authoring.");
            if (GameObject.Find("DefenseCanvas") != null)
                throw new System.InvalidOperationException("DefenseCanvas already exists. Edit the existing hierarchy.");
            string fontPath = "Assets/00_Co-Op/04_Images/DefenseKorean.asset";
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            if (font == null)
            {
                var source = AssetDatabase.LoadAssetAtPath<Font>("Assets/Plugins/TextMesh Pro/Fonts/BlackHanSans-Regular.ttf");
                font = TMP_FontAsset.CreateFontAsset(source, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic);
                font.name = "DefenseKorean";
                font.isMultiAtlasTexturesEnabled = true;
                AssetDatabase.CreateAsset(font, fontPath);
                foreach (var atlas in font.atlasTextures) { atlas.name = "DefenseKorean Atlas"; AssetDatabase.AddObjectToAsset(atlas, font); }
                font.material.name = "DefenseKorean Material";
                AssetDatabase.AddObjectToAsset(font.material, font);
            }

            var root = new GameObject("DefenseCanvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            root.TryGetComponent<Canvas>(out var canvas);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.TryGetComponent<UnityEngine.UI.CanvasScaler>(out var scaler);
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(900, 1800);
            scaler.matchWidthOrHeight = 0.5f;
            var canvasRect = (RectTransform)root.transform;
            // A centered design rect fits inside any aspect ratio, rather than cropping controls.
            var stage = Rect("Stage", canvasRect, 0, 0, 1600, 1000);
            stage.anchorMin = stage.anchorMax = new Vector2(0.5f, 0.5f);
            stage.pivot = new Vector2(0.5f, 0.5f);
            // Children use a fixed 1600x1000 logical space scaled by the stage adapter.
            stage.gameObject.AddComponent<DefenseStageFit>();
            var background = Image("Background", stage, 800, 500, 1600, 1000, new Color(0.035f, 0.065f, 0.10f));
            var view = root.AddComponent<DefenseView>();
            var controller = root.AddComponent<DefenseController>();
            controller.Configure(view);
            Text("Brand", stage, "BLOCK / BASTION", 65, 959, 650, 55, 36, Ink);
            Text("Tagline", stage, "블록을 완성하고, 방어선을 지켜라", 68, 918, 600, 35, 18, Muted);
            view.status = Text("Status", stage, "WAVE  00 / 06     |     방어선  20 / 20", 940, 955, 610, 55, 26, Ink);
            view.record = Text("Record", stage, "최고 기록  0웨이브 / 0줄", 1020, 917, 530, 30, 16, Muted);
            Image("PartnerField", stage, 800, 840, 1470, 100, Panel);
            for (int i = 0; i < 19; i++) Image("PartnerTile", stage, 170 + i * 70, 840, 60, 60, new Color(0.09f, 0.14f, 0.19f));
            Image("PartnerLabelBacking", stage, 800, 840, 520, 62, Panel);
            Text("PartnerLabel", stage, "상단 협동 필드  ·  이번 검증에서는 비활성", 540, 840, 520, 45, 20, Muted, TextAlignmentOptions.Center);

            view.arena = Rect("MonsterLane", stage, 40, 710, 1520, 110);
            view.arena.pivot = new Vector2(0, 0.5f);
            Image("Road", view.arena, 760, 0, 1520, 110, new Color(0.13f, 0.16f, 0.22f));
            Image("RoadTop", view.arena, 760, 55, 1520, 3, new Color(0.34f, 0.27f, 0.34f));
            Image("RoadBottom", view.arena, 760, -55, 1520, 3, new Color(0.34f, 0.27f, 0.34f));
            for (int i = 0; i < 17; i++) Text("RoadDirection", view.arena, "›", 95 + i * 80, 0, 35, 40, 32, new Color(0.25f, 0.28f, 0.34f), TextAlignmentOptions.Center);
            Image("Portal", view.arena, 32, 0, 44, 84, new Color(0.58f, 0.27f, 0.56f));
            Image("Gate", view.arena, 1490, 0, 30, 84, new Color(0.28f, 0.79f, 0.74f));
            Text("PortalLabel", stage, "출현", 65, 632, 120, 32, 18, Muted);
            Text("GateLabel", stage, "방어선", 1430, 632, 120, 32, 18, Muted, TextAlignmentOptions.Right);
            Image("HeroBase", view.arena, 760, -115, 70, 42, new Color(0.15f, 0.31f, 0.38f));
            var body = Image("Player", view.arena, 760, -105, 34, 40, new Color(0.37f, 0.92f, 0.87f));
            Image("PlayerVisor", view.arena, 760, -98, 25, 8, new Color(0.04f, 0.16f, 0.23f));
            Image("PlayerBarrel", view.arena, 760, -77, 10, 25, new Color(0.71f, 1, 0.92f));
            view.enemies = new DefenseEnemyView[40];
            for (int i = 0; i < view.enemies.Length; i++)
            {
                var enemy = Rect("Enemy" + i, view.arena, 100, 0, 36, 36);
                var enemyBody = Image("Body", enemy, 0, 0, 36, 36, Color.white);
                Stretch(enemyBody.rectTransform, 0);
                Image("LeftEye", enemy, -8, 3, 6, 7, new Color(0.08f, 0.08f, 0.13f));
                Image("RightEye", enemy, 8, 3, 6, 7, new Color(0.08f, 0.08f, 0.13f));
                Image("HealthBack", enemy, 0, 34, 48, 5, new Color(0.1f, 0.07f, 0.12f));
                var hp = Image("Health", enemy, 0, 34, 48, 5, new Color(0.5f, 0.94f, 0.7f));
                Filled(hp);
                var label = Text("Kind", enemy, "", -35, -32, 70, 22, 14, Ink, TextAlignmentOptions.Center);
                view.enemies[i] = new DefenseEnemyView { root = enemy, body = enemyBody, health = hp, label = label };
                enemy.gameObject.SetActive(false);
            }
            view.beam = Image("AttackBeam", view.arena, 760, -115, 10, 3, new Color(0.7f, 1, 0.83f)).rectTransform;
            view.beam.pivot = new Vector2(0, 0.5f);
            view.beam.gameObject.SetActive(false);
            view.hitText = Text("DamageNumber", view.arena, "", 0, 40, 100, 40, 24, new Color(1, 0.87f, 0.5f), TextAlignmentOptions.Center);
            view.hitText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            view.pulse = Image("ClearPulse", view.arena, 760, 0, 1520, 110, Color.clear);

            Text("FieldTitle", stage, "01 / MY FIELD", 510, 604, 250, 32, 18, Muted);
            Image("BoardBorder", stage, 800, 404, 596, 364, new Color(0.22f, 0.34f, 0.4f));
            view.board = Rect("PlayerBoard", stage, 510, 230, 580, 348);
            view.board.pivot = Vector2.zero;
            var boardSurface = view.board.gameObject.AddComponent<UnityEngine.UI.Image>();
            boardSurface.color = new Color(0.045f, 0.085f, 0.12f);
            boardSurface.raycastTarget = true;
            view.boardInput = view.board.gameObject.AddComponent<DefensePointer>();
            view.boardCells = new UnityEngine.UI.Image[60];
            for (int y = 0; y < 6; y++) for (int x = 0; x < 10; x++)
            {
                var tile = Image($"Cell_{x}_{y}", view.board, x * 58 + 29, y * 58 + 29, 53, 53, DefenseView.ElementColor(0));
                view.boardCells[y * 10 + x] = tile;
            }
            view.ghosts = new UnityEngine.UI.Image[4];
            for (int i = 0; i < 4; i++)
            {
                view.ghosts[i] = Image("PlacementGhost" + i, view.board, 0, 0, 53, 53, Color.white);
                view.ghosts[i].gameObject.SetActive(false);
            }
            view.clearText = Text("ClearFeedback", stage, "", 470, 410, 660, 80, 32, Ink, TextAlignmentOptions.Center);
            view.clearText.gameObject.SetActive(false);

            Text("HowToTitle", stage, "조합이 곧 힘이다", 75, 565, 360, 50, 28, Ink);
            Text("HowTo", stage, "1. 블록을 선택 / 드래그\n2. 빈 칸에 배치\n3. 가로 10칸 완성 → 강화", 78, 485, 370, 105, 22, Muted);
            view.rotateButton = Button("Rotate", stage, "회전  [R]", 250, 370, 345, 54, new Color(0.17f, 0.3f, 0.37f), out _);
            view.redrawButton = Button("Redraw", stage, "다시 뽑기  ·  부품 1", 250, 301, 345, 54, Panel, out _);
            view.rescueButton = Button("Rescue", stage, "긴급 정리  ·  부품 4", 250, 232, 345, 54, Panel, out _);
            Text("RescueHint", stage, "가장 찬 줄 제거 · 강화 없음\n막혔을 때 부품 부족 시 내구도 2 사용", 78, 170, 365, 52, 16, Muted);
            Text("KeyboardHint", stage, "1 / 2 / 3 선택   ·   R 회전   ·   ESC 일시정지", 78, 65, 390, 35, 14, Muted);

            Image("StatsPanel", stage, 1340, 416, 390, 332, Panel);
            Text("StatsTitle", stage, "DEFENDER / 성장", 1172, 548, 340, 40, 24, Ink);
            view.stats = Text("Stats", stage, "", 1175, 407, 345, 236, 23, Ink);
            Text("Elements", stage, "<color=#FF6B57>화력</color> 공격력  ·  <color=#4DB3FF>속사</color> 공속\n<color=#52E0A6>냉기</color> 둔화  ·  모든 줄 기본 공격력 +2", 1168, 208, 380, 65, 17, Muted);
            view.pauseButton = Button("Pause", stage, "일시정지", 1340, 116, 350, 54, Panel, out view.pauseLabel);

            view.supply = Text("Supply", stage, "보급 5 / 5", 510, 202, 580, 30, 17, Muted, TextAlignmentOptions.Center);
            Image("SupplyBack", stage, 800, 179, 580, 5, Panel);
            view.supplyBar = Image("SupplyFill", stage, 800, 179, 580, 5, new Color(0.33f, 0.85f, 0.71f));
            Filled(view.supplyBar);
            view.offers = new DefenseOfferView[3];
            for (int i = 0; i < 3; i++)
            {
                var card = Image("Offer" + i, stage, 600 + i * 200, 106, 180, 128, Panel);
                card.raycastTarget = true;
                var pointer = card.gameObject.AddComponent<DefensePointer>();
                var tiles = new UnityEngine.UI.Image[4];
                for (int j = 0; j < 4; j++) tiles[j] = Image("Block" + j, card.rectTransform, 0, 0, 22, 22, Ink);
                var label = Text("Label", card.rectTransform, "", -85, -46, 170, 24, 16, Ink, TextAlignmentOptions.Center);
                view.offers[i] = new DefenseOfferView { background = card, cells = tiles, label = label, input = pointer };
            }
            view.notice = Text("Notice", stage, "블록을 배치하고 가로 한 줄을 완성하세요", 350, 20, 900, 30, 17, new Color(0.65f, 0.88f, 0.83f), TextAlignmentOptions.Center);
            view.progress = Text("Progress", stage, "", 520, 780, 600, 30, 17, Muted, TextAlignmentOptions.Center);
            Image("WaveProgressBack", stage, 800, 769, 580, 4, Panel);
            view.waveBar = Image("WaveProgress", stage, 800, 769, 580, 4, new Color(0.6f, 0.83f, 0.97f));
            Filled(view.waveBar);

            var overlayImage = Image("Overlay", stage, 800, 500, 1600, 1000, new Color(0.015f, 0.03f, 0.05f, 0.91f));
            overlayImage.raycastTarget = true;
            view.overlay = overlayImage.gameObject;
            Image("Modal", overlayImage.rectTransform, 0, 0, 1120, 570, new Color(0.075f, 0.13f, 0.18f));
            Image("ModalAccent", overlayImage.rectTransform, 0, 285, 1120, 4, new Color(0.37f, 0.9f, 0.77f));
            view.overlayTitle = Text("Title", overlayImage.rectTransform, "작은 블록, 거대한 방어", -520, 198, 1040, 70, 42, Ink, TextAlignmentOptions.Center);
            view.overlayBody = Text("Description", overlayImage.rectTransform, "", -490, 45, 980, 210, 25, Muted, TextAlignmentOptions.Center);
            view.primaryButton = Button("Primary", overlayImage.rectTransform, "방어 시작", 0, -195, 520, 72, new Color(0.18f, 0.54f, 0.47f), out view.primaryLabel);
            view.upgrades = new UnityEngine.UI.Button[3];
            string[] choices = { "집중 포격\n<size=18>공격력 +6</size>", "연쇄 탄환\n<size=18>공속 +20% · 인접 적에 20% 추가 피해</size>", "재정비\n<size=18>내구도 +6 · 부품 +4 · 둔화 +5%</size>" };
            for (int i = 0; i < 3; i++)
            {
                view.upgrades[i] = Button("Upgrade" + i, overlayImage.rectTransform, choices[i], -360 + i * 360, -176, 335, 125, new Color(0.14f, 0.28f, 0.34f), out _);
                view.upgrades[i].gameObject.SetActive(false);
            }
            // Preview data ensures the authored scene is readable before Play mode too.
            if (Object.FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            stage.TryGetComponent<DefenseStageFit>(out var stageFit);
            stageFit.ApplyPortraitLayout();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            DefenseAudioAuthoring.Install();
            DefenseVisualRevision.Apply();
            Debug.Log("OpenFrame defense scene authored and saved.");
        }

        private static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            // Nested UI elements use coordinates relative to the parent's pivot.
            if (parent is RectTransform pr && parent.name != "Stage") rect.anchorMin = rect.anchorMax = pr.pivot;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        private static UnityEngine.UI.Image Image(string name, Transform parent, float x, float y, float w, float h, Color color)
        {
            var rect = Rect(name, parent, x, y, w, h);
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static TMP_Text Text(string name, Transform parent, string content, float x, float y, float w, float h, float size, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft)
        {
            var rect = Rect(name, parent, x, y, w, h);
            rect.pivot = new Vector2(0, 0.5f);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false;
            return text;
        }

        private static UnityEngine.UI.Button Button(string name, Transform parent, string label, float x, float y, float w, float h, Color color, out TMP_Text text)
        {
            var image = Image(name, parent, x, y, w, h, color);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(1.22f, 1.22f, 1.22f);
            colors.pressedColor = new Color(0.75f, 0.85f, 0.9f);
            button.colors = colors;
            button.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
            text = Text("Label", image.rectTransform, label, -w / 2 + 8, 0, w - 16, h - 8, 23, Ink, TextAlignmentOptions.Center);
            return button;
        }

        private static void Filled(UnityEngine.UI.Image image)
        {
            // A white sprite is required for Image.Type.Filled to clip rather than draw a simple quad.
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = UnityEngine.UI.Image.Type.Filled;
            image.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            image.fillOrigin = 0;
        }

        private static void Stretch(RectTransform rect, float padding)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.one * padding;
            rect.offsetMax = Vector2.one * -padding;
        }
    }
}
