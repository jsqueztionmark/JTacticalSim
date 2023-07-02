using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceModel;
using JTacticalSim.API.Game;
using JTacticalSim.API.Service;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API;
using JTacticalSim.API.Data;
using JTacticalSim.DataContext;
using JTacticalSim.Component.GameBoard;
using JTacticalSim.Component.Game;
using JTacticalSim.Component.AI;
using JTacticalSim.Utility;
using ctxUtil = JTacticalSim.DataContext.Utility;

namespace JTacticalSim.Service
{
	[ServiceBehavior]
	public sealed class GameService : BaseGameService, IGameService
	{
		static readonly object padlock = new object();

		private static volatile IGameService _instance;
		public static IGameService Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new GameService();
				}

				return _instance;
			}
		}

		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		private GameService()
		{ }

#region Service Methods

		[OperationBehavior]
		public IEnumerable<ISavedGame> GetSavedGames()
		{
			return ComponentRepository.GetSavedGames().Select(sg => sg.ToComponent());
		}

		[OperationBehavior]
		public IResult<ISavedGame, ISavedGame> SaveSavedGame(ISavedGame game)
		{
			return ComponentRepository.SaveSavedGame(game);
		}

		[OperationBehavior]
		public IResult<ISavedGame, ISavedGame> RemoveSavedGame(ISavedGame game)
		{
			return ComponentRepository.RemoveSavedGame(game);
		}

		[OperationBehavior]
		public IResult<ISavedGame, ISavedGame> UpdateSavedGame(ISavedGame game)
		{
			return ComponentRepository.UpdateSavedGame(game);
		}

		[OperationBehavior]
		public ISavedGame GetSavedGameByName(string name)
		{
			var dto = ComponentRepository.GetSavedGames().SingleOrDefault(m => m.Name.ToLowerInvariant() == name.ToLowerInvariant());

			if (dto == null)
				throw new ComponentNotFoundException("No saved game found with name {0}".F(name));

			return dto.ToComponent();
		}

		[OperationBehavior]
		public ISavedGame GetLastPlayedGame()
		{
			var dto = ComponentRepository.GetSavedGames().FirstOrDefault(sg => sg.LastPlayed);

			if (dto == null)
				dto = ComponentRepository.GetSavedGames().FirstOrDefault();

			if (dto == null) throw new ComponentNotFoundException("No saved game found.");

			return dto.ToComponent();
		}


		[OperationBehavior]
		public IEnumerable<IScenario> GetScenarios()
		{
			return ComponentRepository.GetScenarios().Select(s => s.ToComponent());
		}

		[OperationBehavior]
		public IScenario GetScenarioByName(string name)
		{
			var dto = ComponentRepository.GetScenarios().SingleOrDefault(m => m.Name.ToLowerInvariant() == name.ToLowerInvariant());

			if (dto == null)
				throw new ComponentNotFoundException("No scenario found with name {0}".F(name));

			return dto.ToComponent();
		}

		[OperationBehavior]
		public IResult<IScenario, IScenario> SaveScenario(IScenario scenario)
		{
			return ComponentRepository.SaveScenario(scenario);
		}

		[OperationBehavior]
		public IResult<IScenario, IScenario> RemoveScenario(IScenario scenario)
		{
			return ComponentRepository.RemoveScenario(scenario);
		}

		[OperationBehavior]
		public IResult<IScenario, IScenario> UpdateScenario(IScenario scenario)
		{
			return ComponentRepository.UpdateScenario(scenario);
		}


		[OperationBehavior]
		public IEnumerable<IVictoryConditionType> GetVictoryConditionTypes()
		{
			return ComponentRepository.GetVictoryConditionTypes().Select(vc => vc.ToComponent());
		}

		[OperationBehavior]
		public List<IVictoryCondition> GetVictoryConditionsByFaction(IFaction faction)
		{
			var t1 = ComponentRepository.GetVictoryConditionTypes().Select(vc => vc);
			var t2 = DataRepository.GetFactionVictoryConditions().Where(fvc => fvc.FactionID == faction.ID).ToList();

			var retVal = new List<IVictoryCondition>();

			t2.ForEach(fvc =>
			{
				var tmp = new VictoryCondition();
				tmp.VictoryConditionType = t1.Single(vc => vc.ID == (int)fvc.ConditionType).ToComponent();
				tmp.Value = fvc.Value;
				retVal.Add(tmp);
			});


			return retVal;
		}

		[OperationBehavior]
		public IBoard CreateGameBoard()
		{
			return new Board(DataRepository.GetGameboardAttributes());
		}

		[OperationBehavior]
		public IBasePointValues GetGameBasePointValues()
		{
			return DataRepository.GetGameBasePointValues();
		}


		[OperationBehavior]
		public IEnumerable<IPlayer> GetPlayers()
		{
			return ComponentRepository.GetPlayers().Select(p => p.ToComponent());
		}

		[OperationBehavior]
		public IResult<IPlayer, IPlayer> SavePlayers(List<IPlayer> players)
		{
			return ComponentRepository.SavePlayers(players);
		}

		[OperationBehavior]
		public IResult<IPlayer, IPlayer> RemovePlayers(List<IPlayer> players)
		{
			return ComponentRepository.RemovePlayers(players);
		}

		[OperationBehavior]
		public IResult<IPlayer, IPlayer> UpdatePlayers(List<IPlayer> players)
		{
			return ComponentRepository.UpdatePlayers(players);
		}

		[OperationBehavior]
		public IPlayerTurn CreateNewPlayerTurn(IPlayer player)
		{
			return new PlayerTurn(player);
		}

		[OperationBehavior]
		public IPlayer GetFirstPlayer()
		{
			return ComponentRepository.GetPlayers().SingleOrDefault(p => p.ID == 0).ToComponent();
		}

		[OperationBehavior]
		public IPlayer GetNextPlayer(IPlayer player)
		{
			// get the next player based on ID
			var nextPlayer = ComponentRepository.GetPlayers().SingleOrDefault(p => p.ID == (player.ID + 1)).ToComponent();
			return nextPlayer ?? GetFirstPlayer();
		}

		[OperationBehavior]
		public IPlayer GetCurrentPlayer()
		{
			var r = ComponentRepository.GetPlayers().SingleOrDefault(p => p.IsCurrentPlayer);
			return r.ToComponent();
		}


	//Country

		[OperationBehavior]
		public ICountry GetCountryByID(int id)
		{
			var component = ComponentRepository.GetCountries().SingleOrDefault(c => c.ID == id);

			if (component == null)
				throw new ComponentNotFoundException("No country found with id {0}".F(id));

			return component.ToComponent();
		}

		[OperationBehavior]
		public List<ICountry> GetCountriesByFaction(IFaction faction)
		{
			var retVal = ComponentRepository.GetCountries().Where(c => c.Faction.ID == faction.ID).Select(c => c.ToComponent()).ToList();
			return retVal;
		}

		[OperationBehavior]
		public IEnumerable<ICountry> GetCountries()
		{
			return ComponentRepository.GetCountries().Select(c => c.ToComponent());
		}

		[OperationBehavior]
		public IResult<ICountry, ICountry> SaveCountries(List<ICountry> countries)
		{
			return ComponentRepository.SaveCountries(countries);
		}

		[OperationBehavior]
		public IResult<ICountry, ICountry> RemoveCountries(List<ICountry> countries)
		{
			return ComponentRepository.RemoveCountries(countries);
		}

		[OperationBehavior]
		public IResult<ICountry, ICountry> UpdateCountries(List<ICountry> countries)
		{
			return ComponentRepository.UpdateCountries(countries);
		}


	//Faction

		[OperationBehavior]
		public IResult<IFaction, IFaction> SaveFactions(List<IFaction> factions)
		{
			return ComponentRepository.SaveFactions(factions);
		}

		[OperationBehavior]
		public IResult<IFaction, IFaction> RemoveFactions(List<IFaction> factions)
		{
			return ComponentRepository.RemoveFactions(factions);
		}

		[OperationBehavior]
		public IResult<IFaction, IFaction> UpdateFactions(List<IFaction> factions)
		{
			return ComponentRepository.UpdateFactions(factions);
		}

		[OperationBehavior]
		public IFaction GetFactionByID(int id)
		{
			var dto = ComponentRepository.GetFactions().SingleOrDefault(f => f.ID == id);

			if (dto == null)
				throw new ComponentNotFoundException("No faction found with id {0}".F(id));

			return dto.ToComponent();
		}

		[OperationBehavior]
		public IFaction GetFactionByCountry(ICountry country)
		{
			var dto = ComponentRepository.GetFactions().SingleOrDefault(f => f.ID == country.Faction.ID);

			if (dto == null)
				throw new ComponentNotFoundException("No faction found with country {0}".F(country));

			return dto.ToComponent();
		}

		[OperationBehavior]
		public List<IFaction> GetAllFactions()
		{
			return ComponentRepository.GetFactions().Select(f => f.ToComponent()).ToList();
		}

	// Battle

		[OperationBehavior]
		public IResult<IBattle, IBattle> CreateNewBattle(List<IUnit> attackers,
														List<IUnit> defenders,
														BattleType battleType)
		{
			var r = new OperationResult<IBattle, IBattle> { Status = ResultStatus.SUCCESS };
			var battle = new Battle(battleType);

			// Add attackers/defenders - this checks unit tasks rules by relagating the task assignment to the unit
			Action<IUnit> attackerAction = u =>
			{
				lock (attackers)
				{
					u.Attack(battle);
				}
			};


			Action<IUnit> defenderAction = u =>
			{
				lock (defenders)
				{
					u.Defend(battle);
				}
			};

			if (TheGame().IsMultiThreaded)
			{
				Parallel.ForEach(attackers, attackerAction);
				Parallel.ForEach(defenders, defenderAction);
			}
			else
			{
				attackers.ForEach(u => attackerAction(u));
				defenders.ForEach(u => attackerAction(u));
			}


			r.Result = battle;

			return r;
		}
#endregion

	}
}
