using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTacticalSim.API.Component;
using JTacticalSim.Utility;
using JTacticalSim.API;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.Component.World;

namespace JTacticalSim.ConsoleApp
{
	public class TileQuadrants
	{
		private List<QuadrantInfo> _quadrants { set; get; }
		public ICoordinate NodeLocation { get; private set; }

		public QuadrantInfo TopLeftQuadrant { get { return _quadrants.SingleOrDefault(q => q.Location == QuadrantLocation.TOPLEFT); } }
		public QuadrantInfo BottomLeftQuadrant { get { return _quadrants.SingleOrDefault(q => q.Location == QuadrantLocation.BOTTOMLEFT); } }
		public QuadrantInfo TopRightQuadrant { get { return _quadrants.SingleOrDefault(q => q.Location == QuadrantLocation.TOPRIGHT); } }
		public QuadrantInfo BottomRightQuadrant { get { return _quadrants.SingleOrDefault(q => q.Location == QuadrantLocation.BOTTOMRIGHT); } }

		public IEnumerable<QuadrantInfo> Quadrants { get { return _quadrants.AsEnumerable(); } }

		public TileQuadrants(ICoordinate nodeLocation, 
							int widthZoom1, 
							int heightZoom1, 
							int widthZoom2, 
							int heightZoom2, 
							int widthZoom3, 
							int heightZoom3,
							int widthZoom4,
							int heightZoom4)
		{
			NodeLocation = nodeLocation;

			_quadrants = new List<QuadrantInfo>();

			_quadrants.Add(new QuadrantInfo
							{
								Location = QuadrantLocation.TOPLEFT,
								ZoomLevels = new Dictionary<ZoomLevel, QuadrantZoomInfo>
								{
									{ZoomLevel.ONE, new QuadrantZoomInfo
														{
															 Level = ZoomLevel.ONE,
															 DrawHeight = heightZoom1,
															 DrawWidth = widthZoom1
														}},
									{ZoomLevel.TWO, new QuadrantZoomInfo
														{
															 Level = ZoomLevel.TWO,
															 DrawHeight = heightZoom2,
															 DrawWidth = widthZoom2
														}},
									{ZoomLevel.THREE, new QuadrantZoomInfo
														{
															 Level = ZoomLevel.THREE,
															 DrawHeight = heightZoom3,
															 DrawWidth = widthZoom3
														}},
									{ZoomLevel.FOUR, new QuadrantZoomInfo
														{
															 Level = ZoomLevel.FOUR,
															 DrawHeight = heightZoom4,
															 DrawWidth = widthZoom4
														}},
								},
							});
			_quadrants.Add(new QuadrantInfo
							{
								Location = QuadrantLocation.TOPRIGHT,
								ZoomLevels = new Dictionary<ZoomLevel, QuadrantZoomInfo>
								{
									{ZoomLevel.ONE, new QuadrantZoomInfo
														{
															 Level = ZoomLevel.ONE,
															 DrawHeight = heightZoom1,
															 DrawWidth = widthZoom1
														}},
									{ZoomLevel.TWO, new QuadrantZoomInfo
														{
															 Level = ZoomLevel.TWO,
															 DrawHeight = heightZoom2,
															 DrawWidth = widthZoom2
														}},
									{ZoomLevel.THREE, new QuadrantZoomInfo
														{
															 Level = ZoomLevel.THREE,
															 DrawHeight = heightZoom3,
															 DrawWidth = widthZoom3
														}},
									{ZoomLevel.FOUR, new QuadrantZoomInfo
														{
															 Level = ZoomLevel.FOUR,
															 DrawHeight = heightZoom4,
															 DrawWidth = widthZoom4
														}},
								},
							});
			_quadrants.Add(new QuadrantInfo
							{
								Location = QuadrantLocation.BOTTOMLEFT,
								ZoomLevels = new Dictionary<ZoomLevel, QuadrantZoomInfo>
								{
									{ZoomLevel.ONE, new QuadrantZoomInfo
														{
															 Level = ZoomLevel.ONE,
															 DrawHeight = heightZoom1,
															 DrawWidth = widthZoom1
														}},
									{ZoomLevel.TWO, new QuadrantZoomInfo
														{
															 Level = ZoomLevel.TWO,
															 DrawHeight = heightZoom2,
															 DrawWidth = widthZoom2
														}},
									{ZoomLevel.THREE, new QuadrantZoomInfo
														{
															 Level = ZoomLevel.THREE,
															 DrawHeight = heightZoom3,
															 DrawWidth = widthZoom3
														}},
									{ZoomLevel.FOUR, new QuadrantZoomInfo
														{
															 Level = ZoomLevel.FOUR,
															 DrawHeight = heightZoom4,
															 DrawWidth = widthZoom4
														}},
								},
							});
			_quadrants.Add(new QuadrantInfo
							{
								Location = QuadrantLocation.BOTTOMRIGHT,
								ZoomLevels = new Dictionary<ZoomLevel, QuadrantZoomInfo>
								{
									{ZoomLevel.ONE, new QuadrantZoomInfo
														{
															 Level = ZoomLevel.ONE,
															 DrawHeight = heightZoom1,
															 DrawWidth = widthZoom1
														}},
									{ZoomLevel.TWO, new QuadrantZoomInfo
														{
															 Level = ZoomLevel.TWO,
															 DrawHeight = heightZoom2,
															 DrawWidth = widthZoom2
														}},
									{ZoomLevel.THREE, new QuadrantZoomInfo
														{
															 Level = ZoomLevel.THREE,
															 DrawHeight = heightZoom3,
															 DrawWidth = widthZoom3
														}},
									{ZoomLevel.FOUR, new QuadrantZoomInfo
														{
															 Level = ZoomLevel.FOUR,
															 DrawHeight = heightZoom4,
															 DrawWidth = widthZoom4
														}},
								},
							});
			
		}

		public IEnumerable<QuadrantInfo> GetLandBasedQuadrants()
		{
			return _quadrants.Where(qi => qi.IsLand);
		}

		public IEnumerable<QuadrantInfo> GetWaterBasedQuadrants()
		{
			return _quadrants.Where(qi => !qi.IsLand);
		}
	}

	public class QuadrantInfo : BaseGameObject
	{
		public QuadrantLocation Location { get; set; }
		public ConsoleColor BGColor { get; set; }
		public ConsoleColor HighlightColor { get; set; }
		public bool IsLand { get; set; }
		public Dictionary<ZoomLevel, QuadrantZoomInfo> ZoomLevels { get; set; }

		public QuadrantInfo(ConsoleColor bgColor, bool isLand)
			: this()
		{
			BGColor = bgColor;
			IsLand = isLand;
		}

		public QuadrantInfo() 
			: base(GameObjectType.COMPONENT)
		{ 
			ZoomLevels = new Dictionary<ZoomLevel, QuadrantZoomInfo>();
		}

		/// <summary>
		/// Renders the quadrant display elements to the screen
		/// </summary>
		/// <param name="leftOffset"></param>
		/// <param name="topOffset"></param>
		/// <param name="zoomLevel"></param>
		public void DisplayDemographics(int leftOffset, int topOffset, ZoomLevel zoomLevel)
		{
			var chars = ZoomLevels[zoomLevel].DisplayChars;

			Action<QuadrantCharInfo> charAction = q =>
				{
					Console.BackgroundColor = q.BGColor;					
					Console.SetCursorPosition(q.LeftOrigin - leftOffset, q.TopOrigin - topOffset);

					if (TheGame().MapModeHandler.CurrentMapMode.MapMode != MapMode.HIGH_CONTRAST)
						Console.ForegroundColor = (Console.ForegroundColor == q.TextColor1) ? q.TextColor2 : q.TextColor1;

					Console.Write(q.DisplayText);
				};

			foreach(var c in chars)
			{
				charAction(c);
			}
		}

