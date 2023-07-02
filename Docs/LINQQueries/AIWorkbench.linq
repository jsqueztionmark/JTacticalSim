<Query Kind="Program">
  <Reference Relative="..\..\build\JTacticalSim.Base.dll">C:\dev\JTacticalSim\build\JTacticalSim.Base.dll</Reference>
  <Reference Relative="..\..\build\JTacticalSim.Component.dll">C:\dev\JTacticalSim\build\JTacticalSim.Component.dll</Reference>
  <Reference Relative="..\..\build\JTacticalSim.Console_OLD.exe">C:\dev\JTacticalSim\build\JTacticalSim.Console_OLD.exe</Reference>
  <Reference Relative="..\..\build\JTacticalSim.DataContext.dll">C:\dev\JTacticalSim\build\JTacticalSim.DataContext.dll</Reference>
  <Reference Relative="Plugins\JTacticalSim.LINQPad.Plugins.dll">C:\dev\JTacticalSim\Docs\LINQQueries\Plugins\JTacticalSim.LINQPad.Plugins.dll</Reference>
  <Reference Relative="..\..\build\JTacticalSim.Service.dll">C:\dev\JTacticalSim\build\JTacticalSim.Service.dll</Reference>
  <Reference Relative="..\..\build\JTacticalSim.Utility.dll">C:\dev\JTacticalSim\build\JTacticalSim.Utility.dll</Reference>
  <Reference>&lt;ProgramFilesX86&gt;\Microsoft XNA\XNA Game Studio\v4.0\References\Windows\x86\Microsoft.Xna.Framework.dll</Reference>
  <Reference>&lt;ProgramFilesX86&gt;\Microsoft XNA\XNA Game Studio\v4.0\References\Windows\x86\Microsoft.Xna.Framework.Game.dll</Reference>
  <Reference>&lt;ProgramFilesX86&gt;\Microsoft XNA\XNA Game Studio\v4.0\References\Windows\x86\Microsoft.Xna.Framework.Graphics.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Dynamic.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.IO.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Linq.Expressions.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Threading.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Threading.Tasks.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Threading.Tasks.Parallel.dll</Reference>
  <Namespace>JTacticalSim</Namespace>
  <Namespace>JTacticalSim.API</Namespace>
  <Namespace>JTacticalSim.API.AI</Namespace>
  <Namespace>JTacticalSim.API.Component</Namespace>
  <Namespace>JTacticalSim.API.Game</Namespace>
  <Namespace>JTacticalSim.API.InfoObjects</Namespace>
  <Namespace>JTacticalSim.API.Service</Namespace>
  <Namespace>JTacticalSim.Cache</Namespace>
  <Namespace>JTacticalSim.Component.AI</Namespace>
  <Namespace>JTacticalSim.Component.Data</Namespace>
  <Namespace>JTacticalSim.Component.Game</Namespace>
  <Namespace>JTacticalSim.Component.GameBoard</Namespace>
  <Namespace>JTacticalSim.Component.World</Namespace>
  <Namespace>JTacticalSim.Console_OLD</Namespace>
  <Namespace>JTacticalSim.DataContext</Namespace>
  <Namespace>JTacticalSim.GameState</Namespace>
  <Namespace>JTacticalSim.LINQPad.Plugins</Namespace>
  <Namespace>JTacticalSim.Service</Namespace>
  <Namespace>JTacticalSim.Utility</Namespace>
  <Namespace>System.Configuration</Namespace>
  <Namespace>System.Dynamic</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludePredicateBuilder>true</IncludePredicateBuilder>
</Query>

JTacticalSim.API.Game.IGame TheGame = null;

void Main()
{
	TheGame = ComponentUtilities.CreateNewGameInstance("UNIT_TEST");
	var countries = TheGame.JTSServices.GameService.GetCountries();
	var unit = ScriptTestUnits.MEDSP_Tanks;
	
	var demographics = TheGame.JTSServices.NodeService.GetNodeAt(2, 0, 0).DefaultTile.GetAllDemographics();
	var road = TheGame.JTSServices.DemographicService.GetDemographicByID(11);
	road.Orientation = new List<Direction> {Direction.NORTH, Direction.EAST};
	var bridge = TheGame.JTSServices.DemographicService.GetDemographicByID(12);
	bridge.Orientation = new List<Direction> {Direction.SOUTH};
	var bridge2 = TheGame.JTSServices.DemographicService.GetDemographicByID(12);
	bridge2.Orientation = new List<Direction> {Direction.SOUTH, Direction.EAST};
	demographics.Add(road);
	demographics.Add(bridge);
	demographics.Add(bridge2);
	
	var q = demographics.EnsureUnique();
	
	q.Dump();
	
	/*
	var args = new object[] {node.DefaultTile(), demographic};
	
	var missionType = TheGame.JTSServices.AIService.GetMissionTypes().Where(mot => mot.Name == "Build").SingleOrDefault();
	var mission = new Mission(missionType, unit);
	var unitTaskType = TheGame.JTSServices.AIService.GetUnitTaskTypes().Where(utt => utt.Name == "BuildInfrastructure").SingleOrDefault();
	var task = new UnitTask(unitTaskType, mission, args, demographic.DemographicClass.BuildInfo.BuildTurns);
	var tactic = new Tactic(StrategicalStance.ADMINISTRATIVE, TheGame.CurrentTurn.Player);
	
	mission.AddChildComponent(task);
	mission.SetCurrentTask();
	tactic.AddChildComponent(mission);
	
	TheGame.JTSServices.AIService.SaveTactic(tactic);
	
	
	unit.HasCurrentMission().Dump();
	unit.GetCurrentMission().TurnsToComplete.Dump();
	tactic.Execute();
	unit.HasCurrentMission().Dump();
	unit.GetCurrentMission().TurnsToComplete.Dump();
	TheGame.CurrentTurn.End();
	TheGame.CurrentTurn.End();
	tactic.Execute();
	unit.HasCurrentMission().Dump();
	unit.GetCurrentMission().TurnsToComplete.Dump();
	TheGame.CurrentTurn.End();
	TheGame.CurrentTurn.End();
	tactic.Execute();
	unit.HasCurrentMission().Dump();
	unit.GetCurrentMission().TurnsToComplete.Dump();
	TheGame.CurrentTurn.End();
	TheGame.CurrentTurn.End();
	tactic.Execute();	
	unit.HasCurrentMission().Dump();
	*/
							
}