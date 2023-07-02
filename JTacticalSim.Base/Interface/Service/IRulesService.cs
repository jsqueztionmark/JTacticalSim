using System.Collections.Generic;
using System.ServiceModel;
using System;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Service;
using JTacticalSim.Utility;

namespace JTacticalSim.API.Service
{
	/// <summary>
	/// Checks rules for game operations and returns a service result with appropriate actions
	/// </summary>
	[ServiceContract]
	public interface IRulesService
	{
		[OperationContract]
		IResult<bool, string> UnitNameIsUnique(string name);

		/// <summary>
		/// Determines whether a given unit is allowed to do battle with an opponent
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="opponent"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, Tuple<IUnit, IUnit>> UnitCanDoBattleWithUnit(IUnit unit, IUnit opponent, BattleType battleType);

		/// <summary>
		/// Determines whether any unit can do battle with any opponent given two collections of units
		/// </summary>
		/// <param name="units"></param>
		/// <param name="opponents"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, Tuple<List<IUnit>, List<IUnit>>> UnitsCanDoBattleWithUnits(List<IUnit> units, List<IUnit> opponents, BattleType battleType);

		/// <summary>
		/// Determines whether a given unit can still do battle within the current turn
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, IUnit> UnitCanDoBattle(IUnit unit);

		/// <summary>
		/// Determines whether a given unit is able to move from it's current location to an adjacent location
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, IUnit> UnitCanMoveOntoNode(IUnit unit, INode target);

		/// <summary>
		/// Determines whether a unit can occupy and claim a location for its faction
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, IUnit> UnitCanClaimNodeForFaction(IUnit unit);

		/// <summary>
		/// Determines whether a given unit is allowed to be attached to another unit
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="attachToUnit"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, Tuple<IUnit, IUnit>> UnitCanAttachToUnit(IUnit unit, IUnit attachToUnit);

		/// <summary>
		/// Determines whether a given unit can peform an assignable task based the task's type and UnitClass and UnitGroupType restrictions
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="taskType"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, Tuple<IUnit, IUnitTaskType>> UnitCanPerformTask(IUnit unit, IUnitTaskType taskType);

		/// <summary>
		/// Determines if the node at a given location currently belongs to the given faction
		/// </summary>
		/// <param name="location"></param>
		/// <param name="faction"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, ICoordinate> NodeIsFactionNode(ICoordinate location, IFaction faction);

		/// <summary>
		/// Determines if a given node is within the subset of board nodes available for the movement of a given unit
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="targetNode"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, Tuple<IUnit, INode>> NodeIsValidForMove(IUnit unit, INode targetNode);

		/// <summary>
		/// Determines if the given node has strategic value as a geographical chokepoint
		/// </summary>
		/// <param name="tile"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, ITile> TileIsChokepoint(ITile tile);

		/// <summary>
		/// Determines if a given tile allows for movement between other movement restricted tiles
		/// </summary>
		/// <param name="tile"></param>
		/// <param name="neighborPairs"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, ITile> TileIsPassThroughRestrictedMovement(ITile tile, List<Tuple<ITile, ITile>> neighborPairs);

		/// <summary>
		/// Determines if a given tile forces movement between tiles of other base geography types
		/// </summary>
		/// <param name="tile"></param>
		/// <param name="neighborPairs"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, ITile> TileIsNarrowGeography(ITile tile, List<Tuple<ITile, ITile>> neighborPairs);

		/// <summary>
		/// Determines if the total number of units for a faction at a given location exceeds the configured maximum
		/// </summary>
		/// <param name="tile"></param>
		/// <param name="faction"></param>
		/// <param name="MaxUnits"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, Tuple<ITile, IFaction>> TileHasMaxUnitsForFaction(ITile tile, IFaction faction, int MaxUnits);

		/// <summary>
		/// Determines if the total number of units for a faction after a proposed move to a given location exceeds the configured maximum
		/// </summary>
		/// <param name="tile"></param>
		/// <param name="movingUnits"></param>
		/// <param name="MaxUnits"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, ITile> TileWillExceedMaxUnitsForFaction(ITile tile, IEnumerable<IUnit> movingUnits, int MaxUnits);

		/// <summary>
		/// Determines whether a given tile can support the building of a given infrastructure demographic
		/// </summary>
		/// <param name="tile"></param>
		/// <param name="infrastructure"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, ITile> TileCanSupportInfrastructureBuilding(ITile tile, IDemographic infrastructure);

		/// <summary>
		/// Returns the maximum deployment distance for a given unit based on deployment distance rules
		/// </summary>
		/// <param name="transport"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<int, IUnit> GetAllowableDeployDistanceForTransport(IUnit transport);

		/// <summary>
		/// Returns the maximum load distance for a given unit based on load distance rules
		/// </summary>
		/// <param name="transport"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<int, IUnit> GetAllowableLoadDistanceForTransport(IUnit transport);

