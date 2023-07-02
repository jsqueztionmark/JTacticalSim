using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.Service;

namespace JTacticalSim.Component.Data
{
	public class SavedGame : GameComponentBase, ISavedGame
	{
		public string GameFileDirectory { get; set; }
		public bool LastPlayed { get; set; }
		public bool IsScenario { get { return false; } }
		public IScenario Scenario { get; set; }

		/// <summary>
		/// Returns a copy of the Saved Game with an incremented ID and new UID
		/// </summary>
		/// <returns></returns>
		public ISavedGame ShallowCopy()
		{			
			var r = (ISavedGame)MemberwiseClone();
			r.Scenario = r.Scenario.ShallowCopy();

			r.SetNextID();
			r.UID = Guid.NewGuid();

			return r;
		}
	}
}
