using System.Collections.Generic;
using System.ServiceModel;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;

namespace JTacticalSim.API.Service
{
	[ServiceContract]
	public interface IUnitService
	{
		[OperationContract]
		IResult<IUnit, IUnit> CreateUnit(string name,
										ICoordinate coordinate,
										ICountry country,
										UnitInfo unitInfo);


		[OperationContract]
		IResult<IUnit, IUnit> SaveUnits(List<IUnit> units);

		[OperationContract]
		IResult<IUnit, IUnit> RemoveUnits(List<IUnit> units);

		[OperationContract]
		IResult<IUnit, IUnit> UpdateUnits(List<IUnit> units);

		[OperationContract]
		IResult<IUnit, IUnit> RemoveUnitFromGame(IUnit unit);

		[OperationContract]
		IResult<IUnit, IUnit> SaveReinforcementUnitToPlayer(IPlayer player, IUnit unit);

		[OperationContract]
		IResult<IUnit, IUnit> RemoveReinforcementUnitFromPlayer(IPlayer player, IUnit unit);

		[OperationContract]
		void UpdateUnitLocation(IUnit unit, INode node, INode lastNode);

		[OperationContract]
		IResult<IUnit, IUnit> UpdateUnitFuelRange(IUnit unit, int nodeDistance);

		[OperationContract]
		IEnumerable<IUnit> GetAllUnits(IEnumerable<IFaction> factions);

		[OperationContract]
		IEnumerable<IUnit> GetAllUnits(ICountry country);

		[OperationContract]
		IEnumerable<IUnit> GetAllUnits();

		[OperationContract]
		List<IUnit> GetUnitsByUnitAssignment(int unitID);

		[OperationContract]
		List<IUnit> GetUnitsByUnitAssignmentRecursive(int unitID);

		[OperationContract]
		IUnit GetUnitAssignedToUnit(int unitID);

		[OperationContract]
		List<IUnit> GetUnitsByTransport(int transportID);

		[OperationContract]
		List<IUnit> GetUnitsAt(ICoordinate coordinate, IEnumerable<IFaction> factions);

		[OperationContract]
		List<IUnit> GetUnitsAt(ICoordinate coordinate, IEnumerable<ICountry> countries);

		[OperationContract]
		List<IUnit> GetAllUnitsAt(ICoordinate coordinate);

		[OperationContract]
		IUnit GetUnitByID(int ID);


		[OperationContract]
		IUnitClass GetUnitClassByID(int id);

		[OperationContract]
		IUnitClass GetUnitClassByName(string name);

		[OperationContract]
		IEnumerable<IUnitClass> GetUnitClasses();

		[OperationContract]
		IResult<IEnumerable<IUnitClass>, IUnitClass> GetUnitClassesByIDs(IEnumerable<int> id);

		[OperationContract]
		IResult<IEnumerable<IUnitClass>, IUnitClass> GetAllowableUnitClassesForUnit(IUnit unit);

		[OperationContract]
		IResult<IEnumerable<IUnit>, IUnit> GetAllowableAttachUnitsForUnit(IUnit unit);

		/// <summary>
		/// Returns list of allowable unit classes for a given unit type
		/// </summary>
		/// <param name="unitType"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<IEnumerable<IUnitClass>, IUnitClass> GetAllowableUnitClassesForUnitType(IUnitType unitType);

		[OperationContract]
		IResult<IUnitClass, IUnitClass> SaveUnitClasses(List<IUnitClass> unitClasses);

		[OperationContract]
		IResult<IUnitClass, IUnitClass> RemoveUnitClasses(List<IUnitClass> unitClasses);

		[OperationContract]
		IResult<IUnitClass, IUnitClass> UpdateUnitClasses(List<IUnitClass> unitClasses);


		[OperationContract]
		IEnumerable<IUnitType> GetUnitTypes();

		[OperationContract]
		IUnitType GetUnitTypeByID(int id);

		[OperationContract]
		IUnitType GetUnitTypeByName(string name);

		[OperationContract]
		IResult<IEnumerable<IUnitType>, IUnitType> GetUnitTypesByIDs(IEnumerable<int> id);

		[OperationContract]
		IResult<IUnitType, IUnitType> SaveUnitTypes(List<IUnitType> unitTypes);

		[OperationContract]
		IResult<IUnitType, IUnitType> RemoveUnitTypes(List<IUnitType> unitTypes);

		[OperationContract]
		IResult<IUnitType, IUnitType> UpdateUnitTypes(List<IUnitType> unitTypes);

		[OperationContract]
		IEnumerable<IUnitType> GetUnitTypesAllowableForTile(ITile tile);


		[OperationContract]
		IResult<IUnitBaseType, IUnitBaseType> SaveUnitBaseTypes(List<IUnitBaseType> unitBaseTypes);

		[OperationContract]
		IResult<IUnitBaseType, IUnitBaseType> RemoveUnitBaseTypes(List<IUnitBaseType> unitBaseTypes);

		[OperationContract]
		IResult<IUnitBaseType, IUnitBaseType> UpdateUnitBaseTypes(List<IUnitBaseType> unitBaseTypes);


		[OperationContract]
		IUnitGroupType GetUnitGroupTypeByID(int id);

		[OperationContract]
		IUnitGroupType GetUnitGroupTypeByName(string name);

		[OperationContract]
		IResult<IEnumerable<IUnitGroupType>, IUnitGroupType> GetUnitGroupTypesByIDs(IEnumerable<int> id);

		/// <summary>
		/// Returns the next highest level of UnitGroupType given a current UnitGroupType
		/// </summary>
		/// <param name="ugt"></param>
		/// <returns></returns>
		[OperationContract]
		IUnitGroupType GetNextHighestUnitGroupType(IUnitGroupType ugt);

		[OperationContract]
		IEnumerable<IUnitGroupType> GetUnitGroupTypes();

		[OperationContract]
		IResult<IUnitGroupType, IUnitGroupType> SaveUnitGroupTypes(List<IUnitGroupType> unitGroupTypes);

		[OperationContract]
		IResult<IUnitGroupType, IUnitGroupType> RemoveUnitGroupTypes(List<IUnitGroupType> unitGroupTypes);

		[OperationContract]
		IResult<IUnitGroupType, IUnitGroupType> UpdateUnitGroupTypes(List<IUnitGroupType> unitGroupTypes);

	}
}
