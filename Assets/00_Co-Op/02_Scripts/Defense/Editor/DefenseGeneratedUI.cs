using System.Linq;
using TMPro;
using UnityEngine;

namespace OpenFrame.Editor
{
    public static class DefenseGeneratedUI
    {
        public static void Apply(DefenseView view)
        {
            var icons=DefenseReferenceUI.Atlas("UIControlsV11.png",3,2,"Control",new[]{0f,1f/3f,.61f,1f});
            var frames=DefenseReferenceUI.Atlas("UIFramesV11.png",2,1,"Frame",new[]{0f,.38f,1f});
            string accentPath="Assets/00_Co-Op/04_Images/UIAccentsV9.png";
            DefenseArtAuthoring.Configure(accentPath);
            var accents=DefenseArtAuthoring.Slice(accentPath,new[]{new Rect(118,110,490,493),new Rect(810,350,550,16),new Rect(1530,350,550,16),new Rect(810,358,550,5)},"Accent");
            var details=DefenseReferenceUI.Atlas("UIDetailsV7.png",3,2,"Detail");
            var summon=DefenseReferenceUI.Atlas("UISummonV11.png",1,1,"Summon")[0];
            var stage=view.transform.Find("Stage");
            foreach(var name in new[]{"MatchHeader","PartnerBadge","PartnerBadgeGlyph","PartnerLabel","FieldTitle","WeaponCaption","WeaponStrip","GoldChip","PartsChip","PartsCaption","SupportChip"})
                stage.Find(name).gameObject.SetActive(false);
            Place(view.status.transform,150,1960,230,46,32);
            var danger=stage.Find("MatchHeader");danger.gameObject.SetActive(true);Set(danger,icons[5]);Place(danger,450,1944,330,100);
            Place(view.monsterCount.transform,450,1944,280,66,46);
            Place(view.progress.transform,150,1915,230,46,32);
            Set(view.pauseButton.transform,icons[0]);Place(view.pauseButton.transform,823,1940,72,72);view.pauseLabel.gameObject.SetActive(false);
            Set(stage.Find("WaveProgressBack"),accents[1]);
            stage.Find("WaveProgressBack").TryGetComponent<UnityEngine.UI.Image>(out var track);track.preserveAspect=true;
            Place(track.transform,450,1882,340,10);
            view.waveBar.sprite=accents[2];view.waveBar.color=Color.white;view.waveBar.type=UnityEngine.UI.Image.Type.Filled;view.waveBar.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;view.waveBar.preserveAspect=true;
            Place(view.waveBar.transform,450,1882,340,10);
            Place(view.supportChargeLabel.transform,395,1220,110,48,32);
            view.supportPips=new UnityEngine.UI.Image[3];
            for(int i=0;i<3;i++){var pip=Node(stage,"SupportPip"+i);Set(pip,accents[0]);Place(pip,465+i*28,1220,18,18);pip.TryGetComponent<UnityEngine.UI.Image>(out view.supportPips[i]);}
            Place(view.clearText.transform,450,1370,750,58,36);

            // The preview remains a transparent raycast target for dragging a held block.
            Place(view.offers[0].background.transform,150,240,190,220);
            view.offers[0].background.color=Color.clear;
            var heldBase=Node(view.offers[0].background.transform,"HeldBase");Set(heldBase,view.world.supportShadows[0].sprite);Place(heldBase,0,-74,170,40);heldBase.SetAsFirstSibling();heldBase.TryGetComponent<UnityEngine.UI.Image>(out var heldImage);heldImage.color=new Color(1,1,1,.18f);
            view.offers[0].label.rectTransform.sizeDelta=new Vector2(180,52);
            var gold=stage.Find("GoldIcon");gold.gameObject.SetActive(true);Set(gold,icons[3]);Place(gold,386,350,38,38);
            var wallet=stage.Find("GoldChip");wallet.gameObject.SetActive(true);Set(wallet,icons[5]);Place(wallet,450,350,220,70);wallet.SetSiblingIndex(gold.GetSiblingIndex());gold.SetSiblingIndex(wallet.GetSiblingIndex()+1);
            Place(view.supply.transform,470,350,140,52,38);
            var parts=Node(stage,"PartsIcon");Set(parts,icons[4]);Place(parts,765,350,36,36);
            Place(view.record.transform,818,350,65,48,34);
            Set(view.redrawButton.transform,summon);Place(view.redrawButton.transform,450,240,340,142);
            Place(view.drawLabel.transform,0,22,320,64,44);
            var price=view.redrawButton.transform.Find("PriceChip");price.TryGetComponent<UnityEngine.UI.Image>(out var priceImage);Set(price,icons[5]);
            Place(price,0,-29,145,44);
            var coin=price.Find("Coin");coin.gameObject.SetActive(true);Set(coin,icons[3]);Place(coin,-32,0,28,28);
            coin.TryGetComponent<UnityEngine.UI.Image>(out var coinImage);coinImage.color=Color.white;
            Place(view.rollCostLabel.transform,18,0,90,42,32);
            Set(view.rotateButton.transform,icons[1]);Place(view.rotateButton.transform,710,240,82,82);
            foreach(var label in view.rotateButton.GetComponentsInChildren<TMP_Text>(true))label.gameObject.SetActive(false);
            Set(view.rescueButton.transform,icons[2]);Place(view.rescueButton.transform,815,240,82,82);
            foreach(var label in view.rescueButton.GetComponentsInChildren<TMP_Text>(true))label.gameObject.SetActive(false);
            var cost=view.rescueButton.transform.Find("PartsCost");cost.gameObject.SetActive(true);Place(cost,16,-62,60,40,32);
            var costIcon=Node(view.rescueButton.transform,"CostIcon");Set(costIcon,icons[4]);Place(costIcon,-20,-62,28,28);
            cost.TryGetComponent<TMP_Text>(out var costLabel);costLabel.text="4";
            for(int i=0;i<3;i++)
            {
                var oldCard=stage.Find("WeaponCard"+i);if(oldCard!=null)oldCard.gameObject.SetActive(false);
                view.evolutionIcons[i].type=UnityEngine.UI.Image.Type.Simple;Place(view.evolutionIcons[i].transform,160+i*250,53,64,64);
                Place(view.evolutionLevels[i].transform,240+i*250,53,100,48,32);
            }

            var divider=Node(stage,"EvolutionDivider");Set(divider,accents[3]);Place(divider,450,108,770,7);
            view.modalSprites=frames;Set(view.overlay.transform.Find("Modal"),frames[0]);
            view.modal=(RectTransform)view.overlay.transform.Find("Modal");
            view.modal.TryGetComponent<UnityEngine.UI.Image>(out var modalImage);modalImage.type=UnityEngine.UI.Image.Type.Simple;modalImage.preserveAspect=true;
            view.overlay.TryGetComponent<UnityEngine.UI.Image>(out var scrim);scrim.color=new Color(.16f,.12f,.11f,.6f);
            Place(view.modal,0,0,760,760);view.overlay.transform.Find("ModalAccent").gameObject.SetActive(false);
            Place(view.overlayTitle.transform,0,330,650,70,42);
            Set(view.primaryButton.transform,summon);Place(view.primaryButton.transform,0,-280,330,135);Place(view.primaryLabel.transform,0,5,280,70,40);
            for(int i=0;i<3;i++)
            {
                view.upgrades[i].transform.TryGetComponent<UnityEngine.UI.Image>(out var row);row.color=Color.clear;
                Place(view.upgrades[i].transform,0,160-i*180,650,164);
                var badge=view.upgrades[i].transform.Find("WeaponBadge");badge.TryGetComponent<UnityEngine.UI.Image>(out var image);image.enabled=false;
            Place(badge,-250,5,112,112);Place(view.upgradeIcons[i].transform,0,0,108,108);
                Place(view.upgradeLabels[i].transform,-12,5,350,130,36);
                var action=Node(view.upgrades[i].transform,"SelectAction");Set(action,summon);Place(action,260,5,110,48);
                action.TryGetComponent<UnityEngine.UI.Image>(out var actionImage);view.upgrades[i].targetGraphic=actionImage;
                var select=Label(action,"SelectLabel",view.drawLabel);select.text="선택";Place(select.transform,0,1,100,44,32);
            }
            view.unitSprites=details.Take(3).ToArray();view.world.blockArt=view.unitSprites;
            foreach(var offer in view.offers)
            {
                offer.weapons=new UnityEngine.UI.Image[offer.cells.Length];
                for(int i=0;i<offer.cells.Length;i++)
                {
                    var cell=offer.cells[i];cell.type=UnityEngine.UI.Image.Type.Simple;cell.preserveAspect=true;
                    var art=Node(cell.transform,"WeaponPreview");Set(art,view.world.weaponSprites[0]);Place(art,0,0,30,30);art.TryGetComponent<UnityEngine.UI.Image>(out offer.weapons[i]);
                }
            }
            foreach(var button in new[]{view.redrawButton,view.primaryButton,view.rotateButton,view.rescueButton,view.pauseButton}.Concat(view.upgrades))
            {
                var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1,.94f,.9f);colors.pressedColor=new Color(.82f,.72f,.66f);colors.disabledColor=new Color(.62f,.6f,.58f);colors.fadeDuration=.08f;button.colors=colors;
            }
            view.world.transform.Find("CompositionBackdrop").gameObject.SetActive(false);
            var shadow=DefenseReferenceUI.Atlas("UnitShadowV11.png",1,1,"UnitShadow")[0];
            foreach(var renderer in view.world.monsterShadows.Concat(view.world.supportShadows))
            {
                renderer.sprite=shadow;DefenseWorldView.FitArtwork(renderer,new Vector2(.52f,.18f));
            }
            view.world.effectLines=new LineRenderer[view.world.effects.Length];
            for(int i=0;i<view.world.effects.Length;i++)
            {
                var effect=view.world.effects[i];
                if(!effect.TryGetComponent<LineRenderer>(out var line))line=effect.gameObject.AddComponent<LineRenderer>();
                line.sharedMaterial=effect.sharedMaterial;line.useWorldSpace=true;line.positionCount=2;
                line.sortingOrder=effect.sortingOrder;line.enabled=false;line.numCapVertices=2;
                view.world.effectLines[i]=line;
            }
            ApplyEqualFields(view);
            foreach(var cell in view.world.GetComponentsInChildren<SpriteRenderer>(true))
                if(cell.name.StartsWith("PartnerCell_")||cell.name.StartsWith("CenterCell_")||cell.name.StartsWith("Cell_")){cell.sprite=accents[0];cell.color=new Color(.55f,.48f,.43f,cell.name.StartsWith("PartnerCell_") ? .10f : cell.name.StartsWith("CenterCell_") ? .15f : .23f);DefenseWorldView.FitArtwork(cell,cell.size);}
            view.overlayBody.fontSize=34;
            foreach(var enemy in view.world.enemies)enemy.Label.fontSize=3;
            ApplyTypography(view);
            view.rollCostLabel.color=Color.white;view.supply.color=Color.white;view.monsterCount.color=Color.white;
            foreach(var button in view.upgrades){button.transform.Find("SelectAction/SelectLabel").TryGetComponent<TMP_Text>(out var label);label.color=Color.white;label.fontSharedMaterial=view.drawLabel.fontSharedMaterial;label.alignment=TextAlignmentOptions.Midline;}
        }
        private static void ApplyEqualFields(DefenseView view)
        {
            var stage=(RectTransform)view.transform.Find("Stage");stage.sizeDelta=DefenseArena.StageSize;
            var overlay=(RectTransform)view.overlay.transform;overlay.anchorMin=Vector2.zero;overlay.anchorMax=Vector2.one;overlay.offsetMin=Vector2.zero;overlay.offsetMax=Vector2.zero;
            var canvas=view.GetComponentInParent<Canvas>();canvas.TryGetComponent<UnityEngine.UI.CanvasScaler>(out var scaler);scaler.referenceResolution=DefenseArena.StageSize;
            view.world.worldCamera.transform.position=new Vector3(4.5f,10,-10);view.world.FitCamera();
            for(int y=0;y<6;y++)for(int x=0;x<10;x++)
            {
                var upper=view.world.transform.Find($"PartnerCell_{x}_{y}");var lower=view.world.transform.Find($"Cell_{x}_{y}");
                upper.TryGetComponent<SpriteRenderer>(out var top);lower.TryGetComponent<SpriteRenderer>(out var bottom);
                upper.position=DefenseWorldView.Position(DefenseArena.PartnerCellPosition(x,y));top.size=bottom.size;top.color=bottom.color;
            }
        }
        private static void ApplyTypography(DefenseView view)
        {
            var panel=TextMaterial(view.status.font,"DefenseTextPanel",0);
            var action=TextMaterial(view.status.font,"DefenseTextAction",.08f);
            foreach(var label in view.GetComponentsInChildren<TMP_Text>(true))
            {
                label.fontSharedMaterial=panel;label.extraPadding=true;label.raycastTarget=false;label.color=new Color(.21f,.18f,.18f,1);
            }
            view.drawLabel.fontSharedMaterial=action;view.primaryLabel.fontSharedMaterial=action;
            view.drawLabel.alignment=TextAlignmentOptions.Midline;view.primaryLabel.alignment=TextAlignmentOptions.Midline;
            view.status.color=new Color(.86f,.24f,.17f,1);
            view.drawLabel.color=Color.white;view.primaryLabel.color=Color.white;view.overlayTitle.color=new Color(.21f,.18f,.18f,1);
            foreach(var label in view.GetComponentsInChildren<TMP_Text>(true))label.UpdateMeshPadding();
        }
        private static TMP_Text Label(Transform parent,string name,TMP_Text template)
        {
            var found=parent.Find(name);if(found!=null){found.TryGetComponent<TMP_Text>(out var existing);return existing;}
            var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);go.TryGetComponent<TextMeshProUGUI>(out var label);label.font=template.font;label.raycastTarget=false;label.alignment=TextAlignmentOptions.Center;return label;
        }
        private static Material TextMaterial(TMP_FontAsset font,string name,float width)
        {
            string path="Assets/00_Co-Op/04_Images/"+name+".mat";
            var material=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(font.material){name=name};UnityEditor.AssetDatabase.CreateAsset(material,path);}
            material.SetColor(ShaderUtilities.ID_OutlineColor,new Color(.065f,.075f,.12f,1));material.SetFloat(ShaderUtilities.ID_OutlineWidth,width);material.EnableKeyword("OUTLINE_ON");UnityEditor.EditorUtility.SetDirty(material);return material;
        }
        private static Transform Node(Transform parent,string name){var found=parent.Find(name);if(found!=null)return found;var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));go.transform.SetParent(parent,false);go.TryGetComponent<UnityEngine.UI.Image>(out var image);image.raycastTarget=false;return go.transform;}
        private static void Set(Transform node,Sprite sprite){node.TryGetComponent<UnityEngine.UI.Image>(out var image);image.sprite=sprite;image.type=UnityEngine.UI.Image.Type.Simple;image.preserveAspect=true;image.color=Color.white;}
        private static void Place(Transform node,float x,float y,float width,float height,float fontSize=0)
        {
            var rect=(RectTransform)node;rect.anchorMin=rect.anchorMax=node.parent.name=="Stage"?Vector2.zero:Vector2.one*.5f;rect.pivot=Vector2.one*.5f;rect.anchoredPosition=new Vector2(x,y);rect.sizeDelta=new Vector2(width,height);
            if(fontSize>0&&node.TryGetComponent<TMP_Text>(out var label))label.fontSize=fontSize;
        }
    }
}
