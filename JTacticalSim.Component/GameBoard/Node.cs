using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Service;
using JTacticalSim.Utility;

namespace JTacticalSim.Component.GameBoard
{
	public class Node : BoardComponentBase, INode
	{
		//Events
		public event ComponentSelectedEvent ComponentSelected;
		public event ComponentClickedEvent ComponentClicked;
		public event ComponentUnSelectedEvent ComponentUnSelected;

		public ITile DefaultTile { get; private set; }

		public bool IsWestOuterBoundary { get {return this.Location.X == 0;}}
		public bool IsEastOuterBoundary { get {return this.Location.X == TheGame().GameBoard.DefaultAttributes.Width - 1;}}
		public bool IsNorthOuterBoundary { get {return this.Location.Y == 0;}}
		public bool IsSouthOuterBoundary { get {return this.Location.Y == TheGame().GameBoard.DefaultAttributes.Height - 1;}}

		public List<NodeNeighborInfo> NeighborNodes { get; set; }

		public string DisplayName { get { return this.Name; } }

		// ----------------------------------------------------------------------------------------------------------

		public Node(ICoordinate location, ITile defaultTile)
		{
			this.Location = location;
			this.NeighborNodes = new List<NodeNeighborInfo>();
			this.DefaultTile = defaultTile;
		}

		public void Render(int zoomLevel)
		{
			TheGame().Renderer.On_NodePreRender(new EventArgs());
			TheGame().Renderer.RenderNode(this, zoomLevel);
			TheGame().Renderer.On_NodePostRender(new EventArgs());
		}

		public void DisplayInfo()
		{
			TheGame().Renderer.DisplayNodeInfo(this);
		}

		public void Select()
		{
			var r = TheGame().GameBoard.SelectedNode = this;
			On_ComponentSelected(new ComponentSelectedEventArgs());
		}

		public void UnSelect()
		{
			var r = TheGame().GameBoard.SelectedNode = null;
			On_ComponentUnSelected(new ComponentUnSelectedEventArgs());

		}

		// ----------------------------------------------------------------------------------------------------------

		// Graphics

		//public override IEnumerable<Rectangle?> GetSprites(SpriteSheet sheet)
		//{
		//	throw new NotImplementedException();
		//}

		public int TotalUnitCount()
		{
			var factions = TheGame().JTSServices.GameService.GetAllFactions();
			return TheGame().JTSServices.UnitService.GetUnitsAt(this.Location, factions).Count(); 
		}

		public int VisibleUnitCount()
		{
			var factions = TheGame().JTSServices.GameService.GetAllFactions();
			return TheGame().JTSServices.UnitService.GetUnitsAt(this.Location, factions)
															.Count(u => u.IsVisible()); 
		}

		public void ResetUnitStackOrder()
		{
			var tile = this.DefaultTile;
			var stacks = tile.GetAllComponentStacks();
			var updateUnits = new List<IUnit>();

			stacks.ForEach(us =>
			{
				int i = 1;

				us.GetAllUnits().ForEach(u =>
				{
					u.StackOrder = i; i++;
					updateUnits.Add(u);
				});
			});

			tile.ResetComponentStackDisplayOrder();
			TheGame().JTSServices.TileService.UpdateTiles(new List<ITile> { tile });
			TheGame().JTSServices.UnitService.UpdateUnits(updateUnits);
		}

		public void ResetUnitStackOrder(IUnit topUnit)
		{
			var tile = this.DefaultTile;
			var stack = tile.GetAllComponentStacks().SingleOrDefault(cs => cs.Country.Equals(topUnit.Country));

			if (stack == null)
				return;

			topUnit.StackOrder = 1;

			var updateUnits = new List<IUnit>();

			int i = 2;

			stack.GetAllUnits().Where(u => !u.Equals(topUnit))
								.ToList()
								.ForEach(u =>
			{
				u.StackOrder = i; i++;
				updateUnits.Add(u);
			});

			tile.ResetComponentStackDisplayOrder();
			TheGame().JTSServices.TileService.UpdateTiles(new List<ITile> { tile });
			TheGame().JTSServices.UnitService.UpdateUnits(updateUnits);
		}

		public IPathNode ToPathNode()
		{
			var r = new PathNode
				{
					Description = Description, 
					GOType = GOType, 
					Location = Location, 
					Parent = Parent, 
					Source = Source, 
					Target = Target, 
					ID = ID,
					Name = Name,
					UID = UID,
					SubNodeLocation = SubNodeLocation
				};

			return r;
		}

	//Handle tiles at this node


		public INode GetNodeInDirection(Direction direction, int distance)
		{
			try
			{
				var r = TheGame().JTSServices.NodeService.GetNodeAt(TheGame().JTSServices.TileService.CreateCoordinateForDirection(this.Location, direction, distance));
				return r;
			}
			catch (ComponentNotFoundException)
			{
				return null;
			}			
		}

		public INode Clone()
		{
			return (INode)this.MemberwiseClone();
		}

	//Event Handlers

		public void On_ComponentClicked(object sender, ComponentClickedEventArgs e)
		{
			MouseButton clicked = e.ButtonClicked;

			switch (clicked)
			{
				case MouseButton.LEFT :
				case MouseButton.RIGHT :
				case MouseButton.DOUBLELEFT :
				case MouseButton.DOUBLERIGHT :
					{
						TheGame().GameBoard.SelectedNode = this;
						break;
					}
					
			}

			if (ComponentClicked != null) ComponentClicked(this, e);

		}

		public void On_ComponentSelected(ComponentSelectedEventArgs e)
		{	
			TheGame().GameBoard.SetCurrentRoute(RouteType.FASTESTUNIT);
			if (ComponentSelected != null) ComponentSelected(this, e);
		}

		public void On_ComponentUnSelected(ComponentUnSelectedEventArgs e)
		{
			if (ComponentUnSelected != null) ComponentUnSelected(this, e);
		}

	}
}
