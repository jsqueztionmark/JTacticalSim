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
	
	const int width = 36;			// Max 36 for console
	const int height = 20;			// Max 20 for console
	
	var ID = 0;
	
	var file = new XDocument();
	var nodeList = new List<XElement>();
	
	for (var i = 0; i < width; i++)
	{
		for (var j = 0; j < height; j++)
		{	
			var coordinate = new XElement("Coordinate");
			coordinate.Add(new XAttribute("X", i));
			coordinate.Add(new XAttribute("Y", j));
			coordinate.Add(new XAttribute("Z", 0));
			
			var location = new XElement("Location");
			location.Add(coordinate);
			
			var country = new XElement("Country");
			country.Add(new XAttribute("ID", 0));
		
			var demo1 = new XElement("Demographic");
			demo1.Add(new XAttribute("ID", 0));
			demo1.Add(new XAttribute("InstanceName", ""));
			demo1.Add(new XAttribute("Name", ""));
			demo1.Add(new XAttribute("Orientation", "none"));
		
			var demographics = new XElement("Demographics");
			demographics.Add(demo1);
		
			var tile = new XElement("Tile");
			tile.Add(new XAttribute("VictoryPoints", 0));
			tile.Add(new XAttribute("ID", ID));
			tile.Add(new XAttribute("Name", ""));
			tile.Add(demographics);
		
			var defaultTile = new XElement("DefaultTile");
			defaultTile.Add(tile);
		
			var node = new XElement("Node");
			node.Add(new XAttribute("ID", ID));
			node.Add(location);
			node.Add(country);
			node.Add(defaultTile);
			
			nodeList.Add(node);
			
			ID++;
		}
	}
	
	var nodes = new XElement("Nodes");
	nodes.Add(nodeList);
	file.Add(nodes);
	
	file.Dump();
	
	
}