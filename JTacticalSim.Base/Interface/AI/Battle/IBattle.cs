using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Service;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.AI
{
	public interface IBattle : IBaseComponent, IRenderable
	{
		event BattleStart BattleStarted;
		event BattleEnd BattleEnded;

		List<IUnit> Attackers { get; }
		List<IUnit> Defenders { get; }
		IFaction VictorFaction { get; set; }
		IFaction AttackerFaction { get; }
		IFaction DefenderFaction { get; }
		IRound CurrentRound { get; }
		int CurrentRoundCount { get; }
		List<IUnit> DefeatedUnits { get; }
		BattleVictoryCondition VictoryCondition { get; set; }
		/// <summary>
		/// Used to track which units actually fought so that we can update the movement cache accordingly
		/// after the battle is finished
		/// </summary>
		List<IUnit> AttackersEngaged { get; }
		BattleType BattleType { get; set; }

		void AddAttacker(IUnit unit);
		void AddDefender(IUnit unit);

		/// <summary>
		/// Starts the full battle.
		/// </summary>
		void DoBattle();

		IResult<bool, IBattle> CanContinue();

		void RenderOutcome();
		void RenderRetreat();

		/// <summary>
		/// Starts a forced engagement for moving forces.
		/// </summary>
		void DoBattleForcedEngagement();

		/// <summary>
		/// Performs a nuclear strike on a given target
		/// </summary>
		/// <param name="target"></param>
		void DoNuclearStrike(INode target);

		void On_BattleStart(EventArgs e);
		void On_BattleEnded(EventArgs e);
		void On_RoundEnded(object sender, EventArgs e);
		void On_RoundStarted(object sender, EventArgs e);
	}
}
