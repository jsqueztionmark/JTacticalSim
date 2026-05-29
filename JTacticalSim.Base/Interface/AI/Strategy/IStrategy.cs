using System;
using System.Text;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.AI
{
	[Obsolete("Tactics are now the highest level container in the strategy stack.")]
	public interface IStrategy : IAIStrategyObjectContainer<ITactic>, ICompletable, IExecutableTask<IStrategy, ITactic>
	{
		StrategicalStance Stance { get; }
	}
}
