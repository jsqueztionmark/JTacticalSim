using JTacticalSim.API;
using JTacticalSim.API.Game.State;

namespace JTacticalSim.GameState
{
	public sealed class GameInPlayState : BaseGameState
	{

#region Properties and Fields


#endregion

#region Methods

		// Constructors

		public GameInPlayState(IGameStateSystem system)
			: base(system)
		{}

		// Base Game State overrides

		public override void Update(double elapsedTime)
		{
			// Check game victory condition and set to the GameOver state if there is a winner
			foreach(var player in TheGame().GetPlayers())
			{
				if (player.Country.Faction.GameVictoryAchieved())
				{
					TheGame().GameVictor = player.Country.Faction;
					TheGame().StateSystem.ChangeState(StateType.GAME_OVER);
					return;
				}
			}

			TheGame().CommandProcessor.ProcessInput(StateType.GAME_IN_PLAY);
		}

		public override void Render()
		{
			TheGame().Renderer.RenderMainScreen();	
		}

#endregion

	}
}
