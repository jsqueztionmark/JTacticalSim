using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.Data.DTO;
using JTacticalSim.API.Service;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Game;
using JTacticalSim.API.Data;

namespace JTacticalSim.DataContext.Repository
{
	public interface IComponentRepository
	{
		IEnumerable<NodeDTO> GetNodes();
		IResult<INode, INode> SaveNodes(List<INode> nodes);
		IResult<INode, INode> RemoveNodes(List<INode> nodes);
		IResult<INode, INode> UpdateNodes(List<INode> nodes);

		IEnumerable<UnitDTO> GetUnits();
		IResult<IUnit, IUnit> CreateUnit(string name,
										ICoordinate coordinate,
										ICountry country,
										UnitInfo unitInfo,
										ISubNodeLocation subNodeLocation);

		IResult<IUnit, IUnit> SaveUnits(List<IUnit> units);
		IResult<IUnit, IUnit> RemoveUnits(List<IUnit> units);
		IResult<IUnit, IUnit> UpdateUnits(List<IUnit> units);

		IEnumerable<UnitClassDTO> GetUnitClasses();
		IResult<IUnitClass, IUnitClass> SaveUnitClasses(List<IUnitClass> unitClasses);
		IResult<IUnitClass, IUnitClass> RemoveUnitClasses(List<IUnitClass> unitClasses);
		IResult<IUnitClass, IUnitClass> UpdateUnitClasses(List<IUnitClass> unitClasses);

		IEnumerable<UnitTypeDTO> GetUnitTypes();
		IResult<IUnitType, IUnitType> SaveUnitTypes(List<IUnitType> unitTypes);
		IResult<IUnitType, IUnitType> RemoveUnitTypes(List<IUnitType> unitTypes);
		IResult<IUnitType, IUnitType> UpdateUnitTypes(List<IUnitType> unitTypes);

		IEnumerable<UnitBaseTypeDTO> GetUnitBaseTypes();
		IResult<IUnitBaseType, IUnitBaseType> SaveUnitBaseTypes(List<IUnitBaseType> unitBaseTypes);
		IResult<IUnitBaseType, IUnitBaseType> RemoveUnitBaseTypes(List<IUnitBaseType> unitBaseTypes);
		IResult<IUnitBaseType, IUnitBaseType> UpdateUnitBaseTypes(List<IUnitBaseType> unitBaseTypes);

		IEnumerable<UnitGroupTypeDTO> GetUnitGroupTypes();
		IResult<IUnitGroupType, IUnitGroupType> SaveUnitGroupTypes(List<IUnitGroupType> unitGroupTypes);
		IResult<IUnitGroupType, IUnitGroupType> RemoveUnitGroupTypes(List<IUnitGroupType> unitGroupTypes);
		IResult<IUnitGroupType, IUnitGroupType> UpdateUnitGroupTypes(List<IUnitGroupType> unitGroupTypes);

		IEnumerable<UnitGeogTypeDTO> GetUnitGeogTypes();
		IResult<IUnitGeogType, IUnitGeogType> SaveUnitGeogTypes(List<IUnitGeogType> unitGeogTypes);
		IResult<IUnitGeogType, IUnitGeogType> RemoveUnitGeogTypes(List<IUnitGeogType> unitGeogTypes);
		IResult<IUnitGeogType, IUnitGeogType> UpdateUnitGeogTypes(List<IUnitGeogType> unitGeogTypes);

		IEnumerable<UnitTaskTypeDTO> GetUnitTaskTypes();
		IResult<IUnitTaskType, IUnitTaskType> SaveUnitTaskTypes(List<IUnitTaskType> unitTaskTypes);
		IResult<IUnitTaskType, IUnitTaskType> RemoveUnitTaskTypes(List<IUnitTaskType> unitTaskTypes);
		IResult<IUnitTaskType, IUnitTaskType> UpdateUnitTaskTypes(List<IUnitTaskType> unitTaskTypes);

