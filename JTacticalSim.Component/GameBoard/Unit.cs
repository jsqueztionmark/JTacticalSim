using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Transactions;
using JTacticalSim.API.Service;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API;
using JTacticalSim.API.Media.Sound;
using JTacticalSim.Utility;
using JTacticalSim.Component.AI;
using JTacticalSim.Media.Sound;

namespace JTacticalSim.Component.GameBoard
{
	public sealed class Unit : BoardComponentBase, IUnit
	{
		public event ComponentMoveEvent ComponentEndMove;
		public event ComponentMoveEvent ComponentBeginMove;
		public event WaypointReachedEvent WaypointReached;
		public event UnitAttachedEvent UnitAttached;
		public event UnitDetachedEvent UnitDetached;
		public event UnitsDeployedEvent UnitsDeployed;
		public event UnitsLoadedEvent UnitsLoaded;
		public event ComponentSelectedEvent ComponentSelected;
		public event ComponentUnSelectedEvent ComponentUnSelected;
		public event ComponentClickedEvent ComponentClicked;

		public int StackOrder { get; set; }
		public UnitInfo UnitInfo { get; set; }

		public CurrentMoveStatInfo CurrentMoveStats
		{
			get { return Cache().TurnMoveCache.TryFind(UID); }
			set { Cache().TurnMoveCache.TryAdd(UID, value); }
		}

		public BattlePosture Posture { get; set; }

		public int MovementPoints 
		{ 
			get 
			{ 
				var netMovement = GetNetMovementAdjustment();
				return (netMovement < 0) ? 0 : (netMovement + TheGame().BasePointValues.Movement); 
			}
		}

		public int RemoteAttackDistance
		{
			get
			{
				var netAttackDistance = GetNetAttackDistanceAdjustment();
				netAttackDistance = (netAttackDistance < 0) ? 0 : netAttackDistance;  // Base value for this should always be 0
				var r = TheGame().JTSServices.RulesService.CalculateCellCountFromRealWorldMeasurements(netAttackDistance, 
																										TheGame().BasePointValues.MeterDistanceBase, 
																										TheGame().GameBoard.DefaultAttributes.CellMeters);

				return r.Result;
			}
		}

		public int CurrentFuelRange { get; set; }

		public double FuelLevelPercent
		{
			get 
			{ 
				var fuelLevel = (UnitInfo.UnitType.FuelRange > 0) 
								? (Convert.ToDouble(CurrentFuelRange) / Convert.ToDouble(UnitInfo.UnitType.FuelRange)) * 100 
								: 0;

				return Math.Round(fuelLevel, 1);
			}
		}

		public int RemoteFirePoints
		{
			get { return UnitInfo.UnitClass.RemoteFirePoints + UnitInfo.UnitType.RemoteFirePoints; }
		}

		public double TotalUnitWeight { get { return TheGame().JTSServices.RulesService.CalculateUnitWeight(this).Result; }}
		public double TotalNetUnitWeight {get { return TheGame().JTSServices.RulesService.CalculateTotalUnitWeight(this).Result; }}
		public double UnitCost { get { return TheGame().JTSServices.RulesService.CalculateTotalRPCostForUnit(this).Result; }}
		public IUnit AttachedToUnit { get { return TheGame().JTSServices.UnitService.GetUnitAssignedToUnit(ID); }}
		public string CurrentLocation { get { return (Location != null) ? Location.ToStringForName() : string.Empty; }}

	// Constructors

		public Unit() { }
		
