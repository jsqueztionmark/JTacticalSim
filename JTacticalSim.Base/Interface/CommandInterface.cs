using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using JTacticalSim.API.Component;
using JTacticalSim.Utility;

namespace JTacticalSim.API.Game
{
	public sealed class CommandInterface : ICommandInterface
	{
		private readonly IInputCommandHandler _commandHandler;
		public event CommandCancelHandler CancelCommand;

		public CommandInterface(IInputCommandHandler commandHandler)
		{
			_commandHandler = commandHandler;
		}

#region Commands

	#region Game Admin

		[Command(Commands.EXIT, "Exit", "Exit", "q", CommandType.UTILITY, false, "", false)]
		[CommandAvailable(StateType.TITLE_MENU, StateType.GAME_IN_PLAY, StateType.REINFORCE, StateType.AI_IN_PLAY)]
		public void Exit()
		{
			_commandHandler.Exit();
		}

		[Command(Commands.PLAY, "Play", "Play", "p", CommandType.UTILITY, false, "", false)]
		[CommandAvailable(StateType.TITLE_MENU)]
		public void Play()
		{
			_commandHandler.Play();
		}

		[Command(Commands.NEW_GAME, "NewGame", "New Game", "newgame", CommandType.UTILITY, false, "[Scenario Title] [Game Title] (No spaces or special characters)", false)]
		[CommandAvailable(StateType.TITLE_MENU)]
		public void NewGame()
		{
			_commandHandler.NewGame();
		}

		[Command(Commands.LOAD_GAME, "LoadGame", "Load Game", "loadgame", CommandType.UTILITY, true, "[Game Title]", false)]
		[CommandAvailable(StateType.TITLE_MENU)]
		public void LoadGame()
		{
			_commandHandler.LoadGame();
		}