		/// <summary>
		/// Determines if the given unit can occupy the given tile
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="tile"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, Tuple<IUnit, ITile>> TileIsAllowableForUnit(IUnit unit, ITile tile);

		/// <summary>
		/// Determines if the given unit type can occupy the given tile
		/// </summary>
		/// <param name="unitType"></param>
		/// <param name="tile"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, Tuple<IUnitType, ITile>> TileIsAllowableForUnitType(IUnitType unitType, ITile tile);

		/// <summary>
		/// Determines if a given tile has a movement override based on it's demographics in any direction
		/// </summary>
		/// <param name="unitType"></param>
		/// <param name="tile"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, Tuple<IUnitGeogType, ITile>> TileHasMovementOverrideForUnitGeogType(IUnitGeogType unitGeogType, ITile tile);

		/// <summary>
		/// Determines if a given tile has a movement override based on it's demographics in a given direction
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="tile"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, Tuple<IUnitType, ITile>> TileHasMovementOverrideForUnit(IUnit unit, ITile tile, Direction direction);

		/// <summary>
		/// Determines if a given tile has a movement override based on it's demographics
		/// </summary>
		/// <param name="unitType"></param>
		/// <param name="tile"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, Tuple<IUnitType, ITile>> TileHasMovementOverrideForUnitType(IUnitType unitType, ITile tile, Direction direction);

		/// <summary>
		/// Returns the calculated sum weight of all units assigned under a given unit recursively
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<double, IUnit> CalculateTotalUnitWeight(IUnit unit);

		/// <summary>
		/// Returns the calculated weight of a single unit
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<double, IUnit> CalculateUnitWeight(IUnit unit);
		
		/// <summary>
		/// Returns maximun allowable transport weight for unit
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<double, IUnit> CalculateAllowableTransportWeight(IUnit unit);

		/// <summary>
		/// Returns the calculated heuristic value for a movement based on source, target and current locations
		/// Keeping this a primitive for performance as it's recursive in the pathfinding algorithm.
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		[OperationContract]
		double? CalculateMovementHeuristic(IPathableObject component);

		/// <summary>
		/// Returns the total victory points accumulated for a faction
		/// </summary>
		/// <param name="faction"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<int, IFaction> CalculateTotalVictoryPoints(IFaction faction);

		/// <summary>
		/// Returns the cell count distance based on the gameboard's real world measurements and a component's modifier
		/// </summary>
		/// <param name="modifier"></param>
		/// <param name="baseSize"></param>
		/// <param name="cellSize"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<int, int> CalculateCellCountFromRealWorldMeasurements(int modifier, int baseSize, int cellSize);

		/// <summary>
		/// Returns the total reinforcement points granted to a player per turn
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<int, IPlayer> CalculateReinforcementPointsForTurn(IPlayer player);

		/// <summary>
		/// Returns the net stealth value for a unit including adjustments for the current tile's geography
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		double CalculateUnitStealthValueForCurrentGeog(IUnit unit);

		/// <summary>
		/// Returns the net attack value for a unit including adjustments for the current tile's geography
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		double CalculateUnitAttackValueForCurrentGeog(IUnit unit);

		/// <summary>
		/// Returns the net defence value for a unit including adjustments for the current tile's geography
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		double CalculateUnitDefenceValueForCurrentGeog(IUnit unit);

		/// <summary>
		/// Returns the target desirability factor for a unit based on contained properties
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		double CalculateTargetDesirabilityForUnit(IUnit unit);

		/// <summary>
		/// Returns the total reinforcment point cost for a unit
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<double, IUnit> CalculateTotalRPCostForUnit(IUnit unit);

		/// <summary>
		/// Returns the total reinforcement point cost for a unit rounded to next highest integer value
		/// </summary>
		/// <param name="ut"></param>
		/// <param name="uc"></param>
		/// <returns></returns>
		[OperationContract]
		double CalculateTotalRPByUnitTypeUnitClass(IUnitType ut, IUnitClass uc);

		/// <summary>
		/// Returns the relative 'strength' factor for a given unit based on rules criteria
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		double CalculateUnitStrength(IUnit unit);

		/// <summary>
		/// Returns the distance in number of spaces at which the AI will attempt an action
		/// </summary>
		/// <param name="factorValue">The modifier value</param>
		/// <param name="baseValue">The base roll value</param>
		/// <returns></returns>
		[OperationContract]
		int CalculateThreatDistance(int factorValue, int baseValue);

		/// <summary>
		/// Determines whether a given unit has uninterupted access to supplies
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, IUnit> UnitIsSupplied(IUnit unit);

		/// <summary>
		/// Determines whether a given unit has access to a medical unit
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, IUnit> UnitHasMedicalSupport(IUnit unit);

