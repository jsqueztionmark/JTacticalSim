using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.Component.AI;
using JTacticalSim.Component.Data;
using JTacticalSim.Component.GameBoard;
using JTacticalSim.Utility;

namespace JTacticalSim.Component
{
	public abstract class InputCommandHandlerBase : BaseGameObject, IInputCommandHandler
	{
		protected bool _inputError = false;
		protected ParsedCommandArgs _commandArgs;
		protected int CurrentCMDRow;

		protected InputCommandHandlerBase()
			: base(GameObjectType.HANDLER)
		{}

		public abstract void GetCommandInput(ICommandInterface ci);

		public virtual void HandleInput(ICommandInterface ci, StateType state)
		{
			switch (state)
			{
				case StateType.GAME_IN_PLAY:
					{
						Handle_GAME_IN_PLAY_Input(ci);
						return;
					}
				case StateType.AI_IN_PLAY:
					{
						Handle_AI_IN_PLAY_Input(ci);
						return;
					}
				case StateType.BATTLE:
					{
						Handle_BATTLE_Input(ci);
						return;
					}
				case StateType.GAME_OVER:
					{
						Handle_GAME_OVER_Input(ci);
						return;
					}
				case StateType.HELP:
					{
						Handle_HELP_Input(ci);
						return;
					}
				case StateType.QUICK_SELECT:
					{
						Handle_QUICK_SELECT_Input(ci);
						return;
					}
				case StateType.REINFORCE:
					{
						Handle_REINFORCE_Input(ci);
						return;
					}
				case StateType.SCENARIO_INFO:
					{
						Handle_SCENARIO_INFO_Input(ci);
						return;
					}
				case StateType.SETTINGS_MENU:
					{
						Handle_SETTINGS_MENU_Input(ci);
						return;
					}
				case StateType.SPLASH_SCREEN:
					{
						Handle_SPLASH_SCREEN_Input(ci);
						return;
					}
				case StateType.TITLE_MENU:
					{
						Handle_TITLE_MENU_Input(ci);
						return;
					}
				default:
					{
						Handle_GAME_IN_PLAY_Input(ci);
						return;
					}
			}
		}

		protected abstract void Handle_GAME_IN_PLAY_Input(ICommandInterface ci);
		protected abstract void Handle_AI_IN_PLAY_Input(ICommandInterface ci);
		protected abstract void Handle_BATTLE_Input(ICommandInterface ci);
		protected abstract void Handle_GAME_OVER_Input(ICommandInterface ci);
		protected abstract void Handle_HELP_Input(ICommandInterface ci);
		protected abstract void Handle_QUICK_SELECT_Input(ICommandInterface ci);
		protected abstract void Handle_REINFORCE_Input(ICommandInterface ci);
		protected abstract void Handle_SCENARIO_INFO_Input(ICommandInterface ci);
		protected abstract void Handle_SETTINGS_MENU_Input(ICommandInterface ci);
		protected abstract void Handle_SPLASH_SCREEN_Input(ICommandInterface ci);
		protected abstract void Handle_TITLE_MENU_Input(ICommandInterface ci);

#region Interface

	// Game

		public virtual void Exit()
		{
			TheGame().NullGame();
			Environment.Exit(1);
		}

		public virtual void Play()
		{
			TheGame().Start();
			DisplayMainBoardScreen();
		}

		public virtual void LoadGame()
		{
			throw new NotImplementedException();			
		}

		public virtual void NewGame()
		{
			throw new NotImplementedException();
		}

		public virtual void SaveGame()
		{
			var cResult = TheGame().Save(TheGame().LoadedGame);
			HandleResultDisplay(cResult, false);
		}

		public virtual void SaveGameAs()
		{
			throw new NotImplementedException();
		}

		public virtual void DeleteGame()
		{
			throw new NotImplementedException();
		}

	// Screens

		public virtual void DisplayMainBoardScreen()
		{
			TheGame().Renderer.RenderMainScreen();
		}

		public virtual void DisplayReinforcementsScreen()
		{
			GetReinforcements();	
		}

		public virtual void DisplayUnitQuickSelectScreen()
		{
			TheGame().StateSystem.ChangeState(StateType.QUICK_SELECT);
		}

		public virtual void DisplayScenarioInfoScreen()
		{
			TheGame().StateSystem.ChangeState(StateType.SCENARIO_INFO);
		}

		public virtual void DisplayTitleScreen()
		{
			TheGame().StateSystem.ChangeState(StateType.TITLE_MENU);
		}

		public virtual void DisplayHelpScreen()
		{
			TheGame().StateSystem.ChangeState(StateType.HELP);
		}

