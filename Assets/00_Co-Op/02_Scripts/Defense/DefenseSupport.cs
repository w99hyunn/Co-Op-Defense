using System;
using System.Collections.Generic;
using UnityEngine;
namespace OpenFrame
{
    public sealed class DefenseSupport
    {
        public int Kind;
        public int Level=1;
        public float Cooldown;
        public float SpawnFlash=1;
        public Vector2 Aim=Vector2.up;
    }
    public sealed partial class DefenseModel
    {
        public readonly List<DefenseSupport> Supports=new List<DefenseSupport>(16);
        public int SupportCharge {get;private set;}
        public event Action<int> SupportSummoned;
        public static Vector2 SupportPosition(int index)
        {
            int step=index%8;int column=step%2==0 ? 3-step/2 : 4+step/2;
            return new Vector2(226+column*64,1060+(index/8)*80);
        }
        private void ChargeSupport(int lines)
        {
            SupportCharge+=lines+(lines>=2 ? 1:0);
            while(SupportCharge>=3)
            {
                SupportCharge-=3;
                int kind=random.Next(3);
                if(Supports.Count<16)Supports.Add(new DefenseSupport{Kind=kind,Cooldown=.2f});
                else {var unit=Supports[random.Next(Supports.Count)];unit.Level++;unit.SpawnFlash=1;kind=unit.Kind;}
                SupportSummoned?.Invoke(kind);
            }
        }
        private void TickSupport(float dt)
        {
            for(int i=0;i<Supports.Count;i++)
            {
                var unit=Supports[i];unit.SpawnFlash=Math.Max(0,unit.SpawnFlash-dt);unit.Cooldown-=dt;
                if(unit.Cooldown>0 || Enemies.Count==0)continue;
                var origin=SupportPosition(i);var target=Enemies[0];float distance=float.MaxValue;
                foreach(var enemy in Enemies){float d=(DefenseArena.LoopPoint(enemy.Progress)-origin).sqrMagnitude;if(d<distance){distance=d;target=enemy;}}
                Vector2 point=DefenseArena.LoopPoint(target.Progress);unit.Aim=(point-origin).normalized;
                float damage=Damage*(1+unit.Level*.4f);
                unit.Cooldown=unit.Kind==0 ? 1.8f:unit.Kind==1 ? 1.1f:1.5f;
                target.Health-=damage;target.HitFlash=.2f;
                if(unit.Kind==0)
                {
                    AttackEffect?.Invoke(new DefenseAttackEffect(4,origin,point,5));
                    Blast(point,damage,85,target);
                }
                else if(unit.Kind==1)
                {
                    target.Health-=damage;
                    AttackEffect?.Invoke(new DefenseAttackEffect(2,origin,point,7));
                }
                else
                {
                    AttackEffect?.Invoke(new DefenseAttackEffect(3,origin,point));
                    foreach(var enemy in Enemies)if((DefenseArena.LoopPoint(enemy.Progress)-point).sqrMagnitude<110*110)
                    {enemy.FrostTime=2.5f;enemy.HitFlash=.2f;}
                }
                RemoveDead();
            }
        }
    }
}


