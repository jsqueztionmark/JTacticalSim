using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using JTacticalSim.API;
using JTacticalSim.API.Game.State;
using JTacticalSim.API.Component;

namespace JTacticalSim.GameState
{
	public sealed class TitleScreenState : BaseGameState
	{
		public TitleScreenState(IGameStateSystem system)
			: base(system)
		{}

		public override void Update(double elapsedTime)
		{
			TheGame().CommandProcessor.ProcessInput(StateType.TITLE_MENU);
		}

		public override void Render()
		{
			TheGame().Renderer.RenderTitleScreen();		
		}
	}
}