		public void SetQuadrantDemographics(ITile tile)
		{
			// Order of precedence - Least to most durable - new elements override old
			SetUrban(tile);
			SetFlora(tile);
			SetGeographyTerrain(tile);
			SetSmallTowns(tile);
			SetCreeksSmallRivers(tile);
			SetBridgesDams(tile);
			SetRoadsRRTracks(tile);
			SetMilitaryBases(tile);
			SetAirports(tile);
			SetLargeMountains(tile);
			SetNuclearWasteland(tile);
		}

#region Pre-Fill Display Elements

	// Generic demographic rendering
		/// <summary>
		/// Fills the quadrant with a specific demographic element
		/// </summary>
		/// <param name="demo"></param>
		/// <param name="displayColor1"></param>
		/// <param name="displayColor2"></param>
		/// <param name="bgColor"></param>
		/// <param name="h_frequency"></param>
		/// <param name="v_frequency"></param>
		/// <param name="displayOnce"></param>
		/// <param name="zoomLevels"></param>
		public void FillDemographicDisplay(IDemographic demo, 
											ConsoleColor displayColor1, 
											ConsoleColor displayColor2,
											ConsoleColor bgColor,
											int h_frequency,
											int v_frequency,
											bool displayOnce,
											List<ZoomLevel> zoomLevels)
		{
			if (demo == null) return;
			var currentIndex = 0;

			foreach (var kvp in ZoomLevels)
			{
				if (!zoomLevels.Contains(kvp.Key)) continue;

				var tempList = new List<QuadrantCharInfo>();
				var renderText = ParseDemoDisplayString(demo.DemographicClass.GetTextDisplayForZoom(kvp.Key));

				// For the most zoomed out level, we'll throw the demographics into all spaces
				if (kvp.Value.Level == ZoomLevel.ONE || (h_frequency == 0 && v_frequency == 0))
				{
					for (var j = kvp.Value.CurrentOrigin.Y; j < (kvp.Value.CurrentOrigin.Y + kvp.Value.DrawHeight); j++)
					{
						for (var i = kvp.Value.CurrentOrigin.X; i < (kvp.Value.CurrentOrigin.X + kvp.Value.DrawWidth); i++)
						{
							// Scroll through characters
							var displayText = (renderText.Count() > 0)
											? NextCharacter(renderText, currentIndex)
											: renderText[0];
							currentIndex = Array.IndexOf(renderText, displayText);

							// Overwrite the existing
							var existing =
								kvp.Value.DisplayChars.Where(dc => dc.LeftOrigin == i && dc.TopOrigin == j).ToArray();
							if (existing.Any())
							{
								kvp.Value.DisplayChars.Remove(existing.SingleOrDefault());
							}

							tempList.Add(new QuadrantCharInfo
								{
									DisplayText =  displayText,
									BGColor = bgColor,
									TextColor1 = displayColor1,
									TextColor2 = displayColor2,
									TopOrigin = j,
									LeftOrigin = i
								});
						}
					}

					kvp.Value.DisplayChars = kvp.Value.DisplayChars.Concat(tempList).ToList();

					continue;
				}

				// All other levels

				var row = 5;

				for (var j = kvp.Value.CurrentOrigin.Y; j < (kvp.Value.CurrentOrigin.Y + kvp.Value.DrawHeight); j += v_frequency)
				{
					var oddRow = (row % 2 == 1);

					//if (oddRow && renderText.Length > 2)
					//	displayColor1 = renderText.Substring(0, renderText.Length - 2);

					for (var	i = (oddRow ? kvp.Value.CurrentOrigin.X : kvp.Value.CurrentOrigin.X + 1);
								i < (oddRow ? (kvp.Value.CurrentOrigin.X + kvp.Value.DrawWidth - 2) : kvp.Value.CurrentOrigin.X + kvp.Value.DrawWidth);
								i += h_frequency)
					{
						// Scroll characters
						var displayText = (renderText.Count() > 0)
										? NextCharacter(renderText, currentIndex)
										: renderText[0];
						currentIndex = Array.IndexOf(renderText, displayText);

						// Overwrite the existing
						var existing =
							kvp.Value.DisplayChars.Where(dc => dc.LeftOrigin == i && dc.TopOrigin == j).ToArray();
						if (existing.Any())
						{
							kvp.Value.DisplayChars.Remove(existing.SingleOrDefault());
						}

						tempList.Add(new QuadrantCharInfo
							{
								DisplayText = displayText,
								BGColor = bgColor,
								TextColor1 = displayColor1,
								TextColor2 = displayColor2,
								TopOrigin = j,
								LeftOrigin = i
							});
					}

					row++;
				}

				kvp.Value.DisplayChars = kvp.Value.DisplayChars.Concat(tempList).ToList();
			}
		}

