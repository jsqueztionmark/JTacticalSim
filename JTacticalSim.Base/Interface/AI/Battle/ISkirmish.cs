using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Service;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.AI
{
	public interface ISkirmish : IBaseComponent
	{
		event BattleSkirmishStart SkirmishStart;
		event BattleSkirmishEnd SkirmishEnd;

		void On_SkirmishStart(EventArgs e);
		void On_SkirmishEnd(EventArgs e);

		SkirmishType Type { get; }
		IUnit Attacker { get; }
		IUnit Defender { get; }
		IUnit Victor { get; set; }
		List<IUnit> Destroyed { get; set; }
		IResult<ISkirmish, ISkirmish> BattleServiceResult { get; }
		BattleVictoryCondition VictoryCondition { get; set; }

		bool DefenderEvaded { get; set; }
		IResult<ISkirmish, ISkirmish> GetSkirmishResults();

		void Render();
		void RenderOutcome();
		void DoBattle();
	}
}