		IEnumerable<MissionTypeDTO> GetMissionTypes();
		IResult<IMissionType, IMissionType> SaveMissionTypes(List<IMissionType> MissionTypes);
		IResult<IMissionType, IMissionType> RemoveMissionTypes(List<IMissionType> MissionTypes);
		IResult<IMissionType, IMissionType> UpdateMissionTypes(List<IMissionType> MissionTypes);

		IEnumerable<FactionDTO> GetFactions();
		IResult<IFaction, IFaction> SaveFactions(List<IFaction> factions);
		IResult<IFaction, IFaction> RemoveFactions(List<IFaction> factions);
		IResult<IFaction, IFaction> UpdateFactions(List<IFaction> factions);

		IEnumerable<CountryDTO> GetCountries();
		IResult<ICountry, ICountry> SaveCountries(List<ICountry> countries);
		IResult<ICountry, ICountry> RemoveCountries(List<ICountry> countries);
		IResult<ICountry, ICountry> UpdateCountries(List<ICountry> countries);

		IEnumerable<TileDTO> GetTiles();
		IResult<ITile, ITile> SaveTiles(List<ITile> tiles);
		IResult<ITile, ITile> RemoveTiles(List<ITile> tiles);
		IResult<ITile, ITile> UpdateTiles(List<ITile> tiles);

		IEnumerable<PlayerDTO> GetPlayers();
		IResult<IPlayer, IPlayer> SavePlayers(List<IPlayer> players);
		IResult<IPlayer, IPlayer> RemovePlayers(List<IPlayer> players);
		IResult<IPlayer, IPlayer> UpdatePlayers(List<IPlayer> players);

		IEnumerable<DemographicDTO> GetDemographics();
		IResult<IDemographic, IDemographic> SaveDemographics(List<IDemographic> demographics);
		IResult<IDemographic, IDemographic> RemoveDemographics(List<IDemographic> demographics);
		IResult<IDemographic, IDemographic> UpdateDemographics(List<IDemographic> demographics);

		IEnumerable<DemographicClassDTO> GetDemographicClasses();
		IResult<IDemographicClass, IDemographicClass> SaveDemographicClasses(List<IDemographicClass> demographicClasses);
		IResult<IDemographicClass, IDemographicClass> RemoveDemographicClasses(List<IDemographicClass> demographicClasses);
		IResult<IDemographicClass, IDemographicClass> UpdateDemographicClasses(List<IDemographicClass> demographicClasses);

		IEnumerable<DemographicTypeDTO> GetDemographicTypes();
		IResult<IDemographicType, IDemographicType> SaveDemographicTypes(List<IDemographicType> demographicTypes);
		IResult<IDemographicType, IDemographicType> RemoveDemographicTypes(List<IDemographicType> demographicTypes);
		IResult<IDemographicType, IDemographicType> UpdateDemographicTypes(List<IDemographicType> demographicTypes);

		IEnumerable<SavedGameDTO> GetSavedGames();
		IResult<ISavedGame, ISavedGame> SaveSavedGame(ISavedGame game);
		IResult<ISavedGame, ISavedGame> RemoveSavedGame(ISavedGame game);
		IResult<ISavedGame, ISavedGame> UpdateSavedGame(ISavedGame game);

		IEnumerable<ScenarioDTO> GetScenarios();
		IResult<IScenario, IScenario> SaveScenario(IScenario scenario);
		IResult<IScenario, IScenario> RemoveScenario(IScenario scenario);
		IResult<IScenario, IScenario> UpdateScenario(IScenario scenario);

		IEnumerable<VictoryConditionTypeDTO> GetVictoryConditionTypes();

		//IEnumerable<IStrategy> GetStrategies();
		//IResult<IStrategy, IStrategy> SaveStrategy(IStrategy strategy);
		//IResult<IStrategy, IStrategy> RemoveStrategy(IStrategy strategy);
		//IResult<IStrategy, IStrategy> UpdateStrategy(IStrategy strategy);

		IEnumerable<ITactic> GetTactics();
		IResult<ITactic, ITactic> SaveTactic(ITactic tactic);
		IResult<ITactic, ITactic> RemoveTactic(ITactic tactic);
		IResult<ITactic, ITactic> UpdateTactic(ITactic tactic);
	}
}
