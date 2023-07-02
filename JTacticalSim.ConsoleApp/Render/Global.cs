using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTacticalSim.ConsoleApp
{
	public static class Global
	{
		public static class Measurements
		{
			public const int ROWSPACINGZOOM4 = 8;									
			public const int COLUMNSPACINGZOOM4 = 12;
			public const int ROWSPACINGZOOM3 = 6;
			public const int COLUMNSPACINGZOOM3 = 10;				
			public const int ROWSPACINGZOOM2 = 4;										
			public const int COLUMNSPACINGZOOM2 = 6;	
			public const int ROWSPACINGZOOM1 = 2;										
			public const int COLUMNSPACINGZOOM1 = 2;							

			public const int MAINBORDERWIDTH = 1;
			public const int MAINHEADERHEIGHT = 5;
			public const int MAINMARGIN = 1;
			public const int WESTMARGIN = MAINBORDERWIDTH + MAINMARGIN;
			public const int NORTHMARGIN = MAINHEADERHEIGHT;
			public const int BOARDBOUNDARYWIDTH = 1;

			public static int BASE_ERROR_CMD_ORIGIN_TOP = Console.WindowHeight - 21;
			public static int BASE_ERROR_CMD_ORIGIN_LEFT = 15;
			public static int BASE_ERROR_CMD_WIDTH = Console.WindowWidth - 80;
			public const int BASE_ERROR_CMD_HEIGHT = 8;

			public static int BASE_REPORT_ORIGIN_TOP = Console.WindowHeight - 21;
			public static int BASE_REPORT_ORIGIN_LEFT = 15;
			public static int BASE_REPORT_WIDTH = Console.WindowWidth - 80;
			public const int BASE_REPORT_HEIGHT = 15;

			public static int BASE_CMD_ORIGIN_TOP = Console.WindowHeight - 21;
			public static int BASE_CMD_ORIGIN_LEFT = 15;
			public static int BASE_CMD_WIDTH = Console.WindowWidth - 80;
			public const int BASE_CMD_HEIGHT = 8;

			public const int NODE_ACTION_SELECT_WIDTH = 32;
			public const int NODE_ACTION_SELECT_HEIGHT = 20;
			public const int MAIN_MENU_ACTION_SELECT_WIDTH = 32;
			public const int MAIN_MENU_ACTION_SELECT_HEIGHT = 20;
		}

		public static class Colors
		{
			public const ConsoleColor BASE_BG_COLOR = ConsoleColor.DarkGray;
			public const ConsoleColor BASE_FG_COLOR = ConsoleColor.White;
			public const ConsoleColor BASE_DROPSHADOW_COLOR = ConsoleColor.DarkRed;

			public const ConsoleColor NodeAvailableForMoveColor = ConsoleColor.Cyan;
			public const ConsoleColor NodeNotAvailableForMoveColor = ConsoleColor.Red;
			public const ConsoleColor NodeHighlightedColor = ConsoleColor.White;
			public const ConsoleColor NodeHighlightedUnitSelectedColor = ConsoleColor.Red;
			public const ConsoleColor UnitSelectedBGColor = ConsoleColor.Cyan;
			public const ConsoleColor UnitSelectedColor = ConsoleColor.Cyan;
			public const ConsoleColor UnitMarkerDropshadowColor = ConsoleColor.DarkRed;

			public const ConsoleColor BoardBoundaryBGColor = BASE_BG_COLOR;
			public const ConsoleColor BoardBoundaryFGColor = BASE_FG_COLOR;
		
			public const ConsoleColor LandBGColor = ConsoleColor.DarkGreen;
			public const ConsoleColor WaterBGColor = ConsoleColor.Blue;
			public const ConsoleColor NuclearWastelandBGColor = ConsoleColor.Red;
			public const ConsoleColor TransportationBGColor = ConsoleColor.Black;

			public const ConsoleColor MapLabelForeColor = ConsoleColor.White;
			public const ConsoleColor MapLableBackColor = ConsoleColor.Black;

			public const ConsoleColor TopMenuBGColor = ConsoleColor.Black;
			public const ConsoleColor TopMenuForeColor = BASE_FG_COLOR;
			public const ConsoleColor TopMenuForeColorUnavailable = ConsoleColor.DarkGray;
			public const ConsoleColor MainMenuBorderBackColor = BASE_BG_COLOR;
			public const ConsoleColor MainMenuBorderForeColor = ConsoleColor.Yellow;
			public const ConsoleColor MainMenuForeColor = ConsoleColor.White;
			public const ConsoleColor MainMenuBackColor = BASE_BG_COLOR;

			public const ConsoleColor MapMenuBorderBackColor = BASE_BG_COLOR;
			public const ConsoleColor MapMenuBorderForeColor = ConsoleColor.Yellow;
			public const ConsoleColor MapMenuForeColor = ConsoleColor.White;
			public const ConsoleColor MapMenuBackColor = BASE_BG_COLOR;

			public const ConsoleColor MapInfoBoxBorderBackColor = BASE_BG_COLOR;
			public const ConsoleColor MapInfoBoxBorderForeColor = ConsoleColor.Yellow;
			public const ConsoleColor MapInfoBoxBackColor = BASE_BG_COLOR;
			public const ConsoleColor MapInfoBoxForeColor = ConsoleColor.White;

			public const ConsoleColor VictoryPointBackColor = ConsoleColor.Red;
			public const ConsoleColor VictoryPointForeColor = ConsoleColor.White;

			// General
			public const ConsoleColor ScreenBGColor = BASE_BG_COLOR;
			public const ConsoleColor ScreenForeColor = ConsoleColor.Black;
			public const ConsoleColor ScreenBorderBGColor = BASE_FG_COLOR;
			public const ConsoleColor ScreenBorderForeColor = ConsoleColor.Black;

			public const ConsoleColor ScenarioInfoTextColor = ConsoleColor.Cyan;

			public const ConsoleColor SelectContainerBGColor = BASE_BG_COLOR;
			public const ConsoleColor SelectContainerForeColor = BASE_FG_COLOR;
			public const ConsoleColor SelectContainerBorderBGColor = BASE_BG_COLOR;
			public const ConsoleColor SelectContainerBorderForeColor = BASE_FG_COLOR;

			public const ConsoleColor MainMapBackColor = ConsoleColor.DarkGreen;
		}
	}
}
