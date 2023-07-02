using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Component.Util;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.Component.World;
using JTacticalSim.Utility;
using ConsoleControls;

namespace JTacticalSim.ConsoleApp
{
	public sealed class MainScreenRenderer : BaseScreenRenderer, IScreenRenderer
	{
		private ZoomInfo zlInfo1;
		private ZoomInfo zlInfo2;
		private ZoomInfo zlInfo3;
		private ZoomInfo zlInfo4;

#region Controls

		// Main Screen
		public ConsoleBox TopMenuBox;
		public ConsoleBox TitleBox;
		public ConsoleBox HeaderBox;
		public ConsoleBox LeftBGBox;
		public ConsoleBox RightBGBox;
		public ConsoleBox MiddleBox;
		public ConsoleBox PlayerInfoBox;
		public ConsoleBox NodeInfoBox;
		public ConsoleBox MapBoundary;
        //public ConsoleBox FullMapBoundary;
		//public ConsoleBox InstructionsBox;
		public ConsoleBox UnplaceReinforcementsBox;

#endregion

		private IEnumerable<TileQuadrants> Quadrants;

		public MainScreenRenderer(ConsoleRenderer baseRenderer)
		{
			_baseRenderer = baseRenderer;
		}

		public void LoadContent()
		{
			InitializeControls();
			TheGame().GameBoard.MainMapOrigin = new Coordinate(MapBoundary.LeftOrigin + 1, MapBoundary.TopOrigin + 1, 0);
			//_baseRenderer.FullMapOrigin = new Coordinate(FullMapBoundary.LeftOrigin + 1, FullMapBoundary.TopOrigin + 1, 0);
			Thread.Sleep(500);

			// Pre-set all the draw elements for the map
			ResetTileDemographics(TheGame().JTSServices.NodeService.GetAllNodes());

			zlInfo1 = ZoomHandler.GetZoomLevelInfo(ZoomLevel.ONE);
			zlInfo2 = ZoomHandler.GetZoomLevelInfo(ZoomLevel.TWO);
			zlInfo3 = ZoomHandler.GetZoomLevelInfo(ZoomLevel.THREE);
			zlInfo4 = ZoomHandler.GetZoomLevelInfo(ZoomLevel.FOUR);
			
			Console.Clear();			
		}

		public void UnloadContent()
		{
			// Do nothing....
		}

		protected override void InitializeControls()
		{
			TopMenuBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE)
				{
					Height = 3,
					Width = Console.WindowWidth - 2,
					LeftOrigin = 0,
					TopOrigin = 0,
					BackColor = Global.Colors.TopMenuBGColor,
					Border = false,
					Text = "   [M]ain Menu      [H]elp"
				};

			// Main game top box
			HeaderBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE)
				{
					Height = 1,
					Width = Console.WindowWidth - 2,
					LeftOrigin = 0,
					TopOrigin = TopMenuBox.TopOrigin + TopMenuBox.Height,
					BackColor = Global.Colors.BoardBoundaryBGColor,
					ForeColor = ConsoleColor.Black,
					FillElement = '░',
					Border = false
				};

			// Main Screen
			var mainBoardRight = Global.Measurements.WESTMARGIN + (Global.Measurements.BOARDBOUNDARYWIDTH * 2) + (ZoomHandler.GetZoomLevelInfo(ZoomLevel.FOUR).ColumnSpacing * TheGame().GameBoard.DefaultAttributes.DrawWidth);
			var nodeInfoLeftMargin = mainBoardRight;

			var mapSpacesHeight = ZoomHandler.GetZoomLevelInfo(ZoomLevel.FOUR).RowSpacing * TheGame().GameBoard.DefaultAttributes.DrawHeight;
			var drawSpacesWidth = ZoomHandler.GetZoomLevelInfo(ZoomLevel.FOUR).ColumnSpacing * TheGame().GameBoard.DefaultAttributes.DrawWidth;
			var drawSpacesHeight = ZoomHandler.GetZoomLevelInfo(ZoomLevel.FOUR).RowSpacing * 6;

