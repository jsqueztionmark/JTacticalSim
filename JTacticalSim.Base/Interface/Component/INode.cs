using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.Utility;
using JTacticalSim.API.InfoObjects;

namespace JTacticalSim.API.Component
{
	public interface INode : IBoardComponent, ISelectableComponent
	{
		bool IsWestOuterBoundary { get; }
		bool IsEastOuterBoundary { get; }
		bool IsNorthOuterBoundary { get; }
		bool IsSouthOuterBoundary { get; }

		List<NodeNeighborInfo> NeighborNodes { get; set; }

		/// <summary>
		/// This is required for some reason in order to bind Name to Winforms controls... ????
		/// </summary>
		string DisplayName { get; }

		ITile DefaultTile { get; }

		INode GetNodeInDirection(Direction direction, int distance);
		int VisibleUnitCount();
		int TotalUnitCount();

		IPathNode ToPathNode();

		INode Clone();

		/// <summary>
		/// Resets the stack display order for individual units; refreshes and resets unit stacks at the node.
		/// </summary>
		/// <param name="node"></param>
		void ResetUnitStackOrder();

		/// <summary>
		/// Resets the stack for the given unit bringing the given unit to the top of the stack
		/// </summary>
		/// <param name="topUnit"></param>
		void ResetUnitStackOrder(IUnit topUnit);

		void Render(int zoomLevel);
		void DisplayInfo();
	}
}
