using System;
using System.Text;
using JTacticalSim.API.Game;

namespace JTacticalSim.API.Component
{
	public interface IFaction : IBaseComponent
	{
		int? GetCurrentVictoryPoints();

		List<IVictoryCondition> VictoryConditions();

		bool GameVictoryAchieved();
	}
}