		/// <summary>
		/// Creates a square centered demographic at a location
		/// </summary>
		/// <param name="demo"></param>
		/// <param name="displayColor1"></param>
		/// <param name="displayColor2"></param>
		/// <param name="bgColor"></param>
		/// <param name="bgColorBorder"></param>
		/// <param name="zoomLevels"></param>
		private void FillTileCenteredDemographicDisplay(IDemographic demo,
														ConsoleColor displayColor1,
														ConsoleColor displayColor2,
														ConsoleColor bgColor,
														ConsoleColor bgColorBorder,
														List<ZoomLevel> zoomLevels)
		{
			if (demo == null) return;

			foreach (var kvp in ZoomLevels)
			{
				if (!zoomLevels.Contains(kvp.Key)) continue;

				var zoomInfo = kvp.Value;
				var renderTextElements = ParseDemoDisplayString(demo.DemographicClass.GetTextDisplayForZoom(kvp.Key));
				var renderText = renderTextElements[0];
				var renderTextBorder = (renderTextElements.Length > 1) ? renderTextElements[1] : " ";
				var tempList = new List<QuadrantCharInfo>();
				var drawLength = (zoomInfo.DrawHeight - 1);

				switch (Location)
				{
					case QuadrantLocation.TOPLEFT:
						{
							var vOffset = drawLength;
							var hOffset = drawLength;
							for (var i = 0; i < drawLength; i++)
							{
								for (var j = 0; j < drawLength; j++)
								{
									var topOrigin = (zoomInfo.CurrentOrigin.Y + zoomInfo.DrawHeight) - vOffset;
									var leftOrigin = ((zoomInfo.CurrentOrigin.X + zoomInfo.DrawWidth) - hOffset) + j;
									var isBorder = (i == 0 || j == 0);

									DrawTileCenteredDemographic(zoomInfo, 
																leftOrigin, topOrigin, 
																renderText, renderTextBorder, 
																isBorder, 
																tempList, 
																(isBorder) ? displayColor2 :displayColor1, 
																displayColor2, 
																bgColorBorder, 
																bgColor);
								}
								vOffset--;
							}

							break;
						}
					case QuadrantLocation.TOPRIGHT:
						{
							var vOffset = drawLength;
							for (var i = 0; i < drawLength; i++)
							{
								for (var j = 0; j < drawLength; j++)
								{
									var topOrigin = (zoomInfo.CurrentOrigin.Y + zoomInfo.DrawHeight) - vOffset;
									var leftOrigin = zoomInfo.CurrentOrigin.X + j;
									var isBorder = (i == 0 || j == drawLength - 1);

									DrawTileCenteredDemographic(zoomInfo, 
																leftOrigin, topOrigin, 
																renderText, renderTextBorder, 
																isBorder, 
																tempList, 
																(isBorder) ? displayColor2 :displayColor1, 
																displayColor2, 
																bgColorBorder, 
																bgColor);
								}
								vOffset--;
							}

							break;
						}
					case QuadrantLocation.BOTTOMLEFT:
						{
							var hOffset = drawLength;
							for (var i = 0; i < drawLength; i++)
							{
								for (var j = 0; j < drawLength; j++)
								{
									var topOrigin = zoomInfo.CurrentOrigin.Y + i;
									var leftOrigin = ((zoomInfo.CurrentOrigin.X + zoomInfo.DrawWidth) - hOffset) + j;
									var isBorder = (i == drawLength - 1 || j == 0);

									DrawTileCenteredDemographic(zoomInfo, 
																leftOrigin, topOrigin, 
																renderText, renderTextBorder, 
																isBorder, 
																tempList, 
																(isBorder) ? displayColor2 :displayColor1, 
																displayColor2, 
																bgColorBorder, 
																bgColor);
								}
	
							}

							break;
						}
					case QuadrantLocation.BOTTOMRIGHT:
						{
							for (var i = 0; i < drawLength; i++)
							{
								for (var j = 0; j < drawLength; j++)
								{
									var topOrigin = zoomInfo.CurrentOrigin.Y + i;
									var leftOrigin = zoomInfo.CurrentOrigin.X + j;
									var isBorder = (i == drawLength - 1 || j == drawLength - 1);

									DrawTileCenteredDemographic(zoomInfo, 
																leftOrigin, topOrigin, 
																renderText, renderTextBorder, 
																isBorder, 
																tempList, 
																(isBorder) ? displayColor2 :displayColor1, 
																displayColor2, 
																bgColorBorder, 
																bgColor);
								}
							}

							break;
						}

				}

				zoomInfo.DisplayChars = zoomInfo.DisplayChars.Concat(tempList).ToList();
			}
		}

	// Specific demographic rendering

		/// <summary>
		/// Creates Transportation related draw elements on the quadrant based on orientation
		/// </summary>
		/// <param name="demo"></param>
		/// <param name="displayColor1"></param>
		/// <param name="displayColor2"></param>
		/// <param name="displayColorBG"></param>
		/// <param name="orientation"></param>
		/// <param name="zoomLevels"></param>
		public void FillTransportationDemographicDisplay(IDemographic demo, 
															ConsoleColor displayColor1,
															ConsoleColor displayColor2,
															ConsoleColor displayColorBG,
															Direction orientation,
															List<ZoomLevel> zoomLevels)
		{

			if (demo == null) return;
			

			foreach (var kvp in ZoomLevels)
			{
				if (!zoomLevels.Contains(kvp.Key)) continue;

				var renderText = demo.DemographicClass.GetTextDisplayForZoom(kvp.Key);

				// We have multiple characters for orientations
				if (renderText.Contains(","))
				{
					var renderTexts = ParseDemoDisplayString(renderText);
					renderText = (orientation == Direction.EAST || orientation == Direction.WEST) ? renderTexts[0] : renderTexts[1];
				}

				switch (orientation)
				{
					case Direction.NORTH:
						{
							if (Location != QuadrantLocation.TOPLEFT)
								return;

							DrawNorthSouthOrientedDemographics(renderText, 
																displayColor1, 
																displayColor2, 
																displayColorBG, 
																kvp.Value, (kvp.Value.DrawWidth - 1));
							continue;
						}
					case Direction.SOUTH:
						{
							if (Location != QuadrantLocation.BOTTOMLEFT)
								return;

							DrawNorthSouthOrientedDemographics(renderText, 
																displayColor1, 
																displayColor2, 
																displayColorBG, 
																kvp.Value, (kvp.Value.DrawWidth - 1));
							continue;
						}
					case Direction.EAST:
						{
							if (Location != QuadrantLocation.TOPRIGHT)
								return;

							DrawEastWestOrientedDemographics(renderText, 
																displayColor1, 
																displayColor2, 
																displayColorBG, 
																kvp.Value, (kvp.Value.DrawHeight - 1));
							continue;
						}
					case Direction.WEST:
						{
							if (Location != QuadrantLocation.TOPLEFT)
								return;

							DrawEastWestOrientedDemographics(renderText, 
																displayColor1, 
																displayColor2, 
																displayColorBG, 
																kvp.Value, (kvp.Value.DrawHeight - 1));
							continue;
						}
				}
			}
		}

