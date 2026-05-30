using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.GameState;
using NUnit.Framework;

namespace JTacticalSim.Test
{
	[TestFixture]
	public class CommandInterfaceTests : BaseTest
	{
		// GetAvailableCommandsForNode requires a StateSystem — the test harness uses Create(IServiceDependencies)
		// which leaves StateSystem null. We add a minimal GAME_IN_PLAY setup here.
		[SetUp]
		public void InitStateSystem()
		{
			TheGame.StateSystem = new GameStateSystem();
			TheGame.StateSystem.AddState(StateType.GAME_IN_PLAY, new GameInPlayState(TheGame.StateSystem));
			TheGame.StateSystem.ChangeState(StateType.GAME_IN_PLAY);
		}

		// DO_BATTLE command availability
		//
		// Node (4,2,0) — MI_HQ only (class 9/HQ, CanDoBattleThisTurn=true, ValidateTask("Attack")=false)
		// Node (6,2,0) — friendly combat units (A_Infantry, BigGuns, B_Tank); A_Infantry can perform Attack
		// Node (8,4,0) — enemy-only units (A_SandyTanks, Fadaykin_SF, inf_HQ); used as the target node

		[Test]
		public void GetAvailableCommandsForNode_DoBattle_Hidden_When_No_Attack_Capable_Friendly_Units()
		{
			var friendlyNode = TheGame.JTSServices.NodeService.GetNodeAt(4, 2, 0);
			var enemyNode    = TheGame.JTSServices.NodeService.GetNodeAt(8, 4, 0);

			TheGame.GameBoard.SelectedNode  = friendlyNode;
			TheGame.GameBoard.SelectedUnits = friendlyNode.GetAllUnits()
				.Where(u => u.IsFriendly()).ToList();

			var commands = CommandInterface.GetAvailableCommandsForNode(enemyNode, TheGame);

			Assert.IsFalse(commands.Any(c => c.CommandIdentifier == Commands.DO_BATTLE),
				"DO_BATTLE should not appear when no friendly unit at the selected node can perform the Attack task.");
		}

		[Test]
		public void GetAvailableCommandsForNode_DoBattle_Shown_When_Attack_Capable_Friendly_Unit_Present()
		{
			var friendlyNode = TheGame.JTSServices.NodeService.GetNodeAt(6, 2, 0);
			var enemyNode    = TheGame.JTSServices.NodeService.GetNodeAt(8, 4, 0);

			TheGame.GameBoard.SelectedNode  = friendlyNode;
			TheGame.GameBoard.SelectedUnits = friendlyNode.GetAllUnits()
				.Where(u => u.IsFriendly()).ToList();

			var commands = CommandInterface.GetAvailableCommandsForNode(enemyNode, TheGame);

			Assert.IsTrue(commands.Any(c => c.CommandIdentifier == Commands.DO_BATTLE),
				"DO_BATTLE should appear when at least one friendly unit at the selected node can perform the Attack task.");
		}
	}
}
