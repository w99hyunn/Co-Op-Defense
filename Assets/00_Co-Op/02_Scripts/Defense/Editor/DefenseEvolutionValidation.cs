namespace OpenFrame.Editor { public static class DefenseEvolutionValidation { public static string Run() {var results=new System.Collections.Generic.List<string>();
for(int element=1;element<=3;element++)
{
 var m=new OpenFrame.DefenseModel(new OpenFrame.DefenseRules(),1);m.StartWave();m.Evolution[element-1]=1;m.Board[0,0]=element;
 var a=new OpenFrame.DefenseEnemy{Id=101,Health=1000,MaxHealth=1000,Progress=.02f};
 var b=new OpenFrame.DefenseEnemy{Id=102,Health=1000,MaxHealth=1000,Progress=.03f};
 var distant=new OpenFrame.DefenseEnemy{Id=103,Health=1000,MaxHealth=1000,Progress=.3f};
 m.Enemies.Add(a);m.Enemies.Add(b);m.Enemies.Add(distant);int effects=0;m.AttackEffect+=e=>effects++;
 m.Tick(.01f);
 if(effects==0 || b.Health>=1000)throw new System.Exception("Missing evolved effect or secondary damage: "+element);
 if(distant.Health!=1000)throw new System.Exception("Attack hit distant off-axis target");
 if(element==3 && b.FrostTime<=0)throw new System.Exception("Chain frost missing");
 results.Add(element+": secondary damage + visible effect passed");
 var reset=new OpenFrame.DefenseModel(new OpenFrame.DefenseRules(),1);
 if(reset.Evolution[element-1]!=0)throw new System.Exception("Evolution leaked to new run");
}
return string.Join("\n",results);
} } }

