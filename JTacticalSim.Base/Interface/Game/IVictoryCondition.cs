using System;
using System.Text;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.Game
{
	public interface IVictoryCondition : IBaseComponent
	{
		int Value { get; set; }
		IVictoryConditionType VictoryConditionType { set; get; }
		GameVictoryCondition ConditionType { get; }
		IFaction Faction { get; set; }
	}
}
