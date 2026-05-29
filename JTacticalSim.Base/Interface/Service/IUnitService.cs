using System.Collections.Generic;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;

namespace JTacticalSim.API.Service
{
	public interface IUnitService
	{
		IResult<IUnit, IUnit> CreateUnit(string name,
										ICoordinate coordinate,
										ICountry country,
										UnitInfo unitInfo);


		IResult<IUnit, IUnit> SaveUnits(List<IUnit> units);

		IResult<IUnit, IUnit> RemoveUnits(List<IUnit> units);

		IResult<IUnit, IUnit> UpdateUnits(List<IUnit> units);

		IResult<IUnit, IUnit> RemoveUnitFromGame(IUnit unit);

		IResult<IUnit, IUnit> SaveReinforcementUnitToPlayer(IPlayer player, IUnit unit);

		IResult<IUnit, IUnit> RemoveReinforcementUnitFromPlayer(IPlayer player, IUnit unit);

		void UpdateUnitLocation(IUnit unit, INode node, INode lastNode);

		IResult<IUnit, IUnit> UpdateUnitFuelRange(IUnit unit, int nodeDistance);

		IEnumerable<IUnit> GetAllUnits(IEnumerable<IFaction> factions);

		IEnumerable<IUnit> GetAllUnits(ICountry country);

		IEnumerable<IUnit> GetAllUnits();

		List<IUnit> GetUnitsByUnitAssignment(int unitID);

		List<IUnit> GetUnitsByUnitAssignmentRecursive(int unitID);

		IUnit GetUnitAssignedToUnit(int unitID);

		List<IUnit> GetUnitsByTransport(int transportID);

		List<IUnit> GetUnitsAt(ICoordinate coordinate, IEnumerable<IFaction> factions);

		List<IUnit> GetUnitsAt(ICoordinate coordinate, IEnumerable<ICountry> countries);

		List<IUnit> GetAllUnitsAt(ICoordinate coordinate);

		IUnit GetUnitByID(int ID);


		IUnitClass GetUnitClassByID(int id);

		IUnitClass GetUnitClassByName(string name);

		IEnumerable<IUnitClass> GetUnitClasses();

		IResult<IEnumerable<IUnitClass>, IUnitClass> GetUnitClassesByIDs(IEnumerable<int> id);

		IResult<IEnumerable<IUnitClass>, IUnitClass> GetAllowableUnitClassesForUnit(IUnit unit);

		IResult<IEnumerable<IUnit>, IUnit> GetAllowableAttachUnitsForUnit(IUnit unit);

		/// <summary>
		/// Returns list of allowable unit classes for a given unit type
		/// </summary>
		/// <param name="unitType"></param>
		/// <returns></returns>
		IResult<IEnumerable<IUnitClass>, IUnitClass> GetAllowableUnitClassesForUnitType(IUnitType unitType);

		IResult<IUnitClass, IUnitClass> SaveUnitClasses(List<IUnitClass> unitClasses);

		IResult<IUnitClass, IUnitClass> RemoveUnitClasses(List<IUnitClass> unitClasses);

		IResult<IUnitClass, IUnitClass> UpdateUnitClasses(List<IUnitClass> unitClasses);


		IEnumerable<IUnitType> GetUnitTypes();

		IUnitType GetUnitTypeByID(int id);

		IUnitType GetUnitTypeByName(string name);

		IResult<IEnumerable<IUnitType>, IUnitType> GetUnitTypesByIDs(IEnumerable<int> id);

		IResult<IUnitType, IUnitType> SaveUnitTypes(List<IUnitType> unitTypes);

		IResult<IUnitType, IUnitType> RemoveUnitTypes(List<IUnitType> unitTypes);

		IResult<IUnitType, IUnitType> UpdateUnitTypes(List<IUnitType> unitTypes);

		IEnumerable<IUnitType> GetUnitTypesAllowableForTile(ITile tile);


		IResult<IUnitBaseType, IUnitBaseType> SaveUnitBaseTypes(List<IUnitBaseType> unitBaseTypes);

		IResult<IUnitBaseType, IUnitBaseType> RemoveUnitBaseTypes(List<IUnitBaseType> unitBaseTypes);

		IResult<IUnitBaseType, IUnitBaseType> UpdateUnitBaseTypes(List<IUnitBaseType> unitBaseTypes);


		IUnitGroupType GetUnitGroupTypeByID(int id);

		IUnitGroupType GetUnitGroupTypeByName(string name);

		IResult<IEnumerable<IUnitGroupType>, IUnitGroupType> GetUnitGroupTypesByIDs(IEnumerable<int> id);

		/// <summary>
		/// Returns the next highest level of UnitGroupType given a current UnitGroupType
		/// </summary>
		/// <param name="ugt"></param>
		/// <returns></returns>
		IUnitGroupType GetNextHighestUnitGroupType(IUnitGroupType ugt);

		IEnumerable<IUnitGroupType> GetUnitGroupTypes();

		IResult<IUnitGroupType, IUnitGroupType> SaveUnitGroupTypes(List<IUnitGroupType> unitGroupTypes);

		IResult<IUnitGroupType, IUnitGroupType> RemoveUnitGroupTypes(List<IUnitGroupType> unitGroupTypes);

		IResult<IUnitGroupType, IUnitGroupType> UpdateUnitGroupTypes(List<IUnitGroupType> unitGroupTypes);

	}
}
