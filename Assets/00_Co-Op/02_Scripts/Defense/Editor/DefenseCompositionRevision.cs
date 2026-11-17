using System.Linq;
using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace OpenFrame.Editor
{
 public static class DefenseCompositionRevision
 {
  public static string Apply()
  {
   var v=Object.FindAnyObjectByType<DefenseView>();var w=v.world;var stage=v.transform.Find("Stage");
   w.worldCamera.backgroundColor=new Color(.115f,.16f,.105f);
   float[] target={0,130,400,450,940,965,1405,1800};
   for(int i=0;i<7;i++){var t=w.transform.Find("PaintedArena_"+i);t.TryGetComponent<SpriteRenderer>(out var sr);sr.size=new Vector2(i<3?6.2f:9,(target[i+1]-target[i])*.01f);sr.color=i<3?new Color(.76f,.8f,.86f):new Color(.92f,.93f,.97f);t.position=new Vector3(4.5f,(1800-(target[i]+target[i+1])*.5f)*.01f,0);}
   var backdrop=SpriteNode(w.transform,"CompositionBackdrop",w.lineSprite,-120);backdrop.transform.position=new Vector3(4.5f,9,0);backdrop.size=new Vector2(9,18);backdrop.color=new Color(.17f,.23f,.18f);
   for(int y=0;y<6;y++)for(int x=0;x<10;x++)
   {
    var t=w.transform.Find($"PartnerCell_{x}_{y}");t.position=new Vector3((235+(x+.5f)*43)*.01f,(1405+(y+.5f)*43)*.01f,0);t.TryGetComponent<SpriteRenderer>(out var sr);sr.size=Vector2.one*.39f;sr.color=new Color(.2f,.23f,.2f,.1f);
    t=w.transform.Find($"Cell_{x}_{y}");t.position=DefenseWorldView.Position(DefenseArena.CellPosition(x,y));t.TryGetComponent<SpriteRenderer>(out sr);sr.size=Vector2.one*.6f;sr.color=new Color(.28f,.2f,.17f,.11f);
    int i=y*10+x;w.blocks[i].transform.position=t.position;w.blocks[i].size=Vector2.one*.6f;w.weapons[i].transform.position=t.position;w.weapons[i].size=Vector2.one*.44f;
   }
   for(int y=0;y<2;y++)for(int x=0;x<8;x++){var t=w.transform.Find($"CenterCell_{x}_{y}");t.position=new Vector3((226+x*64)*.01f,(1060+y*80)*.01f,0);t.TryGetComponent<SpriteRenderer>(out var sr);sr.size=new Vector2(.58f,.68f);sr.color=new Color(.25f,.19f,.15f,.12f);}
   foreach(var ghost in w.ghosts)ghost.size=Vector2.one*.6f;
   w.boardInput.transform.position=new Vector3(4.5f,6.18f,0);w.boardInput.TryGetComponent<BoxCollider2D>(out var collider);collider.size=new Vector2(6.6f,3.96f);
   var shadow=ShadowSprite();w.monsterShadows=new SpriteRenderer[80];w.supportShadows=new SpriteRenderer[16];
   for(int i=0;i<80;i++){w.monsterShadows[i]=SpriteNode(w.transform,"MonsterShadow"+i,shadow,18);w.monsterShadows[i].color=new Color(.12f,.12f,.18f,.28f);w.monsterShadows[i].gameObject.SetActive(false);w.enemies[i].Label.fontSize=1.65f;}
   for(int i=0;i<16;i++){w.supportShadows[i]=SpriteNode(w.transform,"SupportShadow"+i,shadow,8);w.supportShadows[i].size=new Vector2(.52f,.18f);w.supportShadows[i].color=new Color(.12f,.12f,.18f,.32f);w.supportShadows[i].gameObject.SetActive(false);}
   Place(stage.Find("MatchHeader"),450,1750,840,82);Tint(stage.Find("MatchHeader"),new Color(.105f,.09f,.16f));
   Place(v.status.transform,245,1750,340,60);v.status.alignment=TextAlignmentOptions.Left;v.status.fontSize=36;v.status.color=new Color(1,.88f,.55f);
   var count=Node(stage,"MonsterCount");Place(count,665,1750,235,55);if(!count.TryGetComponent<TextMeshProUGUI>(out var countText))countText=count.gameObject.AddComponent<TextMeshProUGUI>();countText.font=v.status.font;countText.fontSize=26;countText.color=new Color(.92f,.91f,1);countText.alignment=TextAlignmentOptions.Right;countText.raycastTarget=false;v.monsterCount=countText;
   Place(v.pauseButton.transform,825,1750,52,52);v.pauseLabel.fontSize=22;Tint(v.pauseButton.transform,new Color(.25f,.21f,.34f));
   Place(stage.Find("WaveProgressBack"),450,1707,760,5);Place(v.waveBar.transform,450,1707,760,5);
   Place(stage.Find("PartnerLabel"),215,1684,300,25);stage.Find("PartnerLabel").TryGetComponent<TMP_Text>(out var partnerLabel);partnerLabel.text="동료 대기";partnerLabel.fontSize=18;partnerLabel.color=new Color(.73f,.76f,.75f);
   Place(v.progress.transform,666,1684,300,25);v.progress.fontSize=18;v.progress.alignment=TextAlignmentOptions.Right;v.progress.color=new Color(.85f,.87f,.85f);
   Place(stage.Find("FieldTitle"),245,850,250,26);stage.Find("FieldTitle").TryGetComponent<TMP_Text>(out var playerLabel);playerLabel.fontSize=20;playerLabel.alignment=TextAlignmentOptions.Left;
   Place(stage.Find("ControlDeck"),450,205,840,330);Tint(stage.Find("ControlDeck"),new Color(.13f,.11f,.2f));stage.Find("ControlDeck").TryGetComponent<UnityEngine.UI.Image>(out var deck);deck.sprite=v.offers[0].background.sprite;deck.type=UnityEngine.UI.Image.Type.Sliced;deck.pixelsPerUnitMultiplier=1;
   Place(v.offers[0].background.transform,255,240,220,170);v.offers[0].label.rectTransform.anchoredPosition=new Vector2(-104,-66);v.offers[0].label.fontSize=20;foreach(var cell in v.offers[0].cells)cell.rectTransform.sizeDelta=Vector2.one*34;
   Place(v.supply.transform,590,325,340,40);v.supply.alignment=TextAlignmentOptions.Center;v.supply.fontSize=32;v.supply.color=new Color(1,.87f,.45f);
   Place(v.redrawButton.transform,590,240,340,100);Tint(v.redrawButton.transform,new Color(.87f,.65f,.26f));v.drawLabel.color=new Color(.16f,.1f,.12f);v.drawLabel.fontSize=31;
   Place(v.rotateButton.transform,255,103,220,58);Tint(v.rotateButton.transform,new Color(.25f,.21f,.37f));Place(v.rescueButton.transform,590,103,340,58);Tint(v.rescueButton.transform,new Color(.25f,.21f,.37f));
   Place(v.record.transform,590,165,350,28);v.record.fontSize=18;v.record.alignment=TextAlignmentOptions.Center;v.record.color=new Color(.71f,.69f,.8f);
   Place(v.clearText.transform,450,1380,650,38);v.clearText.fontSize=27;
   Place(v.modal,0,0,790,660);v.modal.parent.Find("ModalAccent").gameObject.SetActive(false);Place(v.overlayTitle.transform,0,260,700,65);v.overlayTitle.fontSize=32;Place(v.overlayBody.transform,0,80,680,220);v.overlayBody.fontSize=24;Place(v.primaryButton.transform,0,-240,640,75);
   for(int i=0;i<3;i++)
   {
    var button=v.upgrades[i];Place(button.transform,0,115-i*125,680,105);Tint(button.transform,new Color(.23f,.19f,.33f));
    Place(v.upgradeLabels[i].transform,55,0,500,80);v.upgradeLabels[i].fontSize=23;v.upgradeLabels[i].alignment=TextAlignmentOptions.Left;
    var badge=Node(button.transform,"WeaponBadge");Place(badge,-267,0,75,75);
    if(!badge.TryGetComponent<UnityEngine.UI.Image>(out var badgeImage))badgeImage=badge.gameObject.AddComponent<UnityEngine.UI.Image>();
    badgeImage.sprite=v.offers[0].background.sprite;badgeImage.type=UnityEngine.UI.Image.Type.Sliced;badgeImage.color=DefenseView.ElementColor(i+1);badgeImage.raycastTarget=false;
    var icon=Node(badge,"WeaponIcon");Place(icon,0,0,58,58);if(!icon.TryGetComponent<UnityEngine.UI.Image>(out var iconImage))iconImage=icon.gameObject.AddComponent<UnityEngine.UI.Image>();
    iconImage.sprite=w.weaponSprites[i*3];iconImage.preserveAspect=true;iconImage.raycastTarget=false;
   }
   InstallBackground(w);
   if(System.IO.File.Exists("Assets/00_Co-Op/04_Images/DefenseArenaV6.png"))return DefenseReferenceUI.Apply();
   v.Refresh(new DefenseModel(new DefenseRules(),1),0);EditorUtility.SetDirty(v);EditorUtility.SetDirty(w);EditorSceneManager.MarkSceneDirty(v.gameObject.scene);EditorSceneManager.SaveScene(v.gameObject.scene);AssetDatabase.SaveAssets();return "Composition V5 saved";
  }
  private static void InstallBackground(DefenseWorldView world)
  {
   string path="Assets/00_Co-Op/04_Images/DefenseArenaV5.png";AssetDatabase.ImportAsset(path);
   DefenseArtAuthoring.Configure(path);var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
   float[] source={0,170,470,510,1020,1050,1470,1774};float[] target={0,130,400,450,940,965,1405,1800};
   var rects=new Rect[7];for(int i=0;i<7;i++)rects[i]=new Rect(0,texture.height*(1-source[i+1]/1774f),texture.width,texture.height*(source[i+1]-source[i])/1774f);
   var sprites=DefenseArtAuthoring.Slice(path,rects,"CompositionBand");
   for(int i=0;i<7;i++)
   {
    world.transform.Find("PaintedArena_"+i).gameObject.SetActive(false);
    var sr=SpriteNode(world.transform,"CompositionArt_"+i,sprites[i],-110);sr.size=new Vector2(9,(target[i+1]-target[i])*.01f);sr.color=new Color(.91f,.94f,.98f);sr.transform.position=new Vector3(4.5f,(1800-(target[i]+target[i+1])*.5f)*.01f,0);
   }
  }
  private static Transform Node(Transform p,string name){var t=p.Find(name);if(t!=null)return t;var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(p,false);return g.transform;}
  private static void Place(Transform t,float x,float y,float width,float height){var r=(RectTransform)t;r.anchorMin=r.anchorMax=t.parent.name=="Stage"?Vector2.zero:new Vector2(.5f,.5f);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(width,height);}
  private static void Tint(Transform t,Color color){t.TryGetComponent<UnityEngine.UI.Image>(out var image);image.color=color;}
  private static SpriteRenderer SpriteNode(Transform p,string name,Sprite sprite,int order){var t=p.Find(name);var g=t!=null?t.gameObject:new GameObject(name,typeof(SpriteRenderer));g.transform.SetParent(p,false);g.TryGetComponent<SpriteRenderer>(out var sr);sr.sprite=sprite;sr.drawMode=SpriteDrawMode.Sliced;sr.sortingOrder=order;return sr;}
  private static Sprite ShadowSprite()
  {
   string path="Assets/00_Co-Op/04_Images/UnitShadowV5.asset";var existing=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();if(existing!=null)return existing;
   var t=new Texture2D(64,64,TextureFormat.RGBA32,false);for(int y=0;y<64;y++)for(int x=0;x<64;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))/31.5f;t.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01((1-d)*5)));}t.Apply();AssetDatabase.CreateAsset(t,path);var sprite=Sprite.Create(t,new Rect(0,0,64,64),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);AssetDatabase.AddObjectToAsset(sprite,t);return sprite;
  }
 }
}




