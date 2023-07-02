using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API
{
	public static class Constants
	{
	}

#region enums

	public enum Platform
	{
		UNKNOWN = 0,
		CONSOLE = 1,
		WINDOWS_TEXT = 2,
		WINDOWS_GUI = 5,
		WEB = 8
	}

	public enum DataSourceType
	{
		UNKNOWN = 0,
		MEMORY = 1,
		XML = 2,
		SQL = 5
	}

	public enum SoundSourceType
	{
		UNKNOWN = 0,
		WAV = 2
	}

	public enum UserInterfaceType
	{
		UNKNOWN = 0,
		CONSOLE = 1,
		WINDOWS = 2
	}

	public enum CommandType
	{
		UTILITY,
		MOVE,
		SELECT,
		DISPLAY,
		UNIT,
		BATTLE,
		SCREEN
	}

	public enum RouteType
	{
		SLOWESTUNIT,
		FASTESTUNIT,
		ALLUNITS
	}

	public enum ResultStatus
	{
		SUCCESS,
		FAILURE,
		SOME_FAILURE,
		EXCEPTION,
		OTHER
	}

	public enum GameObjectType
	{
		GAME_BASE,
		GAME_STATE,
		HANDLER,
		SERVICE,
		AGENT,
		SPRITE,
		COMPONENT,
		DTO,
		RENDER,
		COMMAND,
		TEST
	}

	public enum StateType
	{
		GAME_IN_PLAY,
		AI_IN_PLAY,
		BATTLE,
		REINFORCE,
		QUICK_SELECT,
		SCENARIO_INFO,
		SETTINGS_MENU,
		SPLASH_SCREEN,
		GAME_OVER,
		TITLE_MENU,
		HELP
	}
	
	public enum Commands
	{
		EXIT,
		PLAY,
		LOAD_GAME,
		NEW_GAME,
		SAVE_GAME,
		SAVE_GAME_AS,
		DELETE_GAME,
		DISPLAY_LEGEND,
		DISPLAY_MAIN_SCREEN,
		DISPLAY_REINFORCEMENTS_SCREEN,
		DISPLAY_UNIT_QUICK_SELECT_SCREEN,
		DISPLAY_TITLE_SCREEN,
		DISPLAY_SCENARIO_INFO_SCREEN,
		DISPLAY_HELP_SCREEN,
		CYCLE_ZOOM_LEVEL,
		ZOOM_MAP_IN,
		ZOOM_MAP_OUT,
		CYCLE_MAP_MODE_UP,
		CYCLE_MAP_MODE_DOWN,
		DISPLAY_COMMAND_LIST,
		REFRESH_BOARD,
		END_TURN,
		DISPLAY_UNIT,
		DISPLAY_PLAYER,
		DISPLAY_ASSIGNED_UNITS,
		DISPLAY_UNITS,
		SET_CURRENT_NODE,
		SET_SELECTED_UNIT,
		SET_SELECTED_UNITS,
		SET_SELECTED_UNITS_W_ATTACHED,
		UNSELECT_ALL_UNITS,
		DISPLAY_CURRENT_NODE,
		MOVE_UNITS_TO_SELECTED_NODE,
		MOVE_UNIT_TO_SELECTED_NODE,
		CYCLE_UNITS,
		ADD_UNIT,
		REMOVE_UNIT,
		EDIT_UNIT,
		ATTACH_UNIT,
		ATTACH_UNITS,
		DETACH_UNIT,
		DETACH_UNITS,
		LOAD_UNIT,
		DEPLOY_UNIT,
		SET_BATTLE_POSTURE_OFFENSE,
		SET_BATTLE_POSTURE_DEFENCE,
		SET_BATTLE_POSTURE_STANDARD,
		SET_BATTLE_POSTURE_EVASION,
		BARRAGE,
		NUKE,
		DO_BATTLE,
		SCENARIO_EDITOR,
		GET_REINFORCEMENTS,
		ADD_REINFORCEMENT_UNIT,
		PLACE_REINFORCEMENT_UNIT,
		BUILD_INFRASTRUCTURE,
		DESTROY_INFRASTRUCTURE
	}

	public enum MouseButton
	{
		LEFT,
		DOUBLELEFT,
		RIGHT,
		DOUBLERIGHT,
		MIDDLE
	}

	public enum BattleType
	{
		LOCAL,
		BARRAGE,
		NUCLEAR,
		FORCED_ENGAGEMENT
	}

	public enum SkirmishType
	{
		AIR_DEFENCE,
		MISSILE_DEFENCE,
		FULL
	}

	public enum BattleVictoryCondition
	{
		ATTACKERS_VICTORIOUS,
		DEFENDERS_VICTORIOUS,
		ALL_DESTROYED,
		SURRENDER,
		STALEMATE,
		RETREAT,
		EVADED,
		NO_VICTOR
	}

	public enum GameVictoryCondition
	{
		ENEMY_UNITS_REMAINING = 0,
		VICTORY_POINTS_HELD = 1,
		FLAG_CAPTURED = 2
	}

	public enum StrategicalStance
	{
		OFFENSIVE,
		DEFENSIVE,
		ADMINISTRATIVE,
		DECOY,
		OTHER
	}

	public enum BattlePosture
	{
		STANDARD,
		OFFENSIVE,
		DEFENSIVE,
		EVASION
	}

	public enum StrategicAssessmentRating
	{
		NONE = 0,
		VERYLOW = 1,
		LOW = 2,
		MEDIUM = 3,
		HIGH = 4,
		VERYHIGH = 5,
		ULTIMATE = 6
	}

	public enum SoundType
	{
		MOVE,
		FIRE,
		BUILD,
		CLICK1,
		CLICK2,
		DESTROY
	}

	public enum CycleDirection
	{
		IN,
		OUT,
		LEFT,
		RIGHT,
		UP,
		DOWN
	}

	public enum ZoomLevel
	{
		ONE = 1,
		TWO = 2,
		THREE = 3,
		FOUR = 4
	}

	public enum MapMode
	{
		HIGH_CONTRAST,
		GEOGRAPHICAL,		
		POLITICAL		
	}

	public enum MessageDisplayType : int
	{
		DISPLAY,
		TEXT,
		CMD,
		INFO,
		WARNING,
		ERROR
	}

#endregion
}
