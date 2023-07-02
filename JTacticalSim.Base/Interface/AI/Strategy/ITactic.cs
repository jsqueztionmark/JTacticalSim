using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;

namespace JTacticalSim.API.AI
{
	public interface ITactic : IAIStrategyObjectContainer<IMission>, ICompletable, IExecutableTask<ITactic, IMission>
	{
		IPlayer Player { get; }
		StrategicalStance Stance { get; }
	}
}
