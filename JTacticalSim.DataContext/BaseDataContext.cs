using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Xml.Linq;
using JTacticalSim.API.Data;
using JTacticalSim.Data.DTO;
using JTacticalSim.API;
using JTacticalSim.API.Game;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.Component.Data;
using JTacticalSim.Component.GameBoard;
using JTacticalSim.Component.Game;
using JTacticalSim.Component.AI;
using JTacticalSim.Component.World;

namespace JTacticalSim.DataContext
{
	public abstract class BaseDataContext : IDataContext
	{
		private readonly DataFileFactory _dataFileFactory = DataFileFactory.Instance;

#region Objects/Accessors

	// Component Objects

		protected BoardDTO _board;
		public BoardDTO Board
		{
			get { return this._board; }
			protected set { this._board = value; }
		}

		protected BasePointValuesDTO _basePointValues;
		public BasePointValuesDTO BasePointValues
		{
			get { return this._basePointValues; }
			protected set { this._basePointValues = value; }
		}

		protected TableInfo _demographicsTable;

		[TableRecognizable(typeof(Demographic), typeof(DemographicDTO))]
		public TableInfo DemographicsTable 
		{
			get { return this._demographicsTable; }
			protected set { this._demographicsTable = value; }
		}

		protected TableInfo _nodesTable;

		[TableRecognizable(typeof(Node), typeof(NodeDTO))]
		public TableInfo NodesTable
		{
			get { return this._nodesTable; }
			protected set { this._nodesTable = value; }
		}
		
		protected TableInfo _unitsTable;

		[TableRecognizable(typeof(Unit), typeof(UnitDTO))]
		public TableInfo UnitsTable 
		{ 
			get { return this._unitsTable; }
			protected set { this._unitsTable = value; }
		}

		protected TableInfo _factionsTable;

		[TableRecognizable(typeof(Faction), typeof(FactionDTO))]
		public TableInfo FactionsTable 
		{ 
			get { return this._factionsTable; }
			protected set { this._factionsTable = value; }
		}

		protected TableInfo _countriesTable;

		[TableRecognizable(typeof(Country), typeof(CountryDTO))]
		public TableInfo CountriesTable
		{
			get { return this._countriesTable; }
			protected set { this._countriesTable = value; }
		}

		protected TableInfo _playersTable;

		[TableRecognizable(typeof(Player), typeof(PlayerDTO))]
		public TableInfo PlayersTable 
		{ 
			get { return this._playersTable; }
			protected set { this._playersTable = value; }
		}

		protected TableInfo _tilesTable;
		
		[TableRecognizable(typeof(Tile), typeof(TileDTO))]
		public TableInfo TilesTable 
		{ 
			get { return this._tilesTable; }
			protected set { this._tilesTable = value; }
		}

		protected TableInfo _savedGames;
		
		[TableRecognizable(typeof(SavedGame), typeof(SavedGameDTO))]
		public TableInfo SavedGames
		{
			get { return this._savedGames; }
			protected set { this._savedGames = value; }
		}

		protected TableInfo _componentSets;

		[TableRecognizable(typeof(ComponentSet), typeof(ComponentSetDTO))]
		public TableInfo ComponentSets
		{
			get { return this._componentSets; }
			protected set { this._componentSets = value; }
		}

		protected TableInfo _scenarios;
		
		[TableRecognizable(typeof(Scenario), typeof(ScenarioDTO))]
		public TableInfo Scenarios
		{
			get { return this._scenarios; }
			protected set { this._scenarios = value; }
		}


	// Data Objects

		protected TableInfo _unitBaseTypesTable;
		
		[TableRecognizable(typeof(UnitBaseType), typeof(UnitBaseTypeDTO))]
		public TableInfo UnitBaseTypesTable
		{
			get { return this._unitBaseTypesTable; }
			protected set { this._unitBaseTypesTable = value; }
		}

		protected TableInfo _unitClassesTable;
		
		[TableRecognizable(typeof(UnitClass), typeof(UnitClassDTO))]
		public TableInfo UnitClassesTable
		{
			get { return this._unitClassesTable; }
			protected set { this._unitClassesTable = value; }
		}

		protected TableInfo _unitBranchesTable;
		
		[TableRecognizable(typeof(UnitBranch), typeof(UnitBranchDTO))]
		public TableInfo UnitBranchesTable
		{
			get { return this._unitBranchesTable; }
			protected set { this._unitBranchesTable = value; }
		}

		protected TableInfo _unitTypesTable;
		
		[TableRecognizable(typeof(UnitType), typeof(UnitTypeDTO))]
		public TableInfo UnitTypesTable
		{
			get { return this._unitTypesTable; }
			protected set { this._unitTypesTable = value; }
		}

