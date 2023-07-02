using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Transactions;
using JTacticalSim.API.Component;
using JTacticalSim.API;
using JTacticalSim.API.Service;
using JTacticalSim.Utility;
using JTacticalSim.DataContext;
using ctxUtil = JTacticalSim.DataContext.Utility;
using JTacticalSim.Component.World;

namespace JTacticalSim.Service
{
	[ServiceBehavior]
	public sealed class TileService : BaseGameService, ITileService
	{
		static readonly object padlock = new object();

		private static volatile ITileService _instance;
		public static ITileService Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new TileService();
				}

				return _instance;
			}
		}

		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		private TileService()
		{}


#region Service Methods

		[OperationBehavior]
		public IEnumerable<ITile> GetTiles()
		{
			return ComponentRepository.GetTiles().Select(t => t.ToComponent());
		}

		[OperationBehavior]
		public IResult<ITile, ITile> SaveTiles(List<ITile> tiles)
		{
			return ComponentRepository.SaveTiles(tiles);
		}

		[OperationBehavior]
		public IResult<ITile, ITile> RemoveTiles(List<ITile> tiles)
		{
			return ComponentRepository.RemoveTiles(tiles);
		}

		[OperationBehavior]
		public IResult<ITile, ITile> UpdateTiles(List<ITile> tiles)
		{
			return ComponentRepository.UpdateTiles(tiles);
		}

		[OperationBehavior]
		public IResult<ITile, ITile> NukeAffectTile(ITile tile, IDemographic nukeDemographic)
		{
			var result = new OperationResult<ITile, ITile>();

			if (tile == null || nukeDemographic == null)
			{
				result.Status = ResultStatus.FAILURE;
				result.Messages.Add("Arguments not of the correct type.");
				result.FailedObjects.Add(tile);
				return result;
			}

			using (var txn = new TransactionScope())
			{
				// Remove all flora, infrastructure, population (currently not used)
				var removeDemos = tile.Infrastructure.Concat(tile.Flora).Concat(tile.Population).ToList();
				var failedDemos = new List<IDemographic>();

				removeDemos.ForEach(d =>
					{
						var r = tile.RemoveDemographic(d);
						failedDemos.AddRange(r.FailedObjects);
					});

				// Add the nuke demo
				var addResult = tile.AddDemographic(nukeDemographic);
					
				tile.ReCalculateTileInfo();
				TheGame().Renderer.ResetTileDemographics(TheGame().JTSServices.NodeService.GetAllNodes());
				var tileUpdateResult = TheGame().JTSServices.TileService.UpdateTiles(new List<ITile> { tile });
				var nodeUpdateResult = TheGame().JTSServices.NodeService.UpdateNodes(new List<INode> { tile.GetNode() });

				if (failedDemos.Any())
				{
					result.Status = ResultStatus.FAILURE;
					result.Messages.Add("Not all demographics could be removed.");
					result.FailedObjects.Add(tile);
					return result;
				}

				txn.Complete();
				return result;
			}
		}

	// Other

		[OperationBehavior]
		public ICoordinate CreateCoordinate(int X, int Y, int Z)
		{
			return new Coordinate(X, Y, Z);
		}

		[OperationBehavior]
		public ICoordinate CreateCoordinateForDirection(ICoordinate location, Direction direction, int distance)
		{
			switch (direction)
			{
				case Direction.NORTH:
					return new Coordinate(location.X, location.Y - distance, 0);
				case Direction.SOUTH:
					return new Coordinate(location.X, location.Y + distance, 0);
				case Direction.EAST:
					return new Coordinate(location.X + distance, location.Y, 0);
				case Direction.WEST:
					return new Coordinate(location.X - distance, location.Y, 0);
				case Direction.NORTHWEST:
					return new Coordinate(location.X - distance, location.Y - distance, 0);
				case Direction.NORTHEAST:
					return new Coordinate(location.X + distance, location.Y - distance, 0);
				case Direction.SOUTHWEST:
					return new Coordinate(location.X - distance, location.Y + distance, 0);
				case Direction.SOUTHEAST:
					return new Coordinate(location.X + distance, location.Y + distance, 0);
				default:
					return null;
			}
		}

		[OperationBehavior]
		public IUnitStack GetCurrentStack(IMoveableComponent component)
		{
			var tile = component.GetNode().DefaultTile;
			var retVal = tile.GetCountryComponentStack(component.Country);
			return retVal;
		}


		[OperationBehavior]
		public IEnumerable<Direction> GetOrientationAllowableForDemographicClassByTile(ITile tile, IDemographicClass demographicClass)
		{
			// Get allowable orientations by base geographies
			// Compare to existing demographics of class type if any
	
			var retVal = new List<Direction>();
			var allAllowableOrientations = new List<Direction>();
	
			var allCurrentOrientations = tile.Infrastructure
												.Where(d => d.DemographicClass.Equals(demographicClass))
												.SelectMany(b => b.Orientation);
	
			foreach(var bg in tile.BaseGeography)
			{
				var	orientations = GetOrientationAllowableByBaseGeography(bg, demographicClass);
		
				// null if there is no compatibility
				if (orientations == null) continue;
		
				if (orientations.Any())
					allAllowableOrientations.AddRange(orientations.Where(d => !allAllowableOrientations.Contains(d)));
			}
	
			retVal = allAllowableOrientations.Where(d => !allCurrentOrientations.Contains(d)).ToList();
			return retVal;
		}

		private List<Direction> GetOrientationAllowableByBaseGeography(IDemographic baseDemographic, IDemographicClass newDemographicClass)
		{	
			if (baseDemographic == null || !baseDemographic.IsDemographicType("BaseGeography"))
				return null;
		
			switch(newDemographicClass.Name.ToLowerInvariant())
			{
				case "commandpost":
					{
						switch(baseDemographic.DemographicClass.Name.ToLowerInvariant())
						{
							case "land":
							case "shorelinenorth":
							case "shorelinesouth":
							case "shorelineeast":
							case "shorelinewest":
								return new List<Direction> {Direction.NONE};
							default :
								return null;
						}
					}
				case "bridge" :
					{
						switch(baseDemographic.DemographicClass.Name.ToLowerInvariant())
						{
							case "river":
								return new List<Direction> {Direction.NORTH, Direction.SOUTH, Direction.EAST, Direction.WEST};
							case "shorelinenorth":
								return new List<Direction> {Direction.SOUTH};
							case "shorelinesouth":
								return new List<Direction> {Direction.NORTH};
							case "shorelineeast":
								return new List<Direction> {Direction.WEST};
							case "shorelinewest":
								return new List<Direction> {Direction.EAST};
							default :
								return null;
						}
					}
				case "road" :
				case "traintrack" :
					{
						switch(baseDemographic.DemographicClass.Name.ToLowerInvariant())
						{
							case "land":
								return new List<Direction> {Direction.NORTH, Direction.SOUTH, Direction.EAST, Direction.WEST};
							case "shorelinenorth":
								return new List<Direction> {Direction.NORTH, Direction.EAST, Direction.WEST};
							case "shorelinesouth":
								return new List<Direction> {Direction.SOUTH, Direction.EAST, Direction.WEST};
							case "shorelineeast":
								return new List<Direction> {Direction.NORTH, Direction.SOUTH, Direction.EAST};
							case "shorelinewest":
								return new List<Direction> {Direction.NORTH, Direction.SOUTH, Direction.WEST};
							default :
								return null;
						}
					}
				default:
					{
						throw new Exception("{0} is not currently configured to determine allowable orientations".F(newDemographicClass.Name));
					}
			}				
		}

#endregion

	}
}
