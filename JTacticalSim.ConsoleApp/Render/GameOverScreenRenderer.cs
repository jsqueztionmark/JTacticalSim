using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using JTacticalSim.API;
using JTacticalSim.API.AI;
using JTacticalSim.API.Component;
using JTacticalSim.API.Component.Util;
using JTacticalSim.API.Game;
using JTacticalSim.Component.World;
using JTacticalSim.Utility;
using ConsoleControls;

namespace JTacticalSim.ConsoleApp
{
	public sealed class GameOverScreenRenderer : BaseScreenRenderer, IScreenRenderer
	{

#region Controls


#endregion

		public GameOverScreenRenderer(ConsoleRenderer baseRenderer)
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
					Caption = "Game Over!!!"
				};

			MainBorder.WindowClosePressed += On_CtlXPressed;
		}

		protected override void DrawOverlay()
		{
		}

	}
}
