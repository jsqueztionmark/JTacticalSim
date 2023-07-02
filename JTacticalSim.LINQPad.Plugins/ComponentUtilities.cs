using System.Collections.Generic;
using System.Linq;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.Service;
using JTacticalSim.Media.Sound;
using JTacticalSim.Console_OLD;

namespace JTacticalSim.LINQPad.Plugins
{
	public static class ComponentUtilities
	{
		public static IGame CreateNewGameInstance(string gameName)
		{
			Game.Instance.NullGame();
			var TheGame = Game.Instance;

			TheGame.Create(	new ServiceDependencies(), 
							new ConsoleRenderer(), 
							new SoundSystem(),
							null);

			TheGame.LoadGame(gameName);
			TheGame.Start();

			return TheGame;
		}
	}

	public class PathSpec
	{
		public INode SourceNode { get; private set; } 
		public INode TargetNode { get; private set; }
		public IUnit Unit { get; private set; }
		public IEnumerable<IPathableObject> NodeMap { get; private set; } 

		public PathSpec(ICoordinate location, IGame theGame)
		{
			Unit = ScriptTestUnits.A_Tank;
			SourceNode = Unit.GetNode();
			TargetNode = theGame.JTSServices.NodeService.GetNodeAt(location);
			NodeMap = theGame
				.JTSServices.NodeService.GetAllNodesWithinDistance(SourceNode, 
																	Unit.CurrentMoveStats.MovementPoints, 
																	true, false)
												.Where(n => theGame.JTSServices.RulesService.TileIsAllowableForUnit(Unit, n.DefaultTile).Result);
		}
	}
}
