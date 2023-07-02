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
	public class Battle : GameComponentBase, IBattle
	{
		public event BattleStart BattleStarted;
		public event BattleEnd BattleEnded;

		public List<IUnit> Attackers { get; private set; }
		public List<IUnit> Defenders { get; private set; }

		public IFaction AttackerFaction { get { return (Attackers.Any()) ? Attackers.First().Country.Faction : null; } }
		public IFaction DefenderFaction { get { return (Defenders.Any()) ? Defenders.First().Country.Faction : null; } }

		public IFaction VictorFaction { get; set; }
		public IRound CurrentRound { get; private set; }
		public int CurrentRoundCount { get; private set; }
		public List<IUnit> DefeatedUnits { get; private set; }

		/// <summary>
		/// Used to track which units actually fought so that we can update the movement cache accordingly
		/// after the battle is finished
		/// </summary>
		public List<IUnit> AttackersEngaged { get; private set; }

		public BattleVictoryCondition VictoryCondition { get; set; }
		public BattleType BattleType { get; set; }

		/// <summary>
		/// Primary constructor. Use. Force Units to add themselves to the battle
		/// </summary>
		/// <param name="battleType"></param>
		public Battle(BattleType battleType)
		{
			CurrentRoundCount = 0;
			DefeatedUnits = new List<IUnit>();
			AttackersEngaged = new List<IUnit>();
			Attackers = new List<IUnit>();
			Defenders = new List<IUnit>();
			BattleType = battleType;
			VictoryCondition = BattleVictoryCondition.NO_VICTOR;
		}

		/// <summary>
		/// Try not to use outside of battle scripts. 
		/// Kept mostly for ease of use with linqPad battle scripts
		/// </summary>
		/// <param name="attackers"></param>
		/// <param name="defenders"></param>
		/// <param name="battleType"></param>
		public Battle(List<IUnit> attackers, List<IUnit> defenders, BattleType battleType)
		{
			CurrentRoundCount = 0;
			DefeatedUnits = new List<IUnit>();
			AttackersEngaged = new List<IUnit>();
			Attackers = attackers;
			Defenders = defenders;
			BattleType = battleType;
			VictoryCondition = BattleVictoryCondition.NO_VICTOR;
		}

		public void AddAttacker(IUnit unit)
		{
			Attackers.Add(unit);
		}

		public void AddDefender(IUnit unit)
		{
			Defenders.Add(unit);
		}

		public void DoBattle()
		{
			// 1. Air defence fires
			// 2. Missile defence fires (TODO)
			// 3. Start rounds with remaining units

			On_BattleStart(new EventArgs());
			HandleSpecialDefenceRounds();

			// While there's no victor, start new round. 
			// Else raise BattleEnd Event and handle victory conditions

			while (this.VictoryCondition == BattleVictoryCondition.NO_VICTOR)
			{
				StartNewRound(SkirmishType.FULL);
			}

			On_BattleEnded(new EventArgs());	
		}

		public void DoBattleForcedEngagement()
		{
			// No engagement if the battle can't continue anyway....
			if (!this.CanContinue().Result) return;

			On_BattleStart(new EventArgs());
			var i = 0;

			// Perform configured number of forced rounds
			while (this.VictoryCondition == BattleVictoryCondition.NO_VICTOR && i < 1)
			{
				StartNewRound(SkirmishType.FULL);
				i++;
			}

			On_BattleEnded(new EventArgs());

		}

		public void DoNuclearStrike(INode target)
		{
			On_BattleStart(new EventArgs());
			var BattleServiceResult = TheGame().JTSServices.AIService.ResolveNuclearBattle(this, target);
			DefeatedUnits.ForEach(u => { u.RemoveFromGame(); u.RemoveFromStack(); });
			On_BattleEnded(new EventArgs());
		}

		private void HandleSpecialDefenceRounds()
		{
			// Air defence round
			StartNewRound(SkirmishType.AIR_DEFENCE);

			// TODO: Not yet working with missile defence play
			// Missile defence round
			// StartNewRound(SkirmishType.MISSILE_DEFENCE);
		}

		private void StartNewRound(SkirmishType skirmishType)
		{
			CurrentRound = new Round(this);
			CurrentRound.RoundStart += On_RoundStarted;
			CurrentRound.RoundEnd += On_RoundEnded;

			IResult<List<ISkirmish>, ISkirmish> skirmishesResult;

			switch (skirmishType)
			{
				case SkirmishType.AIR_DEFENCE :
					skirmishesResult = TheGame().JTSServices.AIService.CreateAirDefenceSkirmishes(this);
					break;
				case SkirmishType.MISSILE_DEFENCE :
					skirmishesResult = TheGame().JTSServices.AIService.CreateMissileDefenceSkirmishes(this);
					break;
				default :
					skirmishesResult = TheGame().JTSServices.AIService.CreateFullCombatSkirmishes(this);
					break;
			}

			// Handle no available skirmishes for full combat
			if (skirmishesResult.Status == ResultStatus.FAILURE && skirmishType == SkirmishType.FULL)
			{
				VictoryCondition = BattleVictoryCondition.STALEMATE;
				return;
			}

			if (skirmishesResult.Status == ResultStatus.SUCCESS)
			{
				CurrentRoundCount += 1;
				skirmishesResult.Result.ForEach(sk => CurrentRound.AddSkirmish(sk));			
				CurrentRound.DoBattle();
			}

		}

		private void CheckVictoryStatus()
		{
			// Check for victory
			// Sets the victory conditions for the current battle
			var result = TheGame().JTSServices.RulesService.CheckBattleVictoryCondition(this);

			// Handle exception
			if (result.Status == ResultStatus.EXCEPTION)
				throw result.ex;

			// Handle failure - no victory condition found
			if (result.Status == ResultStatus.FAILURE)
			{ }
			//	Do stuff
			// Inform the UI?
		}

		public void Render()
		{
			TheGame().Renderer.On_BattlePreRender(new EventArgs());
			TheGame().Renderer.RenderBattle(this);
			TheGame().Renderer.On_BattlePostRender(new EventArgs());
		}

		public void RenderOutcome()
		{
			TheGame().Renderer.RenderBattleOutcome(this);
		}

		public void RenderRetreat()
		{
			TheGame().Renderer.RenderBattleRetreat(this);
		}

	// Rules

		public IResult<bool, IBattle> CanContinue()
		{
			return TheGame().JTSServices.RulesService.BattleCanContinue(this);
		}

	// Event Handlers

		public void On_BattleStart(EventArgs e)
		{
			TheGame().CurrentBattle = this;
			
			if (BattleStarted != null) BattleStarted(this, e);
		}

		public void On_BattleEnded(EventArgs e)
		{
			if (VictoryCondition == BattleVictoryCondition.SURRENDER)
			{
				// Handle Surrender
			}

			if (VictoryCondition == BattleVictoryCondition.RETREAT)
			{
				// Handle retreat 
			}

			if (VictoryCondition == BattleVictoryCondition.ALL_DESTROYED)
			{
				// Handle all units destroyed
			}

			if (VictoryCondition == BattleVictoryCondition.ATTACKERS_VICTORIOUS)
			{
				// Attempt to claim the node for victor faction
				if (Attackers.Any() & BattleType == BattleType.LOCAL)
					TheGame().JTSServices.AIService.ClaimNodeForVictorFaction(Attackers, TheGame().GameBoard.SelectedNode);
			}

			if (VictoryCondition == BattleVictoryCondition.DEFENDERS_VICTORIOUS)
			{
				// Handle victorious defenders
			}

			// Update the movement cache for having performed an action this turn'
			// remote battle units will be allowed to continue with battle this turn as long as
			// they have remote fire points remaining
			// It is possible for there to be no attackers engaged if the battle ends with a
			// special defence round that takes out all attackers (which can not fire)
			AttackersEngaged.ForEach(u => u.CurrentMoveStats.HasPerformedAction = true);

			if (BattleEnded != null) BattleEnded(this, e);
			TheGame().CurrentBattle = null;
		}

		public void On_RoundEnded(object sender, EventArgs e)
		{
			var defeatedUnits = CurrentRound.GetDefeatedUnits().ToList();
			if (defeatedUnits.Any())
			{
				DefeatedUnits.AddRange(CurrentRound.GetDefeatedUnits());
				defeatedUnits.ForEach(u => { u.RemoveFromGame(); u.RemoveFromStack(); });
			}

			CheckVictoryStatus();
			CurrentRound = null;
		}

		public void On_RoundStarted(object sender, EventArgs e)
		{
			CurrentRound = (IRound)sender;
		}

	}
}
