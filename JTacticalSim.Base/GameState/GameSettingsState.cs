using System;
using System.Collections.Generic;
using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.Game.State;
using JTacticalSim.API.Component;

namespace JTacticalSim.GameState
{
	public sealed class GameSettingsState : BaseGameState
	{
		public GameSettingsState(IGameStateSystem system)
			: base(system)
		{ }

		public override void Update(double elapsedTime)
		{
			if (TheGame().IsConsoleGame)
				TheGame().CommandProcessor.ProcessInput(StateType.SETTINGS_MENU);
		}

		public override void Render()
		{
			//TODO: Render game settings screen
			//TheGame().Renderer.RenderReinforcementScreen();
		}
	}
}

