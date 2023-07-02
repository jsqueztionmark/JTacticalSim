using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;

namespace JTacticalSim.Component.Data
{
	public class Scenario : GameComponentBase, IScenario
	{
		public string GameFileDirectory { get; set; }
		public string Author { get; set; }
		public bool IsScenario { get { return true; }}
		public IComponentSet ComponentSet { get; set; }
		public List<ICountry> Countries { get; set; }
		public List<IFaction> Factions { get; set; }
 		public List<IVictoryCondition> VictoryConditions { get; set; } 
		public string Synopsis { get; set; }

		public Scenario()
		{
			Countries = new List<ICountry>();
			Factions = new List<IFaction>();
			VictoryConditions = new List<IVictoryCondition>();
			Synopsis = "No synopsis defined.";
		}

		/// <summary>
		/// Returns a copy of the Scenario
		/// </summary>
		/// <returns></returns>
		public IScenario ShallowCopy()
		{			
			var r = (IScenario)MemberwiseClone();
			return r;
		}
	}
}
