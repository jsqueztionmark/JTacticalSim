using System;
using JTacticalSim.API.InfoObjects;
using System.Collections.Generic;
using JTacticalSim.API.AI;
using JTacticalSim.API.Media.Sound;
using JTacticalSim.Utility;

namespace JTacticalSim.API.Component
{
	public interface IUnit : IMoveableComponent, IInfoDisplayable
	{
		// Events;
		event UnitAttachedEvent UnitAttached;
		event UnitDetachedEvent UnitDetached;
		event UnitsDeployedEvent UnitsDeployed;
		event UnitsLoadedEvent UnitsLoaded;

		UnitInfo UnitInfo { get; set; }
		BattlePosture Posture { get; set; }

		/// <summary>
		/// Total movement points considering all adjustments
		/// </summary>
		int MovementPoints { get; }

		/// <summary>
		/// Total configured fire points
		/// </summary>
		int RemoteFirePoints { get; }

		/// <summary>
		/// Total Remote attack distance in spaces calculated off base modifier and real-world measurements
		/// </summary>
		int RemoteAttackDistance { get; }

		/// <summary>
		/// Total unit weight for current unit and all assigned units
		/// </summary>
		double TotalNetUnitWeight { get; }

		/// <summary>
		/// Total unit weight for current unit
		/// </summary>
		double TotalUnitWeight { get; }

		/// <summary>
		/// Total cost of unit in reinforcement points
		/// </summary>
		double UnitCost { get; }

		/// <summary>
		/// Remaining range based on fuel amount
		/// </summary>
		int CurrentFuelRange { get; set; }

		/// <summary>
		/// Percentage of remaining fuel to capacity
		/// </summary>
		double FuelLevelPercent { get; }

		void Render(int zoomLevel);
		void DisplayName();

		/// <summary>
		/// Units directly assigned to the current unit
		/// </summary>
		IEnumerable<IUnit> GetDirectAttachedUnits();

		/// <summary>
		/// All Units assigned under the current unit
		/// </summary>
		IEnumerable<IUnit> GetAllAttachedUnits();

		/// <summary>
		/// All units being transported by current unit
		/// </summary>
		/// <returns></returns>
		IEnumerable<IUnit> GetTransportedUnits();

		/// <summary>
		/// Returns the unit's current location formatted for display
		/// </summary>
		string CurrentLocation { get; }

		/// <summary>
		/// Places the unit at a specified node location based on rules for location occupation
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		IResult<IMoveableComponent, IUnit> PlaceAtLocation(INode node);

		/// <summary>
		/// Places the unit at the specified node location regardless of compatibility
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		IResult<IMoveableComponent, IUnit> PlaceAtLocationWithoutTileValidation(INode node);

		/// <summary>
		/// The Unit that the current unit is assigned to
		/// returns null if none
		/// </summary>
		IUnit AttachedToUnit { get; }

		// ORDERS:

		IResult<IUnit, IUnit> SetUnitBattlePosture(BattlePosture posture);

		/// <summary>
		/// Deploys specified units from current transport unit to a specified location based on rules for location occupation
		/// </summary>
		/// <param name="units"></param>
		/// <param name="node"></param>
		IResult<IUnit, IUnit> DeployUnits(List<IUnit> units, INode node);

		/// <summary>
		/// Moves a unit to a specified node location regardless of rules
		/// Necessary for loading to transports, etc..
		/// </summary>
		/// <param name="targetNode"></param>
		/// <param name="sourceNode"></param>
		/// <returns></returns>
		IResult<IMoveableComponent, IMoveableComponent> LoadToLocation(INode targetNode, INode sourceNode);

		/// <summary>
		/// Loads specified units to current transport
		/// </summary>
		/// <param name="units"></param>
		/// <returns></returns>
		IResult<IUnit, IUnit> LoadUnits(List<IUnit> units);

		/// <summary>
		/// Makes and attempt to assign the current unit to another unit.
		/// Returns a result with information regarding operation.
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		IResult<IUnit, IUnit> AttachToUnit(IUnit unit);

		/// <summary>
		/// Makes an attempt to detach the current unit from an asigned unit.
		/// Returns a result with information regarding the operation.
		/// </summary>
		/// <returns></returns>
		IResult<IUnit, IUnit> DetachFromUnit();

		/// <summary>
		/// Add unit as an attacker to the current battle
		/// </summary>
		/// <param name="battle"></param>
		/// <returns></returns>
		IResult<IUnit, IUnit> Attack(IBattle battle);

		/// <summary>
		/// Add unit as a defender to the current battle
		/// </summary>
		/// <param name="battle"></param>
		/// <returns></returns>
		IResult<IUnit, IUnit> Defend(IBattle battle);

		/// <summary>
		/// Creates infrastructure at tile location
		/// </summary>
		/// <param name="tile"></param>
		/// <param name="demographic"></param>
		/// <returns></returns>
		IResult<IUnit, IUnit> BuildInfrastructure(IUnitTask task, ITile tile, IDemographic demographic, Direction direction);