		/// <summary>
		/// Creates a military base centered demographic at a location
		/// </summary>
		/// <param name="demo"></param>
		/// <param name="displayColor1"></param>
		/// <param name="displayColor2"></param>
		/// <param name="bgColor"></param>
		/// <param name="bgColorBorder"></param>
		/// <param name="zoomLevels"></param>
		private void FillMilitaryBaseDemographicDisplay(IDemographic demo,
														ConsoleColor displayColor1,
														ConsoleColor displayColor2,
														ConsoleColor bgColor,
														ConsoleColor bgColorBorder,
														ConsoleColor foreColorBorder,
														List<ZoomLevel> zoomLevels)
		{
			if (demo == null) return;

			foreach (var kvp in ZoomLevels)
			{
				if (!zoomLevels.Contains(kvp.Key)) continue;

				var zoomInfo = kvp.Value;
				var renderTextElements = ParseDemoDisplayString(demo.DemographicClass.GetTextDisplayForZoom(kvp.Key));
				var renderTankLeft = renderTextElements[0];
				var renderTankRight = renderTextElements[1];
				var renderTextBorder = renderTextElements[2];
				var tempList = new List<QuadrantCharInfo>();
				var drawLength = zoomInfo.DrawHeight - 1;


				switch (Location)
				{
					case QuadrantLocation.TOPLEFT:
						{
							var vOffset = drawLength;
							var hOffset = drawLength;
							for (var i = 0; i < drawLength; i++)
							{
								for (var j = 0; j < drawLength; j++)
								{
									var topOrigin = (zoomInfo.CurrentOrigin.Y + zoomInfo.DrawHeight) - vOffset;
									var leftOrigin = ((zoomInfo.CurrentOrigin.X + zoomInfo.DrawWidth) - hOffset) + j;
									var isBorder = (i == 0 || j == 0);

									var renderText = " ";
									if (i == 2 && j == 1) renderText = renderTankLeft;
									if (i == 2 && j == 2) renderText = renderTankRight;

									DrawMilitaryBaseDemographic(zoomInfo, 
																leftOrigin, topOrigin, 
																renderText, renderTextBorder, 
																isBorder, 
																tempList, 
																(isBorder) ? displayColor2 :displayColor1, 
																displayColor2, 
																bgColorBorder, 
																bgColor,
																foreColorBorder);
								}
								vOffset--;
							}

							break;
						}
					case QuadrantLocation.TOPRIGHT:
						{
							var vOffset = drawLength;
							for (var i = 0; i < drawLength; i++)
							{
								for (var j = 0; j < drawLength; j++)
								{
									var topOrigin = (zoomInfo.CurrentOrigin.Y + zoomInfo.DrawHeight) - vOffset;
									var leftOrigin = zoomInfo.CurrentOrigin.X + j;
									var isBorder = (i == 0 || j == drawLength - 1);

									DrawMilitaryBaseDemographic(zoomInfo, 
																leftOrigin, topOrigin, 
																" ", renderTextBorder, 
																isBorder, 
																tempList, 
																(isBorder) ? displayColor2 :displayColor1, 
																displayColor2, 
																bgColorBorder, 
																bgColor,
																foreColorBorder);
								}
								vOffset--;
							}

							break;
						}
					case QuadrantLocation.BOTTOMLEFT:
						{
							var hOffset = drawLength;
							for (var i = 0; i < drawLength; i++)
							{
								for (var j = 0; j < drawLength; j++)
								{
									var topOrigin = zoomInfo.CurrentOrigin.Y + i;
									var leftOrigin = ((zoomInfo.CurrentOrigin.X + zoomInfo.DrawWidth) - hOffset) + j;
									var isBorder = (i == drawLength - 1 || j == 0);

									DrawMilitaryBaseDemographic(zoomInfo, 
																leftOrigin, topOrigin, 
																" ", renderTextBorder,
																isBorder, 
																tempList, 
																(isBorder) ? displayColor2 :displayColor1, 
																displayColor2, 
																bgColorBorder, 
																bgColor,
																foreColorBorder);
								}
	
							}

							break;
						}
					case QuadrantLocation.BOTTOMRIGHT:
						{
							for (var i = 0; i < drawLength; i++)
							{
								for (var j = 0; j < drawLength; j++)
								{
									var topOrigin = zoomInfo.CurrentOrigin.Y + i;
									var leftOrigin = zoomInfo.CurrentOrigin.X + j;
									var isBorder = (i == drawLength - 1 || j == drawLength - 1);

									var renderText = " ";
									if (i == 0 && j == 0) renderText = renderTankLeft;
									if (i == 0 && j == 1) renderText = renderTankRight;

									DrawMilitaryBaseDemographic(zoomInfo, 
																leftOrigin, topOrigin, 
																renderText, renderTextBorder, 
																isBorder, 
																tempList, 
																(isBorder) ? displayColor2 :displayColor1, 
																displayColor2, 
																bgColorBorder, 
																bgColor,
																foreColorBorder);
								}
							}

							break;
						}

				}

				zoomInfo.DisplayChars = zoomInfo.DisplayChars.Concat(tempList).ToList();
			}
		}

