using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;

namespace OpenFrame.Editor
{
    /// <summary>Updates the existing Canvas in place; never replaces the scene hierarchy.</summary>
    public static class DefenseVisualRevision
    {
        private static Sprite rounded;
        private static TMP_FontAsset font;
        private static readonly Color Ink = new Color(0.20f, 0.23f, 0.30f);
        private static readonly Color Gray = new Color(0.87f, 0.88f, 0.90f);

        public static string Apply()
        {
            var view = UnityEngine.Object.FindAnyObjectByType<DefenseView>();
            if(view.world!=null)return DefenseWorldAuthoring.Apply();
            if (view.gameObject.scene.path != "Assets/00_Co-Op/01_Scenes/Ingame.unity") throw new InvalidOperationException("Open Ingame first.");
            rounded = RoundedSprite();
            font = view.status.font;
            var sprites = Enumerable.Repeat(rounded, 9).ToArray();
            view.unitSprites = new[] { sprites[0], sprites[1], sprites[2] };
            view.enemySprites = new[] { sprites[3], sprites[4], sprites[5], sprites[6] };
            view.projectileSprites = new[] { sprites[7], sprites[8] };
            var stage = (RectTransform)view.transform.Find("Stage");
            stage.sizeDelta = new Vector2(900, 1800);
            view.TryGetComponent<UnityEngine.UI.CanvasScaler>(out var scaler);
            scaler.referenceResolution = new Vector2(900, 1800);
            foreach (var text in view.GetComponentsInChildren<TMP_Text>(true))
            {
                text.color = Ink;
                text.fontStyle = FontStyles.Normal;
            }
            string[] hide = { "Brand", "Tagline", "HowToTitle", "HowTo", "KeyboardHint", "RescueHint", "StatsTitle", "Elements", "StatsPanel", "PartnerLabelBacking", "PortalLabel", "GateLabel" };
            foreach (string name in hide) stage.Find(name).gameObject.SetActive(false);
            foreach (Transform child in stage) if (child.name == "PartnerTile") child.gameObject.SetActive(false);
            var background = ExistingImage(stage, "Background");
            Put(background.rectTransform, 450, 900, 900, 1800);
            background.sprite = null;
            background.color = Color.clear;
            var terrain = Node(stage, "IllustratedTerrain"); Put(terrain, 450, 900, 900, 1800); terrain.SetSiblingIndex(1);
            if (!terrain.TryGetComponent<CanvasRenderer>(out var renderer)) terrain.gameObject.AddComponent<CanvasRenderer>();
            if (!terrain.TryGetComponent<DefenseTerrainGraphic>(out var ground)) ground = terrain.gameObject.AddComponent<DefenseTerrainGraphic>(); ground.raycastTarget = false;
            var header = Panel(stage, "MatchHeader", 450, 1753, 840, 74, new Color(0.12f, 0.11f, 0.16f), 18);
            header.transform.SetSiblingIndex(2);
            Put(view.status.rectTransform, 52, 1753, 685, 58, new Vector2(0, 0.5f));
            view.status.fontSize = 28;
            view.status.color = Color.white;
            Put((RectTransform)view.pauseButton.transform, 796, 1753, 80, 52);
            SkinButton(view.pauseButton, new Color(0.36f, 0.42f, 0.54f));
            view.pauseLabel.text = "II";
            view.pauseLabel.fontSize = 24;
            view.pauseLabel.color = Color.white;
            Put(view.stats.rectTransform, 73, 1710, 754, 30, new Vector2(0, 0.5f));
            view.stats.fontSize = 19;
            view.stats.alignment = TextAlignmentOptions.Center;
            Put(view.record.rectTransform, 520, 1195, 305, 30, new Vector2(0, 0.5f));
            view.record.fontSize = 19;
            view.record.alignment = TextAlignmentOptions.Right;

            // Both deployment fields have the same actual 10x6 grid and dimensions.
            var partnerFrame = ExistingImage(stage, "PartnerField");
            Put(partnerFrame.rectTransform, 450, 1449, 778, 476);
            Skin(partnerFrame, new Color(0.88f, 0.71f, 0.53f), 12);
            Outline(partnerFrame, new Color(0.47f, 0.37f, 0.31f), 2);
            for (int y = 0; y < 6; y++) for (int x = 0; x < 10; x++)
                Panel(partnerFrame.rectTransform, $"PartnerCell_{x}_{y}", -377 + (x + 0.5f) * 75.4f, -226 + (y + 0.5f) * 75.4f, 69, 69,
                    (x + y) % 2 == 0 ? new Color(0.77f, 0.62f, 0.47f, 0.27f) : new Color(0.96f, 0.80f, 0.62f, 0.36f), 7);
            Put(((RectTransform)stage.Find("PartnerLabel")), 78, 1195, 440, 30, new Vector2(0, 0.5f));
            SetText(stage, "PartnerLabel", "동료 필드  ·  참가 대기", 20, new Color(0.32f, 0.24f, 0.22f));
            var frame = ExistingImage(stage, "BoardBorder");
            Put(frame.rectTransform, 450, 566, 778, 476);
            Skin(frame, new Color(0.97f, 0.80f, 0.60f), 12);
            Outline(frame, new Color(0.49f, 0.37f, 0.28f), 3);
            Put(view.board, 73, 340, 580, 348, Vector2.zero);
            view.board.localScale = new Vector3(1.3f, 1.3f, 1);
            view.board.TryGetComponent<UnityEngine.UI.Image>(out var boardBacking);
            boardBacking.color = Color.clear;
            view.boardUnits = new UnityEngine.UI.Image[60];
            for (int y = 0; y < 6; y++) for (int x = 0; x < 10; x++)
            {
                int index = y * 10 + x;
                Skin(view.boardCells[index], Gray, 6);
                var unit = Panel(view.board, "BlockUnit" + index, x * 58 + 29, y * 58 + 29, 54, 54, Color.white, 7);
                unit.type = UnityEngine.UI.Image.Type.Sliced; unit.pixelsPerUnitMultiplier = 3.2f; Outline(unit, new Color(0.15f,0.12f,0.19f), 1.4f);
                unit.sprite = sprites[0];
                unit.preserveAspect = true;
                unit.gameObject.SetActive(false);
                view.boardUnits[index] = unit;
            }
            foreach (var ghost in view.ghosts) { Skin(ghost, new Color(0.3f, 0.7f, 1, 0.5f), 6); ghost.transform.SetAsLastSibling(); }
            Put((RectTransform)stage.Find("FieldTitle"), 75, 816, 500, 38, new Vector2(0, 0.5f));
            SetText(stage, "FieldTitle", "내 필드", 24, new Color(0.21f, 0.40f, 0.68f));

            // Rounded rectangular loop. The center remains a separate usable-looking field.
            foreach (Transform child in view.arena) child.gameObject.SetActive(false);
            Put(view.arena, 0, 0, 900, 1800, Vector2.zero);
            view.arena.localScale = Vector3.one;
            var roadBorder = Panel(view.arena, "LoopBorder", 450, 1005, 850, 338, new Color(0.46f, 0.35f, 0.27f), 95);
            Panel(view.arena, "LoopRoad", 450, 1005, 840, 328, new Color(0.99f, 0.85f, 0.65f), 90);
            Panel(view.arena, "LoopInset", 450, 1005, 665, 182, new Color(0.62f, 0.47f, 0.32f), 30);
            var island = Panel(view.arena, "CenterField", 450, 1005, 655, 172, new Color(0.88f, 0.70f, 0.49f), 26);
            for (int y = 0; y < 2; y++) for (int x = 0; x < 8; x++)
                Panel(island.rectTransform, $"CenterCell_{x}_{y}", -280 + x * 80, -38 + y * 76, 73, 67, new Color(0.95f, 0.79f, 0.57f), 8);
            var islandLabel = Label(island.rectTransform, "CenterLabel", "중앙 필드", 0, 0, 300, 35, 20, new Color(0.56f, 0.42f, 0.28f));
            for (int i = 0; i < 28; i++)
            {
                float t = i / 28f;
                Vector2 point = DefenseArena.LoopPoint(t);
                Vector2 tangent = DefenseArena.LoopPoint(t + 0.002f) - point;
                var arrow = Label(view.arena, "LoopArrow" + i, "›", point.x, point.y, 24, 28, 26, new Color(0.73f, 0.55f, 0.36f));
                arrow.rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg);
            }
            Put(view.progress.rectTransform, 110, 853, 680, 30, new Vector2(0, 0.5f));
            view.progress.fontSize = 18;
            Put(((RectTransform)stage.Find("WaveProgressBack")), 450, 1730, 640, 4);
            Put(view.waveBar.rectTransform, 450, 1730, 640, 4);
            view.waveBar.color = new Color(1, 0.53f, 0.28f);
            // Expand existing pool, preserving the original bound objects.
            var pool = new DefenseEnemyView[80];
            for (int i = 0; i < pool.Length; i++)
            {
                if (i < view.enemies.Length) pool[i] = view.enemies[i];
                else
                {
                    var clone = UnityEngine.Object.Instantiate(view.enemies[0].root.gameObject, view.arena);
                    clone.name = "Enemy" + i;
                    clone.transform.Find("Body").TryGetComponent<UnityEngine.UI.Image>(out var body);
                    clone.transform.Find("Health").TryGetComponent<UnityEngine.UI.Image>(out var health);
                    clone.transform.Find("Kind").TryGetComponent<TMP_Text>(out var label);
                    pool[i] = new DefenseEnemyView { root = (RectTransform)clone.transform, body = body, health = health, label = label };
                }
                var enemy = pool[i];
                enemy.root.SetAsLastSibling();
                enemy.root.anchorMin = enemy.root.anchorMax = Vector2.zero;
                foreach (Transform child in enemy.root) child.gameObject.SetActive(child.name == "Body" || child.name == "Kind");
                enemy.body.color = Color.white;
                enemy.body.preserveAspect = true;
                enemy.body.sprite = sprites[3];
                Put(enemy.label.rectTransform, 0, 0, 60, 35);
                enemy.label.fontSize = 21;
                enemy.label.color = Color.white;
                enemy.label.alignment = TextAlignmentOptions.Center;
                enemy.root.gameObject.SetActive(false);
            }
            view.enemies = pool;
            var effects = Node(stage, "CombatEffects");
            Put(effects, 0, 0, 900, 1800, Vector2.zero);
            view.projectiles = new UnityEngine.UI.Image[96];
            for (int i = 0; i < view.projectiles.Length; i++)
            {
                var shot = Panel(effects, "Projectile" + i, 0, 0, 12, 12, Color.white, 0);
                shot.sprite = sprites[7]; shot.type = UnityEngine.UI.Image.Type.Simple;
                shot.preserveAspect = true; shot.gameObject.SetActive(false);
                view.projectiles[i] = shot;
            }
            view.pulse.transform.SetParent(effects, false);
            Put(view.pulse.rectTransform, 450, 1005, 840, 328);
            Skin(view.pulse, Color.clear, 90);
            view.pulse.gameObject.SetActive(true);
            view.beam.gameObject.SetActive(false); view.hitText.gameObject.SetActive(false);
            Put(view.clearText.rectTransform, 70, 805, 760, 65, new Vector2(0, 0.5f));
            view.clearText.fontSize = 30;
            view.clearText.outlineWidth = 0.18f;
            view.clearText.outlineColor = new Color(0.19f, 0.22f, 0.32f);
            view.clearText.transform.SetAsLastSibling();

