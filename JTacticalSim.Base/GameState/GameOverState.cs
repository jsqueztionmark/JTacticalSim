using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using JTacticalSim.API;
using JTacticalSim.API.Game.State;
using JTacticalSim.API.Component;

namespace JTacticalSim.GameState
{
	public sealed class GameOverState : BaseGameState
	{
#region Properties and Fields


#endregion

#region Methods

		public GameOverState(IGameStateSystem system)
			: base(system)
		{}

		public override void Update(double elapsedTime)
		{
			TheGame().CommandProcessor.ProcessInput(StateType.GAME_OVER);
		}

		public override void Render()
		{
			TheGame().Renderer.RenderGameOverScreen();		
		}

#endregion
	}
}