		public virtual void ZoomMap(CycleDirection direction)
		{
			TheGame().GameBoard.Zoom(direction);
		}

		public virtual void CycleMapMode(CycleDirection direction)
		{
			TheGame().Renderer.CycleMapMode(direction);
		}

		public virtual void DisplayCommandList()
		{			
			throw new NotImplementedException();
		}

		public virtual void EndTurn()
		{
			TheGame().CurrentTurn.End();	
		}

		public virtual void DisplayUnit()
		{
			if (!TheGame().GameBoard.SelectedUnits.Any())
			{
				SetInputError("No units currently selected.");
				return;
			}

			TheGame().GameBoard.SelectedUnits.First().DisplayInfo();
		}

		public virtual void DisplayPlayer()
		{
			TheGame().CurrentTurn.Player.DisplayInfo();
		}

		public virtual void DisplayAssignedUnits()
		{
			throw new NotImplementedException();
		}

		public virtual void DisplayUnits()
		{
			throw new NotImplementedException();
		}

		public virtual void SetCurrentNode()
		{
			throw new NotImplementedException();
		}

		public virtual void SetSelectedUnit()
		{
			var sourceNode = (TheGame().GameBoard.SelectedUnits.Any()) ? TheGame().GameBoard.SelectedUnits.First().GetNode() : null;
			var selectNode = TheGame().GameBoard.SelectedNode;

			var unit = selectNode.DefaultTile
                                .GetCountryComponentStack(TheGame().CurrentTurn.Player.Country)
								.GetTopUnit();
	
			if (unit == null)
				return;

			TheGame().GameBoard.ClearSelectedItems(false);
			unit.Select();
			selectNode.Select();

			TheGame().Renderer.RefreshActiveNodes();
			TheGame().Renderer.RefreshNodes(new[] { sourceNode });

			TheGame().Renderer.RenderTileUnitInfoArea();
		}

		public virtual void SetSelectedUnits()
		{
			var sourceNode = (TheGame().GameBoard.SelectedUnits.Any()) ? TheGame().GameBoard.SelectedUnits.First().GetNode() : null;
			var selectNode = TheGame().GameBoard.SelectedNode;

			var units = selectNode.DefaultTile
								.GetCountryComponentStack(TheGame().CurrentTurn.Player.Country)
								.GetAllUnits();
	
			if (units == null || units.Count < 1)
				return;

			TheGame().GameBoard.ClearSelectedItems(false);

			units.ToList().ForEach(unit => unit.Select());

			selectNode.Select();
			TheGame().Renderer.RefreshActiveNodes();
			TheGame().Renderer.RefreshNodes(new[] { sourceNode });

			TheGame().Renderer.RenderTileUnitInfoArea();
		}

		public virtual void SetSelectedUnitWithAttached()
		{
			var sourceNode = (TheGame().GameBoard.SelectedUnits.Any()) ? TheGame().GameBoard.SelectedUnits.First().GetNode() : null;
			var selectNode = TheGame().GameBoard.SelectedNode;

			var unit = selectNode.DefaultTile
                                .GetCountryComponentStack(TheGame().CurrentTurn.Player.Country)
								.GetTopUnit();

			if (unit == null)
				return;

			var attached = unit.GetAllAttachedUnits().Where(u => u.LocationEquals(unit.Location)).ToList();

			TheGame().GameBoard.ClearSelectedItems(false);
			unit.Select();
			attached.ForEach(u => u.Select());
			selectNode.Select();

			TheGame().Renderer.RefreshActiveNodes();
			TheGame().Renderer.RefreshNodes(new[] { sourceNode });

			TheGame().Renderer.RenderTileUnitInfoArea();
		}

		public virtual void UnselectAllUnits()
		{
			var sourceNode = (TheGame().GameBoard.SelectedUnits.Any()) ? TheGame().GameBoard.SelectedUnits.First().GetNode() : null;
			var selectNode = TheGame().GameBoard.SelectedNode;

			TheGame().GameBoard.ClearSelectedItems(false);
			TheGame().GameBoard.SelectedNode = selectNode;
			TheGame().Renderer.RefreshActiveNodes();
			TheGame().Renderer.RefreshNodes(new[] { sourceNode });
			TheGame().Renderer.RenderTileUnitInfoArea();
		}

		public virtual void DisplayCurrentNode()
		{
			TheGame().GameBoard.SelectedNode.DisplayInfo();	
		}

