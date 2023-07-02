<Query Kind="Program">
  <Reference Relative="..\..\build\JTacticalSim.Base.dll">C:\dev\JTacticalSim\build\JTacticalSim.Base.dll</Reference>
  <Reference Relative="..\..\build\JTacticalSim.Component.dll">C:\dev\JTacticalSim\build\JTacticalSim.Component.dll</Reference>
  <Reference Relative="..\..\build\JTacticalSim.Console_OLD.exe">C:\dev\JTacticalSim\build\JTacticalSim.Console_OLD.exe</Reference>
  <Reference Relative="..\..\build\JTacticalSim.DataContext.dll">C:\dev\JTacticalSim\build\JTacticalSim.DataContext.dll</Reference>
  <Reference Relative="Plugins\JTacticalSim.LINQPad.Plugins.dll">C:\dev\JTacticalSim\Docs\LINQQueries\Plugins\JTacticalSim.LINQPad.Plugins.dll</Reference>
  <Reference Relative="..\..\build\JTacticalSim.Service.dll">C:\dev\JTacticalSim\build\JTacticalSim.Service.dll</Reference>
  <Reference Relative="..\..\build\JTacticalSim.Utility.dll">C:\dev\JTacticalSim\build\JTacticalSim.Utility.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Dynamic.dll</Reference>
  <Namespace>JTacticalSim</Namespace>
  <Namespace>JTacticalSim.API</Namespace>
  <Namespace>JTacticalSim.API.AI</Namespace>
  <Namespace>JTacticalSim.API.Component</Namespace>
  <Namespace>JTacticalSim.API.Game</Namespace>
  <Namespace>JTacticalSim.API.Service</Namespace>
  <Namespace>JTacticalSim.Cache</Namespace>
  <Namespace>JTacticalSim.Component.AI</Namespace>
  <Namespace>JTacticalSim.Component.AI</Namespace>
  <Namespace>JTacticalSim.Component.Data</Namespace>
  <Namespace>JTacticalSim.Component.Game</Namespace>
  <Namespace>JTacticalSim.Component.GameBoard</Namespace>
  <Namespace>JTacticalSim.Component.GameBoard</Namespace>
  <Namespace>JTacticalSim.Component.World</Namespace>
  <Namespace>JTacticalSim.Component.World</Namespace>
  <Namespace>JTacticalSim.Console_OLD</Namespace>
  <Namespace>JTacticalSim.Data</Namespace>
  <Namespace>JTacticalSim.DataContext</Namespace>
  <Namespace>JTacticalSim.GameState</Namespace>
  <Namespace>JTacticalSim.LINQPad.Plugins</Namespace>
  <Namespace>JTacticalSim.Service</Namespace>
  <Namespace>System.Configuration</Namespace>
  <Namespace>System.Dynamic</Namespace>
  <IncludePredicateBuilder>true</IncludePredicateBuilder>
</Query>

const bool SHOW_FULL_BATTLE_OUTPUT = false;
const bool SHOW_ROUNDS_OUTPUT = true;
const int RUN_COUNT = 10;
const bool SHOW_ATTACK_DEFENCE_VALUES = false;

JTacticalSim.API.Game.IGame TheGame = null;

List<IUnit> Attackers = new List<IUnit>();
List<IUnit> Defenders = new List<IUnit>();
	
void Main()
{	
	TheGame = ComponentUtilities.CreateNewGameInstance("UNIT_TEST");
	
	//var dieRoll = JTacticalSim.Utility.Die.Roll(7);
	//dieRoll.Dump();
	
	// Build armies for battle

	Attackers.Add(ScriptTestUnits.HellCats);
	Attackers.Add(ScriptTestUnits.A_Tank);
	Attackers.Add(ScriptTestUnits.C_Tank);
	//Attackers.Add(ScriptTestUnits.ScoutCavA);
	
	Defenders.Add(ScriptTestUnits.FighterSquad_A);
	Defenders.Add(ScriptTestUnits.Sam_Site);
	//Defenders.Add(ScriptTestUnits.Fadaykin_SF);
	Defenders.Add(ScriptTestUnits.A_SandyTanks);
		
	var battle = new Battle(Attackers, Defenders, BattleType.LOCAL);
	TheGame.GameBoard.SelectedNode = ScriptTestUnits.Sam_Site_A.GetNode();
	
	if(SHOW_ATTACK_DEFENCE_VALUES) ShowBattleAttackDefenseMetrics(Attackers, Defenders);

	FightBattle(battle);	
}


private void ShowBattleAttackDefenseMetrics(List<IUnit> attackers, List<IUnit> defenders)
{
	var sbAttackers = new StringBuilder();
	var sbDefenders = new StringBuilder();
	
	attackers.ForEach(u =>
	{
		sbAttackers.AppendLine(GetMetricsValues(u));
	});
	
	sbAttackers.ToString().Dump("Attacker Metrics : ");
	"".Dump();
		
	defenders.ForEach(u =>
	{
		sbDefenders.AppendLine(GetMetricsValues(u));
	});
	
	sbDefenders.ToString().Dump("Defender Metrics : ");
	"".Dump();
}

private string GetMetricsValues(IUnit unit)
{
	return string.Format("{0} full net attack : {1}  full net defence : {2} full net stealth : {3}", unit.Name, unit.GetFullNetAttackValue(), unit.GetFullNetDefenceValue(), unit.GetFullNetStealthValue());
}

private void FightBattle(IBattle battle)
{
	battle.BattleEnded += On_BattleEnded;;	
	battle.DoBattle();
}

private void On_BattleEnded(object sender, EventArgs e)
{
	var battle = (IBattle)sender;
	
	battle.VictoryCondition.Dump();
	battle.Attackers.Select(a => a.Name).Dump("Surviving Attackers : ");
	battle.Defenders.Select(a => a.Name).Dump("Surviving Defenders : ");
	battle.DefeatedUnits.Select(du => du.Name).Dump("Defeated Units : ");
	"".Dump();
	
	if (SHOW_FULL_BATTLE_OUTPUT) battle.Dump();
	
}