using System;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.Data;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Service;
using JTacticalSim.Utility;

namespace JTacticalSim.API.Service
{
	public interface IGameService
	{
		IEnumerable<ISavedGame> GetSavedGames();

		IResult<ISavedGame, ISavedGame> SaveSavedGame(ISavedGame game);

		IResult<ISavedGame, ISavedGame> RemoveSavedGame(ISavedGame game);

		IResult<ISavedGame, ISavedGame> UpdateSavedGame(ISavedGame game);

		ISavedGame GetSavedGameByName(string name);

		IEnumerable<IScenario> GetScenarios();

		IScenario GetScenarioByName(string name);

		IResult<IScenario, IScenario> SaveScenario(IScenario scenario);

		IResult<IScenario, IScenario> RemoveScenario(IScenario scenario);

		IResult<IScenario, IScenario> UpdateScenario(IScenario scenario);


		IEnumerable<IVictoryConditionType> GetVictoryConditionTypes();

		List<IVictoryCondition> GetVictoryConditionsByFaction(IFaction faction);

		IBasePointValues GetGameBasePointValues();

		/// <summary>
		/// Returns the last played game. If none are set, returns the first game in the list.
		/// </summary>
		/// <returns></returns>
		ISavedGame GetLastPlayedGame();

		IEnumerable<IPlayer> GetPlayers();

		IResult<IPlayer, IPlayer> SavePlayers(List<IPlayer> players);

		IResult<IPlayer, IPlayer> RemovePlayers(List<IPlayer> players);

		IResult<IPlayer, IPlayer> UpdatePlayers(List<IPlayer> players);

		IPlayerTurn CreateNewPlayerTurn(IPlayer player);

		IPlayer GetNextPlayer(IPlayer player);

		IPlayer GetFirstPlayer();

		/// <summary>
		/// Returns the player stored as current for a saved game. If none, returns player 0.
		/// </summary>
		/// <returns></returns>
		IPlayer GetCurrentPlayer();

		IBoard CreateGameBoard();

	// Countries

		ICountry GetCountryByID(int id);

		List<ICountry> GetCountriesByFaction(IFaction faction);

		IEnumerable<ICountry> GetCountries();

		IResult<ICountry, ICountry> SaveCountries(List<ICountry> countries);

		IResult<ICountry, ICountry> RemoveCountries(List<ICountry> countries);

		IResult<ICountry, ICountry> UpdateCountries(List<ICountry> countries);


	// Factions

		IResult<IFaction, IFaction> SaveFactions(List<IFaction> factions);

		IResult<IFaction, IFaction> RemoveFactions(List<IFaction> factions);

		IResult<IFaction, IFaction> UpdateFactions(List<IFaction> factions);

		IFaction GetFactionByID(int id);

		IFaction GetFactionByCountry(ICountry country);

		List<IFaction> GetAllFactions();

	// Battle

		IResult<IBattle, IBattle> CreateNewBattle(List<IUnit> attackers,
												List<IUnit> defenders,
												BattleType battleType);
	}
}