		public virtual void CycleUnits()
		{
			// We want to scroll the units only if we are not freshly selecting a unit
			// This requires us to determine not only if any are selected, but also if
			// we are now on a different node
			if (TheGame().GameBoard.SelectedUnits.Any() &&
				TheGame().GameBoard.SelectedNode.Equals(TheGame().GameBoard.SelectedUnits.First().GetNode()))
				{
					var n = GetSelectedNode();
					if (n == null) return;

					n.DefaultTile.GetCountryComponentStack(TheGame().CurrentTurn.Player.Country).CycleUnits();
				}	
			SetSelectedUnit();					
		}

		public virtual void AddUnit()
		{
			throw new NotImplementedException();
		}

		public virtual void RemoveUnit()
		{
			throw new NotImplementedException();
		}

		public virtual void EditUnit()
		{
			throw new NotImplementedException();
		}


	// Unit tasks

		public virtual void MoveUnitsToSelectedNode()
		{
			var units = TheGame().GameBoard.SelectedUnits;
			var targetNode = TheGame().GameBoard.SelectedNode;

			if (!units.Any())
				return;

			// Validate that max units are not violated
			if (targetNode.DefaultTile.WillExceedMaxUnits(units))
			{
				SetInputError("The target location will exceed the maximum units. Please select fewer units to move.");
				return;
			}

			if (units == null || units.Count == 0)
				return;

			var results = new List<IResult<IMoveableComponent, IMoveableComponent>>();

			foreach (var u in units.ToArray())
			{
				// Are we out of fuel at this point?
				if (u.UnitInfo.UnitType.FuelConsumer && u.CurrentFuelRange <= TheGame().GameBoard.DefaultAttributes.CellMeters)
				{
					TheGame().Renderer.DisplayUserMessage(MessageDisplayType.ERROR, "{0} is out of fuel and can not move.".F(u.Name, u.FuelLevelPercent), null);
					continue;
				}

				// Check for currently assigned missions that will be canceled
				var willCancelMission = (u.GetCurrentMission() != null) && u.GetCurrentMission().MissionType.CanceledByMove;
				if (willCancelMission)
				{
					var proceed = TheGame().Renderer.ConfirmAction("{0} has a currently assigned mission which will be canceled by this action. Proceed?".F(u.Name));
					if (!proceed) continue;
				}
				
				// Notify user of impending fuel depletion
				// if they're gonna run out, don't bother warning of low fuel
				var distanceToNode = TheGame().GameBoard.CurrentRoute.Nodes.Count() * TheGame().GameBoard.DefaultAttributes.CellMeters;
				if (u.UnitInfo.UnitType.FuelConsumer && distanceToNode > u.CurrentFuelRange)
				{
					var proceed = TheGame().Renderer.ConfirmAction("{0} will run out of fuel before reaching the destination. Proceed?".F(u.Name));
					if (!proceed) continue;
				}
				else if (u.UnitInfo.UnitType.FuelConsumer && u.FuelLevelPercent < 3)
				{
					TheGame().Renderer.DisplayUserMessage(MessageDisplayType.WARNING, "{0} is critically low on fuel with {1}% remaining.".F(u.Name, u.FuelLevelPercent), null);
				}				

				var sourceNode = TheGame().JTSServices.NodeService.GetNodeAt(u.Location);
				var moveResults = u.MoveToLocation(targetNode, sourceNode);
				results.Add(moveResults);
			}

			TheGame().Renderer.RenderTileUnitInfoArea();

			if (results.Any(r => r.Status == ResultStatus.FAILURE))
			{
				var failedResults = results.Where(r => r.Status == ResultStatus.FAILURE).ToList();
				TheGame().Renderer.DisplayUserMessage(MessageDisplayType.ERROR, failedResults.MessageSummary(), null);
			}

			if (results.Any(r => r.Status == ResultStatus.SOME_FAILURE))
			{
				var infoResults = results.Where(r => r.Status == ResultStatus.SOME_FAILURE).ToList();
				TheGame().Renderer.DisplayUserMessage(MessageDisplayType.INFO, infoResults.MessageSummary(), null);
			}

			TheGame().GameBoard.ClearSelectedItems(false);
            targetNode.Select();
            TheGame().Renderer.RefreshActiveNodes();

		}

		public virtual void MoveUnitToSelectedNode()
		{
			throw new NotImplementedException();
		}

