using System;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace OpenFrame.Editor
{
    public static class DefenseArtAuthoring
    {
        private static readonly string Folder="Assets/00_Co-Op/04_Images/";
        public static string Apply()
        {
            AssetDatabase.Refresh();
            string castPath=Folder+"DefenseCastV4.png",arenaPath=Folder+"DefenseArenaV4.png";
            var castImporter=Configure(castPath);var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(castPath);
            castImporter.GetSourceTextureWidthAndHeight(out int width,out int height);
            if(texture.width!=width || texture.height!=height)throw new InvalidOperationException("Sprite source dimensions differ");
            if(texture.GetPixel(0,0).a>.05f)throw new InvalidOperationException("Sprite atlas lacks transparency");
            var rectangles=new Rect[8];
            for(int i=0;i<8;i++)
            {
                int x0=i%4*width/4,y0=(1-i/4)*height/2,x1=x0+width/4,y1=y0+height/2;
                int left=x1,right=x0,bottom=y1,top=y0;
                for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(texture.GetPixel(x,y).a>.08f){left=Math.Min(left,x);right=Math.Max(right,x);bottom=Math.Min(bottom,y);top=Math.Max(top,y);}
                if(right<=left)throw new InvalidOperationException("Empty atlas cell "+i);
                rectangles[i]=Rect.MinMaxRect(Math.Max(x0,left-4),Math.Max(y0,bottom-4),Math.Min(x1,right+5),Math.Min(y1,top+5));
            }
            var cast=Slice(castPath,rectangles,"Cast");
            Configure(arenaPath);var arenaTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(arenaPath);
            // Sprite strips map the painted clearing boundaries onto existing gameplay coordinates.
            float[] source={0,128,633,650,974,994,1485,1774};float[] target={0,113,589,626,964,996,1472,1800};
            var bands=new Rect[source.Length-1];
            for(int i=0;i<bands.Length;i++)bands[i]=new Rect(0,arenaTexture.height*(1-source[i+1]/1774f),arenaTexture.width,arenaTexture.height*(source[i+1]-source[i])/1774f);
            var fieldSprites=Slice(arenaPath,bands,"ArenaBand");
            var view=UnityEngine.Object.FindAnyObjectByType<DefenseView>();var world=view.world;
            world.monsterArt=cast.Take(4).ToArray();world.supportArt=cast.Skip(4).Take(3).ToArray();
            foreach(var sr in world.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if(new[]{"Terrain","PartnerField","BoardBorder","LoopBorder","LoopRoad","LoopInset","CenterField"}.Contains(sr.name))sr.gameObject.SetActive(false);
                if(sr.name.StartsWith("PartnerCell") || sr.name.StartsWith("CenterCell") || sr.name.StartsWith("Cell"))sr.color=new Color(.36f,.24f,.16f,.13f);
            }
            for(int i=0;i<bands.Length;i++)
            {
                var found=world.transform.Find("PaintedArena_"+i);var go=found!=null ? found.gameObject:new GameObject("PaintedArena_"+i,typeof(SpriteRenderer));go.transform.SetParent(world.transform,false);
                go.TryGetComponent<SpriteRenderer>(out var sr);sr.sprite=fieldSprites[i];sr.drawMode=SpriteDrawMode.Sliced;sr.size=new Vector2(9,(target[i+1]-target[i])*.01f);sr.sortingOrder=-100;
                go.transform.position=new Vector3(4.5f,(1800-(target[i]+target[i+1])*.5f)*.01f,0);
            }
            var block=BlockSprite();view.unitSprites=new[]{block,block,block};
            foreach(var sr in world.blocks){sr.sprite=block;sr.size=new Vector2(.69f,.69f);}
            foreach(var unit in world.supports)unit.drawMode=SpriteDrawMode.Sliced;
            string materialPath=Folder+"CombatLabelV4.mat";
            var labelMaterial=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(labelMaterial==null){labelMaterial=new Material(view.status.font.material);AssetDatabase.CreateAsset(labelMaterial,materialPath);}
            labelMaterial.SetFloat(TMPro.ShaderUtilities.ID_OutlineWidth,.2f);labelMaterial.SetColor(TMPro.ShaderUtilities.ID_OutlineColor,new Color(.12f,.09f,.18f));
            foreach(var enemy in world.enemies)
            {
                enemy.Body.drawMode=SpriteDrawMode.Sliced;enemy.Label.fontSize=1.85f;enemy.Label.fontSharedMaterial=labelMaterial;
            }
            view.record.color=new Color(1,.94f,.8f);view.record.fontSharedMaterial=labelMaterial;
            foreach(string name in new[]{"PartnerLabel","FieldTitle"})
            {view.transform.Find("Stage").Find(name).TryGetComponent<TMPro.TMP_Text>(out var label);label.color=new Color(1,.94f,.8f);label.fontSharedMaterial=labelMaterial;}
            foreach(var offer in view.offers)foreach(var cell in offer.cells){cell.sprite=block;cell.type=UnityEngine.UI.Image.Type.Sliced;cell.pixelsPerUnitMultiplier=1;}
            EditorUtility.SetDirty(view);EditorUtility.SetDirty(world);EditorSceneManager.MarkSceneDirty(view.gameObject.scene);EditorSceneManager.SaveScene(view.gameObject.scene);AssetDatabase.SaveAssets();
            DefenseCompositionRevision.Apply();
            return $"Applied {cast.Length} alpha-trimmed sprites and {fieldSprites.Length} mapped terrain strips";
        }
        internal static TextureImporter Configure(string path)
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.isReadable=true;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);importer.SaveAndReimport();return importer;
        }
        internal static Sprite[] Slice(string path,Rect[] rectangles,string prefix,Vector4[] borders=null)
        {
            var importer=AssetImporter.GetAtPath(path);var factories=new SpriteDataProviderFactories();factories.Init();var provider=factories.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var edit=provider.GetDataProvider<ISpriteFrameEditCapability>();if(edit==null)throw new InvalidOperationException("Sprite capability unavailable");
            var capability=edit.GetEditCapability();foreach(var required in new[]{EEditCapability.CreateAndDeleteSprite,EEditCapability.EditSpriteRect,EEditCapability.EditSpriteName,EEditCapability.EditPivot})
                if(!capability.HasCapability(required))throw new InvalidOperationException("Sprite capability denied: "+required);
            var old=provider.GetSpriteRects();var rects=new SpriteRect[rectangles.Length];
            if(borders!=null&&!capability.HasCapability(EEditCapability.EditBorder))throw new InvalidOperationException("Sprite border capability denied");
            for(int i=0;i<rects.Length;i++)
            {
                string name=prefix+i;var previous=old.FirstOrDefault(r=>r.name==name);
                rects[i]=new SpriteRect{name=name,rect=rectangles[i],pivot=new Vector2(.5f,.5f),alignment=SpriteAlignment.Center,spriteID=previous!=null ? previous.spriteID:GUID.Generate()};
                if(borders!=null)rects[i].border=borders[i];
            }
            provider.SetSpriteRects(rects);var names=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if(names==null)throw new InvalidOperationException("Sprite ID provider unavailable");names.SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
            provider.Apply();importer.SaveAndReimport();var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();return rects.Select(r=>sprites.Single(s=>s.name==r.name)).ToArray();
        }
        private static Sprite BlockSprite()
        {
            string path=Folder+"BlockFaceV4.asset";var existing=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();if(existing!=null)return existing;
            var texture=new Texture2D(96,96,TextureFormat.RGBA32,false){name="BlockFaceV4"};
            for(int y=0;y<96;y++)for(int x=0;x<96;x++)
            {
                float dx=Mathf.Max(Mathf.Abs(x-47.5f)-35,0),dy=Mathf.Max(Mathf.Abs(y-47.5f)-35,0);float alpha=Mathf.Clamp01(12-Mathf.Sqrt(dx*dx+dy*dy));
                float tone=x<5||x>90||y<5||y>90 ? .26f:y<15 ? .58f:y>81 ? 1:x<12 ? .94f:.82f;
                texture.SetPixel(x,y,new Color(tone,tone,tone,alpha));
            }
            texture.Apply();AssetDatabase.CreateAsset(texture,path);var sprite=Sprite.Create(texture,new Rect(0,0,96,96),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(15,15,15,15));sprite.name="BlockFaceV4";AssetDatabase.AddObjectToAsset(sprite,texture);return sprite;
        }
    }
}


