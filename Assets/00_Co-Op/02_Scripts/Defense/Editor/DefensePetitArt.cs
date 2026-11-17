using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace OpenFrame.Editor
{
    // Visual sources, frame timecodes and adaptation rationale: Docs/ArtReferences/PetitPlanet/Review.md.
    public static class DefensePetitArt
    {
        public static void Apply(DefenseView view)
        {
            var world = view.world;
            var stage = view.transform.Find("Stage");
            var kit = DefenseReferenceUI.Atlas("PetitUI.png", 3, 4, "PetitUI", rowEdges: new[] {0f, .24f, .49f, .735f, 1f});
            var panels = DefenseReferenceUI.Atlas("PetitPanels.png", 2, 1, "PetitPanel");
            var cast = DefenseReferenceUI.Atlas("PetitCast.png", 4, 4, "PetitCast");
            var feedback = DefenseReferenceUI.Atlas("PetitFeedback.png", 3, 2, "PetitFeedback");
            world.shotArt = feedback.Take(3).ToArray();
            world.sparkSprite = feedback[3];
            world.ringSprite = feedback[4];
            world.rewardSprite = feedback[5];
            var grid = DefenseReferenceUI.Atlas("PetitCell.png", 1, 1, "PetitCell")[0];
            var gridImporter = (TextureImporter)AssetImporter.GetAtPath("Assets/00_Co-Op/04_Images/PetitCell.png");
            gridImporter.mipmapEnabled = true;
            gridImporter.filterMode = FilterMode.Trilinear;
            gridImporter.SaveAndReimport();
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++) world.weaponSprites[row * 3 + col] = cast[row * 4 + col];
                world.supportArt[row] = cast[row * 4 + 3];
            }
            world.monsterArt = cast.Skip(12).ToArray();
            view.unitSprites = kit.Skip(9).Take(3).ToArray();
            world.blockArt = view.unitSprites;
            var terrain = world.transform.Find("SeamlessArena");
            terrain.TryGetComponent<SpriteRenderer>(out var ground);
            var path = "Assets/00_Co-Op/04_Images/PetitGarden.png";
            DefenseArtAuthoring.Configure(path);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            ground.sprite = DefenseArtAuthoring.Slice(path, new[] {new Rect(0, 0, texture.width, texture.height)}, "PetitGarden")[0];
            ground.size = new Vector2(20f * texture.width / texture.height, 20);
            ground.color = Color.white;
            ground.transform.localScale = Vector3.one;
            world.worldCamera.backgroundColor = Hex("FDE9C7");

            // Keep rendered grid, unit origins and screen hit testing on the same stage coordinates.
            for (int y = 0; y < 6; y++) for (int x = 0; x < 10; x++)
            {
                int i = y * 10 + x;
                var bottom = world.transform.Find($"Cell_{x}_{y}");
                var top = world.transform.Find($"PartnerCell_{x}_{y}");
                bottom.position = DefenseWorldView.Position(DefenseArena.CellPosition(x, y));
                top.position = DefenseWorldView.Position(DefenseArena.PartnerCellPosition(x, y));
                world.blocks[i].transform.position = bottom.position;
                world.weapons[i].transform.position = bottom.position + new Vector3(0, .04f, -.01f);
                foreach (var cell in new[] {bottom, top})
                {
                    cell.TryGetComponent<SpriteRenderer>(out var renderer);
                    renderer.sprite = grid;
                    renderer.color = new Color(1, 1, .88f, cell == top ? .38f : .58f);
                    DefenseWorldView.FitArtwork(renderer, Vector2.one * .655f);
                }
                world.clearEchoes[i].sprite = view.unitSprites[0];
            }
            world.boardInput.transform.position = DefenseWorldView.Position((DefenseArena.CellPosition(0, 0) + DefenseArena.CellPosition(9, 5)) * .5f);
            world.boardInput.TryGetComponent<BoxCollider2D>(out var fieldCollider);
            fieldCollider.size = new Vector2(6.6f, 3.96f);
            foreach (var renderer in world.GetComponentsInChildren<SpriteRenderer>(true))
                if (renderer.name.StartsWith("CenterCell_"))
                {renderer.sprite = grid; renderer.color = new Color(1, 1, .88f, .34f); DefenseWorldView.FitArtwork(renderer, Vector2.one * .62f);}
            foreach (var ghost in world.ghosts) {ghost.sprite = grid; DefenseWorldView.FitArtwork(ghost, Vector2.one * .655f);}

            Set(stage.Find("MatchHeader"), kit[1]); Place(stage.Find("MatchHeader"), 450, 1940, 270, 78);
            Place(view.monsterCount.transform, 450, 1941, 240, 60, 44);
            Place(view.status.transform, 165, 1958, 230, 48, 34);
            Place(view.progress.transform, 165, 1912, 230, 42, 30);
            Set(view.pauseButton.transform, kit[3]); Place(view.pauseButton.transform, 812, 1940, 78, 78);
            stage.Find("WaveProgressBack").TryGetComponent<UnityEngine.UI.Image>(out var track);
            track.color = Hex("D7D3B5"); Place(track.transform, 450, 1880, 270, 8);
            view.waveBar.sprite = track.sprite;
            view.waveBar.color = Hex("E1BD51"); Place(view.waveBar.transform, 450, 1880, 270, 8);

            Place(view.supportChargeLabel.transform, 352, 1212, 96, 44, 30);
            for (int i = 0; i < 3; i++) {Set(view.supportPips[i].transform, feedback[5]); Place(view.supportPips[i].transform, 432 + i * 43, 1212, 34, 34);}
            Place(view.clearText.transform, 450, 1343, 750, 52, 34);

            Set(stage.Find("GoldChip"), kit[1]); Place(stage.Find("GoldChip"), 450, 373, 192, 58);
            Set(stage.Find("GoldIcon"), kit[6]); Place(stage.Find("GoldIcon"), 394, 373, 38, 38);
            Place(view.supply.transform, 469, 374, 100, 48, 36);
            Set(stage.Find("PartsIcon"), kit[7]); Place(stage.Find("PartsIcon"), 770, 373, 34, 40);
            Place(view.record.transform, 817, 374, 60, 44, 32);
            Set(view.redrawButton.transform, kit[0]); Place(view.redrawButton.transform, 450, 265, 360, 115);
            Place(view.drawLabel.transform, 0, 0, 320, 70, 42);
            var price = view.redrawButton.transform.Find("PriceChip");
            price.TryGetComponent<UnityEngine.UI.Image>(out var priceImage); priceImage.color = Color.clear;
            Place(price, 0, -88, 140, 46);
            Set(price.Find("Coin"), kit[6]); Place(price.Find("Coin"), -28, 0, 30, 30);
            Place(view.rollCostLabel.transform, 22, 0, 85, 46, 32);
            Set(view.rotateButton.transform, kit[4]); Place(view.rotateButton.transform, 785, 285, 86, 86);
            Set(view.rescueButton.transform, kit[5]); Place(view.rescueButton.transform, 785, 170, 86, 86);
            Place(view.rescueCostLabel.transform, 16, -57, 65, 42, 30);
            Set(view.rescueCostIcon.transform, kit[7]); Place(view.rescueCostIcon.transform, -22, -57, 26, 30);
            Place(view.offers[0].background.transform, 148, 285, 240, 240);
            stage.Find("EvolutionDivider").TryGetComponent<UnityEngine.UI.Image>(out var divider);
            divider.color = new Color(.43f, .38f, .26f, .18f);
            Place(divider.transform, 450, 92, 690, 3);
            for (int i = 0; i < 3; i++)
            {
                Place(view.evolutionIcons[i].transform, 220 + i * 205, 58, 54, 54);
                Place(view.evolutionLevels[i].transform, 290 + i * 205, 58, 86, 44, 30);
            }

            view.modalSprites = panels;
            Set(view.modal, panels[0]);
            view.overlay.TryGetComponent<UnityEngine.UI.Image>(out var scrim);
            scrim.color = new Color(.13f, .19f, .14f, .64f);
            Place(view.overlayTitle.transform, 0, 300, 650, 70, 46);
            Set(view.primaryButton.transform, kit[0]); Place(view.primaryButton.transform, 0, -280, 330, 106);
            Place(view.primaryLabel.transform, 0, 0, 290, 62, 40);
            for (int i = 0; i < 3; i++)
            {
                var button = view.upgrades[i];
                Place(button.transform, 0, 175 - i * 180, 660, 164);
                Place(button.transform.Find("WeaponBadge"), -252, 0, 112, 112);
                Place(view.upgradeLabels[i].transform, -24, 0, 324, 136, 36);
                var action = button.transform.Find("SelectAction");
                Set(action, kit[2]); Place(action, 252, 0, 144, 60);
                Place(action.Find("SelectLabel"), 0, 1, 102, 44, 28);
                action.Find("SelectLabel").TryGetComponent<TMP_Text>(out view.upgradeActionLabels[i]);
            }

            var regular = Font("NanumSquareRoundB", "PetitBody");
            var bold = Font("NanumSquareRoundEB", "PetitHeading");
            foreach (var text in view.GetComponentsInChildren<TMP_Text>(true))
            {
                text.font = regular; text.fontSharedMaterial = regular.material;
                text.fontStyle = FontStyles.Normal; text.color = Hex("514A39");
                text.enableAutoSizing = false; text.characterSpacing = 0;
                text.extraPadding = true; text.UpdateMeshPadding();
            }
            foreach (var text in new[] {view.status, view.monsterCount, view.supply, view.drawLabel, view.primaryLabel, view.overlayTitle})
            {text.font = bold; text.fontSharedMaterial = bold.material;}
            foreach (var text in new[] {view.monsterCount, view.supply, view.drawLabel, view.primaryLabel}) text.color = Hex("FFFBE7");
            view.drawLabel.alignment = view.primaryLabel.alignment = view.overlayTitle.alignment = TextAlignmentOptions.Midline;
            view.overlayBody.fontSize = 34;
            foreach (var label in view.upgradeLabels) label.lineSpacing = 6;
            foreach (var label in view.upgradeActionLabels) {label.alignment = TextAlignmentOptions.Midline; label.font = bold; label.fontSharedMaterial = bold.material;}
            foreach (var enemy in world.enemies)
            {
                enemy.Label.font = bold; enemy.Label.fontSharedMaterial = bold.material;
                enemy.Label.fontSize = 2.3f; enemy.Label.color = Hex("514A39");
            }
            foreach (var button in new[] {view.redrawButton, view.primaryButton, view.rotateButton, view.rescueButton, view.pauseButton}.Concat(view.upgrades))
            {
                var colors = button.colors; colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1, .98f, .88f);
                colors.pressedColor = new Color(.87f, .85f, .76f);
                colors.disabledColor = new Color(.73f, .74f, .68f, .65f);
                colors.fadeDuration = .12f; button.colors = colors;
                button.TryGetComponent<DefenseButtonMotion>(out var motion);
                motion.feedbackTarget = button.targetGraphic.rectTransform;
                motion.stateContent = button.GetComponentsInChildren<TMP_Text>(true).Cast<UnityEngine.UI.Graphic>().ToArray();
                EditorUtility.SetDirty(motion);
            }
            EditorUtility.SetDirty(view); EditorUtility.SetDirty(world);
        }

        private static TMP_FontAsset Font(string source, string name)
        {
            string path = "Assets/00_Co-Op/05_Fonts/" + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (existing != null) return existing;
            var face = AssetDatabase.LoadAssetAtPath<UnityEngine.Font>("Assets/00_Co-Op/05_Fonts/" + source + ".ttf");
            var font = TMP_FontAsset.CreateFontAsset(face, 48, 5, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048);
            font.name = name;
            string code = string.Concat(Directory.GetFiles("Assets/00_Co-Op/02_Scripts/Defense", "*.cs", SearchOption.AllDirectories).Select(File.ReadAllText));
            string glyphs = new string(code.Where(c => (c >= 32 && c < 127) || (c >= 0xAC00 && c <= 0xD7A3)).Concat("·→×").Distinct().ToArray());
            if (!font.TryAddCharacters(glyphs, out var missing)) throw new InvalidOperationException("Font missing glyphs: " + missing);
            font.atlasPopulationMode = AtlasPopulationMode.Static;
            AssetDatabase.CreateAsset(font, path);
            foreach (var atlas in font.atlasTextures) AssetDatabase.AddObjectToAsset(atlas, font);
            AssetDatabase.AddObjectToAsset(font.material, font);
            EditorUtility.SetDirty(font); return font;
        }
        private static Color Hex(string hex) {ColorUtility.TryParseHtmlString("#" + hex, out var color); return color;}
        private static void Set(Transform transform, Sprite sprite)
        {
            transform.TryGetComponent<UnityEngine.UI.Image>(out var image);
            image.sprite = sprite; image.type = UnityEngine.UI.Image.Type.Simple; image.preserveAspect = true; image.color = Color.white;
        }
        private static void Place(Transform transform, float x, float y, float width, float height, float size = 0)
        {
            var rect = (RectTransform)transform; rect.anchorMin = rect.anchorMax = transform.parent.name == "Stage" ? Vector2.zero : Vector2.one * .5f;
            rect.pivot = Vector2.one * .5f; rect.anchoredPosition = new Vector2(x, y); rect.sizeDelta = new Vector2(width, height);
            if (size > 0 && transform.TryGetComponent<TMP_Text>(out var label)) label.fontSize = size;
        }
    }
}
