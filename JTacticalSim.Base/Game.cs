using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using JTacticalSim.API.Game.State;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.Service;
using JTacticalSim.API.Game;
using JTacticalSim.API.Data;
using JTacticalSim.AI;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Cache;
using JTacticalSim.API.Media.Sound;
using JTacticalSim.Cache;
using JTacticalSim.GameState;
using JTacticalSim.Utility;

namespace JTacticalSim
{
	public class Game : IGame
	{
		public event EventHandler GameLoading;
		public event EventHandler GameLoaded;
		public event EventHandler ScenarioLoading;
		public event EventHandler ScenarioLoaded;
		public event EventHandler MapLoading;
		public event EventHandler MapLoaded;
		public event EventHandler PlaySoundFinished;

		//Instance of the game
		private static volatile Game _instance;
		static readonly object padlock = new object();		

		public static Game Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new Game();
				}

				return _instance;
			}
		}

		public IGameStateSystem StateSystem { get; set; }
		//public GraphicsDevice gfxDevice { get; private set; }

		public ISavedGame LoadedGame { get; private set; }
		public IScenario LoadedScenario { get; private set; }

		public ISoundSystem Sounds { get; private set; }
		public IRenderer Renderer { get; private set; }
		public IBattleHandler BattleHandler { get; private set; }
		public IStrategyHandler StrategyHandler { get; private set; }
		public IMapModeHandler MapModeHandler { get; set; }
		public IZoomHandler ZoomHandler { get; set; }
		public IBoard GameBoard { get; private set; }
		public ICommandProcessor CommandProcessor { get; private set; }
		public IServiceDependencies JTSServices { get; private set; }
		public IGameCacheDependencies Cache { get { return GameCache.Instance; } }
		public IBasePointValues BasePointValues { get; private set; }

		public bool IsConsoleGame { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["userinterface"] == "console"); } }
		public bool IsMultiThreaded { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["run_multithreaded"]); } }

		public IFaction CurrentPlayerFaction { get { return CurrentTurn.Player.Country.Faction; } }
		public IPlayerTurn CurrentTurn { get; set; }
		public IBattle CurrentBattle { get; set; }
		public IFaction GameVictor { get; set; }

		public Game() {}

		public void Create(IServiceDependencies services,
							IRenderer renderer,
							ISoundSystem sounds,
							ICommandProcessor commandProcessor)
		{
			// Set up modules
			JTSServices			= services;
			Renderer			= renderer;
			Sounds				= sounds;
			CommandProcessor	= commandProcessor;
			BattleHandler		= new BattleHandler();
			StrategyHandler		= new StrategyHandler();
			StateSystem			= new GameStateSystem();
			LoadGameStates();
			LoadGameSounds();

			// Required to select the default game
			LoadSavedGameData(false);
		}

		public void Create(IServiceDependencies services)
		{
			// Set up modules
			JTSServices			= services;
			Renderer			= null;
			CommandProcessor	= null;
			BattleHandler		= null;
			StrategyHandler		= null;
			StateSystem			= null;

			// Required to select the default game
			LoadSavedGameData(true);
		}

		public void NullGame()
		{
			_instance = null;
		}

		public void Start()
		{
			StartNewTurn(JTSServices.GameService.GetCurrentPlayer(), true);
		}

		/// <summary>
		/// Loads saved games and scenarios as required in the loading of other game data
		/// Saved game data must be stored in the datafilepathdefault root directory
		/// </summary>
		private void LoadSavedGameData(bool isScenario)
		{
			var curDrive = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());
			var filePath = "{0}{1}".F(curDrive, ConfigurationManager.AppSettings["datafilepathDefault"]);
			JTSServices.DataService.LoadSavedGameData(filePath, isScenario);
		}

		private void UnloadGame()
		{
			JTSServices.DataService.ResetDataContext();
			Cache.ClearAll();
			LoadedGame = null;
			LoadedScenario = null;
		}


		/// <summary>
		/// Loads in a game by name and sets the current saved game to the newly loaded game
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IResult<ISavedGame, ISavedGame> LoadGame(string name)
		{
			On_GameLoading(this, new EventArgs());

			if (string.IsNullOrEmpty(name))
				return null;

			UnloadGame();

			var r = JTSServices.GenericComponentService.CreateNewComponentResult<ISavedGame, ISavedGame>();
			r.Status = ResultStatus.SUCCESS;
			r.Messages.Add("{0} Loaded".F(name));

			try
			{
				// Set the current saved game to last played false if not first load				
				var lastPlayed = JTSServices.GameService.GetSavedGames().Where(sg => sg.LastPlayed).ToList();

				lastPlayed.ForEach(g =>
					{
						g.LastPlayed = false;
						var sResultB = JTSServices.GameService.UpdateSavedGame(g);
						if (sResultB.Status == ResultStatus.EXCEPTION) Renderer.DisplayUserMessage(MessageDisplayType.ERROR, sResultB.Message, sResultB.ex);
					});								

				LoadedGame = r.Result = JTSServices.GameService.GetSavedGameByName(name);
				LoadedScenario = LoadedGame.Scenario;

				// Set the new current saved game to last played true
				LoadedGame.LastPlayed = true;
				var sResult = JTSServices.GameService.UpdateSavedGame(LoadedGame);
				if (sResult.Status == ResultStatus.EXCEPTION) throw sResult.ex;
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				r.Messages.Add("Could not load {0}".F(name));
				return r;
			}

			try
			{
				var sResult = JTSServices.DataService.LoadData(r.Result.GameFileDirectory, LoadedScenario.ComponentSet, false);
				if (sResult.Status == ResultStatus.EXCEPTION) throw sResult.ex;
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				r.Messages.Add("Could not load data for {0}".F(name));
				return r;
			}

			BasePointValues	= JTSServices.DataService.GetBasePointValues();
			
			// Finally, load board and setup all board-based game resident values
			GameBoard = JTSServices.GameService.CreateGameBoard();			
			SetupBoard();
			SetupUnitInfo();

			// Load Content
			if (Renderer != null)  Renderer.LoadContent();

			On_GameLoaded(this, new EventArgs());

			return r;
		}

		/// <summary>
		/// Loads in a scenario by name
		/// </summary>
		/// <param name="name"></param>
		public IResult<IScenario, IScenario> LoadScenario(string name)
		{
			On_ScenarioLoading(this, new EventArgs());

			JTSServices.DataService.ResetDataContext();

			Cache.ClearAll();

			var r = JTSServices.GenericComponentService.CreateNewComponentResult<IScenario, IScenario>();
			r.Status = ResultStatus.SUCCESS;
			r.Messages.Add("{0} Loaded".F(name));

			try
			{
				r.Result = JTSServices.GameService.GetScenarioByName(name);
				LoadedScenario = r.Result;
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				r.Messages.Add("Could not load {0}".F(name));
				return r;
			}

			try
			{
				JTSServices.DataService.LoadData(r.Result.GameFileDirectory, LoadedScenario.ComponentSet, true);
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				r.Messages.Add("Could not load data for {0}".F(name));
				return r;
			}

			BasePointValues		= JTSServices.DataService.GetBasePointValues();
			
			// Load Content
			if (Renderer != null) Renderer.LoadContent();
			
			// Finally, load board and setup all board-based game resident values
			GameBoard = JTSServices.GameService.CreateGameBoard();			
			SetupBoard();
			SetupUnitInfo();

			On_ScenarioLoaded(this, new EventArgs());

			return r;
		}


		/// <summary>
		/// Take care of any game resident data assessments at load time
		/// </summary>
		private void SetupBoard()
		{
			On_MapLoading(this, new EventArgs());

			CalculateTileInfo();			
			SetCalculatedNodeInfo();
			SetCalculatedBoardInfo();
			SetNodeNeighbors();
		}

		private void SetupUnitInfo()
		{
			SetUnitTypeGeographyTypes();
		}

		private void SetNodeNeighbors()
		{
			var nodes = JTSServices.NodeService.GetAllNodes().ToList();

			Action<INode> nodeAction = n =>
			{
				lock (nodes)
				{
					var allNeighbors = JTSServices.NodeService.GetAllNodesAtDistance(n, 1, false);

					//n.NeighborNodes = allNeighbors
					//						.Where(node => node != null)
					//						.Select(node =>
					//							{
					//								return new NodeNeighborInfo
					//								{
					//									Node = node,
					//									NodeReference = node.UID,
					//									Direction = JTSServices.NodeService.GetNodeDirectionFromNeighborSourceNode(node, n)
					//								};
					//							});

					foreach (var node in allNeighbors)
					{
						if (node == null) continue;

						var neighborInfo = new NodeNeighborInfo
						{
							Node = node,
							NodeReference = node.UID,
							Direction = JTSServices.NodeService.GetNodeDirectionFromNeighborSourceNode(node, n)
						};
						
						n.NeighborNodes.Add(neighborInfo);
					}
				}
			};

			if (IsMultiThreaded)
			{
				Parallel.ForEach(nodes, nodeAction);
			}
			else
			{
				foreach (var n in nodes)
				{
					nodeAction(n);
				}
			}

			JTSServices.NodeService.UpdateNodes(nodes);
		}

		private void SetCalculatedNodeInfo()
		{
			var nodes = JTSServices.NodeService.GetAllNodes().ToList();

			if (!nodes.Any()) return;

			Action<INode> nodeAction = n =>
			{
				lock (nodes)
				{
					var result = JTSServices.RulesService.TileIsChokepoint(n.DefaultTile);
					if (result.Status == ResultStatus.EXCEPTION) throw result.ex;
					n.DefaultTile.IsGeographicChokePoint = result.Result;
				}
			};

			if (IsMultiThreaded)
			{
				Parallel.ForEach(nodes, nodeAction);	
			}
			else
			{
				nodes.ForEach(n =>
					{
						nodeAction(n);
					});
			}

			JTSServices.NodeService.UpdateNodes(nodes);
		}

		private void SetCalculatedBoardInfo()
		{
			//Strategic attributes - Max/Min for all nodes
			var nodes = JTSServices.NodeService.GetAllNodes();

			if (!nodes.Any()) return;

			GameBoard.StrategicValuesAttributes = new GameboardStrategicValueAttributesInfo();

			GameBoard.StrategicValuesAttributes.Defense = new StrategicAttributeInfo
				(
					nodes.Min(n => n.DefaultTile.NetDefenceAdjustment),
					nodes.Max(n => n.DefaultTile.NetDefenceAdjustment)
				);

			GameBoard.StrategicValuesAttributes.Offense = new StrategicAttributeInfo
				(
					nodes.Min(n => n.DefaultTile.NetAttackAdjustment),
					nodes.Max(n => n.DefaultTile.NetAttackAdjustment)
				);

			GameBoard.StrategicValuesAttributes.Stealth = new StrategicAttributeInfo
				(
					nodes.Min(n => n.DefaultTile.NetStealthAdjustment),
					nodes.Max(n => n.DefaultTile.NetStealthAdjustment)
				);

			GameBoard.StrategicValuesAttributes.Movement = new StrategicAttributeInfo
				(
					nodes.Min(n => n.DefaultTile.NetMovementAdjustment),
					nodes.Max(n => n.DefaultTile.NetMovementAdjustment)
				);
			GameBoard.StrategicValuesAttributes.VictoryPoints = new StrategicAttributeInfo
				(
					nodes.Min(n => n.DefaultTile.VictoryPoints),
					nodes.Max(n => n.DefaultTile.VictoryPoints)
				);

		}

		private void CalculateTileInfo()
		{
			var nodes = JTSServices.NodeService.GetAllNodes().ToList();
			var tiles = nodes.Select(n => n.DefaultTile).ToList();

			Action<ITile> tileAction = t =>
			{
				lock (tiles)
				{
					t.ReCalculateTileInfo();
					t.SetTileName();
				}
			};

			if (IsMultiThreaded)
			{
				Parallel.ForEach(tiles, tileAction);
			}
			else
			{
				tiles.ForEach(t =>
				{
					tileAction(t);
				});
			}

			JTSServices.TileService.UpdateTiles(tiles);
		}

		private void StartNewTurn(IPlayer player, bool isGameLoad)
		{
			CurrentTurn = JTSServices.GameService.CreateNewPlayerTurn(player);
			CurrentTurn.TurnEnded += OnCurrentTurnEnded;
			CurrentTurn.TurnStarted += OnCurrentTurnStarted;
			
			// Start the first turn by default
			CurrentTurn.Start(isGameLoad);
		}

		private void SetUnitTypeGeographyTypes()
		{
			var units = JTSServices.UnitService.GetAllUnits();

			foreach (var unit in units)
			{
				// Get geog types
				unit.UnitInfo.UnitType.BaseType.SupportedUnitGeographyTypes = 
					JTSServices.DataService.LookupUnitGeogTypesByBaseTypes(new [] {unit.UnitInfo.UnitType.BaseType.ID});

				// Determine global override value
				unit.UnitInfo.UnitType.HasGlobalMovementOverride =
					JTSServices.RulesService.UnitHasGlobalMovementOverride(unit).Result;
			}
		}


		/// <summary>
		/// Loads in all the game states
		/// </summary>
		private void LoadGameStates()
		{
			StateSystem.AddState(StateType.GAME_IN_PLAY, new GameInPlayState(StateSystem));
			StateSystem.AddState(StateType.AI_IN_PLAY, new AIInPlayState(StateSystem));
			StateSystem.AddState(StateType.BATTLE, new BattleState(StateSystem));
			StateSystem.AddState(StateType.REINFORCE, new ReinforceState(StateSystem));
			StateSystem.AddState(StateType.QUICK_SELECT, new QuickSelectState(StateSystem));
			StateSystem.AddState(StateType.SCENARIO_INFO, new ScenarioInfoState(StateSystem));
			StateSystem.AddState(StateType.HELP, new HelpState(StateSystem));
			StateSystem.AddState(StateType.TITLE_MENU, new TitleScreenState(StateSystem));
			StateSystem.AddState(StateType.SETTINGS_MENU, new GameSettingsState(StateSystem));
			StateSystem.AddState(StateType.GAME_OVER, new GameOverState(StateSystem));
		}

		private void LoadGameSounds()
		{
			Sounds.AddSound(SoundType.CLICK1, "Click_1");
			Sounds.AddSound(SoundType.CLICK2, "Click_2");
		}

		/// <summary>
		/// Saves the current game or scenario
		/// </summary>z
		public IResult<IGameFileCopyable, IGameFileCopyable> Save(IGameFileCopyable currentData)
		{
			var sResult = JTSServices.DataService.SaveData(currentData);
			return sResult;
		}

		/// <summary>
		/// Saves the current game as a new game or scenario
		/// </summary>
		public IResult<IGameFileCopyable, IGameFileCopyable> SaveAs(IGameFileCopyable currentData, IGameFileCopyable newData)
		{
			var sResult = JTSServices.DataService.SaveDataAs(currentData, newData);
			return sResult;
		}

	// Other

		public List<IPlayer> GetPlayers()
		{
			return JTSServices.GameService.GetPlayers().ToList();
		}