		public Unit(string name, ICoordinate location, UnitInfo unitInfo)
		{
			Name = name;
			Location = location;
			StackOrder = 1;
			UnitInfo = unitInfo;
			CurrentMoveStats = new CurrentMoveStatInfo();

			// SoundSystem events
			// When in the reinforcement screen, we pass null info then construct the unit
			if (UnitInfo.UnitClass != null && unitInfo.UnitType != null)
			{
				UnitInfo.UnitClass.Sounds.PlayFinished += On_PlaySoundFinished;
				UnitInfo.UnitType.Sounds.PlayFinished += On_PlaySoundFinished;
			}			

			// Add Move stats to cache if on board
			if (location != null)
			{
				Cache().TurnMoveCache.TryAdd(UID, new CurrentMoveStatInfo
										{ 
											HasPerformedAction = false,
											MovementPoints = MovementPoints,
											RemoteFirePoints = RemoteFirePoints
										});
			}
			
		}


		public void Render(int zoomLevel)
		{
			TheGame().Renderer.On_UnitPreRender(new EventArgs());
			TheGame().Renderer.RenderUnit(this, zoomLevel);
			TheGame().Renderer.On_UnitPostRender(new EventArgs());
		}

		public void DisplayInfo()
		{
			TheGame().Renderer.DisplayUnitInfo(this);
		}

		public void DisplayName()
		{
			TheGame().Renderer.RenderUnitName(this);
		}

		public void RemoveFromStack()
		{
			var stack = this.GetCurrentStack();
			if (stack != null) this.GetCurrentStack().RemoveUnit(this);
		}

		public void RemoveFromGame()
		{
			var r = TheGame().JTSServices.UnitService.RemoveUnitFromGame(this);

			// TODO: handle other than UI display
		}

		public void CancelMission()
		{
			if (GetCurrentMission() == null)
				return;

			var r = TheGame().JTSServices.AIService.CancelMission(GetCurrentMission());
			HandleResultDisplay(r, true);
		}

		public void ConsumeFuel(int nodeDistance)
		{
			if (!UnitInfo.UnitType.FuelConsumer)
				return;

		    var result = TheGame().JTSServices.UnitService.UpdateUnitFuelRange(this, nodeDistance);

			HandleResultDisplay(result, true);
		}

		public void Select()
		{
			var r = TheGame().GameBoard.AddSelectedUnit(this);
			On_ComponentSelected(new ComponentSelectedEventArgs());

			HandleResultDisplay(r, true);
		}

		public void UnSelect()
		{
			var r = TheGame().GameBoard.RemoveSelectedUnit(this);
			On_ComponentUnSelected(new ComponentUnSelectedEventArgs());

			HandleResultDisplay(r, true);
		}

	// Rules 

		public bool IsHiddenFromEnemy()
		{
			return TheGame().JTSServices.RulesService.UnitIsHiddenFromEnemy(this, TheGame().BasePointValues.HiddenStealthThreshhold).Result;
		}

		public bool IsSupplied()
		{
			return TheGame().JTSServices.RulesService.UnitIsSupplied(this).Result;
		}

		public bool IsCompatibleWithTransport(IUnit transport)
		{
			return TheGame().JTSServices.RulesService.UnitCanTransportUnitTypeAndClass(transport, this).Result;
		}

		public bool HasMedicalSupport()
		{
			return TheGame().JTSServices.RulesService.UnitHasMedicalSupport(this).Result;
		}

		public bool IsUnitClass(string className)
		{
			IResult<bool, IUnit> r = TheGame().JTSServices.RulesService.UnitIsUnitClass(this, className);
			//TODO: Handle result
			return r.Result;
		}

		public bool IsUnitBaseType(string baseTypeName)
		{
			IResult<bool, IUnit> r = TheGame().JTSServices.RulesService.UnitIsUnitBaseType(this, baseTypeName);
			//TODO: Handle result
			return r.Result;
		}

		public bool CanDoBattleThisTurn()
		{
			return TheGame().JTSServices.RulesService.UnitCanDoBattle(this).Result;
		}

		public bool CanClaimLocationForFaction()
		{
			return TheGame().JTSServices.RulesService.UnitCanClaimNodeForFaction(this).Result;
		}

		public bool CanMoveOntoNode(INode target)
		{
			return TheGame().JTSServices.RulesService.UnitCanMoveOntoNode(this, target).Result;
		}