			// Area Map
			MapBoundary = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE)
				{
					Height = mapSpacesHeight,
					Width = drawSpacesWidth,
					LeftOrigin = Global.Measurements.WESTMARGIN,
					TopOrigin = Global.Measurements.NORTHMARGIN,
					BorderForeColor = Global.Colors.BoardBoundaryFGColor,
					BackColor = Global.Colors.MainMapBackColor,
					DrawElements = new DoubleLineBoxElements()
				};

			// Tile/Unit Info
			NodeInfoBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE)
				{
					Height = drawSpacesHeight,
					Width = 47,
					LeftOrigin = nodeInfoLeftMargin,
					TopOrigin = Global.Measurements.NORTHMARGIN,
					BorderForeColor = Global.Colors.BoardBoundaryFGColor,
					BackColor = ConsoleColor.Black,
					DrawElements = new DoubleLineBoxElements(),
					Caption = "Location/Unit Info"
				};

			// Main game middle box
			MiddleBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE)
				{
					Height = 6,
					Width = Console.WindowWidth - 2,
					LeftOrigin = 0,
					TopOrigin = MapBoundary.TopOrigin + MapBoundary.Height + 1,
					BackColor = Global.Colors.BoardBoundaryBGColor,
					ForeColor = ConsoleColor.Black,
					FillElement = '░',
					Border = false
				};

			// Left side box
			LeftBGBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE)
				{
					Height = Console.WindowHeight - (HeaderBox.Height + MiddleBox.Height),
					Width = Global.Measurements.WESTMARGIN,
					LeftOrigin = 0,
					TopOrigin = HeaderBox.TopOrigin + HeaderBox.Height,
					BackColor = Global.Colors.BoardBoundaryBGColor,
					ForeColor = ConsoleColor.Black,
					FillElement = '░',
					Border = false
				};

			// Left side box
			RightBGBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE)
				{
					Height = Console.WindowHeight - (HeaderBox.Height + MiddleBox.Height),
					Width = Global.Measurements.WESTMARGIN,
					LeftOrigin = HeaderBox.LeftOrigin + HeaderBox.Width - Global.Measurements.WESTMARGIN,
					TopOrigin = HeaderBox.TopOrigin + HeaderBox.Height,
					BackColor = Global.Colors.BoardBoundaryBGColor,
					ForeColor = ConsoleColor.Black,
					FillElement = '░',
					Border = false
				};

			// Player info container box
			PlayerInfoBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE)
				{
					Height = 3,
					Width = Console.WindowWidth - 2,
					LeftOrigin = 0,
					TopOrigin = MiddleBox.TopOrigin + 2,
					BackColor = ConsoleColor.Black,
					ForeColor = ConsoleColor.White,
					Border = false
				};

			// Unplaced Reinforcements
			UnplaceReinforcementsBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE)
				{
					Height = 22,
					Width = NodeInfoBox.Width,
					TopOrigin = NodeInfoBox.TopOrigin + NodeInfoBox.Height + 2,
					LeftOrigin = NodeInfoBox.LeftOrigin,
                    BackColor = ConsoleColor.Black,
                    BorderForeColor = Global.Colors.BoardBoundaryFGColor,
					ForeColor = Global.Colors.BoardBoundaryFGColor,
					DrawElements = new DoubleLineBoxElements(),
					Caption = "Available Reinforcements"
				};
		}

		public override void RenderScreen()
		{
			Console.ResetColor();
			RenderBoardFrame();
			RenderBoard();
			base.RenderScreen();
		}

		public override void RefreshScreen()
		{
		}

		private void DrawPlayerSummary()
		{
			var rpPerTurn = TheGame().JTSServices.RulesService.CalculateReinforcementPointsForTurn(TheGame().CurrentTurn.Player).Result;
			PlayerInfoBox.ClearAndRedraw();
			var currentRow = PlayerInfoBox.TopOrigin + 1;
			const int margin = 4;

			Console.BackgroundColor = PlayerInfoBox.BackColor;
			Console.ForegroundColor = PlayerInfoBox.ForeColor;

			Console.SetCursorPosition(PlayerInfoBox.LeftOrigin + margin, currentRow);
			Console.ForegroundColor = TheGame().CurrentTurn.Player.Country.TextDisplayColor;
			Console.Write("{0}  ".F(TheGame().CurrentTurn.Player.Country.Description));
			DrawFlag(TheGame().CurrentTurn.Player.Country);

			Console.BackgroundColor = PlayerInfoBox.BackColor;
			Console.ForegroundColor = PlayerInfoBox.ForeColor;

			Console.SetCursorPosition(Console.CursorLeft + 5, currentRow);
			Console.Write("    Faction Alignment: ");

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write(TheGame().CurrentTurn.Player.Country.Faction.Name);
			Console.ForegroundColor = PlayerInfoBox.ForeColor;

			Console.SetCursorPosition(Console.WindowWidth - 84, currentRow);
			Console.Write("Nuclear Charges: ");
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("{0}",TheGame().CurrentTurn.Player.TrackedValues.NuclearCharges.ToString());
			Console.ForegroundColor = PlayerInfoBox.ForeColor;

			Console.SetCursorPosition(Console.WindowWidth - 60, currentRow);
			Console.Write("Reinforcement Points: ");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write("{0}",TheGame().CurrentTurn.Player.TrackedValues.ReinforcementPoints.ToString());
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write(" / ");
			Console.ForegroundColor = ConsoleColor.DarkCyan;
			Console.Write("{0}", rpPerTurn);
			Console.ForegroundColor = PlayerInfoBox.ForeColor;

			Console.SetCursorPosition(Console.WindowWidth - 23, currentRow);
			Console.Write("Victory Points: ");
			Console.BackgroundColor = Global.Colors.VictoryPointBackColor;
            Console.ForegroundColor = Global.Colors.VictoryPointForeColor;
			Console.Write(" {0}".F(TheGame().CurrentTurn.Player.Country.Faction.GetCurrentVictoryPoints()));

			Console.ResetColor();
		}

	// Board

		public void RenderBoardFrame()
		{
			TopMenuBox.ForeColor = (TheGame().StateSystem.CurrentStateType == StateType.GAME_IN_PLAY)
									? Global.Colors.TopMenuForeColor
									: Global.Colors.TopMenuForeColorUnavailable;
			TopMenuBox.ClearAndRedraw();
			HeaderBox.Fill();
			MiddleBox.Fill();
			LeftBGBox.Fill();
			RightBGBox.Fill();
			PlayerInfoBox.Fill();
			PlayerInfoBox.Draw();
			DrawAvailableReinforcements();
			DrawPlayerSummary();
			MapBoundary.Caption = GetMapLabel();
			MapBoundary.Draw();
			NodeInfoBox.Fill();
			NodeInfoBox.Draw();			
			RenderTileUnitInfoArea();
		}

		public void RenderBoard()
		{
			NodeInfoBox.ClearAndRedraw();
			MapBoundary.ClearAndRedraw();
			//FullMapBoundary.ClearAndRedraw();
			RenderMap(false);			
           // RenderFullMap(false);
		}

		public void RenderMap(bool clear)
		{
			if (clear) 
			{
				MapBoundary.ClearAndRedraw();
				NodeInfoBox.ClearAndRedraw();
			}

			DrawMapArea();
			RenderTileUnitInfoArea();
		}

        public void RenderFullMap(bool clear)
        {
			//if (clear)
			//	FullMapBoundary.ClearAndRedraw();

			//DrawFullMapArea();
        }

		private void DrawMapArea()
		{
			Console.ResetColor();
			var currentZoom = ZoomHandler.CurrentZoom;

			if (!TheGame().GameBoard.CurrentViewableAreaNodes.Any())
				TheGame().Renderer.SetCurrentViewableArea();

			Action<INode> nodeAction = n =>
			{
				lock (TheGame().GameBoard.CurrentViewableAreaNodes)
				{
					n.DefaultTile.Render((int)currentZoom.Level);
				}
			};

			if (TheGame().IsMultiThreaded)
			{
				Parallel.ForEach(TheGame().GameBoard.CurrentViewableAreaNodes, nodeAction);
			}
			else
			{
				foreach (INode n in TheGame().GameBoard.CurrentViewableAreaNodes)
				{
					nodeAction(n);
				}
			}
		}

		public static void DrawMapBottomArea(int rows)
		{
			var game = Game.Instance;
			var zoomHandler = game.ZoomHandler;
			var maxX = zoomHandler.CurrentZoom.CurrentOrigin.X + zoomHandler.CurrentZoom.DrawWidth;
			var maxY = zoomHandler.CurrentZoom.CurrentOrigin.Y + zoomHandler.CurrentZoom.DrawHeight;
			var bottomAreaNodes = game.JTSServices.NodeService.GetAllNodes()
											.Where(n =>
												((n.Location.X >= zoomHandler.CurrentZoom.CurrentOrigin.X) && (n.Location.X < (maxX)))
												&&
												((n.Location.Y >= zoomHandler.CurrentZoom.CurrentOrigin.Y + zoomHandler.CurrentZoom.DrawHeight - rows) && (n.Location.Y < (maxY)))
												).Where(n => n != null);
			
			if (!game.GameBoard.CurrentViewableAreaNodes.Any())
				game.Renderer.SetCurrentViewableArea();

			game.Renderer.RefreshNodes(bottomAreaNodes);
		}

        private void DrawFullMapArea()
        {
	        var nodes = TheGame().JTSServices.NodeService.GetAllNodes().Where(n => n != null);

			Action<INode> nodeAction = n =>
			{
				lock (nodes)
				{
					RenderTileZoomLevel1(n.DefaultTile);
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

		private void DrawAvailableReinforcements()
		{
			//TODO: Make this a select list so the user can scroll
			UnplaceReinforcementsBox.ClearAndRedraw();

			var units = TheGame().CurrentTurn.Player.UnplacedReinforcements;
			var currentLine = UnplaceReinforcementsBox.TopOrigin + 1;
			var textMargin = UnplaceReinforcementsBox.LeftOrigin + 2;

			Console.BackgroundColor = UnplaceReinforcementsBox.BackColor;

			foreach (var u in units)
			{
				Console.ForegroundColor = u.Country.TextDisplayColor;

				currentLine++;
				Console.SetCursorPosition(textMargin, currentLine);
				Console.Write(u.FullTabbedDisplayName());
			}

			Console.ResetColor();

		}

		private string GetMapLabel()
		{
			var sb = new StringBuilder();

			var currentZoom = ZoomHandler.CurrentZoom;
			var distanceTag = "meters";
			double widthDistance = currentZoom.DrawWidth * TheGame().GameBoard.DefaultAttributes.CellMeters;
			double heightDistance = currentZoom.DrawHeight * TheGame().GameBoard.DefaultAttributes.CellMeters;

			if (widthDistance > 1000 && heightDistance > 1000)
			{
				widthDistance = (widthDistance / 1000);
				heightDistance = (heightDistance / 1000);
				distanceTag = "km";
			}            
			
			// FYI: We're keeping the full map at zoom level 1. Might change this
            // This requires us to display the actual zoom level as one less than it's actual configured level
            // e.g. zoom level 2 is actually level 1 in the zoom area
            sb.Append(" Area Map : Level {0}  @  {1} X {2} {3} : {4} mode ".F(((int)currentZoom.Level - 1).ToString(), 
																				widthDistance, 
																				heightDistance,
																				distanceTag,
																				MapModeHandler.CurrentMapMode.Name));
			return sb.ToString();
		}


		public void RenderTileUnitInfoArea()
		{
			Console.ResetColor();

			NodeInfoBox.Clear();
			var textMargin = NodeInfoBox.LeftOrigin + 2;
			var currentLine = Global.Measurements.NORTHMARGIN + 2;

			Console.SetCursorPosition(textMargin, currentLine++);
			var node = TheGame().GameBoard.SelectedNode;

			// Only render content if there is a currently selected node

			if (node == null)
				return;

			var tile = node.DefaultTile;

			if (!string.IsNullOrWhiteSpace(tile.Name))
			{
				DrawNodeInfoBoxValue("", tile.Name, textMargin);
				Console.SetCursorPosition(textMargin, currentLine++);
			}

			var demNames = tile.GetAllDemographicNames();
			if (demNames.Any())
			{
				tile.GetAllDemographicNames().ForEach(name =>
					{
						DrawNodeInfoBoxValue("", name, textMargin);
						Console.SetCursorPosition(textMargin, currentLine++);
					});
			}
			
			Console.SetCursorPosition(textMargin, currentLine++);
			DrawNodeInfoBoxValue("Current Location  ", node.Location.ToString(), textMargin + 26);
			Console.SetCursorPosition(textMargin, currentLine);
			Console.Write("Country  ");
			Console.SetCursorPosition(textMargin + 26, currentLine);
			DrawFlag(tile.Country);
			Console.ResetColor();

		// ~~~~~~~~~~~~ List Units

			var selectedUnits = TheGame().GameBoard.SelectedUnits.Where(u => !u.IsHiddenFromEnemy() && u.IsVisible());

			if (TheGame().GameBoard.SelectedUnits.Any())
			{
				Console.SetCursorPosition(textMargin, currentLine++);
				Console.SetCursorPosition(textMargin, currentLine++);
				Console.SetCursorPosition(textMargin, currentLine++);
				Console.Write(String.Concat(Enumerable.Repeat("_", 36)));
				Console.SetCursorPosition(textMargin, currentLine++);
				Console.SetCursorPosition(textMargin, currentLine++);
				Console.Write("Selected Units");
				Console.SetCursorPosition(textMargin, currentLine++);

				foreach (var u in selectedUnits)
				{
					Console.ForegroundColor = SetCursorColor();

					currentLine++;
					Console.SetCursorPosition(textMargin, currentLine);
					Console.Write(u.FullTabbedDisplayName());

					if (u.GetTransportedUnits().Any())
					{
						u.GetTransportedUnits().ToList().ForEach(tu =>
							{
								currentLine++;

								var resetColor = Console.ForegroundColor;
								Console.SetCursorPosition(textMargin + 9, currentLine);
								Console.ForegroundColor = ConsoleColor.DarkMagenta;
								Console.Write("→ ");
								Console.ForegroundColor = resetColor;
								Console.Write(tu.UnitInfo.UnitType.TextDisplayZ4);
								Console.SetCursorPosition(textMargin + 13, currentLine);
								Console.Write(tu.Name);
							});
					}

					Console.ResetColor();
				}
			}

			var units = node.GetAllUnits().Where(u => !u.IsHiddenFromEnemy() && 
														u.IsVisible() && 
														!TheGame().GameBoard.SelectedUnits.Contains(u));
			if (units.Any())
			{
				Console.SetCursorPosition(textMargin, currentLine++);
				Console.SetCursorPosition(textMargin, currentLine++);
				Console.Write(String.Concat(Enumerable.Repeat("_", 36)));
				Console.SetCursorPosition(textMargin, currentLine++);
				Console.SetCursorPosition(textMargin, currentLine++);
				Console.Write("Units At Current Location");
				Console.SetCursorPosition(textMargin, currentLine++);

				foreach (var u in units)
				{
					Console.ForegroundColor = u.Country.TextDisplayColor;

					currentLine++;
					Console.SetCursorPosition(textMargin, currentLine);
					Console.Write(u.FullTabbedDisplayName());

					if (u.GetTransportedUnits().Any())
					{
						u.GetTransportedUnits().ToList().ForEach(tu =>
							{
								currentLine++;

								var resetColor = Console.ForegroundColor;
								Console.SetCursorPosition(textMargin + 9, currentLine);
								Console.ForegroundColor = ConsoleColor.DarkMagenta;
								Console.Write("→ ");
								Console.ForegroundColor = resetColor;
								Console.Write(tu.UnitInfo.UnitType.TextDisplayZ4);
								Console.SetCursorPosition(textMargin + 13, currentLine);
								Console.Write(tu.Name);
							});
					}

					Console.ResetColor();
				}
			}
			
		}

		private void DrawNodeInfoBoxValue(string text, string value, int tab)
		{
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write(text);
			Console.CursorLeft = tab;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write(value);
			Console.ResetColor();
		}

		public void RenderNode(INode node, int zoomLevel)
		{
			// Only dealing with the default tile now - flat dimensions
			RenderTile(node.DefaultTile, zoomLevel);
		}

		public void RenderTile(ITile tile, int zoomLevel)
		{
			// Since we have two different zoom levels at one time in this version
			switch (zoomLevel)
			{
				case 1:
					RenderTileZoomLevel1(tile);
					break;
				case 2:
					RenderTileZoomLevel2(tile);
					break;
				case 3:
					RenderTileZoomLevel3(tile);
					break;
				case 4:
					RenderTileZoomLevel4(tile);
					break;

			}
		}

	// Zoom ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		// TODO: Consolodate these methods as well


		private void RenderTileZoomLevel4(ITile tile)
		{
			ICoordinate origin = GetOriginForTileRender(tile.Location);

			// 1. Render Base Geography
			// 2. Render Demographics
			// 3. Render Units
			// 4. Create selected node outline

			var quadrants = RenderQuadrants(tile.Location, ZoomLevel.FOUR, MapModeHandler.CurrentMapMode);

			Console.SetCursorPosition(origin.X + 1, origin.Y + 1);
			DrawFlag(tile.Country);

			if (tile.VictoryPoints > 0)
			{
				Console.SetCursorPosition(origin.X + 3, origin.Y + 1);
				Console.BackgroundColor = quadrants.TopRightQuadrant.BGColor;
				DrawVictoryPoints(tile.VictoryPoints.ToString());
			}

			if (tile.TotalUnitCount > 0)
			{
				Console.SetCursorPosition(origin.X + 4, origin.Y + 4);
				RenderUnitStacksZoom4(tile, origin.X, origin.Y);
			}
			
			var IsCurrentNode = (TheGame().GameBoard.SelectedNode != null) && (TheGame().GameBoard.SelectedNode.LocationEquals(tile.Location));

			if (IsCurrentNode)
				DrawSelectedNodeBox(origin.X, origin.Y, ZoomLevel.FOUR);		
		}

		private void RenderTileZoomLevel3(ITile tile)
		{
			// Don't pre-set this. Save the processing if we can
			ICoordinate origin = null;

			var quadrants = RenderQuadrants(tile.Location, ZoomLevel.THREE, MapModeHandler.CurrentMapMode);

			if (tile.VictoryPoints > 0)
			{
				origin = GetOriginForTileRender(tile.Location);
				Console.SetCursorPosition(origin.X + 1,  origin.Y + 1);
				Console.BackgroundColor = quadrants.TopRightQuadrant.BGColor;
				DrawVictoryPoints(tile.VictoryPoints.ToString());
			}

			if (MapModeHandler.CurrentMapMode.MapMode == MapMode.POLITICAL)
			{ 
				if (origin == null)
					origin = GetOriginForTileRender(tile.Location);

				Console.SetCursorPosition(origin.X + 1, origin.Y + 1);
				DrawFlag(tile.Country);
			}

			if (tile.TotalUnitCount > 0)
			{
				if (origin == null)
					origin = GetOriginForTileRender(tile.Location);

				Console.SetCursorPosition(origin.X,  origin.Y);
				RenderUnitStacksZoom3(tile, origin.X,  origin.Y);
			}

			var IsCurrentNode = (TheGame().GameBoard.SelectedNode != null) && (TheGame().GameBoard.SelectedNode.LocationEquals(tile.Location));

			if (IsCurrentNode)
			{
				if (origin == null)
					origin = GetOriginForTileRender(tile.Location);

				DrawSelectedNodeBox(origin.X, origin.Y, ZoomLevel.THREE);
			}
		}

		private void RenderTileZoomLevel2(ITile tile)
		{
			// Don't pre-set this. Save the processing if we can
			ICoordinate origin = null;

			var quadrants = RenderQuadrants(tile.Location, ZoomLevel.TWO, MapModeHandler.CurrentMapMode);

			if (tile.VictoryPoints > 0)
			{
				origin = GetOriginForTileRender(tile.Location);
				Console.SetCursorPosition(origin.X + 3, origin.Y + 1);
				Console.BackgroundColor = quadrants.TopRightQuadrant.BGColor;
				DrawVictoryPoints(tile.VictoryPoints.ToString());
			}

			if (MapModeHandler.CurrentMapMode.MapMode == MapMode.POLITICAL)
			{ 
				if (origin == null)
					origin = GetOriginForTileRender(tile.Location);

				Console.SetCursorPosition(origin.X + 1, origin.Y + 1);
				DrawFlag(tile.Country);
			}

			// Cover the victory points with the units --- we don't have much room at this zoom
			if (tile.TotalUnitCount > 0)
			{
				if (origin == null)
					origin = GetOriginForTileRender(tile.Location);
 
				Console.SetCursorPosition(origin.X + 3, origin.Y);
				RenderUnitStacksZoom2(tile, origin.X, origin.Y);
			}

			var IsCurrentNode = (TheGame().GameBoard.SelectedNode != null) && (TheGame().GameBoard.SelectedNode.LocationEquals(tile.Location));

			if (IsCurrentNode)
			{
				if (origin == null)
					origin = GetOriginForTileRender(tile.Location);

				DrawSelectedNodeBox(origin.X, origin.Y, ZoomLevel.TWO);
			}
		}

		private void RenderTileZoomLevel1(ITile tile)
		{
			//int baseLeftOrigin = tile.Location.X * zlInfo1.ColumnSpacing;
			//int baseTopOrigin = tile.Location.Y * zlInfo1.RowSpacing;
			//int? leftOrigin = null;
			//int? topOrigin = null;

			//var quadrants = RenderQuadrants(tile.Location, ZoomLevel.ONE, MapModeHandler.CurrentMapMode);

			//if (MapModeHandler.CurrentMapMode.MapMode == MapMode.POLITICAL && tile.TotalUnitCount < 1)
			//{
			//	leftOrigin = (FullMapBoundary.LeftOrigin + Global.Measurements.BOARDBOUNDARYWIDTH + (baseLeftOrigin));
			//	topOrigin = (FullMapBoundary.TopOrigin + Global.Measurements.BOARDBOUNDARYWIDTH + (baseTopOrigin));
			//	Console.SetCursorPosition((int)leftOrigin, (int)topOrigin);
			//	DrawFlag(tile.Country);
			//}

			//var IsCurrentNode = (TheGame().GameBoard.SelectedNode != null) && (TheGame().GameBoard.SelectedNode.LocationEquals(tile.Location));

			//if (IsCurrentNode)
			//{
			//	if (leftOrigin == null)
			//	{
			//		leftOrigin = (FullMapBoundary.LeftOrigin + Global.Measurements.BOARDBOUNDARYWIDTH + (baseLeftOrigin));
			//		topOrigin = (FullMapBoundary.TopOrigin + Global.Measurements.BOARDBOUNDARYWIDTH + (baseTopOrigin));
			//	}

			//	DrawSelectedNodeBox((int)leftOrigin, (int)topOrigin, ZoomLevel.ONE);
			//}

			//if (tile.TotalUnitCount > 0)
			//{
			//	leftOrigin = (FullMapBoundary.LeftOrigin + Global.Measurements.BOARDBOUNDARYWIDTH + (baseLeftOrigin));
			//	topOrigin = (FullMapBoundary.TopOrigin + Global.Measurements.BOARDBOUNDARYWIDTH + (baseTopOrigin));
				
			//	Console.SetCursorPosition((int)leftOrigin, (int)topOrigin);
			//	RenderUnitStacksZoom1(tile, (int)leftOrigin, (int)topOrigin);
			//}				
		}

		private ICoordinate GetOriginForTileRender(ICoordinate tileLocation)
		{
			var leftOrigin = ((tileLocation.X * ZoomHandler.CurrentZoom.ColumnSpacing) -
							  (ZoomHandler.CurrentZoom.CurrentOrigin.X * ZoomHandler.CurrentZoom.ColumnSpacing)) +
							 (MapBoundary.LeftOrigin + Global.Measurements.BOARDBOUNDARYWIDTH);
			var topOrigin = ((tileLocation.Y * ZoomHandler.CurrentZoom.RowSpacing) -
							 (ZoomHandler.CurrentZoom.CurrentOrigin.Y * ZoomHandler.CurrentZoom.RowSpacing)) +
							(MapBoundary.TopOrigin + Global.Measurements.BOARDBOUNDARYWIDTH);

			return new Coordinate(leftOrigin, topOrigin, 0);
		}

		private void DrawSelectedNodeBox(int leftOrigin, int topOrigin, ZoomLevel zoomLevel)
		{
			var zoomLevelInfo = ZoomHandler.GetZoomLevelInfo(zoomLevel);
			
			var color = SetCursorColor();
			var selectedNodeBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE)
				{
					Height = zoomLevelInfo.RowSpacing - 2,
					Width = zoomLevelInfo.ColumnSpacing - 2,
					LeftOrigin = leftOrigin,
					TopOrigin = topOrigin,
					BorderForeColor = color,
					BorderBackColor = color,
					DrawElements = new SingleLineBoxElements()
				};

			selectedNodeBox.Draw();
		}


	// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		private TileQuadrants RenderQuadrants(ICoordinate location, ZoomLevel zoomLevel, MapModeInfo mapMode)
		{
			var quadrants = Quadrants.SingleOrDefault(q => q.NodeLocation.Equals(location));

			var zoomLevelInfo = ZoomHandler.GetZoomLevelInfo(zoomLevel);
			var leftOffset = (zoomLevelInfo.CurrentOrigin.X * ZoomHandler.CurrentZoom.ColumnSpacing);
			var topOffset = (zoomLevelInfo.CurrentOrigin.Y * ZoomHandler.CurrentZoom.RowSpacing);

			Action<QuadrantInfo> qAction = q =>
			{
				lock (quadrants.Quadrants)
				{
					RenderQuadrantBaseGeog(q, zoomLevel, leftOffset, topOffset);

					if (mapMode.MapMode == MapMode.GEOGRAPHICAL || mapMode.MapMode == MapMode.HIGH_CONTRAST)
						q.DisplayDemographics(leftOffset, topOffset, zoomLevel);
				}				
			};

			if (TheGame().IsMultiThreaded)
			{
				Parallel.ForEach(quadrants.Quadrants, qAction);
			}
			else
			{
				foreach (var q in quadrants.Quadrants)
					qAction(q);
			}

			return quadrants;
		}

		private void RenderQuadrantBaseGeog(QuadrantInfo quadrant,
											ZoomLevel zoomLevel,
											int leftOffset, int topOffset)
		{
			var quadrantZoomLevelInfo = quadrant.ZoomLevels[zoomLevel];
			var bgColor = quadrant.BGColor;
			var overlay = (quadrant.IsLand) ? " " : "≈";

			Console.BackgroundColor = bgColor;
			Console.ForegroundColor = ConsoleColor.DarkBlue; //(quadrant.IsLand) ? ConsoleColor.Green : ConsoleColor.DarkBlue;

			// BaseGeog
			for (var i = 0; i < quadrantZoomLevelInfo.DrawHeight; i++)
			{
				Console.SetCursorPosition(quadrantZoomLevelInfo.CurrentOrigin.X - leftOffset, (quadrantZoomLevelInfo.CurrentOrigin.Y + i) - topOffset);
				Console.Write(String.Concat(Enumerable.Repeat(overlay, quadrantZoomLevelInfo.DrawWidth)));
			}
		}


		private void RenderUnitStacksZoom4(ITile tile, int leftOrigin, int topOrigin)
		{
			tile.ResetComponentStackDisplayOrder();
			TheGame().JTSServices.TileService.UpdateTiles(new List<ITile> { tile });
			var stacks = tile.GetAllComponentStacks().Where(cs => (cs.DisplayOrder != 0 && cs.GetAllUnits().Any()));

			var unitStacks = stacks as IUnitStack[] ?? stacks.ToArray();
			if (!unitStacks.Any(us => us.HasVisibleComponents))
				return;

			// center single stack
			if (stacks.Count() < 3) topOrigin += 2;

			foreach (var us in unitStacks)
			{
				if (!us.HasVisibleComponents)
					continue;

				var boxTopOrigin = topOrigin;
				var units = us.GetAllUnits();
				var selected = units.Any(u => TheGame().GameBoard.SelectedUnits.Contains(u));

				var bgColor = (selected)
							? us.Country.Color
							: us.Country.BGColor;
				var foreColor = (selected)
								? us.Country.BGColor
								: us.Country.Color;

				// Draw the units box
				var unitBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE);
				unitBox.Height = 1;
				unitBox.Width = 9;
				unitBox.LeftOrigin = leftOrigin;
				unitBox.TopOrigin = boxTopOrigin;
				unitBox.BackColor = bgColor;
				unitBox.BorderBackColor = bgColor;
				unitBox.BorderForeColor = foreColor;
				unitBox.DrawElements = new SingleLineBoxElements();
				unitBox.Border = true;
				unitBox.DropShadowColor = Global.Colors.UnitMarkerDropshadowColor;
				unitBox.DropShadow = true;

				unitBox.ClearAndRedraw();

				// No visible units
				if (!us.HasVisibleComponents) continue;

				Console.SetCursorPosition(leftOrigin + 1, boxTopOrigin + 1);
				// The unit to render
				var unit = us.GetFirstVisibleUnit();				
				unit.Render((int)ZoomLevel.FOUR);
				Console.CursorLeft = leftOrigin + 7;
				us.Render();

				topOrigin += 2;
			}

		}

		private void RenderUnitStacksZoom3(ITile tile, int leftOrigin, int topOrigin)
		{
			tile.ResetComponentStackDisplayOrder();
			TheGame().JTSServices.TileService.UpdateTiles(new List<ITile> { tile });
			var stacks = tile.GetAllComponentStacks().Where(cs => (cs.DisplayOrder != 0 && cs.HasVisibleComponents));

			var unitStacks = stacks as IUnitStack[] ?? stacks.ToArray();
			if (!unitStacks.Any(us => us.HasVisibleComponents))
				return;

			// center single stack
			if (stacks.Count() < 3) topOrigin += 2;

			foreach (var us in unitStacks)
			{
				if (!us.HasVisibleComponents)
					continue;

				var boxTopOrigin = topOrigin;

				// The unit to render
				var unit = us.GetFirstVisibleUnit();

				var selected = TheGame().GameBoard.SelectedUnits.Contains(unit);

				var bgColor = (selected)
							? us.Country.Color
							: us.Country.BGColor;
				var foreColor = (selected)
								? us.Country.BGColor
								: us.Country.Color;

				// Draw the units box
				var unitBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE);
				unitBox.Height = 0;
				unitBox.Width = 7;
				unitBox.LeftOrigin = leftOrigin;
				unitBox.TopOrigin = boxTopOrigin;
				unitBox.BackColor = bgColor;
				unitBox.BorderBackColor = bgColor;
				unitBox.BorderForeColor = foreColor;
				unitBox.DrawElements = new SingleLineBoxElements();
				unitBox.Border = true;
				unitBox.DropShadowColor = Global.Colors.UnitMarkerDropshadowColor;
				unitBox.DropShadow = true;

				unitBox.ClearAndRedraw();

				Console.SetCursorPosition(leftOrigin + 1, boxTopOrigin);
			
				unit.Render((int)ZoomLevel.THREE);
				Console.CursorLeft = leftOrigin + 5;
				us.Render();

				topOrigin += 1;
			}
		}

		private void RenderUnitStacksZoom2(ITile tile, int leftOrigin, int topOrigin)
		{
			tile.ResetComponentStackDisplayOrder();
			TheGame().JTSServices.TileService.UpdateTiles(new List<ITile> { tile });
			var stacks = tile.GetAllComponentStacks().Where(cs => (cs.DisplayOrder != 0 && cs.HasVisibleComponents));

			var unitStacks = stacks as IUnitStack[] ?? stacks.ToArray();
			if (!unitStacks.Any(us => us.HasVisibleComponents))
				return;

			// center single stack
			if (stacks.Count() < 3) topOrigin += 1;

			foreach (var us in unitStacks)
			{
				if (!us.HasVisibleComponents)
					continue;

				var boxTopOrigin = topOrigin;

				// The unit to render
				var unit = us.GetFirstVisibleUnit();

				var selected = TheGame().GameBoard.SelectedUnits.Contains(unit);

				var bgColor = (selected)
							? us.Country.Color
							: us.Country.BGColor;
				var foreColor = (selected)
								? us.Country.BGColor
								: us.Country.Color;

				// Draw the units box
				var unitBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE);
				unitBox.Height = 0;
				unitBox.Width = 6;
				unitBox.LeftOrigin = leftOrigin;
				unitBox.TopOrigin = boxTopOrigin;
				unitBox.BackColor = bgColor;
				unitBox.BorderBackColor = bgColor;
				unitBox.BorderForeColor = foreColor;
				unitBox.DrawElements = new SingleLineBoxElements();
				unitBox.Border = false;

				unitBox.Clear();
				unitBox.Draw();

				Console.SetCursorPosition(leftOrigin + 1, boxTopOrigin);
			
				unit.Render((int)ZoomLevel.TWO);
				Console.CursorLeft = leftOrigin + 3;
				us.Render();

				topOrigin += 1;
			}
		}

		private void RenderUnitStacksZoom1(ITile tile, int leftOrigin, int topOrigin)
		{
			tile.ResetComponentStackDisplayOrder();
			TheGame().JTSServices.TileService.UpdateTiles(new List<ITile> { tile });
			var stacks = tile.GetAllComponentStacks().Where(cs => (cs.DisplayOrder != 0 && 
																	cs.HasVisibleComponents && 
																	cs.Country.Equals(tile.Country)));

			var us = stacks.SingleOrDefault();
			if (us == null || !us.HasVisibleComponents)
				return;

			var boxTopOrigin = topOrigin;

			// The unit to render
			var unit = us.GetFirstVisibleUnit();

			var selected = TheGame().GameBoard.SelectedUnits.Contains(unit);

			var bgColor = (selected)
						? us.Country.Color
						: us.Country.BGColor;
			var foreColor = (selected)
							? us.Country.BGColor
							: us.Country.Color;

			// Draw the units box
			var unitBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE);
			unitBox.Height = 0;
			unitBox.Width = 2;
			unitBox.LeftOrigin = leftOrigin;
			unitBox.TopOrigin = boxTopOrigin;
			unitBox.BackColor = bgColor;
			unitBox.BorderBackColor = bgColor;
			unitBox.BorderForeColor = foreColor;
			unitBox.Border = false;

			unitBox.Clear();
			unitBox.Draw();

			Console.SetCursorPosition(leftOrigin, boxTopOrigin);			
			unit.Render((int)ZoomLevel.ONE);
		}
		
		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		public void ResetTileDemographics(IEnumerable<INode> nodes)
		{
			SetNodeQuadrants(nodes);
		}

		public void DisplayNodeInfo(INode node)
		{
			var mapHeight = ZoomHandler.CurrentZoom.DrawHeight * ZoomHandler.CurrentZoom.RowSpacing;
			var origin = GetOriginAdjacentToNode(node);

			var nodeInfoBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.PRESS_ANY_KEY)
				{
					Width = 40,
					LeftOrigin = origin.X,
					BorderForeColor = Global.Colors.MapInfoBoxBorderForeColor,
					BorderBackColor = Global.Colors.MapInfoBoxBorderBackColor,
					PromptColor = ConsoleColor.Yellow,
					DrawElements = new SingleLineBoxElements(),
					BackColor = Global.Colors.MapInfoBoxBackColor,
					ForeColor = Global.Colors.MapInfoBoxForeColor,
					EraseColor = Global.Colors.MainMapBackColor,
					DropShadow = true,
					DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
					Caption = "{0}".F(node.DefaultTile.Name),
					CaptionColor = ConsoleColor.Yellow,
					Text = node.DefaultTile.TextInfo()
				};
			
			// Keeps us off the bottom of the screen
			if (origin.Y + nodeInfoBox.Height > mapHeight)
				origin.Y -= nodeInfoBox.Height - (ZoomHandler.CurrentZoom.RowSpacing - 2);

			nodeInfoBox.TopOrigin = origin.Y;
			nodeInfoBox.Erased += On_InfoBoxErased;
			nodeInfoBox.ClearAndRedraw();
		}

		public void DisplayUnitInfo(IUnit unit)
		{
			var mapHeight = ZoomHandler.CurrentZoom.DrawHeight * ZoomHandler.CurrentZoom.RowSpacing;
			var origin = GetOriginAdjacentToNode(unit.GetNode());

			var unitInfoBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.PRESS_ANY_KEY)
				{
					Width = 40,
					LeftOrigin = origin.X,
					BorderForeColor = Global.Colors.MapInfoBoxBorderForeColor,
					BorderBackColor = Global.Colors.MapInfoBoxBorderBackColor,
					PromptColor = ConsoleColor.Yellow,
					DrawElements = new SingleLineBoxElements(),
					BackColor = Global.Colors.MapInfoBoxBackColor,
					ForeColor = Global.Colors.MapInfoBoxForeColor,
					EraseColor = Global.Colors.MainMapBackColor,
					DropShadow = true,
					DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
					Caption = "{0}".F(unit.Name),
					CaptionColor = ConsoleColor.Yellow,
					Text = unit.TextInfo()
				};

			// Keeps us off the bottom of the screen
			if (origin.Y + unitInfoBox.Height > mapHeight)
				origin.Y -= unitInfoBox.Height - (ZoomHandler.CurrentZoom.RowSpacing - 2);

			unitInfoBox.TopOrigin = origin.Y;
			unitInfoBox.Erased += On_InfoBoxErased;
			unitInfoBox.ClearAndRedraw();
		}

		public void RenderUnit(IUnit unit, int zoomLevel)
		{
			switch (zoomLevel)
			{
				case 1:
					RenderUnitZoom1(unit);
					break;
				case 2:
					RenderUnitZoom2(unit);
					break;
				case 3:
					RenderUnitZoom3(unit);
					break;
				case 4:
					RenderUnitZoom4(unit);
					break;
			}
		}
		
		private void RenderUnitZoom1(IUnit unit)
		{
			SetUnitRenderColors(unit);
			Console.Write("{0}", (unit.UnitInfo.UnitType.TextDisplayZ3));
		}

		private void RenderUnitZoom2(IUnit unit)
		{
			SetUnitRenderColors(unit);
			Console.Write("{0} ", unit.UnitInfo.UnitType.TextDisplayZ3);
		}

		private void RenderUnitZoom3(IUnit unit)
		{
			SetUnitRenderColors(unit);
			Console.Write(" {0} ", (unit.UnitInfo.UnitType.TextDisplayZ3));
		}

		private void RenderUnitZoom4(IUnit unit)
		{
			SetUnitRenderColors(unit);
			Console.Write("{0}", unit.UnitInfo.UnitGroupType.TextDisplayZ4);
			Console.Write(" {0}{1} ", 
				(!String.IsNullOrWhiteSpace(unit.UnitInfo.UnitClass.TextDisplayZ4)) ? unit.UnitInfo.UnitClass.TextDisplayZ4 : " ",
							unit.UnitInfo.UnitType.TextDisplayZ4);
		}

		public int RenderUnitStackInfo(IUnitStack stack)
		{
			var allVisibleUnits = stack.GetAllUnits().Where(u => !u.IsHiddenFromEnemy());
			var sb = new StringBuilder(" {0} ".F(allVisibleUnits.Count()));

			Console.ForegroundColor = stack.Country.Color;
			Console.BackgroundColor = stack.Country.BGColor;

			if (stack.GetAllUnits().Any(u => TheGame().GameBoard.SelectedUnits.Contains(u)))
			{
				Console.BackgroundColor = stack.Country.Color;
				Console.ForegroundColor = stack.Country.BGColor;
			}

			Console.Write(sb.ToString());

			return sb.Length;
		}


