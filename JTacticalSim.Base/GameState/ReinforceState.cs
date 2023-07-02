using System;
using System.Collections.Generic;
using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.Game.State;
using JTacticalSim.API.Component;

namespace JTacticalSim.GameState
{
	public sealed class ReinforceState : BaseGameState
	{
		public ReinforceState(IGameStateSystem system)
			: base(system)
		{}

		public override void Update(double elapsedTime)
		{
			TheGame().CommandProcessor.ProcessInput(StateType.REINFORCE);
		}

		public override void Render()
		{
			TheGame().Renderer.RenderReinforcementScreen();		
		}
	}
}
