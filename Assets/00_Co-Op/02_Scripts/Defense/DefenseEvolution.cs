using System;
using System.Collections.Generic;
using UnityEngine;
namespace OpenFrame
{
    public readonly struct DefenseAttackEffect
    {
        public readonly int Kind;
        public readonly Vector2 From, To;
        public readonly float Radius;
        public DefenseAttackEffect(int kind, Vector2 from, Vector2 to, float radius=0) { Kind=kind; From=from; To=to; Radius=radius; }
    }
    public sealed partial class DefenseModel
    {
        public readonly int[] Evolution = new int[3];
        public event Action<DefenseAttackEffect> AttackEffect;
        private readonly HashSet<int> chained = new HashSet<int>();
        public string UpgradeName(int choice)
        {
            string[,] names = { { "폭발탄", "집속 폭발", "연쇄 포격" }, { "관통 레이저", "분열 광선", "삼중 광선" }, { "연쇄 냉기", "빙결 전파", "빙하 폭풍" } };
            return names[choice, Math.Max(0,Evolution[choice]-1)];
        }
        public string UpgradeDescription(int choice)
        {
            int level=Evolution[choice];
            string[] first = { "빨강 → 범위 폭발탄\n줄 완성: 폭발 범위 확대", "파랑 → 일직선 관통 레이저\n줄 완성: 광선 폭 확대", "초록 → 적 3마리 연쇄 냉기\n줄 완성: 연쇄 수 +3" };
            string[] second = { "빨강 → 주변에 추가 집속 폭발\n줄 완성: 큰 폭발 2회", "파랑 → 두 갈래 관통 광선\n줄 완성: 넓은 광선", "초록 → 5마리 연쇄 + 강한 둔화\n줄 완성: 8마리 전파" };
            string[] third = { "빨강 → 세 지점 연쇄 포격", "파랑 → 세 갈래 관통 광선", "초록 → 7마리 연쇄 + 빙결" };
            return level==0 ? first[choice] : level==1 ? second[choice] : level==2 ? third[choice] : "최종 진화 장착 완료";
        }
        private void EvolvedAttack(Vector2Int cell,int element,DefenseEnemy target,float damage,Vector2 source)
        {
            int level=Evolution[element-1];
            if(level==0) return;
            bool surge=Overdrive>0;
            Vector2 point=DefenseArena.LoopPoint(target.Progress);
            if(element==1)
            {
                Blast(point,damage, surge ? 145 : 95, target);
                for(int i=1;i<level;i++) Blast(DefenseArena.LoopPoint(target.Progress+(i==1 ? .055f : -.055f)),damage*.65f,surge ? 110 : 70,null);
            }
            else if(element==2)
            {
                Vector2 direction=(point-source).normalized;
                for(int ray=0;ray<level;ray++)
                {
                    float angle=ray==0 ? 0 : ray==1 ? 9 : -9;
                    Vector2 end=source+(Vector2)(Quaternion.Euler(0,0,angle)*direction)*1100;
                    if(end.y>1335) end=source+(end-source)*((1335-source.y)/(end.y-source.y));
                    AttackEffect?.Invoke(new DefenseAttackEffect(2,source,end,surge ? 18 : 7));
                    foreach(var enemy in Enemies)
                    {
                        if(enemy==target && ray==0)continue;
                        Vector2 pos=DefenseArena.LoopPoint(enemy.Progress);
                        Vector2 delta=end-source;
                        float t=Vector2.Dot(pos-source,delta)/delta.sqrMagnitude;
                        if(t>=0 && t<=1 && Vector2.Distance(pos,source+delta*t)<(surge ? 52 : 30)) {enemy.Health-=damage*.85f; enemy.HitFlash=.15f;}
                    }
                }
            }
            else
            {
                chained.Clear(); chained.Add(target.Id);
                target.FrostTime=level>=3 ? 3 : 1.8f;
                for(int i=0;i<level*2+(surge ? 3 : 0);i++)
                {
                    DefenseEnemy next=null; float best=260*260;
                    foreach(var enemy in Enemies)
                    {
                        if(chained.Contains(enemy.Id)||enemy.Health<=0)continue;
                        float distance=(DefenseArena.LoopPoint(enemy.Progress)-point).sqrMagnitude;
                        if(distance<best){best=distance;next=enemy;}
                    }
                    if(next==null)break; // No unvisited target within chain range.
                    var end=DefenseArena.LoopPoint(next.Progress);
                    AttackEffect?.Invoke(new DefenseAttackEffect(3,point,end));
                    next.Health-=damage*.75f; next.FrostTime=level>=3 ? 3 : 1.8f; next.HitFlash=.15f;
                    chained.Add(next.Id); point=end;
                }
            }
        }
        private void Blast(Vector2 point,float damage,float radius,DefenseEnemy primary)
        {
            AttackEffect?.Invoke(new DefenseAttackEffect(1,point,point,radius));
            foreach(var enemy in Enemies)
                if(enemy!=primary && (DefenseArena.LoopPoint(enemy.Progress)-point).sqrMagnitude<radius*radius)
                {enemy.Health+=-damage*.8f;enemy.HitFlash=.2f;}
        }
    }
}


