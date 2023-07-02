using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Service;
using JTacticalSim.API.Cache;
using JTacticalSim.API.Game;
using JTacticalSim.AI;
using JTacticalSim.Utility;

namespace JTacticalSim.API.Service
{
	[ServiceContract]
	public interface IAIService
	{
		/// <summary>
		/// Deploys units from a transport unit to a specified destination
		/// </summary>
		/// <param name="transport"></param>
		/// <param name="units"></param>
		/// <param name="destinationNode"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<IUnit, IUnit> DeployUnitsFromTransportToNode(IUnit transport, IEnumerable<IUnit> units, INode destinationNode);

		/// <summary>
		/// Loads units to a specified transport
		/// </summary>
		/// <param name="transport"></param>
		/// <param name="units"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<IUnit, IUnit> LoadUnitsToTransport(IUnit transport, List<IUnit> units);

		/// <summary>
		/// Attaches a given unit to a parent unit
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<IUnit, IUnit> AttachUnitToUnit(IUnit parent, IUnit unit);

		/// <summary>
		/// Detaches a given unit from a parent unit
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<IUnit, IUnit> DetachUnitFromUnit(IUnit unit);

		/// <summary>
		/// Checks the current location and attempts to refuel
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<IUnit, IUnit> AttemptRefuelUnit(IUnit unit);

		/// <summary>
		/// Used in various scenarios, e.g. plane runs out of fuel, when a unit crashes and is removed from the game
		/// </summary>
		[OperationContract]
		IResult<IUnit, IUnit> HandleZeroFuelForUnit(IUnit unit);

		/// <summary>
		/// Returns the shortest path from a source node to any node within the configured supply distance
		/// with a supply unit
		/// Returns null if none found.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="faction"></param>
		/// <param name="supplyDistance"></param>
		/// <returns></returns>
		[OperationContract]
		RouteInfo FindSupplyPath(INode source, IFaction faction, int supplyDistance);

		/// <summary>
		/// Returns the shortest path between two nodes given the movement restrictions of a given unit
		/// Returns null if none found.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="map"></param>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<RouteInfo, IMoveableComponent> FindPath(INode source, 
												INode target, 
												IEnumerable<IPathableObject> map, 
												IUnit unit);

		
		/// <summary>
		/// Returns the actual number of nodes between a source unit and target unit within a max distance radius
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="maxDistance"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<int?, INode> CalculateNodeCountToUnit(IUnit source, IUnit target, int maxDistance);

		/// <summary>
		/// Returns the actual number of nodes between a source node and target node within a max distance radius
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="maxDistance"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<int?, INode> CalculateNodeCountToNode(INode source, INode target, int maxDistance);

		 
		/// <summary>
		/// Returns the best suited target unit for a given unit from a pool of avaialble candidate units
		/// </summary>
		/// <param name="candidates"></param>
		/// <param name="attacker"></param>
		/// <param name="battleType"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<IUnit, IUnit> GetPrimeUnitTargetForUnit(List<IUnit> candidates, 
															IUnit attacker, 
															BattleType battleType);

		/// <summary>
		/// Returns a list of skirmishes created based on a collection of attacker units and a collection of defender unit candidates
		/// decided by determining prime defender targets for each attacker.
		/// </summary>
		/// <param name="battle"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<List<ISkirmish>, ISkirmish> CreateFullCombatSkirmishes(IBattle battle);

		/// <summary>
		/// Returns a list of skirmishes created based on a collection of attacker units and a collection of defender unit candidates
		/// ---- Matches only air defence capable units against air units
		/// </summary>
		/// <param name="battle"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<List<ISkirmish>, ISkirmish> CreateAirDefenceSkirmishes(IBattle battle);

		/// <summary>
		/// Returns a list of skirmishes created based on a collection of attacker units and a collection of defender unit candidates
		/// ---- Matches only missile defence capable units against missile units
		/// </summary>
		/// <param name="battle"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<List<ISkirmish>, ISkirmish> CreateMissileDefenceSkirmishes(IBattle battle);

		/// <summary>
		/// Process a nuclear battle. Single skirmish with single attacker/multiple defenders
		/// </summary>
		/// <param name="battle"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<IBattle, IBattle> ResolveNuclearBattle(IBattle battle, INode target);

		/// <summary>
		/// Processes a battle skirmish between two units
		/// Uses the simultaneous die-roll for attack/defend a'la Axis and Allies
		/// </summary>
		/// <param name="skirmish"></param>
		[OperationContract]
		IResult<ISkirmish, ISkirmish> ResolveSkirmish(ISkirmish skirmish, BattleType battleType);


		/// <summary>
		/// Attempts to claim the battle node for the victor faction
		/// </summary>
		/// <param name="units"></param>
		/// <param name="node"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<INode, INode> ClaimNodeForVictorFaction(List<IUnit> units, INode node);

		/// <summary>
		/// Performs automated operations on units at turn end
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		IResult<IUnit, IUnit> HandleSpecialTurnEndUnitManagement();

		/// <summary>
		/// Performs automated operations on units at turn start
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		IResult<IUnit, IUnit> HandleSpecialTurnStartUnitManagement();

