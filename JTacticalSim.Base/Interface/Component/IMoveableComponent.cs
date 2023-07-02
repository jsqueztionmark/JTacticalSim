using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Service;

namespace JTacticalSim.API.Component
{
	public interface IMoveableComponent : IBoardComponent, ISelectableComponent
	{
		// Events
		event ComponentMoveEvent ComponentBeginMove;
		event ComponentMoveEvent ComponentEndMove;
		event WaypointReachedEvent WaypointReached;

		/// <summary>
		/// Tracks the running totals of move statistics within the scope of a single move
		/// </summary>
		CurrentMoveStatInfo CurrentMoveStats { get; set; }

		/// <summary>
		/// Stack order within the current container
		/// </summary>
		int StackOrder { get; set; }

		/// <summary>
		/// Resets the movement stats
		/// Should be called on the TurnEndEvent
		/// </summary>
		void ResetMoveStats();

		IResult<IMoveableComponent, IMoveableComponent> MoveToLocation(INode targetNode, INode sourceNode);
	}
}