		/// <summary>
		/// Determines whether a given unit is a given unit class
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="className"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, IUnit> UnitIsUnitClass(IUnit unit, string className);

		/// <summary>
		/// Determines whether a given unit is a given unit type
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="baseTypeName"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, IUnit> UnitIsUnitBaseType(IUnit unit, string baseTypeName);

		/// <summary>
		/// Determines whether the given node is valid as a deployment location for a unit
		/// being deployed froma transport
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="node"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, Tuple<IUnit, INode>> UnitIsDeployableToNode(IUnit unit, INode node);

		/// <summary>
		/// Determines whether a unit is displayed on the board during an opponent's turn
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="hiddenStealthThreshhold"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, IUnit> UnitIsHiddenFromEnemy(IUnit unit, double hiddenStealthThreshhold);

		/// <summary>
		/// Determines whether a unit can be placed at a location after being acquired as a reinforcement
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="node"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, Tuple<IUnit, INode>> UnitCanReinforceAtLocation(IUnit unit, INode node);

		/// <summary>
		/// Determines whether a unit can be refueled by a demographic or other unit at a given location
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="node"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, Tuple<IUnit, INode>> UnitCanRefuelAtLocation(IUnit unit, INode node);

		/// <summary>
		/// Determines whether a given unit is allowed to carry and deploy another unit based on the carried unit's type and class
		/// </summary>
		/// <param name="transport"></param>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, Tuple<IUnit, IUnit>> UnitCanTransportUnitTypeAndClass(IUnit transport, IUnit unit);

		/// <summary>
		/// Determines if the current unit is allowed to move from the given node in the given direction
		/// based solely on the movement hinderance if any of both the source and target nodes
		/// IMPORTANT: This does not check the unit's ability to OCCUPY a tile.
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="currentNode"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<MoveInDirectionResult, Tuple<IUnit, INode>> UnitCanMoveInDirection(IUnit unit, ITile currentNodeTile, Direction direction);

		/// <summary>
		/// Determines whether a unit has a global override based on all geographies and can effectively move anywhere on the board without hinderance within its 
		/// allowable movement points at any given time
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, IUnit> UnitHasGlobalMovementOverride(IUnit unit);

		/// <summary>
		/// Determines whether a given Moveable Component should be rendered to the board
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, IMoveableComponent> ComponentIsVisible(IMoveableComponent component);

		/// <summary>
		/// Determines whether a Moveable Component is currently being transported by another unit
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, IMoveableComponent> ComponentIsBeingTransported(IMoveableComponent component);

		/// <summary>
		/// Determines whether the given unitType is allowable for the current game/scenario
		/// </summary>
		/// <param name="unitType"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, IUnitType> UnitTypeIsAllowedTypeForScenario(IUnitType unitType);

		/// <summary>
		/// Determines whether a given faction has achieved any of its victory conditions for the game
		/// FYI: We're only basing off of 2 factions. TODO: Implemente for multiple factions
		/// </summary>
		/// <param name="faction"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, IFaction> GameVictoryAchieved(IFaction faction);

		/// <summary>
		/// Determines whether a given string name is valid to use for a base game comonent. Will also check if the name is in use.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, TComponent> NameIsValid<TComponent>(string name)
			where TComponent : class, IBaseComponent;

		/// <summary>
		/// Determines whether a given string name represents a valid available scenario.
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, string> ScenarioTitleIsValid(string title);

		/// <summary>
		/// Determines whether a given country currently has any dependant components in context.
		/// </summary>
		/// <param name="country"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, ICountry> CountryHasDependantComponents(ICountry country);

		/// <summary>
		/// Sets battle victory conditions wrapped in a service result
		/// </summary>
		/// <param name="battle"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<IBattle, IBattle> CheckBattleVictoryCondition(IBattle battle);

		/// <summary>
		/// Determines whether the given battle can continue
		/// </summary>
		/// <param name="battle"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, IBattle> BattleCanContinue(IBattle battle);

		/// <summary>
		/// Sets Skirmish victory conditions wrapped in a service result
		/// </summary>
		/// <param name="skirmish"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<ISkirmish, ISkirmish> CheckSkirmishVictoryCondition(ISkirmish skirmish);

		/// <summary>
		/// Sums and converts all StrategicAssessmentRatings properties
		/// </summary>
		/// <param name="assessment"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<StrategicAssessmentRating, StrategicAssessmentInfo> GetOverallRatingForStrategicAssessment(StrategicAssessmentInfo assessment);

		[OperationContract]
		bool DemographicIsHybrid(IDemographic demographic);

		/// <summary>
		/// Determines whether a given mission should be canceled by an order to move the assigned unit
		/// </summary>
		/// <param name="mission"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<bool, IMission> MissionCanceledByMove(IMission mission);
	} 
}