		public bool IsRemoteBattleCapable()
		{
			return (RemoteAttackDistance > 0);
		}

		public bool IsNuclearCapable()
		{
			return UnitInfo.UnitType.Nuclear;
		}

		public bool HasPathFromNodeInDirection(Direction direction)
		{
			return TheGame().JTSServices.RulesService.UnitCanMoveInDirection(this, GetNode().DefaultTile, direction).Result.CanMoveInDirection;
		}

		public bool HasUniqueHame()
		{
			return TheGame().JTSServices.RulesService.UnitNameIsUnique(Name).Result;
		}

		public bool HasCurrentMission()
		{
			return (GetCurrentMission() != null);
		}

#region Unit Methods

		public IEnumerable<IUnit> GetDirectAttachedUnits() { return TheGame().JTSServices.UnitService.GetUnitsByUnitAssignment(ID); }
		public IEnumerable<IUnit> GetAllAttachedUnits() { return TheGame().JTSServices.UnitService.GetUnitsByUnitAssignmentRecursive(ID); }
		public IEnumerable<IUnit> GetTransportedUnits() { return TheGame().JTSServices.UnitService.GetUnitsByTransport(ID); }
		public IEnumerable<IUnitClass> GetAllowableUnitClasses() { return TheGame().JTSServices.UnitService.GetAllowableUnitClassesForUnit(this).Result; }
		public IEnumerable<IPathNode> GetAllowableMovements() { return TheGame().JTSServices.NodeService.GetAllowableMovementNodesForUnit(this); }
		public double GetNetStealthAdjustment()
		{
			double postureBonus = 0;

			switch (Posture)
			{
				case BattlePosture.EVASION:
					{
						postureBonus += TheGame().BasePointValues.BattlePostureBonus;
						break;
					}
				case BattlePosture.OFFENSIVE:
					{
						postureBonus -= TheGame().BasePointValues.BattlePostureBonus;
						break;
					}
			}

			var r = UnitInfo.UnitClass.StealthModifier + UnitInfo.UnitType.StealthModifier + postureBonus;
			return r;
		}
		public double GetNetAttackAdjustment()
		{
			double postureBonus = 0;

			switch (Posture)
			{
				case BattlePosture.OFFENSIVE:
					{
						postureBonus += TheGame().BasePointValues.BattlePostureBonus;
						break;
					}
				case BattlePosture.DEFENSIVE:
					{
						postureBonus -= TheGame().BasePointValues.BattlePostureBonus;
						break;
					}
			}

			var r = UnitInfo.UnitClass.AttackModifier + UnitInfo.UnitType.AttackModifier + postureBonus;
			return r;
		}
		public double GetNetDefenceAdjustment()
		{
			double postureBonus = 0;

			switch (Posture)
			{
				case BattlePosture.DEFENSIVE:
					{
						postureBonus += TheGame().BasePointValues.BattlePostureBonus;
						break;
					}
				case BattlePosture.OFFENSIVE:
					{
						postureBonus -= TheGame().BasePointValues.BattlePostureBonus;
						break;
					}
			}

			var r = UnitInfo.UnitClass.DefenceModifier + UnitInfo.UnitType.DefenceModifier + postureBonus;
			return r;
		}
		public double GetFullNetStealthValue() { return TheGame().JTSServices.RulesService.CalculateUnitStealthValueForCurrentGeog(this); }
		public double GetFullNetAttackValue() { return TheGame().JTSServices.RulesService.CalculateUnitAttackValueForCurrentGeog(this); }
		public double GetFullNetDefenceValue() { return TheGame().JTSServices.RulesService.CalculateUnitDefenceValueForCurrentGeog(this); }
		public int GetNetAttackDistanceAdjustment() { return Convert.ToInt32(Math.Round(UnitInfo.UnitClass.AttackDistanceModifier)) + Convert.ToInt32(Math.Round(UnitInfo.UnitType.AttackDistanceModifier)); }
		public double GetNetCostMultiplier() { return UnitInfo.UnitClass.UnitCostModifier +	UnitInfo.UnitType.UnitCostModifier;	}
		public int GetNetMovementAdjustment() {	return Convert.ToInt32(Math.Round(UnitInfo.UnitClass.MovementModifier)) + Convert.ToInt32(Math.Round(UnitInfo.UnitType.MovementModifier)); }
		public double GetWeight() {	return TheGame().JTSServices.RulesService.CalculateUnitWeight(this).Result;	}
		public double GetNetStrengthFactor() { return TheGame().JTSServices.RulesService.CalculateUnitStrength(this); }
		public double GetUnitTargetDesirabilityFactor()	{ return TheGame().JTSServices.RulesService.CalculateTargetDesirabilityForUnit(this); }
		public double GetAllowableTransportWeight()	{ return TheGame().JTSServices.RulesService.CalculateAllowableTransportWeight(this).Result; }
		public IMission GetCurrentMission() { return TheGame().JTSServices.AIService.GetCurrentAssignedMissionForUnit(this).Result; } 

