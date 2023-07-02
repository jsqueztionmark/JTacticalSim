using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Transactions;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.Component;
using JTacticalSim.Component.GameBoard;
using JTacticalSim.Utility;
using ConsoleControls;

// ReSharper disable CheckNamespace
namespace JTacticalSim.ConsoleApp
// ReSharper restore CheckNamespace
{
	internal class ConsoleCommandHandler : InputCommandHandlerBase
	{
		// Controls
		public TextBox CmdBox { get; set; }
		private IZoomHandler _zoomHandler { get { return TheGame().ZoomHandler; } }

#region Base Overrides

	#region Input Handling

		protected override void Handle_GAME_IN_PLAY_Input(ICommandInterface ci)
		{
			Thread.Sleep(50);

			if (Console.KeyAvailable)
			{
				ConsoleKeyInfo keyInfo = Console.ReadKey(true);				
				
				// shift+control modifier
				if ((keyInfo.Modifiers & ConsoleModifiers.Shift) != 0 && 
					(keyInfo.Modifiers & ConsoleModifiers.Control) != 0)
				{
					switch (keyInfo.Key)
					{
						case ConsoleKey.Spacebar:
							{
								//TheGame().PlaySound(SoundType.CLICK1);
								ci.RunCommand(Commands.SET_SELECTED_UNITS);
								return;
							}
					}
				}

				// shift modifier
				if ((keyInfo.Modifiers & ConsoleModifiers.Shift) != 0)
				{
					// Pass off to the movement handling
					if (IsMoveKey(keyInfo.Key))
					{
						ScrollMainBoardFullArea(keyInfo.Key);
						return;
					}
					switch (keyInfo.Key)
					{
						case ConsoleKey.Spacebar:
							{
								//TheGame().PlaySound(SoundType.CLICK1);
								ci.RunCommand(Commands.UNSELECT_ALL_UNITS);
								return;
							}
						case ConsoleKey.Add:
							{
								ci.RunCommand(Commands.CYCLE_MAP_MODE_UP);
								return;
							}
						case ConsoleKey.Subtract:
							{
								ci.RunCommand(Commands.CYCLE_MAP_MODE_DOWN);
								return;
							}
					}
				}


				// control modifier
				if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0)
				{
					switch (keyInfo.Key)
					{
						case ConsoleKey.R:
							{
								ci.RunCommand(Commands.DISPLAY_REINFORCEMENTS_SCREEN);
								return;
							}
						case ConsoleKey.U:
							{
								ci.RunCommand(Commands.DISPLAY_UNIT_QUICK_SELECT_SCREEN);
								return;
							}
						case ConsoleKey.Q:
							{
								//TheGame().PlaySound(SoundType.CLICK1);
								ci.RunCommand(Commands.EXIT);
								return;
							}
						case ConsoleKey.End:
							{
								//TheGame().PlaySound(SoundType.CLICK1);
								ci.RunCommand(Commands.END_TURN);
								return;
							}
						case ConsoleKey.Spacebar:
							{
								//TheGame().PlaySound(SoundType.CLICK1);
								ci.RunCommand(Commands.SET_SELECTED_UNITS_W_ATTACHED);
								return;
							}
						case ConsoleKey.UpArrow:
							{
								ci.RunCommand(Commands.ZOOM_MAP_OUT);
								return;
							}
						case ConsoleKey.DownArrow:
							{
								ci.RunCommand(Commands.ZOOM_MAP_IN);
								return;
							}
						case ConsoleKey.LeftArrow:
							{
								ci.RunCommand(Commands.CYCLE_MAP_MODE_DOWN);
								return;
							}
						case ConsoleKey.RightArrow:
							{
								ci.RunCommand(Commands.CYCLE_MAP_MODE_UP);
								return;
							}
					}
				}

				// No key modifiers
				if (((keyInfo.Modifiers & ConsoleModifiers.Control) == 0) &&
					((keyInfo.Modifiers & ConsoleModifiers.Alt) == 0) &&
					((keyInfo.Modifiers & ConsoleModifiers.Shift) == 0))
				{
					// Pass off to the movement handling
					if (IsMoveKey(keyInfo.Key))
					{
						// We'll render separately here to accomodate the rendering of only the nodes
						// or the whole map on scroll
						MoveSelectedNode(keyInfo.Key);
						return;
					}	

					switch (keyInfo.Key)
					{
						case ConsoleKey.NumPad5:
						case ConsoleKey.Enter:
							{
								var cmd = GetNodeAction();
	
								if (cmd != null)
								{
									ci.RunCommand(cmd.CommandIdentifier);
								}
									
								return;
							}
						case ConsoleKey.Add:
							{
								ci.RunCommand(Commands.ZOOM_MAP_OUT);
								return;
							}
						case ConsoleKey.Subtract:
							{
								ci.RunCommand(Commands.ZOOM_MAP_IN);
								return;
							}
						case ConsoleKey.Tab:
							{
								GetCommandInput(ci);
								break;
							}
						case ConsoleKey.Spacebar:
							{
								//TheGame().PlaySound(SoundType.CLICK1);
								ci.RunCommand(Commands.CYCLE_UNITS);
								return;
							}
						case ConsoleKey.M:
							{
								var cmd = GetMainMenuAction();
	
								if (cmd != null)
									ci.RunCommand(cmd.CommandIdentifier);

								return;
							}
						case ConsoleKey.H:
							{
								var cmd = GetHelpMenuAction();
	
								if (cmd != null)
									ci.RunCommand(cmd.CommandIdentifier);

								return;
							}

					}
				}									
			}
		}

		protected override void Handle_AI_IN_PLAY_Input(ICommandInterface ci)
		{
			return;
		}

		protected override void Handle_BATTLE_Input(ICommandInterface ci)
		{
			return;
		}

		protected override void Handle_GAME_OVER_Input(ICommandInterface ci)
		{
			return;
		}

		protected override void Handle_HELP_Input(ICommandInterface ci)
		{
			return;
		}

		protected override void Handle_QUICK_SELECT_Input(ICommandInterface ci)
		{
			return;
		}

		protected override void Handle_REINFORCE_Input(ICommandInterface ci)
		{
			return;
		}

		protected override void Handle_SCENARIO_INFO_Input(ICommandInterface ci)
		{
			return;
		}

		protected override void Handle_SETTINGS_MENU_Input(ICommandInterface ci)
		{
			return;
		}

		protected override void Handle_SPLASH_SCREEN_Input(ICommandInterface ci)
		{
			return;
		}

		protected override void Handle_TITLE_MENU_Input(ICommandInterface ci)
		{
			return;
		}

	#endregion

		public override void Exit()
		{
			if (TheGame().LoadedGame != null)
			{
				var proceed = StatusDisplay.Display("Save before quitting?", BoxDisplayType.INFO, PromptType.YES_NO_CANCEL);
				if (proceed == null)
				{
					TheGame().Renderer.RenderBoardFrame();
					StatusDisplay.StatusBox.RedrawControlAffectedNodes();
					return;
				}
			
				if (Convert.ToBoolean(proceed))
				{
					SaveGame();	
				}
			}

			base.Exit();
		}

		public override void DeleteGame()
		{
			StatusDisplay.Display("Delete command not implemented.", BoxDisplayType.ERROR);
		}

		public override void DisplayCommandList()
		{			
			StatusDisplay.Display("Command List command not implemented.", BoxDisplayType.ERROR);
		}

		public override void DisplayAssignedUnits()
		{
			StatusDisplay.Display("Display Assigned Units command not implemented.", BoxDisplayType.ERROR);
		}

		public override void DisplayUnits()
		{
			StatusDisplay.Display("Display Units command not implemented.", BoxDisplayType.ERROR);
		}

		public override void SetCurrentNode()
		{
			StatusDisplay.Display("Set Current Node command not implemented.", BoxDisplayType.ERROR);
		}

		public override void RemoveUnit()
		{
			StatusDisplay.Display("Remove Unit command not implemented.", BoxDisplayType.ERROR);
		}

		public override void MoveUnitToSelectedNode()
		{
			StatusDisplay.Display("Move Unit To Selected Node command not implemented.", BoxDisplayType.ERROR);
		}

		public override void AttachUnits()
		{
			StatusDisplay.Display("Attach Units command not implemented.", BoxDisplayType.ERROR);
		}

		public override void DetachUnits()
		{
			StatusDisplay.Display("Detach Units command not implemented.", BoxDisplayType.ERROR);
		}

		public override void AddReinforcementUnit()
		{
			StatusDisplay.Display("Add Reinforcement Unit command not implemented.", BoxDisplayType.ERROR);
		}

		public override void EditUnit()
		{
			var newName = GetNewUnitName();
			var unit = TheGame().GameBoard.SelectedUnits.SingleOrDefault();

			// Canceled or no/multiple units selected
			if (newName == null || unit == null)
				return;

			var node = unit.GetNode();
			var result = IsValidUnitNameEntered(newName);

			if (result.Status != ResultStatus.SUCCESS)
			{
				HandleResultDisplay(result, true);
				return;
			}			

			unit.Name = newName;

			var updateResult = TheGame().JTSServices.UnitService.UpdateUnits(new List<IUnit> { unit });
			HandleResultDisplay(updateResult, false);

			TheGame().Renderer.RefreshNodes(new[] { node });
		}

	#region event handlers

		protected override void On_MenuClickAction(object sender, EventArgs e)
		{
			//TheGame().PlaySound(SoundType.CLICK1);
		}

		protected override void On_MapMenuErased(object sender, EventArgs e)
		{
			// Re-draw affected areas
			TheGame().Renderer.RenderBoardFrame();
			//TheGame().Renderer.RenderMap(true);
			var control = sender as ConsoleControl;
			control.RedrawControlAffectedNodes();
		}

		protected override void On_MainMenuErased(object sender, EventArgs e)
		{
			TheGame().Renderer.RenderBoardFrame();
			var control = sender as ConsoleControl;
			control.RedrawControlAffectedNodes();
		}

		protected override void On_CmdBoxErased(object sender, EventArgs e)
		{
			// Re-draw affected areas
			TheGame().Renderer.RenderBoardFrame();
			var control = sender as ConsoleControl;
			control.RedrawControlAffectedNodes();
		}

		protected override void On_CmdBoxEscapePressed(object sender, EventArgs e)
		{
		}

		protected override void On_SaveAsGameTitleEscapePressed(object sender, EventArgs e)
		{
			TheGame().Renderer.RenderTileUnitInfoArea();
		}

		protected override void On_SaveAsGameTitleEntered(object sender, EventArgs e)
		{
			TheGame().Renderer.RenderTileUnitInfoArea();
		}

		private void On_NewUnitNameEscapePressed(object sender, EventArgs e)
		{
			TheGame().Renderer.RenderBoardFrame();
			var control = sender as ConsoleControl;
			control.RedrawControlAffectedNodes();
		}

		private void On_NewUnitNameBoxErased(object sender, EventArgs e)
		{
			TheGame().Renderer.RenderBoardFrame();
			var control = sender as ConsoleControl;
			control.RedrawControlAffectedNodes();
		}

	#endregion

#endregion

#region ActionMenus

		protected override Command GetNodeAction()
		{
			var node = TheGame().GameBoard.SelectedNode;

			var nodeActionBox = new MapMenuBox<Command>("Choose Action", TheGame(), _zoomHandler);
			nodeActionBox.Erased += On_MapMenuErased;
			nodeActionBox.EscapePressed += On_MenuClickAction;
			nodeActionBox.ItemSelected += On_MenuClickAction;

			foreach (var cmd in CommandInterface.GetAvailableCommandsForNode(node, TheGame()))
			{
				nodeActionBox.AddItem(new ListBoxItem<Command>(cmd, cmd.DisplayName));
			}

			if (nodeActionBox.ItemCount == 0) return null;

			nodeActionBox.ClearAndRedraw();
			return (nodeActionBox.SelectedItem != null) ? nodeActionBox.SelectedItem.Value : null;
		}

		protected override Command GetMainMenuAction()
		{
			var menuActionBox = new SelectListBox<Command>
				{
					Height = Global.Measurements.MAIN_MENU_ACTION_SELECT_HEIGHT,
					Width = Global.Measurements.MAIN_MENU_ACTION_SELECT_WIDTH,
					TopOrigin = Global.Measurements.MAINHEADERHEIGHT - 2,
					LeftOrigin = 3,
					BorderForeColor = Global.Colors.MainMenuBorderForeColor,
					BorderBackColor = Global.Colors.MainMenuBorderBackColor,
					DrawElements = new SingleLineBoxElements(),
					BackColor = Global.Colors.MainMenuBackColor,
					ForeColor = Global.Colors.MainMenuForeColor,
					CancelKeys = new List<ConsoleKey> {ConsoleKey.M},
					EraseColor = Global.Colors.MainMapBackColor,
					PromptColor = Global.Colors.MainMenuBorderForeColor,
					PageSize = 9,
					DropShadow = true,
					DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
				};

			menuActionBox.Erased += On_MainMenuErased;
			menuActionBox.EscapePressed += On_MenuClickAction;
			menuActionBox.ItemSelected += On_MenuClickAction;

			foreach (var cmd in CommandInterface.GetAvailableCommandsForMainMenu(TheGame()))
			{
				menuActionBox.AddItem(new ListBoxItem<Command>(cmd, cmd.DisplayName));
			}

			if (menuActionBox.ItemCount == 0) return null;

			menuActionBox.ClearAndRedraw();
			return (menuActionBox.SelectedItem != null) ? menuActionBox.SelectedItem.Value : null;
		}

		protected override Command GetHelpMenuAction()
		{
			var menuActionBox = new SelectListBox<Command>
				{
					Height = Global.Measurements.MAIN_MENU_ACTION_SELECT_HEIGHT,
					Width = Global.Measurements.MAIN_MENU_ACTION_SELECT_WIDTH,
					TopOrigin = Global.Measurements.MAINHEADERHEIGHT - 2,
					LeftOrigin = 20,
					BorderForeColor = Global.Colors.MainMenuBorderForeColor,
					BorderBackColor = Global.Colors.MainMenuBorderBackColor,
					DrawElements = new SingleLineBoxElements(),
					BackColor = Global.Colors.MainMenuBackColor,
					ForeColor = Global.Colors.MainMenuForeColor,
					EraseColor = Global.Colors.MainMapBackColor,
					PromptColor = Global.Colors.MainMenuBorderForeColor,
					CancelKeys = new List<ConsoleKey> {ConsoleKey.H},
					PageSize = 9,
					DropShadow = true,
					DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
				};

			menuActionBox.Erased += On_MainMenuErased;
			menuActionBox.EscapePressed += On_MenuClickAction;
			menuActionBox.ItemSelected += On_MenuClickAction;

			foreach (var cmd in CommandInterface.GetAvailableCommandsForHelpMenu(TheGame()))
			{
				menuActionBox.AddItem(new ListBoxItem<Command>(cmd, cmd.DisplayName));
			}

			if (menuActionBox.ItemCount == 0) return null;

			menuActionBox.ClearAndRedraw();
			return (menuActionBox.SelectedItem != null) ? menuActionBox.SelectedItem.Value : null;
		}

		protected override IUnit SelectUnit(IEnumerable<IUnit> units)
		{
			var selectUnitBox = new MapMenuBox<IUnit>("Select Unit", TheGame(), _zoomHandler);
			selectUnitBox.Erased += On_MapMenuErased;

			foreach (var u in units)
			{
				selectUnitBox.AddItem(new ListBoxItem<IUnit>(u, u.Name));
			}

			selectUnitBox.ClearAndRedraw();

			return (selectUnitBox.SelectedItem != null) ? selectUnitBox.SelectedItem.Value : null;
		}

		protected override IDemographic SelectDemographic(IEnumerable<IDemographic> demographics, string action)
		{
			var selectDemographicBox = new MapMenuBox<IDemographic>("Select Demographic to {0}".F(action), TheGame(), _zoomHandler);
			selectDemographicBox.Erased += On_MapMenuErased;

			foreach (var d in demographics)
			{
				selectDemographicBox.AddItem(new ListBoxItem<IDemographic>(d, (!string.IsNullOrWhiteSpace(d.InstanceName)) ? d.InstanceName : d.DemographicClass.Name));
			}

			selectDemographicBox.ClearAndRedraw();
			return (selectDemographicBox.SelectedItem != null) ? selectDemographicBox.SelectedItem.Value : null;
		}

		protected override IResult<Direction, Direction> SelectOrientation(IEnumerable<Direction> directions)
		{
			var retVal = new OperationResult<Direction, Direction>();

			var selectDirectionBox = new MapMenuBox<Direction>("Select Orientation", TheGame(), _zoomHandler);
			selectDirectionBox.Erased += On_MapMenuErased;

			foreach (var d in directions)
			{
				selectDirectionBox.AddItem(new ListBoxItem<Direction>(d, d.ToString()));
			}

			selectDirectionBox.ClearAndRedraw();

			if (selectDirectionBox.SelectedItem == null)
			{
				retVal.Status = ResultStatus.FAILURE;
				retVal.Messages.Add("Selected item is null.");
				return retVal;
			}

			retVal.Status = ResultStatus.SUCCESS;
			retVal.Result = selectDirectionBox.SelectedItem.Value;
			return retVal;
		}

		protected override string GetSaveGameAsTitle()
		{
			var saveAsGameTitle = new TextBox
				{
					Height = 6,
					Width = 30,
					LeftOrigin = (Console.WindowWidth / 2) - (Global.Measurements.MAIN_MENU_ACTION_SELECT_WIDTH / 2) - 2,
					TopOrigin = (Console.WindowHeight / 2) - (Global.Measurements.MAIN_MENU_ACTION_SELECT_HEIGHT / 2),
					BorderForeColor = Global.Colors.SelectContainerBorderForeColor,
					BorderBackColor = Global.Colors.SelectContainerBorderBGColor,
					BackColor = Global.Colors.SelectContainerForeColor,
					ForeColor = Global.Colors.ScreenBorderForeColor,
					PromptColor = Global.Colors.ScreenBorderForeColor,
					DrawElements = new SingleLineBoxElements(),
					MessageColor = ConsoleColor.Red,
					DropShadow = false,
					MaxCharacters = 30,
					Caption = "Save As Game Title"
				};

			saveAsGameTitle.CenterPositionHorizontal();

			saveAsGameTitle.EscapePressed += On_SaveAsGameTitleEscapePressed;
			saveAsGameTitle.TextEntered += On_SaveAsGameTitleEntered;

			saveAsGameTitle.ClearAndRedraw();

			var retVal = saveAsGameTitle.Text;
			return retVal;
		}

		private string GetNewUnitName()
		{
			var node = TheGame().GameBoard.SelectedNode;
			var mapHeight = _zoomHandler.CurrentZoom.DrawHeight * _zoomHandler.CurrentZoom.RowSpacing;
			var mapWidth = _zoomHandler.CurrentZoom.DrawWidth * _zoomHandler.CurrentZoom.ColumnSpacing;

			var leftOrigin = ((node.Location.X * _zoomHandler.CurrentZoom.ColumnSpacing) -
				                (_zoomHandler.CurrentZoom.CurrentOrigin.X * _zoomHandler.CurrentZoom.ColumnSpacing)) +
				                (Global.Measurements.WESTMARGIN + Global.Measurements.BOARDBOUNDARYWIDTH +
				                _zoomHandler.CurrentZoom.ColumnSpacing);
			var topOrigin = ((node.Location.Y * _zoomHandler.CurrentZoom.RowSpacing) -
				                (_zoomHandler.CurrentZoom.CurrentOrigin.Y * _zoomHandler.CurrentZoom.RowSpacing)) +
				            (Global.Measurements.NORTHMARGIN + Global.Measurements.BOARDBOUNDARYWIDTH);

			// Keeps us off the bottom of the screen
			if (topOrigin + Global.Measurements.NODE_ACTION_SELECT_HEIGHT > mapHeight)
				topOrigin -= Global.Measurements.NODE_ACTION_SELECT_HEIGHT - (_zoomHandler.CurrentZoom.RowSpacing - 2);

			var unitName = new TextBox
				{
					Height = 6,
					Width = 40,
					LeftOrigin = leftOrigin,
					TopOrigin = topOrigin,
					BorderForeColor = Global.Colors.SelectContainerBorderForeColor,
					BorderBackColor = Global.Colors.SelectContainerBorderBGColor,
					BackColor = Global.Colors.SelectContainerForeColor,
					ForeColor = Global.Colors.ScreenBorderForeColor,
					PromptColor = Global.Colors.ScreenBorderForeColor,
					DrawElements = new SingleLineBoxElements(),
					MessageColor = ConsoleColor.Red,
					DropShadow = false,
					MaxCharacters = 30,
					Caption = "Rename Unit"
				};

			unitName.EscapePressed += On_NewUnitNameEscapePressed;
			unitName.Erased += On_NewUnitNameBoxErased; 

			unitName.ClearAndRedraw();

			var retVal = unitName.Text;
			return retVal;
		}

		/// <summary>
		/// Creates a pop-up style box on the map adjacent to the currently selected node
		/// </summary>
		/// <typeparam name="T"></typeparam>
		private class MapMenuBox<T> : SelectListBox<T>, IMapMenuBox
		{
			public IEnumerable<INode> RedrawNodes { get; private set; }

			public MapMenuBox(string caption, IGame game, IZoomHandler zoomHandler)
			{
				var node = game.GameBoard.SelectedNode;
				var mapHeight = zoomHandler.CurrentZoom.DrawHeight * zoomHandler.CurrentZoom.RowSpacing;
				var mapWidth = zoomHandler.CurrentZoom.DrawWidth * zoomHandler.CurrentZoom.ColumnSpacing;

				var leftOrigin = ((node.Location.X * zoomHandler.CurrentZoom.ColumnSpacing) -
				                  (zoomHandler.CurrentZoom.CurrentOrigin.X * zoomHandler.CurrentZoom.ColumnSpacing)) +
				                 (Global.Measurements.WESTMARGIN + Global.Measurements.BOARDBOUNDARYWIDTH +
				                  zoomHandler.CurrentZoom.ColumnSpacing);
				var topOrigin = ((node.Location.Y * zoomHandler.CurrentZoom.RowSpacing) -
				                 (zoomHandler.CurrentZoom.CurrentOrigin.Y * zoomHandler.CurrentZoom.RowSpacing)) +
				                (Global.Measurements.NORTHMARGIN + Global.Measurements.BOARDBOUNDARYWIDTH);

				// Keeps us off the bottom of the screen
				if (topOrigin + Global.Measurements.NODE_ACTION_SELECT_HEIGHT > mapHeight)
					topOrigin -= Global.Measurements.NODE_ACTION_SELECT_HEIGHT - (zoomHandler.CurrentZoom.RowSpacing - 2);

				Height = Global.Measurements.NODE_ACTION_SELECT_HEIGHT;
				Width = Global.Measurements.NODE_ACTION_SELECT_WIDTH;
				LeftOrigin = leftOrigin;
				TopOrigin = topOrigin;
				BorderForeColor = Global.Colors.MapMenuBorderForeColor;
				BorderBackColor = Global.Colors.MapMenuBorderBackColor;
				DrawElements = new SingleLineBoxElements();
				BackColor = Global.Colors.MapMenuBackColor;
				ForeColor = Global.Colors.MapMenuForeColor;
				DropShadow = true;
				DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR;
				PromptColor = Global.Colors.MainMenuBorderForeColor; 
				EraseColor = Global.Colors.MainMapBackColor;
				PageSize = 15;
				Bullet = "∙";
				Caption = caption;
				CaptionColor = Global.Colors.MainMenuBorderForeColor;
			}
		}

		public interface IMapMenuBox
		{
			IEnumerable<INode> RedrawNodes { get; }
		}

#endregion

#region Input Validation

		protected override bool IsValidRow(string input)
		{
			int intVal;


			if (!CommandLineUtil.ConsoleUtils.IsValidateNumericInput(input))
			{
				SetInputError("Row must be a positive numeric value.");
				return false;
			}				

			intVal = Convert.ToInt32(input);

			if (intVal > TheGame().GameBoard.DefaultAttributes.Height - 1 || intVal < 0)
			{
				SetInputError("Row must be between 0 and {0}".F(TheGame().GameBoard.DefaultAttributes.Height - 1));
				return false;
			}

			_inputError = false;

			return true;
		}

		protected override bool IsValidColumn(string input)
		{
			int intVal;

			if (!CommandLineUtil.ConsoleUtils.IsValidateNumericInput(input))
			{
				SetInputError("Column must be a positive numeric value.");
				return false;
			}				

			intVal = Convert.ToInt32(input);

			if (intVal > TheGame().GameBoard.DefaultAttributes.Width - 1 || intVal < 0)
			{
				SetInputError("Column must be between 0 and {0}".F(TheGame().GameBoard.DefaultAttributes.Width - 1));
				return false;
			}

			_inputError = false;

			return true;
		}

		protected override bool IsValidLocationEntered()
		{
			if (_commandArgs.Args.Length < 2)
			{
				SetInputError("Command requires a Column and Row value.");
				return false;
			}

			_inputError = false;

			return (IsValidRow(_commandArgs.Args[1]) && IsValidColumn(_commandArgs.Args[0]));
		}

		protected override MouseButton GetValidMouseButtonClick(string input)
		{
			throw new NotImplementedException();
		}


#endregion

#region Command Line

		public override void GetCommandInput(ICommandInterface ci)
		{
			CurrentCMDRow = Global.Measurements.BASE_CMD_ORIGIN_TOP + 2;

			var command = HandleCMDInput("Enter Command");
			if (string.IsNullOrEmpty(command))
			{
				Console.ResetColor();
				return;
			} 

			ParseCommandArgs(command);
			RunCommand(ci);
		}

		protected override string HandleCMDInput(string message)
		{
			CmdBox = new TextBox
			{
				Height = Global.Measurements.BASE_CMD_HEIGHT,
				Width = Global.Measurements.BASE_CMD_WIDTH,
				LeftOrigin = Global.Measurements.BASE_CMD_ORIGIN_LEFT,
				TopOrigin = Global.Measurements.BASE_CMD_ORIGIN_TOP,
				BorderForeColor = Global.Colors.MainMenuBorderForeColor,
				BorderBackColor = Global.Colors.MainMenuBorderBackColor,
				DrawElements = new SingleLineBoxElements(),
				BackColor = Global.Colors.MainMenuBackColor,
				ForeColor = ConsoleColor.White,
				PromptColor = Global.Colors.MainMenuBorderForeColor,
				DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
				EraseColor = Global.Colors.MainMapBackColor,
				DropShadow = true,
				CancelKeys = new List<ConsoleKey> { ConsoleKey.Tab },
				Caption = message
			};

			CmdBox.MaxCharacters = CmdBox.Width - 6;
			CmdBox.Erased += On_CmdBoxErased;
			CmdBox.EscapePressed += On_CmdBoxEscapePressed;

			CmdBox.CenterPositionHorizontal();
			CmdBox.ClearAndRedraw();

			return CmdBox.Text;
		}

#endregion

#region Map Movement

		/// <summary>
		/// Moves the highlighted node as a cursor and scrolls the board when we reach the edges
		/// </summary>
		/// <param name="key"></param>
		private void MoveSelectedNode(ConsoleKey key)
		{
			switch (key)
			{
				case ConsoleKey.NumPad4:
                case ConsoleKey.LeftArrow:
					{
						var target = TheGame().JTSServices.NodeService.GetNodeAt(TheGame().GameBoard.SelectedNode.Location.X - 1,
																				TheGame().GameBoard.SelectedNode.Location.Y,
																				TheGame().GameBoard.SelectedNode.Location.Z);

						if (target == null) return;
						target.Select();

						HandleScrollAndRedraw(key);
						_zoomHandler.SyncAllZoomLevels();
						break;
					}
				case ConsoleKey.NumPad6:
                case ConsoleKey.RightArrow:
					{
						var target = TheGame().JTSServices.NodeService.GetNodeAt(TheGame().GameBoard.SelectedNode.Location.X + 1,
																				TheGame().GameBoard.SelectedNode.Location.Y,
																				TheGame().GameBoard.SelectedNode.Location.Z);

						if (target == null) return;
						target.Select();

						HandleScrollAndRedraw(key);
						_zoomHandler.SyncAllZoomLevels();

						break;
					}
				case ConsoleKey.NumPad2:
                case ConsoleKey.DownArrow:
					{						
						var target = TheGame().JTSServices.NodeService.GetNodeAt(TheGame().GameBoard.SelectedNode.Location.X,
																				TheGame().GameBoard.SelectedNode.Location.Y + 1,
																				TheGame().GameBoard.SelectedNode.Location.Z);

						if (target == null) return;
						target.Select();
							

						HandleScrollAndRedraw(key);
						_zoomHandler.SyncAllZoomLevels();
						break;
					}
				case ConsoleKey.NumPad8:
                case ConsoleKey.UpArrow:
					{
						var target = TheGame().JTSServices.NodeService.GetNodeAt(TheGame().GameBoard.SelectedNode.Location.X,
																				TheGame().GameBoard.SelectedNode.Location.Y - 1,
																				TheGame().GameBoard.SelectedNode.Location.Z);

						if (target == null) return;
						target.Select();

						HandleScrollAndRedraw(key);
						_zoomHandler.SyncAllZoomLevels();
						break;

					}
				case ConsoleKey.NumPad7:
					{
						var target = TheGame().JTSServices.NodeService.GetNodeAt(TheGame().GameBoard.SelectedNode.Location.X - 1,
																				TheGame().GameBoard.SelectedNode.Location.Y - 1,
																				TheGame().GameBoard.SelectedNode.Location.Z);

						
						if (target == null) return;
						target.Select();

						HandleScrollAndRedraw(key);
						_zoomHandler.SyncAllZoomLevels();
						break;
					}
				case ConsoleKey.NumPad9:
					{
						var target = TheGame().JTSServices.NodeService.GetNodeAt(TheGame().GameBoard.SelectedNode.Location.X + 1,
																				TheGame().GameBoard.SelectedNode.Location.Y - 1,
																				TheGame().GameBoard.SelectedNode.Location.Z);
						if (target == null) return;
						target.Select();

						HandleScrollAndRedraw(key);
						_zoomHandler.SyncAllZoomLevels();
						break;
					}
				case ConsoleKey.NumPad1:
					{						
						var target = TheGame().JTSServices.NodeService.GetNodeAt(TheGame().GameBoard.SelectedNode.Location.X - 1,
																				TheGame().GameBoard.SelectedNode.Location.Y + 1,
																				TheGame().GameBoard.SelectedNode.Location.Z);
						if (target == null) return;
						target.Select();

						HandleScrollAndRedraw(key);
						_zoomHandler.SyncAllZoomLevels();
						break;
					}
				case ConsoleKey.NumPad3:
					{						
						var target = TheGame().JTSServices.NodeService.GetNodeAt(TheGame().GameBoard.SelectedNode.Location.X + 1,
																				TheGame().GameBoard.SelectedNode.Location.Y + 1,
																				TheGame().GameBoard.SelectedNode.Location.Z);
						if (target == null) return;
						target.Select();
						
						HandleScrollAndRedraw(key);
						_zoomHandler.SyncAllZoomLevels();
						break;
					}

			}
		}

		private void HandleScrollAndRedraw(ConsoleKey key)
		{
			var width = _zoomHandler.CurrentZoom.DrawWidth;
			var height = _zoomHandler.CurrentZoom.DrawHeight;
			var currentOrigin = _zoomHandler.CurrentZoom.CurrentOrigin;

			var scroll = ((TheGame().GameBoard.SelectedNode.Location.X == (currentOrigin.X + width) ||
			               TheGame().GameBoard.SelectedNode.Location.Y == (currentOrigin.Y + height)))
			             ||
			             ((TheGame().GameBoard.SelectedNode.Location.X == (currentOrigin.X - 1) ||
			               TheGame().GameBoard.SelectedNode.Location.Y == (currentOrigin.Y + height)))
			             ||
			             ((TheGame().GameBoard.SelectedNode.Location.X == (currentOrigin.X + width) ||
			               TheGame().GameBoard.SelectedNode.Location.Y == (currentOrigin.Y - 1)))
			             ||
			             ((TheGame().GameBoard.SelectedNode.Location.X == (currentOrigin.X - 1) ||
			               TheGame().GameBoard.SelectedNode.Location.Y == (currentOrigin.Y - 1)))
			             ||
			             (TheGame().GameBoard.SelectedNode.Location.Y == (currentOrigin.Y - 1))
			             ||
			             (TheGame().GameBoard.SelectedNode.Location.Y == (currentOrigin.Y + height))
			             ||
			             (TheGame().GameBoard.SelectedNode.Location.X == (currentOrigin.X + width))
			             ||
			             (TheGame().GameBoard.SelectedNode.Location.X == (currentOrigin.X - 1));

			if (scroll)
			{
				ScrollMainBoard(key);
				TheGame().Renderer.RefreshActiveNodes();
				TheGame().Renderer.RenderTileUnitInfoArea();
			}
			else
			{
				TheGame().Renderer.RefreshActiveNodes();				
			}
		}

		private void ScrollMainBoard(ConsoleKey key)
		{
			// Console specific rendering that we don't buy with IRenderer
			var renderer = new ConsoleRenderer();
			var zoom = _zoomHandler.CurrentZoom;
			var width = zoom.DrawWidth;
			var height = zoom.DrawHeight;
			var mapWidth = zoom.DrawWidth * zoom.ColumnSpacing;
			var mapHeight = zoom.DrawHeight * zoom.RowSpacing;
			var currentOrigin = zoom.CurrentOrigin;
			var maxX = zoom.CurrentOrigin.X + zoom.DrawWidth;
			var maxY = zoom.CurrentOrigin.Y + zoom.DrawHeight;

			int grabWidth = mapWidth;
			int grabHeight = mapHeight;
			int newX = TheGame().GameBoard.MainMapOrigin.X;
			int newY = TheGame().GameBoard.MainMapOrigin.Y;
			var redrawNodes = new List<INode>();

			switch (key)
			{
				case ConsoleKey.NumPad4:
                case ConsoleKey.LeftArrow:
					{
						if (currentOrigin.X > 0)
						{
							TheGame().Renderer.RefreshNodes(new[] {TheGame().GameBoard.LastSelectedNode});
							_zoomHandler.CurrentZoom.CurrentOrigin.X--;
							TheGame().Renderer.SetCurrentViewableArea();

							grabWidth = mapWidth - zoom.ColumnSpacing;
							grabHeight = mapHeight;
							newX = TheGame().GameBoard.MainMapOrigin.X + zoom.ColumnSpacing;
							newY = TheGame().GameBoard.MainMapOrigin.Y;
							Console.MoveBufferArea(TheGame().GameBoard.MainMapOrigin.X, 
													TheGame().GameBoard.MainMapOrigin.Y, 
													grabWidth, grabHeight, newX, newY,
													' ', Global.Colors.MainMapBackColor, Global.Colors.MainMapBackColor);
							
							redrawNodes.AddRange(TheGame().JTSServices.NodeService.GetAllNodes()
																	.Where(n =>
																		(n.Location.X == zoom.CurrentOrigin.X)
																		&&
																		((n.Location.Y >= zoom.CurrentOrigin.Y) && (n.Location.Y < (maxY)))
																		).Where(n => n != null));
							
							renderer.RefreshNodesAreaOnly(redrawNodes);
						}
						break;
					}
				case ConsoleKey.NumPad6:
                case ConsoleKey.RightArrow:
					{
						TheGame().Renderer.RefreshNodes(new[] {TheGame().GameBoard.LastSelectedNode});		// Erases the cursor before the grab
						_zoomHandler.CurrentZoom.CurrentOrigin.X++;
						TheGame().Renderer.SetCurrentViewableArea();										// Resets for the redrawNodes

						grabWidth = mapWidth - zoom.ColumnSpacing;
						grabHeight = mapHeight;
						newX = TheGame().GameBoard.MainMapOrigin.X;
						newY = TheGame().GameBoard.MainMapOrigin.Y;
						Console.MoveBufferArea(TheGame().GameBoard.MainMapOrigin.X + zoom.ColumnSpacing, 
												TheGame().GameBoard.MainMapOrigin.Y, 
												grabWidth, grabHeight, newX, newY,
												' ', Global.Colors.MainMapBackColor, Global.Colors.MainMapBackColor);
							
						redrawNodes.AddRange(TheGame().JTSServices.NodeService.GetAllNodes()
																.Where(n =>
																	(n.Location.X == zoom.CurrentOrigin.X + (zoom.DrawWidth - 1))
																	&&
																	((n.Location.Y >= zoom.CurrentOrigin.Y) && (n.Location.Y <= (maxY)))
																	).Where(n => n != null));
							
						renderer.RefreshNodesAreaOnly(redrawNodes);
						break;
					}
				case ConsoleKey.NumPad2:
                case ConsoleKey.DownArrow:
					{
						TheGame().Renderer.RefreshNodes(new[] {TheGame().GameBoard.LastSelectedNode});		// Erases the cursor before the grab
						_zoomHandler.CurrentZoom.CurrentOrigin.Y++;
						TheGame().Renderer.SetCurrentViewableArea();										// Resets for the redrawNodes

						grabWidth = mapWidth;
						grabHeight = mapHeight - zoom.RowSpacing;
						newX = TheGame().GameBoard.MainMapOrigin.X;
						newY = TheGame().GameBoard.MainMapOrigin.Y;
						Console.MoveBufferArea(TheGame().GameBoard.MainMapOrigin.X, 
												TheGame().GameBoard.MainMapOrigin.Y + zoom.RowSpacing, 
												grabWidth, grabHeight, newX, newY,
												' ', Global.Colors.MainMapBackColor, Global.Colors.MainMapBackColor);
							
						redrawNodes.AddRange(TheGame().JTSServices.NodeService.GetAllNodes()
																.Where(n =>
																	(n.Location.Y == zoom.CurrentOrigin.Y + (zoom.DrawHeight - 1))
																	&&
																	((n.Location.X >= zoom.CurrentOrigin.X) && (n.Location.X <= (maxX)))
																	).Where(n => n != null));
							
						renderer.RefreshNodesAreaOnly(redrawNodes);
						break;
					}
				case ConsoleKey.NumPad8:
                case ConsoleKey.UpArrow:
					{
						if (currentOrigin.Y > 0)
						{
							TheGame().Renderer.RefreshNodes(new[] {TheGame().GameBoard.LastSelectedNode});
							_zoomHandler.CurrentZoom.CurrentOrigin.Y--;
							TheGame().Renderer.SetCurrentViewableArea();

							grabWidth = mapWidth;
							grabHeight = mapHeight - zoom.RowSpacing;
							newX = TheGame().GameBoard.MainMapOrigin.X;
							newY = TheGame().GameBoard.MainMapOrigin.Y + zoom.RowSpacing;
							Console.MoveBufferArea(TheGame().GameBoard.MainMapOrigin.X, 
													TheGame().GameBoard.MainMapOrigin.Y, 
													grabWidth, grabHeight, newX, newY,
													' ', Global.Colors.MainMapBackColor, Global.Colors.MainMapBackColor);
							
							redrawNodes.AddRange(TheGame().JTSServices.NodeService.GetAllNodes()
																	.Where(n =>
																		(n.Location.Y == zoom.CurrentOrigin.Y)
																		&&
																		((n.Location.X >= zoom.CurrentOrigin.X) && (n.Location.X <= (maxX)))
																		).Where(n => n != null));
							
							renderer.RefreshNodesAreaOnly(redrawNodes);
						}

						break;
					}
				case ConsoleKey.NumPad7:
					{
						int startOriginX = TheGame().GameBoard.MainMapOrigin.X;
						int startOriginY = TheGame().GameBoard.MainMapOrigin.Y;

						TheGame().Renderer.RefreshNodes(new[] {TheGame().GameBoard.LastSelectedNode});

						if (currentOrigin.X > 0)
						{
							_zoomHandler.CurrentZoom.CurrentOrigin.X--;
							grabWidth = mapWidth - zoom.ColumnSpacing;
							newX = TheGame().GameBoard.MainMapOrigin.X + zoom.ColumnSpacing;
							redrawNodes.AddRange(TheGame().JTSServices.NodeService.GetAllNodes()
							                              .Where(n =>
							                                     (n.Location.X == zoom.CurrentOrigin.X)
							                                     &&
							                                     ((n.Location.Y >= zoom.CurrentOrigin.Y) && (n.Location.Y <= (maxY)))
																).Where(n => n != null));
						}
							
						if (currentOrigin.Y > 0)
						{
							_zoomHandler.CurrentZoom.CurrentOrigin.Y--;
							grabHeight = mapHeight - zoom.RowSpacing;
							newY = TheGame().GameBoard.MainMapOrigin.Y + zoom.RowSpacing;
							redrawNodes.AddRange(TheGame().JTSServices.NodeService.GetAllNodes()
																	.Where(n =>
																		(n.Location.Y == zoom.CurrentOrigin.Y)
																		&&
																		((n.Location.X >= zoom.CurrentOrigin.X) && (n.Location.X <= (maxX)))
																		).Where(n => n != null));
						}

						Console.MoveBufferArea(startOriginX, startOriginY, 
												grabWidth, grabHeight, newX, newY,
												' ', Global.Colors.MainMapBackColor, Global.Colors.MainMapBackColor);
						TheGame().Renderer.SetCurrentViewableArea();
						renderer.RefreshNodesAreaOnly(redrawNodes);
						break;
					}
				case ConsoleKey.NumPad9:
					{
						int startOriginX = TheGame().GameBoard.MainMapOrigin.X;
						int startOriginY = TheGame().GameBoard.MainMapOrigin.Y;

						TheGame().Renderer.RefreshNodes(new[] {TheGame().GameBoard.LastSelectedNode});

						if (currentOrigin.X < (TheGame().GameBoard.DefaultAttributes.Width - width))
						{
							_zoomHandler.CurrentZoom.CurrentOrigin.X++;
							grabWidth = mapWidth - zoom.ColumnSpacing;
							newX = TheGame().GameBoard.MainMapOrigin.X;
							startOriginX += zoom.ColumnSpacing;
							redrawNodes.AddRange(TheGame().JTSServices.NodeService.GetAllNodes()
																.Where(n =>
																	(n.Location.X == zoom.CurrentOrigin.X + (zoom.DrawWidth - 1))
																	&&
																	((n.Location.Y >= zoom.CurrentOrigin.Y) && (n.Location.Y <= (maxY)))
																	).Where(n => n != null));
						}
							
						if (currentOrigin.Y > 0)
						{
							_zoomHandler.CurrentZoom.CurrentOrigin.Y--;
							grabHeight = mapHeight - zoom.RowSpacing;
							newY = TheGame().GameBoard.MainMapOrigin.Y + zoom.RowSpacing;
							redrawNodes.AddRange(TheGame().JTSServices.NodeService.GetAllNodes()
																	.Where(n =>
																		(n.Location.Y == zoom.CurrentOrigin.Y)
																		&&
																		((n.Location.X >= zoom.CurrentOrigin.X) && (n.Location.X <= (maxX)))
																		).Where(n => n != null));
						}

						Console.MoveBufferArea(startOriginX, startOriginY, grabWidth, 
												grabHeight, newX, newY,
												' ', Global.Colors.MainMapBackColor, Global.Colors.MainMapBackColor);
						TheGame().Renderer.SetCurrentViewableArea();
						renderer.RefreshNodesAreaOnly(redrawNodes);
						break;
					}
				case ConsoleKey.NumPad1:
					{
						TheGame().Renderer.RefreshNodes(new[] {TheGame().GameBoard.LastSelectedNode});

						int startOriginX = TheGame().GameBoard.MainMapOrigin.X;
						int startOriginY = TheGame().GameBoard.MainMapOrigin.Y;

						if (currentOrigin.X > 0)
						{
							_zoomHandler.CurrentZoom.CurrentOrigin.X--;
							grabWidth = mapWidth - zoom.ColumnSpacing;
							newX = TheGame().GameBoard.MainMapOrigin.X + zoom.ColumnSpacing;
							redrawNodes.AddRange(TheGame().JTSServices.NodeService.GetAllNodes()
																	.Where(n =>
																		(n.Location.X == zoom.CurrentOrigin.X)
																		&&
																		((n.Location.Y >= zoom.CurrentOrigin.Y) && (n.Location.Y <= (maxY)))
																		).Where(n => n != null));
						}
							
						if (currentOrigin.Y < (TheGame().GameBoard.DefaultAttributes.Height - height))
						{
							_zoomHandler.CurrentZoom.CurrentOrigin.Y++;
							grabHeight = mapHeight - zoom.RowSpacing;
							newY = TheGame().GameBoard.MainMapOrigin.Y;
							startOriginY += zoom.RowSpacing;
							redrawNodes.AddRange(TheGame().JTSServices.NodeService.GetAllNodes()
																.Where(n =>
																	(n.Location.Y == zoom.CurrentOrigin.Y + (zoom.DrawHeight - 1))
																	&&
																	((n.Location.X >= zoom.CurrentOrigin.X) && (n.Location.X <= (maxX)))
																	).Where(n => n != null));
							
						}

						Console.MoveBufferArea(startOriginX, startOriginY, 
												grabWidth, grabHeight, newX, newY,
												' ', Global.Colors.MainMapBackColor, Global.Colors.MainMapBackColor);
						TheGame().Renderer.SetCurrentViewableArea();
						renderer.RefreshNodesAreaOnly(redrawNodes);

						break;
					}
				case ConsoleKey.NumPad3:
					{
						TheGame().Renderer.RefreshNodes(new[] {TheGame().GameBoard.LastSelectedNode});

						int startOriginX = TheGame().GameBoard.MainMapOrigin.X;
						int startOriginY = TheGame().GameBoard.MainMapOrigin.Y;

						if (currentOrigin.X < (TheGame().GameBoard.DefaultAttributes.Width - width))
						{
							_zoomHandler.CurrentZoom.CurrentOrigin.X++;
							grabWidth = mapWidth - zoom.ColumnSpacing;
							newX = TheGame().GameBoard.MainMapOrigin.X;
							startOriginX += zoom.ColumnSpacing;
							redrawNodes.AddRange(TheGame().JTSServices.NodeService.GetAllNodes()
																.Where(n =>
																	(n.Location.X == zoom.CurrentOrigin.X + (zoom.DrawWidth - 1))
																	&&
																	((n.Location.Y >= zoom.CurrentOrigin.Y) && (n.Location.Y <= (maxY)))
																	).Where(n => n != null));
						}
							
						if (currentOrigin.Y < (TheGame().GameBoard.DefaultAttributes.Height - height))
						{
							_zoomHandler.CurrentZoom.CurrentOrigin.Y++;
							grabHeight = mapHeight - zoom.RowSpacing;
							newY = TheGame().GameBoard.MainMapOrigin.Y;
							startOriginY += zoom.RowSpacing;
							redrawNodes.AddRange(TheGame().JTSServices.NodeService.GetAllNodes()
																.Where(n =>
																	(n.Location.Y == zoom.CurrentOrigin.Y + (zoom.DrawHeight - 1))
																	&&
																	((n.Location.X >= zoom.CurrentOrigin.X) && (n.Location.X <= (maxX)))
																	).Where(n => n != null));
							
						}

						Console.MoveBufferArea(startOriginX, startOriginY, grabWidth, 
												grabHeight, newX, newY,
												' ', Global.Colors.MainMapBackColor, Global.Colors.MainMapBackColor);
						TheGame().Renderer.SetCurrentViewableArea();
						renderer.RefreshNodesAreaOnly(redrawNodes);

						break;
					}
			}
		}

		private void ScrollMainBoardFullArea(ConsoleKey key)
		{
			var width = _zoomHandler.CurrentZoom.DrawWidth;
			var height = _zoomHandler.CurrentZoom.DrawHeight;
			var currentOrigin = _zoomHandler.CurrentZoom.CurrentOrigin;
			var scroll = false;
			var previousNode = TheGame().GameBoard.SelectedNode;
			INode target = null;

			switch (key)
			{
				case ConsoleKey.NumPad4:
				case ConsoleKey.LeftArrow:
					{
						if (currentOrigin.X > 0 && currentOrigin.X < width)
						{
							_zoomHandler.CurrentZoom.CurrentOrigin.X = 0;
							scroll = true;
						}

						if (currentOrigin.X >= width)
						{
							_zoomHandler.CurrentZoom.CurrentOrigin.X = _zoomHandler.CurrentZoom.CurrentOrigin.X - width;
							scroll = true;
						}

						if (scroll)
							target = TheGame().JTSServices.NodeService.GetNodeAt(_zoomHandler.CurrentZoom.CurrentOrigin.X + (width - 1), previousNode.Location.Y, 0);					

						break;
					}
				case ConsoleKey.NumPad6:
				case ConsoleKey.RightArrow:
					{
						if (currentOrigin.X > (TheGame().GameBoard.DefaultAttributes.Width - (width * 2)))
						{
							_zoomHandler.CurrentZoom.CurrentOrigin.X = TheGame().GameBoard.DefaultAttributes.Width - width;
							scroll = true;
						}

						if (currentOrigin.X <= (TheGame().GameBoard.DefaultAttributes.Width - (width * 2)))
						{
							_zoomHandler.CurrentZoom.CurrentOrigin.X = _zoomHandler.CurrentZoom.CurrentOrigin.X + width;
							scroll = true;
						}

						if (scroll)
							target = TheGame().JTSServices.NodeService.GetNodeAt(_zoomHandler.CurrentZoom.CurrentOrigin.X, previousNode.Location.Y, 0);
							
						break;
					}
				case ConsoleKey.NumPad2:
				case ConsoleKey.DownArrow:
					{
						if (currentOrigin.Y > (TheGame().GameBoard.DefaultAttributes.Height - (height * 2)))
						{
							_zoomHandler.CurrentZoom.CurrentOrigin.Y = TheGame().GameBoard.DefaultAttributes.Height - height;
							scroll = true;
						}

						if (currentOrigin.Y <= (TheGame().GameBoard.DefaultAttributes.Height - (height * 2)))
						{
							_zoomHandler.CurrentZoom.CurrentOrigin.Y = _zoomHandler.CurrentZoom.CurrentOrigin.Y + height;
							scroll = true;
						}

						if (scroll)
							target = TheGame().JTSServices.NodeService.GetNodeAt(previousNode.Location.X, _zoomHandler.CurrentZoom.CurrentOrigin.Y, 0);
							
						break;
					}
				case ConsoleKey.NumPad8:
				case ConsoleKey.UpArrow:
					{
						if (currentOrigin.Y > 0 && currentOrigin.Y < height)
						{
							_zoomHandler.CurrentZoom.CurrentOrigin.Y = 0;
							scroll = true;
						}

						if (currentOrigin.Y >= height)
						{
							_zoomHandler.CurrentZoom.CurrentOrigin.Y = _zoomHandler.CurrentZoom.CurrentOrigin.Y - height;
							scroll = true;
						}

						if (scroll)
							target = TheGame().JTSServices.NodeService.GetNodeAt(previousNode.Location.X, _zoomHandler.CurrentZoom.CurrentOrigin.Y + (height - 1), 0);

						break;
					}
			}

			if (scroll)
			{
				target.Select();
				TheGame().Renderer.SetCurrentViewableArea();
				TheGame().Renderer.RenderMap(false);
				TheGame().Renderer.RenderTileUnitInfoArea();
				TheGame().Renderer.RefreshActiveNodes();
				_zoomHandler.SyncAllZoomLevelsFullArea();
			}
		}

		private bool IsMoveKey(ConsoleKey key)
		{
			var moveKeys = new[] {	ConsoleKey.NumPad1, 
									ConsoleKey.NumPad2, 
									ConsoleKey.NumPad3, 
									ConsoleKey.NumPad4,
									ConsoleKey.NumPad6,
									ConsoleKey.NumPad7,
									ConsoleKey.NumPad8,
									ConsoleKey.NumPad9,
                                    ConsoleKey.UpArrow,
                                    ConsoleKey.DownArrow,
                                    ConsoleKey.LeftArrow,
                                    ConsoleKey.RightArrow};

			return moveKeys.Contains(key);
		}

#endregion

	}

}
