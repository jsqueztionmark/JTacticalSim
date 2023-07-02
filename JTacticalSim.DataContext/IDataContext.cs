using System;
using System.Collections.Generic;
using System.Dynamic;
using JTacticalSim.API.Data;
using JTacticalSim.Data.DTO;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Component;


namespace JTacticalSim.DataContext
{
	public interface IDataContext : IDisposable, IDataHandler
	{
		// Component Objects

		TableInfo ComponentSets { get; }
		TableInfo Scenarios { get; }
		TableInfo SavedGames { get; }
		TableInfo NodesTable { get; }
		TableInfo UnitsTable { get; }
		TableInfo FactionsTable { get; }
		TableInfo CountriesTable { get; }
		TableInfo PlayersTable { get; }
		TableInfo DemographicsTable { get; }
		TableInfo TilesTable { get; }
		BoardDTO Board { get; }
		BasePointValuesDTO BasePointValues { get; }

		// Data Objects

		TableInfo UnitBaseTypesTable { get; }
		TableInfo UnitClassesTable { get; }
		TableInfo UnitBranchesTable { get; }
		TableInfo UnitTypesTable { get; }
		TableInfo UnitGroupTypesTable { get; }
		TableInfo UnitGeogTypesTable { get; }
		TableInfo UnitTaskTypesTable { get; }
		TableInfo MissionTypesTable { get; }
		TableInfo DemographicTypesTable { get; }
		TableInfo DemographicClassesTable { get; }
		TableInfo VictoryConditionTypesTable { get; }

		//Lookups

		List<dynamic> UnitBaseTypeUnitClassesLookup { get; }
		List<dynamic> UnitBaseTypeUnitGeogTypesLookup { get; }
		List<dynamic> UnitGeogTypeDemographicClassesLookup { get; }
		List<dynamic> UnitGroupTypeUnitTaskTypeLookup { get; }
		List<dynamic> UnitTaskTypeUnitClassesLookup { get; }
		List<dynamic> MissionTypeUnitTaskType { get; }
		List<dynamic> UnitAssignments { get; }
		List<dynamic> UnitTransports { get; }
		List<dynamic> UnitGeogTypeMovementOverrides { get; }
		List<dynamic> UnitBattleEffectiveLookup { get; }
		List<dynamic> UnitTransportUnitTypeUnitClasses { get; }
		List<dynamic> FactionVictoryConditions { get; }
		List<int> HybridDemographicClasses { get; }
		List<dynamic> MovementHinderanceInDirection { get; }
		List<dynamic> AllowedUnitTypes { get; }
		List<dynamic> AllowedUnitGroupTypes { get; }
	}
}