		protected TableInfo _unitTaskTypesTable;
		
		[TableRecognizable(typeof(UnitTaskType), typeof(UnitTaskTypeDTO))]
		public TableInfo UnitTaskTypesTable
		{
			get { return this._unitTaskTypesTable; }
			protected set { this._unitTaskTypesTable = value; }
		}

		protected TableInfo _MissionTypesTable;
		
		[TableRecognizable(typeof(MissionType), typeof(MissionTypeDTO))]
		public TableInfo MissionTypesTable
		{
			get { return this._MissionTypesTable; }
			protected set { this._MissionTypesTable = value; }
		}

		protected TableInfo _unitGroupTypesTable;
		
		[TableRecognizable(typeof(UnitGroupType), typeof(UnitGroupTypeDTO))]
		public TableInfo UnitGroupTypesTable
		{
			get { return this._unitGroupTypesTable; }
			protected set { this._unitGroupTypesTable = value; }
		}

		protected TableInfo _unitGeogTypesTable;

		[TableRecognizable(typeof(UnitGeogType), typeof(UnitGeogTypeDTO))]
		public TableInfo UnitGeogTypesTable
		{
			get { return this._unitGeogTypesTable; }
			protected set { this._unitGeogTypesTable = value; }
		}

		protected TableInfo _demographicClassesTable;
		
		[TableRecognizable(typeof(DemographicClass), typeof(DemographicClassDTO))]
		public TableInfo DemographicClassesTable
		{
			get { return this._demographicClassesTable; }
			protected set { this._demographicClassesTable = value; }
		}

		protected TableInfo _demographicTypesTable;
		
		[TableRecognizable(typeof(DemographicType), typeof(DemographicTypeDTO))]
		public TableInfo DemographicTypesTable
		{
			get { return this._demographicTypesTable; }
			protected set { this._demographicTypesTable = value; }
		}

		protected TableInfo _victoryConditionsTypeTable;

		[TableRecognizable(typeof(VictoryConditionType), typeof(VictoryConditionTypeDTO))]
		public TableInfo VictoryConditionTypesTable
		{
			get { return this._victoryConditionsTypeTable; }
			protected set { this._victoryConditionsTypeTable = value; }
		}

	// Lookups

		protected List<dynamic> _unitBaseTypeUnitClassesLookup;
		public List<dynamic> UnitBaseTypeUnitClassesLookup
		{
			get { return this._unitBaseTypeUnitClassesLookup; }
			protected set { this._unitBaseTypeUnitClassesLookup = value; }
		}

		protected List<dynamic> _unitBaseTypeUnitGeogTypesLookup;
		public List<dynamic> UnitBaseTypeUnitGeogTypesLookup
		{
			get { return this._unitBaseTypeUnitGeogTypesLookup; }
			protected set { this._unitBaseTypeUnitGeogTypesLookup = value; }
		}

		protected List<dynamic> _unitGeogTypeDemographicClassesLookup;
		public List<dynamic> UnitGeogTypeDemographicClassesLookup
		{
			get { return this._unitGeogTypeDemographicClassesLookup; }
			protected set { this._unitGeogTypeDemographicClassesLookup = value; }
		}

	// Missions

		protected List<dynamic> _unitGroupTypeUnitTaskTypeLookup;
		public List<dynamic> UnitGroupTypeUnitTaskTypeLookup
		{
			get { return this._unitGroupTypeUnitTaskTypeLookup; }
			protected set { this._unitGroupTypeUnitTaskTypeLookup = value; }
		}

		protected List<dynamic> _unitTaskTypeUnitClassesLookup;
		public List<dynamic> UnitTaskTypeUnitClassesLookup
		{
			get { return this._unitTaskTypeUnitClassesLookup; }
			protected set { this._unitTaskTypeUnitClassesLookup = value; }
		}

		protected List<dynamic> _MissionTypeUnitTaskType;
		public List<dynamic> MissionTypeUnitTaskType
		{
			get { return this._MissionTypeUnitTaskType; }
			protected set { this._MissionTypeUnitTaskType = value; }
		}

	// ----------------------------------------------------

		protected List<dynamic> _allowedUnitTypes;		
		public List<dynamic> AllowedUnitTypes
		{
			get { return this._allowedUnitTypes; }
			protected set { this._allowedUnitTypes = value; }
		}

		protected List<dynamic> _allowedUnitGroupTypes;		
		public List<dynamic> AllowedUnitGroupTypes
		{
			get { return this._allowedUnitGroupTypes; }
			protected set { this._allowedUnitGroupTypes = value; }
		}

		protected List<dynamic> _unitAssignments;
		public List<dynamic> UnitAssignments
		{
			get { return this._unitAssignments; }
			protected set { this._unitAssignments = value; }
		}