#region Sounds

		public void PlaySound(SoundType soundType)
		{
			Sounds.Play(soundType);	
		}

		public void PlaySoundAsync(SoundType soundType)
		{
			Sounds.PlayAsync(soundType);
		}

		public void StopSoundPlayback()
		{
			Sounds.StopPlayback();
		}

		public void StopSoundPlaybackAsync()
		{
			Sounds.StopPlaybackAsync();
		}

#endregion

#region Event Handlers

		public void OnCurrentTurnStarted(object sender, PlayerTurnStartEventArgs e)
		{
			// Update units with refreshed cache values
			var factions = JTSServices.GameService.GetAllFactions();
			var units = JTSServices.UnitService.GetAllUnits(factions);
			JTSServices.UnitService.UpdateUnits(units.ToList());

			// Update game players
			JTSServices.GameService.UpdatePlayers(GetPlayers());

			// Refresh move caches for new turn if not game loading
			// we need this stuff to happen BEFORE the board renders in the case of
			// a console game
			if (!e.IsGameLoad)
			{
				// Refresh the board
				GameBoard.Refresh();
				GameBoard.SelectedUnits.Clear();
				Cache.TurnMoveCache.Refresh();
				Cache.TurnStrategyCache.Clean();
			}

			// Make sure that there is a selected current node
			// and that that it's in the visible area
			if (GameBoard.SelectedNode == null)
				JTSServices.NodeService.GetNodeAt(0, 0, 0).Select();

			// Split processing - human vs AI player
			if (StateSystem != null)
				StateSystem.ChangeState((CurrentTurn.Player.IsAIPlayer) ? StateType.AI_IN_PLAY : StateType.GAME_IN_PLAY);

			// Since the execution handles displaying the messages to the user
			// we don't want those messages showing until AFTER the screen refreshes
			if (!e.IsGameLoad)
			{
				StrategyHandler.ExecuteStrategiesForPlayer(CurrentTurn.Player);
			}

		}

		public void OnCurrentTurnEnded(object sender, EventArgs e)
		{
			// Keep the currently selected node
			var resetNode = GameBoard.SelectedNode;
			GameBoard.ClearSelectedItems(false);
			GameBoard.SelectedNode = resetNode;
			StartNewTurn(JTSServices.GameService.GetNextPlayer(CurrentTurn.Player), false);
		}

		public void On_GameLoading(object sender, EventArgs e)
		{
			if (GameLoading != null) GameLoading(this, e);
		}

		public void On_GameLoaded(object sender, EventArgs e)
		{
			if (GameLoaded != null) GameLoaded(this, e);
		}

		public void On_ScenarioLoading(object sender, EventArgs e)
		{
			if (ScenarioLoading != null) ScenarioLoading(this, e);
		}

		public void On_ScenarioLoaded(object sender, EventArgs e)
		{
			if (ScenarioLoaded != null) ScenarioLoaded(this, e);
		}

		public void On_MapLoading(object sender, EventArgs e)
		{
			if (MapLoading != null) MapLoading(sender, e);
		}

		public void On_MapLoaded(object sender, EventArgs e)
		{
			if (MapLoaded != null) MapLoaded(sender, e);
		}

		private void On_PlaySoundFinished(object sender, EventArgs e)
		{
			if (PlaySoundFinished != null) PlaySoundFinished(this, e);
		}


#endregion

#region IDisposable


#endregion

	}
}
