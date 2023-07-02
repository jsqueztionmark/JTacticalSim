using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Service;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.AI
{
	public interface IRound : IBaseComponent, IRenderable
	{
		event BattleRoundStart RoundStart;
		event BattleRoundEnd RoundEnd;

		void AddSkirmish(ISkirmish skirmish);
		IEnumerable<IUnit> GetDefeatedUnits();
		void DoBattle();

		ISkirmish CurrentSkirmish { get; }
		int CurrentSkirmishCount { get; }
		List<ISkirmish> Skirmishes { get; }

		// Event Handlers
		void On_RoundStart(EventArgs e);
		void On_RoundEnd(EventArgs e);
	}
}
