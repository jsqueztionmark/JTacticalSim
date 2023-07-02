using System;
using JTacticalSim.API.Game;
using ConsoleControls;

namespace JTacticalSim.ConsoleApp
{
	public static class StatusDisplay
	{
		public static ConsoleBox StatusBox;
		private static IGame _game = Game.Instance;

		public static bool? Display(string message, BoxDisplayType displayType, PromptType promptType = PromptType.PRESS_ANY_KEY)
		{
			switch (displayType)
			{
				case BoxDisplayType.ERROR:
					return DisplayError(message, promptType);
				case BoxDisplayType.INFO:
					return DisplayInfo(message, promptType);
				case BoxDisplayType.WARNING:
					return DisplayWarning(message, promptType);
				default:
					return false;
			}
		}

		private static bool? DisplayError(string message, PromptType promptType)
		{	
			if (string.IsNullOrWhiteSpace(message))
				return false;

			StatusBox = new ConsoleBox(BoxDisplayType.ERROR, promptType)
				{
					Width = Global.Measurements.BASE_ERROR_CMD_WIDTH,
					LeftOrigin = Global.Measurements.BASE_ERROR_CMD_ORIGIN_LEFT,
					TopOrigin = Global.Measurements.BASE_ERROR_CMD_ORIGIN_TOP,
					Height = Global.Measurements.BASE_ERROR_CMD_HEIGHT,
					BorderForeColor = ConsoleColor.White,
					BorderBackColor = ConsoleColor.Red,
					PromptColor = ConsoleColor.White,
					DrawElements = new SingleLineBoxElements(),
					BackColor = ConsoleColor.Red,
					ForeColor = ConsoleColor.White,
					DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
					EraseColor = Global.Colors.MainMapBackColor,
					DropShadow = true,
					Caption = "Error!",
					Text = message
				};

			StatusBox.CenterPositionHorizontal();
			StatusBox.ClearAndRedraw();
			_game.Renderer.RenderBoardFrame();
			StatusBox.RedrawControlAffectedNodes();
			return StatusBox.Prompt;
		}

		private static bool? DisplayInfo(string message, PromptType promptType)
		{
			if (string.IsNullOrWhiteSpace(message))
				return false;

			StatusBox = new ConsoleBox(BoxDisplayType.INFO, promptType)
				{
					Width = Global.Measurements.BASE_ERROR_CMD_WIDTH,
					LeftOrigin = Global.Measurements.BASE_ERROR_CMD_ORIGIN_LEFT,
					TopOrigin = Global.Measurements.BASE_ERROR_CMD_ORIGIN_TOP,
					Height = Global.Measurements.BASE_ERROR_CMD_HEIGHT,
					BorderForeColor = ConsoleColor.White,
					BorderBackColor = ConsoleColor.DarkCyan,
					PromptColor = ConsoleColor.Gray,
					DrawElements = new SingleLineBoxElements(),
					BackColor = ConsoleColor.DarkCyan,
					ForeColor = ConsoleColor.White,
					DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
					EraseColor = Global.Colors.MainMapBackColor,
					DropShadow = true,
					Caption = "Info!",
					Text = message
				};

			StatusBox.CenterPositionHorizontal();
			StatusBox.ClearAndRedraw();
			_game.Renderer.RenderBoardFrame();
			StatusBox.RedrawControlAffectedNodes();
			return StatusBox.Prompt;
		}

		private static bool? DisplayWarning(string message, PromptType promptType)
		{
			if (string.IsNullOrWhiteSpace(message))
				return false;

			StatusBox = new ConsoleBox(BoxDisplayType.WARNING, promptType)
				{
					Width = Global.Measurements.BASE_ERROR_CMD_WIDTH,
					LeftOrigin = Global.Measurements.BASE_ERROR_CMD_ORIGIN_LEFT,
					TopOrigin = Global.Measurements.BASE_ERROR_CMD_ORIGIN_TOP,
					Height = Global.Measurements.BASE_ERROR_CMD_HEIGHT,
					BorderForeColor = ConsoleColor.Black,
					BorderBackColor = ConsoleColor.Yellow,
					PromptColor = ConsoleColor.Black,
					DrawElements = new SingleLineBoxElements(),
					BackColor = ConsoleColor.Yellow,
					ForeColor = ConsoleColor.Black,
					DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
					EraseColor = Global.Colors.MainMapBackColor,
					DropShadow = true,
					Caption = "Warning!",
					Text = message
				};

			StatusBox.CenterPositionHorizontal();
			StatusBox.ClearAndRedraw();
			_game.Renderer.RenderBoardFrame();
			StatusBox.RedrawControlAffectedNodes();
			return StatusBox.Prompt;
		}
	}
}
