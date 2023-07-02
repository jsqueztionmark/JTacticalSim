using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.API;

namespace JTacticalSim.Component.Game
{
	public class VictoryCondition : GameComponentBase, IVictoryCondition
	{
		public int Value { get; set; }
		public IVictoryConditionType VictoryConditionType { set; get; }
		public GameVictoryCondition ConditionType { get { return (GameVictoryCondition)VictoryConditionType.ID; } }
		public IFaction Faction { get; set; }
	}
}
