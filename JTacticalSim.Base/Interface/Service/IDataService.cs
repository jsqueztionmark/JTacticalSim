using System;
using System.ServiceModel;
using System.Collections.Generic;
using JTacticalSim.API.Component;
using JTacticalSim.API.Data;
using JTacticalSim.API.Game;
using JTacticalSim.API.Service;
using JTacticalSim.API.AI;

namespace JTacticalSim.API.Service
{
	[ServiceContract]
	public interface IDataService
	{
		[OperationContract]
		void ResetDataContext();

		/// <summary>
		/// Loads saved game info and saved scenario info
		/// </summary>
		/// <param name="gameDataDirectory"></param>
		/// <param name="IsScenario"></param>
		[OperationContract]
		IResult<string, string> LoadSavedGameData(string gameDataDirectory, bool IsScenario);

		[OperationContract]
		IResult<string, string> LoadData(string gameFileDirectory, IComponentSet componentSet, bool IsScenario);

		[OperationContract]
		IResult<IGameFileCopyable, IGameFileCopyable> SaveData(IGameFileCopyable currentData);

		[OperationContract]
		IResult<IGameFileCopyable, IGameFileCopyable> SaveDataAs(IGameFileCopyable currentData, IGameFileCopyable newData);

		[OperationContract]
		IResult<IGameFileCopyable, IGameFileCopyable> RemoveSavedData(IGameFileCopyable delData);


		[OperationContract]
		IBasePointValues GetBasePointValues();

		[OperationContract]
		IEnumerable<dynamic> GetUnitAssignments();

		[OperationContract]
		IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> SaveUnitAssignment(IUnit unit, IUnit assignToUnit);

		[OperationContract]
		IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> RemoveUnitAssignment(IUnit unit, IUnit assignedToUnit);

		/// <summary>
		/// Removes all assignements from a given unit
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<IUnit, IUnit> RemoveUnitAssignmentsFromUnit(IUnit unit);


		[OperationContract]
		IEnumerable<dynamic> GetUnitTransports();

		[OperationContract]
		IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> SaveUnitTransport(IUnit unit, IUnit transport);

		[OperationContract]
		IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> RemoveUnitTransport(IUnit unit, IUnit transport);

		[OperationContract]
		IEnumerable<IUnitType> GetAllowedUnitTypes(ICountry country);

		[OperationContract]
		IEnumerable<IUnitGroupType> GetAllowedUnitGroupTypes();


		[OperationContract]
		IEnumerable<dynamic> GetFactionVictoryConditions();

		[OperationContract]
		IResult<Tuple<IFaction, IVictoryCondition>, Tuple<IFaction, IVictoryCondition>> SaveFactionVictoryConditions(IFaction faction, IVictoryCondition victoryCondition);

		[OperationContract]
		IResult<Tuple<IFaction, IVictoryCondition>, Tuple<IFaction, IVictoryCondition>> RemoveFactionVictoryConditions(IFaction faction, IVictoryCondition victoryCondition);


		[OperationContract]
		IEnumerable<dynamic> GetUnitBaseTypeUnitGeogTypesLookup();

		[OperationContract]
		IEnumerable<dynamic> GetUnitGeogTypeDemographicClassesLookup();

		[OperationContract]
		IEnumerable<dynamic> GetUnitBaseTypeUnitClassesLookup();

		[OperationContract]
		IEnumerable<dynamic> GetUnitTaskUnitClassesLookup();

	// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	// Lookups
	// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		[OperationContract]
		IEnumerable<int> LookupUnitGeogTypesByBaseTypes(IEnumerable<int> baseTypeids);

		[OperationContract]
		IEnumerable<int> LookupDemographicClassesByUnitGeogTypes(IEnumerable<int> unitGeogTypeids);

		[OperationContract]
		IEnumerable<int> LookupAllowableUnitClassesByUnitBaseType(int unitBaseTypeid);

		[OperationContract]
		IResult<int, int> LookupUnitTaskTypeStepOrderForMissionType(IUnitTaskType unitTaskType, IMissionType missionType);

		/// <summary>
		/// Returns the configured Unit Tasks for a given mission
		/// </summary>
		/// <param name="missionType"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<List<IUnitTaskType>, IUnitTaskType> LookupUnitTaskTypesForMissionType(IMissionType missionType);
	}
}
