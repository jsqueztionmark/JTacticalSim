using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.Service;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Game;
using JTacticalSim.API.Data;

namespace JTacticalSim.DataContext.Repository
{
	public interface IDataRepository
	{
		// Code Data

		IBasePointValues GetGameBasePointValues();

		GameboardAttributeInfo GetGameboardAttributes();

		// Lookups

		List<int> GetHybridDemographicsClasses();

		IEnumerable<dynamic> GetUnitAssignments();
		IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> SaveUnitAssignment(IUnit unit, IUnit assignToUnit);
		IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> RemoveUnitAssignment(IUnit unit, IUnit assignedToUnit);
		IResult<IUnit, IUnit> RemoveUnitAssignmentsFromUnit(IUnit unit);

		IEnumerable<dynamic> GetUnitTransports();
		IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> SaveUnitTransport(IUnit unit, IUnit transport);
		IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> RemoveUnitTransport(IUnit unit, IUnit transport);

		IEnumerable<dynamic> GetAllowedUnitTypes();
		IEnumerable<dynamic> GetAllowedUnitGroupTypes();

		IEnumerable<dynamic> GetFactionVictoryConditions();
		IResult<Tuple<IFaction, IVictoryCondition>, Tuple<IFaction, IVictoryCondition>> SaveFactionVictoryConditions(IFaction faction, IVictoryCondition victoryCondition);
		IResult<Tuple<IFaction, IVictoryCondition>, Tuple<IFaction, IVictoryCondition>> RemoveFactionVictoryConditions(IFaction faction, IVictoryCondition victoryCondition);

		IEnumerable<dynamic> GetUnitTaskTypeUnitClassesLookup();
		
		IEnumerable<dynamic> GetUnitGroupTypeUnitTaskTypeLookup();

		IEnumerable<dynamic> GetUnitGeogTypeMovementOverrides();

		IEnumerable<dynamic> GetUnitBattleEffectiveLookup();

		IEnumerable<dynamic> GetUnitTransportUnitTypeUnitClasses();

		IEnumerable<dynamic> GetUnitBaseTypeUnitGeogTypesLookup();
		
		IEnumerable<dynamic> GetUnitGeogTypeDemographicClassesLookup();

		IEnumerable<dynamic> GetUnitBaseTypeUnitClassesLookup();

		IEnumerable<dynamic> GetMovementHinderanceInDirection();

		IEnumerable<dynamic> GetMissionTypeUnitTaskTypes();
	}
}
