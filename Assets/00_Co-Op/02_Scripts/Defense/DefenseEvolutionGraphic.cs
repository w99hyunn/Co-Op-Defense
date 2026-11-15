using System.Collections.Generic;
using UnityEngine;
namespace OpenFrame
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class DefenseEvolutionGraphic : UnityEngine.UI.MaskableGraphic
    {
        private struct Flash { public DefenseAttackEffect Effect; public float Life; }
        private readonly List<Flash> flashes=new List<Flash>(128);
        private DefenseModel model;
        public void Add(DefenseAttackEffect effect)
        {
            if(flashes.Count==128)flashes.RemoveAt(0);
            flashes.Add(new Flash{Effect=effect,Life=.35f});
        }
        public void Clear(){flashes.Clear();SetVerticesDirty();}
        public void Render(DefenseModel state,float dt)
        {
            model=state;
            for(int i=flashes.Count-1;i>=0;i--){var f=flashes[i]; f.Life-=dt;if(f.Life<=0)flashes.RemoveAt(i);else flashes[i]=f;}
            SetVerticesDirty();
        }
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear(); if(model==null)return; // Editor before a session is bound.
            for(int y=0;y<6;y++)for(int x=0;x<10;x++)
            {
                int element=model.Board[x,y];if(element==0)continue;
                int level=model.Evolution[element-1];if(level==0)continue;
                Vector2 p=DefenseArena.CellPosition(x,y);
                Color ink=new Color(.18f,.13f,.25f);
                Color glow=model.Overdrive>0 ? new Color(1,.9f,.3f) : Color.white;
                if(element==1){Ring(mesh,p,18,7,ink);Ring(mesh,p,12,4,glow);Line(mesh,p+Vector2.up*14,p+Vector2.up*28,10,ink);}
                else if(element==2){for(int k=0;k<level;k++){float dx=(k-(level-1)*.5f)*13;Line(mesh,p+new Vector2(dx,-17),p+new Vector2(dx,22),7,ink);Line(mesh,p+new Vector2(dx,-12),p+new Vector2(dx,20),3,glow);}}
                else {for(int k=0;k<3;k++){float a=k*Mathf.PI/3;Vector2 d=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*21;Line(mesh,p-d,p+d,5,ink);Line(mesh,p-d*.85f,p+d*.85f,2,glow);}}
                for(int k=0;k<level;k++)Line(mesh,p+new Vector2(-16+k*14,-25),p+new Vector2(-8+k*14,-25),4,glow);
                if(model.Overdrive>0)Ring(mesh,p,31+Mathf.Sin(model.Elapsed*10)*2,2,new Color(1,.85f,.3f,.8f));
            }
            for(int i=0;i<model.Supports.Count;i++)
            {
                var unit=model.Supports[i];Vector2 p=DefenseModel.SupportPosition(i);Vector2 aim=unit.Aim;
                Color ink=new Color(.19f,.16f,.25f);Color tint=unit.Kind==0 ? new Color(1,.45f,.28f):unit.Kind==1 ? new Color(.32f,.72f,1):new Color(.35f,.9f,.72f);
                Ring(mesh,p+Vector2.down*4,20,15,new Color(.39f,.29f,.23f,.65f));
                Ring(mesh,p,15,22,ink);Ring(mesh,p,12,17,tint);
                if(unit.Kind==0){Line(mesh,p,p+aim*30,15,ink);Line(mesh,p+aim*8,p+aim*29,8,tint);Ring(mesh,p,8,7,Color.white);}
                else if(unit.Kind==1){var side=new Vector2(-aim.y,aim.x)*7;Line(mesh,p-side,p+aim*31-side,8,ink);Line(mesh,p+side,p+aim*31+side,8,ink);Line(mesh,p-side,p+aim*29-side,3,Color.white);Line(mesh,p+side,p+aim*29+side,3,Color.white);}
                else {for(int k=0;k<3;k++){float a=k*Mathf.PI/3;Vector2 d=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*19;Line(mesh,p-d,p+d,4,Color.white);}Ring(mesh,p,5,6,ink);}
                if(unit.SpawnFlash>0)Ring(mesh,p,25+(1-unit.SpawnFlash)*20,4,new Color(1,.92f,.5f,unit.SpawnFlash));
                if(unit.Level>1)Ring(mesh,p,28,2,new Color(1,.8f,.25f));
            }
            foreach(var flash in flashes)
            {
                var e=flash.Effect;float alpha=flash.Life/.35f;
                if(e.Kind==1){Ring(mesh,e.To,e.Radius*(1-alpha*.6f),8*alpha,new Color(1,.5f,.12f,alpha));Ring(mesh,e.To,e.Radius*(1-alpha*.6f)*.65f,5*alpha,new Color(1,.9f,.4f,alpha));}
                else if(e.Kind==4){Line(mesh,e.From,e.To,5*alpha,new Color(1,.55f,.25f,alpha));}
                else if(e.Kind==2){Line(mesh,e.From,e.To,e.Radius*alpha,new Color(.2f,.8f,1,alpha));Line(mesh,e.From,e.To,3*alpha,new Color(1,1,1,alpha));}
                else {Vector2 prev=e.From;for(int i=1;i<=6;i++){Vector2 p=Vector2.Lerp(e.From,e.To,i/6f);if(i<6)p+=new Vector2(i%2==0 ? 9:-9,6);Line(mesh,prev,p,4,new Color(.55f,1,.8f,alpha));prev=p;}}
            }
        }
        private void Ring(UnityEngine.UI.VertexHelper mesh,Vector2 p,float radius,float width,Color color)
        {for(int i=0;i<24;i++){float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;Line(mesh,p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,p+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,width,color);}}
        private void Line(UnityEngine.UI.VertexHelper mesh,Vector2 a,Vector2 b,float width,Color color)
        {
            var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width*.5f;int i=mesh.currentVertCount;
            mesh.AddVert(a-n,color,Vector2.zero);mesh.AddVert(a+n,color,Vector2.zero);mesh.AddVert(b+n,color,Vector2.zero);mesh.AddVert(b-n,color,Vector2.zero);
            mesh.AddTriangle(i,i+1,i+2);mesh.AddTriangle(i,i+2,i+3);
        }
    }
}

