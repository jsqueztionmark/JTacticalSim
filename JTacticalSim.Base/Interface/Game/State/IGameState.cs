using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.Game.State
{
	public interface IGameState
	{
		void Update(double elapsedTime);
		void Render();
	}
}
