using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Game;
using JTacticalSim.Component.World;
using JTacticalSim.Utility;
using ConsoleControls;
using API = JTacticalSim.API;

namespace JTacticalSim.ConsoleApp
{
	public class ConsoleRenderer : BaseRenderer
	{
		private IBoard _board { get { return TheGame().GameBoard; } }
		private IZoomHandler _zoomHandler { get { return TheGame().ZoomHandler; } }
		private IMapModeHandler _mapModeHandler { get { return TheGame().MapModeHandler; } }

		public ICoordinate FullMapOrigin { get; set; }

#region Screen Renderers

		public MainScreenRenderer MainScreenRenderer;
		public BattleScreenRenderer BattleScreenRenderer;
		public TitleScreenRenderer TitleScreenRenderer;
		public ReinforcementsScreenRenderer ReinforcementsScreenRenderer;
		public QuickSelectRenderer QuickSelectRenderer;
		public ScenarioInfoScreenRenderer ScenarioInfoScreenRenderer;
		public HelpScreenRenderer HelpScreenRenderer;
		public GameOverScreenRenderer GameOverScreenRenderer;

#endregion

		public ConsoleRenderer()
		{
			MainScreenRenderer = new MainScreenRenderer(this);
			BattleScreenRenderer = new BattleScreenRenderer(this);
			TitleScreenRenderer = new TitleScreenRenderer(this);
			ReinforcementsScreenRenderer = new ReinforcementsScreenRenderer(this);
			QuickSelectRenderer = new QuickSelectRenderer(this);
			ScenarioInfoScreenRenderer = new ScenarioInfoScreenRenderer(this);
			HelpScreenRenderer = new HelpScreenRenderer(this);
			GameOverScreenRenderer = new GameOverScreenRenderer(this);
		}

		public override void LoadContent()
		{
			Console.Title = "JTacticalSim - {0}".F(TheGame().LoadedGame.Name);
			TheGame().ZoomHandler = new ConsoleZoomHandler();
			TheGame().MapModeHandler = new ConsoleMapModeHandler();
			LoadZoomInfo();
			LoadMapModes();

			MainScreenRenderer.LoadContent();			
		}

		private void LoadZoomInfo()
		{
			var zooms = new List<ZoomInfo>
				{
					new ZoomInfo
						{
							RowSpacing = 8,
							ColumnSpacing = 12,
							CurrentOrigin = new Coordinate(0, 0, 0),
							DrawHeight = 9,
							DrawWidth = 12,
							IsCurrent = true,
							Level = ZoomLevel.FOUR
						},
					new ZoomInfo
						{
							RowSpacing = 6,
							ColumnSpacing = 10,
							CurrentOrigin = new Coordinate(0, 0, 0),
							DrawHeight = 12,
							DrawWidth = 14,
							IsCurrent = false,
							Level = ZoomLevel.THREE
						},
					new ZoomInfo
						{
							RowSpacing = 4,
							ColumnSpacing = 6,
							CurrentOrigin = new Coordinate(0, 0, 0),
							DrawHeight = 18,
							DrawWidth = 24,
							IsCurrent = false,
							Level = ZoomLevel.TWO
						},
					new ZoomInfo
						{
							RowSpacing = 2,
							ColumnSpacing = 2,
							CurrentOrigin = new Coordinate(0, 0, 0),
							DrawHeight = 16,
							DrawWidth = 25,
							IsCurrent = false,
							Level = ZoomLevel.ONE
						}
				};

			TheGame().ZoomHandler.LoadZooms(zooms);
			TheGame().GameBoard.DefaultAttributes.DrawHeight = _zoomHandler.GetZoomLevelInfo(ZoomLevel.FOUR).DrawHeight;
			TheGame().GameBoard.DefaultAttributes.DrawWidth = _zoomHandler.GetZoomLevelInfo(ZoomLevel.FOUR).DrawWidth;
		}

		public override void UnloadContent()
		{
			MainScreenRenderer.UnloadContent();
		}

		public override void DisplayUserMessage(MessageDisplayType messageType, string message, Exception ex)
		{
			var display = new StringBuilder("{0}".F(message));

			if (ex != null)
				display.AppendLine(ex.Message);

			StatusDisplay.Display(display.ToString(), (BoxDisplayType)messageType);
			RenderBoardFrame();
			StatusDisplay.StatusBox.RedrawControlAffectedNodes();
		}

