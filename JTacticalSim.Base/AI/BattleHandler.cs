using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.Game;
using JTacticalSim.Utility;

namespace JTacticalSim.AI
{
	public class BattleHandler : BaseGameObject,  IBattleHandler
	{
		public BattleHandler()
			: base(GameObjectType.HANDLER)
		{}

		public IResult<IBattle, IBattle> BarrageUnitsAtLocation(INode target)
		{
			var attackers = TheGame().GameBoard.SelectedUnits.Where(u => u.IsFriendly())
															.Where(u => u.CurrentMoveStats.RemoteFirePoints > 0)
															.Where(u => u.CanDoBattleThisTurn()).ToList();


            var defenders = target.GetAllUnits().Where(u => !u.IsFriendly()).ToList();

			var r = CreateNewBattle(attackers, defenders, BattleType.BARRAGE);
			
			if (r.Status == ResultStatus.FAILURE)
			{
				r.Messages.Add("Can not do battle at selected location.");
				return r;
			}

			TheGame().CurrentBattle = r.Result;

			// So we can handle output to the UI
			SubscribeToCurrentBattleEvents();

			TheGame().CurrentBattle.DoBattle();

			return r;
		}

		public IResult<IBattle, IBattle> DoBattleAtLocation(INode target)
		{
			var allUnits = target.GetAllUnits();

			// Movers
			var attackers = allUnits.Where(u => u.IsFriendly())
									.Where(u => !u.IsBeingTransported())
									.Where(u => u.CanDoBattleThisTurn()).ToList();
			// Occupiers
			var defenders = allUnits.Where(u => !u.IsFriendly())
                                    .Where(u => !u.IsHiddenFromEnemy()).ToList();

			var r = CreateNewBattle(attackers, defenders, BattleType.LOCAL);
			
			if (r.Status == ResultStatus.FAILURE)
			{
				r.Messages.Add("Can not do battle at selected location.");
				return r;
			}

			TheGame().CurrentBattle = r.Result;

			// So we can handle output to the UI
			SubscribeToCurrentBattleEvents();

			TheGame().CurrentBattle.DoBattle();

			return r;
		}

		public IResult<IBattle, IBattle> NukeLocation(INode target)
		{
			var attackers = TheGame().GameBoard.SelectedUnits.Where(u => u.IsFriendly())
															.Where(u => u.CurrentMoveStats.RemoteFirePoints > 0)
															.Where(u => u.UnitInfo.UnitType.Nuclear)
															.Where(u => u.CanDoBattleThisTurn()).ToList();

            var defenders = target.GetAllUnits()
										.Where(u => u.UnitInfo.UnitType.BaseType.NuclearAffected).ToList();

			var r = CreateNewBattle(attackers, defenders, BattleType.NUCLEAR);
			
			if (r.Status == ResultStatus.FAILURE)
			{
				r.Messages.Add("Can not do battle at selected location.");
				return r;
			}

			TheGame().CurrentBattle = r.Result;

			// So we can handle output to the UI
			SubscribeToCurrentBattleEvents();
			TheGame().CurrentBattle.DoNuclearStrike(target);

			return r;
		}

		public IResult<IBattle, IBattle> ForceMovementBattleEngagement(INode target)
		{
			var allUnits = target.GetAllUnits();
			
			// Get attackers and defenders groups
			// moving units are defending
            // Units will only attack(ambush) if the current posture is NOT Evasion/Includes hidden units
			
			// Occupiers
			var attackers = allUnits.Where(u => !u.IsFriendly())
                                    .Where(u => u.Posture != BattlePosture.EVASION)
									.Where(u => u.CanDoBattleThisTurn()).ToList();
			// Movers
			var defenders = allUnits.Where(u => u.IsFriendly())
                                    .Where(u => !u.IsHiddenFromEnemy()).ToList();

			// No battle here.
			if (!attackers.Any() || !defenders.Any()) return null;

			var r = CreateNewBattle(attackers, defenders, BattleType.FORCED_ENGAGEMENT);
			
			if (r.Status == ResultStatus.FAILURE)
			{
				r.Messages.Add("Can not do battle at selected location.");
				return r;
			}

			TheGame().CurrentBattle = r.Result;

			// So we can handle output to the UI
			SubscribeToCurrentBattleEvents();

			// Enter battle state ...
			TheGame().StateSystem.ChangeState(StateType.BATTLE);

			TheGame().CurrentBattle.DoBattleForcedEngagement();

			// Handle issues with creating and doing battle
			if (r.Status == ResultStatus.FAILURE)
			{
				// Exit battle state ...
				TheGame().StateSystem.ChangeState(StateType.GAME_IN_PLAY);
				TheGame().Renderer.DisplayUserMessage(MessageDisplayType.ERROR, r.Message, null);
				return r;
			}

			target.ResetUnitStackOrder();

			// Exit battle state ...
			TheGame().StateSystem.ChangeState(StateType.GAME_IN_PLAY);

			return r;
		}


	// ---------------------------------------------------------------------------------

		private IResult<IBattle, IBattle> CreateNewBattle(List<IUnit> attackers, List<IUnit> defenders, BattleType battleType)
		{
			var serviceResult = TheGame().JTSServices.GameService.CreateNewBattle(attackers, defenders, battleType);

			var canContinue = serviceResult.Result.CanContinue();

			if (!canContinue.Result)
			{
				serviceResult.Messages.Add(canContinue.Message);
				serviceResult.Status = ResultStatus.FAILURE;
			}

			return serviceResult;
		}


	// Event Handlers
		
		private void SubscribeToCurrentBattleEvents()
		{
			TheGame().CurrentBattle.BattleStarted += On_BattleStarted;
			TheGame().CurrentBattle.BattleEnded += On_BattleEnded;
		}

		protected void On_BattleStarted(object sender, EventArgs e)
		{
			TheGame().CurrentBattle.Render();
		}

		protected void On_BattleEnded(object sender, EventArgs e)
		{
			TheGame().CurrentBattle.RenderOutcome();
		}

	}
}
