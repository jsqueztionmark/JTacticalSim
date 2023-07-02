<Query Kind="Program">
  <Reference Relative="..\..\build\JTacticalSim.Base.dll">C:\dev\JTacticalSim\build\JTacticalSim.Base.dll</Reference>
  <Reference Relative="..\..\build\JTacticalSim.Component.dll">C:\dev\JTacticalSim\build\JTacticalSim.Component.dll</Reference>
  <Reference Relative="..\..\build\JTacticalSim.Console_OLD.exe">C:\dev\JTacticalSim\build\JTacticalSim.Console_OLD.exe</Reference>
  <Reference Relative="..\..\build\JTacticalSim.DataContext.dll">C:\dev\JTacticalSim\build\JTacticalSim.DataContext.dll</Reference>
  <Reference Relative="Plugins\JTacticalSim.LINQPad.Plugins.dll">C:\dev\JTacticalSim\Docs\LINQQueries\Plugins\JTacticalSim.LINQPad.Plugins.dll</Reference>
  <Reference Relative="..\..\build\JTacticalSim.Media.dll">C:\dev\JTacticalSim\build\JTacticalSim.Media.dll</Reference>
  <Reference Relative="..\..\build\JTacticalSim.Service.dll">C:\dev\JTacticalSim\build\JTacticalSim.Service.dll</Reference>
  <Reference Relative="..\..\build\JTacticalSim.Utility.dll">C:\dev\JTacticalSim\build\JTacticalSim.Utility.dll</Reference>
  <Reference>&lt;ProgramFilesX86&gt;\Microsoft XNA\XNA Game Studio\v4.0\References\Windows\x86\Microsoft.Xna.Framework.dll</Reference>
  <Reference>&lt;ProgramFilesX86&gt;\Microsoft XNA\XNA Game Studio\v4.0\References\Windows\x86\Microsoft.Xna.Framework.Game.dll</Reference>
  <Reference>&lt;ProgramFilesX86&gt;\Microsoft XNA\XNA Game Studio\v4.0\References\Windows\x86\Microsoft.Xna.Framework.Graphics.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Dynamic.dll</Reference>
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
  <Namespace>JTacticalSim.Component.AI</Namespace>
  <Namespace>JTacticalSim.Component.Data</Namespace>
  <Namespace>JTacticalSim.Component.Game</Namespace>
  <Namespace>JTacticalSim.Component.GameBoard</Namespace>
  <Namespace>JTacticalSim.Component.GameBoard</Namespace>
  <Namespace>JTacticalSim.Component.World</Namespace>
  <Namespace>JTacticalSim.Component.World</Namespace>
  <Namespace>JTacticalSim.Console_OLD</Namespace>
  <Namespace>JTacticalSim.DataContext</Namespace>
  <Namespace>JTacticalSim.GameState</Namespace>
  <Namespace>JTacticalSim.LINQPad.Plugins</Namespace>
  <Namespace>JTacticalSim.Media.Sound</Namespace>
  <Namespace>JTacticalSim.Service</Namespace>
  <Namespace>JTacticalSim.Utility</Namespace>
  <Namespace>System.Configuration</Namespace>
  <Namespace>System.Dynamic</Namespace>
  <Namespace>System.Media</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludePredicateBuilder>true</IncludePredicateBuilder>
</Query>

JTacticalSim.API.Game.IGame TheGame = null;
	

void Main()
{
	TheGame = ComponentUtilities.CreateNewGameInstance("UNIT_TEST");
	
	var unit = ScriptTestUnits.Snipers;				

	unit.PlaySoundFinished += On_PlayFinished;
	unit.UnitInfo.UnitType.Name.Dump();
	unit.UnitInfo.UnitClass.Name.Dump();
	"Sound is playing....".Dump();
	unit.PlaySound(SoundType.FIRE);
	"Sound is playing....".Dump();
	unit.PlaySound(SoundType.MOVE);
	//"Sound is playing....".Dump();
	//u.PlaySound(SoundType.BUILD);

	
	
	
}

public void On_FileLoaded(object Sender, EventArgs e)
{
	"On_FileLoaded called....".Dump();	
}

public void On_PlayFinished(object Sender, EventArgs e)
{
	"On_PlayFinished called....".Dump();
}