		/// <summary>
		/// Removes infrastructure at tile location.
		/// Will remove only the direction specified if direction is other than Direction.NONE
		/// </summary>
		/// <param name="tile"></param>
		/// <param name="demographic"></param>
		/// <returns></returns>
		IResult<IUnit, IUnit> DestroyInfrastructure(IUnitTask task, ITile tile, IDemographic demographic, Direction direction);

		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		/// <summary>
		/// All allowable unit classes for the current unit's unitType
		/// </summary>
		IEnumerable<IUnitClass> GetAllowableUnitClasses();

		IEnumerable<IPathNode> GetAllowableMovements();

		/// <summary>
		/// Determine whether this unit is allowed to perform a given task
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		IResult<bool, Tuple<IUnit, IUnitTaskType>> ValidateTask(IUnitTask task);

		/// <summary>
		/// Determine whether this unit is allowed to perform a given task by task name
		/// </summary>
		/// <param name="taskTypeName"></param>
		/// <returns></returns>
		IResult<bool, Tuple<IUnit, IUnitTaskType>> ValidateTask(string taskTypeName);

		/// <summary>
		/// Returns the net stealth value for the current unit including adjustments for the current tile's geography
		/// </summary>
		/// <returns></returns>
		double GetFullNetStealthValue();

		/// <summary>
		/// Returns the net attack value for the current unit including adjustments for the current tile's geography
		/// </summary>
		/// <returns></returns>
		double GetFullNetAttackValue();

		/// <summary>
		/// Returns the net defence value for the current unit including adjustments for the current tile's geography
		/// </summary>
		/// <returns></returns>
		double GetFullNetDefenceValue();

		/// <summary>
		/// Returns the net stealth adjustment for the type and class of a given unit
		/// </summary>
		/// <returns></returns>
		double GetNetStealthAdjustment();

		/// <summary>
		/// Returns the net attack adjustment for the type and class of a given unit
		/// </summary>
		/// <returns></returns>
		double GetNetAttackAdjustment();

		/// <summary>
		/// Returns the net attack distance adjustment for the type and class of a given unit
		/// </summary>
		/// <returns></returns>
		int GetNetAttackDistanceAdjustment();

		/// <summary>
		/// Returns the net defence adjustment for the type and class of a given unit
		/// </summary>
		/// <returns></returns>
		double GetNetDefenceAdjustment();

		/// <summary>
		/// Returns the net cost multiplier for the type and class of a given unit
		/// </summary>
		/// <returns></returns>
		double GetNetCostMultiplier();

		/// <summary>
		/// Returns the net movement adjustment for the type and class of a given unit
		/// </summary>
		/// <returns></returns>
		int GetNetMovementAdjustment();

		double GetWeight();
		double GetAllowableTransportWeight();

		/// <summary>
		/// Returns all currently assigned missions
		/// </summary>
		/// <returns></returns>
		IMission GetCurrentMission();

		/// <summary>
		/// Returns the relative 'strength' factor for a given unit based on rules criteria
		/// </summary>
		/// <returns></returns>
		double GetNetStrengthFactor();

		double GetUnitTargetDesirabilityFactor();

		bool IsCompatibleWithTransport(IUnit transport);

		/// <summary>
		/// Removes the unit from it's current stack
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		void RemoveFromStack();

		/// <summary>
		/// Fully removes the unit from the game data store, cache and current unit stack (if unit has a location)
		/// including any dependencies (assignments, transported units, etc..)
		/// </summary>
		void RemoveFromGame();

		/// <summary>
		/// Cancels any currently assigned missions
		/// </summary>
		void CancelMission();

		/// <summary>
		/// Updates the current fuel range subtracting the distance in meters based on the node count
		/// </summary>
		void ConsumeFuel(int nodeDistance);

		/// <summary>
		/// Checks the current location and attempts to refuel
		/// </summary>
		void Refuel();

		bool IsHiddenFromEnemy();
		bool IsSupplied();
		bool HasMedicalSupport();
		bool IsUnitClass(string className);
		bool IsUnitBaseType(string baseTypeName);
		bool CanDoBattleThisTurn();
		bool IsRemoteBattleCapable();
		bool IsNuclearCapable();
		bool HasPathFromNodeInDirection(Direction direction);
		bool HasUniqueHame();

		/// <summary>
		/// Determines whether this unit has at leas one currently assigned mission
		/// </summary>
		/// <returns></returns>
		bool HasCurrentMission();

		/// <summary>
		/// Determines whether this unit can occupy and claim a location for its faction
		/// </summary>
		/// <returns></returns>
		bool CanClaimLocationForFaction();

		/// <summary>
		/// Determines whether this unit can move from it's current location to an adjacent location
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		bool CanMoveOntoNode(INode target);
	}
}