		public virtual void AttachUnit()
		{
			var unitToAttach = (TheGame().GameBoard.SelectedUnits.Any()) ? TheGame().GameBoard.SelectedUnits.Single() : null;

			if (unitToAttach == null)
			{
				SetInputError("You must first select a unit to attach.");
				return;
			}

			var attachToUnits = TheGame().GameBoard.SelectedNode.GetAllUnits()
			                            .Where(u => TheGame().JTSServices.RulesService.UnitCanAttachToUnit(unitToAttach, u).Result &&
										!u.Equals(unitToAttach)).ToArray();

			if (!attachToUnits.Any())
			{
				SetInputError("No suitable units to attach to at the selected location for {0}.".F(unitToAttach.Name));
				return;
			}

			var attachToUnit = SelectUnit(attachToUnits);

			if (attachToUnit == null) return;

			var result = unitToAttach.AttachToUnit(attachToUnit);
			HandleResultDisplay(result, false);
		}

		public virtual void AttachUnits()
		{
			throw new NotImplementedException();
		}

		public virtual void DetachUnit()
		{
			var unitToDetach = (TheGame().GameBoard.SelectedUnits.Any()) ? TheGame().GameBoard.SelectedUnits.Single() : null;

			if (unitToDetach == null)
			{
				SetInputError("You must first select a unit to detach.");
				return;
			}

			var result = unitToDetach.DetachFromUnit();
			HandleResultDisplay(result, false);
		}

		public virtual void DetachUnits()
		{
			throw new NotImplementedException();
		}

		public virtual void LoadUnit()
		{
			var unitToLoad = (TheGame().GameBoard.SelectedUnits.Any()) ? TheGame().GameBoard.SelectedUnits.Single() : null;

			if (unitToLoad == null)
			{
				SetInputError("You must first select a unit to load.");
				return;
			}

			var transportUnits = TheGame().GameBoard.SelectedNode.GetAllUnits()
									.Where(u => u.IsFriendly() 
											&& u.GetAllowableTransportWeight() > 0
                                            && !u.Equals(unitToLoad) 
                                            && !u.IsBeingTransported()).ToArray();

			if (!transportUnits.Any())
			{
				SetInputError("No suitable units to load to at the selected location for {0}.".F(unitToLoad.Name));
				return;
			}

			var transport = SelectUnit(transportUnits);

			if (transport == null) return;

			var fromNode = unitToLoad.GetNode();
			var toNode = transport.GetNode();
			
			var result = transport.LoadUnits(new List<IUnit>{unitToLoad}); 

			if (result.Status != ResultStatus.SUCCESS)
			{
				HandleResultDisplay(result, true);
			}
			else
			{
				fromNode.ResetUnitStackOrder();
				toNode.ResetUnitStackOrder();
				TheGame().GameBoard.ClearSelectedItems(false);
				toNode.Select();
				TheGame().Renderer.RefreshNodes(new[] {toNode, fromNode});
				TheGame().Renderer.RenderTileUnitInfoArea();
				HandleResultDisplay(result, false);
			}	
		}

		public virtual void DeployUnit()
		{
			var transportUnit = (TheGame().GameBoard.SelectedUnits.Any()) ? TheGame().GameBoard.SelectedUnits.Single() : null;
			var selectedNode = TheGame().GameBoard.SelectedNode ?? ((transportUnit == null) ? null : transportUnit.GetNode());

			if (transportUnit == null)
			{
				SetInputError("You must first select a transport unit.");
				return;
			}

			if (selectedNode == null)
			{
				SetInputError("You must first select a node to deploy to.");
				return;
			}

			var fromNode = transportUnit.GetNode();
			var toNode = selectedNode.GetNode();

			var deployUnits = transportUnit.GetTransportedUnits().ToArray();

			if (!deployUnits.Any())
			{
				SetInputError("{0} is not currently transporting any units.".F(transportUnit.Name));
				return;
			}

			var unitToDeploy = SelectUnit(deployUnits);

			if (unitToDeploy == null) return;

			var result = transportUnit.DeployUnits(new List<IUnit> {unitToDeploy}, toNode);

			if (result.Status != ResultStatus.SUCCESS)
			{
				HandleResultDisplay(result, true);
			}
			else
			{
				fromNode.ResetUnitStackOrder();
				toNode.ResetUnitStackOrder();
				TheGame().GameBoard.ClearSelectedItems(false);
				toNode.Select();
				TheGame().Renderer.RefreshNodes(new[] {toNode, fromNode});
				TheGame().Renderer.RenderTileUnitInfoArea();
				HandleResultDisplay(result, false);
			}			
		}

		public virtual void SetBattlePosture(BattlePosture posture)
		{
			var units = TheGame().GameBoard.SelectedUnits.Where(u => !u.CurrentMoveStats.HasPerformedAction).ToList();

			if (!units.Any())
			{
				SetInputError("No units currently selected.");
				return;
			}

			units.ForEach(u => u.SetUnitBattlePosture(posture));
			TheGame().JTSServices.UnitService.SaveUnits(units);
		}
		