            var footer = Panel(stage, "ControlDeck", 450, 160, 900, 320, new Color(0.15f, 0.13f, 0.22f), 0);
            footer.transform.SetSiblingIndex(3);
            Put(view.supply.rectTransform, 50, 309, 800, 30, new Vector2(0, 0.5f));
            view.supply.fontSize = 21; view.supply.color = Color.white;
            Put(((RectTransform)stage.Find("SupplyBack")), 450, 289, 760, 5);
            Put(view.supplyBar.rectTransform, 450, 289, 760, 5);
            view.supplyBar.color = new Color(0.2f, 0.78f, 0.91f);
            for (int i = 0; i < 3; i++)
            {
                var offer = view.offers[i];
                Put(offer.background.rectTransform, 200 + i * 250, 207, 225, 140);
                offer.background.rectTransform.localScale = Vector3.one;
                Skin(offer.background, Color.white, 14);
                Outline(offer.background, new Color(0.11f, 0.15f, 0.24f), 3);
                foreach (var cell in offer.cells) { cell.rectTransform.sizeDelta = new Vector2(28, 28); cell.preserveAspect = true; Skin(cell, Color.white, 5); Outline(cell, new Color(0.13f,0.11f,0.18f), 1); }
                Put(offer.label.rectTransform, -104, -51, 208, 25, new Vector2(0, 0.5f));
                offer.label.fontSize = 20; offer.label.color = Color.white;
            }
            Put((RectTransform)view.rotateButton.transform, 173, 91, 250, 60);
            Put((RectTransform)view.redrawButton.transform, 450, 91, 250, 60);
            Put((RectTransform)view.rescueButton.transform, 727, 91, 250, 60);
            SkinButton(view.rotateButton, new Color(0.45f, 0.28f, 0.78f));
            SkinButton(view.redrawButton, new Color(0.34f, 0.27f, 0.49f));
            SkinButton(view.rescueButton, new Color(0.34f, 0.27f, 0.49f));
            foreach (var button in new[] { view.rotateButton, view.redrawButton, view.rescueButton })
            { button.transform.GetChild(0).TryGetComponent<TMP_Text>(out var label); label.color = Color.white; label.fontSize = 22; }
            SetText(view.rotateButton.transform, "Label", "회전", 22, Color.white);
            Put(view.notice.rectTransform, 40, 30, 820, 40, new Vector2(0, 0.5f));
            view.notice.fontSize = 18; view.notice.color = new Color(0.84f, 0.89f, 0.98f);
            SkinModal(view);
            view.overlay.transform.SetAsLastSibling();
            // Replace old serialized values, not merely the C# defaults.
            view.TryGetComponent<DefenseController>(out var controller);
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("rules.startingDamage").floatValue = 1.8f;
            serialized.FindProperty("rules.attackSeconds").floatValue = 1.1f;
            serialized.FindProperty("rules.maximumEnemies").intValue = 80;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            DefenseEvolutionAuthoring.Apply();
            DefenseEconomyAuthoring.Apply();
            stage.TryGetComponent<DefenseStageFit>(out var fit);
            fit.ApplyPortraitLayout();
            EditorUtility.SetDirty(view);
            EditorSceneManager.MarkSceneDirty(view.gameObject.scene);
            EditorSceneManager.SaveScene(view.gameObject.scene);
            AssetDatabase.SaveAssets();
            DefenseWorldAuthoring.Apply();
            return "Updated existing scene: two 10x6 fields, center field, loop, cell attacks, reference UI tone.";
        }