		/// <summary>
		/// Creates Creek/Small River related draw elements on the quadrant based on orientation
		/// </summary>
		/// <param name="demo"></param>
		/// <param name="displayColor1"></param>
		/// <param name="displayColor2"></param>
		/// <param name="orientation"></param>
		/// <param name="zoomLevels"></param>
		public void FillCreekDemographicDisplay(IDemographic demo, 
												ConsoleColor displayColor1, 
												ConsoleColor displayColor2,
												Direction orientation,
												List<ZoomLevel> zoomLevels)
		{

			if (demo == null) return;			

			foreach (var kvp in ZoomLevels)
			{
				if (!zoomLevels.Contains(kvp.Key)) continue;

				var renderText = demo.DemographicClass.GetTextDisplayForZoom(kvp.Key);

				// We have multiple characters for orientations
				if (renderText.Contains(","))
				{
					var renderTexts = ParseDemoDisplayString(renderText);
					renderText = (orientation == Direction.EAST || orientation == Direction.WEST) ? renderTexts[0] : renderTexts[1];
				}

				switch (orientation)
				{
					case Direction.NORTH:
						{
							if (Location != QuadrantLocation.TOPRIGHT)
								return;

							DrawNorthSouthOrientedDemographics(renderText, 
																displayColor1, 
																displayColor2, 
																Global.Colors.WaterBGColor, 
																kvp.Value, 0);
							continue;
						}
					case Direction.SOUTH:
						{
							if (Location != QuadrantLocation.BOTTOMRIGHT)
								return;

							DrawNorthSouthOrientedDemographics(renderText, 
																displayColor1, 
																displayColor2, 
																Global.Colors.WaterBGColor, 
																kvp.Value, 0);
							continue;
						}
					case Direction.EAST:
						{
							if (Location != QuadrantLocation.BOTTOMRIGHT)
								return;

							DrawEastWestOrientedDemographics(renderText, 
															displayColor1, 
															displayColor2, 
															Global.Colors.WaterBGColor, 
															kvp.Value, 0);
							continue;
						}
					case Direction.WEST:
						{
							if (Location != QuadrantLocation.BOTTOMLEFT)
								return;

							DrawEastWestOrientedDemographics(renderText, 
															displayColor1, 
															displayColor2, 
															Global.Colors.WaterBGColor, 
															kvp.Value, 0);
							continue;
						}
				}
			}
		}
		/// <summary>
		/// Creates draw elements for a round demographic element centered on the tile
		/// </summary>
		/// <param name="demo"></param>
		/// <param name="displayColor1"></param>
		/// <param name="displayColor2"></param>
		/// <param name="zoomLevels"></param>
		private void FillFullRoundDemographicDisplay(IDemographic demo,
													ConsoleColor altBGColor1,
													ConsoleColor altBGColor2,
													ConsoleColor displayColor1, 
													ConsoleColor displayColor2,
													List<ZoomLevel> zoomLevels)
		{
			foreach (var kvp in ZoomLevels)
			{
				if (!zoomLevels.Contains(kvp.Key)) continue;

				var zoomInfo = kvp.Value;
				var tempList = new List<QuadrantCharInfo>();
				var renderText = demo.DemographicClass.GetTextDisplayForZoom(kvp.Key);
				var drawLength = (kvp.Value.Level == ZoomLevel.TWO) ? 2 : 3;

				switch (Location)
				{
					case QuadrantLocation.TOPLEFT:
						{
							var itemsInRow = 1;
							var vOffset = drawLength;
							for (int i = 0; i < drawLength; i++)
							{
								var hOffset = 1;
								for (int r = 0; r < itemsInRow; r++)
								{
									var topOrigin = (zoomInfo.CurrentOrigin.Y + zoomInfo.DrawHeight) - vOffset;
									var leftOrigin = (zoomInfo.CurrentOrigin.X + zoomInfo.DrawWidth) - hOffset;

									// Overwrite the existing
									var existing = 
										kvp.Value.DisplayChars.Where(dc => dc.LeftOrigin == leftOrigin && dc.TopOrigin == topOrigin).ToArray();
									if (existing.Any())
									{
										kvp.Value.DisplayChars.Remove(existing.SingleOrDefault());
									}

									var bgColor = (r == itemsInRow - 1) ? ConsoleColor.DarkGreen : altBGColor1;
									if (r == 0 && i == drawLength - 1) bgColor = altBGColor2;

									tempList.Add(new QuadrantCharInfo
									{
										DisplayText = renderText,
										BGColor = bgColor,
										TextColor1 = (r == itemsInRow - 1) ? displayColor1 : displayColor2,
										TextColor2 = (r == itemsInRow - 1) ? displayColor1 : displayColor2,
										TopOrigin = topOrigin,
										LeftOrigin = leftOrigin
									});

									hOffset++;
								}

								itemsInRow++;
								vOffset--;
							}

							break;
						}
					case QuadrantLocation.TOPRIGHT:
						{
							var itemsInRow = 1;
							var vOffset = drawLength;
							for (int i = 0; i < drawLength; i++)
							{
								for (int r = 0; r < itemsInRow; r++)
								{
									var topOrigin = (zoomInfo.CurrentOrigin.Y + zoomInfo.DrawHeight) - vOffset;
									var leftOrigin = zoomInfo.CurrentOrigin.X + r;

									// Overwrite the existing
									var existing = 
										kvp.Value.DisplayChars.Where(dc => dc.LeftOrigin == leftOrigin && dc.TopOrigin == topOrigin).ToArray();
									if (existing.Any())
									{
										kvp.Value.DisplayChars.Remove(existing.SingleOrDefault());
									}

									var bgColor = (r == itemsInRow - 1) ? ConsoleColor.DarkGreen : altBGColor1;
									if (r == 0 && i == drawLength - 1) bgColor = altBGColor2;

									tempList.Add(new QuadrantCharInfo
									{
										DisplayText = renderText,
										BGColor = bgColor,
										TextColor1 = (r == itemsInRow - 1) ? displayColor1 : displayColor2,
										TextColor2 = (r == itemsInRow - 1) ? displayColor1 : displayColor2,
										TopOrigin = topOrigin,
										LeftOrigin = leftOrigin
									});

								}

								itemsInRow++;
								vOffset--;
							}

							break;
						}
					case QuadrantLocation.BOTTOMLEFT:
						{
							var itemsInRow = 3;
							for (int i = 0; i < drawLength; i++)
							{
								var hOffset = 1;
								for (int r = 0; r < itemsInRow; r++)
								{
									var topOrigin = zoomInfo.CurrentOrigin.Y + i;
									var leftOrigin = (zoomInfo.CurrentOrigin.X + zoomInfo.DrawWidth) - hOffset;

									// Overwrite the existing
									var existing = 
										kvp.Value.DisplayChars.Where(dc => dc.LeftOrigin == leftOrigin && dc.TopOrigin == topOrigin).ToArray();
									if (existing.Any())
									{
										kvp.Value.DisplayChars.Remove(existing.SingleOrDefault());
									}

									var bgColor = (r == itemsInRow - 1) ? ConsoleColor.DarkGreen : altBGColor1;
									if (r == 0 && i == 0) bgColor = altBGColor2;

									tempList.Add(new QuadrantCharInfo
									{
										DisplayText = renderText,
										BGColor = bgColor,
										TextColor1 = (r == itemsInRow - 1) ? displayColor1 : displayColor2,
										TextColor2 = (r == itemsInRow - 1) ? displayColor1 : displayColor2,
										TopOrigin = topOrigin,
										LeftOrigin = leftOrigin
									});

									hOffset++;
								}

								itemsInRow--;
							}

							break;
						}
					case QuadrantLocation.BOTTOMRIGHT:
						{
							var itemsInRow = 3;
							for (int i = 0; i < drawLength; i++)
							{
								for (int r = 0; r < itemsInRow; r++)
								{
									var topOrigin = zoomInfo.CurrentOrigin.Y + i;
									var leftOrigin = zoomInfo.CurrentOrigin.X + r;

									// Overwrite the existing
									var existing = 
										kvp.Value.DisplayChars.Where(dc => dc.LeftOrigin == leftOrigin && dc.TopOrigin == topOrigin).ToArray();
									if (existing.Any())
									{
										kvp.Value.DisplayChars.Remove(existing.SingleOrDefault());
									}

									var bgColor = (r == itemsInRow - 1) ? ConsoleColor.DarkGreen : altBGColor1;
									if (r == 0 && i == 0) bgColor = altBGColor2;

									tempList.Add(new QuadrantCharInfo
									{
										DisplayText = renderText,
										BGColor = bgColor,
										TextColor1 = (r == itemsInRow - 1) ? displayColor1 : displayColor2,
										TextColor2 = (r == itemsInRow - 1) ? displayColor1 : displayColor2,
										TopOrigin = topOrigin,
										LeftOrigin = leftOrigin
									});

								}

								itemsInRow--;
							}

							break;
						}

				}

				zoomInfo.DisplayChars = zoomInfo.DisplayChars.Concat(tempList).ToList();
			}
		}
		/// <summary>
		/// Creates draw elements for small towns
		/// </summary>
		/// <param name="demo"></param>
		/// <param name="displayColor1"></param>
		/// <param name="displayColor2"></param>
		/// <param name="orientation"></param>
		/// <param name="zoomLevels"></param>
		private void FillTownDemographicDisplay(IDemographic demo, 
												ConsoleColor displayColor1, 
												ConsoleColor displayColor2,
												List<ZoomLevel> zoomLevels)
		{
			if (demo.Orientation.Count == 1 && demo.Orientation.Single() == Direction.NONE)
				return;

			foreach (var kvp in ZoomLevels)
			{
				if (!zoomLevels.Contains(kvp.Key)) continue;

				var zoomInfo = kvp.Value;
				var renderText = demo.DemographicClass.GetTextDisplayForZoom(kvp.Key);
				var tempList = new List<QuadrantCharInfo>();

				Action action = () =>
					{
						DrawSmallTownDemographic(zoomInfo, renderText, tempList, displayColor1, displayColor2);							
						zoomInfo.DisplayChars = zoomInfo.DisplayChars.Concat(tempList).ToList();
					};

				FillDemographicForQuadrantByOrientation(action, demo.Orientation);

			}
		}

		private void FillAirportDemographicDisplay(	IDemographic demo,
													ConsoleColor displayColor1,
													ConsoleColor displayColor2,
													ConsoleColor displayColorBG,
													List<ZoomLevel> zoomLevels)
		{
			if (demo.Orientation.Count == 1 && demo.Orientation.Single() == Direction.NONE)
				return;

			foreach (var kvp in ZoomLevels)
			{
				if (!zoomLevels.Contains(kvp.Key)) continue;

				var zoomInfo = kvp.Value;
				
				var tempList = new List<QuadrantCharInfo>();

				Action action = () =>
					{
						DrawAirportDemographic(zoomInfo, kvp.Key, demo, tempList, displayColor1, displayColor2, displayColorBG);							
						zoomInfo.DisplayChars = zoomInfo.DisplayChars.Concat(tempList).ToList();
					};

				FillDemographicForQuadrantByOrientation(action, demo.Orientation);
			}
		}



		private void SetGeographyTerrain(ITile tile)
		{
			if (!IsLand) return;

			if (tile.ConsoleRenderHelper.HasMountains)
			{
				var demo = tile.AllGeography.SingleOrDefault(d => d.IsDemographicClass("Mountains"));
				FillDemographicDisplay(demo, 
										ConsoleColor.Gray, 
										ConsoleColor.DarkGray,
										BGColor,
										1, 1, false,
										new List<ZoomLevel>{ZoomLevel.ONE, ZoomLevel.TWO, ZoomLevel.THREE, ZoomLevel.FOUR});				
			}
			if (tile.ConsoleRenderHelper.HasHills)
			{
				var demo = tile.AllGeography.SingleOrDefault(d => d.IsDemographicClass("Hills"));
				FillDemographicDisplay(demo, 
										ConsoleColor.DarkGray, 
										ConsoleColor.DarkGray,
										BGColor,
										2, 1, false,
										new List<ZoomLevel>{ZoomLevel.ONE, ZoomLevel.TWO, ZoomLevel.THREE, ZoomLevel.FOUR});	
			}
			if (tile.ConsoleRenderHelper.HasLakes)
			{
				var demo = tile.AllGeography.SingleOrDefault(d => d.IsDemographicClass("Lake"));
				FillDemographicDisplay(demo, 
										ConsoleColor.Blue,
										ConsoleColor.Blue, 
										BGColor,
										0, 0, false,
										new List<ZoomLevel>{ZoomLevel.ONE, ZoomLevel.TWO, ZoomLevel.THREE, ZoomLevel.FOUR});	
			}
			
		}

