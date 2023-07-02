using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceModel;
using JTacticalSim.Component.World;
using JTacticalSim.API.Component;
using JTacticalSim.API.Service;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.DataContext;
using JTacticalSim.Utility;
using ctxUtil = JTacticalSim.DataContext.Utility;

namespace JTacticalSim.Service
{
	[ServiceBehavior]
	public sealed class NodeService : BaseGameService, INodeService
	{
		static readonly object padlock = new object();

		private static volatile INodeService _instance;
		public static INodeService Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new NodeService();
				}

				return _instance;
			}
		}

		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		private NodeService()
		{ }

#region Service Methods

		[OperationBehavior]
		public IResult<INode, INode> SaveNodes(List<INode> nodes)
		{
			return ComponentRepository.SaveNodes(nodes);
		}

		[OperationBehavior]
		public IResult<INode, INode> RemoveNodes(List<INode> nodes)
		{
			return ComponentRepository.RemoveNodes(nodes);
		}

		[OperationBehavior]
		public IResult<INode, INode> UpdateNodes(List<INode> nodes)
		{
			return ComponentRepository.UpdateNodes(nodes);
		}

		[OperationBehavior]
		public IEnumerable<INode> GetAllNodes()
		{
			var retVal = new List<INode>();

			foreach (var dto in ComponentRepository.GetNodes())
			{
				// Check cache first
				var node = Cache().NodeCache.TryFind(dto.UID);

				// if it's there, grab it
				if (node != null)
				{
					retVal.Add(node);
					continue;
				}

				// if not, create the component and add it to the cache and grab it
				node = dto.ToComponent();
				Cache().NodeCache.TryAdd(node.UID, node);
				retVal.Add(node);
			}

			return retVal;
		}

		[OperationBehavior]
		public INode GetNodeAt(ICoordinate coordinate)
		{
			var node = GetAllNodes().SingleOrDefault(n => n.LocationEquals(coordinate));
			//if (node == null)
			//	throw new ComponentNotFoundException("No node found at board location {0}".F(coordinate));
			return node;
		}

		[OperationBehavior]
		public INode GetNodeAt(int X, int Y, int Z)
		{
			return GetNodeAt(new Coordinate(X, Y, Z));
		}

		[OperationBehavior]
		public IEnumerable<Tuple<INode, bool>> GetAllowableNeighborNodesForGrid(IUnit unit, INode node, IEnumerable<IPathableObject> map)
		{
			// allow for nodes off the start map given movement adjustments for that node			
			var allNeighbors = node.NeighborNodes.Where(nn => map.Any(mn => mn.Equals(nn.Node)));

			var retVal = new List<Tuple<INode, bool>>();

			// Remove all neighbor nodes that we can not move to from the current node
			Action<NodeNeighborInfo> componentAction = nnInfo =>
			{
				lock (retVal)
				{
					var r = TheGame().JTSServices.RulesService.UnitCanMoveInDirection(unit, node.DefaultTile, nnInfo.Direction).Result;
					if (r.CanMoveInDirection)
						retVal.Add(new Tuple<INode, bool>(nnInfo.Node, r.HasMovementOverrideInDirection));
				}
			};

			if (Convert.ToBoolean(ConfigurationManager.AppSettings["run_multithreaded"]))
			{
				Parallel.ForEach(allNeighbors, componentAction);
			}
			else
			{
				foreach (var n in allNeighbors)
				{
					componentAction(n);
				}
			}

			return retVal.AsEnumerable();
		}


		[OperationBehavior]
		public IEnumerable<INode> GetAllNodesWithinMaxDistance(IUnit unit,
																bool includeCurrentNode,
																bool includeExtendedNodes)
		{
			// If unit has no  movement points, get out of here.
			if (unit.MovementPoints == 0) return new List<INode>();
			var currentNode = GetNodeAt(unit.Location);
			return GetAllNodesWithinDistance(currentNode,
											unit.CurrentMoveStats.MovementPoints,
											includeCurrentNode,
											includeExtendedNodes);

		}


		[OperationBehavior]
		public List<IPathNode> GetAllowableMovementNodesForUnit(IUnit unit)
		{
			var retVal = new List<IPathNode>();
			var sourceNode = TheGame().JTSServices.NodeService.GetNodeAt(unit.Location);

			// If unit has no  movement points, get out of here.
			if (unit.CurrentMoveStats.MovementPoints <= 0) return retVal;

			// Get all nodes within reach
			var allPossibleNodes = TheGame().JTSServices.NodeService.GetAllNodesWithinDistance(sourceNode, unit.CurrentMoveStats.MovementPoints, false, false).ToList();

			// Remove any nodes with default tiles that prohibit movement for this unit based on geography or other
			allPossibleNodes.RemoveAll(n => 
							!TheGame().JTSServices.RulesService.TileIsAllowableForUnit(unit, n.DefaultTile).Result);

			// Reduce the overall amount of iterations
			for (var i = unit.CurrentMoveStats.MovementPoints; i >= 0; i--)
			{
				// Finally, remove any nodes without a connected path to the current node
				var nodesToCheck = allPossibleNodes.Where(n => n.G == i).ToList();

				Action<INode> componentAction = targetNode =>
				{
					lock (retVal)
					{
						var path = TheGame().JTSServices.AIService.FindPath(sourceNode, targetNode, nodesToCheck, unit).Result;
						// Null -- no path found
						if (path != null)
						{
							retVal.AddRange(path.Nodes.Where(pn => !retVal.Contains(pn)));
						}
					}
				};

				if (TheGame().IsMultiThreaded)
				{
					Parallel.ForEach(nodesToCheck, componentAction);
				}
				else
				{
					nodesToCheck.ForEach(targetNode =>
					{
						componentAction(targetNode);
					});
				}
			}

			return retVal;
		}

		[OperationBehavior]
		public IEnumerable<INode> GetAllNodesAtDistance(INode node,
														int distance,
														bool includeExtendedNodes)
		{
			//var extNodes = new List<INode>();
			var nodes = GetNodesAtDistance(node, distance);

			// Get Extended movements based on movement modifiers
			//extNodes = (includeExtendedNodes) ? GetExtendedNodes(nodes) : null;			
			//if (extNodes != null) nodes.AddRange(extNodes);

			return nodes;
		}

		/// <summary>
		/// Returns all nodes at a certain radius from the source node
		/// </summary>
		/// <param name="node"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		[OperationBehavior]
		public IEnumerable<INode> GetNodesAtDistance(INode node, int distance)
		{
			return GetNodesWithinDistance(node, distance)
						.Where(n =>
						(
							n.Location.X == (node.Location.X + distance) ||
							n.Location.X == (node.Location.X - distance) ||
							n.Location.Y == (node.Location.Y + distance) ||
							n.Location.Y == (node.Location.Y - distance)
						)).ToList();
		}

		[OperationBehavior]
		public IEnumerable<INode> GetAllNodesWithinDistance(INode node,
															int distance,
															bool includeCurrentNode,
															bool includeExtendedNodes)
		{
			//var extNodes = new List<INode>();	

			var nodes = GetNodesWithinDistance(node, distance).ToList();

			// Get Extended movements based on movement modifiers
			//if (includeExtendedNodes) extNodes = GetExtendedNodes(nodes);

			if (!includeCurrentNode) nodes.RemoveAll(i => i.Equals(node));
			//var retVal = nodes.Concat(extNodes);

			return nodes;
		}


		[OperationBehavior]
		public Direction GetNodeDirectionFromNeighborSourceNode(INode neighbor, INode source)
		{
			var LocationsWithDirections = GetAllNeighborNodeLocationsWithDirections(source);
			var retVal = LocationsWithDirections.Where(kvp => kvp.Value.Equals(neighbor.Location)).Select(kvp => kvp.Key).SingleOrDefault();
			return retVal;
		}

		[OperationBehavior]
		public List<Tuple<ITile, ITile>> GetNeighborNodesOppositeTilePairs(INode source)
		{
			var r = new List<Tuple<ITile, ITile>>();

			r.Add(GetTileOppositePair(Direction.NORTH, Direction.SOUTH, source));
			r.Add(GetTileOppositePair(Direction.NORTHEAST, Direction.SOUTHWEST, source));
			r.Add(GetTileOppositePair(Direction.EAST, Direction.WEST, source));
			r.Add(GetTileOppositePair(Direction.SOUTHEAST, Direction.NORTHWEST, source));

			return r;
		}