#region AI Strategy

		[OperationContract]
		IEnumerable<IUnitTaskType> GetUnitTaskTypes();

		[OperationContract]
		IResult<IUnitTaskType, IUnitTaskType> SaveUnitTaskTypes(List<IUnitTaskType> unitTaskTypes);

		[OperationContract]
		IResult<IUnitTaskType, IUnitTaskType> RemoveUnitTaskTypes(List<IUnitTaskType> unitTaskTypes);

		[OperationContract]
		IResult<IUnitTaskType, IUnitTaskType> UpdateUnitTaskTypes(List<IUnitTaskType> unitTaskTypes);


		[OperationContract]
		IEnumerable<IMissionType> GetMissionTypes();

		[OperationContract]
		IResult<IMissionType, IMissionType> SaveMissionTypes(List<IMissionType> MissionTypes);

		[OperationContract]
		IResult<IMissionType, IMissionType> RemoveMissionTypes(List<IMissionType> MissionTypes);

		[OperationContract]
		IResult<IMissionType, IMissionType> UpdateMissionTypes(List<IMissionType> MissionTypes);


		///// <summary>
		///// Returns all strategies in the cache
		///// </summary>
		///// <returns></returns>
		//[OperationContract]
		//IEnumerable<IStrategy> GetStrategies();

		///// <summary>
		///// Saves a new strategy to the cache
		///// </summary>
		///// <param name="strategy"></param>
		///// <returns></returns>
		//[OperationContract]
		//IResult<IStrategy, IStrategy> SaveStrategy(IStrategy strategy);

		///// <summary>
		///// Removes a strategy from the cache
		///// </summary>
		///// <param name="strategy"></param>
		///// <returns></returns>
		//[OperationContract]
		//IResult<IStrategy, IStrategy> RemoveStrategy(IStrategy strategy);

		///// <summary>
		///// Updates a strategy in the cache
		///// </summary>
		///// <param name="strategy"></param>
		///// <returns></returns>
		//[OperationContract]
		//IResult<IStrategy, IStrategy> UpdateStrategy(IStrategy strategy);

		/// <summary>
		/// Returns all tactics in the cache
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		IEnumerable<ITactic> GetTactics();

		/// <summary>
		/// Saves a tactic to the cache
		/// </summary>
		/// <param name="tactic"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<ITactic, ITactic> SaveTactic(ITactic tactic);

		/// <summary>
		/// Removes a tactic from the cache
		/// </summary>
		/// <param name="tactic"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<ITactic, ITactic> RemoveTactic(ITactic tactic);

		/// <summary>
		/// Updates a tactic in the cache
		/// </summary>
		/// <param name="tactic"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<ITactic, ITactic> UpdateTactic(ITactic tactic);

		/// <summary>
		/// Returns a new Tactic with missions populated
		/// </summary>
		/// <param name="missions"></param>
		/// <param name="player"></param>
		/// <param name="stance"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<ITactic, ITactic> CreateNewTactic(IEnumerable<IMission> missions, IPlayer player, StrategicalStance stance);

		/// <summary>
		/// Returns an info object for various points of a node's strategic value based on value rules
		/// </summary>
		/// <param name="tile"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<StrategicAssessmentInfo, ITile> DetermineTileStrategicValue(ITile tile, GameboardStrategicValueAttributesInfo attributes);

		/// <summary>
		/// Returns the parent mission for a given task
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		IResult<IMission, IMission> GetCurrentMissionForUnitTask(IUnitTask task);

		/// <summary>
		/// Returns the parent tactic for a given mission
		/// </summary>
		/// <param name="mission"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<ITactic, ITactic> GetCurrentTacticForMission(IMission mission);

		///// <summary>
		///// Returns the parent strategy for a given tactic
		///// </summary>
		///// <param name="tactic"></param>
		///// <returns></returns>
		//[OperationContract]
		//IResult<IStrategy, IStrategy> GetCurrentStrategyForTactic(ITactic tactic);

		/// <summary>
		/// Executes strategies and aggragates results
		/// </summary>
		/// <param name="tactics"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<ITactic, ITactic> ExecuteStrategies(List<ITactic> tactics);


	#region Unit Tasks

		/// <summary>
		/// Returns a mission containing the configured Tasks for the mission type
		/// </summary>
		/// <param name="missionType"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<IMission, IMission> CreateMission(IMissionType missionType);

		/// <summary>
		/// Returns a mission with a single task
		/// </summary>
		/// <param name="missionType"></param>
		/// <param name="unitTask"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<IMission, IMission> CreateMission(IMissionType missionType, IUnitTask unitTask);

		/// <summary>
		/// Removes the currently assigned mission for a unit
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<IMission, IMission> CancelMission(IMission mission);

		/// <summary>
		/// Returns the currently assigned mission
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<IMission, IUnit> GetCurrentAssignedMissionForUnit(IUnit unit);

		/// <summary>
		/// Performs the task of building infrastructure
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="tile"></param>
		/// <param name="demographic"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<IUnit, IUnit> BuildInfrastructure(IUnit unit, ITile tile, IDemographic demographic);

		/// <summary>
		/// Performs the task of destroying infrastructure
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="tile"></param>
		/// <param name="demographic"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<IUnit, IUnit> DestroyInfrastructure(IUnit unit, ITile tile, IDemographic demographic, Direction direction);

		/// <summary>
		/// Returns object instances from the DataContext in the case of components and prmitive value types for others 
		/// </summary>
		/// <param name="argument"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<IEnumerable<object>, TaskExecutionArgument> GetExecutionArgumentObjects(TaskExecutionArgument argument);

	#endregion


#endregion

	}
}
