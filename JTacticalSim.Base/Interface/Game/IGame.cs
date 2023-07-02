using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Service;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.Data;
using JTacticalSim.API.Game.State;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Cache;
using JTacticalSim.API.Media.Sound;

namespace JTacticalSim.API.Game
{
	public interface IGame : ISoundPlayable
	{
		event EventHandler GameLoading;
		event EventHandler GameLoaded;
		event EventHandler ScenarioLoading;
		event EventHandler ScenarioLoaded;
		event EventHandler MapLoading;
		event EventHandler MapLoaded;

		IGameStateSystem StateSystem { get; set; }
		IRenderer Renderer { get; }
		IBoard GameBoard { get; }
		ICommandProcessor CommandProcessor { get; }
		IBattleHandler BattleHandler { get; }
		IStrategyHandler StrategyHandler { get; }
		IMapModeHandler MapModeHandler { get; set; }
		IZoomHandler ZoomHandler { get; set; }
		IServiceDependencies JTSServices { get; }
		IGameCacheDependencies Cache { get; }

		/// <summary>
		/// The saved game object that is currently loaded for play
		/// </summary>
		ISavedGame LoadedGame { get; }

		/// <summary>
		/// The saved scenario object that is currently loaded for edit.
		/// </summary>
		IScenario LoadedScenario { get; }

		IBasePointValues BasePointValues { get; }

		bool IsConsoleGame { get; }
		bool IsMultiThreaded { get; }

		List<IPlayer> GetPlayers();

		IFaction CurrentPlayerFaction { get; }
		IPlayerTurn CurrentTurn { get; set; }
		IBattle CurrentBattle { get; set; }
		IFaction GameVictor { get; set; }

		/// <summary>
		/// Game creator
		/// </summary>
		/// <param name="services"></param>
		/// <param name="renderer"></param>
		/// <param name="sounds"></param>
		/// <param name="commandProcessor"></param>
		void Create(IServiceDependencies services,
					IRenderer renderer, 
					ISoundSystem sounds,
					ICommandProcessor commandProcessor);

		/// <summary>
		/// Editor and other non-game scope contexts creator
		/// </summary>
		/// <param name="services"></param>
		void Create(IServiceDependencies services);

		void Start();

		/// <summary>
		/// Resets the singleton instance. Required for repeated running of test scripts.
		/// </summary>
		void NullGame();

		IResult<IGameFileCopyable, IGameFileCopyable> Save(IGameFileCopyable currentData);
		IResult<IGameFileCopyable, IGameFileCopyable> SaveAs(IGameFileCopyable currentData, IGameFileCopyable newGame);
		IResult<ISavedGame, ISavedGame> LoadGame(string name);
		IResult<IScenario, IScenario> LoadScenario(string name);
	}
}