		private void SetLargeMountains(ITile tile)
		{
			if (tile.ConsoleRenderHelper.HasMountain)
			{
				var demo_mountain = tile.AllGeography.SingleOrDefault(d => d.IsDemographicClass("Mountain"));
				FillFullRoundDemographicDisplay(demo_mountain,
												ConsoleColor.Gray,
												ConsoleColor.White,
												ConsoleColor.DarkGray, 
												ConsoleColor.White,
												new List<ZoomLevel>{ZoomLevel.THREE, ZoomLevel.FOUR});
			}
		}

		private void SetCreeksSmallRivers(ITile tile)
		{
			if (!IsLand) return;

			if (tile.ConsoleRenderHelper.HasCreeks)
			{
				var demo_creek = tile.AllGeography.SingleOrDefault(d => d.IsDemographicClass("Creek"));
				SetCreekGeographyDemographic(demo_creek);
			} 
		}

		private void SetRoadsRRTracks(ITile tile)
		{
			if (tile.ConsoleRenderHelper.HasRoad)
			{
				var demos = tile.Infrastructure.Where(d => d.IsDemographicClass("Road")).ToList();
				demos.ForEach(demo => SetRoadInfrastructureDemographic(demo, ConsoleColor.White, ConsoleColor.Black, ConsoleColor.Black));
			}

		}

		private void SetSmallTowns(ITile tile)
		{
			if (tile.ConsoleRenderHelper.HasTown)
			{
				var demo = tile.Infrastructure.SingleOrDefault(d => d.IsDemographicClass("Town"));
				FillTownDemographicDisplay(demo, 
											ConsoleColor.Black, 
											ConsoleColor.DarkRed, 
											new List<ZoomLevel>{ZoomLevel.THREE, ZoomLevel.FOUR});

			}
		}

		private void SetFlora(ITile tile)
		{
			if (tile.ConsoleRenderHelper.HasMarsh && IsLand)
			{
				var demo = tile.Flora.SingleOrDefault(d => d.IsDemographicClass("Marsh"));
				FillDemographicDisplay(demo, 
										ConsoleColor.DarkYellow, 
										ConsoleColor.DarkGray,
										BGColor,
										1, 1, false, 
										new List<ZoomLevel>{ZoomLevel.ONE, ZoomLevel.TWO, ZoomLevel.THREE, ZoomLevel.FOUR});				
			}
			if (tile.ConsoleRenderHelper.HasForests && IsLand)
			{
				var demo = tile.Flora.SingleOrDefault(d => d.IsDemographicClass("Forested"));
				FillDemographicDisplay(demo, 
										ConsoleColor.Green,
 										ConsoleColor.DarkYellow,
										BGColor,
										1, 1, false, 
										new List<ZoomLevel>{ZoomLevel.ONE, ZoomLevel.TWO, ZoomLevel.THREE, ZoomLevel.FOUR});
			}
			if (tile.ConsoleRenderHelper.HasTrees && IsLand)
			{
				var demo = tile.Flora.SingleOrDefault(d => d.IsDemographicClass("Trees"));
				FillDemographicDisplay(demo, 
										ConsoleColor.DarkYellow, 
										ConsoleColor.Green,
										BGColor,
										3, 2, false, 
										new List<ZoomLevel>{ZoomLevel.ONE, ZoomLevel.TWO, ZoomLevel.THREE, ZoomLevel.FOUR});
			}
			if (tile.ConsoleRenderHelper.HasWoodlands && IsLand)
			{
				var demo = tile.Flora.SingleOrDefault(d => d.IsDemographicClass("Woodland"));
				FillDemographicDisplay(demo, 
										ConsoleColor.Green, 
										ConsoleColor.DarkYellow,
										BGColor,
										2, 2, false, 
										new List<ZoomLevel>{ZoomLevel.ONE, ZoomLevel.TWO, ZoomLevel.THREE, ZoomLevel.FOUR});
			}
		}

		private void SetBridgesDams(ITile tile)
		{
			if (tile.ConsoleRenderHelper.HasDam)
			{
				var demos = tile.Infrastructure.Where(d => d.IsDemographicClass("Dam")).ToList();

				demos.ForEach(demo => 
							{
								foreach (var o in demo.Orientation)
								{
									FillTransportationDemographicDisplay(demo, 
																		ConsoleColor.DarkGray, 
																		ConsoleColor.DarkGray,
																		ConsoleColor.Gray,
																		o, 
																		new List<ZoomLevel>{ZoomLevel.ONE, ZoomLevel.TWO, ZoomLevel.THREE, ZoomLevel.FOUR});
							}
			});	
			}
			if (tile.ConsoleRenderHelper.HasBridge)
			{
				var demos = tile.Infrastructure.Where(d => d.IsDemographicClass("Bridge")).ToList();
				
				demos.ForEach(demo => 
								{
									foreach (var o in demo.Orientation)
									{
										FillTransportationDemographicDisplay(demo, 
																			ConsoleColor.DarkGray, 
																			ConsoleColor.DarkGray,
																			ConsoleColor.Black,
																			o, 
																			new List<ZoomLevel>{ZoomLevel.ONE, ZoomLevel.TWO, ZoomLevel.THREE, ZoomLevel.FOUR});
									}
								});
			}
		}

		private void SetUrban(ITile tile)
		{
			if (tile.ConsoleRenderHelper.HasCities && IsLand)
			{
				var demo = tile.Infrastructure.SingleOrDefault(d => d.IsDemographicClass("Urban"));
				FillDemographicDisplay(demo, 
										ConsoleColor.Black, 
										ConsoleColor.DarkRed, 
										ConsoleColor.DarkGray,
										1, 1, false, 
										new List<ZoomLevel>{ZoomLevel.ONE, ZoomLevel.TWO, ZoomLevel.THREE, ZoomLevel.FOUR});
			}
			if (tile.ConsoleRenderHelper.HasIndustrial && IsLand)
			{
				var demo_industrial = tile.Infrastructure.SingleOrDefault(d => d.IsDemographicClass("Industrial"));
				FillDemographicDisplay(demo_industrial, 
										ConsoleColor.Gray, 
										ConsoleColor.Black, 
										ConsoleColor.DarkGray,
										1, 1, false,
				                        new List<ZoomLevel> {ZoomLevel.ONE, ZoomLevel.TWO, ZoomLevel.THREE, ZoomLevel.FOUR});
			}			
		}

		private void SetNuclearWasteland(ITile tile)
		{
			if (tile.ConsoleRenderHelper.IsNuclearWasteland && IsLand)
			{
				var demo = tile.AllGeography.SingleOrDefault(d => d.IsDemographicClass("NuclearWasteland"));

				FillFullRoundDemographicDisplay(demo,
												ConsoleColor.Gray,
												ConsoleColor.Black,
												ConsoleColor.DarkRed, 
												ConsoleColor.Black,
												new List<ZoomLevel>{ZoomLevel.TWO, ZoomLevel.THREE, ZoomLevel.FOUR});
			}		
		}

