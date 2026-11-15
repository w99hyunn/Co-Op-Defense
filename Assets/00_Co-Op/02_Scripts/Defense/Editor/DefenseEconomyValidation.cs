using System;
using System.Collections.Generic;
namespace OpenFrame.Editor
{
    public static class DefenseEconomyValidation
    {
        public static string Run()
        {
            int checks=0;
            void Check(bool value,string name){if(!value)throw new InvalidOperationException(name);checks++;}
            var m=new DefenseModel(new DefenseRules(),17);m.StartWave();
            Check(m.Offers.Length==1 && !m.HasPiece && m.Coins==60,"single empty slot and starting gold");
            Check(!m.Place(0,0,0),"cannot place without purchase");
            m.Redraw();Check(m.Coins==50 && m.HasPiece && m.RollCost==4,"purchase");
            var old=m.Offers[0];m.Redraw();Check(m.Coins==46 && m.RollCost==6,"reroll escalates");
            Check(old.Shape!=m.Offers[0].Shape || old.Element!=m.Offers[0].Element,"paid reroll changes result");
            m.Paused=true;m.Redraw();Check(m.Coins==46,"pause prevents spending");m.Paused=false;
            Check(m.Place(0,0,0) && !m.HasPiece && m.Coins==46,"placement consumes only held piece");
            Check(!m.Place(0,4,0),"no duplicate placement");m.Redraw();Check(m.RollCost==4,"reroll cost resets after placement");
            int coins=m.Coins;
            m.Enemies.Add(new DefenseEnemy{Id=101,Health=-1,Kind=0});m.Enemies.Add(new DefenseEnemy{Id=102,Health=-1,Kind=3});
            m.Tick(.2f);Check(m.Coins==coins+28,"kill bounty paid by type");m.Tick(.2f);Check(m.Coins==coins+28,"bounty paid once");
            for(int n=0;n<3;n++)
            {
                if(!m.HasPiece)m.Redraw();
                Array.Clear(m.Board,0,m.Board.Length);
                for(int y=0;y<2;y++)for(int x=2;x<10;x++)m.Board[x,y]=1;
                m.Offers[0]=new DefensePiece(1,1);Check(m.Place(0,0,0),"double row placement");
                Check(m.Supports.Count==n+1 && m.SupportCharge==0,"double row summons persistent support");
            }
            Array.Clear(m.Board,0,m.Board.Length);
            m.Enemies.Clear();var target=new DefenseEnemy{Id=500,Health=10000,MaxHealth=10000,Progress=.1f};m.Enemies.Add(target);
            int shots=0;m.AttackEffect+=e=>shots++;
            m.Paused=true;m.Tick(2);Check(target.Health==10000,"pause freezes supports");m.Paused=false;
            m.Tick(.5f);Check(target.Health<10000 && shots>0,"central supports attack without board units");
            int count=m.Supports.Count;
            while(m.Phase==DefensePhase.Combat)m.Tick(.1f);
            m.ChooseUpgrade(0);Check(m.Supports.Count==count,"supports persist across waves");
            Check(new DefenseModel(new DefenseRules(),1).Supports.Count==0,"new session clears supports");
            var empty=new DefenseModel(new DefenseRules(),2);empty.StartWave();empty.Redraw();
            while(empty.Coins>=empty.RollCost)empty.Redraw();
            empty.Place(0,0,0);Array.Clear(empty.Board,0,empty.Board.Length);
            Check(empty.EmergencyDraw,"empty army recovery available");empty.Redraw();Check(empty.HasPiece,"free recovery gives usable piece");
            return $"PASS {checks} economy/support checks";
        }
    }
}