		protected List<dynamic> _unitTransports;
		public List<dynamic> UnitTransports
		{
			get { return this._unitTransports; }
			protected set { this._unitTransports = value; }
		}

		protected List<dynamic> _unitBattleEffectiveLookup;
		public List<dynamic> UnitBattleEffectiveLookup
		{
			get { return this._unitBattleEffectiveLookup; }
			protected set { this._unitBattleEffectiveLookup = value; }
		}

		protected List<dynamic> _unitGeogTypeMovementOverrides;
		public List<dynamic> UnitGeogTypeMovementOverrides
		{
			get { return this._unitGeogTypeMovementOverrides; }
			protected set { this._unitGeogTypeMovementOverrides = value; }
		}

		protected List<dynamic> _unitTransportUnitTypeUnitClasses;
		public List<dynamic> UnitTransportUnitTypeUnitClasses
		{
			get { return this._unitTransportUnitTypeUnitClasses; }
			protected set { this._unitTransportUnitTypeUnitClasses = value; }
		}

		protected List<dynamic> _factionVictoryConditions;
		public List<dynamic> FactionVictoryConditions
		{
			get { return this._factionVictoryConditions; }
			protected set { this._factionVictoryConditions = value; }
		}

		protected List<int> _hybridDemographicClasses;
		public List<int> HybridDemographicClasses
		{
			get { return this._hybridDemographicClasses; }
			protected set { this._hybridDemographicClasses = value; }
		}

		protected List<dynamic> _movementHinderanceInDirection;
		public List<dynamic> MovementHinderanceInDirection
		{
			get { return this._movementHinderanceInDirection; }
			protected set { this._movementHinderanceInDirection = value; }
		}

#endregion

		protected BaseDataContext()
		{
			// We don't want saved games refreshed when loading new games
			_savedGames = new TableInfo();
			_scenarios = new TableInfo();
			_componentSets = new TableInfo();
			
			Reset();
		}

		public virtual void Reset()
		{
			//	Component Objects

			_nodesTable = new TableInfo();
			_unitsTable = new TableInfo();
			_factionsTable = new TableInfo();
			_countriesTable = new TableInfo();
			_playersTable = new TableInfo();
			_demographicsTable = new TableInfo();
			_tilesTable = new TableInfo();
			_board = new BoardDTO();
			_basePointValues = new BasePointValuesDTO();

			// Data Objects

			_unitBaseTypesTable = new TableInfo();
			_unitClassesTable = new TableInfo();
			_unitBranchesTable = new TableInfo();
			_unitTypesTable = new TableInfo();
			_unitGroupTypesTable = new TableInfo();
			_unitGeogTypesTable = new TableInfo();
			_unitTaskTypesTable = new TableInfo();
			_MissionTypesTable = new TableInfo();
			_demographicClassesTable = new TableInfo();
			_demographicTypesTable = new TableInfo();
			_victoryConditionsTypeTable = new TableInfo();

			// Lookups

			_unitBaseTypeUnitClassesLookup = new List<dynamic>();
			_unitBaseTypeUnitGeogTypesLookup = new List<dynamic>();
			_unitGeogTypeDemographicClassesLookup = new List<dynamic>();
			_unitTaskTypeUnitClassesLookup = new List<dynamic>();
			_MissionTypeUnitTaskType = new List<dynamic>();
			_unitGroupTypeUnitTaskTypeLookup = new List<dynamic>();
			_unitAssignments = new List<dynamic>();
			_unitTransports = new List<dynamic>();
			_unitGeogTypeMovementOverrides = new List<dynamic>();
			_unitBattleEffectiveLookup = new List<dynamic>();
			_unitTransportUnitTypeUnitClasses = new List<dynamic>();
			_factionVictoryConditions = new List<dynamic>();
			_hybridDemographicClasses = new List<int>();
			_movementHinderanceInDirection = new List<dynamic>();
			_allowedUnitTypes = new List<dynamic>();
			_allowedUnitGroupTypes = new List<dynamic>();
		}

		public abstract IResult<string, string> LoadSavedGameData(string gameDataDirectory, bool IsScenario);
		public abstract IResult<string, string> LoadData(string gameFileDirectory, IComponentSet componentSet, bool IsScenario);
		public abstract IResult<IGameFileCopyable, IGameFileCopyable> SaveData(IGameFileCopyable currentGame);
		public abstract IResult<IGameFileCopyable, IGameFileCopyable> SaveDataAs(IGameFileCopyable current, IGameFileCopyable newGame);
		public abstract IResult<IGameFileCopyable, IGameFileCopyable> RemoveSavedGameData(IGameFileCopyable delGame);

		public void Dispose()
		{}
	}
}