		private void SetMilitaryBases(ITile tile)
		{
			if (tile.ConsoleRenderHelper.HasMilitaryBase)
			{ 
				var demo = tile.Infrastructure.SingleOrDefault(d => d.IsDemographicClass("MilitaryBase"));
				FillMilitaryBaseDemographicDisplay(demo, 
													ConsoleColor.Black, 
													ConsoleColor.Black, 
													ConsoleColor.DarkYellow, 
													ConsoleColor.Gray, 
													ConsoleColor.DarkGray,
													new List<ZoomLevel>{ZoomLevel.TWO, ZoomLevel.THREE, ZoomLevel.FOUR});

			}	
		
			if (tile.ConsoleRenderHelper.HasCommandPost)
			{ 
				var demo = tile.Infrastructure.SingleOrDefault(d => d.IsDemographicClass("CommandPost"));
				FillTileCenteredDemographicDisplay(demo, 
													ConsoleColor.Black, 
													ConsoleColor.Black, 
													ConsoleColor.DarkYellow, 
													ConsoleColor.DarkGreen,
													new List<ZoomLevel>{ZoomLevel.TWO, ZoomLevel.THREE, ZoomLevel.FOUR});

			}
		}

		private void SetAirports(ITile tile)
		{
			if (tile.ConsoleRenderHelper.HasAirports)
			{
				var demo = tile.Infrastructure.SingleOrDefault(d => d.IsDemographicClass("Airport"));
				FillAirportDemographicDisplay(demo,
												ConsoleColor.Black, 
												ConsoleColor.Black,
												ConsoleColor.DarkGray,
												new List<ZoomLevel>{ZoomLevel.THREE, ZoomLevel.FOUR});
			}
		}


		private void SetRoadInfrastructureDemographic(IDemographic demo, ConsoleColor color1, ConsoleColor color2, ConsoleColor colorBG)
		{
			if (demo.Orientation.Count == 1 && demo.Orientation.Single() == Direction.NONE)
				return;

			foreach (var o in demo.Orientation)
			{
				FillTransportationDemographicDisplay(demo, 
													color1, 
													color2,
													colorBG,
													o, 
													new List<ZoomLevel>{ZoomLevel.TWO, ZoomLevel.THREE, ZoomLevel.FOUR});
			}
		}

		private void SetCreekGeographyDemographic(IDemographic demo)
		{
			if (demo.Orientation.Count == 1 && demo.Orientation.Single() == Direction.NONE)
				return;

			foreach (var o in demo.Orientation)
			{
				FillCreekDemographicDisplay(demo, 
											Global.Colors.WaterBGColor, 
											Global.Colors.WaterBGColor, 
											o, 
											new List<ZoomLevel>{ZoomLevel.ONE, ZoomLevel.TWO, ZoomLevel.THREE, ZoomLevel.FOUR});
			}
		}


#endregion

#region internal helpers

		/// <summary>
		/// Fills demographic display for specified orientation if the fill action is a single action
		/// </summary>
		/// <param name="action"></param>
		/// <param name="orientations"></param>
		private void FillDemographicForQuadrantByOrientation(Action action, IEnumerable<Direction> orientations)
		{
			foreach (var orientation in orientations)
				{
					switch (orientation)
					{
						case Direction.NORTHEAST:
							{
								if (Location != QuadrantLocation.TOPRIGHT)
									break;
								action();
								continue;
							}
						case Direction.SOUTHEAST:
							{
								if (Location != QuadrantLocation.BOTTOMRIGHT) 
									break;
								action();
								continue;
							}
						case Direction.SOUTHWEST:
							{
								if (Location != QuadrantLocation.BOTTOMLEFT) 
									break;
								action();
								continue;
							}
						case Direction.NORTHWEST:
							{
								if (Location != QuadrantLocation.TOPLEFT) 
									break;
								action();
								continue;
							}
					}
				}
		}

		/// <summary>
		/// Fills the full background of a quadrant
		/// </summary>
		/// <param name="zoomLevels"></param>
		/// <param name="bgColor"></param>
		/// <param name="foreColor"></param>
		private void FillBackgroundColor(List<ZoomLevel> zoomLevels, ConsoleColor bgColor, ConsoleColor foreColor)
		{
			foreach (var kvp in ZoomLevels)
			{
				if (!zoomLevels.Contains(kvp.Key)) continue;

				var tempList = new List<QuadrantCharInfo>();
				
				for (var j = kvp.Value.CurrentOrigin.Y; j < (kvp.Value.CurrentOrigin.Y + kvp.Value.DrawHeight); j++)
				{
					for (var i = kvp.Value.CurrentOrigin.X; i < kvp.Value.CurrentOrigin.X + kvp.Value.DrawWidth; i++)
					{
						// Overwrite the existing
						var existing =
							kvp.Value.DisplayChars.Where(dc => dc.LeftOrigin == i && dc.TopOrigin == j).ToArray();
						if (existing.Any())
						{
							kvp.Value.DisplayChars.Remove(existing.SingleOrDefault());
						}

						tempList.Add(new QuadrantCharInfo
							{
								DisplayText = "▒",
								BGColor = bgColor,
								TextColor1 = foreColor,
								TextColor2 = foreColor,
								TopOrigin = j,
								LeftOrigin = i
							});
					}
				}

				kvp.Value.DisplayChars = kvp.Value.DisplayChars.Concat(tempList).ToList();
				continue;
			}
		}

		private void DrawTileCenteredDemographic(	QuadrantZoomInfo zoomInfo, 
													int leftOrigin, 
													int topOrigin,
													string renderText, 
													string renderTextBorder,
													bool isBorder,
													ICollection<QuadrantCharInfo> tempList,
													ConsoleColor displayColor1,
													ConsoleColor displayColor2,
													ConsoleColor bgColorBorder,
													ConsoleColor bgColor)
		{
			// Overwrite the existing
			var existing = 
				zoomInfo.DisplayChars.Where(dc => dc.LeftOrigin == leftOrigin && dc.TopOrigin == topOrigin).ToArray();
			if (existing.Any())
			{
				zoomInfo.DisplayChars.Remove(existing.SingleOrDefault());
			}

			tempList.Add(new QuadrantCharInfo
			{
				DisplayText =  isBorder ? renderTextBorder : renderText,
				BGColor = isBorder ? bgColorBorder : bgColor,
				TextColor1 = displayColor1,
				TextColor2 = displayColor2,
				TopOrigin = topOrigin,
				LeftOrigin = leftOrigin
			});
		}

		private void DrawSmallTownDemographic(	QuadrantZoomInfo zoomInfo, 
												string renderText,
												List<QuadrantCharInfo> tempList,
												ConsoleColor displayColor1,
												ConsoleColor displayColor2)
		{
			var vOffset = 2;
			var hOffset = 2;

			for (var i = 0; i < 2; i++)
			{
				var topOrigin = zoomInfo.CurrentOrigin.Y + vOffset;
				var leftOrigin = zoomInfo.CurrentOrigin.X + hOffset + i;

				// Overwrite the existing
				var existing = 
					zoomInfo.DisplayChars.Where(dc => dc.LeftOrigin == leftOrigin && dc.TopOrigin == topOrigin).ToArray();
				if (existing.Any())
				{
					zoomInfo.DisplayChars.Remove(existing.SingleOrDefault());
				}

				tempList.Add(new QuadrantCharInfo
				{
					DisplayText = renderText,
					BGColor = ConsoleColor.DarkGray,
					TextColor1 = displayColor1,
					TextColor2 = displayColor2,
					TopOrigin = topOrigin,
					LeftOrigin = leftOrigin
				});
			}
		}