		public virtual void BarrageLocation()
		{
			var units = (TheGame().GameBoard.SelectedUnits.Any()) ? TheGame().GameBoard.SelectedUnits : null;

			// Enter battle state ...
			TheGame().StateSystem.ChangeState(StateType.BATTLE);

			var sourceNode = units.First().GetNode();
			var targetNode = TheGame().GameBoard.SelectedNode;

			var r = TheGame().BattleHandler.BarrageUnitsAtLocation(TheGame().GameBoard.SelectedNode);

			// Handle issues with creating and doing battle
			if (r.Status == ResultStatus.FAILURE)
			{
				// Exit battle state ....
				TheGame().StateSystem.ChangeState(StateType.GAME_IN_PLAY);
				SetInputError(r.Message);
				return;
			}

			sourceNode.ResetUnitStackOrder();
			targetNode.ResetUnitStackOrder();
			TheGame().GameBoard.ClearSelectedItems(false);
			// Reset the selected node
			targetNode.Select();

			// Exit battle state ...
			TheGame().StateSystem.ChangeState(StateType.GAME_IN_PLAY);
		}

		public virtual void NukeLocation()
		{
			var units = (TheGame().GameBoard.SelectedUnits.Any()) ? TheGame().GameBoard.SelectedUnits : null;

			// Change if we get more than on possible nuke demographic
			var demo = TheGame().JTSServices.DemographicService.GetDemographics().Where(d => d.IsDemographicClass("NuclearWasteland")).SingleOrDefault();

			if (demo == null)
			{
				SetInputError("No appropriate demographic was found for nuclear effect.");
				return;
			}

			// Enter battle state ...
			TheGame().StateSystem.ChangeState(StateType.BATTLE);

			var sourceNode = units.First().GetNode();
			var targetNode = TheGame().GameBoard.SelectedNode;

			var battleResult = TheGame().BattleHandler.NukeLocation(targetNode);

			// Handle issues with creating and doing battle
			if (battleResult.Status == ResultStatus.FAILURE)
			{
				// Exit battle state ....
				TheGame().StateSystem.ChangeState(StateType.GAME_IN_PLAY);
				SetInputError(battleResult.Message);
				return;
			}

			TheGame().GameBoard.ClearSelectedItems(false);
			var nukeResult = TheGame().JTSServices.TileService.NukeAffectTile(targetNode.DefaultTile, demo);
			
			if (nukeResult.Status == ResultStatus.FAILURE)
			{
				// Exit battle state ....
				TheGame().StateSystem.ChangeState(StateType.GAME_IN_PLAY);
				SetInputError(nukeResult.Message);
				return;
			}

			// Reset the selected node
			sourceNode.Select();

			// Exit battle state ...
			TheGame().StateSystem.ChangeState(StateType.GAME_IN_PLAY);
		}

		public virtual void DoBattleAtLocation()
		{
			// Enter battle state ...
			TheGame().StateSystem.ChangeState(StateType.BATTLE);

			var targetNode = TheGame().GameBoard.SelectedNode;

			var r = TheGame().BattleHandler.DoBattleAtLocation(TheGame().GameBoard.SelectedNode);

			// Handle issues with creating and doing battle
			if (r.Status == ResultStatus.FAILURE)
			{
				// Exit battle state ...
				TheGame().StateSystem.ChangeState(StateType.GAME_IN_PLAY);
				SetInputError(r.Message);
				return;
			}

			targetNode.ResetUnitStackOrder();
			TheGame().GameBoard.ClearSelectedItems(false);
			// Reset the selected node
			targetNode.Select();

			// Exit battle state ...
			TheGame().StateSystem.ChangeState(StateType.GAME_IN_PLAY);
		}

