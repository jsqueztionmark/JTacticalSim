using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using JTacticalSim.API;
using JTacticalSim.API.Game.State;
using JTacticalSim.API.Component;
using JTacticalSim.Utility;

namespace JTacticalSim.GameState
{
	/// <summary>
	/// Game state to handle when the computer AI is taking it's turn
	/// </summary>
	public sealed class AIInPlayState : BaseGameState
	{

		#region Properties and Fields


		#endregion

		#region Methods

		// Constructors

		public AIInPlayState(IGameStateSystem system)
			: base(system)
		{ }

		// Base Game State overrides

		public override void Update(double elapsedTime)
		{
			if (TheGame().IsConsoleGame)
				TheGame().CommandProcessor.ProcessInput(StateType.AI_IN_PLAY);
		}


		public override void Render()
		{
			TheGame().Renderer.RenderMainScreen();
		}

		#endregion

	}
}
