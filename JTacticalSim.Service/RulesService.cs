using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using LinqKit;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JTacticalSim.API;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.Game;
using JTacticalSim.API.Service;
using JTacticalSim.DataContext;
using JTacticalSim.Utility;

namespace JTacticalSim.Service
{
	[ServiceBehavior]
	public class RulesService : BaseGameService, IRulesService
	{
		static readonly object padlock = new object();

		private static volatile IRulesService _instance;
		public static IRulesService Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new RulesService();
				}

				return _instance;
			}
		}

		private RulesService()
		{}

#region Service Methods


	// Node/Tile

		[OperationBehavior]
		public IResult<bool, ICoordinate> NodeIsFactionNode(ICoordinate location, IFaction faction)
		{
			var r = new OperationResult<bool, ICoordinate>();

			var existingNode =  TheGame().JTSServices.NodeService.GetNodeAt(location);

			if (existingNode.Country.Faction.Equals(faction))
				r.Result = true;

			return r;
		}

		[OperationBehavior]
		public IResult<bool, Tuple<IUnit, INode>> NodeIsValidForMove(IUnit unit, INode targetNode)
		{
			var r = new OperationResult<bool, Tuple<IUnit, INode>>{Status = ResultStatus.SUCCESS, Result = true};
			
			try
			{
				var sourceNode = unit.GetNode();
				var allPossibleNodes = TheGame().JTSServices.NodeService.GetAllNodesWithinDistance(sourceNode, unit.CurrentMoveStats.MovementPoints, true, false).ToList();

				if (!allPossibleNodes.Any(n => n.Equals(targetNode)))
				{
					r.Status = ResultStatus.OTHER;
					r.Messages.Add("Node is not in movement range for {0}".F(unit.Name));
					r.Result = false;
					return r;
				}

				// Remove any nodes with default tiles that prohibit movement for this unit based on geography
				allPossibleNodes.RemoveAll(n => !TheGame().JTSServices.RulesService.TileIsAllowableForUnit(unit, n.DefaultTile).Result);

				var path = TheGame().JTSServices.AIService.FindPath(sourceNode, targetNode, allPossibleNodes, unit).Result;

				if (path == null)
				{
					r.Messages.Add("Node is not a valid move for {0}".F(unit.Name));
					r.Result = false;
					return r;
				}
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.Result = false;
				r.ex = ex;
				return r;
			}

			r.Messages.Add("Node is a valid move for {0}".F(unit.Name));
			return r;
		}

		[OperationBehavior]
		public IResult<bool, ITile> TileIsChokepoint(ITile tile)
		{
			var node = tile.GetNode();
			var r = new OperationResult<bool, ITile> { Status = ResultStatus.SUCCESS, Result = false};

			List<Tuple<ITile, ITile>> neighborPairs;

			// Bridge
			var isBridge = node.DefaultTile.Infrastructure.Any(d => d.IsDemographicClass("bridge"));

			if (isBridge)
			{
				r.Result = true;
				r.Messages.Add("Tile has a bridge.");
				return r;
			}
			
			// Get all the surrounding node tiles in opposite pairs.
			// We determine if this is a chokepoint by ascertaining whether there is ANY opposite pair
			// That would force units to use this node in a path
			try
			{
				neighborPairs = TheGame().JTSServices.NodeService.GetNeighborNodesOppositeTilePairs(node);
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				return r;
			}

			try
			{
				var pathThroughRestrictedMovementResult = TileIsPassThroughRestrictedMovement(node.DefaultTile, neighborPairs);
				if (pathThroughRestrictedMovementResult.Status == ResultStatus.EXCEPTION) throw pathThroughRestrictedMovementResult.ex;
				if (pathThroughRestrictedMovementResult.Result) return pathThroughRestrictedMovementResult;

				var narrowGeogResult = TileIsNarrowGeography(node.DefaultTile, neighborPairs);
				if (narrowGeogResult.Status == ResultStatus.EXCEPTION) throw narrowGeogResult.ex;
				if (narrowGeogResult.Result) return narrowGeogResult;
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				return r;
			}
			
			// Is not a chokepoint
			r.Messages.Add("Tile is not a chokepoint.");
			return r;
		}


		[OperationBehavior]
		public IResult<bool, ITile> TileIsPassThroughRestrictedMovement(ITile tile, List<Tuple<ITile, ITile>> neighborPairs)
		{
			var r = new OperationResult<bool, ITile> { Status = ResultStatus.SUCCESS, Result = false };
			
			// We're concerned with whether a non-move tile has an override for units that move based on the geography
			// namely land and water units. water spaces should never have a movement adjustment less than 0 so checking
			// here is just a fail-safe.
			var checkUnitGeogTypes = ComponentRepository.GetUnitGeogTypes()
												.Where(ugt => ugt.Name.ToLowerInvariant() == "land" || ugt.Name == "water-surface")
												.Select(ugt => ugt.ToComponent());
			var hasOverride = false;

			foreach(var ugt in checkUnitGeogTypes)
			{
				var overrideResult = TileHasMovementOverrideForUnitGeogType(ugt, tile);
				if (overrideResult.Result)
				{
					hasOverride = true;
					break;
				}
			}			

			// Mountain Pass/ or pass through any other restricted movement areas
			try
			{
				var isPathThroughRestrictedMovement =	(tile.NetMovementAdjustment >= 0 || hasOverride)  && 
														(neighborPairs.Any(p =>  
																// One is null. The other has movement restrictions		
																(p.Item1 == null && p.Item2 != null) && (p.Item2.NetMovementAdjustment < 0)
																||
																(p.Item2 == null && p.Item1 != null) && (p.Item1.NetMovementAdjustment < 0)
																|| // Neither are null and both have restricted movement
																(p.Item1 != null && p.Item2 != null) && (p.Item1.NetMovementAdjustment < 0 && p.Item2.NetMovementAdjustment < 0)));

				if (isPathThroughRestrictedMovement)
				{
					r.Result = true;
					r.Messages.Add("Tile allows for movement through otherwise movement restricted area.");
					return r;
				}

				return r;
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				return r;
			}
		}


		[OperationBehavior]
		public IResult<bool, ITile> TileIsNarrowGeography(ITile tile, List<Tuple<ITile, ITile>> neighborPairs)
		{
			var r = new OperationResult<bool, ITile> { Status = ResultStatus.SUCCESS, Result = false};

			// Narrow Land/Waterway (base geog)
			// Either of the pair tiles' base geography can not match the source node
			// The pair tiles MUST match each other. One can be a hybrid.
			try
			{
				var isNarrowGeog = neighborPairs.Any(p => (p.Item1 == null && p.Item2 == null)		// both are null - corner node
				                                            ||	// One is null and the other is either a hybrid or does not match the source
				                                            (p.Item1 == null && p.Item2 != null) &&
				                                            (p.Item2.BaseGeography.Any(g => g.IsHybrid()) || !p.Item2.BaseGeography.Any(g => tile.BaseGeography.Contains(g)))
				                                            ||
				                                            (p.Item2 == null && p.Item1 != null) &&
				                                            (p.Item1.BaseGeography.Any(g => g.IsHybrid()) || !p.Item1.BaseGeography.Any(g => tile.BaseGeography.Contains(g)))
				                                            ||	// Neither are null and both opposites are hybrids
				                                            (p.Item1 != null && p.Item2 != null) &&
				                                            p.Item2.BaseGeography.Any(g => g.IsHybrid()) && p.Item1.BaseGeography.Any(g => g.IsHybrid())
				                                            ||	// Neither are null and neither match the source
				                                            (p.Item1 != null && p.Item2 != null) &&
				                                            (p.Item1.BaseGeography.Any(g => g.IsHybrid()) || !p.Item1.BaseGeography.Any(g => tile.BaseGeography.Contains(g))) &&
				                                            (p.Item2.BaseGeography.Any(g => g.IsHybrid()) || !p.Item2.BaseGeography.Any(g => tile.BaseGeography.Contains(g))));

				if (isNarrowGeog)
				{
					r.Result = true;
					r.Messages.Add("Tile geography is narrow.");
					return r;
				}

				return r;
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				return r;
			}
		}

		[OperationBehavior]
		public IResult<bool, Tuple<IUnit, ITile>> TileIsAllowableForUnit(IUnit unit, ITile tile)
		{
			var r = new OperationResult<bool, Tuple<IUnit, ITile>>{Status = ResultStatus.SUCCESS, Result = true};

			var rGeog = TileIsAllowableForUnitType(unit.UnitInfo.UnitType, tile).Result;
			var rOverride = TileHasMovementOverrideForUnitType(unit.UnitInfo.UnitType, tile, Direction.NONE);

			r.Result = (rGeog || rOverride.Result);

			return r;
		}

		[OperationBehavior]
		public IResult<bool, Tuple<IUnitType, ITile>> TileIsAllowableForUnitType(IUnitType unitType, ITile tile)
		{
			var r = new OperationResult<bool, Tuple<IUnitType, ITile>>{Status = ResultStatus.SUCCESS, Result = true};

			try
			{
				var tileGeogTypeIDs = tile.AllGeography.Select(g => g.DemographicClass.ID).ToArray();
				var unitGeogTypeIDs = TheGame().JTSServices.DataService.LookupUnitGeogTypesByBaseTypes(new[] {unitType.BaseType.ID});
				var demoClassIDs = TheGame().JTSServices.DataService.LookupDemographicClassesByUnitGeogTypes(unitGeogTypeIDs).ToArray();

				// Tile has at least one geographic demographic compatible with this unit, a movement override or no geographic demos
				r.Result = (demoClassIDs.Any(tileGeogTypeIDs.Contains) ||
							demoClassIDs.Any(dID => dID == 9999) || // Applies to all records in a table
							!tileGeogTypeIDs.Any());

				return r;
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				r.Result = false;
				return r;
			}
		}

		[OperationBehavior]
		public IResult<bool, Tuple<IUnitType, ITile>> TileHasMovementOverrideForUnit(IUnit unit, ITile tile, Direction direction)
		{
			var r = TileHasMovementOverrideForUnitType(unit.UnitInfo.UnitType, tile, direction);
			return r;
		}

		[OperationBehavior]
		public IResult<bool, IUnit> UnitHasGlobalMovementOverride(IUnit unit)
		{
			var r = new OperationResult<bool, IUnit>{Status = ResultStatus.SUCCESS, Result = false};

			try
			{
				var unitGeogTypeIDs = unit.UnitInfo.UnitType.BaseType.SupportedUnitGeographyTypes;
				var overrides = DataRepository.GetUnitGeogTypeMovementOverrides().ToArray();

				// Global override
				if (overrides.Any(ov => unitGeogTypeIDs.Contains((int)ov.UnitGeogType) &&
								((int)ov.Geography == 9999 & (int)ov.Infrastructure == 9999)))
				{
					r.Result = true;
				}
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				r.Result = false;
			}

			return r;
		}

		[OperationBehavior]
		public IResult<bool, Tuple<IUnitGeogType, ITile>> TileHasMovementOverrideForUnitGeogType(IUnitGeogType unitGeogType, ITile tile)
		{
			var r = new OperationResult<bool, Tuple<IUnitGeogType, ITile>>{Status = ResultStatus.SUCCESS, Result = false};

			foreach (var direction in Utility.Orienting.GetAllDirections())
			{
				var result = new OperationResult<bool, Tuple<IUnitType, ITile>>{Status = ResultStatus.SUCCESS, Result = false};
			
				try
				{
					var tileGeogTypeIDs = tile.AllGeography.Select(g => g.DemographicClass.ID);
					var tileInfraTypeIDs = tile.Infrastructure.Select(i => i.DemographicClass.ID);
					var overrides = DataRepository.GetUnitGeogTypeMovementOverrides().ToArray();

					// Global override
					if (overrides.Any(ov => unitGeogType.ID == ((int)ov.UnitGeogType) &&
									((int)ov.Geography == 9999 & (int)ov.Infrastructure == 9999)))
					{
						r.Result = true;
						return r;
					}

					// Geography override
					if (overrides.Any(ov => unitGeogType.ID == ((int)ov.UnitGeogType) && 
											tileInfraTypeIDs.Contains((int)ov.Infrastructure) &&
											((int)ov.Geography == 9999)))
					{
						r.Result = true;
						return r;
					}

					// Infrastructure override
					if (overrides.Any(ov => unitGeogType.ID == ((int)ov.UnitGeogType) &&
											tileGeogTypeIDs.Contains((int)ov.Geography) &&
											((int)ov.Infrastructure == 9999)))
					{
						r.Result = true;
						return r;
					}

					var q = from ov in overrides
							join tgt in tileGeogTypeIDs 
								on (int)ov.Geography equals tgt
							join tit in tileInfraTypeIDs 
								on (int)ov.Infrastructure equals tit
								where (direction == Direction.NONE || tile.Infrastructure.Any(i => i.Orientation.Any(o => o.Equals(direction))))
								where (unitGeogType.ID == (int)ov.UnitGeogType)
							select new {ov.UnitGeogType, ov.Geography, ov.Infrastructure};


					if (q.Any())
					{
						r.Result = true;
						return r;
					}
				}
				catch (Exception ex)
				{
					r.Status = ResultStatus.EXCEPTION;
					r.ex = ex;
					r.Result = false;
					return r;
				}
			}

			return r;
		}

		[OperationBehavior]
		public IResult<bool, Tuple<IUnitType, ITile>> TileHasMovementOverrideForUnitType(IUnitType unitType, ITile tile, Direction direction)
		{
			var r = new OperationResult<bool, Tuple<IUnitType, ITile>>{Status = ResultStatus.SUCCESS, Result = false};
			
			try
			{
				var unitGeogTypeIDs = unitType.BaseType.SupportedUnitGeographyTypes;
				var tileGeogTypeIDs = tile.AllGeography.Select(g => g.DemographicClass.ID);
				var tileInfraTypeIDs = tile.Infrastructure.Select(i => i.DemographicClass.ID);

				var overrides = DataRepository.GetUnitGeogTypeMovementOverrides().ToArray();

				// Global override
				if (unitType.HasGlobalMovementOverride)
				{
					r.Result = true;
					return r;
				}

				// Geography override
				if (overrides.Any(ov => unitGeogTypeIDs.Contains((int)ov.UnitGeogType) && 
										tileInfraTypeIDs.Contains((int)ov.Infrastructure) &&
										((int)ov.Geography == 9999)))
				{
					//Check the orientation of the infrastrucure
					if (direction == Direction.NONE ||
					    tile.Infrastructure.Any(i => i.Orientation.Any(o => o.Equals(direction))))
					{
						r.Result = true;
						return r;
					}
				}

				// Infrastructure override
				if (overrides.Any(ov => unitGeogTypeIDs.Contains((int)ov.UnitGeogType) &&
										tileGeogTypeIDs.Contains((int)ov.Geography) &&
										((int)ov.Infrastructure == 9999)))
				{
					r.Result = true;
					return r;
				}

				var q = from ov in overrides
						join ugt in unitGeogTypeIDs 
							on (int)ov.UnitGeogType equals ugt
						join tgt in tileGeogTypeIDs 
							on (int)ov.Geography equals tgt
						join tit in tileInfraTypeIDs 
							on (int)ov.Infrastructure equals tit
							where (direction == Direction.NONE || tile.Infrastructure.Any(i => i.Orientation.Any(o => o.Equals(direction))))
						select new {ov.UnitGeogType, ov.Geography, ov.Infrastructure};


				r.Result = q.Any();
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				r.Result = false;
				return r;
			}

			return r;
		}
		

	// Unit

		[OperationBehavior]
		public IResult<int, IUnit> GetAllowableDeployDistanceForTransport(IUnit transport)
		{
			var r = new OperationResult<int, IUnit> {Status = ResultStatus.SUCCESS};

			try
			{
				r.Result = (transport.IsUnitBaseType("Watercraft") || transport.IsUnitBaseType("Watercraft-sub")) ? 1 : 0;
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
			}

			return r;
		}

		[OperationBehavior]
		public IResult<int, IUnit> GetAllowableLoadDistanceForTransport(IUnit transport)
		{
			var r = new OperationResult<int, IUnit> {Status = ResultStatus.SUCCESS};

			try
			{
				r.Result = (transport.IsUnitBaseType("Watercraft") || transport.IsUnitBaseType("Watercraft-sub")) ? 1 : 0;
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
			}

			return r;
		}

		[OperationBehavior]
		public IResult<bool, string> UnitNameIsUnique(string name)
		{
			var r = new OperationResult<bool, string>();
			r.Result = ComponentRepository.GetUnits().All(m => m.Name.ToLowerInvariant() != name.ToLowerInvariant());
			return r;
		}

		[OperationBehavior]
		public IResult<bool, Tuple<List<IUnit>, List<IUnit>>> UnitsCanDoBattleWithUnits(List<IUnit> units, List<IUnit> opponents, BattleType battleType)
		{

			var r = new OperationResult<bool, Tuple<List<IUnit>, List<IUnit>>> {Status = ResultStatus.SUCCESS, Result = false};

			units.ForEach(u => opponents.ForEach(o =>
				{
					if (UnitCanDoBattleWithUnit(u, o, battleType).Result)
					{
						r.Result = true;
					}
				}));

			return r;
		}

		[OperationBehavior]
		public IResult<bool, Tuple<IUnit, IUnit>> UnitCanDoBattleWithUnit(IUnit unit, IUnit opponent, BattleType battleType)
		{
			var r = new OperationResult<bool, Tuple<IUnit, IUnit>> { Status = ResultStatus.SUCCESS };

			// This isn't actually necessary as local battles will always have a distance of 0
			// I'm leaving this in here to
			//		a) as a gate to save some processing time
			//		b) assist in testing where the sim may be assembling units at different locations to simulate a local battle
			if (battleType == BattleType.BARRAGE)
			{
				// Check distance first. Local battles should all pass with distance of 0
				// else, opponent (target) needs to be within the range of the remote attack distance for the attacker (source)
				var distanceResult = TheGame().JTSServices.AIService.CalculateNodeCountToUnit(unit, opponent, unit.RemoteAttackDistance);
				if (distanceResult.Status == ResultStatus.FAILURE)
				{
					r.Result = false;
					r.Messages.Add(distanceResult.Message);
					return r;
				}
			}			

			// Within range, check other rules

			IEnumerable<int> battleAllowedGeogTypeIDs = DataRepository.GetUnitBattleEffectiveLookup()
																.Where(ube => ube.UnitType == unit.UnitInfo.UnitType.ID)
																.Where(ube => ube.UnitClass == unit.UnitInfo.UnitClass.ID || ube.UnitClass == 9999)
																.Select(ube => Convert.ToInt32(ube.UnitGeogType as int?)).ToArray();

			var defenderGeogTypes = TheGame().JTSServices.DataService.LookupUnitGeogTypesByBaseTypes(new[] {opponent.UnitInfo.UnitType.BaseType.ID}).ToArray();
			//var defenderGeogTypes = opponent.UnitInfo.UnitType.BaseType.SupportedUnitGeographyTypes;

			r.Result =	battleAllowedGeogTypeIDs.Any(defenderGeogTypes.Contains) ||
						battleAllowedGeogTypeIDs.Any(id => defenderGeogTypes.Contains(9999)) ||
						battleAllowedGeogTypeIDs.Any(id => id == 9999);


			return r;
		}

		[OperationBehavior]
		public IResult<bool, IUnit> UnitCanDoBattle(IUnit unit)
		{
			var r = new OperationResult<bool, IUnit>();

			r.Result =	((unit.CurrentMoveStats.RemoteFirePoints > 0) || 
						!unit.CurrentMoveStats.HasPerformedAction);

			return r;
		}

		[OperationBehavior]
		public IResult<bool, IUnit> UnitCanMoveOntoNode(IUnit unit, INode target)
		{
			var r = new OperationResult<bool, IUnit>();

			var currentNode = unit.GetNode();
			var direction = TheGame().JTSServices.NodeService.GetNodeDirectionFromNeighborSourceNode(target, currentNode);
			var dResult = TheGame().JTSServices.RulesService.UnitCanMoveInDirection(unit, currentNode.DefaultTile, direction).Result;
			r.Result = (unit.CurrentMoveStats.MovementPoints > 0) && (dResult.CanMoveInDirection || dResult.HasMovementOverrideInDirection);

			return r;
		}

		[OperationBehavior]
		public IResult<bool, IUnit> UnitCanClaimNodeForFaction(IUnit unit)
		{
			//For now, unit must only not be an air unit
			var r = new OperationResult<bool, IUnit>();
			r.Result = (!unit.IsUnitBaseType("Plane") && !unit.IsUnitBaseType("Helicopter"));
			return r;
		}

		[OperationBehavior]
		public IResult<bool, Tuple<IUnit, IUnit>> UnitCanAttachToUnit(IUnit unit, IUnit attachToUnit)
		{
			var r = new OperationResult<bool, Tuple<IUnit, IUnit>>();
			r.Status = ResultStatus.SUCCESS;

			// First rule : only attach to units at the next highest group type level
			r.Result = attachToUnit.UnitInfo.UnitGroupType.Equals(unit.UnitInfo.UnitGroupType.NextHighestGroupType);

			if (!r.Result)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("{0} can not be attached to {1} due to it's organization level.".F(unit.Name, attachToUnit.Name));
				return r;
			}

			return r;
		}

		[OperationBehavior]
		public IResult<bool, IUnit> UnitIsSupplied(IUnit unit)
		{
			var r = new OperationResult<bool, IUnit> {Result = false, Status = ResultStatus.SUCCESS};

			if (unit.UnitInfo.UnitType.BaseType.CanBeSupplied)
			{
				try
				{
					r.Result = (TheGame().JTSServices.AIService.FindSupplyPath(unit.GetNode(), unit.Country.Faction, BaseGamePointValues.MaxSupplyDistance) != null);
				}
				catch (Exception ex)
				{
					r.Status = ResultStatus.EXCEPTION;
					r.ex = ex;
				}
			}	

			return r;
		}

		[OperationBehavior]
		public IResult<bool, IUnit> UnitHasMedicalSupport(IUnit unit)
		{
			// Should only be determined based on medical unit or demographic that provides medical AT current node including current unit
			// Aircraft that can not land should not get medical bonus unless it is medical unit class
			var r = new OperationResult<bool, IUnit> {Result = true, Status = ResultStatus.SUCCESS};

			// Check the current unit first to save some processing
			if (unit.IsUnitClass("medical"))
			{
				r.Messages.Add("{0} is a medical unit.".F(unit.Name));
				return r;
			}

			if (!unit.UnitInfo.UnitType.BaseType.CanReceiveMedicalSupport)
			{
				r.Result = false;
				r.Messages.Add("{0} has a base type that can not receive medical suppport.".F(unit.Name));
				return r;
			}

			var units = TheGame().JTSServices.UnitService.GetAllUnits(new[] {unit.Country.Faction})
			                   .Where(u => u.Location != null &&
			                               u.LocationEquals(unit.Location));

			r.Result = units.Any(u => u.IsUnitClass("medical"));

			if (!r.Result)
			{
				var demographics = unit.GetNode().DefaultTile.GetAllDemographics().Where(d => d.ProvidesMedical);
				r.Result = demographics.Any();
			}

			return r;
		}

		[OperationBehavior]
		public IResult<bool, IUnit> UnitIsUnitClass(IUnit unit, string className)
		{
			var r = new OperationResult<bool, IUnit> {Result = false, Status = ResultStatus.SUCCESS};

			try
			{
				var uc = TheGame().JTSServices.UnitService.GetUnitClassByName(className);
				r.Result = (unit.UnitInfo.UnitClass.Equals(uc));
			}
			catch (ComponentNotFoundException ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				return r;
			}

			return r;
		}

		[OperationBehavior]
		public IResult<bool, IUnit> UnitIsUnitBaseType(IUnit unit, string baseTypeName)
		{
			var r = new OperationResult<bool, IUnit> {Result = false, Status = ResultStatus.SUCCESS};

			try
			{

					var baseType = ComponentRepository.GetUnitBaseTypes()
										.Single(ubt => ubt.Name.ToLowerInvariant() == baseTypeName.ToLowerInvariant())
										.ToComponent();					
					
					if (baseTypeName == null)
						throw new ComponentNotFoundException("UnitBaseType '{0}' not found.");

					r.Result = (unit.UnitInfo.UnitType.BaseType.Equals(baseType));

				
			}
			catch (ComponentNotFoundException ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
			}

			return r;
		}

		[OperationBehavior]
		public IResult<bool, Tuple<IUnit, INode>> UnitIsDeployableToNode(IUnit unit, INode node)
		{
			// Node must be compatible with the unit
			// Unit must have the 1 remaining movement point to move
			return TheGame().JTSServices.RulesService.NodeIsValidForMove(unit, node);
		}

		[OperationBehavior]
		public IResult<bool, Tuple<IUnit, INode>> UnitCanReinforceAtLocation(IUnit unit, INode node)
		{
			// Node must be compatible with the unit
			// Node must be a friendly
			// There must be an HQ for the player at the location
			//		or in the case of placing an HQ, there must be friendly units
			// For planes, there must be an airport
			// For HQ units, there must be either a military base or a command post
			var r = new OperationResult<bool, Tuple<IUnit, INode>>
				{
					Result = true, 
					Status = ResultStatus.SUCCESS
				};

			var playerUnits = node.GetAllUnits().Where(u => u.Country.Equals(unit.Country));

			var tileValidResult = TheGame().JTSServices.RulesService.TileIsAllowableForUnit(unit, node.DefaultTile);

			if (!tileValidResult.Result)
			{
				r.Result = false;
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Selected location is not allowable for unit.");
				return r;
			}

			if (!node.IsFriendly())
			{
				r.Result = false;
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Selected location is not friendly.");
				return r;
			}

			// planes have to reinforce at an airport
			if (unit.IsUnitBaseType("plane") && !node.DefaultTile.Infrastructure.Any(i => i.IsDemographicClass("Airport")))
			{
				r.Result = false;
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("There is no airport at the selected location.");
				return r;
			}

			// HQ class units must be placed at a location with a military base or command post
			if (unit.IsUnitClass("HQ") &&
			    !node.DefaultTile
			         .Infrastructure.Any(i => i.IsDemographicClass("militarybase") || i.IsDemographicClass("commandpost")))
			{
				r.Result = false;
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("There is no military installment at the selected location for an HQ unit.");
				return r;	
			}

			if (!unit.IsUnitClass("HQ") && !playerUnits.Any(u => u.IsUnitClass("HQ")))
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Selected location has no HQ for {0}.".F(unit.Country.Name));
				r.Result = false;
				return r;
			}

			r.Messages.Add("{0} is deployable to selected location".F(unit.Name));
			return r;
		}

		[OperationBehavior]
		public IResult<bool, Tuple<IUnit, INode>> UnitCanRefuelAtLocation(IUnit unit, INode node)
		{
			// Unit is supplied - assumes fuel supply
			// Location must be friendly
			// Location must be occupiable by the unit
			// Location contains an appropriate refueling facility for unit
			// Location contains a unit on which the refueling unit can be transported
			//		Logic behind this is that if a unit can be transported by another unit, the transport would carry fuel for said unit
			//		in the case of infantry, since fuel is food, same logic should apply.
			// TODO: We may want to re-visit this and create a refueling compatibility lookup instead

			var r = new OperationResult<bool, Tuple<IUnit, INode>>
				{
					Result = true, 
					Status = ResultStatus.SUCCESS,
				};
			

			var playerUnits = node.GetAllUnits().Where(u => u.Country.Equals(unit.Country)).ToArray();
			var tileValidResult = TheGame().JTSServices.RulesService.TileIsAllowableForUnit(unit, node.DefaultTile);

			// We can be refueled by another unit.
			// Tile is assumed to be OK for unit or there would be no playerUnits to check
			if (playerUnits.Any(unit.IsCompatibleWithTransport) || unit.IsSupplied())
			{
				r.Messages.Add("{0} was refueled.".F(unit.Name));
				return r;
			}
			
			if (!tileValidResult.Result)
			{
				r.Result = false;
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("{0} can not refuel because it can not occupy the location.".F(unit.Name));
				return r;
			}

			if (!node.IsFriendly())
			{
				r.Result = false;
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("{0} can not refuel because the location is not friendly.".F(unit.Name));
				return r;
			}

			// planes have to refuel at an airport 
			if (unit.IsUnitBaseType("plane") && !node.DefaultTile.Infrastructure.Any(i => i.IsDemographicClass("Airport")))
			{
				r.Result = false;
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("There is no suitable facility to refuel {0}.".F(unit.Name));
				return r;
			}

			// helicopters can refuel at an airport or military base 
			if (unit.IsUnitBaseType("helicopter") && 
				(!node.DefaultTile.Infrastructure.Any(i => i.IsDemographicClass("airport") || i.IsDemographicClass("militarybase"))))
			{
				r.Result = false;
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("There is no suitable facility to refuel {0}.".F(unit.Name));
				return r;
			}

			r.Messages.Add("{0} was refueled.".F(unit.Name));
			return r;
		}
			
		[OperationBehavior]
		public IResult<bool, Tuple<IUnit, IUnit>> UnitCanTransportUnitTypeAndClass(IUnit transport, IUnit unit)
		{
			var r = new OperationResult<bool, Tuple<IUnit, IUnit>> {Result = false, Status = ResultStatus.SUCCESS};

			r.Result = DataRepository.GetUnitTransportUnitTypeUnitClasses().Any(te => ((int)te.TransportUnitType == transport.UnitInfo.UnitType.ID &&
																				((int)te.CarriedUnitType == unit.UnitInfo.UnitType.ID || (int)te.CarriedUnitType == 9999) &&
																				((int)te.CarriedUnitClass == unit.UnitInfo.UnitClass.ID || (int)te.CarriedUnitClass == 9999)));

			return r;
		}

		[OperationBehavior]
		public IResult<MoveInDirectionResult, Tuple<IUnit, INode>> UnitCanMoveInDirection(IUnit unit, ITile currentNodeTile, Direction direction)
		{
			var r = new OperationResult<MoveInDirectionResult, Tuple<IUnit, INode>>
								{
									Status = ResultStatus.SUCCESS,
									Result = new MoveInDirectionResult { CanMoveInDirection = true, HasMovementOverrideInDirection = false }
								};

			//var unitGeog = TheGame().JTSServices.DataService.LookupUnitGeogTypesByBaseTypes(new [] {unit.UnitInfo.UnitType.BaseType.ID});
			var unitGeog = unit.UnitInfo.UnitType.BaseType.SupportedUnitGeographyTypes;

			var	targetNode = currentNodeTile.GetNode().NeighborNodes.SingleOrDefault(n => n.Direction == direction).Node;

			if (targetNode == null) // At the board edge
			{
				r.Result.CanMoveInDirection = false;
				return r;
			}

			// Do check on demographics first
			// Check for configured hinderances
			var currentHinderanceDemographics = currentNodeTile.GetAllDemographics()
														.Where(d => d.DemographicClass.MovementHinderanceConfigured)
														.Where(d =>		!currentNodeTile.Infrastructure.Contains(d) ||							// is not Infrastructure OR
																		(d.Orientation.Any() &&	d.Orientation.Any(o => o != Direction.NONE)))	// Has orientation and orientation is not none
															.Select(d => d.DemographicClass.ID).ToArray();

			var canExitCurrentNode = (!currentHinderanceDemographics.Any()) || !DataRepository.GetMovementHinderanceInDirection()
			                                                                                   .Any(h => ((currentHinderanceDemographics.Contains((int)h.DemographicClass))) &&
			                                                                                             (unitGeog.Contains((int)h.UnitGeogType) || (int)h.UnitGeogType == 9999) &&
			                                                                                             ((Direction)h.Direction == direction));

			// Then Check for overrides
			if (!canExitCurrentNode)
				canExitCurrentNode = r.Result.HasMovementOverrideInDirection = TheGame().JTSServices.RulesService.TileHasMovementOverrideForUnit(unit, currentNodeTile, direction).Result;

			// If we can't exit the node, then we don't need to waste cycles checking the target node
			if (!canExitCurrentNode)
			{
				r.Result.CanMoveInDirection = false;
				return r;
			}
				
			var targetTile = targetNode.DefaultTile;

			// Check for configured hinderances
			var targetHinderanceDemographics = targetTile.GetAllDemographics()
														.Where(d => d.DemographicClass.MovementHinderanceConfigured)
														.Where(d =>		!targetTile.Infrastructure.Contains(d) ||								// is not Infrastructure OR
																		(d.Orientation.Any() &&	d.Orientation.Any(o => o != Direction.NONE)))	// Has orientation and orientation is not none
															.Select(d => d.DemographicClass.ID).ToArray();

			var canEnterTargetNode = (!targetHinderanceDemographics.Any()) 
				                          ? TileIsAllowableForUnitType(unit.UnitInfo.UnitType, targetTile).Result				
				                          : !DataRepository.GetMovementHinderanceInDirection()
				                                           .Any(h => ( (targetHinderanceDemographics.Contains((int)h.DemographicClass)) &&
				                                                       (unitGeog.Contains((int)h.UnitGeogType) || (int)h.UnitGeogType == 9999) &&
				                                                       ((Direction)h.Direction == direction.Reverse())));

			// Then Check for overrides
			if (!canEnterTargetNode)
				canEnterTargetNode = r.Result.HasMovementOverrideInDirection = TheGame().JTSServices.RulesService.TileHasMovementOverrideForUnit(unit, targetTile, direction.Reverse()).Result;


			if (canEnterTargetNode)
				canEnterTargetNode = !targetTile.HasMaxUnits(unit.Country.Faction);			

			
			r.Result.CanMoveInDirection = canEnterTargetNode;
			return r;
		}

		[OperationBehavior]
		public IResult<bool, Tuple<ITile, IFaction>> TileHasMaxUnitsForFaction(ITile tile, IFaction faction, int MaxUnits)
		{
			var r = new OperationResult<bool, Tuple<ITile, IFaction>> { Status = ResultStatus.SUCCESS, Result = false };

			try
			{
				var units = tile.GetAllUnits().Where(u => u.Country.Faction.Equals(faction));
				r.Result = (units.Count() >= MaxUnits);
				return r;
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				return r;
			}
		}

		[OperationBehavior]
		public IResult<bool, ITile> TileWillExceedMaxUnitsForFaction(ITile tile, IEnumerable<IUnit> movingUnits, int maxUnits)
		{
			movingUnits = movingUnits.ToArray();
			var r = new OperationResult<bool, ITile> { Status = ResultStatus.SUCCESS, Result = false };
			var f = movingUnits.First().Country.Faction;

			try
			{
				var units = tile.GetAllUnits().Where(u => u.Country.Faction.Equals(f));
				r.Result = ((units.Count() + movingUnits.Count()) >= maxUnits);
				return r;
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				return r;
			}
		}

		[OperationBehavior]
		public IResult<bool, ITile> TileCanSupportInfrastructureBuilding(ITile tile, IDemographic infrastructure)
		{
			// true if Tile does not have existing infrastructure of requested type in any of the allowed orientations
			var result = new OperationResult<bool, ITile>();

			if (infrastructure == null || !infrastructure.IsDemographicType("Infrastructure"))
			{
				result.Status = ResultStatus.FAILURE;
				result.Messages.Add("Argument null or of incorrect type.");
				return result;
			}
			
			try
			{
				result.Result = TheGame().JTSServices.TileService.GetOrientationAllowableForDemographicClassByTile(tile, infrastructure.DemographicClass).Any();
				result.Status = ResultStatus.SUCCESS;
				result.SuccessfulObjects.Add(tile);
			}
			catch (Exception ex)
			{
				result.ex = ex;
				result.Status = ResultStatus.EXCEPTION;
				result.FailedObjects.Add(tile);
			}			

			return result;
		}

		[OperationBehavior]
		public IResult<bool, Tuple<IUnit, IUnitTaskType>> UnitCanPerformTask(IUnit unit, IUnitTaskType taskType)
		{
			// Based on allowable task assignments for a UnitClass and UnitGroupType
			var r = new OperationResult<bool, Tuple<IUnit, IUnitTaskType>> { Status = ResultStatus.SUCCESS, Result = false};

			try
			{
				bool unitClassCanPerformTask = DataRepository.GetUnitTaskTypeUnitClassesLookup()
				                                             .Any(utuc => utuc.UnitTask == taskType.ID && 
				                                                          (utuc.UnitClass == unit.UnitInfo.UnitClass.ID || utuc.UnitClass == 9999));
				bool unitGroupTypeCanPerformTask = DataRepository.GetUnitGroupTypeUnitTaskTypeLookup()
																.Any(ugtut => ugtut.UnitTask == taskType.ID && 
																	(ugtut.UnitGroupType == unit.UnitInfo.UnitGroupType.ID || ugtut.UnitGroupType == 9999));

				if (!unitClassCanPerformTask)
				{
					r.Messages.Add("{0} can not perform task {1} due to its unit class.".F(unit.Name, taskType.Name));
					return r;
				}

				if (!unitGroupTypeCanPerformTask)
				{
					r.Messages.Add("{0} can not perform task {1} due to its unit group type.".F(unit.Name, taskType.Name));
					return r;
				}

				r.Result = true;
				r.Messages.Add("{0} is allowable for {1}.".F(taskType.Name, unit.Name));
				return r;

			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				return r;
			}	
		}

		[OperationBehavior]
		public IResult<bool, IMoveableComponent> ComponentIsVisible(IMoveableComponent component)
		{
			var r = new OperationResult<bool, IMoveableComponent> {Result = false, Status = ResultStatus.SUCCESS};
			r.Result = (!component.IsBeingTransported());
			return r;
		}

		/// <summary>
		/// Determines whether a unit should be displayed during an opponent's turn
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="hiddenStealthThreshhold"></param>
		/// <returns></returns>
		[OperationBehavior]
		public IResult<bool, IUnit> UnitIsHiddenFromEnemy(IUnit unit, double hiddenStealthThreshhold)
		{
			var r = new OperationResult<bool, IUnit> {Result = false, Status = ResultStatus.SUCCESS};
			r.Result = (!Convert.ToBoolean(ConfigurationManager.AppSettings["show_hidden_units"]) &&
						!unit.IsFriendly() && 
						(unit.GetFullNetStealthValue() >= hiddenStealthThreshhold));
			return r;
		}

		[OperationBehavior]
		public IResult<bool, IMoveableComponent> ComponentIsBeingTransported(IMoveableComponent component)
		{
			var r = new OperationResult<bool, IMoveableComponent> {Result = false, Status = ResultStatus.SUCCESS};
			r.Result = DataRepository.GetUnitTransports().Any(ut => ut.Unit == component.ID);
			return r;
		}

		[OperationBehavior]
		public IResult<bool, IUnitType> UnitTypeIsAllowedTypeForScenario(IUnitType unitType)
		{
			var r = new OperationResult<bool, IUnitType> {Result = false, Status = ResultStatus.SUCCESS};
			r.Result = DataRepository.GetAllowedUnitTypes().Any(ut => ut.UnitType == unitType.ID);
			return r;
		}

		[OperationBehavior]
		public IResult<bool, IUnitGroupType> UnitGroupTypeIsAllowedForScenario(IUnitGroupType unitGroupType)
		{
			var r = new OperationResult<bool, IUnitGroupType> {Result = false, Status = ResultStatus.SUCCESS};
			r.Result = DataRepository.GetAllowedUnitGroupTypes().Any(ut => ut.UnitType == unitGroupType.ID);
			return r;
		}


	// Game

		[OperationBehavior]
		public IResult<bool, IFaction> GameVictoryAchieved(IFaction faction)
		{
			var conditions = faction.VictoryConditions();
			var r = new OperationResult<bool, IFaction>{Status = ResultStatus.SUCCESS, Result = false};

			//if (conditions == null || conditions.Count == 0)
			//	throw new Exception("{0} has no victory condition and can not win the game.".F(faction.Name));

			// Check each condition based on its own set of rules
			
			foreach(var vc in conditions)
			{
				switch (vc.ConditionType)
				{
					case GameVictoryCondition.ENEMY_UNITS_REMAINING :
						{
							var enemyFactions = ComponentRepository.GetFactions()
														.Where(f => !f.ToComponent().Equals(faction))
														.Select(f => f.ToComponent());

							var enemyUnits = TheGame().JTSServices.UnitService.GetAllUnits(enemyFactions);
								
								
							r.Result = enemyUnits.Count() <= vc.Value;

							// Condition met... we're good
							if (r.Result) return r;

							break;
						}
					case GameVictoryCondition.VICTORY_POINTS_HELD :
						{
							r.Result = faction.GetCurrentVictoryPoints() >= vc.Value;

							// Condition met... we're good
							if (r.Result) return r;

							break;
						}
					case GameVictoryCondition.FLAG_CAPTURED :
						{
							var enemyFactions = ComponentRepository.GetFactions()
														.Where(f => !f.ToComponent().Equals(faction))
														.Select(f => f.ToComponent());

								
							// Get all the remaining flag-holders for all factions
							var enemyUnits = TheGame().JTSServices.UnitService.GetAllUnits(enemyFactions)
														.Where(u => u.ID == vc.Value);	


							// if there are none, all flags are captured
							r.Result = !enemyUnits.Any();
								
							// Condition met
							if (r.Result) return r;

							break;
						}
				}

			}
			
			// No victory conditions met
			return r;
		}


		[OperationBehavior]
		public IResult<bool, TComponent> NameIsValid<TComponent>(string name)
			where TComponent : class, IBaseComponent
		{
			var r = new OperationResult<bool, TComponent>
				{
					Status = ResultStatus.SUCCESS, 
					Result = true
				};

			var reg = new Regex("^[-_//0-9a-zA-Z ]*$");

			if (name == string.Empty)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Name is empty.");
				r.Result = false;
				return r;
			}
			
			if (!reg.IsMatch(name))
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Name contains special characters.");
				r.Result = false;
				return r;
			}

			var component = GenericComponentService.Instance.GetByName<TComponent>(name);
			var nameInUse = (component != null);

			if (nameInUse)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Name currently in use.");
				r.Result = false;
				return r;
			}

			r.Messages.Add("Name is valid");
			return r;
		}

		[OperationBehavior]
		public IResult<bool, string> ScenarioTitleIsValid(string title)
		{

			var r = new OperationResult<bool, string>
				{
					Status = ResultStatus.SUCCESS, 
					Result = true
				};

			bool validScenario = (ComponentRepository.GetScenarios().Any(f => f.Name.ToLowerInvariant().Equals(title.ToLowerInvariant())));

			if (!validScenario)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("{0} is not a currently available scenario.".F(title));
				r.Result = false;
				return r;
			}
			
			r.Messages.Add("Scenario title is valid");
			return r;
		}

		[OperationBehavior]
		public IResult<bool, ICountry> CountryHasDependantComponents(ICountry country)
		{
			var r = new OperationResult<bool, ICountry>
				{
					Status = ResultStatus.SUCCESS, 
					Result = false
				};

			//Node, Tile, Unit, UnitStack
			var nodes = ComponentRepository.GetNodes().Select(n => n.ToComponent()).Where(n => n.Country.Equals(country));
			var tiles = ComponentRepository.GetTiles().Select(t => t.ToComponent()).Where(t => t.Country.Equals(country));
			var units = ComponentRepository.GetUnits().Select(u => u.ToComponent()).Where(u => u.Country.Equals(country));

			var hasDependencies = (nodes.Any() || tiles.Any() || units.Any());

			if (hasDependencies)
			{
				r.Messages.Add("{0} currently has dependant components.".F(country.Name));
				r.Result = true;
				return r;
			}
			
			r.Messages.Add("{0} has no dependant components.".F(country.Name));
			return r;
		}


	// Battle

		[OperationBehavior]
		public IResult<ISkirmish, ISkirmish> CheckSkirmishVictoryCondition(ISkirmish skirmish)
		{
			var r = new OperationResult<ISkirmish, ISkirmish> {Status = ResultStatus.SUCCESS, Result = skirmish};

			var attackerDestroyed = (skirmish.Destroyed.Any() && skirmish.Destroyed.Contains(skirmish.Attacker));
			var defenderDestroyed = (skirmish.Destroyed.Any() && skirmish.Destroyed.Contains(skirmish.Defender));

			if (attackerDestroyed && defenderDestroyed)
			{
				skirmish.VictoryCondition = BattleVictoryCondition.ALL_DESTROYED;
				r.Messages.Add("Both units were destroyed.");
				return r;
			}

			if (attackerDestroyed)
			{
				skirmish.VictoryCondition = BattleVictoryCondition.DEFENDERS_VICTORIOUS;
				r.Messages.Add("{0} was destroyed.".F(skirmish.Destroyed.Single().Name));
				return r;
			}

			if (defenderDestroyed)
			{
				skirmish.VictoryCondition = BattleVictoryCondition.ATTACKERS_VICTORIOUS;
				r.Messages.Add("{0} was destroyed.".F(skirmish.Destroyed.Single().Name));
				return r;
			}


			if (skirmish.DefenderEvaded)
			{
				skirmish.VictoryCondition = BattleVictoryCondition.EVADED;
				r.Messages.Add("{0} evaded attack.".F(skirmish.Defender.Name));
				return r;
			}

			return r;
		}

		[OperationBehavior]
		public IResult<IBattle, IBattle> CheckBattleVictoryCondition(IBattle battle)
		{
			// - One or both of the battle factions have no remaining units
			// - No suitable defenders left for attackers 
			//		Local - surrender and retreat
			//		Remote - stalemate
			// - no victor yet

			var r = new OperationResult<IBattle, IBattle> {Status = ResultStatus.SUCCESS, Result = battle};
			
			try
			{
				if (battle.Attackers.Any() && battle.Defenders.Any())
				{
					if (!TheGame().JTSServices.RulesService.UnitsCanDoBattleWithUnits(battle.Attackers, battle.Defenders, battle.BattleType).Result)
					{
						if (battle.BattleType == BattleType.LOCAL)
						{
							// No suitable defenders to attack in a local battle.
							// This emulates that the roles would be reversed and the attackers may have no
							// Defence against the remaining defenders. Attackers will be forced to surrender
							// and retreat.
							battle.VictoryCondition = BattleVictoryCondition.SURRENDER;
							battle.VictorFaction = battle.DefenderFaction;
							return r;
						}

						if (battle.BattleType == BattleType.BARRAGE)
						{
							// In remote battle where the barrage is limited to number of available attack points
							// if attackers and defenders deplete these points and remain standing,
							// the battle is a stalemate. Victory will still be to the defender as they have held off
							// the barrage
							battle.VictoryCondition = BattleVictoryCondition.STALEMATE;
							battle.VictorFaction = battle.DefenderFaction;
							return r;
						}
						
					}

					// Battle is not over.....
					battle.VictoryCondition = BattleVictoryCondition.NO_VICTOR;
					return r;
				}

				if (!battle.Attackers.Any() && !battle.Defenders.Any())
				{
					// All units destroyed
					battle.VictorFaction = battle.DefenderFaction;
					battle.VictoryCondition = BattleVictoryCondition.ALL_DESTROYED;
					return r;
				}

				// Attackers win
				if (battle.Attackers.Any())
				{
					battle.VictoryCondition = BattleVictoryCondition.ATTACKERS_VICTORIOUS;
					battle.VictorFaction = battle.AttackerFaction;
					return r;
				}

				// Defenders win
				if (battle.Defenders.Any())
				{
					battle.VictoryCondition = BattleVictoryCondition.DEFENDERS_VICTORIOUS;
					battle.VictorFaction = battle.DefenderFaction;
					return r;
				}
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				return r;
			}

			r.Status = ResultStatus.FAILURE;
			r.Messages.Add("No victory condition could be met.");
			return r;
		}

		[OperationBehavior]
		public IResult<bool, IBattle> BattleCanContinue(IBattle battle)
		{
			var r = new OperationResult<bool, IBattle> {Status = ResultStatus.SUCCESS, Result = true};

			// Nuclear strikes go ahead unabated
			if (battle.BattleType == BattleType.NUCLEAR)
				return r;

			if (!battle.Defenders.Any())
			{
				r.Status = ResultStatus.FAILURE;
				r.Result = false;
				r.Messages.Add("No Defenders at battle location.");
				return r;
			}

			if (!battle.Attackers.Any())
			{
				r.Status = ResultStatus.FAILURE;
				r.Result = false;
				r.Messages.Add("No Attackers at battle location.");
				return r;
			}

			if (!UnitsCanDoBattleWithUnits(battle.Attackers, battle.Defenders, battle.BattleType).Result)
			{
				r.Status = ResultStatus.FAILURE;
				r.Result = false;
				r.Messages.Add("There are no attackers able to do battle with the current defenders.");
				return r;
			}

			r.Messages.Add("Battle can continue.");
			return r;
		}

	// AI

		[OperationBehavior]
		public IResult<StrategicAssessmentRating, StrategicAssessmentInfo> GetOverallRatingForStrategicAssessment(StrategicAssessmentInfo assessment)
		{
			var r = new OperationResult<StrategicAssessmentRating, StrategicAssessmentInfo> {Status = ResultStatus.SUCCESS};
			double total = 0;

			try
			{
				PropertyInfo[] props = typeof(StrategicAssessmentInfo).GetProperties()
									.Where(p => p.PropertyType == typeof(StrategicAssessmentRating)).ToArray();

				// We didn't pick up any of the assessment properties for some reason
				if (!props.Any())
				{
					r.Status = ResultStatus.FAILURE;
					r.Messages.Add("No property values could be ascertained for the assessment object.");
					return r;
				}

				Action<PropertyInfo> infoAction = p =>
				{
					lock (props)
					{
						total += Convert.ToInt32(p.GetValue(assessment, null));
					}
				};

				if (TheGame().IsMultiThreaded)
				{
					Parallel.ForEach(props, infoAction);
				}
				else
				{
					props.ForEach(infoAction);
				}

				total = (total/props.Count());
				r.Result = (StrategicAssessmentRating) Math.Round(total);
				return r;
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				return r;
			}
		}

		[OperationContract]
		public IResult<bool, IMission> MissionCanceledByMove(IMission mission)
		{
			// Is te mission complete
			// Is the mission's current task not null
			// Is the mission's current task not a move task
			// Is the mission's current task cancelable by a move order

			var r = new OperationResult<bool, IMission> { Status = ResultStatus.SUCCESS };

			if (mission.IsComplete)
			{
				r.SuccessfulObjects.Add(mission);
				r.Messages.Add("Mission is complete.");
				r.Result = true; // In this instance, the mission should have already been removed anyway
				return r;
			}

			if (mission.CurrentTask == null)
			{
				r.FailedObjects.Add(mission);
				r.Messages.Add("Mission does not have a current task.");
				r.Status = ResultStatus.FAILURE;
				return r;
			}

			var currentTaskIsMove = mission.CurrentTask.TaskType.Name == "MoveToLocation";
			r.Result = !currentTaskIsMove && mission.MissionType.CanceledByMove;
			return r;
		}


	#region Rules Calculations

		[OperationBehavior]
		public IResult<double, IUnit> CalculateTotalUnitWeight(IUnit unit)
		{
			var r = new OperationResult<double, IUnit> {Status = ResultStatus.SUCCESS};
			List<IUnit> allUnits;

			try
			{
				allUnits = unit.GetAllAttachedUnits().ToList();
				allUnits.Add(unit);
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				r.Messages.Add("Could not get attached units for {0}".F(unit.Name));
				return r;
			}

			try
			{
				r.Result = allUnits.Sum(s => CalculateUnitWeight(s).Result);
				return r;
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				return r;
			}
		}

		[OperationBehavior]
		public IResult<double, IUnit> CalculateUnitWeight(IUnit unit)
		{
			var r = new OperationResult<double, IUnit> {Status = ResultStatus.SUCCESS};

			var weightBase = TheGame().JTSServices.DataService.GetBasePointValues().WeightBase;
			r.Result = (unit.UnitInfo.UnitType.UnitWeightModifier * weightBase);

			return r;
		}

		[OperationBehavior]
		public IResult<double, IUnit> CalculateAllowableTransportWeight(IUnit unit)
		{
			var r = new OperationResult<double, IUnit>();

			var weightBase = TheGame().JTSServices.DataService.GetBasePointValues().WeightBase;
			r.Result = (unit.UnitInfo.UnitType.AllowableWeightModifier * weightBase);

			return r;
		}

		[OperationBehavior]
		public double? CalculateMovementHeuristic(IPathableObject component)
		{
			double? retVal = 0;

			//retVal = 10*
			//		 (Math.Abs(component.Location.X - component.Target.Location.X) +
			//		  Math.Abs(component.Location.Y - component.Target.Location.Y));

			retVal =	Math.Max(	Math.Abs(component.Location.X - component.Target.Location.X), 
									Math.Abs(component.Location.Y - component.Target.Location.Y));
			
			// Use cross-product to favor direct paths
			var dx1 = component.Location.X - component.Target.Location.X;
			var dy1 = component.Location.Y - component.Target.Location.Y;
			var dx2 = component.Source.Location.X - component.Target.Location.X;
			var dy2 = component.Source.Location.Y - component.Target.Location.Y;

			var cross = Math.Abs((dx1 * dy2) - (dx2 * dy1));

			//var unFriendly = (component.Target.GetNode().IsFriendly()) ? 0 : .01;

			retVal += (cross * .1);
			//retVal += (unFriendly);

			return retVal;

		}

		[OperationBehavior]
		public IResult<int, IFaction> CalculateTotalVictoryPoints(IFaction faction)
		{
			var r = new OperationResult<int, IFaction>();

			// Start off with total of faction-held nodes/tiles
			var nodes = TheGame().JTSServices.NodeService.GetAllNodes();
			var fromNodes = nodes.Where(n => n.Country.Faction.Equals(faction))
									.Sum(n => n.DefaultTile.VictoryPoints);

			// One point per average company (4 platoons). Maybe make this dynamic? Increase with experience or some such
			double fromPlatoons = (TheGame().JTSServices.UnitService.GetAllUnits(new[] { faction }).Count() / 4);

			r.Result = (fromNodes + Convert.ToInt32(Math.Round(fromPlatoons)));

			return r;			
		}

		[OperationBehavior]
		public IResult<int, IPlayer> CalculateReinforcementPointsForTurn(IPlayer player)
		{
			// Percentage of total board held by country
			// Percentage of total board held by other faction countries
			// Percentage of victory points held by country

			var r = new OperationResult<int, IPlayer>();
			var allNodes = TheGame().JTSServices.NodeService.GetAllNodes().ToArray();

			var totalNodeCount = allNodes.Count();
			var playerNodeCount = allNodes.Count(n => n.Country.Equals(player.Country));
			var otherFactionNodeCount = allNodes.Count(n => n.Country.Faction.Equals(player.Country.Faction) && 
															!n.Country.Equals(player.Country));
			var playerVictoryPoints = allNodes
										.Where(n => n.Country.Equals(player.Country))
										.Sum(n => n.DefaultTile.VictoryPoints);

			var pctOfTotalNodes_Country = (Convert.ToDouble(playerNodeCount) / Convert.ToDouble(totalNodeCount));
			var pctOfTotalNodes_Faction = (Convert.ToDouble(otherFactionNodeCount) / Convert.ToDouble(totalNodeCount));

			// Calculate points based on Base Game Point Value modifiers

			var totalPoints =	(pctOfTotalNodes_Country * BaseGamePointValues.ReinforcementCalcBaseCountry) +
								(pctOfTotalNodes_Faction * BaseGamePointValues.ReinforcementCalcBaseFaction) +
								(playerVictoryPoints * BaseGamePointValues.ReinforcementCalcBaseVP);

			r.Result = Convert.ToInt32(Math.Floor(totalPoints));
			return r;			
		}

		[OperationBehavior]
		public double CalculateUnitAttackValueForCurrentGeog(IUnit unit)
		{
			// Combat base value
			// Unit Net Attack adjustment
			// Occupied tile attack adjustment
			// HQ'd bonus
			// Supplied penalty

				var retVal =	unit.GetNetAttackAdjustment() +
								unit.GetNode().DefaultTile.NetAttackAdjustment +
								((unit.AttachedToUnit != null) ? BaseGamePointValues.HQBonus : 0) +
								(!(UnitIsSupplied(unit).Result) ? BaseGamePointValues.NotSuppliedPenalty : 0) +
								BaseGamePointValues.CombatBase;

				if (retVal > BaseGamePointValues.CombatRoll) retVal = BaseGamePointValues.CombatRoll;
				if (retVal < 1) retVal = 1;

				return Math.Floor(retVal);		
		}

		[OperationBehavior]
		public double CalculateUnitDefenceValueForCurrentGeog(IUnit unit)
		{
			// Combat base value
			// Unit Net Defence adjustment
			// Occupied tile defence adjustment
			// HQ'd bonus
			// Supplied penalty

			var retVal =	unit.GetNetDefenceAdjustment() +
							unit.GetNode().DefaultTile.NetDefenceAdjustment +
							((unit.AttachedToUnit != null) ? BaseGamePointValues.HQBonus : 0) +
							(!(UnitIsSupplied(unit).Result) ? BaseGamePointValues.NotSuppliedPenalty : 0) +
							BaseGamePointValues.CombatBase;

			if (retVal > BaseGamePointValues.CombatRoll) retVal = BaseGamePointValues.CombatRoll;
			if (retVal < 1) retVal = 1;

			return Math.Floor(retVal);		
		}

		[OperationBehavior]
		public double CalculateUnitStealthValueForCurrentGeog(IUnit unit)
		{
			// Combat base value
			// Unit Net Defence adjustment
			// Occupied tile defence adjustment
			var retVal =	unit.GetNetStealthAdjustment() +
							unit.GetNode().DefaultTile.NetStealthAdjustment +
							BaseGamePointValues.StealthBase;

			if (retVal > BaseGamePointValues.StealthRoll) retVal = BaseGamePointValues.StealthRoll;
			if (retVal < 0) retVal = 0;

			return retVal;
		}

		[OperationBehavior]
		public double CalculateUnitStrength(IUnit unit)
		{
			// Based on:
			//	unit cost factor
			//	unit movement
			//	total unit attack and defend (after adjustments)
			//	Remote fire points
			//	unit attack distance
			//	1/2 unit stealth

			var strength = 0.0;

			strength += unit.GetNetCostMultiplier();
			strength += unit.GetNetAttackAdjustment() + unit.GetNetDefenceAdjustment();
			strength += unit.MovementPoints;
			strength += unit.RemoteFirePoints;
			strength += unit.RemoteAttackDistance;
			strength += (unit.GetFullNetStealthValue() / 2);

			return strength;
		}

		[OperationBehavior]
		public double CalculateTargetDesirabilityForUnit(IUnit unit)
		{
			// bonus if unit has attached units (depletes HQ bonus for attached units)
			// bonus for medical or supply (depletes medical bonus roll and supply bonuses for other units)

			// Concept is that since Medical and supply give combat bonuses to other units, they have a priority
			// HQ units (units with units attached) give combat bonuses to others, they have a priority
			//		but based on the number of units affected
			// The rest are based on a relative 'strength' metric
			var attachedUnits = unit.GetAllAttachedUnits().ToArray();
			
			// Baseline
			var factor = unit.GetNetStrengthFactor();

			// Bonuses
			factor += (attachedUnits.Any() ? (attachedUnits.Count() * BaseGamePointValues.TargetAttachedUnitBonus) : 0);
			factor += (unit.IsUnitClass("medical")) ? BaseGamePointValues.TargetMedicalUnitBonus : 0;
			factor += (unit.IsUnitClass("supply")) ? BaseGamePointValues.TargetSupplyUnitBonus : 0;

			return factor;
		}

		[OperationBehavior]
		public IResult<double, IUnit> CalculateTotalRPCostForUnit(IUnit unit)
		{
			var r = new OperationResult<double, IUnit> { Status = ResultStatus.SUCCESS };

			try
			{
				r.Result = CalculateTotalRPByUnitTypeUnitClass(unit.UnitInfo.UnitType, unit.UnitInfo.UnitClass);
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
			}

			return r;
		}

		[OperationBehavior]
		public double CalculateTotalRPByUnitTypeUnitClass(IUnitType ut, IUnitClass uc)
		{
			// We want to allow for partially created units to display total cost (reinforcements screen)
			var utMod = (ut == null) ? 0 : ut.UnitCostModifier;
			var ucMod = (uc == null) ? 0 : uc.UnitCostModifier;

			return Math.Ceiling(((utMod + ucMod) * BaseGamePointValues.CostBase));			
		}

		[OperationBehavior]
		public int CalculateThreatDistance(int factorValue, int baseValue)
		{
			return Convert.ToInt32(Math.Round((Convert.ToDouble(factorValue) - 1) * 10 / baseValue));		
		}

		[OperationBehavior]
		public IResult<double, INode> CalculateOffensiveStrategicImportance(INode node)
		{
			var r = CalculateGeneralStrategicImportance(node);
			if (r.Status != ResultStatus.SUCCESS) { return r; }

			r.Result += node.DefaultTile.NetAttackAdjustment;

			return r;
		}

		[OperationBehavior]
		public IResult<double, INode> CalculateDefensiveStrategicImportance(INode node)
		{
			var r = CalculateGeneralStrategicImportance(node);
			if (r.Status != ResultStatus.SUCCESS) { return r; }

			r.Result += node.DefaultTile.NetDefenceAdjustment;

			return r;
		}

		[OperationBehavior]
		private IResult<double, INode> CalculateGeneralStrategicImportance(INode node)
		{
			// Just stealth for now - add more criteria later.
			var r = new OperationResult<double, INode> { Status = ResultStatus.SUCCESS };
			r.Result = node.DefaultTile.NetStealthAdjustment;
			return r;
		}

		[OperationBehavior]
		public IResult<int, int> CalculateCellCountFromRealWorldMeasurements(int modifier, int baseSize, int cellSize)
		{
			var r = new OperationResult<int, int> { Status = ResultStatus.SUCCESS };
			r.Result = Convert.ToInt32(Math.Floor(Convert.ToDouble(modifier * baseSize / cellSize)));
			return r;
		}

	#endregion

#endregion

	// Utility

		[OperationBehavior]
		public bool DemographicIsHybrid(IDemographic demographic)
		{
			return DataRepository.GetHybridDemographicsClasses().Contains(demographic.DemographicClass.ID);
		}


	}
}