		public virtual void BuildInfrastructure()
		{
			// Validate that only one unit is selected and that it can build stuff
			var unit = TheGame().GameBoard.SelectedUnits.SingleOrDefault();
			var node = TheGame().GameBoard.SelectedNode;
			var vResult = unit.ValidateTask("BuildInfrastructure");

			HandleResultDisplay(vResult, true);

			var isUnitNode = node.Equals(unit.GetNode());

			// TODO: Handle vResult

		// ----- Get Allowable actions

			// Get buildable infrastructure types for current node
			var infrastructureDems = TheGame().JTSServices.DemographicService.GetDemographics()
			                                          .Where(d => d.DemographicClass.BuildInfo.Buildable)
			                                          .Where(d => node.DefaultTile.CanSupportInfrastructureBuilding(d)).ToArray();

			if (!infrastructureDems.Any()) return;

			var demographic = SelectDemographic(infrastructureDems, "Build");

			// Menu canceled
			if (demographic == null) return;

			var allowableDirections = TheGame().JTSServices.TileService.GetOrientationAllowableForDemographicClassByTile(node.DefaultTile, 
			                                                                                                              demographic.DemographicClass);

			// If this is an adjacent tile, get only the orientation nearest the current node
			if (!isUnitNode)
			{
				var toDirection = 
							TheGame().JTSServices.NodeService.GetNodeDirectionFromNeighborSourceNode(node, unit.GetNode()).Reverse();
				var allowedDirection = (allowableDirections.Contains(toDirection)) ? toDirection : Direction.NONE;
				
				allowableDirections = new List<Direction> { allowedDirection };

				// node area (orientation) is unreachable for building selected infrastructure
				if (allowedDirection == Direction.NONE)
				{
					TheGame().Renderer.DisplayUserMessage(MessageDisplayType.INFO, 
														"{0} is too far away to build a {1} at the current location.".F(unit.Name, demographic.DemographicClass.Name), 
														null);
					return;
				}
			}


			var directionResult = (allowableDirections.Count() > 1) 
								?  SelectOrientation(allowableDirections)
								: new OperationResult<Direction, Direction> {Status = ResultStatus.SUCCESS, Result = allowableDirections.Single()};
			
			// Menu canceled
			if (directionResult.Status == ResultStatus.FAILURE)	
				return;

			var direction = directionResult.Result;
		

		// ----- Setup strategy
			
			//var args = new object[] {node.DefaultTile, demographic};	
			var args = new List<TaskExecutionArgument>
				{
					new TaskExecutionArgument(node.DefaultTile.GetType().FullName,
					                          node.DefaultTile.GetType().Assembly.FullName,
					                          "tile",
					                          new[] {node.DefaultTile.ID.ToString()}),
					new TaskExecutionArgument(demographic.GetType().FullName,
					                          demographic.GetType().Assembly.FullName,
					                          "demographic",
					                          new[] {demographic.ID.ToString()}),
					new TaskExecutionArgument("int",
					                          null,
					                          "direction",
					                          new[] {((int) direction).ToString()})
				};

			var missionType = TheGame().JTSServices.AIService.GetMissionTypes().SingleOrDefault(mot => mot.Name == "Build");
			var mission = new Mission(missionType, unit.ID);
			var unitTaskType = TheGame().JTSServices.AIService.GetUnitTaskTypes().SingleOrDefault(utt => utt.Name == "BuildInfrastructure");			
			var task = new UnitTask(unitTaskType, mission, args, demographic.DemographicClass.BuildInfo.BuildTurns);

			var result = TheGame().StrategyHandler.SetUpUnitMission(mission, task, StrategicalStance.ADMINISTRATIVE, TheGame().CurrentTurn.Player);
			HandleResultDisplay(result, true);
		}