		public void ResetMoveStats()
		{
			CurrentMoveStats.MovementPoints = MovementPoints;
			CurrentMoveStats.RemoteFirePoints = RemoteFirePoints;
		}

#endregion

#region Unit Orders/Tasks

		public IResult<IUnit, IUnit> SetUnitBattlePosture(BattlePosture posture)
		{
			var r = new OperationResult<IUnit, IUnit> {Status = ResultStatus.SUCCESS, Result = this};

			if (CurrentMoveStats.HasPerformedAction)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Unit requires an available action to set battle posture.");
				r.FailedObjects.Add(this);
				return r;
			}

			try
			{
				Posture = posture;
				CurrentMoveStats.HasPerformedAction = true;

				r.Messages.Add("Unit's battle posture changed to {0}".F(posture.ToString()));
				r.SuccessfulObjects.Add(this);
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				r.FailedObjects.Add(this);
				return r;
			}

			return r;
		}

		public IResult<IMoveableComponent, IUnit> PlaceAtLocation(INode node)
		{
			var r = new OperationResult<IMoveableComponent, IUnit> {Status = ResultStatus.SUCCESS, Result = this};
			var serviceResult = TheGame().JTSServices.RulesService.UnitCanReinforceAtLocation(this, node);

			// Can the unit occupy the space?
			// if not - return the result info.
			if (!serviceResult.Result)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("{0} can not be placed at selected location. {1}".F(Name, serviceResult.Message));
				r.FailedObjects.Add(this);
				return r;
			}

