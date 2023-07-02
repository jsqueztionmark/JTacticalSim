using System;
using System.Collections.Generic;
using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.Game.State;
using JTacticalSim.API.Component;

namespace JTacticalSim.GameState
{
	public class ScenarioInfoState : BaseGameState
	{
		public ScenarioInfoState(IGameStateSystem system)
			: base(system)
		{}

		public override void Update(double elapsedTime)
		{
			TheGame().CommandProcessor.ProcessInput(StateType.SCENARIO_INFO);
		}

		public override void Render()
		{
			TheGame().Renderer.RenderScenarioInfoScreen();		
		}
	}
}