		public override void DisplayTaskExecutionReport(StringBuilder report)
		{
			if (string.IsNullOrWhiteSpace(report.ToString()))
				return;

			var reportBox = new ConsoleBox(BoxDisplayType.INFO, PromptType.PRESS_ANY_KEY)
				{
					Width = Global.Measurements.BASE_REPORT_WIDTH,
					LeftOrigin = Global.Measurements.BASE_REPORT_ORIGIN_LEFT,
					TopOrigin = Global.Measurements.BASE_REPORT_ORIGIN_TOP,
					Height = Global.Measurements.BASE_REPORT_HEIGHT,
					BorderForeColor = ConsoleColor.White,
					BorderBackColor = ConsoleColor.DarkCyan,
					PromptColor = ConsoleColor.Gray,
					DrawElements = new SingleLineBoxElements(),
					BackColor = ConsoleColor.DarkCyan,
					ForeColor = ConsoleColor.White,
					DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
					DropShadow = true,
					Caption = "Current Missions Report",
					Text = report.ToString()
				};

			reportBox.CenterPositionHorizontal();
			reportBox.ClearAndRedraw();
			RenderBoardFrame();
			RenderMap(true);
		}

		public override bool ConfirmAction(string message)
		{
			var statusBox = new ConsoleBox(BoxDisplayType.ERROR, PromptType.YES_NO)
				{
					Width = Global.Measurements.BASE_ERROR_CMD_WIDTH,
					LeftOrigin = Global.Measurements.BASE_ERROR_CMD_ORIGIN_LEFT,
					TopOrigin = Global.Measurements.BASE_ERROR_CMD_ORIGIN_TOP,
					Height = 4,
					BorderForeColor = ConsoleColor.Black,
					BorderBackColor = ConsoleColor.Yellow,
					PromptColor = ConsoleColor.Black,
					DrawElements = new SingleLineBoxElements(),
					BackColor = ConsoleColor.Yellow,
					ForeColor = ConsoleColor.Black,
					DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
					DropShadow = true,
					Caption = "Warning!",
					Text = message
				};

			statusBox.CenterPositionHorizontal();
			statusBox.HasFocus = true;
			statusBox.ClearAndRedraw();
			var retVal = statusBox.Prompt;
			RenderBoardFrame();
			statusBox.RedrawControlAffectedNodes();
			return Convert.ToBoolean(retVal);
		}


	// Screens

#region Main Screen

		public override void RenderMainScreen()
		{
			MainScreenRenderer.RenderScreen();
		}
		
		public override void RenderBoardFrame()
		{
			MainScreenRenderer.RenderBoardFrame();
		}

		public override void RenderBoard()
		{
			MainScreenRenderer.RenderBoard();
		}

		// Map object??
		public override void RenderMap(bool clear)
		{
			MainScreenRenderer.RenderMap(clear);
		}

		public override void RenderFullMap(bool clear)
		{
 			MainScreenRenderer.RenderFullMap(clear);
		}

		public override void RenderTileUnitInfoArea()
		{
			MainScreenRenderer.RenderTileUnitInfoArea();
		}

		public override void RenderNode(INode node, int zoomLevel)
		{
			MainScreenRenderer.RenderNode(node, zoomLevel);
		}

		public override void RenderTile(ITile tile, int zoomLevel)
		{
			MainScreenRenderer.RenderTile(tile, zoomLevel);
		}

		public override void DisplayNodeInfo(INode node)
		{
			MainScreenRenderer.DisplayNodeInfo(node);
		}

		public override void DisplayTileInfo(ITile tile)
		{
			StatusDisplay.Display("Display Tile Info not implemented.", BoxDisplayType.ERROR);
		}

		public override void DisplayPlayerInfo(IPlayer player)
		{
			StatusDisplay.Display("Display Player Info not implemented.", BoxDisplayType.ERROR);
		}

		public override void DisplayLegend()
		{
			StatusDisplay.Display("Display Legend not implemented.", BoxDisplayType.ERROR);
		}

		public override void ResetTileDemographics(IEnumerable<INode> nodes)
		{
			MainScreenRenderer.ResetTileDemographics(nodes);
		}

