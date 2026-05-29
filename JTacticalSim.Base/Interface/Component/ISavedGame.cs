using System;
using System.Text;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.Component
{
	public interface ISavedGame : IGameFileCopyable
	{
		bool LastPlayed { get; set; }
		IScenario Scenario { get; set; }

		ISavedGame ShallowCopy();
	}
}
