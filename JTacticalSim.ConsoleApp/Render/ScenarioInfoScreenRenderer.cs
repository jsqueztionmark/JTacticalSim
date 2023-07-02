using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Text;
using System.Configuration;
using System.Transactions;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Component.Util;
using JTacticalSim.Component;
using JTacticalSim.Component.Data;
using JTacticalSim.Utility;
using ConsoleControls;

namespace JTacticalSim.ConsoleApp
{
	public sealed class ScenarioInfoScreenRenderer : BaseScreenRenderer, IScreenRenderer
	{

#region Controls

		public PagedConsoleBox ScenarioInfoBox;

#endregion

		public ScenarioInfoScreenRenderer(ConsoleRenderer baseRenderer)
		{
			_baseRenderer = baseRenderer;
		}

		protected override void InitializeControls()
		{
			// Screen board border
			MainBorder = new Screen
				{
					Height = (Console.WindowHeight / 2) + 20,
					Width = (Console.WindowWidth / 2) - 11,
					BorderForeColor = Global.Colors.ScreenBorderForeColor,
					BorderBackColor = Global.Colors.ScreenBorderBGColor,
					BackColor = Global.Colors.ScreenBGColor,
					ForeColor = Global.Colors.ScreenForeColor,
					Caption = TheGame().LoadedScenario.Name
				};

			MainBorder.CenterPositionHorizontal(23);
			MainBorder.CenterPositionVertical();
			MainBorder.WindowClosePressed += On_CtlXPressed;

			ScenarioInfoBox = new PagedConsoleBox()
				{
					Height = MainBorder.Height - 6,
					Width = MainBorder.Width - 10,
					TopOrigin = MainBorder.TopOrigin + 3,
					LeftOrigin = MainBorder.LeftOrigin + 5,
					BorderForeColor = Global.Colors.SelectContainerBorderForeColor,
					BorderBackColor = Global.Colors.SelectContainerBorderBGColor,
					BackColor = Global.Colors.SelectContainerBGColor,
					ForeColor = Global.Colors.ScenarioInfoTextColor,
					DrawElements = new SingleLineBoxElements(), 
					Caption = "Scenario Info",
					HasFocus = false,
					Text = TheGame().LoadedScenario.TextInfo()
				};

			ScenarioInfoBox.WindowClosePressed += On_CtlXPressed;
			Controls.Push(ScenarioInfoBox);
		}

	}
}
