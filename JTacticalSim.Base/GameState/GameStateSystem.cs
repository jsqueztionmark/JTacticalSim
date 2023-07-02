using System;
using System.Collections.Generic;
using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.Game;
using JTacticalSim.API.Game.State;

namespace JTacticalSim.GameState
{
	public sealed class GameStateSystem : IGameStateSystem
	{
		public event GameStateChangedEvent GameStateChanged;

		private readonly Dictionary<StateType, IGameState> _stateStore;

		IGameState _currentState = null;
		StateType _currentStateType;
		 
		public StateType CurrentStateType {get { return _currentStateType; }}

		public GameStateSystem()
		{
			_stateStore = new Dictionary<StateType, IGameState>();
			_currentState = null;
		}

		public void Update(double elapsedTime)
		{
			if (_currentState == null)
				return;

			_currentState.Update(elapsedTime);
		}


		public void Render()
		{
			if (_currentState == null)
				return;

			_currentState.Render();
		}

		public void AddState(StateType stateType, IGameState state)
		{
			if (!_stateStore.ContainsKey(stateType))
				_stateStore.Add(stateType, state);

			On_GameStateChanged(this, new EventArgs());
		}

		public void ChangeState(StateType stateType)
		{
			_currentStateType = stateType;
			_currentState = _stateStore[stateType];
			On_GameStateChanged(this, new EventArgs());
		}

		public bool Exists(StateType stateType)
		{
			return _stateStore.ContainsKey(stateType);
		}


	// Event handlers

		public void On_GameStateChanged(object sender, EventArgs e)
		{
			if (GameStateChanged != null) GameStateChanged(sender, e);
		}

	}
}