			using (var txn = new TransactionScope())
			{
				// Update the unit location
				StackOrder = node.VisibleUnitCount() + 1;
				TheGame().JTSServices.UnitService.UpdateUnitLocation(this, node, null);

				var targetTile = node.DefaultTile;
				targetTile.AddComponentsToStacks(new[] {this});
			
				// Update data and cache
				TheGame().JTSServices.TileService.UpdateTiles(new List<ITile> { targetTile });

				node.ResetUnitStackOrder();

				txn.Complete();

				r.SuccessfulObjects.Add(this);
				return r;
			}
		}

		public IResult<IMoveableComponent, IUnit> PlaceAtLocationWithoutTileValidation(INode node)
		{
			var r = new OperationResult<IMoveableComponent, IUnit> { Status = ResultStatus.SUCCESS, Result = this };

			using (var txn = new TransactionScope())
			{
				// Update the unit location
				StackOrder = node.VisibleUnitCount() + 1;
				TheGame().JTSServices.UnitService.UpdateUnitLocation(this, node, null);

				var targetTile = node.DefaultTile;
				targetTile.AddComponentsToStacks(new[] { this });

				// Update data and cache
				TheGame().JTSServices.TileService.UpdateTiles(new List<ITile> { targetTile });

				node.ResetUnitStackOrder();

				txn.Complete();

				r.SuccessfulObjects.Add(this);
				return r;
			}
		}

		public IResult<IMoveableComponent, IMoveableComponent> LoadToLocation(INode targetNode, INode sourceNode)
		{
			var r = new OperationResult<IMoveableComponent, IMoveableComponent>{Status = ResultStatus.SUCCESS, Result = this};
			var v = ValidateTask("MoveToLocation");

			// validate task
			if (!v.Result)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add(v.Message);
				r.FailedObjects.Add(this);
				return r;
			}

			using (var txn = new TransactionScope())
			{
				try
				{
					TheGame().JTSServices.UnitService.UpdateUnitLocation(this, targetNode, sourceNode);
				
					var sourceTile = sourceNode.DefaultTile;
					var targetTile = targetNode.DefaultTile;
				
					// TODO: update this with which stack PER NODE to add to and remove from
					sourceTile.RemoveComponentsFromStacks(new List<IUnit> {this});
					targetTile.AddComponentsToStacks(new List<IUnit> {this});

					// Update data and cache
					TheGame().JTSServices.TileService.UpdateTiles(new List<ITile> { sourceTile, targetTile });
				}
				catch (Exception ex)
				{
					r.Status = ResultStatus.EXCEPTION;
					r.FailedObjects.Add(this);
					r.ex = ex;
				}

				txn.Complete();

				r.SuccessfulObjects.Add(this);
				return r;
			}			
		}

		public IResult<IMoveableComponent, IMoveableComponent> MoveToLocation(INode targetNode, INode sourceNode)
		{
			var r = new OperationResult<IMoveableComponent, IMoveableComponent> { Status = ResultStatus.SUCCESS, Result = this };
			var v = ValidateTask("MoveToLocation");

			// validate task
			if (!v.Result)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add(v.Message);
				r.FailedObjects.Add(this);
				return r;
			}

			// 1. Check and refuel - allows for supply lines to establish a way to fuel
			// 2. Get route
			// 3. Move one node at a time
			// 4. Update location including SubNodeLocation for each node
			//		 -- if unit has not been removed in combat
			// 5. Interact as appropriate for each node --- On_WaypointReached	
			// 6. Consume fuel and act on results

			if (TheGame().GameBoard.CurrentRoute == null)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("There is no route available for {0} to selected location.".F(Name));
				r.FailedObjects.Add(this);
				return r;
			}

			var route = TheGame().GameBoard.CurrentRoute.Nodes.Reverse().Skip(1).ToArray();

			INode lastNode = sourceNode;

			var allUnitsToMove = new List<IUnit> {this};
			if (GetTransportedUnits().Any()) allUnitsToMove.AddRange(GetTransportedUnits());

			On_ComponentBeginMove(new ComponentMoveEventArgs(sourceNode, targetNode));

			foreach (var pNode in route)
			{
				try
				{
					// Allow for removal of the unit during forced engagements
					if (!this.ExistsInContext()) return r;

					var node = pNode.GetNode();

					// If this node is not compatible with the unit, or the unit is out of movement points (RouteType.FASTESTUNIT)
					// Stop at current node and drop out of the route
					if (!this.CanMoveOntoNode(node))
					{
						On_ComponentEndMove(new ComponentMoveEventArgs(sourceNode, node));
						r.Status = ResultStatus.SUCCESS;
						return r;
					}	

					// Update all units within the scope of the move
					allUnitsToMove.ForEach(u => TheGame().JTSServices.UnitService.UpdateUnitLocation(u, node, lastNode));
				
					var sourceTile = lastNode.DefaultTile;
					var targetTile = node.DefaultTile;
				
					// Remove all from the current unit stack and add to the stack at the target location
					// TODO: update this with which stack PER NODE to add to and remove from
					sourceTile.RemoveComponentsFromStacks(allUnitsToMove);
					targetTile.AddComponentsToStacks(allUnitsToMove);

					// Update data and cache
					TheGame().JTSServices.TileService.UpdateTiles(new List<ITile> { sourceTile, targetTile });

					lastNode = node;

					if (r.Status == ResultStatus.SUCCESS)
						On_WaypointReached(new ComponentMoveEventArgs(sourceNode, node));

					// Are we out of fuel at this point?
					if (UnitInfo.UnitType.FuelConsumer && CurrentFuelRange <= TheGame().GameBoard.DefaultAttributes.CellMeters)
					{
						// Will attempt to refuel
						// either way, we stop here
						On_ComponentEndMove(new ComponentMoveEventArgs(sourceNode, node));

						// Display what happened to the user based on the result
						var fuelResult = TheGame().JTSServices.AIService.HandleZeroFuelForUnit(this);
						r.Status = ResultStatus.FAILURE;
						r.Messages.Add(fuelResult.Message);
						return r;
					}
				}
				catch (Exception ex)
				{
					r.Status = ResultStatus.EXCEPTION;
					r.FailedObjects.AddRange(allUnitsToMove);
					r.ex = ex;
				}
			}

			// Cancel any missions cancelable by unit move
			var currentMission = GetCurrentMission();
			if (currentMission != null && TheGame().JTSServices.RulesService.MissionCanceledByMove(currentMission).Result)
			{
				TheGame().JTSServices.AIService.CancelMission(currentMission);					
			}

			// Fire event on completion of move
			if (r.Status == ResultStatus.SUCCESS)
				On_ComponentEndMove(new ComponentMoveEventArgs(sourceNode, targetNode));

			r.SuccessfulObjects.AddRange(allUnitsToMove);
			return r;
		}

		public IResult<IUnit, IUnit> AttachToUnit(IUnit unit)
		{
			var attachResult = TheGame().JTSServices.AIService.AttachUnitToUnit(unit, this);

			if (attachResult.Status == ResultStatus.SUCCESS)
				On_UnitAttached(new EventArgs());

			return attachResult;
		}

		public IResult<IUnit, IUnit> DetachFromUnit()
		{
			var detachResult = TheGame().JTSServices.AIService.DetachUnitFromUnit(this);

			if (detachResult.Status == ResultStatus.SUCCESS)
				On_UnitDetached(new EventArgs());

			return detachResult;
		}

		public IResult<IUnit, IUnit> DeployUnits(List<IUnit> units, INode node)
		{
			var r = new OperationResult<IUnit, IUnit>();
			var vResult = ValidateTask("DeployUnits");

			// validate task
			if (!vResult.Result)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add(vResult.Message);
				r.FailedObjects.Add(this);
				return r;
			}

			var deployedResult = TheGame().JTSServices.AIService.DeployUnitsFromTransportToNode(this, units, node);

			if (deployedResult.Status == ResultStatus.SUCCESS)
				On_UnitsDeployed(new UnitsDeployedEventArgs(units));

			return deployedResult;
			
		}

		public IResult<IUnit, IUnit> LoadUnits(List<IUnit> units)
		{
			var r = new OperationResult<IUnit, IUnit>();
			var vResult = ValidateTask("LoadUnits");

			// validate task
			if (!vResult.Result)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add(vResult.Message);
				r.FailedObjects.Add(this);
				return r;
			}

			var loadedResult = TheGame().JTSServices.AIService.LoadUnitsToTransport(this, units);

			if (loadedResult.Status == ResultStatus.SUCCESS || loadedResult.Status == ResultStatus.SOME_FAILURE)
			    On_UnitsLoaded(new UnitsLoadedEventArgs(units));

			return loadedResult;
		}

		public IResult<IUnit, IUnit> Attack(IBattle battle)
		{
			var r = new OperationResult<IUnit, IUnit> { Status = ResultStatus.SUCCESS, Result = this };
			var v = ValidateTask("Attack");

			// validate task
			if (!v.Result)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add(v.Message);
				r.FailedObjects.Add(this);
				return r;
			}

			battle.AddAttacker(this);

			return r;
		}

		public IResult<IUnit, IUnit> Defend(IBattle battle)
		{
			var r = new OperationResult<IUnit, IUnit> { Status = ResultStatus.SUCCESS, Result = this };
			var v = ValidateTask("Defend");

			// validate task
			if (!v.Result)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add(v.Message);
				r.FailedObjects.Add(this);
				return r;
			}

			battle.AddDefender(this);

			return r;
		}

		public IResult<IUnit, IUnit> BuildInfrastructure(IUnitTask task, ITile tile, IDemographic demographic, Direction direction)
		{
			var r = new OperationResult<IUnit, IUnit>();

			if (CurrentMoveStats.HasPerformedAction)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Unit requires an available action to build infrastructure.");
				r.FailedObjects.Add(this);
				return r;
			}

			var vResult = ValidateTask("BuildInfrastructure");

			// validate task
			if (!vResult.Result)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add(vResult.Message);
				r.FailedObjects.Add(this);
				return r;
			}

			// spend the action point
			CurrentMoveStats.HasPerformedAction = true;

			if (task.IsComplete)
			{
				demographic.Orientation = new List<Direction>() { direction };
				var sResult = TheGame().JTSServices.AIService.BuildInfrastructure(this, tile, demographic);
				TheGame().Renderer.RefreshNodes(new[] {tile.GetNode()});

				return sResult;
			}

			r.Status = ResultStatus.SUCCESS;
			r.Messages.Add("{0} is currently building a {1} and has {2} turns to complete.".F(Name, demographic.DemographicClass.Name, task.TurnsToComplete));
			r.SuccessfulObjects.Add(this);
			return r;
			
		}

		public IResult<IUnit, IUnit> DestroyInfrastructure(IUnitTask task, ITile tile, IDemographic demographic, Direction direction)
		{
			var r = new OperationResult<IUnit, IUnit>();

			if (CurrentMoveStats.HasPerformedAction)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Unit requires an available action to demolish infrastructure.");
				r.FailedObjects.Add(this);
				return r;
			}

			var vResult = ValidateTask("DestroyInfrastructure");

			// validate task
			if (!vResult.Result)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add(vResult.Message);
				r.FailedObjects.Add(this);
				return r;
			}

			// spend the action point
			CurrentMoveStats.HasPerformedAction = true;

			if (task.IsComplete)
			{
				var sResult = TheGame().JTSServices.AIService.DestroyInfrastructure(this, tile, demographic, direction);
				TheGame().Renderer.RefreshNodes(new[] {tile.GetNode()});
				return sResult;
			}

			r.Status = ResultStatus.SUCCESS;
			r.Messages.Add("{0} is currently Demolishing a {1} and has {2} turns to complete.".F(Name, demographic.DemographicClass.Name, task.TurnsToComplete));
			r.SuccessfulObjects.Add(this);
			return r;
		}

		public void Refuel()
		{
			var result = TheGame().JTSServices.AIService.AttemptRefuelUnit(this);
			HandleResultDisplay(result, true);
		}


		public IResult<bool, Tuple<IUnit, IUnitTaskType>> ValidateTask(IUnitTask task)
		{
			var taskResult = TheGame().JTSServices.RulesService.UnitCanPerformTask(this, task.TaskType);
			return taskResult;
		}

		public IResult<bool, Tuple<IUnit, IUnitTaskType>> ValidateTask(string taskTypeName)
		{
			var taskType = TheGame().JTSServices.GenericComponentService.GetByName<UnitTaskType>(taskTypeName);
			var taskResult = TheGame().JTSServices.RulesService.UnitCanPerformTask(this, taskType);
			return taskResult;
		}

	// Tasks not initiated by the user