		private void DrawAirportDemographic(QuadrantZoomInfo zoomInfo,
											ZoomLevel zoomLevel,
		                                    IDemographic demo,
		                                    List<QuadrantCharInfo> tempList,
		                                    ConsoleColor displayColor1,
		                                    ConsoleColor displayColor2,
											ConsoleColor displayColorBG)
		{
			var renderTextElements = ParseDemoDisplayString(demo.DemographicClass.GetTextDisplayForZoom(zoomLevel));
			var renderTextVertical = renderTextElements[0];
			var renderTextHorizontal = renderTextElements[1];
			var renderTextIntersection = renderTextElements[2];
			var offset = 0;

			var drawLength = 4;

			for (var i = 0; i < 2; i++)
			{
				for (var j = 0; j < drawLength; j++)
				{
					var topOrigin = zoomInfo.CurrentOrigin.Y + offset + i;
					var leftOrigin = zoomInfo.CurrentOrigin.X + offset + j;

					var isIntersection = (j == 1);
					var isHorizontal = (!isIntersection);

					var renderText = " ";
					if (isHorizontal) renderText = renderTextHorizontal;
					if (isIntersection) renderText = renderTextIntersection;

					// Overwrite the existing
					var existing = 
						zoomInfo.DisplayChars.Where(dc => dc.LeftOrigin == leftOrigin && dc.TopOrigin == topOrigin).ToArray();
					if (existing.Any())
					{
						zoomInfo.DisplayChars.Remove(existing.SingleOrDefault());
					}

					tempList.Add(new QuadrantCharInfo
					{
						DisplayText = renderText,
						BGColor = displayColorBG,
						TextColor1 = displayColor1,
						TextColor2 = displayColor2,
						TopOrigin = topOrigin,
						LeftOrigin = leftOrigin
					});
				}
			}

		}

		private void DrawMilitaryBaseDemographic(	QuadrantZoomInfo zoomInfo, 
													int leftOrigin, int topOrigin,
													string renderText, string renderTextBorder,
													bool isBorder,
													ICollection<QuadrantCharInfo> tempList,
													ConsoleColor displayColor1,
													ConsoleColor displayColor2,
													ConsoleColor bgColorBorder,
													ConsoleColor bgColor,
													ConsoleColor foreColorBorder)
		{
			// Overwrite the existing
			var existing = 
				zoomInfo.DisplayChars.Where(dc => dc.LeftOrigin == leftOrigin && dc.TopOrigin == topOrigin).ToArray();
			if (existing.Any())
			{
				zoomInfo.DisplayChars.Remove(existing.SingleOrDefault());
			}

			tempList.Add(new QuadrantCharInfo
			{
				DisplayText =  isBorder ? renderTextBorder : renderText,
				BGColor = isBorder ? bgColorBorder : bgColor,
				TextColor1 = isBorder ? foreColorBorder : displayColor1,
				TextColor2 = isBorder ? foreColorBorder : displayColor2,
				TopOrigin = topOrigin,
				LeftOrigin = leftOrigin
			});
		}

		private void DrawNorthSouthOrientedDemographics(string displayText, 
														ConsoleColor displayColor1,
														ConsoleColor displayColor2,
														ConsoleColor bgColor,
														QuadrantZoomInfo zoomInfo, 
														int offSet)
		{
			var tempList = new List<QuadrantCharInfo>();

			for (int i = zoomInfo.CurrentOrigin.Y; i < zoomInfo.CurrentOrigin.Y + zoomInfo.DrawHeight; i++)
			{
				// Overwrite the existing
				var existing =
					zoomInfo.DisplayChars.Where(dc => dc.LeftOrigin == zoomInfo.CurrentOrigin.X + offSet && dc.TopOrigin == i).ToArray();
				if (existing.Any())
				{
					zoomInfo.DisplayChars.Remove(existing.SingleOrDefault());
				}

				tempList.Add(new QuadrantCharInfo
				{
					DisplayText = displayText,
					BGColor = bgColor,
					TextColor1 = displayColor1,
					TextColor2 = displayColor2,
					TopOrigin = i,
					LeftOrigin = zoomInfo.CurrentOrigin.X + offSet
				});
			}

			zoomInfo.DisplayChars = zoomInfo.DisplayChars.Concat(tempList).ToList();
		}

		private void DrawEastWestOrientedDemographics(string displayText, 
														ConsoleColor displayColor1,
														ConsoleColor displayColor2,
														ConsoleColor bgColor,
														QuadrantZoomInfo zoomInfo, 
														int offSet)
		{
			var tempList = new List<QuadrantCharInfo>();

			for (int i = zoomInfo.CurrentOrigin.X; i < zoomInfo.CurrentOrigin.X + zoomInfo.DrawWidth; i++)
			{
				// Overwrite the existing
				var existing =
					zoomInfo.DisplayChars.Where(dc => dc.LeftOrigin == i && dc.TopOrigin == zoomInfo.CurrentOrigin.Y + offSet).ToArray();
				if (existing.Any())
				{
					zoomInfo.DisplayChars.Remove(existing.SingleOrDefault());
				}

				tempList.Add(new QuadrantCharInfo
					{
						DisplayText = displayText,
						BGColor = bgColor,
						TextColor1 = displayColor1,
						TextColor2 = displayColor2,
						TopOrigin = zoomInfo.CurrentOrigin.Y + offSet,
						LeftOrigin = i
					});
			}

			zoomInfo.DisplayChars = zoomInfo.DisplayChars.Concat(tempList).ToList();
		}


		private string[] ParseDemoDisplayString(string fullString)
		{
			var retVal = fullString.Split(new[] {','});
			return retVal;
		}

		private string NextCharacter(string[] text, int currentIndex)
		{
			if (currentIndex == text.Length - 1)
				return text[0];

			return text[currentIndex + 1];
		}


#endregion

	}

	public struct QuadrantCharInfo
	{
		public string DisplayText { get; set; }
		public ConsoleColor BGColor { get; set; }
		public ConsoleColor TextColor1 { get; set; }
		public ConsoleColor TextColor2 { get; set; }
		public int TopOrigin { get; set; }
		public int LeftOrigin { get; set; }
	
		public override int GetHashCode() {return TopOrigin.GetHashCode() ^ LeftOrigin.GetHashCode();}

		public bool LocationEquals(object obj)
		{
			if (obj == null || obj is DBNull) return false;

			var q = (QuadrantCharInfo)obj;

			return	TopOrigin == q.TopOrigin && LeftOrigin == q.LeftOrigin;
		}
	}


	public class QuadrantZoomInfo : ZoomInfo
	{
		public List<QuadrantCharInfo> DisplayChars { get; set; }

		public QuadrantZoomInfo()
		{
			CurrentOrigin = new Coordinate(0, 0, 0);
			DisplayChars = new List<QuadrantCharInfo>();
		}
	}

	public enum QuadrantLocation
	{
		TOPLEFT,
		TOPRIGHT,
		BOTTOMLEFT,
		BOTTOMRIGHT
	}

}