#endregion

		private Tuple<ITile, ITile> GetTileOppositePair(Direction a, Direction b, INode source)
		{
			var nodeA = source.GetNodeInDirection(a, 1);
			var nodeB = source.GetNodeInDirection(b, 1);

			var r = new Tuple<ITile, ITile>((nodeA != null) ? nodeA.DefaultTile : null, (nodeB != null) ? nodeB.DefaultTile : null);
			return r;
		}

		/// <summary>
		/// Returns all nodes within a certain radius from the source node
		/// </summary>
		/// <param name="node"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		private IEnumerable<INode> GetNodesWithinDistance(INode node, int distance)
		{
			//var retVal = new List<INode>();
			var retVal = GetAllNodes().Where(n =>
									n.Location.X >= (node.Location.X - distance) &&
									n.Location.X <= (node.Location.X + distance) &&
									n.Location.Y >= (node.Location.Y - distance) &&
									n.Location.Y <= (node.Location.Y + distance))
									.Select(n => n.Clone()).ToList();

			return retVal;
		}

		/// <summary>
		/// Returns extended nodes from the given node based on the distance to search
		/// </summary>
		/// <param name="nodes"></param>
		/// <returns></returns>
		[Obsolete("This method is causing major issues with some units' pathfinding. Do not use.")]
		private List<INode> GetExtendedNodes(List<INode> nodes)
		{
			var retVal = new List<INode>();

			// Extends the movement by the movement adjustment of the current node
			nodes.ForEach(n =>
			{
				if (n.DefaultTile.NetMovementAdjustment > 0)
					retVal = GetNodesWithinDistance(n, n.DefaultTile.NetMovementAdjustment).ToList();
			});

			var removeNodes = retVal.Where(xn => nodes.Any(n => n.Equals(xn))).ToList();
			removeNodes.ForEach(rn => retVal.RemoveAll(n => n.Equals(rn)));

			return retVal;
		}

		private Dictionary<Direction, ICoordinate> GetAllNeighborNodeLocationsWithDirections(INode source)
		{
			var retVal = new Dictionary<Direction, ICoordinate>
				{
					{Direction.NORTH, TheGame().JTSServices.TileService.CreateCoordinateForDirection(source.Location, Direction.NORTH, 1)},
					{Direction.SOUTH, TheGame().JTSServices.TileService.CreateCoordinateForDirection(source.Location, Direction.SOUTH, 1)},
					{Direction.WEST, TheGame().JTSServices.TileService.CreateCoordinateForDirection(source.Location, Direction.WEST, 1)},
					{Direction.EAST, TheGame().JTSServices.TileService.CreateCoordinateForDirection(source.Location, Direction.EAST, 1)},
					{Direction.NORTHWEST, TheGame().JTSServices.TileService.CreateCoordinateForDirection(source.Location, Direction.NORTHWEST, 1)},
					{Direction.NORTHEAST, TheGame().JTSServices.TileService.CreateCoordinateForDirection(source.Location, Direction.NORTHEAST, 1)},
					{Direction.SOUTHWEST, TheGame().JTSServices.TileService.CreateCoordinateForDirection(source.Location, Direction.SOUTHWEST, 1)},
					{Direction.SOUTHEAST, TheGame().JTSServices.TileService.CreateCoordinateForDirection(source.Location, Direction.SOUTHEAST, 1)},
				};

			return retVal;
		}

	}
}
