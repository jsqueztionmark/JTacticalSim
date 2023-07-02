using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.Component
{
	public interface ICompletable
	{
		event CompletedEvent Completed;

		bool IsComplete { get; }
		int TurnsToComplete { get; }

		int DecrementTurnsToComplete();
	}
}
