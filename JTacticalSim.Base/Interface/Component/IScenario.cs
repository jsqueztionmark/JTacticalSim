using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Game;

namespace JTacticalSim.API.Component
{
	public interface IScenario : IGameFileCopyable
	{
		string Author { get; set; }
		IComponentSet ComponentSet { get; set; }
		List<ICountry> Countries { get; set; }
		List<IFaction> Factions { get; set; }
 		List<IVictoryCondition> VictoryConditions { get; set; } 
		string Synopsis { get; set; }
		IScenario ShallowCopy();
	}
}
