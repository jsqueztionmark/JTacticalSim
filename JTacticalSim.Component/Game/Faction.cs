using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Service;
using JTacticalSim.API.Game;

namespace JTacticalSim.Component.Game
{
	public class Faction : GameComponentBase, IFaction
	{
		public int? GetCurrentVictoryPoints()
		{
			var r = TheGame().JTSServices.RulesService.CalculateTotalVictoryPoints(this);
			HandleResultDisplay(r, true);
			return r.Result;
		}

		public List<IVictoryCondition> VictoryConditions()
		{
			return TheGame().JTSServices.GameService.GetVictoryConditionsByFaction(this);
		}

		public bool GameVictoryAchieved()
		{
			return TheGame().JTSServices.RulesService.GameVictoryAchieved(this).Result;
		}
	}
}
