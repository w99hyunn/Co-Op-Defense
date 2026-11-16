using System;
using TMPro;
using UnityEngine;
namespace OpenFrame
{
    [Serializable] public sealed class DefenseWorldEnemy
    {
        public SpriteRenderer Body;
        public TextMeshPro Label;
    }
    public sealed class DefenseWorldView : MonoBehaviour
    {
        public Camera worldCamera;
        public DefensePointer boardInput;
        public SpriteRenderer[] blocks, weapons, supports, supportWeapons, ghosts, projectiles, effects;
        public DefenseWorldEnemy[] enemies;
        public Sprite[] weaponSprites;
        public Sprite[] monsterArt, supportArt;
        public Sprite[] blockArt;
        public Sprite ringSprite, lineSprite;
        public SpriteRenderer[] monsterShadows, supportShadows;
        public LineRenderer[] effectLines;
        private readonly float[] shotLife=new float[96],effectLife=new float[128],effectRadius=new float[128];
        private readonly Vector2[] shotFrom=new Vector2[96],shotTo=new Vector2[96];
        private readonly bool[] rings=new bool[128];
        private int shotIndex,effectIndex;
        public static Vector3 Position(Vector2 logical)=>new Vector3(logical.x*.01f,logical.y*.01f,0);
        private void LateUpdate()=>FitCamera();
        public void FitCamera(){worldCamera.orthographicSize=Mathf.Max(DefenseArena.StageSize.y*.005f,4.5f/worldCamera.aspect);}
        public bool GetCell(Vector2 screen,out Vector2Int cell)
        {
            var p=worldCamera.ScreenToWorldPoint(new Vector3(screen.x,screen.y,-worldCamera.transform.position.z));
            cell=new Vector2Int(Mathf.FloorToInt((p.x-1.2f)/.66f),Mathf.FloorToInt((p.y-4.2f)/.66f));
            return cell.x>=0 && cell.x<10 && cell.y>=0 && cell.y<6;
        }
        public void ShowGhost(DefenseModel model,int selected,Vector2Int cell,bool visible)
        {
            visible &= model.IsPlaying && model.HasPiece;
            var piece=model.Offers[0];bool legal=model.CanPlace(piece,cell.x,cell.y);
            var points=piece.Cells();
            for(int i=0;i<4;i++)
            {
                var p=cell+points[i];bool inside=p.x>=0 && p.x<10 && p.y>=0 && p.y<6;
                ghosts[i].gameObject.SetActive(visible && inside);
                ghosts[i].transform.position=Position(DefenseArena.CellPosition(p.x,p.y));
                ghosts[i].color=legal ? new Color(.6f,1,.7f,.6f):new Color(1,.2f,.2f,.6f);
            }
        }
        public void Render(DefenseModel model,float dt)
        {
            for(int i=0;i<60;i++)
            {
                int element=model.Board[i%10,i/10];blocks[i].gameObject.SetActive(element!=0);
                int level=element==0 ? 0:model.Evolution[element-1];weapons[i].gameObject.SetActive(level>0);
                if(element==0)continue;
                blocks[i].sprite=blockArt[element-1];blocks[i].color=Color.white;FitArtwork(blocks[i],Vector2.one*.6f);
                if(level>0){weapons[i].sprite=weaponSprites[(element-1)*3+level-1];FitArtwork(weapons[i],Vector2.one*.52f);weapons[i].color=model.Overdrive>0 ? new Color(1,.92f,.5f):Color.white;}
            }
            for(int i=0;i<16;i++)
            {
                bool active=i<model.Supports.Count;supports[i].gameObject.SetActive(active);supportShadows[i].gameObject.SetActive(active);supportWeapons[i].gameObject.SetActive(false);
                if(!active)continue;var unit=model.Supports[i];
                supports[i].sprite=supportArt[unit.Kind];supports[i].color=Color.white;
                var art=supportArt[unit.Kind];supports[i].size=new Vector2(.68f,.68f*art.rect.height/art.rect.width);
                supports[i].flipX=unit.Aim.x>0;
                float scale=1+unit.SpawnFlash*.15f;supports[i].transform.localScale=new Vector3(scale,scale,1);
                var pos=Position(DefenseModel.SupportPosition(i));if(unit.Kind==2)pos.y+=Mathf.Sin(model.Elapsed*3+i)*.025f;supports[i].transform.position=pos;supportShadows[i].transform.position=pos+new Vector3(0,-.22f,0);
            }
            for(int i=0;i<enemies.Length;i++)
            {
                bool active=i<model.Enemies.Count;enemies[i].Body.gameObject.SetActive(active);monsterShadows[i].gameObject.SetActive(active);if(!active)continue;
                var data=model.Enemies[i];var visual=enemies[i];visual.Body.transform.position=Position(DefenseArena.LoopPoint(data.Progress));
                float size=data.Kind==3 ? .95f:data.Kind==2 ? .72f:.62f;
                var art=monsterArt[data.Kind];visual.Body.sprite=art;
                visual.Body.size=new Vector2(size,size*art.rect.height/art.rect.width);
                float bounce=Mathf.Sin(model.Elapsed*(data.Kind==1 ? 16:9)+data.Id)*.025f;
                visual.Body.transform.localScale=Vector3.one;visual.Body.transform.position+=Vector3.up*bounce;
                visual.Body.flipX=data.Progress>.5f;
                visual.Body.color=data.FrostTime>0 ? new Color(.65f,.9f,1):Color.white;
                if(data.HitFlash>0)visual.Body.color=new Color(1,.7f,.55f);
                visual.Label.transform.localPosition=new Vector3(0,-visual.Body.size.y*.46f,-.01f);
                monsterShadows[i].transform.position=visual.Body.transform.position+new Vector3(0,-size*.3f,0);FitArtwork(monsterShadows[i],new Vector2(size*.8f,size*.24f));
                visual.Label.gameObject.SetActive(data.Kind==3 || data.HitFlash>0);
                visual.Label.text=Mathf.CeilToInt(data.Health).ToString();
            }
            for(int i=0;i<projectiles.Length;i++)
            {
                shotLife[i]=Mathf.Max(0,shotLife[i]-dt);projectiles[i].gameObject.SetActive(shotLife[i]>0);
                if(shotLife[i]>0)projectiles[i].transform.position=Position(Vector2.Lerp(shotFrom[i],shotTo[i],1-shotLife[i]/.24f));
            }
            for(int i=0;i<effects.Length;i++)
            {
                effectLife[i]=Mathf.Max(0,effectLife[i]-dt);effects[i].gameObject.SetActive(effectLife[i]>0);
                float t=effectLife[i]/.18f;var c=effects[i].color;c.a=t;effects[i].color=c;effectLines[i].startColor=c;effectLines[i].endColor=c;
                if(rings[i])effects[i].size=Vector2.one*effectRadius[i]*(1-t*.6f)*.015f;
            }
        }
        public void ShowShot(DefenseShot shot)
        {
            int i=shotIndex++%projectiles.Length;shotLife[i]=.24f;shotFrom[i]=DefenseArena.CellPosition(shot.Cell.x,shot.Cell.y);shotTo[i]=shot.Target;
            projectiles[i].color=DefenseView.ElementColor(shot.Element);
        }
        public void AddEffect(DefenseAttackEffect effect)
        {
            if(effect.Kind==3)
            {
                var previous=effect.From;
                for(int j=1;j<=6;j++){var next=Vector2.Lerp(effect.From,effect.To,j/6f);if(j<6)next+=new Vector2(j%2==0 ? 9:-9,6);Segment(previous,next,4,new Color(.55f,1,.8f));previous=next;}
                return;
            }
            if(effect.Kind!=1){Segment(effect.From,effect.To,effect.Kind==4 ? 5:effect.Radius,effect.Kind==4 ? new Color(1,.55f,.25f):new Color(.3f,.85f,1));return;}
            int i=effectIndex++%effects.Length;effectLife[i]=.18f;rings[i]=true;effectRadius[i]=effect.Radius;
            effectLines[i].enabled=false;effects[i].enabled=true;effects[i].sprite=ringSprite;effects[i].color=new Color(1,.8f,.35f);effects[i].transform.position=Position(effect.To);effects[i].transform.rotation=Quaternion.identity;
        }
        private void Segment(Vector2 from,Vector2 to,float width,Color color)
        {
            int i=effectIndex++%effects.Length;effectLife[i]=.18f;rings[i]=false;effects[i].sprite=lineSprite;effects[i].color=color;
            effects[i].enabled=false;var line=effectLines[i];line.enabled=true;
            line.SetPosition(0,Position(from));line.SetPosition(1,Position(to));
            line.startWidth=line.endWidth=Mathf.Clamp(width,2,5)*.01f;
            line.startColor=line.endColor=color;
        }
        public static void FitArtwork(SpriteRenderer renderer,Vector2 bounds)
        {
            var size=renderer.sprite.rect.size;
            renderer.size=size*Mathf.Min(bounds.x/size.x,bounds.y/size.y);
            renderer.transform.localScale=Vector3.one;
        }
        public void Clear()
        {
            Array.Clear(shotLife,0,shotLife.Length);Array.Clear(effectLife,0,effectLife.Length);
            foreach(var p in projectiles)p.gameObject.SetActive(false);foreach(var e in effects)e.gameObject.SetActive(false);foreach(var g in ghosts)g.gameObject.SetActive(false);
        }
    }
}