		[Command(Commands.SAVE_GAME, "SaveGame", "Save Game", "save", CommandType.UTILITY, false, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void SaveGameAs()
		{
			_commandHandler.SaveGame();
		}

		[Command(Commands.DELETE_GAME, "DeleteGame", "Delete Game", "deletegame", CommandType.UTILITY, true, "", false)]
		[CommandAvailable(StateType.TITLE_MENU)]
		public void DeleteGame()
		{
			_commandHandler.DeleteGame();
		}

		[Command(Commands.SAVE_GAME_AS, "SaveGameAs", "Save Game As", "savegameas", CommandType.UTILITY, false, "[Game Title] (No spaces or special characters)", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void SaveGame()
		{
			_commandHandler.SaveGameAs();
		}

		[Command(Commands.SCENARIO_EDITOR, "ScenarioEditor", "Start Scenario Editor", "", CommandType.UTILITY, false, "", false)]
		[CommandAvailable(StateType.TITLE_MENU)]
		public void ScenarioEditor()
		{
			_commandHandler.ScenarioEditor();
		}


	#endregion		

	#region Screens

		[Command(Commands.DISPLAY_REINFORCEMENTS_SCREEN, "ReinforcementsScreen", "Reinforcements Screen", "rs", CommandType.SCREEN, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY)]
		public void DisplayReinforcementsScreen()
		{
			_commandHandler.DisplayReinforcementsScreen();
		}

		[Command(Commands.DISPLAY_UNIT_QUICK_SELECT_SCREEN, "UnitQuickSelectScreen", "Unit Quick Select Screen", "uqs", CommandType.SCREEN, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY)]
		public void DisplayUnitQuickSelectScreen()
		{
			_commandHandler.DisplayUnitQuickSelectScreen();
		}

		[Command(Commands.DISPLAY_SCENARIO_INFO_SCREEN, "ScenarioInfoScreen", "Scenario Info Screen", "sis", CommandType.SCREEN, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY)]
		public void DisplayScenarioInfoScreen()
		{
			_commandHandler.DisplayScenarioInfoScreen();
		}

		[Command(Commands.DISPLAY_TITLE_SCREEN, "TitleScreen", "Title Screen", "ts", CommandType.SCREEN, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY)]
		public void DisplayTitleScreen()
		{
			_commandHandler.DisplayTitleScreen();
		}

		[Command(Commands.DISPLAY_HELP_SCREEN, "HelpScreen", "Help Screen", "hs", CommandType.SCREEN, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY)]
		public void DisplayHelpScreen()
		{
			_commandHandler.DisplayHelpScreen();
		}

		[Command(Commands.DISPLAY_MAIN_SCREEN, "MainBoardScreen", "Main Board Screen", "mbs", CommandType.SCREEN, true, "", false)]
		[CommandAvailable(StateType.REINFORCE, StateType.HELP, StateType.QUICK_SELECT)]
		public void DisplayMainBoardScreen()
		{
			_commandHandler.DisplayMainBoardScreen();
		}

	#endregion

	#region Game Board

		[Command(Commands.DISPLAY_COMMAND_LIST, "Commands", "Commands", "cmd", CommandType.UTILITY, false, "", false)]
		[CommandAvailable(StateType.TITLE_MENU, StateType.GAME_IN_PLAY, StateType.REINFORCE, StateType.AI_IN_PLAY)]
		public void DisplayCommandList()
		{
			_commandHandler.DisplayCommandList();
		}

		[Command(Commands.REFRESH_BOARD, "RefreshBoard", "Refresh Board", "rb", CommandType.UTILITY, false, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void RefreshBoard()
		{
			_commandHandler.DisplayMainBoardScreen();
		}

		[Command(Commands.END_TURN, "EndTurn", "End Player Turn", "et", CommandType.UTILITY, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void EndTurn()
		{
			_commandHandler.EndTurn();
		}

		[Command(Commands.ZOOM_MAP_IN, "ZoomIn", "Zoom map level in", "zIn", CommandType.UTILITY, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void ZoomMapLevelIn()
		{
			_commandHandler.ZoomMap(CycleDirection.IN);
		}

		[Command(Commands.ZOOM_MAP_OUT, "ZoomOut", "Zoom map level out", "zOut", CommandType.UTILITY, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void ZoomMapLevelOut()
		{
			_commandHandler.ZoomMap(CycleDirection.OUT);
		}

		[Command(Commands.CYCLE_MAP_MODE_UP, "MapModeUp", "Cycle map mode up", "mmUp", CommandType.UTILITY, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void CycleMapModeUp()
		{
			_commandHandler.CycleMapMode(CycleDirection.UP);
		}

		[Command(Commands.CYCLE_MAP_MODE_DOWN, "MapModeDown", "Cycle map mode down", "mmDown", CommandType.UTILITY, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void CycleMapModeDown()
		{
			_commandHandler.CycleMapMode(CycleDirection.DOWN);
		}

		[Command(Commands.SET_CURRENT_NODE, "ClickNode", "Click Node", "cn", CommandType.SELECT, true, "(at) [Column] [Row] [button]", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void SetCurrentNode()
		{
			_commandHandler.SetCurrentNode();
		}

		[Command(Commands.DISPLAY_CURRENT_NODE, "DisplayNode", "Display Location Info", "dn", CommandType.DISPLAY, false, "(Selected Location)", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void DisplayCurrentNode()
		{
			_commandHandler.DisplayCurrentNode();
		}

		[Command(Commands.DISPLAY_PLAYER, "DisplayPlayer", "Display Player Info", "dp", CommandType.DISPLAY, false, "(Current Player)", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.REINFORCE, StateType.AI_IN_PLAY)]
		public void DisplayPlayer()
		{
			_commandHandler.DisplayPlayer();
		}

	#endregion

	#region Unit Action

		[Command(Commands.SET_BATTLE_POSTURE_DEFENCE, "SetDefensePosture", "Set Defense Battle Posture", "defP", CommandType.UNIT, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY, StateType.QUICK_SELECT)]
		public void SetDefensiveBattlePosture()
		{
			_commandHandler.SetBattlePosture(BattlePosture.DEFENSIVE);
		}

		[Command(Commands.SET_BATTLE_POSTURE_OFFENSE, "SetAttackPosture", "Set Offense Battle Posture", "attP", CommandType.UNIT, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY, StateType.QUICK_SELECT)]
		public void SetOffensiveBattlePosture()
		{
			_commandHandler.SetBattlePosture(BattlePosture.OFFENSIVE);
		}

		[Command(Commands.SET_BATTLE_POSTURE_EVASION, "SetEvasivePosture", "Set Evasive Battle Posture", "evP", CommandType.UNIT, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY, StateType.QUICK_SELECT)]
		public void SetEvasiveBattlePosture()
		{
			_commandHandler.SetBattlePosture(BattlePosture.EVASION);
		}

		[Command(Commands.SET_BATTLE_POSTURE_STANDARD, "SetStandardPosture", "Set Standard Battle Posture", "stndP", CommandType.UNIT, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY, StateType.QUICK_SELECT)]
		public void SetStandardBattlePosture()
		{
			_commandHandler.SetBattlePosture(BattlePosture.STANDARD);
		}		

		[Command(Commands.MOVE_UNIT_TO_SELECTED_NODE, "MoveUnit", "Move Unit To Location", "m", CommandType.MOVE, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void MoveUnitToSelectedNode()
		{
			_commandHandler.MoveUnitToSelectedNode();
		}

		[Command(Commands.MOVE_UNITS_TO_SELECTED_NODE, "MoveUnits", "Move Units To Location", "mAll", CommandType.MOVE, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void MoveUnitsToSelectedNode()
		{
			_commandHandler.MoveUnitsToSelectedNode();
		}

		[Command(Commands.BARRAGE, "Barrage", "Remote Attack", "", CommandType.BATTLE, false, "(selected location)", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void BarrageLocation()
		{
			_commandHandler.BarrageLocation();	
		}

		[Command(Commands.NUKE, "Nuke", "Nuclear Strike", "", CommandType.BATTLE, false, "(selected location)", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void NukeLocation()
		{
			_commandHandler.NukeLocation();	
		}

		[Command(Commands.DO_BATTLE, "DoBattle", "Attack", "", CommandType.BATTLE, false, "(at selected location)", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void DoBattleAtLocation()
		{
			_commandHandler.DoBattleAtLocation();	
		}

		[Command(Commands.BUILD_INFRASTRUCTURE, "BuildInfrastructure", "Build Infrastructure", "bi", CommandType.UNIT, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void BuildInfrastructure()
		{
			_commandHandler.BuildInfrastructure();
		}

		[Command(Commands.DESTROY_INFRASTRUCTURE, "DestroyInfrastructure", "Destroy Infrastructure", "di", CommandType.UNIT, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void DestroyInfrastructure()
		{
			_commandHandler.DestroyInfrastructure();
		}

	#endregion

	#region Unit

		[Command(Commands.DISPLAY_UNIT, "DisplayUnit", "Display Unit Info", "du", CommandType.DISPLAY, false, "[Unit Name]", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY, StateType.QUICK_SELECT)]
		public void DisplayUnit()
		{
			_commandHandler.DisplayUnit();
		}

		[Command(Commands.DISPLAY_ASSIGNED_UNITS, "DisplayAssignedUnits", "Display Assigned Units", "", CommandType.DISPLAY, false, "[Unit Name]", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void DisplayAssignedUnits()
		{
			_commandHandler.DisplayAssignedUnits();
		}

		[Command(Commands.DISPLAY_UNITS, "ListUnits", "List Units", "lu", CommandType.DISPLAY, false, "[--CurrentNode]", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void DisplayUnits()
		{
			_commandHandler.DisplayUnits();
		}

		[Command(Commands.SET_SELECTED_UNIT, "ClickUnit", "Click Unit", "cu", CommandType.SELECT, true, "[Unit Name] [button]", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY, StateType.QUICK_SELECT)]
		public void SetSelectedUnit()
		{
			_commandHandler.SetSelectedUnit();
		}

		[Command(Commands.SET_SELECTED_UNITS, "ClickUnitsWithAttached", "Click Units", "cAll", CommandType.SELECT, true, "(at) [Column] [Row] [button]", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void SetSelectedUnits()
		{
		    _commandHandler.SetSelectedUnits();
		}

		[Command(Commands.UNSELECT_ALL_UNITS, "UnselectAllUnits", "Unselect all units", "unselectAll", CommandType.SELECT, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void UnselectAllUnits()
		{
		    _commandHandler.UnselectAllUnits();
		}

		[Command(Commands.SET_SELECTED_UNITS_W_ATTACHED, "ClickUnits", "Click Units with attached", "cAttached", CommandType.SELECT, true, "(at) [Column] [Row] [button]", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void SetSelectedUnitsWithAttached()
		{
		    _commandHandler.SetSelectedUnitWithAttached();
		}

		[Command(Commands.CYCLE_UNITS, "ScrollUnits", "Scroll Units", "su", CommandType.UNIT, true, "(at selected location)", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void CycleUnits()
		{
			_commandHandler.CycleUnits();
		}

		[Command(Commands.ATTACH_UNIT, "AttachUnit", "Attach Unit", "", CommandType.UNIT, true, "(to) [Unit Name]", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void AttachUnit()
		{
			_commandHandler.AttachUnit();
		}

		[Command(Commands.ATTACH_UNITS, "AttachUnits", "Attach Units", "", CommandType.UNIT, true, "(to) [Unit Name]", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void AttachUnits()
		{
			_commandHandler.AttachUnits();
		}

		[Command(Commands.DETACH_UNIT, "DetachUnit", "Detach Unit", "", CommandType.UNIT, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void DetachUnit()
		{
			_commandHandler.DetachUnit();
		}

		[Command(Commands.DETACH_UNITS, "DetachUnits", "Detach Units", "", CommandType.UNIT, true, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void DetachUnits()
		{
			_commandHandler.DetachUnits();
		}

		[Command(Commands.LOAD_UNIT, "LoadUnit", "Load Unit To Transport", "", CommandType.UNIT, true, "(on) [Unit Name]", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void LoadUnit()
		{
			_commandHandler.LoadUnit();
		}

		[Command(Commands.DEPLOY_UNIT, "DeployUnit", "Deploy Unit From Transport", "", CommandType.UNIT, true, "[Unit Name]", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void DeployUnit()
		{
			_commandHandler.DeployUnit();
		}
		
		[Command(Commands.ADD_UNIT, "AddUnit", "Add Unit", "", CommandType.UNIT, true, "(at) [Column] [Row]", true)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void AddUnit()
		{
			_commandHandler.AddUnit();
		}

		[Command(Commands.REMOVE_UNIT, "RemoveUnit", "Remove Unit From Game", "", CommandType.UNIT, true, "[Unit Name]", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY, StateType.QUICK_SELECT)]
		public void RemoveUnit()
		{
			_commandHandler.RemoveUnit();
		}

		[Command(Commands.EDIT_UNIT, "EditUnit", "Edit Unit Info", "", CommandType.UNIT, true, "(at) [Column] [Row]", true)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void EditUnit()
		{
			_commandHandler.EditUnit();
		}

		[Command(Commands.GET_REINFORCEMENTS, "GetReinforcements", "Get Reinforcements", "r", CommandType.UNIT, false, "", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void ReinforceUnits()
		{
			_commandHandler.GetReinforcements();	
		}

		[Command(Commands.ADD_REINFORCEMENT_UNIT, "AddReinforcement", "Add Reinforcement", "ar", CommandType.UNIT, true, "", false)]
		[CommandAvailable(StateType.REINFORCE)]
		public void AddReinforcementUnit()
		{
			_commandHandler.AddReinforcementUnit();
		}

		[Command(Commands.PLACE_REINFORCEMENT_UNIT, "PlaceUnit", "Place Reinforcement", "pu", CommandType.UNIT, true, "[Unit Name] (at selected location)", false)]
		[CommandAvailable(StateType.GAME_IN_PLAY, StateType.AI_IN_PLAY)]
		public void PlaceUnit()
		{
			_commandHandler.PlaceReinforcementUnit();
		}


	#endregion		

#endregion

		public void HandleInput(StateType state)
		{
			_commandHandler.HandleInput(this, state);
		}

		public void GetCommand()
		{
			_commandHandler.GetCommandInput(this);
		}

		public void SetInputError<TResult, TObject>(IResult<TResult, TObject> result)
		{
			_commandHandler.SetInputError(result.Message);
		}

		public void RunCommand(Commands command)
		{
			var cmd = GetInputCommandMethods().SingleOrDefault(kvp => kvp.Key.CommandIdentifier == command);

			if (cmd.Key == null)
				return;

			cmd.Value.Invoke(this, null);
		}

		public static Dictionary<Command, MethodInfo> GetInputCommandMethods()
		{
			var commandDict = new Dictionary<Command, MethodInfo>();
			MethodInfo[] methods = typeof(CommandInterface).GetMethods();

			Action<MethodInfo> methodAction = method =>
			{
				lock (methods)
				{
					var command = Attribute.GetCustomAttribute(method, typeof (Command), false) as Command;

					if (command == null) return;

					lock (method)
					{
						if (!commandDict.ContainsKey(command))
							lock (commandDict)
							{
								if (!commandDict.ContainsKey(command)) commandDict.Add(command, method);
							}
					}
				}
			};

			if (JTacticalSim.Game.Instance.IsMultiThreaded)
			{
				Parallel.ForEach(methods, methodAction);
			}
			else
			{
				foreach (var method in methods)
				{
					methodAction(method);
				}				
			}
			
			return commandDict;
		}

		/// <summary>
		/// Returns a collection of command methods for the current game state
		/// </summary>
		/// <param name="currentStateType"></param>
		/// <returns></returns>
		public static Dictionary<Command, MethodInfo> GetInputCommandMethods(StateType currentStateType)
		{
			var commandDict = new Dictionary<Command, MethodInfo>();
			MethodInfo[] methods = typeof(CommandInterface).GetMethods();

			Action<MethodInfo> methodAction = method =>
			{
				lock (methods)
				{
					var command = Attribute.GetCustomAttribute(method, typeof (Command), false) as Command;
					var commandsAvailable = Attribute.GetCustomAttribute(method, typeof(CommandAvailable), false) as CommandAvailable;

					// For current game state only
					if (commandsAvailable != null && commandsAvailable.AvailableInStates.All(ais => ais != currentStateType))
						return;

					if (command == null) return;

					lock (method)
					{
						if (!commandDict.ContainsKey(command))
							lock (commandDict)
							{
								if (!commandDict.ContainsKey(command)) commandDict.Add(command, method);
							}
					}
				}
			};

			if (JTacticalSim.Game.Instance.IsMultiThreaded)
			{
				Parallel.ForEach(methods, methodAction);
			}
			else
			{
				foreach (var method in methods)
				{
					methodAction(method);
				}				
			}
			
			return commandDict;
		}

		/// <summary>
		/// Returns a collection of commands for the current game state
		/// </summary>
		/// <param name="currentStateType"></param>
		/// <returns></returns>
		public static Dictionary<CommandType, List<string>> GetInputCommands(StateType currentStateType)
		{
			var commandDict = new Dictionary<CommandType, List<string>>();

			MethodInfo[] methods = typeof(CommandInterface).GetMethods();

			foreach (MethodInfo method in methods)
			{
			    var command = Attribute.GetCustomAttribute(method, typeof(Command), false) as Command;
				var commandsAvailable = Attribute.GetCustomAttribute(method, typeof(CommandAvailable), false) as CommandAvailable;

				// For current game state only
				if (commandsAvailable != null && commandsAvailable.AvailableInStates.All(ais => ais != currentStateType))
					continue;

			    if (command == null || command.HideMe) continue;

			    if (commandDict.ContainsKey(command.Type))
			    {
			        commandDict[command.Type].Add("{0} | {1}   {2}".F(command.CommandName, command.Alias, command.Args));
			    }
			    else
			    {
			        commandDict.Add(command.Type, new List<string>{"{0} | {1}   {2}".F(command.CommandName, command.Alias, command.Args)});
			    }
			}

			// Sort
			commandDict.Values.ToList().ForEach(l => l.Sort());
			return commandDict;

		}

		/// <summary>
		/// Returns a collection of commands appropriate for the current node at any given game state
		/// TODO: Maybe find a way to move the actual rules on these to the rules service??
		/// </summary>
		/// <param name="node"></param>
		/// <param name="theGame"></param>
		/// <returns></returns>
		public static IEnumerable<Command> GetAvailableCommandsForNode(INode node, IGame theGame)
		{
			var retVal = new List<Command>();

			var appropriatePossibleActions = new[]
				{
					Commands.DISPLAY_UNIT, 
					Commands.DISPLAY_CURRENT_NODE, 
					Commands.DEPLOY_UNIT, 
					Commands.LOAD_UNIT, 
					Commands.MOVE_UNITS_TO_SELECTED_NODE, 
					Commands.DO_BATTLE, 
					Commands.BARRAGE,
 					Commands.NUKE,
					Commands.ATTACH_UNIT, 
					Commands.DETACH_UNIT,
					Commands.PLACE_REINFORCEMENT_UNIT,
					Commands.BUILD_INFRASTRUCTURE, 
					Commands.DESTROY_INFRASTRUCTURE, 
					Commands.SET_BATTLE_POSTURE_DEFENCE,
					Commands.SET_BATTLE_POSTURE_OFFENSE, 
					Commands.SET_BATTLE_POSTURE_STANDARD,
					Commands.SET_BATTLE_POSTURE_EVASION,
					Commands.EDIT_UNIT,
				};
			
			var allCommandsForCurrentState = GetInputCommandMethods(theGame.StateSystem.CurrentStateType)
													.Where(kvp => appropriatePossibleActions.Contains(kvp.Key.CommandIdentifier))
													.Select(kvp => kvp.Key).ToList();

			
			if (node != null)
			{
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.DISPLAY_CURRENT_NODE));
			}
			// Criteria:
			// There must be only one friendly unit selected and the currently selected node must be the samea s the unit's location
			if (theGame.GameBoard.SelectedUnits.Count == 1 &&
				theGame.GameBoard.SelectedUnits.Single().GetNode().Equals(node))
			{
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.DISPLAY_UNIT));
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.EDIT_UNIT));
			}
			// Criteria:
			// There must be only one friendly unit selected and it must currently be transporting units
			if (theGame.GameBoard.SelectedUnits.Count == 1 &&
				theGame.GameBoard.SelectedUnits.Single().GetTransportedUnits().Any())
			{
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.DEPLOY_UNIT));
			}
			// Criteria:
			// There must be only one friendly unit selected and there must be at least one friendly unit
			// at the target node to attempt to load onto that is not the selected unit and the target node must be in the available move area
			if (theGame.GameBoard.SelectedUnits.Count == 1 &&
				theGame.GameBoard.AvailableMovementNodes.Any(n => n.Equals(node)) &&
			    node.DefaultTile.GetAllUnits().Any(u =>	u.IsFriendly() && 
															u.GetAllowableTransportWeight() > 0 &&
															!u.Equals(theGame.GameBoard.SelectedUnits.Single())))
			{
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.LOAD_UNIT));
			}
			// Criteria:
			// There must be friendly units selected and the target node must not be the currently occupied node.
			// The target node must be able to accomodate all moving units
			// but must be in the available movement area
			if (theGame.GameBoard.SelectedUnits.Any() && 
				!node.DefaultTile.WillExceedMaxUnits(theGame.GameBoard.SelectedUnits) &&
				theGame.GameBoard.AvailableMovementNodes.Any(n => n.Equals(node)) &&
				theGame.GameBoard.SelectedUnits.Any(u => !u.GetNode().Equals(node)))
			{
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.MOVE_UNITS_TO_SELECTED_NODE));
			}
			// Criteria:
			// There must be friendly units selected and the target node must be the currently occupied node.
			// Battle Posture can only be set if the unit has not performed an action this turn. 
			if (theGame.GameBoard.SelectedUnits.Any() && 
				theGame.GameBoard.SelectedUnits.Any(u => u.GetNode().Equals(node)) &&
				theGame.GameBoard.SelectedUnits.Any(u => !u.CurrentMoveStats.HasPerformedAction))
			{
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.SET_BATTLE_POSTURE_STANDARD));
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.SET_BATTLE_POSTURE_OFFENSE));
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.SET_BATTLE_POSTURE_DEFENCE));
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.SET_BATTLE_POSTURE_EVASION));
			}
			// Criteria:
			// The target node must have friendly and unfriendly units and the friendly units must be able to do battle this turn
			if (theGame.GameBoard.SelectedNode.GetAllUnits().Any(u1 => u1.IsFriendly() && u1.CanDoBattleThisTurn() &&
			                                                          node.GetAllUnits()
			                                                                   .Any(u2 => !u2.IsFriendly())))
			{
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.DO_BATTLE));
			}
			// Criteria:
			// Must be selected friendly units that can do battle this turn and have remote fire capability
			// Must have remote fire points available
			// and the selected node must have enemy units
			if (theGame.GameBoard.SelectedUnits.Any(u =>    u.IsRemoteBattleCapable() && 
															u.CurrentMoveStats.RemoteFirePoints > 0 &&
                                                            u.CanDoBattleThisTurn() && 
                                                            !u.GetNode().Equals(node) &&
                                                            u.RemoteAttackDistance >= theGame.JTSServices.AIService.CalculateNodeCountToNode(u.GetNode(), 
                                                                                                                                            node, 
                                                                                                                                            u.RemoteAttackDistance).Result))
			{
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.BARRAGE));
			}
			// Criteria:
			// Must be a single selected friendly unit that has a nuclear capability and remote fire capability
			// Current player must have at least 1 remaining nuclear charge
			// Must have remote fire points available
			if (theGame.GameBoard.SelectedUnits.Count == 1 &&
				theGame.CurrentTurn.Player.TrackedValues.NuclearCharges > 0 &&
				theGame.GameBoard.SelectedUnits.Any(u =>    u.IsRemoteBattleCapable() && 
															u.IsNuclearCapable() &&
															u.CurrentMoveStats.RemoteFirePoints > 0 &&
                                                            u.CanDoBattleThisTurn() && 
                                                            !u.GetNode().Equals(node) &&
                                                            u.RemoteAttackDistance >= theGame.JTSServices.AIService.CalculateNodeCountToNode(u.GetNode(), 
                                                                                                                                            node, 
                                                                                                                                            u.RemoteAttackDistance).Result))
			{
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.NUKE));
			}
			// Criteria:
			// Must have only one unit selected and the target node must have at least one friendly unit that
			// the selected unit can attach to
			if (theGame.GameBoard.SelectedUnits.Count == 1 &&
			   node.GetAllUnits()
			            .Any(
				            u =>
				            theGame.JTSServices.RulesService.UnitCanAttachToUnit(theGame.GameBoard.SelectedUnits.Single(), u)
				                    .Result && u.IsFriendly()))
			{
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.ATTACH_UNIT));
			}
			// Criteria:
			// Must have only one unit selected and that unit must currently be attached in an organization
			if (theGame.GameBoard.SelectedUnits.Count == 1 &&
				theGame.GameBoard.SelectedUnits.Any(u => u.GetNode().Equals(node)) &&
			    theGame.GameBoard.SelectedUnits.Single().AttachedToUnit != null)
			{
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.DETACH_UNIT));
			}
			// Criteria:
			// Must not have any units selected and must have available reinforcements.
			// node must not be at the maximum unit count
			// Can't restrict selected to appropriate nodes because we don't yet know which unit
			if (!theGame.GameBoard.SelectedUnits.Any() && 
				!node.DefaultTile.HasMaxUnits(theGame.CurrentPlayerFaction) &&
				theGame.CurrentTurn.Player.UnplacedReinforcements.Any() && 
				node.IsFriendly())
			{
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.PLACE_REINFORCEMENT_UNIT));
			}

			// Criteria:
			// Must have only a single unit selected.
			// Selected unit must be able to perform action
			// Selected unit must be allowed to perform task
			// node must have existing infrastructure that is destroyable
			// Selected unit must be at at or within one space of the selected node
			if (BuildCriteriaMet(node, theGame) &&
				theGame.GameBoard.SelectedUnits.Single().ValidateTask("DestroyInfrastructure").Result &&
				node.DefaultTile.Infrastructure.Any(i => i.DemographicClass.BuildInfo.Destroyable))				
			{
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.DESTROY_INFRASTRUCTURE));
			}

			// Criteria:
			// Must have only a single unit selected.
			// Selected unit must be able to perform action
			// Selected unit must be allowed to perform task
			// Selected unit must be at at or within one space of the selected node
			if (BuildCriteriaMet(node, theGame) &&
				theGame.GameBoard.SelectedUnits.Single().ValidateTask("BuildInfrastructure").Result)
			{
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.BUILD_INFRASTRUCTURE));
			}
				
// ReSharper disable StringCompareToIsCultureSpecific
			retVal.Sort((cmd1, cmd2) => cmd1.DisplayName.CompareTo(cmd2.DisplayName));
// ReSharper restore StringCompareToIsCultureSpecific
			
			return retVal;
		}

		public static IEnumerable<Command> GetAvailableCommandsForUnitQuickSelect(IUnit unit, IGame theGame)
		{
			var retVal = new List<Command>();

			var appropriatePossibleActions = new[]
				{
					Commands.DISPLAY_UNIT, 
					Commands.SET_SELECTED_UNIT,  
					Commands.SET_BATTLE_POSTURE_DEFENCE,
					Commands.SET_BATTLE_POSTURE_OFFENSE, 
					Commands.SET_BATTLE_POSTURE_STANDARD,
					Commands.SET_BATTLE_POSTURE_EVASION
				};
			
			var allCommandsForCurrentState = GetInputCommandMethods(theGame.StateSystem.CurrentStateType)
													.Where(kvp => appropriatePossibleActions.Contains(kvp.Key.CommandIdentifier))
													.Select(kvp => kvp.Key).ToList();

			
			
			retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.DISPLAY_UNIT));
			retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.SET_SELECTED_UNIT));

			if (!unit.CurrentMoveStats.HasPerformedAction)
			{
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.SET_BATTLE_POSTURE_STANDARD));
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.SET_BATTLE_POSTURE_OFFENSE));
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.SET_BATTLE_POSTURE_DEFENCE));
				retVal.Add(allCommandsForCurrentState.SingleOrDefault(cmd => cmd.CommandIdentifier == Commands.SET_BATTLE_POSTURE_EVASION));
			}


// ReSharper disable StringCompareToIsCultureSpecific
			retVal.Sort((cmd1, cmd2) => cmd1.DisplayName.CompareTo(cmd2.DisplayName));
// ReSharper restore StringCompareToIsCultureSpecific
			
			return retVal;
		}

		public static IEnumerable<Command> GetAvailableCommandsForMainMenu(IGame theGame)
		{
			var appropriatePossibleActions = new[]
				{
					Commands.EXIT, 
					Commands.SAVE_GAME,  
					Commands.DISPLAY_TITLE_SCREEN,
					Commands.DISPLAY_SCENARIO_INFO_SCREEN
				};
			
			var retVal = GetInputCommandMethods(theGame.StateSystem.CurrentStateType)
											.Where(kvp => appropriatePossibleActions.Contains(kvp.Key.CommandIdentifier))
											.Select(kvp => kvp.Key).ToList();

			retVal.Sort((o1, o2) => o1.CommandName.CompareTo(o2.CommandName));
			return retVal;
		}

		public static IEnumerable<Command> GetAvailableCommandsForHelpMenu(IGame theGame)
		{
			var appropriatePossibleActions = new[]
				{
					Commands.DISPLAY_HELP_SCREEN,
					Commands.REFRESH_BOARD
				};
			
			var retVal = GetInputCommandMethods(theGame.StateSystem.CurrentStateType)
											.Where(kvp => appropriatePossibleActions.Contains(kvp.Key.CommandIdentifier))
											.Select(kvp => kvp.Key).ToList();

			retVal.Sort((o1, o2) => o1.CommandName.CompareTo(o2.CommandName));
			return retVal;
		}


		private static bool BuildCriteriaMet(INode node, IGame game)
		{
			return
			!node.DefaultTile.IsDestroyed &&
			game.GameBoard.SelectedUnits.Count == 1 && 
			!game.GameBoard.SelectedUnits.Single().CurrentMoveStats.HasPerformedAction &&
			!game.GameBoard.SelectedUnits.Single().HasCurrentMission() &&
			(game.GameBoard.SelectedUnits.Single().GetNode().Equals(node)
				|| 	game.JTSServices.NodeService.GetAllNodesAtDistance(game.GameBoard.SelectedUnits.Single().GetNode(), 1, false).Contains(node));
		}
	}
}
