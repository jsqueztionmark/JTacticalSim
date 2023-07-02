using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Component.Util;
using JTacticalSim.Component;
using JTacticalSim.Utility;
using ConsoleControls;

namespace JTacticalSim.ConsoleApp
{
	public sealed class HelpScreenRenderer : BaseScreenRenderer, IScreenRenderer
	{

#region Controls

		public ConsoleBox KeyboardCommandsBox;
		public ConsoleBox CommandBox;
		public ConsoleBox CloseMeBox;

#endregion

		public HelpScreenRenderer(ConsoleRenderer baseRenderer)
		{
			_baseRenderer = baseRenderer;
		}

		protected override void InitializeControls()
		{
			// Screen border
			MainBorder = new Screen
				{
					Height = Console.WindowHeight - 12,
					Width = Console.WindowWidth - 6,
					TopOrigin = Global.Measurements.NORTHMARGIN - 1,
					LeftOrigin = Global.Measurements.WESTMARGIN,
					BorderForeColor = Global.Colors.ScreenBorderForeColor,
					BorderBackColor = Global.Colors.ScreenBorderBGColor,
					BackColor = Global.Colors.ScreenBGColor,
					ForeColor = Global.Colors.ScreenForeColor,
					Caption = "Help"
				};

			MainBorder.WindowClosePressed += On_CtlXPressed;

			// Keyboard shortcuts
			KeyboardCommandsBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE)
				{
					Height = 25,
					Width = (MainBorder.Width / 2) - 9,
					TopOrigin = MainBorder.TopOrigin + 3,
					LeftOrigin = MainBorder.LeftOrigin + 5,
					BackColor = Global.Colors.BoardBoundaryBGColor,
					BorderForeColor = Global.Colors.BoardBoundaryFGColor,
					BorderBackColor = Global.Colors.BoardBoundaryBGColor,
					ForeColor = Global.Colors.BoardBoundaryFGColor,
					DrawElements = new SingleLineBoxElements(),
					Caption = "Main Game Keyboard Commands"
				};

			// Commands
			CommandBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE)
				{
					Height = 25,
					Width = (MainBorder.Width / 2) - 9,
					TopOrigin = MainBorder.TopOrigin + 3,
					LeftOrigin = (MainBorder.Width / 2) + 5,
					BackColor = Global.Colors.BoardBoundaryBGColor,
					BorderForeColor = Global.Colors.BoardBoundaryFGColor,
					BorderBackColor = Global.Colors.BoardBoundaryBGColor,
					ForeColor = Global.Colors.BoardBoundaryFGColor,
					DrawElements = new SingleLineBoxElements(),
					Caption = "Command Box Commands"
				};

			// Allows us to add any number of non-focus controls and still Ctl-X
			// out of the screen
			CloseMeBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.CTLX_ONLY){Visible = false};
			CloseMeBox.WindowClosePressed += On_CtlXPressed;
			Controls.Push(CloseMeBox);
		}

		protected override void DrawOverlay()
		{
			DrawMapMoveInstructions();
			DrawCommandBoxInstructions();
		}

		private void DrawMapMoveInstructions()
		{
			KeyboardCommandsBox.ClearAndRedraw();
			var currentRow = KeyboardCommandsBox.TopOrigin + 2;
			const int margin = 2;
			const int tab1 = 45;

			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin + tab1, currentRow++);

			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.White;

			Console.Write("123");
			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin + tab1, currentRow++);
			Console.Write("4 6");
			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin + tab1, currentRow++);
			Console.Write("789");

			Console.BackgroundColor = KeyboardCommandsBox.BackColor;
			Console.ForegroundColor = KeyboardCommandsBox.ForeColor;

			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin, KeyboardCommandsBox.TopOrigin + 3);
			Console.Write("Move Cursor");

			currentRow++;
			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin, currentRow);
			Console.Write("Map Menu/Cancel Map Menu");
			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin + tab1, currentRow++);
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write("5");

			Console.BackgroundColor = KeyboardCommandsBox.BackColor;
			Console.ForegroundColor = KeyboardCommandsBox.ForeColor;

			currentRow++;
			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin, currentRow);
			Console.Write("Zoom Map In/Out");
			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin + tab1, currentRow++);
			Console.Write("[+/-]");

			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin, currentRow);
			Console.Write("Cycle Map Mode");
			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin + tab1, currentRow++);
			Console.Write("[Shift]+[+/-]");

			currentRow++;

			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin, currentRow);
			Console.Write("Select Top Unit/Scroll Unit Stack");
			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin + tab1, currentRow++);
			Console.Write("[SpcBar]");

			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin, currentRow);
			Console.Write("Select Unit w/Attached At Current Location");
			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin + tab1, currentRow++);
			Console.Write("[Ctl]+[SpcBar]");

			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin, currentRow);
			Console.Write("Select All Units At Current Location");
			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin + tab1, currentRow++);
			Console.Write("[Shift]+[Ctl]+[SpcBar]");

			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin, currentRow);
			Console.Write("Unselect All Units");
			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin + tab1, currentRow++);
			Console.Write("[Shift]+[SpcBar]");

			currentRow++;

			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin, currentRow);
			Console.Write("Open Reinforcements Screen");
			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin + tab1, currentRow++);
			Console.Write("[Ctl]+[R]");

			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin, currentRow);
			Console.Write("Open Unit Quick Select Screen");
			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin + tab1, currentRow++);
			Console.Write("[Ctl]+[U]");

			currentRow++;

			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin, currentRow);
			Console.Write("End Turn");
			Console.SetCursorPosition(KeyboardCommandsBox.LeftOrigin + margin + tab1, currentRow++);
			Console.Write("[Ctl]+[End]");
			
			Console.ResetColor();

		}

		private void DrawCommandBoxInstructions()
		{
			CommandBox.ClearAndRedraw();
		}
	}
}
