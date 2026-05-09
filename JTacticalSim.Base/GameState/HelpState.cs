using System;
using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.Game.State;
using JTacticalSim.API.Component;

namespace JTacticalSim.GameState
{
	public class HelpState : BaseGameState
	{
		public HelpState(IGameStateSystem system)
			: base(system)
		{}

		public override void Update(double elapsedTime)
		{
			TheGame().CommandProcessor.ProcessInput(StateType.HELP);
		}

		public override void Render()
		{
			TheGame().Renderer.RenderHelpScreen();		
		}
	}
}
