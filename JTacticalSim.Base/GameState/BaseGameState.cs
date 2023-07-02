using System;
using System.Collections.Generic;
using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.Game.State;

namespace JTacticalSim.GameState
{
	public abstract class BaseGameState : BaseGameObject, IGameState
	{
		protected IGameStateSystem _system;

		protected BaseGameState(IGameStateSystem system)
			: base(GameObjectType.GAME_STATE)
		{
			_system	= system;
		}

		public abstract void Update(double elapsedTime);
		public abstract void Render();
	}
}