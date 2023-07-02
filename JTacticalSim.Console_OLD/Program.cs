using System;
using JTacticalSim.API;
using JTacticalSim.API.Game;
using JTacticalSim.Service;
using JTacticalSim.Media.Sound;

namespace JTacticalSim
{
	class Program : BaseGameObject
	{
		static void Main(string[] args)
		{
			var p = new Program();
			p.InitializeGame();			
			p.GameLoop(0.0);
		}

		public Program()
			: base(GameObjectType.GAME_BASE)
		{ }

		private void InitializeGame()
		{
			TheGame().Create(	new ServiceDependencies(),
								null,
								new SoundSystem(),
								null);

			// Subscribe to the GameState events
			TheGame().StateSystem.GameStateChanged += On_GameStateChangedHandler;
			// Keep the rendering within the very top level
			// this allows us to use Linqpad and other non-rendering tools
			TheGame().StateSystem.ChangeState(StateType.TITLE_MENU);
		}

		/// <summary>
		/// Mock game loop....
		/// </summary>
		/// <param name="elapsedTime"></param>
		private void GameLoop(double elapsedTime)
		{
			do
			{
				TheGame().StateSystem.Update(elapsedTime);

			} while (true);	
		}

		// Handle GameState change
		// Doing this to capture and render for console, since the console doesn't continually render
		private void On_GameStateChangedHandler(object sender, EventArgs e)
		{
			TheGame().StateSystem.Render();
		}
	}
}
