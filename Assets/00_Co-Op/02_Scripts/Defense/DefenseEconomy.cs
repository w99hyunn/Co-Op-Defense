using System;
using UnityEngine;
namespace OpenFrame
{
    public sealed partial class DefenseModel
    {

        public int Coins { get; private set; } = 60;
        public bool HasPiece { get; private set; }
        public int Rerolls { get; private set; }
        public int RollCost => HasPiece ? Math.Min(10,4+Rerolls*2) : EmergencyDraw ? 0 : 10;
        public bool EmergencyDraw => !HasPiece && ActiveCells==0 && Coins<10;
        public void Redraw()
        {
            if(!IsPlaying)return;
            int cost=RollCost;
            if(Coins<cost){Notice="골드 부족 · 몬스터를 처치하면 골드를 얻습니다";Changed?.Invoke();return;}
            Coins-=cost;
            if(HasPiece)Rerolls++;else Rerolls=0;
            var previous=Offers[0];
            var next=Draw();
            if(HasPiece && next.Shape==previous.Shape && next.Element==previous.Element)
                next=new DefensePiece(next.Shape,next.Element%3+1);
            Offers[0]=next;HasPiece=true;
            Notice=cost==0 ? "긴급 블록 지급 · 이 블록으로 처치 재화를 만드세요" : $"{cost}G 사용 · 배치하거나 리롤하세요";
            Changed?.Invoke();
        }
    }
}

