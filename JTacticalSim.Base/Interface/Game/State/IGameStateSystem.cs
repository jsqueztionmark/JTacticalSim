using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.Game.State
{
	public interface IGameStateSystem
	{
		// Events
		event GameStateChangedEvent GameStateChanged;
		void On_GameStateChanged(object sender, EventArgs e);

		StateType CurrentStateType { get; }

		void Update(double elapsedTime);
		void Render();
		void AddState(StateType stateType, IGameState state);
		void ChangeState(StateType stateType);
		bool Exists(StateType stateType);

	}
}