#endregion

#region Sounds

		public override void PlaySound(SoundType soundType)
		{
			if (UnitInfo.UnitClass.Sounds.Exists(soundType).Result)
				UnitInfo.UnitClass.Sounds.Play(soundType);				
			else
				UnitInfo.UnitType.Sounds.Play(soundType);
		}

		public override void PlaySoundAsync(SoundType soundType)
		{
			if (UnitInfo.UnitClass.Sounds.Exists(soundType).Result)
				UnitInfo.UnitClass.Sounds.PlayAsync(soundType);				
			else
				UnitInfo.UnitType.Sounds.PlayAsync(soundType);
		}

		public override void StopSoundPlayback()
		{
			UnitInfo.UnitClass.Sounds.StopPlayback();				
			UnitInfo.UnitType.Sounds.StopPlayback();
		}

#endregion

#region Event Handlers

		public void On_ComponentClicked(object sender, ComponentClickedEventArgs e)
		{
			MouseButton clicked = e.ButtonClicked;

			switch (clicked)
			{
				case MouseButton.LEFT :
				case MouseButton.RIGHT :
				case MouseButton.DOUBLELEFT :
				case MouseButton.DOUBLERIGHT :
					{
						if (this.GetCurrentStack() != null) this.GetCurrentStack().On_ComponentClicked(this, e);
						break;
					}
					
			}

			if (ComponentClicked != null) ComponentClicked(this, e);
			
		}

		public void On_ComponentSelected(ComponentSelectedEventArgs e)
		{	
			if (TheGame().GameBoard.CurrentRoute == null)
				TheGame().GameBoard.ShowMoveRadius();

			// This allows the unit to detect if a supply line has been established 
			Refuel();

			if (ComponentSelected != null) ComponentSelected(this, e);
		}

		public void On_ComponentUnSelected(ComponentUnSelectedEventArgs e)
		{
			if (ComponentUnSelected != null) ComponentUnSelected(this, e);
		}

		// Private

		private void On_UnitAttached(EventArgs e)
		{
			if (UnitAttached != null) UnitAttached(this, e);
		}

		private void On_UnitDetached(EventArgs e)
		{
			if (UnitDetached != null) UnitDetached(this, e);
		}

		private void On_UnitsDeployed(UnitsDeployedEventArgs e)
		{
			if (UnitsDeployed != null) UnitsDeployed(this, e);
		}

		private void On_UnitsLoaded(UnitsLoadedEventArgs e)
		{
			if (UnitsLoaded != null) UnitsLoaded(this, e);
		}

		private void On_ComponentBeginMove(ComponentMoveEventArgs e)
		{
			if (ComponentBeginMove != null) ComponentBeginMove(this, e);
			PlaySoundAsync(SoundType.MOVE);
		}

		private void On_ComponentEndMove(ComponentMoveEventArgs e)
		{
			StopSoundPlayback();
			e.SourceNode.ResetUnitStackOrder();
			e.TargetNode.ResetUnitStackOrder();

			// Allows for automatic refueling when possible
			Refuel();

			if (ComponentEndMove != null) ComponentEndMove(this, e);
		}

		private void On_WaypointReached(ComponentMoveEventArgs e)
		{
			// This is handling only for nodes in a route where we're
			// moving 1 node at a time. Maybe extend this to work with multiple node
			// waypoints in the future... right now, not doin' that.
			CurrentMoveStats.MovementPoints -= 1;
			ConsumeFuel(1);

			// make attempt to claim the new node for our faction
			TheGame().JTSServices.AIService.ClaimNodeForVictorFaction(new List<IUnit>{this}, e.TargetNode);
			
			if (WaypointReached != null) WaypointReached(this, e);
			TheGame().Renderer.RefreshActiveRoute();

			if (!e.TargetNode.Equals(TheGame().GameBoard.CurrentRoute.Target))
				TheGame().BattleHandler.ForceMovementBattleEngagement(e.TargetNode);

			Thread.Sleep(500);
		}

#endregion

	}
}