		public override void SetCurrentViewableArea()
		{
			var maxX = _zoomHandler.CurrentZoom.CurrentOrigin.X + _zoomHandler.CurrentZoom.DrawWidth;
			var maxY = _zoomHandler.CurrentZoom.CurrentOrigin.Y + _zoomHandler.CurrentZoom.DrawHeight;
			TheGame().GameBoard.CurrentViewableAreaNodes = TheGame().JTSServices.NodeService.GetAllNodes()
															.Where(n =>
																((n.Location.X >= _zoomHandler.CurrentZoom.CurrentOrigin.X) && (n.Location.X < (maxX)))
																&&
																((n.Location.Y >= _zoomHandler.CurrentZoom.CurrentOrigin.Y) && (n.Location.Y < (maxY)))
																).Where(n => n != null);
		}

		private void LoadMapModes()
		{
			var modes = new List<MapModeInfo>();
			modes.Add(new MapModeInfo(MapMode.GEOGRAPHICAL, "Geography") { IsCurrent = true });
			modes.Add(new MapModeInfo(MapMode.POLITICAL, "Political") { IsCurrent = false });
			modes.Add(new MapModeInfo(MapMode.HIGH_CONTRAST, "High Contrast") {IsCurrent = false});
			_mapModeHandler.LoadMapModes(modes);

		}

		public override void RefreshActiveNodes()
		{
			if (_board.LastSelectedNode == null)
				_board.LastSelectedNode = _board.SelectedNode;

			var lastNode = _board.LastSelectedNode;
			var selectedNode = _board.SelectedNode;

			RefreshNodes(new[] { lastNode });
			RefreshNodes(new[] { selectedNode });

			TheGame().Renderer.RenderTileUnitInfoArea();
		}

		public override void RefreshActiveRoute()
		{
			if (_board.CurrentRoute == null)
				return;

			foreach (var node in _board.CurrentRoute.Nodes)
			{
				if (_board.NodeIsInViewableArea(node.GetNode()))
				{
					TheGame().Renderer.RenderNode(node.GetNode(), (int)_zoomHandler.CurrentZoom.Level);
				}
				
				//TheGame().Renderer.RenderNode(node.GetNode(), (int)ZoomLevel.ONE);
			}
		}	

		public override void RefreshNodes(IEnumerable<INode> nodes)
		{
			Action<INode> nodeAction = n =>
			{
				lock (nodes)
				{
					if (n == null) return;
					
					if (TheGame().GameBoard.NodeIsInViewableArea(n))
					{
						TheGame().Renderer.RenderNode(n, (int)_zoomHandler.CurrentZoom.Level);
					}
					//TheGame().Renderer.RenderNode(n, (int)ZoomLevel.ONE);
				}
			};

			if (TheGame().IsMultiThreaded)
			{
				Parallel.ForEach(nodes, nodeAction);
			}
			else
			{
				foreach (INode n in nodes)
				{
					nodeAction(n);
				}
			}
		}

		public void RefreshNodesAreaOnly(IEnumerable<INode> nodes)
		{
			Action<INode> nodeAction = n =>
			{
				lock (nodes)
				{
					if (n == null) return;
					
					if (TheGame().GameBoard.NodeIsInViewableArea(n))
					{
						TheGame().Renderer.RenderNode(n, (int)_zoomHandler.CurrentZoom.Level);
					}
				}
			};

			if (TheGame().IsMultiThreaded)
			{
				Parallel.ForEach(nodes, nodeAction);
			}
			else
			{
				foreach (INode n in nodes)
				{
					nodeAction(n);
				}
			}
		}