		public virtual void DestroyInfrastructure()
		{	
			// Validate that only one unit is selected and that it can demolish stuff
			var unit = TheGame().GameBoard.SelectedUnits.SingleOrDefault();
			var node = TheGame().GameBoard.SelectedNode;
			var vResult = unit.ValidateTask("DestroyInfrastructure");
			HandleResultDisplay(vResult, true);

			IEnumerable<Direction> allowableDirections;
			IEnumerable<IDemographic> infrastructureDems;

			IDemographic actionDemographic;
			var isUnitNode = node.Equals(unit.GetNode());

			// Handle vResult

		// ----- Get allowable actions

			// If this is an adjacent tile, get only the demographics with the orientation nearest the current node
			// else just get all the destroyable demographics
			if (!isUnitNode)
			{
				var toDirection =
							TheGame().JTSServices.NodeService.GetNodeDirectionFromNeighborSourceNode(node, unit.GetNode()).Reverse();

				infrastructureDems = TheGame().GameBoard.SelectedNode.DefaultTile.Infrastructure
															.Where(d => d.DemographicClass.BuildInfo.Destroyable
																&& d.Orientation.Contains(toDirection)).ToArray();

				allowableDirections = new List<Direction> { toDirection };

				if (!infrastructureDems.Any())
				{
					TheGame().Renderer.DisplayUserMessage(MessageDisplayType.INFO, 
														"There is no destroyable infrastructure reachable by {0} at the currently selected location.".F(unit.Name), 
														null);
					return;
				}

				actionDemographic = SelectDemographic(infrastructureDems, "Destroy");

				// Menu canceled
				if (actionDemographic == null) 
					return;

			}
			else // Currently occupied node
			{
				infrastructureDems = TheGame().GameBoard.SelectedNode.DefaultTile.Infrastructure
																.Where(d => d.DemographicClass.BuildInfo.Destroyable).ToArray();
				if (!infrastructureDems.Any()) 
					return;

				// Can't leave the unit stranded
				// the unit is occupying this node based only on the infrastructure override
				if (!TheGame().JTSServices.RulesService.TileIsAllowableForUnitType(unit.UnitInfo.UnitType, node.DefaultTile).Result && 
					infrastructureDems.Sum(d => d.Orientation.Count) == 1)
				{
					TheGame().Renderer.DisplayUserMessage(MessageDisplayType.INFO, "Action would leave unit stranded.", null);
					return;
				}

				actionDemographic = SelectDemographic(infrastructureDems, "Destroy");

				// Menu canceled
				if (actionDemographic == null) 
					return;

				allowableDirections = actionDemographic.Orientation;
			}

			var directionResult = (allowableDirections.Count() > 1) 
								?  SelectOrientation(allowableDirections)
								: new OperationResult<Direction, Direction> {Status = ResultStatus.SUCCESS, Result = allowableDirections.Single()};
			
			// Menu canceled
			if (directionResult.Status == ResultStatus.FAILURE)	
				return;

			var direction = directionResult.Result;
			

		// ----- Set up strategy
			var args = new List<TaskExecutionArgument>
				{
					new TaskExecutionArgument(node.DefaultTile.GetType().FullName,
					                          node.DefaultTile.GetType().Assembly.FullName,
					                          "tile",
					                          new[] {node.DefaultTile.ID.ToString()}),
					new TaskExecutionArgument(actionDemographic.GetType().FullName,
					                          actionDemographic.GetType().Assembly.FullName,
					                          "demographic",
					                          new[] {actionDemographic.ID.ToString()}),
					new TaskExecutionArgument("int",
					                          null,
					                          "direction",
					                          new[] {((int) direction).ToString()})
				};

			var missionType = TheGame().JTSServices.AIService.GetMissionTypes().SingleOrDefault(mot => mot.Name == "Demolish");
			var mission = new Mission(missionType, unit.ID);
			var unitTaskType = TheGame().JTSServices.AIService.GetUnitTaskTypes().SingleOrDefault(utt => utt.Name == "DestroyInfrastructure");			
			var task = new UnitTask(unitTaskType, mission, args, actionDemographic.DemographicClass.BuildInfo.DestroyTurns);

			var result = TheGame().StrategyHandler.SetUpUnitMission(mission, task, StrategicalStance.ADMINISTRATIVE, TheGame().CurrentTurn.Player);	
			HandleResultDisplay(result, true);
		}		

	// Other

		public virtual void ScenarioEditor()
		{
			var curDrive = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());
			var scenarioExecutablePath = ConfigurationManager.AppSettings["gameeditor_executablepath"];
			Process.Start("{0}{1}{2}".F(curDrive, scenarioExecutablePath,"\\JTacticalSimEditor.exe"));
		}

	// Reinforcements

		public virtual void GetReinforcements()
		{
			// Enter reinforcement state ...
			TheGame().StateSystem.ChangeState(StateType.REINFORCE);
		}

		public virtual void AddReinforcementUnit()
		{
			throw new NotImplementedException();
		}

		public virtual void PlaceReinforcementUnit()
		{
			var reinforcements = TheGame().CurrentTurn.Player.UnplacedReinforcements;
			var unit = SelectUnit(reinforcements);

			if (unit == null)
				return;
			
			INode node = TheGame().JTSServices.NodeService.GetNodeAt(TheGame().GameBoard.SelectedNode.Location);
			
			using (var txn = new TransactionScope())
			{
				var placeResult = unit.PlaceAtLocation(node);

				// Handle issues and remove
				if (placeResult.Status == ResultStatus.SUCCESS)
				{				
					var removeResult = TheGame().CurrentTurn.Player.RemoveReinforcementUnit(unit);
					HandleResultDisplay(removeResult, true);
				}

				HandleResultDisplay(placeResult, false);
			}
			
		}

#endregion

		public virtual void SetInputError(string error)
		{
			TheGame().Renderer.DisplayUserMessage(MessageDisplayType.ERROR, error, null);
		}

		public virtual void RefreshScreen()
		{
			TheGame().StateSystem.Render();
		}

