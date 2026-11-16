using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OpenFrame.Editor
{
    // Measured from the selected RD2 gameplay/profile references in Docs/ArtReferences/Research50.
    public static class DefenseReferenceUI
    {
        private static string Folder => "Assets/00_Co-Op/04_Images/";
        private static Color Hex(string value) { ColorUtility.TryParseHtmlString("#"+value,out var c); return c; }
        public static string Apply()
        {
            var v=UnityEngine.Object.FindAnyObjectByType<DefenseView>();
            var w=v.world; var stage=v.transform.Find("Stage");
            InstallWeapons(w);
            w.monsterArt=Atlas("DefenseMonstersV6.png",4,1,"ReferenceMonster",new[]{0f,.23f,.463f,.706f,1f});
            var panel=Plate("ReferencePanel",Hex("211D31"),Hex("705393"),Hex("151020"),false);
            var button=Plate("ReferenceButton",Hex("403552"),Hex("8E70AD"),Hex("221A32"),true);
            var summon=Plate("ReferenceSummon",Hex("08DCA4"),Hex("7CFFE0"),Hex("086A59"),true);
            var chip=Plate("ReferenceChip",Hex("100E18"),Hex("100E18"),Hex("100E18"),false);
            var coin=Coin();
            var header=stage.Find("MatchHeader");Place(header,450,1733,285,112);SetImage(header,chip);
            Place(v.status.transform,450,1752,270,52);Style(v.status,38,Hex("FFCE27"));
            Place(v.monsterCount.transform,450,1717,230,30);Style(v.monsterCount,22,Hex("C8B6E3"));
            Place(stage.Find("WaveProgressBack"),450,1693,240,8);Tint(stage.Find("WaveProgressBack"),Hex("4B344F"));
            Place(v.waveBar.transform,450,1693,240,8);v.waveBar.color=Hex("30BDEB");
            Place(v.pauseButton.transform,824,1748,64,64);SetButton(v.pauseButton,button);v.pauseLabel.fontSize=25;
            Place(stage.Find("PartnerLabel"),150,1703,170,30);stage.Find("PartnerLabel").TryGetComponent<TMP_Text>(out var partner);Style(partner,20,Color.white);partner.text="동료 대기";
            Image(stage,"PartnerBadge",panel,150,1760,62,62);
            Text(stage,"PartnerBadgeGlyph",v.status,"P2",150,1764,60,40,25,Hex("C8B6E3"));
            Place(v.progress.transform,710,1749,130,42);Style(v.progress,25,Color.white);
            Place(stage.Find("FieldTitle"),183,855,160,30);stage.Find("FieldTitle").TryGetComponent<TMP_Text>(out var field);Style(field,20,Color.white);field.text="내 필드";
            Place(v.clearText.transform,450,1370,620,40);v.clearText.fontSize=28;
            // Open control cluster: the board stays visible around the primary action.
            stage.Find("ControlDeck").gameObject.SetActive(false);
            Place(v.offers[0].background.transform,215,240,200,190);SetImage(v.offers[0].background.transform,panel);
            Place(v.offers[0].label.transform,0,-66,180,34);Style(v.offers[0].label,20,Hex("C8B6E3"));
            Image(stage,"GoldChip",chip,507,359,225,43);Image(stage,"GoldIcon",coin,435,359,30,30);
            Place(v.supply.transform,520,360,150,38);Style(v.supply,29,Color.white);
            v.supply.transform.SetAsLastSibling();
            Place(v.redrawButton.transform,508,245,330,150);SetButton(v.redrawButton,summon);
            Place(v.drawLabel.transform,0,30,310,62);Style(v.drawLabel,39,Color.white);
            var price=Image(v.redrawButton.transform,"PriceChip",chip,0,-30,175,42);price.color=Hex("185844");
            Image(price.transform,"Coin",coin,-45,0,27,27);
            v.rollCostLabel=Text(price.transform,"Price",v.status,"10",20,1,100,35,27,Color.white);
            Place(v.rotateButton.transform,753,285,100,92);SetButton(v.rotateButton,button);
            var rotateLabel=v.rotateButton.GetComponentInChildren<TMP_Text>(true);Place(rotateLabel.transform,0,3,95,72);rotateLabel.text="회전";Style(rotateLabel,23,Color.white);
            Place(v.rescueButton.transform,753,174,100,92);SetButton(v.rescueButton,button);
            var rescueLabel=v.rescueButton.GetComponentInChildren<TMP_Text>(true);Place(rescueLabel.transform,0,13,95,60);rescueLabel.text="정리";Style(rescueLabel,22,Color.white);
            Text(v.rescueButton.transform,"PartsCost",v.status,"4",0,-23,80,30,18,Hex("C8B6E3"));
            Image(stage,"PartsChip",chip,508,136,155,35);
            Text(stage,"PartsCaption",v.status,"부품",476,137,70,25,17,Hex("C8B6E3"));
            Place(v.record.transform,548,137,65,28);Style(v.record,21,Color.white);
            v.record.transform.SetAsLastSibling();
            Image(stage,"WeaponStrip",panel,450,49,800,68);
            Text(stage,"WeaponCaption",v.status,"무기 진화",160,51,180,36,22,Color.white);
            v.evolutionLevels=new TMP_Text[3];
            v.evolutionIcons=new UnityEngine.UI.Image[3];
            for(int i=0;i<3;i++)
            {
                v.evolutionIcons[i]=Image(stage,"EvolutionIcon"+i,w.weaponSprites[i*3],352+i*170,51,48,48);v.evolutionIcons[i].preserveAspect=true;
                v.evolutionLevels[i]=Text(stage,"EvolutionLevel"+i,v.status,"Lv.1",415+i*170,51,85,35,24,Color.white);
            }
            Image(stage,"SupportChip",chip,450,1243,172,34);
            v.supportChargeLabel=Text(stage,"SupportCharge",v.status,"지원 0 / 3",450,1244,165,28,19,Hex("C8B6E3"));
            // Same panel language for pause, evolution and results; all text stays editable.
            var modal=v.overlay.transform.Find("Modal");Place(modal,0,0,730,640);SetImage(modal,panel);
            var accent=v.overlay.transform.Find("ModalAccent");accent.gameObject.SetActive(true);Place(accent,0,265,720,100);SetImage(accent,button);
            Place(v.overlayTitle.transform,0,269,650,65);Style(v.overlayTitle,35,Color.white);
            Place(v.overlayBody.transform,0,15,650,340);Style(v.overlayBody,24,Hex("C8B6E3"));
            Place(v.primaryButton.transform,0,-230,360,90);SetButton(v.primaryButton,summon);Style(v.primaryLabel,30,Color.white);
            v.upgradeIcons=new UnityEngine.UI.Image[3];
            for(int i=0;i<3;i++)
            {
                Place(v.upgrades[i].transform,0,128-i*150,650,128);SetButton(v.upgrades[i],button);
                Place(v.upgradeLabels[i].transform,75,5,440,95);Style(v.upgradeLabels[i],24,Color.white);v.upgradeLabels[i].alignment=TextAlignmentOptions.Left;
                var badge=v.upgrades[i].transform.Find("WeaponBadge");Place(badge,-235,5,100,100);SetImage(badge,panel);
                badge.TryGetComponent<UnityEngine.UI.Image>(out var badgeImage);badgeImage.color=Color.white;
                var icon=badge.Find("WeaponIcon");Place(icon,0,0,86,86);icon.TryGetComponent<UnityEngine.UI.Image>(out v.upgradeIcons[i]);
                v.upgradeIcons[i].sprite=w.weaponSprites[i*3];v.upgradeIcons[i].preserveAspect=true;
            }
            InstallTerrain(w);
            DefenseGeneratedUI.Apply(v);
            v.overlay.transform.SetAsLastSibling();
            v.Refresh(new DefenseModel(new DefenseRules(),1),0);
            EditorUtility.SetDirty(v);EditorUtility.SetDirty(w);EditorSceneManager.MarkSceneDirty(v.gameObject.scene);EditorSceneManager.SaveScene(v.gameObject.scene);AssetDatabase.SaveAssets();
            return "Reference UI, HUD, modals and sprite terrain saved";
        }
        private static void InstallTerrain(DefenseWorldView w)
        {
            string path=Folder+"DefenseArenaV13.png";AssetDatabase.ImportAsset(path);DefenseArtAuthoring.Configure(path);var t=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var sprite=DefenseArtAuthoring.Slice(path,new[]{new Rect(0,0,t.width,t.height)},"SeamlessArena")[0];
            foreach(var sr in w.GetComponentsInChildren<SpriteRenderer>(true))if(sr.name.StartsWith("CompositionArt_")||sr.name.StartsWith("PaintedArena_")||sr.name.StartsWith("ReferenceTerrain"))sr.gameObject.SetActive(false);
            var found=w.transform.Find("SeamlessArena");var go=found!=null?found.gameObject:new GameObject("SeamlessArena",typeof(SpriteRenderer));go.transform.SetParent(w.transform,false);go.TryGetComponent<SpriteRenderer>(out var terrain);
            terrain.sprite=sprite;terrain.drawMode=SpriteDrawMode.Sliced;terrain.size=new Vector2(20f*t.width/t.height,20f);terrain.transform.localScale=Vector3.one;terrain.color=Color.white;terrain.sortingOrder=-110;go.transform.position=new Vector3(4.5f,10,0);
            w.worldCamera.backgroundColor=Hex("DDDCD0");
            var cell=Plate("ReferenceGroundInset",Hex("DCAF7C"),Hex("D0A16F"),Hex("D0A16F"),false);
            foreach(var sr in w.GetComponentsInChildren<SpriteRenderer>(true))
                if(sr.name.StartsWith("PartnerCell_")||sr.name.StartsWith("CenterCell_")||sr.name.StartsWith("Cell_")){sr.sprite=cell;sr.color=new Color(1,1,1,.54f);}
        }
        private static void InstallWeapons(DefenseWorldView w)
        {
            var cast=Atlas("DefenseWeaponsV6.png",4,3,"ReferenceWeapon",new[]{0f,.24f,.46f,.725f,1f},new[]{0f,1f/3f,.615f,1f});
            w.weaponSprites=new Sprite[9];w.supportArt=new Sprite[3];
            for(int row=0;row<3;row++){for(int col=0;col<3;col++)w.weaponSprites[row*3+col]=cast[row*4+col];w.supportArt[row]=cast[row*4+3];}
            foreach(var weapon in w.weapons)weapon.size=Vector2.one*.52f;
        }
        internal static Sprite[] Atlas(string file,int columns,int rows,string prefix,float[] edges=null,float[] rowEdges=null,Vector4[] borders=null)
        {
            string path=Folder+file;AssetDatabase.ImportAsset(path);DefenseArtAuthoring.Configure(path);var t=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(t.GetPixel(0,0).a>.05f)throw new InvalidOperationException("Atlas requires actual transparency");
            var rects=new Rect[columns*rows];
            for(int i=0;i<rects.Length;i++)
            {
                int x0=i%columns*t.width/columns,y0=(rows-1-i/columns)*t.height/rows,x1=x0+t.width/columns,y1=y0+t.height/rows;
                if(edges!=null){x0=Mathf.RoundToInt(edges[i%columns]*t.width);x1=Mathf.RoundToInt(edges[i%columns+1]*t.width);}
                if(rowEdges!=null){y0=Mathf.RoundToInt((1-rowEdges[i/columns+1])*t.height);y1=Mathf.RoundToInt((1-rowEdges[i/columns])*t.height);}
                int l=x1,r=x0,b=y1,top=y0;
                for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(t.GetPixel(x,y).a>.18f){l=Math.Min(l,x);r=Math.Max(r,x);b=Math.Min(b,y);top=Math.Max(top,y);}
                rects[i]=Rect.MinMaxRect(Math.Max(x0,l-2),Math.Max(y0,b-2),Math.Min(x1,r+3),Math.Min(y1,top+3));
            }
            return DefenseArtAuthoring.Slice(path,rects,prefix,borders);
        }
        private static void Style(TMP_Text t,float size,Color color){t.fontSize=size;t.color=color;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;}
        private static Transform Node(Transform p,string name){var t=p.Find(name);if(t!=null)return t;var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(p,false);return g.transform;}
        private static void Place(Transform t,float x,float y,float width,float height){var r=(RectTransform)t;r.anchorMin=r.anchorMax=t.parent.name=="Stage"?Vector2.zero:Vector2.one*.5f;r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(width,height);}
        private static void Tint(Transform t,Color c){t.TryGetComponent<UnityEngine.UI.Image>(out var image);image.color=c;}
        private static void SetImage(Transform t,Sprite sprite){t.TryGetComponent<UnityEngine.UI.Image>(out var image);image.sprite=sprite;image.type=UnityEngine.UI.Image.Type.Sliced;image.color=Color.white;}
        private static void SetButton(UnityEngine.UI.Button button,Sprite sprite){SetImage(button.transform,sprite);var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1.13f,1.13f,1.13f);colors.pressedColor=new Color(.74f,.85f,.84f);colors.disabledColor=new Color(.4f,.4f,.45f,.8f);colors.fadeDuration=.08f;button.colors=colors;}
        private static UnityEngine.UI.Image Image(Transform p,string name,Sprite sprite,float x,float y,float width,float height){var t=Node(p,name);Place(t,x,y,width,height);if(!t.TryGetComponent<UnityEngine.UI.Image>(out var image))image=t.gameObject.AddComponent<UnityEngine.UI.Image>();image.sprite=sprite;image.type=UnityEngine.UI.Image.Type.Sliced;image.color=Color.white;image.raycastTarget=false;return image;}
        private static TMP_Text Text(Transform p,string name,TMP_Text source,string value,float x,float y,float width,float height,float size,Color color){var t=Node(p,name);Place(t,x,y,width,height);if(!t.TryGetComponent<TextMeshProUGUI>(out var text))text=t.gameObject.AddComponent<TextMeshProUGUI>();text.font=source.font;text.text=value;Style(text,size,color);return text;}
        private static Sprite Plate(string name,Color face,Color edge,Color shadow,bool raised)
        {
            string path=Folder+name+".asset";var existing=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();if(existing!=null)return existing;
            var t=new Texture2D(128,128,TextureFormat.RGBA32,false){name=name,filterMode=FilterMode.Bilinear};
            for(int y=0;y<128;y++)for(int x=0;x<128;x++)
            {
                float d=RoundDistance(x,y,2,2,124,124,12);Color c=shadow;c.a=Mathf.Clamp01(1-d);
                float f=RoundDistance(x,y,2,raised?9:2,124,raised?117:124,10);
                if(f<0){c=f>-2.2f?edge:face;if(raised&&y>115&&f<-3)c=Color.Lerp(face,Color.white,.16f);}
                t.SetPixel(x,y,c);
            }
            t.Apply();AssetDatabase.CreateAsset(t,path);var s=Sprite.Create(t,new Rect(0,0,128,128),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(16,23,16,16));s.name=name;AssetDatabase.AddObjectToAsset(s,t);return s;
        }
        private static float RoundDistance(float x,float y,float left,float bottom,float width,float height,float radius){var q=new Vector2(Mathf.Abs(x-left-width*.5f)-width*.5f+radius,Mathf.Abs(y-bottom-height*.5f)-height*.5f+radius);return new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude+Mathf.Min(Mathf.Max(q.x,q.y),0)-radius;}
        private static Sprite Coin()
        {
            string path=Folder+"ReferenceCoin.asset";var existing=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();if(existing!=null)return existing;
            var t=new Texture2D(64,64,TextureFormat.RGBA32,false);for(int y=0;y<64;y++)for(int x=0;x<64;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f));Color c=d>28?Hex("9C5E0B"):d>24?Hex("FFE879"):d>21?Hex("D89116"):Hex("FFCA24");if(d<20&&y>34)c=Hex("FFDF45");c.a=Mathf.Clamp01(31-d);t.SetPixel(x,y,c);}t.Apply();AssetDatabase.CreateAsset(t,path);var s=Sprite.Create(t,new Rect(0,0,64,64),Vector2.one*.5f,100);s.name="ReferenceCoin";AssetDatabase.AddObjectToAsset(s,t);return s;
        }
    }
}