#region DrawUtilities

		private void SetUnitRenderColors(IUnit unit)
		{
			var IsSelectedUnit =	(TheGame().GameBoard.SelectedUnits != null && TheGame().GameBoard.SelectedUnits.Any()) && 
									(TheGame().GameBoard.SelectedUnits.Any(m => m.Equals(unit)));
			// Render the units box			
			Console.ForegroundColor = unit.Country.Color;
			Console.BackgroundColor = unit.Country.BGColor;
			
			if (IsSelectedUnit)
			{
				Console.BackgroundColor = unit.Country.Color;
				Console.ForegroundColor = unit.Country.BGColor;
			}
		}

		/// <summary>
		/// Places the control to the right of the space
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		private ICoordinate GetOriginAdjacentToNode(INode node)
		{
			var r = new Coordinate(0, 0, 0);

			var leftOrigin = ((node.Location.X * ZoomHandler.CurrentZoom.ColumnSpacing) -
								(ZoomHandler.CurrentZoom.CurrentOrigin.X * ZoomHandler.CurrentZoom.ColumnSpacing)) +
								(Global.Measurements.WESTMARGIN + Global.Measurements.BOARDBOUNDARYWIDTH + ZoomHandler.CurrentZoom.ColumnSpacing);
			var topOrigin = ((node.Location.Y * ZoomHandler.CurrentZoom.RowSpacing) -
								(ZoomHandler.CurrentZoom.CurrentOrigin.Y * ZoomHandler.CurrentZoom.RowSpacing)) +
							(Global.Measurements.NORTHMARGIN + Global.Measurements.BOARDBOUNDARYWIDTH);

			r.X = leftOrigin;
			r.Y = topOrigin;

			return r;
		}

		/// <summary>
		/// Places the control to the left of the space rather than the right
		/// </summary>
		/// <param name="node"></param>
		/// <param name="width"></param>
		/// <returns></returns>
		private ICoordinate GetOriginAdjacentToNode(INode node, int width)
		{
			var r = GetOriginAdjacentToNode(node);

			// Places it on the left side of the node rather than the right
			r.X = (r.X - ZoomHandler.CurrentZoom.ColumnSpacing - (width + 1)); 

			return r;
		}

		private void DrawFlag(ICountry country)
		{
			Console.BackgroundColor = country.FlagBGColor;
			Console.ForegroundColor = country.FlagColorA;
			Console.Write(country.FlagDisplayTextA);
			Console.ForegroundColor = country.FlagColorB;
			Console.Write(country.FlagDisplayTextB);
			Console.CursorLeft -= 2;
		}

        private void DrawVictoryPoints(string victoryPoints)
        {
			Console.BackgroundColor = Global.Colors.VictoryPointBackColor;
            Console.ForegroundColor = Global.Colors.VictoryPointForeColor;
            Console.Write(" {0}".F(victoryPoints));
        }
		
		private ConsoleColor SetCursorColor()
		{
			ConsoleColor retVal;
			
			//var route = TheGame().GameBoard.CurrentRoute;
			var IsMovementAvailableNode = TheGame().GameBoard.AvailableMovementNodes.Any(n => n.Equals(TheGame().GameBoard.SelectedNode));
			var unitSelected = TheGame().GameBoard.SelectedUnits.Any();

			if (IsMovementAvailableNode && unitSelected)
			{
				retVal = Global.Colors.NodeAvailableForMoveColor;
			}
			else
			{
				retVal = (unitSelected) ? Global.Colors.NodeNotAvailableForMoveColor : Global.Colors.NodeHighlightedColor;
			}

			return retVal;
		}

		
		// Pre-Set Draw Elements for each cursor space in a tile

		private void SetNodeQuadrants(IEnumerable<INode> nodes)
		{
			var quadrants = new List<TileQuadrants>();			
			
			// Load Quadrants for all nodes
			Action<INode> nodeAction = n =>
			{
				lock (nodes)
				lock (quadrants)
				{
					quadrants.Add(GetTileQuadrantsForNode(n));
				}
			};

			if (TheGame().IsMultiThreaded)
			{
				Parallel.ForEach(nodes, nodeAction);
			}
			else
			{
				nodes.ToList().ForEach(nodeAction);
			}

			Quadrants = quadrants.AsEnumerable();
		}

		private TileQuadrants GetTileQuadrantsForNode(INode node)
		{
			var tile = node.DefaultTile;
					
			var q = BuildQuadrantDrawInfoForNode(node);
			q.TopLeftQuadrant.SetQuadrantDemographics(tile);
			q.TopRightQuadrant.SetQuadrantDemographics(tile);
			q.BottomLeftQuadrant.SetQuadrantDemographics(tile);
			q.BottomRightQuadrant.SetQuadrantDemographics(tile);

			return q;
		}

		private TileQuadrants BuildQuadrantDrawInfoForNode(INode node)
		{
			var tile = node.DefaultTile;

			var leftOffsetZ4				= (ZoomHandler.GetZoomLevelInfo(ZoomLevel.FOUR).ColumnSpacing / 2);
			var topOffsetZ4					= (ZoomHandler.GetZoomLevelInfo(ZoomLevel.FOUR).RowSpacing / 2);			
			var leftOffsetZ3				= (ZoomHandler.GetZoomLevelInfo(ZoomLevel.THREE).ColumnSpacing / 2);
			var topOffsetZ3					= (ZoomHandler.GetZoomLevelInfo(ZoomLevel.THREE).RowSpacing / 2);
			var leftOffsetZ2				= (ZoomHandler.GetZoomLevelInfo(ZoomLevel.TWO).ColumnSpacing / 2);
			var topOffsetZ2					= (ZoomHandler.GetZoomLevelInfo(ZoomLevel.TWO).RowSpacing / 2);
			var leftOffsetZ1				= (ZoomHandler.GetZoomLevelInfo(ZoomLevel.ONE).ColumnSpacing / 2);
			var topOffsetZ1					= (ZoomHandler.GetZoomLevelInfo(ZoomLevel.ONE).RowSpacing / 2);

			var leftOriginZ4				= ((node.Location.X * ZoomHandler.GetZoomLevelInfo(ZoomLevel.FOUR).ColumnSpacing) + (MapBoundary.LeftOrigin + Global.Measurements.BOARDBOUNDARYWIDTH));
			var topOriginZ4					= ((node.Location.Y * ZoomHandler.GetZoomLevelInfo(ZoomLevel.FOUR).RowSpacing) + (MapBoundary.TopOrigin + Global.Measurements.BOARDBOUNDARYWIDTH));
			var leftOriginZ3				= ((node.Location.X * ZoomHandler.GetZoomLevelInfo(ZoomLevel.THREE).ColumnSpacing) + (MapBoundary.LeftOrigin + Global.Measurements.BOARDBOUNDARYWIDTH));
			var topOriginZ3					= ((node.Location.Y * ZoomHandler.GetZoomLevelInfo(ZoomLevel.THREE).RowSpacing) + (MapBoundary.TopOrigin + Global.Measurements.BOARDBOUNDARYWIDTH));
			var leftOriginZ2				= ((node.Location.X * ZoomHandler.GetZoomLevelInfo(ZoomLevel.TWO).ColumnSpacing) + (MapBoundary.LeftOrigin + Global.Measurements.BOARDBOUNDARYWIDTH));
			var topOriginZ2					= ((node.Location.Y * ZoomHandler.GetZoomLevelInfo(ZoomLevel.TWO).RowSpacing) + (MapBoundary.TopOrigin + Global.Measurements.BOARDBOUNDARYWIDTH));
			//var leftOriginZ1				= ((node.Location.X * ZoomHandler.GetZoomLevelInfo(ZoomLevel.ONE).ColumnSpacing) + (FullMapBoundary.LeftOrigin + Global.Measurements.BOARDBOUNDARYWIDTH));
			//var topOriginZ1					= ((node.Location.Y * ZoomHandler.GetZoomLevelInfo(ZoomLevel.ONE).RowSpacing) + (FullMapBoundary.TopOrigin + Global.Measurements.BOARDBOUNDARYWIDTH));

			var quadrants = new TileQuadrants(node.Location, 
												leftOffsetZ1, 
												topOffsetZ1, 
												leftOffsetZ2, 
												topOffsetZ2, 
												leftOffsetZ3, 
												topOffsetZ3,
												leftOffsetZ4,
												topOffsetZ4);

			quadrants.TopLeftQuadrant.ZoomLevels[ZoomLevel.FOUR].CurrentOrigin.X = leftOriginZ4;
			quadrants.TopLeftQuadrant.ZoomLevels[ZoomLevel.FOUR].CurrentOrigin.Y = topOriginZ4;
			quadrants.TopRightQuadrant.ZoomLevels[ZoomLevel.FOUR].CurrentOrigin.X = leftOriginZ4 + leftOffsetZ4;
			quadrants.TopRightQuadrant.ZoomLevels[ZoomLevel.FOUR].CurrentOrigin.Y = topOriginZ4;
			quadrants.BottomLeftQuadrant.ZoomLevels[ZoomLevel.FOUR].CurrentOrigin.X = leftOriginZ4;
			quadrants.BottomLeftQuadrant.ZoomLevels[ZoomLevel.FOUR].CurrentOrigin.Y = topOriginZ4 + topOffsetZ4;
			quadrants.BottomRightQuadrant.ZoomLevels[ZoomLevel.FOUR].CurrentOrigin.X = leftOriginZ4 + leftOffsetZ4;
			quadrants.BottomRightQuadrant.ZoomLevels[ZoomLevel.FOUR].CurrentOrigin.Y = topOriginZ4 + topOffsetZ4;
			
			quadrants.TopLeftQuadrant.ZoomLevels[ZoomLevel.THREE].CurrentOrigin.X = leftOriginZ3;
			quadrants.TopLeftQuadrant.ZoomLevels[ZoomLevel.THREE].CurrentOrigin.Y = topOriginZ3;
			quadrants.TopRightQuadrant.ZoomLevels[ZoomLevel.THREE].CurrentOrigin.X = leftOriginZ3 + leftOffsetZ3;
			quadrants.TopRightQuadrant.ZoomLevels[ZoomLevel.THREE].CurrentOrigin.Y = topOriginZ3;
			quadrants.BottomLeftQuadrant.ZoomLevels[ZoomLevel.THREE].CurrentOrigin.X = leftOriginZ3;
			quadrants.BottomLeftQuadrant.ZoomLevels[ZoomLevel.THREE].CurrentOrigin.Y = topOriginZ3 + topOffsetZ3;
			quadrants.BottomRightQuadrant.ZoomLevels[ZoomLevel.THREE].CurrentOrigin.X = leftOriginZ3 + leftOffsetZ3;
			quadrants.BottomRightQuadrant.ZoomLevels[ZoomLevel.THREE].CurrentOrigin.Y = topOriginZ3 + topOffsetZ3;

			quadrants.TopLeftQuadrant.ZoomLevels[ZoomLevel.TWO].CurrentOrigin.X = leftOriginZ2;
			quadrants.TopLeftQuadrant.ZoomLevels[ZoomLevel.TWO].CurrentOrigin.Y = topOriginZ2;
			quadrants.TopRightQuadrant.ZoomLevels[ZoomLevel.TWO].CurrentOrigin.X = leftOriginZ2 + leftOffsetZ2;
			quadrants.TopRightQuadrant.ZoomLevels[ZoomLevel.TWO].CurrentOrigin.Y = topOriginZ2;
			quadrants.BottomLeftQuadrant.ZoomLevels[ZoomLevel.TWO].CurrentOrigin.X = leftOriginZ2;
			quadrants.BottomLeftQuadrant.ZoomLevels[ZoomLevel.TWO].CurrentOrigin.Y = topOriginZ2 + topOffsetZ2;
			quadrants.BottomRightQuadrant.ZoomLevels[ZoomLevel.TWO].CurrentOrigin.X = leftOriginZ2 + leftOffsetZ2;
			quadrants.BottomRightQuadrant.ZoomLevels[ZoomLevel.TWO].CurrentOrigin.Y = topOriginZ2 + topOffsetZ2;

			//quadrants.TopLeftQuadrant.ZoomLevels[ZoomLevel.ONE].CurrentOrigin.X = leftOriginZ1;
			//quadrants.TopLeftQuadrant.ZoomLevels[ZoomLevel.ONE].CurrentOrigin.Y = topOriginZ1;
			//quadrants.TopRightQuadrant.ZoomLevels[ZoomLevel.ONE].CurrentOrigin.X = leftOriginZ1 + leftOffsetZ1;
			//quadrants.TopRightQuadrant.ZoomLevels[ZoomLevel.ONE].CurrentOrigin.Y = topOriginZ1;
			//quadrants.BottomLeftQuadrant.ZoomLevels[ZoomLevel.ONE].CurrentOrigin.X = leftOriginZ1;
			//quadrants.BottomLeftQuadrant.ZoomLevels[ZoomLevel.ONE].CurrentOrigin.Y = topOriginZ1 + topOffsetZ1;
			//quadrants.BottomRightQuadrant.ZoomLevels[ZoomLevel.ONE].CurrentOrigin.X = leftOriginZ1 + leftOffsetZ1;
			//quadrants.BottomRightQuadrant.ZoomLevels[ZoomLevel.ONE].CurrentOrigin.Y = topOriginZ1 + topOffsetZ1;


			if (tile.ConsoleRenderHelper.HasShoreLineNorth)
			{
				quadrants.TopLeftQuadrant.BGColor = quadrants.TopRightQuadrant.BGColor = Global.Colors.LandBGColor;
				quadrants.TopLeftQuadrant.IsLand = quadrants.TopRightQuadrant.IsLand = true;
 				quadrants.BottomLeftQuadrant.BGColor = quadrants.BottomRightQuadrant.BGColor = Global.Colors.WaterBGColor;
				quadrants.BottomLeftQuadrant.IsLand = quadrants.BottomRightQuadrant.IsLand = false;
				return quadrants;
			}

			if (tile.ConsoleRenderHelper.HasShoreLineSouth)
			{
				quadrants.TopLeftQuadrant.BGColor = quadrants.TopRightQuadrant.BGColor = Global.Colors.WaterBGColor;
				quadrants.TopLeftQuadrant.IsLand = quadrants.TopRightQuadrant.IsLand = false;
 				quadrants.BottomLeftQuadrant.BGColor = quadrants.BottomRightQuadrant.BGColor = Global.Colors.LandBGColor;
				quadrants.BottomLeftQuadrant.IsLand = quadrants.BottomRightQuadrant.IsLand = true;
				return quadrants;
			}
			if (tile.ConsoleRenderHelper.HasShoreLineWest)
			{
				quadrants.TopLeftQuadrant.BGColor = quadrants.BottomLeftQuadrant.BGColor = Global.Colors.LandBGColor;
				quadrants.TopLeftQuadrant.IsLand = quadrants.BottomLeftQuadrant.IsLand = true;
 				quadrants.TopRightQuadrant.BGColor = quadrants.BottomRightQuadrant.BGColor = Global.Colors.WaterBGColor;
				quadrants.TopRightQuadrant.IsLand = quadrants.BottomRightQuadrant.IsLand = false;
				return quadrants;
			}
			if (tile.ConsoleRenderHelper.HasShoreLineEast)
			{
				quadrants.TopLeftQuadrant.BGColor = quadrants.BottomLeftQuadrant.BGColor = Global.Colors.WaterBGColor;
				quadrants.TopLeftQuadrant.IsLand = quadrants.BottomLeftQuadrant.IsLand = false;
 				quadrants.TopRightQuadrant.BGColor = quadrants.BottomRightQuadrant.BGColor = Global.Colors.LandBGColor;
				quadrants.TopRightQuadrant.IsLand = quadrants.BottomRightQuadrant.IsLand = true;
				return quadrants;
			}
			if (tile.ConsoleRenderHelper.HasShoreLineSouthWest)
			{
				quadrants.TopLeftQuadrant.BGColor = quadrants.TopRightQuadrant.BGColor = quadrants.BottomRightQuadrant.BGColor = Global.Colors.WaterBGColor;
				quadrants.TopLeftQuadrant.IsLand = quadrants.TopRightQuadrant.IsLand = quadrants.BottomRightQuadrant.IsLand = false;
 				quadrants.BottomLeftQuadrant.BGColor = Global.Colors.LandBGColor;
				quadrants.BottomLeftQuadrant.IsLand =  true;
				return quadrants;
			}
			if (tile.ConsoleRenderHelper.HasShoreLineSouthEast)
			{
				quadrants.TopLeftQuadrant.BGColor = quadrants.TopRightQuadrant.BGColor = quadrants.BottomLeftQuadrant.BGColor = Global.Colors.WaterBGColor;
				quadrants.TopLeftQuadrant.IsLand = quadrants.TopRightQuadrant.IsLand = quadrants.BottomLeftQuadrant.IsLand = false;
 				quadrants.BottomRightQuadrant.BGColor = Global.Colors.LandBGColor;
				quadrants.BottomRightQuadrant.IsLand =  true;
				return quadrants;
			}
			if (tile.ConsoleRenderHelper.HasShoreLineNorthWest)
			{
				quadrants.BottomRightQuadrant.BGColor = quadrants.TopRightQuadrant.BGColor = quadrants.BottomLeftQuadrant.BGColor = Global.Colors.WaterBGColor;
				quadrants.BottomRightQuadrant.IsLand = quadrants.TopRightQuadrant.IsLand = quadrants.BottomLeftQuadrant.IsLand = false;
 				quadrants.TopLeftQuadrant.BGColor = Global.Colors.LandBGColor;
				quadrants.TopLeftQuadrant.IsLand =  true;
				return quadrants;
			}
			if (tile.ConsoleRenderHelper.HasShoreLineNorthEast)
			{
				quadrants.BottomRightQuadrant.BGColor = quadrants.TopLeftQuadrant.BGColor = quadrants.BottomLeftQuadrant.BGColor = Global.Colors.WaterBGColor;
				quadrants.BottomRightQuadrant.IsLand = quadrants.TopLeftQuadrant.IsLand = quadrants.BottomLeftQuadrant.IsLand = false;
 				quadrants.TopRightQuadrant.BGColor = Global.Colors.LandBGColor;
				quadrants.TopRightQuadrant.IsLand =  true;
				return quadrants;
			}
			if (tile.ConsoleRenderHelper.IsRiver || tile.ConsoleRenderHelper.IsSea || tile.ConsoleRenderHelper.HasLakes)
			{
 				quadrants.TopLeftQuadrant.BGColor = quadrants.TopRightQuadrant.BGColor = quadrants.BottomLeftQuadrant.BGColor = quadrants.BottomRightQuadrant.BGColor = Global.Colors.WaterBGColor;
				quadrants.TopLeftQuadrant.IsLand = quadrants.TopRightQuadrant.IsLand = quadrants.BottomLeftQuadrant.IsLand = quadrants.BottomRightQuadrant.IsLand  = false;
				return quadrants;
			}
			if (!tile.ConsoleRenderHelper.IsRiver)
			{
				quadrants.TopLeftQuadrant.BGColor = quadrants.TopRightQuadrant.BGColor = quadrants.BottomLeftQuadrant.BGColor = quadrants.BottomRightQuadrant.BGColor = Global.Colors.LandBGColor;
				quadrants.TopLeftQuadrant.IsLand = quadrants.TopRightQuadrant.IsLand = quadrants.BottomLeftQuadrant.IsLand = quadrants.BottomRightQuadrant.IsLand  = true;
				return quadrants;
			}

			return quadrants;
		}
		

#endregion

#region Event Handlers
		
		public void On_InfoBoxErased(object sender, EventArgs e)
		{
			// Re-draw affected areas
			RenderBoardFrame();
			var control = sender as ConsoleControl;
			control.RedrawControlAffectedNodes();
		}

#endregion

	}
}
