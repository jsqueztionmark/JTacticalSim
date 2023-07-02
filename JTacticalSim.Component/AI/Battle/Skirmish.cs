using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API;
using JTacticalSim.API.Service;

namespace JTacticalSim.Component.AI
{
	/// <summary>
	/// Represents a battle skirmish between two single units
	/// </summary>
	public class Skirmish : GameComponentBase, ISkirmish
	{
		public event BattleSkirmishStart SkirmishStart;
		public event BattleSkirmishEnd SkirmishEnd;

		public SkirmishType Type { get; private set; }
		public IUnit Attacker { get; private set; }
		public IUnit Defender { get; private set; }
		public IUnit Victor { get; set; }
		public List<IUnit> Destroyed { get; set; }
		public IResult<ISkirmish, ISkirmish> BattleServiceResult { get; private set; }

		public BattleVictoryCondition VictoryCondition { get; set; }

		public bool DefenderEvaded { get; set; }

		/// <summary>
		/// Handle on the current battle so that we can updated the master attacker and defender collections
		/// </summary>
		private IBattle _battle { get; set; }


		public Skirmish(IUnit attacker, 
						IUnit defender, 
						IBattle battle, 
						SkirmishType type)
		{
			Destroyed = new List<IUnit>();
			Type = type;
			Attacker = attacker;
			Defender = defender;
			_battle = battle;
			DefenderEvaded = false;
			VictoryCondition = BattleVictoryCondition.NO_VICTOR;
		}

		public void DoBattle()
		{
			On_SkirmishStart(new EventArgs());
			// Return this result??
			BattleServiceResult = TheGame().JTSServices.AIService.ResolveSkirmish(this, _battle.BattleType);
			On_SkirmishEnd(new EventArgs());
		}

		public IResult<ISkirmish, ISkirmish> GetSkirmishResults()
		{
			var r = TheGame().JTSServices.RulesService.CheckSkirmishVictoryCondition(this);
			return r;
		}

		public void Render()
		{
			TheGame().Renderer.On_SkirmishPreRender(new EventArgs());
			TheGame().Renderer.RenderBattleSkirmish(this);
			TheGame().Renderer.On_SkirmishPostRender(new EventArgs());
		}

		public void RenderOutcome()
		{
			TheGame().Renderer.RenderBattleSkirmishOutcome(this);
		}
	// Event Handlers

		// FYI: 
		// When the app is running multithreaded, using these handlers can cause collation issues

		public void On_SkirmishStart(EventArgs e)
		{
			Render();
			Attacker.PlaySoundAsync(SoundType.FIRE);
			Defender.PlaySoundAsync(SoundType.FIRE);
			if (SkirmishStart != null) SkirmishStart(this, e);
		}

		public void On_SkirmishEnd(EventArgs e)
		{
			// Update the battle:
			if (_battle != null)
			{
				if (Destroyed.Contains(Attacker)) _battle.Attackers.Remove(Attacker);
				if (Destroyed.Contains(Defender)) _battle.Defenders.Remove(Defender);

				// For special defence skirmishes, the attackers are not penalized
				// since they can not fire we don't consider them fully engaged.
				if (this.Type == SkirmishType.FULL)
					_battle.AttackersEngaged.Add(Attacker);
			}

			RenderOutcome();

			if (SkirmishEnd != null) SkirmishEnd(this, e);
		}
	}
}
