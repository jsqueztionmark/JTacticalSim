using System;
using System.Text;

namespace JTacticalSim.API.Game.State
{
	public interface IGameState
	{
		void Update(double elapsedTime);
		void Render();
	}
}
