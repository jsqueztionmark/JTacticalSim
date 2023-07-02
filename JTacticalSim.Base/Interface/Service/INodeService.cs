using System;
using System.Collections.Generic;
using System.ServiceModel;
using JTacticalSim.API.Component;
using JTacticalSim.API.Data;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Service;
using JTacticalSim.Utility;

namespace JTacticalSim.API.Service
{
	[ServiceContract]
	public interface INodeService
	{
		[OperationContract]
		IResult<INode, INode> SaveNodes(List<INode> nodes);

		[OperationContract]
		IResult<INode, INode> RemoveNodes(List<INode> nodes);

		[OperationContract]
		IResult<INode, INode> UpdateNodes(List<INode> nodes);

		[OperationContract]
		IEnumerable<INode> GetAllNodes();

		///// <summary>
		///// Attempts to pull the node out of cache and adds it to the cache if it's not there
		///// </summary>
		///// <returns></returns>
		//[OperationContract]
		//IEnumerable<INode> GetAllNodesFromCache();

		[OperationContract]
		INode GetNodeAt(ICoordinate coordinate);

		[OperationContract]
		INode GetNodeAt(int x, int y, int z);

		/// <summary>
		/// Returns a collection of nodes that are not disallowed for movement by the demographic class for the unit type
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="node"></param>
		/// <param name="map"></param>
		/// <returns></returns>
		[OperationContract]
		IEnumerable<Tuple<INode, bool>> GetAllowableNeighborNodesForGrid(IUnit unit, INode node, IEnumerable<IPathableObject> map);

		/// <summary>
		/// Returns list of all nodes within the max movement distance of a given unit.
		/// includes the node at unit location if includeCurrent is true
		/// DO NOT INCLUDEEXTENDEDNODES until fixed. Currently causing pathfinding issues.
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="includeCurrent"></param>
		/// <param name="includeExtendedNodes"></param>
		/// <returns></returns>
		[OperationContract]
		IEnumerable<INode> GetAllNodesWithinMaxDistance(IUnit unit,
													bool includeCurrent,
													bool includeExtendedNodes);

		/// <summary>
		/// Returns list of all nodes within the movement distance of a given unit.
		/// includes the node at unit location if includeCurrent is true
		/// DO NOT INCLUDEEXTENDEDNODES until fixed. Currently causing pathfinding issues.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="distance"></param>
		/// <param name="includeCurrentNode"></param>
		/// <param name="includeExtendedNodes"></param>
		/// <returns></returns>
		[OperationContract]
		IEnumerable<INode> GetAllNodesWithinDistance(INode node,
													int distance,
													bool includeCurrentNode,
													bool includeExtendedNodes);

		/// <summary>
		/// Returns a direction from a given node to a neighbor node
		/// </summary>
		/// <param name="neighbor"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		[OperationContract]
		Direction GetNodeDirectionFromNeighborSourceNode(INode neighbor, INode source);

		/// <summary>
		/// Returns list of all nodes on the outer boundary of possible movement nodes based on a given source node and distance
		/// including extended movement nodes
		/// DO NOT INCLUDEEXTENDEDNODES until fixed. Currently causing pathfinding issues.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="distance"></param>
		/// <param name="includeExtendedNodes"></param>
		/// <returns></returns>
		[OperationContract]
		IEnumerable<INode> GetAllNodesAtDistance(INode node, int distance, bool includeExtendedNodes);

		/// <summary>
		/// Returns list of nodes that are within the given unit's movement restrictions
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		List<IPathNode> GetAllowableMovementNodesForUnit(IUnit unit);

		[OperationContract]
		IEnumerable<INode> GetNodesAtDistance(INode node, int distance);

		/// <summary>
		/// Returns a result set containing default tiles from neighbor nodes on opposite sides of a given node
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		[OperationContract]
		List<Tuple<ITile, ITile>> GetNeighborNodesOppositeTilePairs(INode source);
	}
}
