using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.ServiceModel;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Service;
using JTacticalSim.Component;
using JTacticalSim.Utility;
using JTacticalSim.DataContext;
using ctxUtil = JTacticalSim.DataContext.Utility;
using JTacticalSim.Component.GameBoard;
using JTacticalSim.Component.World;

namespace JTacticalSim.Service
{
	[ServiceBehavior]
	public sealed class UnitService : BaseGameService, IUnitService
	{
		static readonly object padlock = new object();

		private static volatile IUnitService _instance;
		public static IUnitService Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new UnitService();
				}

				return _instance;
			}
		}

		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		private UnitService()
		{}


#region Service Methods

		[OperationBehavior]
		public IResult<IUnit, IUnit> CreateUnit(string name,
												ICoordinate coordinate,
												ICountry country,
												UnitInfo unitInfo)
		{
			var result = new OperationResult<IUnit, IUnit>();

			result.Status = ResultStatus.SUCCESS;			
			
			//TODO: Temporary until we determine the SubNodeLocation automatically
			var subNodeLocation = new SubNodeLocation(5);			

			try
			{
				var unit = new Unit(name, coordinate, unitInfo) 
											{ 
												Country = country, 
												SubNodeLocation = subNodeLocation,
												CurrentFuelRange = (unitInfo.UnitType != null) ? unitInfo.UnitType.FuelRange : 0
											};

				var moveStats = new CurrentMoveStatInfo
					{
						RemoteFirePoints = 0,
						MovementPoints = 0,
						HasPerformedAction = true,
					};

				unit.CurrentMoveStats = moveStats;
				result.SuccessfulObjects.Add(unit);
				result.Result = unit;
				result.Messages.Add("Unit Created");
			}
			catch (Exception ex)
			{
				result.ex = ex;
				result.Status = ResultStatus.EXCEPTION;
				result.Messages.Add("Unit not created");
			}


			return result;
		}

		[OperationBehavior]
		public IResult<IUnit, IUnit> SaveUnits(List<IUnit> units)
		{
			return ComponentRepository.SaveUnits(units);
		}

		[OperationBehavior]
		public IResult<IUnit, IUnit> RemoveUnits(List<IUnit> units)
		{
			return ComponentRepository.RemoveUnits(units);
		}

		[OperationBehavior]
		public IResult<IUnit, IUnit> UpdateUnits(List<IUnit> units)
		{
			return ComponentRepository.UpdateUnits(units);
		}

		[OperationBehavior]
		public IResult<IUnit, IUnit> RemoveUnitFromGame(IUnit unit)
		{
			var r = new OperationResult<IUnit, IUnit> {Status = ResultStatus.SUCCESS};

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = ComponentRepository.RemoveUnits(new List<IUnit> { unit });
					var removeAssignedResult = DataRepository.RemoveUnitAssignmentsFromUnit(unit);
					unit.GetTransportedUnits().ToList().ForEach(u => u.RemoveFromGame());					
					unit.CancelMission();
					if (unit.Location != null) unit.RemoveFromStack();
					// Don't include this result in the aggragate. It returns failure if the unit isn't selected (which it probably won't be)
					var removeSelectedResult = TheGame().GameBoard.RemoveSelectedUnit(unit);

					var aggResult = new[] { removeResult, removeAssignedResult }.AggragateResultsData();

					if (aggResult.IsFailed)
					{
						r.Status = ResultStatus.FAILURE;
						r.FailedObjects.Add(unit);
						return r;
					}					
					
					txn.Complete();

					r.SuccessfulObjects.Add(unit);
				}
				catch (Exception ex)
				{
					r.Status = ResultStatus.EXCEPTION;
					r.FailedObjects.Add(unit);
					r.ex = ex;
				}				
			}

			return r;
		}

		[OperationBehavior]
		public IResult<IUnit, IUnit> SaveReinforcementUnitToPlayer(IPlayer player, IUnit unit)
		{
			var r = new OperationResult<IUnit, IUnit> {Status = ResultStatus.SUCCESS};

			// Check available reinforcement points
			if (unit.UnitCost > player.TrackedValues.ReinforcementPoints)
			{
				r.FailedObjects.Add(unit);
				r.Messages.Add("{0} has insufficient reinforcement points for unit {1}".F(player.Country.Name, unit.Name));
				r.Status = ResultStatus.FAILURE;
				return r;
			}

			using (var txn = new TransactionScope())
			{
				player.UnplacedReinforcements.Add(unit);
				player.TrackedValues.ReinforcementPoints -= Convert.ToInt32(unit.UnitCost);
				var updateResult = TheGame().JTSServices.GameService.UpdatePlayers(new List<IPlayer> { player });

				r.ConvertResultData(updateResult);

				if (r.Status == ResultStatus.SUCCESS)
				{
					r.SuccessfulObjects.Add(unit);
					txn.Complete();
				}
				else
				{
					r.FailedObjects.Add(unit);
				}
			}

			return r;
		}

		[OperationBehavior]
		public IResult<IUnit, IUnit> RemoveReinforcementUnitFromPlayer(IPlayer player, IUnit unit)
		{
			var r = new OperationResult<IUnit, IUnit>{Status = ResultStatus.SUCCESS};

			using (var txn = new TransactionScope())
			{
				player.UnplacedReinforcements.Remove(unit);
				var updateResult = TheGame().JTSServices.GameService.UpdatePlayers(new List<IPlayer> { player });

				r.ConvertResultData(updateResult);

				if (r.Status == ResultStatus.SUCCESS)
				{
					r.SuccessfulObjects.Add(unit);
					txn.Complete();
				}
				else
				{
					r.FailedObjects.Add(unit);
				}
			}

			return r;
		}

		[OperationBehavior]
		public void UpdateUnitLocation(IUnit unit, INode node, INode lastNode)
		{
			if (!unit.ExistsInContext()) return;

			// Set node coordinate location
			unit.Location = node.Location;

			// Set SubNodeLocation
			// TODO: we might need to force a setting based on hybrid demographics and multiple stacks. Not sure yet.
			var direction = (lastNode != null)
							? TheGame().JTSServices.NodeService.GetNodeDirectionFromNeighborSourceNode(lastNode, node)
							: Direction.NONE;

			unit.SubNodeLocation = (direction == Direction.NONE)
									? new SubNodeLocation(5)
									: new SubNodeLocation(Orienting.ConvertDirectionToSubNodeLocation(direction));

			UpdateUnits(new List<IUnit> { unit });
		}

		[OperationBehavior]
		public IResult<IUnit, IUnit> UpdateUnitFuelRange(IUnit unit, int nodeDistance)
		{
			var r = new OperationResult<IUnit, IUnit>{Status = ResultStatus.SUCCESS};

			if (!unit.ExistsInContext())
			{
				r.FailedObjects.Add(unit);
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Unit does not exist in context.");
				return r;
			}

			try
			{
				unit.CurrentFuelRange -= TheGame().GameBoard.DefaultAttributes.CellMeters * nodeDistance;
				// Run out of gas!!
				if (unit.CurrentFuelRange < 0)	unit.CurrentFuelRange = 0;
				UpdateUnits(new List<IUnit> {unit});
			}
			catch (Exception ex)
			{
				r.FailedObjects.Add(unit);
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				return r;
			}

			r.SuccessfulObjects.Add(unit);
			r.Status = ResultStatus.SUCCESS;
			return r;
		}

		[OperationBehavior]
		public IEnumerable<IUnit> GetAllUnits()
		{
			var retVal = new List<IUnit>();

			foreach (var dto in ComponentRepository.GetUnits())
			{
				// Check cache first
				var comp = Cache().UnitCache.TryFind(dto.UID);

				// if it's there, grab it
				if (comp != null)
				{
					retVal.Add(comp);
					continue;
				}

				// if not, create the component and add it to the cache and grab it
				comp = dto.ToComponent();
				Cache().UnitCache.TryAdd(comp.UID, comp);
				retVal.Add(comp);
			}

			return retVal;
		}
		

		[OperationBehavior]
		public IEnumerable<IUnit> GetAllUnits(IEnumerable<IFaction> factions)
		{
			return GetAllUnits()
				.Where(u => factions.Contains(u.Country.Faction))
				.Select(u => u);
		}

		[OperationBehavior]
		public IEnumerable<IUnit> GetAllUnits(ICountry country)
		{
			return GetAllUnits()
				.Where(u => u.Country.Equals(country));
		}

		[OperationBehavior]
		public List<IUnit> GetUnitsAt(ICoordinate coordinate, IEnumerable<IFaction> factions)
		{
			var comps = ComponentRepository.GetUnits().Where(u => u.Location != null).Select(u => u.ToComponent());
			return comps
					.Where(u => u.LocationEquals(coordinate))
					.Where(u => factions.Any(f => f.Equals(u.Country.Faction)))
					.Select(u => u).ToList();
		}

		[OperationBehavior]
		public List<IUnit> GetUnitsAt(ICoordinate coordinate, IEnumerable<ICountry> countries)
		{
			var comps = ComponentRepository.GetUnits().Where(u => u.Location != null).Select(u => u.ToComponent());
			return comps
					.Where(u => u.Location.Equals(coordinate))
					.Where(u => countries.Any(c => c.Equals(u.Country)))
					.Select(u => u).ToList();
		}

		[OperationBehavior]
		public List<IUnit> GetAllUnitsAt(ICoordinate coordinate)
		{
			var factions = TheGame().JTSServices.GameService.GetAllFactions();
			return GetUnitsAt(coordinate, factions);
		}

		[OperationBehavior]
		public List<IUnit> GetUnitsByUnitAssignment(int unitID)
		{
			var ids = DataRepository.GetUnitAssignments().Where(ua => ua.AssignedToUnit == unitID).Select(ua => ua.Unit);
			var retVal = ComponentRepository.GetUnits().Where(u => ids.Contains(u.ID)).Select(u => u.ToComponent());
			return retVal.ToList();
		}

		[OperationBehavior]
		public List<IUnit> GetUnitsByUnitAssignmentRecursive(int unitID)
		{
			var retVal = new List<IUnit>();
			var nextLevel = GetUnitsByUnitAssignment(unitID);
			retVal.AddRange(nextLevel);

			while (nextLevel.Any())
			{
				var thisLevel = new List<IUnit>();
				// Get all units by assignment for current level
				nextLevel.ForEach(u => thisLevel.AddRange(GetUnitsByUnitAssignment(u.ID)));

				// Add this level to the total assigned units
				retVal.AddRange(thisLevel);

				nextLevel = thisLevel;
			}

			return retVal;
		}

		[OperationBehavior]
		public List<IUnit> GetUnitsByTransport(int transportID)
		{
			var ids = DataRepository.GetUnitTransports().Where(ut => ut.TransportUnit == transportID).Select(ua => ua.Unit);
			var retVal = ComponentRepository.GetUnits().Where(u => ids.Contains(u.ID)).Select(u => u.ToComponent());
			return retVal.ToList();
		}

		[OperationBehavior]
		public IUnit GetUnitAssignedToUnit(int unitID)
		{
			var id = DataRepository.GetUnitAssignments()
				.Where(ua => ua.Unit == unitID)
				.Select(ua => ua.AssignedToUnit)
				.SingleOrDefault();

			var unit = ComponentRepository.GetUnits().SingleOrDefault(u => u.ID == id);

			return unit.ToComponent();
		}

		[OperationBehavior]
		public IUnit GetUnitByID(int id)
		{
			var dto = ComponentRepository.GetUnits().SingleOrDefault(u => u.ID == id);

			if (dto == null)
				throw new ComponentNotFoundException("No unit found with id {0}".F(id));

			return dto.ToComponent();
		}

		[OperationBehavior]
		public IUnitClass GetUnitClassByID(int id)
		{
			var dto = ComponentRepository.GetUnitClasses().SingleOrDefault(uc => uc.ID == id);

			if (dto == null)
				throw new ComponentNotFoundException("No unit class found with id {0}".F(id));

			return dto.ToComponent();
		}

		[OperationBehavior]
		public IUnitClass GetUnitClassByName(string name)
		{
			var dto = ComponentRepository.GetUnitClasses().SingleOrDefault(uc => uc.Name.ToLowerInvariant() == name.ToLowerInvariant());

			if (dto == null)
				throw new ComponentNotFoundException("No unit class found with name {0}".F(name));

			return dto.ToComponent();
		}

		[OperationBehavior]
		public IResult<IEnumerable<IUnitClass>, IUnitClass> GetUnitClassesByIDs(IEnumerable<int> ids)
		{
			var r = new OperationResult<IEnumerable<IUnitClass>, IUnitClass>();
			r.Result = ComponentRepository.GetUnitClasses().Where(uc => ids.Contains(uc.ID)).Select(uc => uc.ToComponent());
			return r;
		}

		[OperationBehavior]
		public IResult<IEnumerable<IUnitClass>, IUnitClass> GetAllowableUnitClassesForUnit(IUnit unit)
		{
			var r = new OperationResult<IEnumerable<IUnitClass>, IUnitClass>();

			var ids = TheGame().JTSServices.DataService.LookupAllowableUnitClassesByUnitBaseType(unit.UnitInfo.UnitType.BaseType.ID);
			r.Result = TheGame().JTSServices.UnitService.GetUnitClassesByIDs(ids).Result;

			return r;
		}


		public IResult<IEnumerable<IUnit>, IUnit> GetAllowableAttachUnitsForUnit(IUnit unit)
		{
			var r = new OperationResult<IEnumerable<IUnit>, IUnit>();
			var attachUnits = new List<IUnit>();

			try
			{
				var units = GetAllUnits(unit.Country).ToList();

				units.ForEach(u =>
					{
						if (TheGame().JTSServices.RulesService.UnitCanAttachToUnit(unit, u).Result)
						{
							attachUnits.Add(u);
						}

					});

				r.Result = attachUnits;
				r.Status = ResultStatus.SUCCESS;
				r.Messages.Add("Attachable units found.");
			}
			catch (Exception ex)
			{
				r.ex = ex;
				r.Status = ResultStatus.EXCEPTION;
				r.Messages.Add("Could not retrieve attachable units.");
			}

			return r;
		}
			
		[OperationBehavior]
		public IEnumerable<IUnitClass> GetUnitClasses()
		{
			return ComponentRepository.GetUnitClasses().Select(uc => uc.ToComponent());
		}

		[OperationBehavior]
		public IResult<IUnitClass, IUnitClass> SaveUnitClasses(List<IUnitClass> unitClasses)
		{
			return ComponentRepository.SaveUnitClasses(unitClasses);
		}

		[OperationBehavior]
		public IResult<IUnitClass, IUnitClass> RemoveUnitClasses(List<IUnitClass> unitClasses)
		{
			return ComponentRepository.RemoveUnitClasses(unitClasses);
		}

		[OperationBehavior]
		public IResult<IUnitClass, IUnitClass> UpdateUnitClasses(List<IUnitClass> unitClasses)
		{
			return ComponentRepository.UpdateUnitClasses(unitClasses);
		}


		[OperationBehavior]
		public IEnumerable<IUnitBaseType> GetUnitBaseTypes()
		{
			return ComponentRepository.GetUnitBaseTypes().Select(ubt => ubt.ToComponent());
		}

		[OperationBehavior]
		public IResult<IUnitBaseType, IUnitBaseType> SaveUnitBaseTypes(List<IUnitBaseType> unitBaseTypes)
		{
			return ComponentRepository.SaveUnitBaseTypes(unitBaseTypes);
		}

		[OperationBehavior]
		public IResult<IUnitBaseType, IUnitBaseType> RemoveUnitBaseTypes(List<IUnitBaseType> unitBaseTypes)
		{
			return ComponentRepository.RemoveUnitBaseTypes(unitBaseTypes);
		}

		[OperationBehavior]
		public IResult<IUnitBaseType, IUnitBaseType> UpdateUnitBaseTypes(List<IUnitBaseType> unitBaseTypes)
		{
			return ComponentRepository.UpdateUnitBaseTypes(unitBaseTypes);
		}



		[OperationBehavior]
		public IUnitGroupType GetUnitGroupTypeByID(int id)
		{
			var dto = ComponentRepository.GetUnitGroupTypes().SingleOrDefault(ugt => ugt.ID == id);
			if (dto == null)
				throw new ComponentNotFoundException("No unit group type found with id {0}".F(id));

			return dto.ToComponent();
		}

		[OperationBehavior]
		public IUnitGroupType GetUnitGroupTypeByName(string name)
		{
			var dto = ComponentRepository.GetUnitGroupTypes().SingleOrDefault(o => o.Name.ToLowerInvariant() == name.ToLowerInvariant());

			if (dto == null)
				throw new ComponentNotFoundException("No unit group type found with name {0}".F(name));

			return dto.ToComponent();
		}

		[OperationBehavior]
		public IResult<IEnumerable<IUnitGroupType>, IUnitGroupType> GetUnitGroupTypesByIDs(IEnumerable<int> ids)
		{
			var r = new OperationResult<IEnumerable<IUnitGroupType>, IUnitGroupType>();
			r.Result = ComponentRepository.GetUnitGroupTypes().Where(ugt => ids.Contains(ugt.ID)).Select(ugt => ugt.ToComponent());
			return r;
		}

		[OperationBehavior]
		public IEnumerable<IUnitGroupType> GetUnitGroupTypes()
		{
			return ComponentRepository.GetUnitGroupTypes().Select(ugt => ugt.ToComponent());
		}

		[OperationBehavior]
		public IResult<IUnitGroupType, IUnitGroupType> SaveUnitGroupTypes(List<IUnitGroupType> unitGroupTypes)
		{
			return ComponentRepository.SaveUnitGroupTypes(unitGroupTypes);
		}

		[OperationBehavior]
		public IResult<IUnitGroupType, IUnitGroupType> RemoveUnitGroupTypes(List<IUnitGroupType> unitGroupTypes)
		{
			return ComponentRepository.RemoveUnitGroupTypes(unitGroupTypes);
		}

		[OperationBehavior]
		public IResult<IUnitGroupType, IUnitGroupType> UpdateUnitGroupTypes(List<IUnitGroupType> unitGroupTypes)
		{
			return ComponentRepository.UpdateUnitGroupTypes(unitGroupTypes);
		}

		[OperationBehavior]
		public IUnitGroupType GetNextHighestUnitGroupType(IUnitGroupType ugt)
		{
			var allUnitLevels = ComponentRepository.GetUnitGroupTypes().Select(gt => gt.ToComponent()).ToList();
			allUnitLevels.Sort();
			return allUnitLevels.FirstOrDefault(ul => ul.Level > ugt.Level);
		}


		[OperationBehavior]
		public IEnumerable<IUnitGeogType> GetUnitGeogTypes()
		{
			return ComponentRepository.GetUnitGeogTypes().Select(ugt => ugt.ToComponent());
		}

		[OperationBehavior]
		public IResult<IUnitGeogType, IUnitGeogType> SaveUnitGeogTypes(List<IUnitGeogType> unitGeogTypes)
		{
			return ComponentRepository.SaveUnitGeogTypes(unitGeogTypes);
		}

		[OperationBehavior]
		public IResult<IUnitGeogType, IUnitGeogType> RemoveUnitGeogTypes(List<IUnitGeogType> unitGeogTypes)
		{
			return ComponentRepository.RemoveUnitGeogTypes(unitGeogTypes);
		}

		[OperationBehavior]
		public IResult<IUnitGeogType, IUnitGeogType> UpdateUnitGeogTypes(List<IUnitGeogType> unitGeogTypes)
		{
			return ComponentRepository.UpdateUnitGeogTypes(unitGeogTypes);
		}


		[OperationBehavior]
		public IEnumerable<IUnitType> GetUnitTypes()
		{
			return ComponentRepository.GetUnitTypes().Select(ugt => ugt.ToComponent());
		}

		[OperationBehavior]
		public IUnitType GetUnitTypeByID(int id)
		{
			var dto = ComponentRepository.GetUnitTypes().SingleOrDefault(ut => ut.ID == id);

			if (dto == null)
				throw new ComponentNotFoundException("No unit type found with id {0}".F(id));

			return dto.ToComponent();
		}

		[OperationBehavior]
		public IUnitType GetUnitTypeByName(string name)
		{
			var dto = ComponentRepository.GetUnitTypes().SingleOrDefault(o => o.Name.ToLowerInvariant() == name.ToLowerInvariant());

			if (dto == null)
				throw new ComponentNotFoundException("No unit type found with name {0}".F(name));

			return dto.ToComponent();
		}

		[OperationBehavior]
		public IResult<IEnumerable<IUnitType>, IUnitType> GetUnitTypesByIDs(IEnumerable<int> ids)
		{
			var r = new OperationResult<IEnumerable<IUnitType>, IUnitType>();
			r.Result = ComponentRepository.GetUnitTypes().Where(ut => ids.Contains(ut.ID)).Select(ut => ut.ToComponent());
			return r;
		}

		[OperationBehavior]
		public IResult<IUnitType, IUnitType> SaveUnitTypes(List<IUnitType> unitTypes)
		{
			return ComponentRepository.SaveUnitTypes(unitTypes);
		}

		[OperationBehavior]
		public IResult<IUnitType, IUnitType> RemoveUnitTypes(List<IUnitType> unitTypes)
		{
			return ComponentRepository.RemoveUnitTypes(unitTypes);
		}

		[OperationBehavior]
		public IResult<IUnitType, IUnitType> UpdateUnitTypes(List<IUnitType> unitTypes)
		{
			return ComponentRepository.UpdateUnitTypes(unitTypes.ToList());
		}

		[OperationBehavior]
		public IEnumerable<IUnitType> GetUnitTypesAllowableForTile(ITile tile)
		{
			var retVal = new List<IUnitType>();

			var unitTypes = ComponentRepository.GetUnitTypes().Select(ut => ut.ToComponent());

			Action<IUnitType> componentAction = ut =>
			{
				if (TheGame().JTSServices.RulesService.TileIsAllowableForUnitType(ut, tile).Result) retVal.Add(ut);
			};

			if (TheGame().IsMultiThreaded)
			{
				Parallel.ForEach(unitTypes, componentAction);
			}
			else
			{
				foreach (var ut in unitTypes)
				{
					componentAction(ut);
				}
			}

			return retVal;
		}

		[OperationBehavior]
		public IResult<IEnumerable<IUnitClass>, IUnitClass> GetAllowableUnitClassesForUnitType(IUnitType unitType)
		{
			var r = new OperationResult<IEnumerable<IUnitClass>, IUnitClass>();
			var ids = TheGame().JTSServices.DataService.LookupAllowableUnitClassesByUnitBaseType(unitType.BaseType.ID);
			r.Result = TheGame().JTSServices.UnitService.GetUnitClassesByIDs(ids).Result;
			return r;
		}

#endregion

	}
}
