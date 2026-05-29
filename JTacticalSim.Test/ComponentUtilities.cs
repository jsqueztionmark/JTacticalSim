using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.Component.Game;
using JTacticalSim.Service;

namespace JTacticalSim.Test
{
	public static class ComponentUtilities
	{
		public static IGame CreateNewGameInstance(string gameName)
		{
			Game.Instance.NullGame();
			var theGame = Game.Instance;

			theGame.Create(new ServiceDependencies());
			var loadResult = theGame.LoadGame(gameName);
			if (loadResult == null || loadResult.Status == ResultStatus.EXCEPTION)
				throw new InvalidOperationException(
					$"LoadGame('{gameName}') failed: {loadResult?.ex?.Message ?? "null result"}",
					loadResult?.ex);
			theGame.Start();

			return theGame;
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
