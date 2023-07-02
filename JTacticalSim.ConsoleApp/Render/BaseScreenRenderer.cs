using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleControls;
using JTacticalSim.API;
using JTacticalSim.API.Game;
using JTacticalSim.Media.Sound;
using JTacticalSim.Service;
using JTacticalSim.Utility;

namespace JTacticalSim.ConsoleApp
{
	public abstract class BaseScreenRenderer : BaseGameObject
	{
		public Screen MainBorder;
		protected ControlStack Controls { get; set; }
		protected ConsoleRenderer _baseRenderer { get; set; }
		protected IZoomHandler ZoomHandler { get { return TheGame().ZoomHandler; } }
		protected IMapModeHandler MapModeHandler { get { return TheGame().MapModeHandler; } }

		protected BaseScreenRenderer()
			: base(GameObjectType.RENDER)
		{}

		public virtual void RenderScreen()
		{
			// This will allow us to make changes to the main screen to better emphasize this screen
			if (TheGame().LoadedGame != null)
			{
				_baseRenderer.RenderBoardFrame();
			}	
		
			Controls = new ControlStack();
			InitializeControls();
			RefreshScreen();
		}

		public virtual void RefreshScreen()
		{
			if (MainBorder != null)
			{
				MainBorder.HasFocus = (!Controls.Controls.Any(c => c.HasFocus) && MainBorder.CloseWindowX);
				MainBorder.ClearAndRedraw();
			}

			DrawOverlay();
			Controls.RenderControls();
		}

		protected virtual void DrawOverlay()
		{
		}

		public virtual void CloseScreen()
		{
			//TODO: Verify close screen
			Controls.ClearControls();
			TheGame().StateSystem.ChangeState(StateType.GAME_IN_PLAY);
		}

		protected abstract void InitializeControls();

		public virtual void DisplayUserMessage(BoxDisplayType messageType, string message, Exception ex)
		{
			var display = new StringBuilder("{0}".F(message));

			if (ex != null)
				display.AppendLine(ex.Message);

			StatusDisplay.Display(display.ToString(), messageType);
			RefreshScreen();
		}

		public void LoadNewGame(string name)
		{
			var ctx = GameContext.Instance;
			ctx.InitializeGame();
			var r = TheGame().LoadGame(name);
			HandleResultDisplay(r, true);
			TheGame().Start();
		}

#region Event Handlers

		protected void On_CtlXPressed(object sender, EventArgs e)
		{
			if (TheGame().LoadedGame == null)
			{
				TheGame().CommandProcessor.ProcessCommand(Commands.EXIT);
			}
				
			CloseScreen();
		}

#endregion
	}
}
