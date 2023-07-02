using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Game;

namespace JTacticalSim.API.Component
{
	public interface IBoard : IBaseGameObject
	{
		//events
		event WaypointReachedEvent WaypointReached;

		INode SelectedNode { get; set; }
		INode LastSelectedNode { get; set; }

		ICoordinate MainMapOrigin { get; set; }

		INode HighlightedNode { get; set; }
		List<IUnit> SelectedUnits { get; set; }

		IEnumerable<INode> AvailableMovementNodes { get; set; }
		IEnumerable<INode> CurrentViewableAreaNodes { get; set; }
		RouteInfo CurrentRoute { get; set; }
		RouteInfo LastRoute { get; set; }

		GameboardAttributeInfo DefaultAttributes { get; }
		GameboardStrategicValueAttributesInfo StrategicValuesAttributes { get; set; }

		/// <summary>
		/// Clears the temporarily selected items. Does not clear current route.
		/// </summary>
		/// <param name="resetCurrentViewableArea">Resets the current viewable map area</param>
		void ClearSelectedItems(bool resetCurrentViewableArea);
		void ClearCurrentRoute();

		/// <summary>
		/// Attempts to set the gameboard's current route based on current selected node and units
		/// </summary>
		void SetCurrentRoute(RouteType routeType);
		bool NodeIsInViewableArea(INode node);

		/// <summary>
		/// Sets the AvailableMovementNodes to nodes restricted by the currently selected unit(s)
		/// Configured to show all in movement radius or restrict to actual available moves within radius
		/// </summary>
		void ShowMoveRadius();
		void CenterSelectedNode();

		/// <summary>
		/// Attempts to add a unit to the collection of selected units on the board
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		IResult<IUnit, IUnit> AddSelectedUnit(IUnit unit);

		/// <summary>
		/// Attempts to remove a unit from the collection of selected units on the board
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		IResult<IUnit, IUnit> RemoveSelectedUnit(IUnit unit);

		/// <summary>
		/// Cycles the current zoom level in the direction IN/OUT given
		/// </summary>
		/// <param name="direction"></param>
		void Zoom(CycleDirection direction);

		void Refresh();
		void Render();
	}
}
