using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using JTacticalSim.API.Component;
using JTacticalSim.API;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Service;
using JTacticalSim.Component.World;

namespace JTacticalSim.Component.GameBoard
{
	public class Board : BaseGameObject, IBoard
	{
		public event WaypointReachedEvent WaypointReached;

		private INode _selectedNode;
		public INode SelectedNode
		{
			get { return _selectedNode; } 
			set 
			{
				if (_selectedNode != null)
					_lastSelectedNode = _selectedNode;

				_selectedNode = value;
			}
		}

		private INode _lastSelectedNode;
		public INode LastSelectedNode
		{
			get { return _lastSelectedNode; }
			set
			{
				_lastSelectedNode = value;
			}
		}

		public INode HighlightedNode { get; set; }	
	
		public ICoordinate MainMapOrigin { get; set; }

		private List<IUnit> _selectedUnits;
		public List<IUnit> SelectedUnits 
		{
			get { return _selectedUnits; }
			set 
			{ 
				_selectedUnits = value;

				if (value != null)
					_selectedUnits.ForEach(u =>
						{
							u.On_ComponentSelected(new ComponentSelectedEventArgs());
						});
			} 
		}

		public RouteInfo CurrentRoute { get; set; }
		public RouteInfo LastRoute { get; set; }

		public IEnumerable<INode> AvailableMovementNodes { get; set; }
		public IEnumerable<INode> CurrentViewableAreaNodes { get; set; }

		public GameboardAttributeInfo DefaultAttributes { get; private set; }
		public GameboardStrategicValueAttributesInfo StrategicValuesAttributes { get; set; }

		public Board(GameboardAttributeInfo defaultAttributes)
			: base(GameObjectType.COMPONENT)
		{
			AvailableMovementNodes = new List<INode>();
			CurrentViewableAreaNodes = new List<INode>();
			SelectedUnits = new List<IUnit>();
			DefaultAttributes = defaultAttributes;
			
		}

		public void Refresh()
		{
			var nodes = TheGame().JTSServices.NodeService.GetAllNodes().ToList();
			nodes.ForEach(n => n.DefaultTile.RefreshComponentStacks());
		}

		public void Render()
		{
			TheGame().Renderer.On_BoardPreRender(new EventArgs());
			TheGame().Renderer.RenderBoard();
			TheGame().Renderer.On_BoardPostRender(new EventArgs());
		}		

		public IResult<IUnit, IUnit> AddSelectedUnit(IUnit unit)
		{
			var r = new OperationResult<IUnit, IUnit> {Status = ResultStatus.SUCCESS};

			if (_selectedUnits.Contains(unit))
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Unit is already selected.");
				return r;
			}

			try
			{
				_selectedUnits.Add(unit);
				r.Result = unit;
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
			}

			return r;
		}

		public IResult<IUnit, IUnit> RemoveSelectedUnit(IUnit unit)
		{
			var r = new OperationResult<IUnit, IUnit> {Status = ResultStatus.SUCCESS};

			if (!_selectedUnits.Contains(unit))
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Unit is not currently selected.");
				return r;
			}

			try
			{
				_selectedUnits.Remove(unit);
				r.Result = unit;
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
			}

			return r;
		}

		public void ClearSelectedItems(bool resetCurrentViewableArea)
		{
			SelectedNode = null;
			LastSelectedNode = null;
			LastRoute = null;
			AvailableMovementNodes = new List<INode>();

			if (resetCurrentViewableArea)
				CurrentViewableAreaNodes = new List<INode>();

			SelectedUnits = new List<IUnit>();
		}

		public void ClearCurrentRoute()
		{
			CurrentRoute = null;
		}

		public void SetCurrentRoute(RouteType routeType)
		{
			if (SelectedUnits != null && SelectedUnits.Any())
			{
				switch (routeType)
				{
					case RouteType.ALLUNITS:
						{
							foreach (var unit in SelectedUnits)
							{
								var currentNode = unit.GetNode();
								var master = TheGame().JTSServices.NodeService
															.GetAllNodesWithinDistance(currentNode, unit.CurrentMoveStats.MovementPoints, true, false);	
			
								var map = master.Select(n => n.ToPathableObject<PathNode>());

								var r = TheGame().JTSServices.AIService.FindPath(currentNode, SelectedNode, map, unit);
								if (r.Status == ResultStatus.FAILURE)
									return;

								LastRoute = CurrentRoute;
								CurrentRoute = r.Result;
							}

							break;
						}
					default:
						{
							var distance = (routeType == RouteType.SLOWESTUNIT) 
											? SelectedUnits.Min(u => u.CurrentMoveStats.MovementPoints)
											: SelectedUnits.Max(u => u.CurrentMoveStats.MovementPoints);
							IUnit unit = SelectedUnits.FirstOrDefault(u => u.CurrentMoveStats.MovementPoints == distance);

							var currentNode = unit.GetNode();
							var master = TheGame().JTSServices.NodeService
														.GetAllNodesWithinDistance(currentNode, unit.CurrentMoveStats.MovementPoints, true, false);	
			
							var map = master.Select(n => n.ToPathableObject<PathNode>());							

							// Else try to get the actual path
							var r = TheGame().JTSServices.AIService.FindPath(currentNode, SelectedNode, map, unit);
							LastRoute = CurrentRoute;
							CurrentRoute = r.Result;

							break;
						}
				}				
				
				// Double check for null
				if (CurrentRoute != null)
					AvailableMovementNodes = CurrentRoute.Nodes.Where(n => n != null).Select(n => n.GetNode());
			}
		}	

		public bool NodeIsInViewableArea(INode node)
		{
			return	CurrentViewableAreaNodes.Contains(node);
		}

		public void ShowMoveRadius()
		{
			// FYI: Restricting to available moves for unit can, currently, be HIGHLY unperformant
			if (CurrentRoute == null)
			{
				var minDistance = SelectedUnits.Min(u => u.CurrentMoveStats.MovementPoints);
				IUnit slowestUnit = SelectedUnits.FirstOrDefault(u => u.CurrentMoveStats.MovementPoints == minDistance);

				if (Convert.ToBoolean(ConfigurationManager.AppSettings["restrict_moveradius_to_available_only"]))
				{
					var pNodes = slowestUnit.GetAllowableMovements();
					AvailableMovementNodes = pNodes.Where(n => n != null).Select(n => n.GetNode());
				}
				else
				{
					AvailableMovementNodes = TheGame()
						.JTSServices.NodeService.GetAllNodesWithinDistance(slowestUnit.GetNode(),
						                                                    minDistance,
						                                                    true, false);
				}
			}
		}

		public void CenterSelectedNode()
		{
			TheGame().Renderer.CenterSelectedNode();
		}

		public void Zoom(CycleDirection direction)
		{
			TheGame().Renderer.ZoomMap(direction);
		}

	}
}