		public override void CenterSelectedNode()
		{
			var vOffSet = 0;
			var hOffSet = 0;

			var selectedRelativeX = (_board.SelectedNode.Location.X % _zoomHandler.CurrentZoom.DrawWidth);
			var selectedRelativeY = (_board.SelectedNode.Location.Y % _zoomHandler.CurrentZoom.DrawHeight);

			if (_board.SelectedNode.Location.X > (_zoomHandler.CurrentZoom.DrawWidth / 2) &&
				_board.SelectedNode.Location.X < ((_board.DefaultAttributes.Width - 1) - (_zoomHandler.CurrentZoom.DrawWidth)))
			{
				hOffSet = (_zoomHandler.CurrentZoom.DrawWidth / 2);
			}
				
			if (_board.SelectedNode.Location.Y > (_zoomHandler.CurrentZoom.DrawHeight / 2) &&
				_board.SelectedNode.Location.Y < ((_board.DefaultAttributes.Height - 1) - (_zoomHandler.CurrentZoom.DrawHeight)))
			{
				vOffSet = (_zoomHandler.CurrentZoom.DrawHeight / 2);
			}

			SetCurrentViewableArea();
			_zoomHandler.ResetAllZoomLevels(vOffSet, hOffSet);
		}

		public override void ZoomMap(API.CycleDirection direction)
		{
			var oldZoomLevel = _zoomHandler.CurrentZoom.Level;

			if (direction == API.CycleDirection.IN && _zoomHandler.CurrentZoom.Level <= ZoomLevel.TWO)
			{
				return;				
			}

			var cycled = _zoomHandler.CycleZoomLevel(direction);

			if (cycled)
			{
				SetCurrentViewableArea();
				RenderBoardFrame();
				RenderMap(true);
			}
		}

		public override void CycleMapMode(API.CycleDirection direction)
		{
			var cycled = _mapModeHandler.CycleMapMode(direction);

			if (cycled)
			{
				SetCurrentViewableArea();
				RenderBoardFrame();
				RenderMap(true);
				//RenderFullMap(true);
			}
		}

	// Units

		public override void DisplayUnits(List<IUnit> units)
		{
			StatusDisplay.Display("Display Units not implemented.", BoxDisplayType.ERROR);
		}

		public override void DisplayUnitInfo(IUnit unit)
		{
			MainScreenRenderer.DisplayUnitInfo(unit);
		}

		public override void RenderUnit(IUnit unit, int zoomLevel)
		{
			MainScreenRenderer.RenderUnit(unit, zoomLevel);
		}

		public override void RenderUnitName(IUnit unit)
		{
			StatusDisplay.Display("Render Unit Name not implemented.", BoxDisplayType.ERROR);
		}

		public override int RenderUnitStackInfo(IUnitStack stack)
		{
			return MainScreenRenderer.RenderUnitStackInfo(stack);
		}

#endregion

#region Reinforcements Screen

		public override void RenderReinforcementScreen()
		{
			ReinforcementsScreenRenderer.RenderScreen();
		}

#endregion

#region Title Screen

		public override void RenderTitleScreen()
		{
			TitleScreenRenderer.RenderScreen();
		}

#endregion

#region Battle Screen

		public override void RenderBattleScreen()
		{
			BattleScreenRenderer.RenderScreen();
		}

		public override void RenderBattle(IBattle battle)
		{
			BattleScreenRenderer.RenderBattle(battle);
		}

		public override void RenderBattleRound(IRound round)
		{
			BattleScreenRenderer.RenderBattleRound(round);
		}

		public override void RenderBattleSkirmish(ISkirmish skirmish)
		{
			BattleScreenRenderer.RenderBattleSkirmish(skirmish);
		}

		public override void RenderBattleSkirmishOutcome(ISkirmish skirmish)
		{
			BattleScreenRenderer.RenderSkirmishOutcome(skirmish);
		}

		public override void RenderBattleOutcome(IBattle battle)
		{
			BattleScreenRenderer.RenderBattleOutcome(battle);
		}

		public override void RenderBattleRetreat(IBattle battle)
		{
			BattleScreenRenderer.RenderBattleRetreat(battle);
		}

#endregion

#region Quick Select Screen

		public override void RenderQuickSelectScreen()
		{
 			QuickSelectRenderer.RenderScreen();
		}

#endregion

#region Scenario Info Screen

		public override void RenderScenarioInfoScreen()
		{
			ScenarioInfoScreenRenderer.RenderScreen();
		}

#endregion

#region Help Screen

		public override void RenderHelpScreen()
		{
			HelpScreenRenderer.RenderScreen();
		}

#endregion

#region GameOver Screen

		public override void RenderGameOverScreen()
		{
			GameOverScreenRenderer.RenderScreen();
		}

#endregion

#region Event Handlers


		#endregion

	}

}
