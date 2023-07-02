using System;
using System.Collections.Generic;
using System.ServiceModel;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.Data;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Service;
using JTacticalSim.Utility;

namespace JTacticalSim.API.Service
{
	[ServiceContract]
	public interface IGameService
	{
		[OperationContract]
		IEnumerable<ISavedGame> GetSavedGames();

		[OperationContract]
		IResult<ISavedGame, ISavedGame> SaveSavedGame(ISavedGame game);

		[OperationContract]
		IResult<ISavedGame, ISavedGame> RemoveSavedGame(ISavedGame game);

		[OperationContract]
		IResult<ISavedGame, ISavedGame> UpdateSavedGame(ISavedGame game);

		[OperationContract]
		ISavedGame GetSavedGameByName(string name);

		[OperationContract]
		IEnumerable<IScenario> GetScenarios();

		[OperationContract]
		IScenario GetScenarioByName(string name);

		[OperationContract]
		IResult<IScenario, IScenario> SaveScenario(IScenario scenario);

		[OperationContract]
		IResult<IScenario, IScenario> RemoveScenario(IScenario scenario);

		[OperationContract]
		IResult<IScenario, IScenario> UpdateScenario(IScenario scenario);


		[OperationContract]
		IEnumerable<IVictoryConditionType> GetVictoryConditionTypes();

		[OperationContract]
		List<IVictoryCondition> GetVictoryConditionsByFaction(IFaction faction);

		[OperationContract]
		IBasePointValues GetGameBasePointValues();

		/// <summary>
		/// Returns the last played game. If none are set, returns the first game in the list.
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		ISavedGame GetLastPlayedGame();

		[OperationContract]
		IEnumerable<IPlayer> GetPlayers();

		[OperationContract]
		IResult<IPlayer, IPlayer> SavePlayers(List<IPlayer> players);

		[OperationContract]
		IResult<IPlayer, IPlayer> RemovePlayers(List<IPlayer> players);

		[OperationContract]
		IResult<IPlayer, IPlayer> UpdatePlayers(List<IPlayer> players);

		[OperationContract]
		IPlayerTurn CreateNewPlayerTurn(IPlayer player);

		[OperationContract]
		IPlayer GetNextPlayer(IPlayer player);

		[OperationContract]
		IPlayer GetFirstPlayer();

		/// <summary>
		/// Returns the player stored as current for a saved game. If none, returns player 0.
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		IPlayer GetCurrentPlayer();

		[OperationContract]
		IBoard CreateGameBoard();

	// Countries

		[OperationContract]
		ICountry GetCountryByID(int id);

		[OperationContract]
		List<ICountry> GetCountriesByFaction(IFaction faction);

		[OperationContract]
		IEnumerable<ICountry> GetCountries();

		[OperationContract]
		IResult<ICountry, ICountry> SaveCountries(List<ICountry> countries);

		[OperationContract]
		IResult<ICountry, ICountry> RemoveCountries(List<ICountry> countries);

		[OperationContract]
		IResult<ICountry, ICountry> UpdateCountries(List<ICountry> countries);


	// Factions

		[OperationContract]
		IResult<IFaction, IFaction> SaveFactions(List<IFaction> factions);

		[OperationContract]
		IResult<IFaction, IFaction> RemoveFactions(List<IFaction> factions);

		[OperationContract]
		IResult<IFaction, IFaction> UpdateFactions(List<IFaction> factions);

		[OperationContract]
		IFaction GetFactionByID(int id);

		[OperationContract]
		IFaction GetFactionByCountry(ICountry country);

		[OperationContract]
		List<IFaction> GetAllFactions();

	// Battle

		[OperationContract]
		IResult<IBattle, IBattle> CreateNewBattle(List<IUnit> attackers,
												List<IUnit> defenders,
												BattleType battleType);
	}
}
