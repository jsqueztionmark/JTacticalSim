using System;
using System.Collections.Generic;
using JTacticalSim.API.Component;
using JTacticalSim.API.Data;
using JTacticalSim.API.Game;
using JTacticalSim.API.Service;
using JTacticalSim.API.AI;

namespace JTacticalSim.API.Service
{
	public interface IDataService
	{
		void ResetDataContext();

		/// <summary>
		/// Loads saved game info and saved scenario info
		/// </summary>
		/// <param name="gameDataDirectory"></param>
		/// <param name="IsScenario"></param>
		IResult<string, string> LoadSavedGameData(string gameDataDirectory, bool IsScenario);

		IResult<string, string> LoadData(string gameFileDirectory, IComponentSet componentSet, bool IsScenario);

		IResult<IGameFileCopyable, IGameFileCopyable> SaveData(IGameFileCopyable currentData);

		IResult<IGameFileCopyable, IGameFileCopyable> SaveDataAs(IGameFileCopyable currentData, IGameFileCopyable newData);

		IResult<IGameFileCopyable, IGameFileCopyable> RemoveSavedData(IGameFileCopyable delData);


		IBasePointValues GetBasePointValues();

		IEnumerable<dynamic> GetUnitAssignments();

		IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> SaveUnitAssignment(IUnit unit, IUnit assignToUnit);

		IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> RemoveUnitAssignment(IUnit unit, IUnit assignedToUnit);

		/// <summary>
		/// Removes all assignements from a given unit
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		IResult<IUnit, IUnit> RemoveUnitAssignmentsFromUnit(IUnit unit);


		IEnumerable<dynamic> GetUnitTransports();

		IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> SaveUnitTransport(IUnit unit, IUnit transport);

		IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> RemoveUnitTransport(IUnit unit, IUnit transport);

		IEnumerable<IUnitType> GetAllowedUnitTypes(ICountry country);

		IEnumerable<IUnitGroupType> GetAllowedUnitGroupTypes();


		IEnumerable<dynamic> GetFactionVictoryConditions();

		IResult<Tuple<IFaction, IVictoryCondition>, Tuple<IFaction, IVictoryCondition>> SaveFactionVictoryConditions(IFaction faction, IVictoryCondition victoryCondition);

		IResult<Tuple<IFaction, IVictoryCondition>, Tuple<IFaction, IVictoryCondition>> RemoveFactionVictoryConditions(IFaction faction, IVictoryCondition victoryCondition);


		IEnumerable<dynamic> GetUnitBaseTypeUnitGeogTypesLookup();

		IEnumerable<dynamic> GetUnitGeogTypeDemographicClassesLookup();

		IEnumerable<dynamic> GetUnitBaseTypeUnitClassesLookup();

		IEnumerable<dynamic> GetUnitTaskUnitClassesLookup();

	// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	// Lookups
	// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		IEnumerable<int> LookupUnitGeogTypesByBaseTypes(IEnumerable<int> baseTypeids);

		IEnumerable<int> LookupDemographicClassesByUnitGeogTypes(IEnumerable<int> unitGeogTypeids);

		IEnumerable<int> LookupAllowableUnitClassesByUnitBaseType(int unitBaseTypeid);

		IResult<int, int> LookupUnitTaskTypeStepOrderForMissionType(IUnitTaskType unitTaskType, IMissionType missionType);

		/// <summary>
		/// Returns the configured Unit Tasks for a given mission
		/// </summary>
		/// <param name="missionType"></param>
		/// <returns></returns>
		IResult<List<IUnitTaskType>, IUnitTaskType> LookupUnitTaskTypesForMissionType(IMissionType missionType);
	}
}
