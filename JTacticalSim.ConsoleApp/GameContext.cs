using System;
using System.Linq;
using System.Runtime.InteropServices;
using JTacticalSim.API;
using JTacticalSim.API.Game;
using JTacticalSim.Service;
using JTacticalSim.Media.Sound;
using ConsoleControls;
using System.Collections.Generic;

namespace JTacticalSim.ConsoleApp
{
	public class GameContext : BaseGameObject
	{
		static readonly object padlock = new object();

		private static volatile GameContext _instance;
		public static GameContext Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new GameContext();
				}

				return _instance;
			}
		}

		private GameContext()
			: base(GameObjectType.GAME_BASE)
		{
			GLOBAL_CONTROL_VIEW_STACK = new Stack<IConsoleControl>();
		}

		public Stack<IConsoleControl> GLOBAL_CONTROL_VIEW_STACK;

		public void InitializeGame(bool startUp = false)
		{
			TheGame().NullGame();
			TheGame().Create(new ServiceDependencies(),
							new ConsoleRenderer(),
							new SoundSystem(), 
							new ConsoleCommandProcessor());

			// Subscribe to the GameState events
			TheGame().StateSystem.GameStateChanged += On_GameStateChangedHandler;
			// Keep the rendering within the very top level
			// this allows us to use Linqpad and other non-rendering tools
			if (startUp)
				TheGame().StateSystem.ChangeState(StateType.TITLE_MENU);
		}

		/// <summary>
		/// Main Game Loop...
		/// </summary>
		/// <param name="elapsedTime"></param>
		public void GameLoop(double elapsedTime)
		{
			do
			{
				TheGame().StateSystem.Update(elapsedTime);	
			} while (true);	
		}

		// Handle GameState change
		// Doing this to capture and render for console, since the console doesn't continually render
		public void On_GameStateChangedHandler(object sender, EventArgs e)
		{
			Console.ResetColor();
			TheGame().StateSystem.Render();
		}
	}
}