        private static void SkinModal(DefenseView view)
        {
            view.overlay.TryGetComponent<UnityEngine.UI.Image>(out var dim);
            Put(dim.rectTransform, 450, 900, 900, 1800);
            dim.color = new Color(0.12f, 0.16f, 0.23f, 0.78f);
            var modal = ExistingImage(view.overlay.transform, "Modal");
            Skin(modal, new Color(0.18f, 0.15f, 0.26f), 24);
            modal.rectTransform.sizeDelta = new Vector2(820, 850);
            ExistingImage(view.overlay.transform, "ModalAccent").color = new Color(0.2f, 0.58f, 0.95f);
            view.overlayTitle.color = new Color(1, 0.85f, 0.3f); view.overlayTitle.fontSize = 36;
            view.overlayBody.color = new Color(0.87f, 0.85f, 0.94f); view.overlayBody.fontSize = 25;
            SkinButton(view.primaryButton, new Color(0.2f, 0.57f, 0.94f));
            view.primaryLabel.color = Color.white;
            string[] labels = { "화력 증폭\n<size=20>각 칸 기본 공격력 +1.2</size>", "연쇄 탄환\n<size=20>공속 +20% · 주변에 추가 피해</size>", "보급 지원\n<size=20>부품 +4 · 둔화 +5%</size>" };
            for (int i = 0; i < view.upgrades.Length; i++)
            {
                SkinButton(view.upgrades[i], i == 0 ? new Color(0.95f, 0.37f, 0.34f) : i == 1 ? new Color(0.23f, 0.53f, 0.93f) : new Color(0.12f, 0.65f, 0.66f));
                view.upgrades[i].transform.GetChild(0).TryGetComponent<TMP_Text>(out var label);
                label.color = Color.white; label.text = labels[i];
            }
        }

