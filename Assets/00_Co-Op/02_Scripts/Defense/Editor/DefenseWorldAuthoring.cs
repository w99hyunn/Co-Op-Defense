using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace OpenFrame.Editor
{
    public static class DefenseWorldAuthoring
    {
        private static Transform root;
        private static Sprite rounded;
        public static string Apply()
        {
            var view=UnityEngine.Object.FindAnyObjectByType<DefenseView>();
            if(view.world!=null)return "World already migrated";
            root=new GameObject("DefenseWorld").transform;
            var world=root.gameObject.AddComponent<DefenseWorldView>();view.world=world;
            var camera=UnityEngine.Object.FindAnyObjectByType<Camera>();
            if(camera==null){var cameraObject=new GameObject("WorldCamera",typeof(Camera));cameraObject.TryGetComponent<Camera>(out camera);}
            camera.name="WorldCamera";camera.orthographic=true;camera.transform.position=new Vector3(4.5f,9,-10);camera.transform.rotation=Quaternion.identity;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.nearClipPlane=.1f;camera.farClipPlane=100;
            if(!camera.TryGetComponent<Physics2DRaycaster>(out var raycaster))camera.gameObject.AddComponent<Physics2DRaycaster>();
            world.worldCamera=camera;world.FitCamera();
            var stage=(RectTransform)view.transform.Find("Stage");
            var original=view.unitSprites[0];
            rounded=LoadSprite("WorldRounded",()=>Sprite.Create(original.texture,original.rect,new Vector2(.5f,.5f),300,0,SpriteMeshType.FullRect,original.border));
            world.ringSprite=TextureSprite("WorldRing",96,96,(x,y)=>{float d=Vector2.Distance(new Vector2(x,y),new Vector2(47.5f,47.5f));return new Color(1,1,1,Mathf.Clamp01(4-Mathf.Abs(d-42)));});
            world.lineSprite=TextureSprite("WorldLine",8,8,(x,y)=>Color.white);
            world.monsterArt=Enumerable.Repeat(rounded,4).ToArray();world.supportArt=Enumerable.Repeat(rounded,3).ToArray();
            world.weaponSprites=new Sprite[9];
            for(int kind=0;kind<3;kind++)for(int level=1;level<=3;level++)
            {
                int k=kind,l=level;
                world.weaponSprites[kind*3+level-1]=TextureSprite($"WorldWeapon_{kind}_{level}",96,96,(x,y)=>WeaponPixel(k,l,x,y));
            }
            var terrain=stage.Find("IllustratedTerrain");terrain.TryGetComponent<DefenseTerrainGraphic>(out var graphic);
            var ground=BakeTerrain(graphic);var groundRenderer=Make("Terrain",new Vector2(450,900),new Vector2(900,1800),Color.white,-100,ground);groundRenderer.drawMode=SpriteDrawMode.Simple;
            string[] surfaces={"PartnerField","BoardBorder"};
            foreach(string name in surfaces)foreach(var im in stage.Find(name).GetComponentsInChildren<UnityEngine.UI.Image>())CopyImage(im,stage,0);
            foreach(var im in view.boardCells)CopyImage(im,stage,2);
            foreach(string name in new[]{"LoopBorder","LoopRoad","LoopInset","CenterField"})
            {
                var node=view.arena.Find(name);
                foreach(var im in node.GetComponentsInChildren<UnityEngine.UI.Image>())CopyImage(im,stage,1);
            }
            world.blocks=new SpriteRenderer[60];world.weapons=new SpriteRenderer[60];
            for(int i=0;i<60;i++)
            {
                var p=DefenseArena.CellPosition(i%10,i/10);
                world.blocks[i]=Make("Block_"+i,p,new Vector2(69,69),Color.white,10);
                world.weapons[i]=Make("Weapon_"+i,p,new Vector2(56,56),Color.white,12,world.weaponSprites[0]);
                world.blocks[i].gameObject.SetActive(false);world.weapons[i].gameObject.SetActive(false);
            }
            world.supports=new SpriteRenderer[16];world.supportWeapons=new SpriteRenderer[16];
            for(int i=0;i<16;i++)
            {
                var p=DefenseModel.SupportPosition(i);
                world.supports[i]=Make("Support_"+i,p,new Vector2(48,48),Color.white,10);
                world.supportWeapons[i]=Make("SupportWeapon_"+i,p,new Vector2(48,48),Color.white,12,world.weaponSprites[0]);
                world.supports[i].gameObject.SetActive(false);world.supportWeapons[i].gameObject.SetActive(false);
            }
            world.enemies=new DefenseWorldEnemy[80];
            for(int i=0;i<80;i++)
            {
                var body=Make("Monster_"+i,Vector2.zero,new Vector2(50,50),Color.white,20);
                var labelObject=new GameObject("Health",typeof(TextMeshPro));labelObject.TryGetComponent<TextMeshPro>(out var label);label.transform.SetParent(body.transform,false);
                label.font=view.status.font;label.fontSize=2.3f;label.alignment=TextAlignmentOptions.Center;label.color=Color.white;label.rectTransform.sizeDelta=new Vector2(.65f,.4f);
                label.TryGetComponent<MeshRenderer>(out var textRenderer);textRenderer.sortingOrder=21;
                world.enemies[i]=new DefenseWorldEnemy{Body=body,Label=label};body.gameObject.SetActive(false);
            }
            world.projectiles=Pool("Projectile",96,35,new Vector2(12,12));world.effects=Pool("Effect",128,40,new Vector2(12,12));world.ghosts=Pool("PlacementGhost",4,30,new Vector2(69,69));
            var input=new GameObject("PlayerFieldInput",typeof(BoxCollider2D),typeof(DefensePointer));input.transform.SetParent(root,false);input.transform.position=new Vector3(4.5f,5.662f,0);
            input.TryGetComponent<BoxCollider2D>(out var collider);collider.size=new Vector2(7.54f,4.524f);
            input.TryGetComponent<DefensePointer>(out world.boardInput);view.boardInput=world.boardInput;
            var archive=new GameObject("LegacyCanvasArt_Disabled");archive.SetActive(false);
            foreach(string name in new[]{"IllustratedTerrain","Background","PartnerField","BoardBorder","PlayerBoard","MonsterLane","CombatEffects","EvolutionEffects"})
                stage.Find(name).SetParent(archive.transform,true);
            foreach(var old in stage.Cast<Transform>().Where(t=>t.name=="PartnerTile").ToArray())old.SetParent(archive.transform,true);
            EditorUtility.SetDirty(view);EditorUtility.SetDirty(world);EditorSceneManager.MarkSceneDirty(view.gameObject.scene);EditorSceneManager.SaveScene(view.gameObject.scene);AssetDatabase.SaveAssets();
            if(System.IO.File.Exists("Assets/00_Co-Op/04_Images/DefenseCastV4.png")) DefenseArtAuthoring.Apply();
            return "Sprite world, orthographic camera, Physics2D input and pools saved; Canvas contains HUD only";
        }
        private static SpriteRenderer[] Pool(string name,int count,int order,Vector2 size)
        {var result=new SpriteRenderer[count];for(int i=0;i<count;i++){result[i]=Make(name+i,Vector2.zero,size,Color.white,order);result[i].gameObject.SetActive(false);}return result;}
        private static SpriteRenderer Make(string name,Vector2 position,Vector2 size,Color color,int order,Sprite sprite=null)
        {
            var go=new GameObject(name,typeof(SpriteRenderer));go.transform.SetParent(root,false);go.transform.position=DefenseWorldView.Position(position);
            go.TryGetComponent<SpriteRenderer>(out var renderer);renderer.sprite=sprite!=null ? sprite:rounded;renderer.drawMode=SpriteDrawMode.Sliced;renderer.size=size*.01f;renderer.color=color;renderer.sortingOrder=order;return renderer;
        }
        private static void CopyImage(UnityEngine.UI.Image image,RectTransform stage,int order)
        {
            var rect=image.rectTransform;var p=(Vector2)stage.InverseTransformPoint(rect.TransformPoint(rect.rect.center))+Vector2.Scale(stage.rect.size,stage.pivot);
            var size=Vector2.Scale(rect.rect.size,new Vector2(rect.lossyScale.x/stage.lossyScale.x,rect.lossyScale.y/stage.lossyScale.y));
            if(image.name.StartsWith("PartnerCell"))order=1;
            if(image.name=="LoopBorder")order=1;
            if(image.name=="LoopRoad")order=2;
            if(image.name=="LoopInset")order=3;
            if(image.name=="CenterField")order=4;
            if(image.name.StartsWith("CenterCell"))order=5;
            var sr=Make(image.name,p,size,image.color,order);
            if(image.name.StartsWith("Loop"))sr.sprite=LoadSprite("WorldRoadRounded",()=>Sprite.Create(rounded.texture,rounded.rect,new Vector2(.5f,.5f),30,0,SpriteMeshType.FullRect,rounded.border));
        }
        private static Sprite LoadSprite(string name,Func<Sprite> create)
        {string path="Assets/00_Co-Op/04_Images/"+name+".asset";var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(sprite!=null)return sprite;sprite=create();sprite.name=name;AssetDatabase.CreateAsset(sprite,path);return sprite;}
        private static Sprite TextureSprite(string name,int width,int height,Func<int,int,Color> pixel)
        {
            string path="Assets/00_Co-Op/04_Images/"+name+".asset";
            var existing=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();if(existing!=null)return existing;
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,false){name=name,filterMode=FilterMode.Bilinear};
            var colors=new Color[width*height];for(int y=0;y<height;y++)for(int x=0;x<width;x++)colors[y*width+x]=pixel(x,y);texture.SetPixels(colors);texture.Apply();
            AssetDatabase.CreateAsset(texture,path);var sprite=Sprite.Create(texture,new Rect(0,0,width,height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);sprite.name=name;AssetDatabase.AddObjectToAsset(sprite,texture);return sprite;
        }
        private static Color WeaponPixel(int kind,int level,int x,int y)
        {
            Vector2 p=new Vector2(x-47.5f,y-47.5f);float d=p.magnitude;bool ink=false,white=false;
            if(kind==0){ink=d<24 || Mathf.Abs(p.x)<12 && p.y>0 && p.y<40;white=Mathf.Abs(d-15)<4 || Mathf.Abs(p.x)<5 && p.y>20 && p.y<36;}
            else if(kind==1){for(int i=0;i<level;i++){float dx=p.x-(i-(level-1)*.5f)*18;ink|=Mathf.Abs(dx)<6 && Mathf.Abs(p.y)<33;white|=Mathf.Abs(dx)<2.5f && Mathf.Abs(p.y)<29;}}
            else {for(int i=0;i<3;i++){float a=i*Mathf.PI/3;var v=new Vector2(Mathf.Cos(a),Mathf.Sin(a));float along=Vector2.Dot(p,v),cross=Vector2.Dot(p,new Vector2(-v.y,v.x));ink|=Mathf.Abs(along)<31 && Mathf.Abs(cross)<4;white|=Mathf.Abs(along)<28 && Mathf.Abs(cross)<1.8f;}}
            for(int i=0;i<level;i++)if(Mathf.Abs(p.x-((i-(level-1)*.5f)*14))<4 && p.y<-35 && p.y>-40)white=true;
            return white ? Color.white:ink ? new Color(.16f,.12f,.22f):Color.clear;
        }
        private static Sprite BakeTerrain(DefenseTerrainGraphic graphic)
        {
            var mesh=new Mesh();using(var helper=new UnityEngine.UI.VertexHelper())
            {typeof(DefenseTerrainGraphic).GetMethod("OnPopulateMesh",BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly).Invoke(graphic,new object[]{helper});helper.FillMesh(mesh);}
            var vertices=mesh.vertices;var colors=mesh.colors;var triangles=mesh.triangles;var pixels=new Color[900*1800];
            for(int t=0;t<triangles.Length;t+=3)
            {
                var a=(Vector2)vertices[triangles[t]]+new Vector2(450,900);var b=(Vector2)vertices[triangles[t+1]]+new Vector2(450,900);var c=(Vector2)vertices[triangles[t+2]]+new Vector2(450,900);
                int minX=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.x,b.x,c.x)),0,899),maxX=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.x,b.x,c.x)),0,899);
                int minY=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.y,b.y,c.y)),0,1799),maxY=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.y,b.y,c.y)),0,1799);
                float area=Cross(b-a,c-a);if(Mathf.Abs(area)<.001f)continue;Color color=colors[triangles[t]];
                for(int y=minY;y<=maxY;y++)for(int x=minX;x<=maxX;x++)
                {var p=new Vector2(x+.5f,y+.5f);float u=Cross(b-a,p-a)/area,v=Cross(p-a,c-a)/area;if(u>=0 && v>=0 && u+v<=1){int i=y*900+x;pixels[i]=Color.Lerp(pixels[i],color,color.a);pixels[i].a=1;}}
            }
            UnityEngine.Object.DestroyImmediate(mesh);
            return TextureSprite("WorldTerrain",900,1800,(x,y)=>pixels[y*900+x]);
        }
        private static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
    }
}