		protected virtual INode GetSelectedNode()
		{
			if (TheGame().GameBoard.SelectedNode == null)
			{
				SetInputError("You must select a location first.");
				return null;
			}

			return TheGame().GameBoard.SelectedNode;
		}

		protected virtual string GetCommandInputOptions<T>()
			where T : class, IBaseComponent
		{
			var sb = new StringBuilder();

			sb.Append("[ ");

			var table = TheGame().JTSServices.GenericComponentService.GetComponentTable<T>();
			table.Records.ToList().ForEach(t => sb.Append("{0} ".F((string)t.Name)));

			sb.Append("]");

			return sb.ToString();
		}

		protected virtual string GetCommandInputOptions<T>(IEnumerable<T> components)
			where T : IBaseComponent
		{
			var sb = new StringBuilder();

			sb.Append("[ ");
			components.ToList().ForEach(t => sb.Append("{0} ".F(t.Name)));
			sb.Append("]");

			return sb.ToString();
		}


#region CommandLine

		public virtual void RunCommand(ICommandInterface ci)
		{
			var commands = CommandInterface.GetInputCommandMethods(TheGame().StateSystem.CurrentStateType);

			// Allow for the name and alias
			var dictItem = commands.SingleOrDefault(kvp => (kvp.Key.CommandName.ToLowerInvariant() == _commandArgs.Command.ToLowerInvariant()) ||
															kvp.Key.Alias.ToLowerInvariant() == _commandArgs.Command.ToLowerInvariant());
						
				
			// We found the command
			if (dictItem.Key != null && dictItem.Value != null)
			{
				dictItem.Value.Invoke(ci, null);
				return;
			}

			// No command found
			TheGame().Renderer.DisplayUserMessage(MessageDisplayType.ERROR, "Command not recognized", null);
			
		}

		protected virtual void ParseCommandArgs(string command)
		{
			// Get all args. Skip the command text
			string[] tmp = command.Split(' ').ToArray();
			_commandArgs.Command = tmp.First();
			_commandArgs.Args = tmp.Skip(1).Where(arg => !arg.Contains("--")).ToArray();
			_commandArgs.Switches = tmp.Skip(1).Where(arg => arg.Contains("--")).ToList();
			_commandArgs.Cancel = (tmp.Any(i => i.ToLowerInvariant() == "cancel"));

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
			_commandArgs.Switches.ForEach(arg => arg.ToLowerInvariant());
// ReSharper restore ReturnValueOfPureMethodIsNotUsed
		}

		protected abstract string HandleCMDInput(string message);

#endregion

#region ActionMenus

		protected abstract Command GetNodeAction();
		protected abstract Command GetMainMenuAction();
		protected abstract Command GetHelpMenuAction();
		protected abstract IUnit SelectUnit(IEnumerable<IUnit> units);
		protected abstract IDemographic SelectDemographic(IEnumerable<IDemographic> demographics, string action);
		protected abstract IResult<Direction, Direction> SelectOrientation(IEnumerable<Direction> directions);
		protected abstract string GetSaveGameAsTitle();

#endregion

#region Input Validation

		protected virtual IResult<bool, string> IsValidScenarioTitleEntered(string input)
		{
			var result = TheGame().JTSServices.RulesService.ScenarioTitleIsValid(input);
			return result;
		}

		protected virtual IResult<bool, SavedGame> IsValidGameTitleEntered(string input)
		{
			var result = TheGame().JTSServices.RulesService.NameIsValid<SavedGame>(input);
			return result;
		}

		protected virtual IResult<bool, Unit> IsValidUnitNameEntered(string input)
		{
			var result = TheGame().JTSServices.RulesService.NameIsValid<Unit>(input);
			return result;
		}

		protected abstract bool IsValidRow(string input);
		protected abstract bool IsValidColumn(string input);
		protected abstract bool IsValidLocationEntered();
		protected abstract MouseButton GetValidMouseButtonClick(string input);

#endregion

#region event handlers

		protected abstract void On_MenuClickAction(object sender, EventArgs e);
		protected abstract void On_MapMenuErased(object sender, EventArgs e);
		protected abstract void On_MainMenuErased(object sender, EventArgs e);
		protected abstract void On_CmdBoxErased(object sender, EventArgs e);
		protected abstract void On_CmdBoxEscapePressed(object sender, EventArgs e);
		protected abstract void On_SaveAsGameTitleEscapePressed(object sender, EventArgs e);
		protected abstract void On_SaveAsGameTitleEntered(object sender, EventArgs e);

#endregion

	}
}