        private static Sprite RoundedSprite()
        {
            string path = "Assets/00_Co-Op/04_Images/DefenseRoundedPanel.asset";
            var existing = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
            if (existing != null) return existing;
            var texture = new Texture2D(96, 96, TextureFormat.RGBA32, false) { name = "RoundedPanelTexture" };
            for (int y = 0; y < 96; y++) for (int x = 0; x < 96; x++)
            {
                float dx = Mathf.Max(Mathf.Abs(x - 47.5f) - 23.5f, 0), dy = Mathf.Max(Mathf.Abs(y - 47.5f) - 23.5f, 0);
                texture.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(24 - Mathf.Sqrt(dx * dx + dy * dy))));
            }
            texture.Apply();
            AssetDatabase.CreateAsset(texture, path);
            var sprite = Sprite.Create(texture, new Rect(0, 0, 96, 96), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(25, 25, 25, 25));
            sprite.name = "RoundedPanel"; AssetDatabase.AddObjectToAsset(sprite, texture);
            return sprite;
        }

        private static RectTransform Node(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null) return (RectTransform)existing;
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }
        private static void Put(RectTransform rect, float x, float y, float w, float h, Vector2? pivot = null)
        {
            var parent = (RectTransform)rect.parent;
            rect.anchorMin = rect.anchorMax = parent.name == "Stage" ? Vector2.zero : parent.pivot;
            rect.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y); rect.sizeDelta = new Vector2(w, h);
        }
        private static UnityEngine.UI.Image ExistingImage(Transform parent, string name)
        { parent.Find(name).TryGetComponent<UnityEngine.UI.Image>(out var image); return image; }
        private static UnityEngine.UI.Image Panel(Transform parent, string name, float x, float y, float w, float h, Color color, float radius)
        {
            var rect = Node(parent, name);
            Put(rect, x, y, w, h);
            if (!rect.TryGetComponent<UnityEngine.UI.Image>(out var image)) image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            Skin(image, color, radius); image.raycastTarget = false; image.gameObject.SetActive(true);
            return image;
        }
        private static void Skin(UnityEngine.UI.Image image, Color color, float radius)
        { image.sprite = radius > 0 ? rounded : null; image.type = radius > 0 ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple; image.pixelsPerUnitMultiplier = radius > 0 ? 25 / radius : 1; image.color = color; }
        private static void Outline(UnityEngine.UI.Image image, Color color, float distance)
        {
            if (!image.TryGetComponent<UnityEngine.UI.Outline>(out var outline)) outline = image.gameObject.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = color; outline.effectDistance = new Vector2(distance, -distance);
        }
        private static void SkinButton(UnityEngine.UI.Button button, Color color)
        {
            button.TryGetComponent<UnityEngine.UI.Image>(out var image); Skin(image, color, 10);
            Outline(image, new Color(0.14f, 0.18f, 0.27f), 2);
            var label = (RectTransform)button.transform.GetChild(0);
            label.anchorMin = Vector2.zero; label.anchorMax = Vector2.one;
            label.offsetMin = new Vector2(8, 4); label.offsetMax = new Vector2(-8, -4);
        }
        private static TMP_Text Label(Transform parent, string name, string value, float x, float y, float w, float h, float size, Color color)
        {
            var rect = Node(parent, name); Put(rect, x, y, w, h);
            if (!rect.TryGetComponent<TextMeshProUGUI>(out var text)) text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.gameObject.SetActive(true); text.font = font; text.text = value; text.fontSize = size; text.color = color; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            return text;
        }
        private static void SetText(Transform parent, string name, string value, float size, Color color)
        { parent.Find(name).TryGetComponent<TMP_Text>(out var text); text.text = value; text.fontSize = size; text.color = color; }
    }
}








