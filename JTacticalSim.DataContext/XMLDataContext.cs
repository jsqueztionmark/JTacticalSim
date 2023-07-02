using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Dynamic;
using System.IO;
using System.Transactions;
using JTacticalSim.API;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.Component.Data;
using JTacticalSim.Data.DTO;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.Component.AI;
using JTacticalSim.Utility;
using JTacticalSim.DataContext.Repository;

namespace JTacticalSim.DataContext
{
	/// <summary>
	/// File based data context
	/// </summary>
	public sealed class XMLDataContext : BaseDataContext
	{
		private readonly DataFileFactory _dataFileFactory = DataFileFactory.Instance;

		private static volatile IDataContext _instance;
		static readonly object padlock = new object();

		public static IDataContext Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new XMLDataContext();
				}

				return _instance;
			}
		}
		
		// Initialize Context
		private XMLDataContext()
		{}

#region Save

		public override IResult<IGameFileCopyable, IGameFileCopyable> SaveData(IGameFileCopyable currentData)
		{
			var r = new DataResult<IGameFileCopyable, IGameFileCopyable>{Status = ResultStatus.SUCCESS, Result = currentData};

			using (var txn = new TransactionScope())
			{			
				// 1. Create a new file handler for the current game file directory
				IDataFileHandler<XDocument> dataFileHandler;

				try
				{
					dataFileHandler = DataFileHandlerFactory.Instance.GetDataFileHandler(currentData.IsScenario, _dataFileFactory.GetDataFiles<XDocument>(currentData.GameFileDirectory, null, currentData.IsScenario));				
				}
				catch (Exception ex)
				{
					r.Status = ResultStatus.EXCEPTION;
					r.ex = ex;
					r.Messages.Add("Game data not saved. DataFileHandler could not be created.");
					return r;
				}


				// 2. Backup Current Files
				
				if (dataFileHandler.FileDirectoryHasFiles())
				{
					try
					{
						BackupCurrentGameFiles(dataFileHandler);
					}
					catch (Exception ex)
					{
						r.Status = ResultStatus.EXCEPTION;
						r.ex = ex;
						r.Messages.Add("Game data not saved. Could not backup current game files.");
						return r;
					}
				}
				

				// 3. Save data
				try
				{
					SaveGameData(dataFileHandler);
					SaveSavedGameFile(dataFileHandler);
				}
				catch (Exception ex)
				{
					r.Status = ResultStatus.EXCEPTION;
					r.ex = ex;
					r.Messages.Add("Game data not saved.");
					return r;
				}

				txn.Complete();
			}

			r.Messages.Add("Game data saved.");
			return r;
		}

		public override IResult<IGameFileCopyable, IGameFileCopyable> SaveDataAs(IGameFileCopyable currentData, IGameFileCopyable newData)
		{
			var r = new DataResult<IGameFileCopyable, IGameFileCopyable>{Status = ResultStatus.SUCCESS, Result = newData};

			using (var txn = new TransactionScope())
			{	
				// 1. Create a new file handler for the current and new file directories
				IDataFileHandler<XDocument> currentDataFileHandler;
				IDataFileHandler<XDocument> newDataFileHandler;

				try
				{
					currentDataFileHandler = DataFileHandlerFactory.Instance.GetDataFileHandler(currentData.IsScenario, _dataFileFactory.GetDataFiles<XDocument>(currentData.GameFileDirectory, null, currentData.IsScenario)); ;
					newDataFileHandler = DataFileHandlerFactory.Instance.GetDataFileHandler(newData.IsScenario, _dataFileFactory.GetDataFiles<XDocument>(newData.GameFileDirectory, null, newData.IsScenario));
				}
				catch (Exception ex)
				{
					r.Status = ResultStatus.EXCEPTION;
					r.ex = ex;
					r.Messages.Add("Game data not saved. DataFileHandlers could not be created.");
					return r;
				}


				// 2. Verify file directory. Create if it does not exists.
				if (!newDataFileHandler.FileDirectoryExists())
				{
					try
					{
						newDataFileHandler.CreateNewFileDirectory();
					}
					catch (Exception ex)
					{
						r.Status = ResultStatus.EXCEPTION;
						r.ex = ex;
						r.Messages.Add("Game data not saved. Could not create new game directory.");
						return r;
					}
				}

				// 3. Copy current files to new directory
				var result = (newData.IsScenario) 
								? currentDataFileHandler.CopyFiles(newDataFileHandler.DataFiles.ScenarioDirectory)
								: currentDataFileHandler.CopyFiles(newDataFileHandler.DataFiles.GameSaveDirectory);				

				if (result.Status == ResultStatus.EXCEPTION)
				{
					r.Status = ResultStatus.EXCEPTION;
					r.ex = result.ex;
					r.Messages.Add("Game data not saved. {0}.".F(result.Message));
					return r;
				}	
				
				// 3. Save data
				try
				{
					SaveSavedGameFile(newDataFileHandler);
				}
				catch (Exception ex)
				{
					r.Status = ResultStatus.EXCEPTION;
					r.ex = ex;
					r.Messages.Add("Game data file not saved.");
					return r;
				}

				txn.Complete();
			}

			r.Messages.Add("Game data saved.");
			return r;
		}

		public override IResult<IGameFileCopyable, IGameFileCopyable> RemoveSavedGameData(IGameFileCopyable delData)
		{
			var r = new DataResult<IGameFileCopyable, IGameFileCopyable>{Status = ResultStatus.SUCCESS, Result = delData};

			using (var txn = new TransactionScope())
			{

				// 1. Create a new file handler for the game to be deleted.
				IDataFileHandler<XDocument> delDataFileHandler;

				try
				{
					delDataFileHandler = DataFileHandlerFactory.Instance.GetDataFileHandler(delData.IsScenario, _dataFileFactory.GetDataFiles<XDocument>(delData.GameFileDirectory, null, delData.IsScenario));
				}
				catch (Exception ex)
				{
					r.Status = ResultStatus.EXCEPTION;
					r.ex = ex;
					r.Messages.Add("Game data not saved. DataFileHandlers could not be created.");
					return r;
				}

				// 2. Verify game directory. Create if it does not exists.
				if (delDataFileHandler.FileRootDirectoryExists())
				{
					try
					{
						delDataFileHandler.DeleteFileDirectory();
					}
					catch (Exception ex)
					{
						r.Status = ResultStatus.EXCEPTION;
						r.ex = ex;
						r.Messages.Add("Game data not deleted. Could not delete game directory.");
						return r;
					}
				}

				// 3. Save data
				try
				{
					SaveSavedGameFile(delDataFileHandler);
				}
				catch (Exception ex)
				{
					r.Status = ResultStatus.EXCEPTION;
					r.ex = ex;
					r.Messages.Add("Game data file not saved.");
					return r;
				}

				txn.Complete();
			}

			r.Messages.Add("Game data removed.");
			return r;
		}

		/// <summary>
		/// Makes a backup of files for the current data either Scenario or Game
		/// </summary>
		/// <param name="dataFileHandler"></param>
		private void BackupCurrentGameFiles(IDataFileHandler<XDocument> dataFileHandler)
		{
			dataFileHandler.DataFiles.ScenarioFilePaths.ForEach(path =>
			{
				File.Delete("{0}.BAK".F(path));
				File.Copy(path, "{0}.BAK".F(path));
			});	

			// Back up the Saved Games file
			File.Delete("{0}.BAK".F(dataFileHandler.DataFiles.SavedGameDataFilePath));
			File.Copy(dataFileHandler.DataFiles.SavedGameDataFilePath, "{0}.BAK".F(dataFileHandler.DataFiles.SavedGameDataFilePath));
		}

		private void SaveGameData(IDataFileHandler<XDocument> dataFileHandler)
		{
			SaveBoardDataFile(dataFileHandler);
			SaveUnitDataFile(dataFileHandler);
			SaveGameDataFile(dataFileHandler);
			SaveCacheFile(dataFileHandler);
		}

		private void SaveBoardDataFile(IDataFileHandler<XDocument> dataFileHandler)
		{
			// Clear all nodes
			var nodes = dataFileHandler.DataFiles.BoardDataFile.Descendants("Nodes").Single();
			nodes.RemoveAll();

			var attributes = dataFileHandler.DataFiles.BoardDataFile.Descendants("Attributes").Single();

			// Clear the board
			var board = dataFileHandler.DataFiles.BoardDataFile.Descendants("GameBoard").Single();
			board.RemoveAll();

			// Update attributes
			attributes.Attribute("Title").SetValue(Board.Title);
			attributes.Attribute("Width").SetValue(Board.Width);
			attributes.Attribute("Height").SetValue(Board.Height);
			attributes.Attribute("CellSize").SetValue(Board.CellSize);
			attributes.Attribute("CellMaxUnits").SetValue(Board.CellMaxUnits);

			// Create Nodes for save
			foreach (var n in NodesTable.Records.Cast<NodeDTO>())
			{
				nodes.Add(n.ToXML());
			}

			board.Add(attributes);
			board.Add(nodes);
			
			// Save back file
			board.Document.Save(dataFileHandler.DataFiles.BoardDataFilePath);
		}

		private void SaveUnitDataFile(IDataFileHandler<XDocument> dataFileHandler)
		{
			// Remove all existing units
			var units = dataFileHandler.DataFiles.UnitDataFile.Descendants("Units").Single();
			units.RemoveAll();

			// Create units for save
			foreach (var u in UnitsTable.Records.Cast<UnitDTO>())
			{
				units.Add(u.ToXML());				
			}

			units.Document.Save(dataFileHandler.DataFiles.UnitDataFilePath);
		}

		private void SaveGameDataFile(IDataFileHandler<XDocument> dataFileHandler)
		{
			// Remove all existing GameData
			var gameData = dataFileHandler.DataFiles.GameDataFile.Descendants("GameData").Single();
			gameData.RemoveAll();

			// Create Factions for save
			XElement fs = new  XElement("Factions");
			foreach (var f in FactionsTable.Records.Cast<FactionDTO>())
			{
				fs.Add(f.ToXML());
			}
			gameData.Add(fs);

			// Create Countries for save
			XElement cs = new  XElement("Countries");
			foreach (var c in CountriesTable.Records.Cast<CountryDTO>())
			{
				cs.Add(c.ToXML());
			}
			gameData.Add(cs);

			// Create Players for save
			XElement ps = new  XElement("Players");
			foreach (var p in PlayersTable.Records.Cast<PlayerDTO>())
			{
				ps.Add(p.ToXML());
			}
			gameData.Add(ps);

			gameData.Add(new XComment("Conditions for winning the game for a faction. Winning is an OR proposition"));
			XElement fvcs = new XElement("FactionVictoryConditions");
			foreach (var fvc in FactionVictoryConditions)
			{
				XElement vc = new XElement("VictoryCondition");
				vc.Add(new XAttribute("Faction", fvc.FactionID));
				vc.Add(new XAttribute("ConditionType", fvc.ConditionType));
				vc.Add(new XAttribute("Value", fvc.Value));

				fvcs.Add(vc);
			}
			gameData.Add(fvcs);

			gameData.Add(new XComment("Unit Assignments"));
			XElement uas = new XElement("UnitAssignments");
			foreach (var a in UnitAssignments)
			{
				XElement ua = new XElement("UnitAssignment");
				ua.Add(new XAttribute("Unit", a.Unit));
				ua.Add(new XAttribute("AssignedToUnit", a.AssignedToUnit));

				uas.Add(ua);
			}
			gameData.Add(uas);


			gameData.Add(new XComment("Unit Transports"));
			XElement uts = new XElement("UnitTransports");
			foreach (var a in UnitTransports)
			{
				XElement ua = new XElement("UnitTransport");
				ua.Add(new XAttribute("Unit", a.Unit));
				ua.Add(new XAttribute("TransportUnit", a.TransportUnit));

				uts.Add(ua);
			}
			gameData.Add(uts);

			gameData.Add(new XComment("Allowed Unit Types"));
			XElement aut = new XElement("AllowedUnitTypes");
			foreach (var o in AllowedUnitTypes)
			{
				XElement ut = new XElement("UnitType");
				ut.Add(new XAttribute("Country", o.Country));
				ut.Add(new XAttribute("ID", o.UnitType));
				aut.Add(ut);
			}
			gameData.Add(aut);

			gameData.Add(new XComment("Allowed Unit Group Types"));
			XElement augt = new XElement("AllowedUnitGroupTypes");
			foreach (var o in AllowedUnitGroupTypes)
			{
				XElement ugt = new XElement("UnitGroupType");
				ugt.Add(new XAttribute("ID", o.UnitGroupType));
				augt.Add(ugt);
			}
			gameData.Add(augt);

			
			gameData.Document.Save(dataFileHandler.DataFiles.GameDataFilePath);
		}

		private void SaveSavedGameFile(IDataFileHandler<XDocument> dataFileHandler)
		{
			// Remove all existing GameData
			var gameData = dataFileHandler.DataFiles.SavedGameDataFile.Descendants("SavedGames").Single();
			gameData.RemoveAll();

			foreach(var game in SavedGames.Records.Cast<SavedGameDTO>())
			{
				gameData.Add(game.ToXML());
			}

			gameData.Document.Save(dataFileHandler.DataFiles.SavedGameDataFilePath);
		}
		
		private void SaveScenarioFile(IDataFileHandler<XDocument> dataFileHandler)
		{
			// Remove all existing scenario data
			var scenarioData = dataFileHandler.DataFiles.ScenarioDataFile.Descendants("Scenarios").Single();
			scenarioData.RemoveAll();

			foreach (var scenario in Scenarios.Records.Cast<ScenarioDTO>())
			{
				scenarioData.Add(scenario.ToXML());
			}

			scenarioData.Document.Save(dataFileHandler.DataFiles.ScenarioDataFilePath);
		}

		private void SaveCacheFile(IDataFileHandler<XDocument> dataFileHandler)
		{
			SaveStrategyCache(dataFileHandler);
		}

		private void SaveStrategyCache(IDataFileHandler<XDocument> dataFileHandler)
		{
			// Remove all existing StrategyCache data
			var strategyData = dataFileHandler.DataFiles.CacheDataFile.Descendants("StrategyCache").Single();
			strategyData.RemoveAll();

			var tactics = Cache.StrategyCache.Instance.GetAll().ToList();
			tactics.ForEach(t => strategyData.Add(t.ToXml()));
			strategyData.Document.Save(dataFileHandler.DataFiles.CacheDataFilePath);
		}

#endregion

#region Load

		// Pre-load of saved game/scenario data
		public override IResult<string, string> LoadSavedGameData(string gameDataDirectory, bool IsScenario)
		{
			var r = new DataResult<string, string>{Status = ResultStatus.SUCCESS, Result = gameDataDirectory};

			// We'll always store this in XML in the GameData root
			try
			{
				IDataFileInfo<XDocument> dataFiles = _dataFileFactory.GetDataFiles<XDocument>(gameDataDirectory, null, IsScenario);

				LoadComponentSets(dataFiles.ComponentSetDataFile);
				LoadVictoryConditionTypes(dataFiles.ComponentSetDataFile);
				LoadScenarios(dataFiles.ScenarioDataFile);
				LoadSavedGames(dataFiles.SavedGameDataFile);
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				r.Messages.Add("Saved game data not loaded.");
			}

			r.Messages.Add("Saved game data loaded.");
			return r;
		}

		private void LoadComponentSets(XDocument componentDataFile)
		{
			// Get base type with properties
			var componentSets = GetBaseComponentDTOs<ComponentSetDTO>(componentDataFile.Descendants("ComponentSet")).ToList();

			// Set type specific properties
			componentSets.ForEach(o =>
				{
					o.Item1.Path = o.Item2.Attribute("Path").Value;
				});

			// Add to context
			_componentSets.Records = componentSets.Cast<dynamic>().Select(p => p.Item1).ToList();
		}

		private void LoadScenarios(XDocument scenarioDataFile)
		{
			// Get base type with properties
			var scenarios = GetBaseComponentDTOs<ScenarioDTO>(scenarioDataFile.Descendants("Scenario")).ToList();

			// Set type specific properties
			scenarios.ForEach(o =>
				{
					o.Item1.GameFileDirectory = o.Item2.Attribute("GameFileDirectory").Value;
					o.Item1.Author = o.Item2.Attribute("Author").Value;
					o.Item1.ComponentSet = Convert.ToInt32(o.Item2.Attribute("ComponentSet").Value);

					// Get Scenario specific data
					var dataFiles = _dataFileFactory.GetDataFiles<XDocument>(o.Item1.GameFileDirectory, null, true);
					if (dataFiles == null)
						return;

					LoadFactionVictoryConditions(dataFiles.GameDataFile);
					LoadFactions(dataFiles.GameDataFile);
					LoadCountries(dataFiles.GameDataFile);

					var synopsisFile = dataFiles.SynopsisFile;
					o.Item1.Synopsis = synopsisFile.Descendants("Synopsis").FirstOrDefault().Value;

					using (var ctx = DataFactory.Instance.GetDataContext())
					{
						o.Item1.Countries = ctx.CountriesTable.Records.Cast<CountryDTO>().ToList();
						o.Item1.Factions = ctx.FactionsTable.Records.Cast<FactionDTO>().ToList();
						o.Item1.VictoryConditions = ctx.FactionVictoryConditions.Cast<VictoryConditionDTO>().ToList();
						o.Item1.VictoryConditions.ForEach(vc =>
							{
								vc.VictoryConditionType =
									ctx.VictoryConditionTypesTable.Records.SingleOrDefault(type => type.ID == vc.ConditionType);
								vc.Faction =
									ctx.FactionsTable.Records.SingleOrDefault(f => f.ID == vc.FactionID);
							});
					}
				});

			// Add to context
			_scenarios.Records = scenarios.Cast<dynamic>().Select(p => p.Item1).ToList();
		}

		private void LoadSavedGames(XDocument savedGameDataFile)
		{
			// Get base type with properties
			var savedGames = GetBaseComponentDTOs<SavedGameDTO>(savedGameDataFile.Descendants("SavedGame")).ToList();

			// Set type specific properties
			savedGames.ForEach(o =>
				{
					o.Item1.LastPlayed = Convert.ToBoolean(o.Item2.Attribute("LastPlayed").Value);
					o.Item1.GameFileDirectory = o.Item2.Attribute("GameFileDirectory").Value;
					o.Item1.Scenario = Convert.ToInt32(o.Item2.Attribute("Scenario").Value);
				});

			// Add to context
			_savedGames.Records = savedGames.Cast<dynamic>().Select(p => p.Item1).ToList();
		}


		// Loading of specific game data
		public override IResult<string, string> LoadData(string FileDirectory, IComponentSet componentSet, bool IsScenario)
		{
			var r = new DataResult<string, string>{Status = ResultStatus.SUCCESS, Result = FileDirectory};

			try
			{
				IDataFileHandler<XDocument> dataFileHandler = DataFileHandlerFactory.Instance.GetDataFileHandler<XDocument>(IsScenario, _dataFileFactory.GetDataFiles<XDocument>(FileDirectory, componentSet, IsScenario));

				// ~~~~~~~~~~~~~~~~~~~~  Load up context
				// (Order is important for dependant objects)

				LoadComponentData(dataFileHandler.DataFiles.ComponentDataFile, dataFileHandler.DataFiles.ComponentSetDataFile);
				LoadLookupData(dataFileHandler.DataFiles.LookupDataFile);
				LoadGamedata(dataFileHandler.DataFiles.GameDataFile,
								dataFileHandler.DataFiles.BoardDataFile,
								dataFileHandler.DataFiles.UnitDataFile,
								dataFileHandler.DataFiles.ComponentDataFile);
				LoadCacheData(dataFileHandler.DataFiles.CacheDataFile);

				// We need to make sure we have the last played game set correctly in the data file for saved games
				SaveSavedGameFile(dataFileHandler);
				SaveScenarioFile(dataFileHandler);
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				r.Messages.Add("Game data not loaded.");
			}

			r.Messages.Add("Game data loaded.");
			return r;
			
		}

		/// <summary>
		/// Load all static code data tables into context
		/// </summary>
		/// <param name="componentDataFile"></param>
		private void LoadComponentData(XDocument componentDataFile, XDocument componentSetDataFile)
		{
			LoadBasePointValues(componentDataFile);
			LoadUnitBaseTypes(componentDataFile);
			LoadUnitGroupTypes(componentDataFile);
			LoadUnitGeogTypes(componentDataFile);
			LoadUnitBranches(componentDataFile);
			LoadUnitTypes(componentDataFile);
			LoadUnitClasses(componentDataFile);
			LoadDemographicClasses(componentDataFile);
			LoadDemographicTypes(componentDataFile);
			LoadDemographics(componentDataFile);
			LoadVictoryConditionTypes(componentSetDataFile);
			LoadMissionData(componentDataFile);
		}

		/// <summary>
		/// Load all the lookup tables for code game code data into context
		/// </summary>
		/// <param name="lookupDataFile"></param>
		private void LoadLookupData(XDocument lookupDataFile)
		{
			LoadUnitBaseTypeUnitClasses(lookupDataFile);
			LoadUnitBaseTypeUnitGeogTypes(lookupDataFile);
			LoadUnitGeogTypeDemographicClasses(lookupDataFile);
			LoadUnitGroupTypeUnitTaskTypes(lookupDataFile);
			LoadMissionTypeUnitTaskTypes(lookupDataFile);
			LoadUnitTaskTypeUnitClasses(lookupDataFile);
			LoadUnitGeogTypeMovementOverrides(lookupDataFile);
			LoadUnitTypeUnitGeogTypeBattleEffectives(lookupDataFile);
			LoadUnitTransportUnitTypeUnitClasses(lookupDataFile);
			LoadHybridDemographicClasses(lookupDataFile);
			LoadMovementHinderancesInDirection(lookupDataFile);
		}

		private void LoadGamedata(	XDocument gameDataFile, 
									XDocument boardDataFile, 
									XDocument unitDataFile, 
									XDocument componentDataFile)
		{
			LoadBoard(boardDataFile);
			LoadUnits(unitDataFile);
			LoadFactions(gameDataFile);
			LoadCountries(gameDataFile);
			LoadPlayers(gameDataFile);
			LoadUnitAssignments(gameDataFile);
			LoadUnitTransports(gameDataFile);
			LoadFactionVictoryConditions(gameDataFile);
			LoadAllowedUnitTypes(gameDataFile);
			LoadAllowedUnitGroupTypes(gameDataFile);
		}

		private void LoadCacheData(XDocument cacheDataFile)
		{
			LoadStrategyCache(cacheDataFile);
		}

		private void LoadBasePointValues(XDocument staticDataFile)
		{
			var data = staticDataFile.Descendants("BasePointValues").Single();

			var basePointValues = new BasePointValuesDTO
					(
						Convert.ToInt32(data.Attribute("Movement").Value),
						Convert.ToInt32(data.Attribute("CombatRoll").Value),
						Convert.ToInt32(data.Attribute("CombatBase").Value),
						Convert.ToInt32(data.Attribute("StealthRoll").Value),
						Convert.ToInt32(data.Attribute("StealthBase").Value),
						Convert.ToDouble(data.Attribute("HiddenStealthThreshhold").Value),
						Convert.ToInt32(data.Attribute("MedicalSupportBase").Value),
						Convert.ToInt32(data.Attribute("WeightBase").Value),
						Convert.ToInt32(data.Attribute("CostBase").Value),
						Convert.ToInt32(data.Attribute("AIBaseRoll").Value),
						Convert.ToInt32(data.Attribute("AIAggressiveness").Value),
						Convert.ToInt32(data.Attribute("AIDefensiveness").Value),
						Convert.ToInt32(data.Attribute("AIIntelligence").Value),
						Convert.ToInt32(data.Attribute("MeterDistanceBase").Value),
						Convert.ToInt32(data.Attribute("ReinforcementCalcBaseCountry").Value),
						Convert.ToInt32(data.Attribute("ReinforcementCalcBaseFaction").Value),
						Convert.ToDouble(data.Attribute("ReinforcementCalcBaseVP").Value),
						Convert.ToDouble(data.Attribute("HQBonus").Value),
						Convert.ToDouble(data.Attribute("NotSuppliedPenalty").Value),
						Convert.ToInt32(data.Attribute("MaxSupplyDistance").Value),
						Convert.ToDouble(data.Attribute("TargetAttachedUnitBonus").Value),
						Convert.ToDouble(data.Attribute("TargetMedicalUnitBonus").Value),
						Convert.ToDouble(data.Attribute("TargetSupplyUnitBonus").Value),
						Convert.ToDouble(data.Attribute("BattlePostureBonus").Value)
					);
	
			_basePointValues = basePointValues;
		}

		private void LoadUnitBaseTypes(XDocument staticDataFile)
		{
			//Get base type with properties
			var unitBaseTypes = GetBaseComponentDTOs<UnitBaseTypeDTO>(staticDataFile.Descendants("UnitBaseType")).ToList();

			unitBaseTypes.ForEach(o =>
				{
					o.Item1.CanReceiveMedicalSupport = Convert.ToBoolean(o.Item2.Attribute("CanReceiveMedicalSupport").Value);
					o.Item1.CanBeSupplied = Convert.ToBoolean(o.Item2.Attribute("CanBeSupplied").Value);
					o.Item1.NuclearAffected = Convert.ToBoolean(o.Item2.Attribute("NuclearAffected").Value);
					o.Item1.OutOfFuelMoveResultMessage = o.Item2.Attribute("OutOfFuelMoveResultMessage").Value;
				});

			// Add to context
			_unitBaseTypesTable.Records = unitBaseTypes.Cast<dynamic>().Select(p => p.Item1).ToList();
		}

		private void LoadVictoryConditionTypes(XDocument staticDataFile)
		{
			var victoryConditionTypes = GetBaseComponentDTOs<VictoryConditionTypeDTO>(staticDataFile.Descendants("Condition")).ToList();
			_victoryConditionsTypeTable.Records = victoryConditionTypes.Cast<dynamic>().Select(p => p.Item1).ToList();
		}

		private void LoadUnitGroupTypes(XDocument staticDataFile)
		{
			var unitGroupTypes = GetBaseComponentDTOs<UnitGroupTypeDTO>(staticDataFile.Descendants("UnitGroupType")).ToList();

			// Set type specific properties
			unitGroupTypes.ForEach(o =>
				{
					o.Item1.TextDisplayZ1 = o.Item2.Attribute("TextDisplayZ1").Value;
					o.Item1.TextDisplayZ2 = o.Item2.Attribute("TextDisplayZ2").Value;
					o.Item1.TextDisplayZ3 = o.Item2.Attribute("TextDisplayZ3").Value;
					o.Item1.TextDisplayZ4 = o.Item2.Attribute("TextDisplayZ4").Value;
					o.Item1.Level = Convert.ToInt32(o.Item2.Attribute("Level").Value);
				});

			// Add to context
			_unitGroupTypesTable.Records = unitGroupTypes.Cast<dynamic>().Select(p => p.Item1).ToList();
		}

		private void LoadUnitGeogTypes(XDocument staticDataFile)
		{
			var unitGeogTypes = GetBaseComponentDTOs<UnitGeogTypeDTO>(staticDataFile.Descendants("UnitGeogType")).ToList();

			// Add to context
			_unitGeogTypesTable.Records = unitGeogTypes.Cast<dynamic>().Select(p => p.Item1).ToList();
		}

		private void LoadUnitTypes(XDocument staticDataFile)
		{
			// Get base type with properties
			var unitTypes = GetBaseComponentDTOs<UnitTypeDTO>(staticDataFile.Descendants("UnitType")).ToList();

			// Set type specific properties
			unitTypes.ForEach(o =>
				{
					o.Item1.TextDisplayZ1 = o.Item2.Attribute("TextDisplayZ1").Value;
					o.Item1.TextDisplayZ2 = o.Item2.Attribute("TextDisplayZ2").Value;
					o.Item1.TextDisplayZ3 = o.Item2.Attribute("TextDisplayZ3").Value;
					o.Item1.TextDisplayZ4 = o.Item2.Attribute("TextDisplayZ4").Value;
					o.Item1.UnitBaseTypeID = Convert.ToInt32(o.Item2.Attribute("UnitBaseType").Value);
					o.Item1.UnitBranchID = Convert.ToInt32(o.Item2.Attribute("UnitBranch").Value);
					o.Item1.FuelConsumer = Convert.ToBoolean(o.Item2.Attribute("FuelConsumer").Value);
					o.Item1.Nuclear = Convert.ToBoolean(o.Item2.Attribute("Nuclear").Value);
					o.Item1.FuelRange = Convert.ToInt32(o.Item2.Attribute("FuelRange").Value);
					o.Item1.Sound_Fire = o.Item2.Attribute("Sound_Fire").Value;
					o.Item1.Sound_Move = o.Item2.Attribute("Sound_Move").Value;
				});

			// Load stat modifier data
			unitTypes.ForEach(o => LoadStatModifierData(o.Item2, o.Item1));

			// Add to context
			_unitTypesTable.Records = unitTypes.Cast<dynamic>().Select(p => p.Item1).ToList();
		}

		private void LoadUnitClasses(XDocument staticDataFile)
		{
			// Get base type with properties
			var unitClasses = GetBaseComponentDTOs<UnitClassDTO>(staticDataFile.Descendants("UnitClass")).ToList();

			// Set type specific properties
			unitClasses.ForEach(o =>
				{
					o.Item1.TextDisplayZ1 = o.Item2.Attribute("TextDisplayZ1").Value;
					o.Item1.TextDisplayZ2 = o.Item2.Attribute("TextDisplayZ2").Value;
					o.Item1.TextDisplayZ3 = o.Item2.Attribute("TextDisplayZ3").Value;
					o.Item1.TextDisplayZ4 = o.Item2.Attribute("TextDisplayZ4").Value;
					o.Item1.Sound_Fire = o.Item2.Attribute("Sound_Fire").Value;
					o.Item1.Sound_Move = o.Item2.Attribute("Sound_Move").Value;
				});

			// Load stat modifier data
			unitClasses.ForEach(o => LoadStatModifierData(o.Item2, o.Item1));

			// Add to context
			_unitClassesTable.Records = unitClasses.Cast<dynamic>().Select(p => p.Item1).ToList();
		}

		private void LoadUnitBranches(XDocument staticDataFile)
		{
			// Get base type with properties
			var unitBranches = GetBaseComponentDTOs<UnitBranchDTO>(staticDataFile.Descendants("UnitBranch")).ToList();

			// Add to context
			_unitBranchesTable.Records = unitBranches.Cast<dynamic>().Select(p => p.Item1).ToList();
		}


		private void LoadMissionData(XDocument staticDataFile)
		{
			// UnitTaskTypes
			var unitTaskTypes = GetBaseComponentDTOs<UnitTaskTypeDTO>(staticDataFile.Descendants("UnitTaskType")).ToList();
			_unitTaskTypesTable.Records = unitTaskTypes.Cast<dynamic>().Select(p => p.Item1).ToList();

			//Missions Types
			var MissionTypes = GetBaseComponentDTOs<MissionTypeDTO>(staticDataFile.Descendants("MissionType")).ToList();

			MissionTypes.ForEach(o =>
				{
					o.Item1.Priority = Convert.ToInt32(o.Item2.Attribute("Priority").Value);
					o.Item1.TurnOrder = Convert.ToInt32(o.Item2.Attribute("TurnOrder").Value);
					o.Item1.CanceledByMove = Convert.ToBoolean(o.Item2.Attribute("CanceledByMove").Value);
				});

			_MissionTypesTable.Records = MissionTypes.Cast<dynamic>().Select(mo => mo.Item1).ToList();
		}

		private void LoadDemographicClasses(XDocument staticDataFile)
		{
			// Get base type with properties
			var demographicClasses = GetBaseComponentDTOs<DemographicClassDTO>(staticDataFile.Descendants("DemographicClass")).ToList();

			var defaultBuildInfo = new BuildInfo
									{
										Buildable = false, Destroyable = false, BuildTurns = 0, DestroyTurns = 0
									};

			var buildInfos = staticDataFile.Descendants("BuildInfo")
										   .Select(o => new Tuple<int, BuildInfo>
										   (Convert.ToInt32(o.Attribute("DemographicClass").Value), new BuildInfo
										   													 {
										   														 Buildable = Convert.ToBoolean(o.Attribute("Buildable").Value),
										   														 Destroyable = Convert.ToBoolean(o.Attribute("Destroyable").Value),
										   														 BuildTurns = Convert.ToInt32(o.Attribute("BuildTurns").Value),
										   														 DestroyTurns = Convert.ToInt32(o.Attribute("DestroyTurns").Value)
										   													 }
										    ));


			// Set type specific properties
			demographicClasses.ForEach(o =>
				{
					var bi = buildInfos.Where(p => p.Item1 == o.Item1.ID).Select(p => p.Item2).SingleOrDefault();
					o.Item1.BuildInfo = bi ?? defaultBuildInfo;
					o.Item1.TextDisplayZ1 = o.Item2.Attribute("TextDisplayZ1").Value;
					o.Item1.TextDisplayZ2 = o.Item2.Attribute("TextDisplayZ2").Value;
					o.Item1.TextDisplayZ3 = o.Item2.Attribute("TextDisplayZ3").Value;
					o.Item1.TextDisplayZ4 = o.Item2.Attribute("TextDisplayZ4").Value;
					o.Item1.MovementHinderanceConfigured = Convert.ToBoolean(o.Item2.Attribute("MovementHinderanceConfigured").Value);
					o.Item1.DemographicType = Convert.ToInt32(o.Item2.Attribute("DemographicType").Value);
				});

			// Load stat modifier data
			demographicClasses.ForEach(o => LoadStatModifierData(o.Item2, o.Item1));


			// Add to context
			_demographicClassesTable.Records = demographicClasses.Cast<dynamic>().Select(p => p.Item1).ToList();
		}

		private void LoadDemographicTypes(XDocument staticDataFile)
		{
			// Get base type with properties
			var demographicTypes = GetBaseComponentDTOs<DemographicTypeDTO>(staticDataFile.Descendants("DemographicType")).ToList();

			demographicTypes.ForEach(dt =>
				{
					dt.Item1.DisplayOrder = Convert.ToInt32(dt.Item2.Attribute("DisplayOrder").Value);
				});

			// Add to context
			_demographicTypesTable.Records = demographicTypes.Cast<dynamic>().Select(p => p.Item1).ToList();
		}

		private void LoadDemographics(XDocument componentDataFile)
		{
			var demographicsList = GetBaseComponentDTOs<DemographicDTO>(componentDataFile.Descendants("Demographic")).ToList();

			// Set type specific properties
			demographicsList.ForEach(o =>
				{
					o.Item1.DemographicClass = Convert.ToInt32(o.Item2.Attribute("DemographicClass").Value);
					o.Item1.ProvidesMedical = Convert.ToBoolean(o.Item2.Attribute("ProvidesMedical").Value);
					o.Item1.ProvidesSupply = Convert.ToBoolean(o.Item2.Attribute("ProvidesSupply").Value);
					o.Item1.Orientation = (o.Item2.Attribute("Orientation") != null) ? o.Item2.Attribute("Orientation").Value : null;
					o.Item1.Value = o.Item2.Attribute("Value").Value;
				});

			// Add to context
			_demographicsTable.Records = demographicsList.Cast<dynamic>().Select(p => p.Item1).ToList();
		}


		private void LoadFactions(XDocument gameDataFile)
		{
			// Get base type with properties
			var factionList = GetBaseComponentDTOs<FactionDTO>(gameDataFile.Descendants("Faction")).ToList();

			// Add to context
			_factionsTable.Records = factionList.Cast<dynamic>().Select(p => p.Item1).ToList();
		}

		private void LoadCountries(XDocument gameDataFile)
		{
			// Get base type with properties
			var countryList = GetBaseComponentDTOs<CountryDTO>(gameDataFile.Descendants("Country")).ToList();

			// Set type specific properties
			countryList.ForEach(o =>
				{
					o.Item1.Faction = _factionsTable.Records.SingleOrDefault(f => f.ID == Convert.ToInt32(o.Item2.Attribute("Faction").Value));
                    o.Item1.Color = o.Item2.Attribute("Color").Value;
					o.Item1.BGColor = o.Item2.Attribute("BGColor").Value;
					o.Item1.TextDisplayColor = o.Item2.Attribute("TextDisplayColor").Value;
					o.Item1.FlagBGColor = o.Item2.Attribute("FlagBGColor").Value;
					o.Item1.FlagColorA = o.Item2.Attribute("FlagColorA").Value;
					o.Item1.FlagColorB = o.Item2.Attribute("FlagColorB").Value;
                    o.Item1.FlagDisplayTextA = o.Item2.Attribute("FlagDisplayTextA").Value;
					o.Item1.FlagDisplayTextB = o.Item2.Attribute("FlagDisplayTextB").Value;
				});


			// Add to context
			_countriesTable.Records = countryList.Cast<dynamic>().Select(p => p.Item1).ToList();
		}

		private void LoadPlayers(XDocument gameDataFile)
		{
			// Get base type with properties
			var playerList = GetBaseComponentDTOs<PlayerDTO>(gameDataFile.Descendants("Player")).ToList();

			// Set type specific properties
			playerList.ForEach(o =>
				{
					var units = o.Item2.Descendants("Unit").ToList();
					units.ForEach(u => o.Item1.UnplacedReinforcements.Add(Convert.ToInt32(u.Attribute("ID").Value)));
					o.Item1.Country = Convert.ToInt32(o.Item2.Attribute("Country").Value);
					o.Item1.IsCurrentPlayer = (o.Item2.Attribute("IsCurrentPlayer") != null) && Convert.ToBoolean(o.Item2.Attribute(("IsCurrentPlayer")).Value);
					o.Item1.IsAIPlayer = (o.Item2.Attribute("IsAIPlayer") != null) && Convert.ToBoolean(o.Item2.Attribute(("IsAIPlayer")).Value);
					o.Item1.ReinforcementPoints = Convert.ToInt32(o.Item2.Descendants("TrackedValues").SingleOrDefault().Attribute("ReinforcementPoints").Value);
					o.Item1.NuclearCharges = Convert.ToInt32(o.Item2.Descendants("TrackedValues").SingleOrDefault().Attribute("NuclearCharges").Value);
				});


			// Add to context
			_playersTable.Records = playerList.Cast<dynamic>().Select(p => p.Item1).ToList();
		}

		private void LoadBoard(XDocument boardDataFile)
		{
			// Ensure IDs
			var id = 0;

			// Board Data
			var dimension = boardDataFile.Descendants("Attributes").SingleOrDefault();
			
			var board = new BoardDTO
					{
						Title = dimension.Attribute("Title").Value,
						Height = Convert.ToInt32(dimension.Attribute("Height").Value),
						Width = Convert.ToInt32(dimension.Attribute("Width").Value),
						DrawHeight = Convert.ToInt32(dimension.Attribute("DrawHeight").Value),
						DrawWidth = Convert.ToInt32(dimension.Attribute("DrawWidth").Value),
						CellSize = Convert.ToInt32(dimension.Attribute("CellSize").Value),
						CellMeters = Convert.ToInt32(dimension.Attribute("CellMeters").Value),
						CellMaxUnits = Convert.ToInt32(dimension.Attribute("CellMaxUnits").Value),
					};			

			var nodeList = boardDataFile.Descendants("Node");
			var nodesToSave = new List<dynamic>();
			var tilesToSave = new List<dynamic>();

			foreach (var e in nodeList)
			{
				var c = new CoordinateDTO
					{
						X = Convert.ToInt32(e.Descendants("Coordinate").SingleOrDefault().Attribute("X").Value),
						Y = Convert.ToInt32(e.Descendants("Coordinate").SingleOrDefault().Attribute("Y").Value),
						Z = Convert.ToInt32(e.Descendants("Coordinate").SingleOrDefault().Attribute("Z").Value)
					};
				

				var dgs = e.Descendants("Demographics").Descendants("Demographic")
									.Select(d => 
										new DemographicDTO
											{
												ID = Convert.ToInt32(d.Attribute("ID").Value), 
												InstanceName = (d.Attribute("InstanceName") != null) ? d.Attribute("InstanceName").Value : "",
												Orientation = (d.Attribute("Orientation") != null) ? d.Attribute("Orientation").Value : ""
											});								
						

				var v = Convert.ToInt32(e.Descendants("Tile").SingleOrDefault().Attribute("VictoryPoints").Value);
				var isPrimeTarget = (e.Descendants("Tile").SingleOrDefault().Attribute("IsPrimeTarget") != null) 
									? Convert.ToBoolean(e.Descendants("Tile").SingleOrDefault().Attribute("IsPrimeTarget").Value)
									: false;
				var tileID = e.Descendants("Tile").SingleOrDefault().Attribute("ID");

				var name = string.Empty;

				if (e.Descendants("Tile").SingleOrDefault().Attribute("Name") != null)
					 name = e.Descendants("Tile").SingleOrDefault().Attribute("Name").Value;

				var t = new TileDTO
							{
								ID = (tileID == null) ? id : Convert.ToInt32(tileID.Value),
								UID = Guid.NewGuid(),
								Name = name,
								VictoryPoints = v,
								IsPrimeTarget = isPrimeTarget,
								Location = c,
								Demographics = dgs
							};

				tilesToSave.Add(t);

				var nodeID = e.Attribute("ID");
				
				var n = new NodeDTO
							{
								ID = (nodeID == null) ? id : Convert.ToInt32(nodeID.Value),
								Name = "Node - {0}".F(c.ToString()),
								UID = Guid.NewGuid(),
								Location = c,
								Country = Convert.ToInt32(e.Descendants("Country").SingleOrDefault().Attribute("ID").Value),
								DefaultTile = t
							};


				nodesToSave.Add(n);

				id++;

			}
				
			// Add to context
			_tilesTable.Records = tilesToSave;
			_nodesTable.Records = nodesToSave;
			_board = board;
		}

		private void LoadUnits(XDocument unitDataFile)
		{
			var unitUnitList = unitDataFile.Descendants("Unit");
			var unitsToSave = new List<dynamic>();

			foreach (var e in unitUnitList)
			{

				// No Location indicates unplaced unit
				var	c = (e.Descendants("Coordinate").SingleOrDefault() == null) ? null : new CoordinateDTO
																							{
																								X = Convert.ToInt32(e.Descendants("Coordinate").SingleOrDefault().Attribute("X").Value),
																								Y = Convert.ToInt32(e.Descendants("Coordinate").SingleOrDefault().Attribute("Y").Value),
																								Z = Convert.ToInt32(e.Descendants("Coordinate").SingleOrDefault().Attribute("Z").Value)
																							};
				
				

				var m = new UnitDTO
							{
								ID = Convert.ToInt32(e.Attribute("ID").Value),
								UID = Guid.NewGuid(),
								Name = e.Attribute("Name").Value,
								SubNodeLocation = Convert.ToInt32(e.Descendants("SubNodeLocation").SingleOrDefault().Attribute("Value").Value),
								Location = c,
								Description = e.Attribute("Description").Value,
								Country = Convert.ToInt32(e.Descendants("Country").SingleOrDefault().Attribute("ID").Value),
								StackOrder = Convert.ToInt32(e.Attribute("StackOrder").Value),
								UnitClass = Convert.ToInt32(e.Descendants("UnitInfo").SingleOrDefault().Attribute("UnitClass").Value),
								UnitType = Convert.ToInt32(e.Descendants("UnitInfo").SingleOrDefault().Attribute("UnitType").Value),
								UnitGroupType = Convert.ToInt32(e.Descendants("UnitInfo").SingleOrDefault().Attribute("UnitGroupType").Value),
								CurrentFuelRange = (e.Attribute("CurrentFuelRange") != null) 
												? Convert.ToInt32(e.Attribute("CurrentFuelRange").Value) 
												: (int?)null,
								Posture = (e.Descendants("Posture").SingleOrDefault() != null) 
										? Convert.ToInt32(e.Descendants("Posture").SingleOrDefault().Attribute("ID").Value) 
										: (int?)null,
								
							};

				// Movement stats if this is a saved game
				// Data saved mid-turn
				if (e.Descendants("MovementStats").SingleOrDefault() != null)
				{
					m.CurrentHasPerformedAction =
						Convert.ToBoolean(e.Descendants("MovementStats").SingleOrDefault().Attribute("CurrentHasPerformedAction").Value);
					m.CurrentMovementPoints =
						Convert.ToInt32(e.Descendants("MovementStats").SingleOrDefault().Attribute("CurrentMovementPoints").Value);
					m.CurrentRemoteFirePoints =
						Convert.ToInt32(e.Descendants("MovementStats").SingleOrDefault().Attribute("CurrentRemoteFirePoints").Value);
				}
					
				unitsToSave.Add(m);
			}	


			_unitsTable.Records = unitsToSave;
		}

		private void LoadAllowedUnitTypes(XDocument gameDataFile)
		{
			var lookupTmp = gameDataFile.Descendants("UnitType");
			var lookupListTmp = new List<dynamic>();

			foreach (var e in lookupTmp)
			{
				dynamic d = new ExpandoObject();
				d.UnitType = Convert.ToInt32(e.Attribute("ID").Value);
				d.Country = Convert.ToInt32(e.Attribute("Country").Value);

				lookupListTmp.Add(d);
			}

			_allowedUnitTypes = lookupListTmp;
		}

		private void LoadAllowedUnitGroupTypes(XDocument gameDataFile)
		{
			var lookupTmp = gameDataFile.Descendants("UnitGroupType");
			var lookupListTmp = new List<dynamic>();

			foreach (var e in lookupTmp)
			{
				dynamic d = new ExpandoObject();
				d.UnitGroupType = Convert.ToInt32(e.Attribute("ID").Value);

				lookupListTmp.Add(d);
			}

			_allowedUnitGroupTypes = lookupListTmp;
		}

		private void LoadUnitAssignments(XDocument gameDataFile)
		{
			var lookupTmp = gameDataFile.Descendants("UnitAssignment");
			var lookupListTmp = new List<dynamic>();

			foreach (var e in lookupTmp)
			{
				dynamic d = new ExpandoObject();
				d.Unit = Convert.ToInt32(e.Attribute("Unit").Value);
				d.AssignedToUnit = Convert.ToInt32(e.Attribute("AssignedToUnit").Value);

				lookupListTmp.Add(d);
			}

			_unitAssignments = lookupListTmp;
		}

		private void LoadUnitTransports(XDocument gameDataFile)
		{
			var lookupTmp = gameDataFile.Descendants("UnitTransport");
			var lookupListTmp = new List<dynamic>();

			foreach (var e in lookupTmp)
			{
				dynamic d = new ExpandoObject();
				d.Unit = Convert.ToInt32(e.Attribute("Unit").Value);
				d.TransportUnit = Convert.ToInt32(e.Attribute("TransportUnit").Value);

				lookupListTmp.Add(d);
			}

			_unitTransports = lookupListTmp;
		}

		private void LoadUnitBaseTypeUnitClasses(XDocument lookupDataFile)
		{
			var lookupTmp = lookupDataFile.Descendants("UnitBaseTypeUnitClass");
			var lookupListTmp = new List<dynamic>();

			foreach (var e in lookupTmp)
			{
				dynamic d = new ExpandoObject();
				d.UnitBaseType = Convert.ToInt32(e.Attribute("UnitBaseType").Value);
				d.UnitClass = Convert.ToInt32(e.Attribute("UnitClass").Value);

				lookupListTmp.Add(d);
			}

			_unitBaseTypeUnitClassesLookup = lookupListTmp;
		}

		private void LoadUnitBaseTypeUnitGeogTypes(XDocument lookupDataFile)
		{
			var lookupTmp = lookupDataFile.Descendants("UnitBaseTypeUnitGeogType");
			var lookupListTmp = new List<dynamic>();

			foreach (var e in lookupTmp)
			{
				dynamic d = new ExpandoObject();
				d.UnitBaseType = Convert.ToInt32(e.Attribute("UnitBaseType").Value);
				d.UnitGeogType = Convert.ToInt32(e.Attribute("UnitGeogType").Value);

				lookupListTmp.Add(d);
			}

			_unitBaseTypeUnitGeogTypesLookup = lookupListTmp;
		}

		private void LoadHybridDemographicClasses(XDocument lookupDataFile)
		{
			var lookupTmp = lookupDataFile.Descendants("HybridDemographicClass");
			var lookupListTmp = new List<int>();

			foreach (var e in lookupTmp)
			{
				lookupListTmp.Add(Convert.ToInt32(e.Attribute("DemographicClass").Value));
			}

			_hybridDemographicClasses = lookupListTmp;
		}

		private void LoadMovementHinderancesInDirection(XDocument lookupDataFile)
		{
			var lookupTmp = lookupDataFile.Descendants("MovementHinderance");
			var lookupListTmp = new List<dynamic>();

			foreach (var e in lookupTmp)
			{
				dynamic d = new ExpandoObject();
				d.DemographicClass = Convert.ToInt32(e.Attribute("DemographicClass").Value);
				d.UnitGeogType = Convert.ToInt32(e.Attribute("UnitGeogType").Value);
				d.Direction = (Direction)Convert.ToInt32(e.Attribute("Direction").Value);

				lookupListTmp.Add(d);
			}

			_movementHinderanceInDirection = lookupListTmp;
		}

		private void LoadUnitGeogTypeDemographicClasses(XDocument lookupDataFile)
		{
			var lookupTmp = lookupDataFile.Descendants("UnitGeogTypeDemographicClass");
			var lookupListTmp = new List<dynamic>();

			foreach (var e in lookupTmp)
			{
				dynamic d = new ExpandoObject();
				d.UnitGeogType = Convert.ToInt32(e.Attribute("UnitGeogType").Value);
				d.DemographicClass = Convert.ToInt32(e.Attribute("DemographicClass").Value);

				lookupListTmp.Add(d);
			}

			_unitGeogTypeDemographicClassesLookup = lookupListTmp;
		}

		private void LoadUnitGroupTypeUnitTaskTypes(XDocument lookupDataFile)
		{
			var lookupTmp = lookupDataFile.Descendants("UnitGroupTypeUnitTaskType");
			var lookupListTmp = new List<dynamic>();

			foreach (var e in lookupTmp)
			{
				dynamic d = new ExpandoObject();
				d.UnitGroupType = Convert.ToInt32(e.Attribute("UnitGroupType").Value);
				d.UnitTask = Convert.ToInt32(e.Attribute("UnitTaskType").Value);

				lookupListTmp.Add(d);
			}

			_unitGroupTypeUnitTaskTypeLookup = lookupListTmp;
		}

		private void LoadMissionTypeUnitTaskTypes(XDocument lookupDataFile)
		{
			var lookupTmp = lookupDataFile.Descendants("MissionTypeUnitTaskType");
			var lookupListTmp = new List<dynamic>();

			foreach (var e in lookupTmp)
			{
				dynamic d = new ExpandoObject();
				d.MissionType = Convert.ToInt32(e.Attribute("MissionType").Value);
				d.UnitTaskType = Convert.ToInt32(e.Attribute("UnitTaskType").Value);
				d.StepOrder = Convert.ToInt32(e.Attribute("StepOrder").Value);

				lookupListTmp.Add(d);
			}

			_MissionTypeUnitTaskType = lookupListTmp;
		}

		private void LoadUnitTaskTypeUnitClasses(XDocument lookupDataFile)
		{
			var lookupTmp = lookupDataFile.Descendants("UnitTaskTypeUnitClass");
			var lookupListTmp = new List<dynamic>();

			foreach (var e in lookupTmp)
			{
				dynamic d = new ExpandoObject();
				d.UnitClass = Convert.ToInt32(e.Attribute("UnitClass").Value);
				d.UnitTask = Convert.ToInt32(e.Attribute("UnitTaskType").Value);

				lookupListTmp.Add(d);
			}

			_unitTaskTypeUnitClassesLookup = lookupListTmp;
		}

		private void LoadUnitGeogTypeMovementOverrides(XDocument lookupDataFile)
		{
			var lookupTmp = lookupDataFile.Descendants("UnitGeogTypeMovementOverrides").First().Descendants("Override");
			var lookupListTmp = new List<dynamic>();

			foreach (var e in lookupTmp)
			{
				dynamic d = new ExpandoObject();
				d.UnitGeogType = Convert.ToInt32(e.Attribute("UnitGeogType").Value);
				d.Geography = Convert.ToInt32(e.Attribute("DemographicClassA").Value);
				d.Infrastructure = Convert.ToInt32(e.Attribute("DemographicClassB").Value);

				lookupListTmp.Add(d);
			}

			_unitGeogTypeMovementOverrides = lookupListTmp;
		}

		private void LoadUnitTypeUnitGeogTypeBattleEffectives(XDocument lookupDataFile)
		{
			var lookupTmp = lookupDataFile.Descendants("UnitTypeUnitGeogTypeBattleEffective").First().Descendants("BattleEffective");
			var lookupListTmp = new List<dynamic>();

			foreach (var e in lookupTmp)
			{
				dynamic d = new ExpandoObject();
				d.UnitType = Convert.ToInt32(e.Attribute("UnitType").Value);
				d.UnitClass = Convert.ToInt32(e.Attribute("UnitClass").Value);
				d.UnitGeogType = Convert.ToInt32(e.Attribute("UnitGeogType").Value);

				lookupListTmp.Add(d);
			}

			_unitBattleEffectiveLookup = lookupListTmp;
		}

		private void LoadUnitTransportUnitTypeUnitClasses(XDocument lookupDataFile)
		{
			var lookupTmp = lookupDataFile.Descendants("UnitTransportUnitTypeUnitClasses").First().Descendants("TransportEffective");
			var lookupListTmp = new List<dynamic>();

			foreach (var e in lookupTmp)
			{
				dynamic d = new ExpandoObject();
				d.TransportUnitType = Convert.ToInt32(e.Attribute("TransportUnitType").Value);
				d.CarriedUnitType = Convert.ToInt32(e.Attribute("CarriedUnitType").Value);
				d.CarriedUnitClass = Convert.ToInt32(e.Attribute("CarriedUnitClass").Value);

				lookupListTmp.Add(d);
			}

			_unitTransportUnitTypeUnitClasses = lookupListTmp;
		}		

		private void LoadFactionVictoryConditions(XDocument gameDataFile)
		{
			var lookupTmp = gameDataFile.Descendants("VictoryCondition");
			var lookupListTmp = lookupTmp.Select(e => new VictoryConditionDTO
				{
					FactionID = Convert.ToInt32(e.Attribute("Faction").Value), 
					ConditionType = Convert.ToInt32(e.Attribute("ConditionType").Value), 
					Value = Convert.ToInt32(e.Attribute("Value").Value)
				}).ToList();

			_factionVictoryConditions = lookupListTmp.Cast<dynamic>().ToList();
		}

		/// <summary>
		/// Creates strategy related components from data context and stores in cache
		/// </summary>
		/// <param name="cacheDataFile"></param>
		private void LoadStrategyCache(XDocument cacheDataFile)
		{
			var tactics = cacheDataFile.Descendants("Tactic");
			
			var repository = XMLComponentRepository.Instance;

			foreach (var tactic in tactics)
			{
				var player = repository.GetPlayers().SingleOrDefault(p => p.ID == Convert.ToInt32(tactic.Attribute("Player").Value)).ToComponent();
				var stance = (StrategicalStance)Convert.ToInt32(tactic.Attribute("Stance").Value);
				var tComponent = new Tactic(stance, player);

				var missions = tactic.Descendants("Mission");

				foreach (var mission in missions)
				{
					var unitID = Convert.ToInt32(mission.Attribute("Unit").Value);
					var missionType = repository.GetMissionTypes().SingleOrDefault(mt => mt.ID == Convert.ToInt32(mission.Attribute("MissionType").Value)).ToComponent();
					var mComponent = new Mission(missionType, unitID);

					var unitTasks = mission.Descendants("UnitTask");

					foreach (var unitTask in unitTasks)
					{
						var args = unitTask.Descendants("TaskExecutionArgument");
						var argComponents = new List<TaskExecutionArgument>();

						foreach (var arg in args)
						{
							var values = arg.Descendants("ArgumentValue");
							var valueComponents = new List<string>();

							foreach (var value in values)
							{
								valueComponents.Add(value.Attribute("Value").Value);
							}

							argComponents.Add(new TaskExecutionArgument
													(
														arg.Attribute("Type").Value, 
														arg.Attribute("Assembly").Value, 
														arg.Attribute("Name").Value, 
														valueComponents
													));
						}

						var unitTaskType = repository.GetUnitTaskTypes().SingleOrDefault(utt => utt.ID == Convert.ToInt32(unitTask.Attribute("TaskType").Value)).ToComponent();
						var turnsToComplete = Convert.ToInt32(unitTask.Attribute("TurnsToComplete").Value);
						var utComponent = new UnitTask(unitTaskType, mComponent, argComponents, turnsToComplete);

						mComponent.AddChildComponent(utComponent);
					}

					mComponent.SetCurrentTask();
					tComponent.AddChildComponent(mComponent);
				}

				var result = Game.Instance.StrategyHandler.SetUpTactic(tComponent);
			}	
			
		}

#endregion

#region Helper Methods

		/// <summary>
		/// Returns a list of IBaseGameComponentDTO and matching source elements with the properties set from the data source
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="elements"></param>
		/// <returns></returns>
		private IEnumerable<Tuple<T, XElement>> GetBaseComponentDTOs<T>(IEnumerable<XElement> elements)
			where T : class, IBaseGameComponentDTO, new() 
		{
			var retVal = elements
						.Select(c => new {element = c, pair = new Tuple<T, XElement>(GetBaseComponentDTO<T>(c), c)})
						.Select(p => p.pair);

			return retVal;
		}

		/// <summary>
		/// Returns an IBaseGameComponentDTO with the properties set from the data source
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="e"></param>
		/// <returns></returns>
		private T GetBaseComponentDTO<T>(XElement e) 
			where T : class, IBaseGameComponentDTO, new()
		{
			var retVal = new T
						{
							UID = Guid.NewGuid(),
							Name = e.Attribute("Name").Value, 
							Description = (e.Attribute("Description") != null) ? e.Attribute("Description").Value :string.Empty,
							ID = Convert.ToInt32(e.Attribute("ID").Value)
						} ;

			return retVal;
		}

		
		private void LoadStatModifierData(XElement e, IStatModifier dto)
		{
			dto.AttackModifier = (e.Attribute("AttackModifier") != null) ? Convert.ToDouble(e.Attribute("AttackModifier").Value) : 0;
			dto.AttackDistanceModifier = (e.Attribute("AttackDistanceModifier") != null) ? Convert.ToDouble(e.Attribute("AttackDistanceModifier").Value) : 0;
			dto.RemoteFirePoints = (e.Attribute("RemoteFirePoints") != null) ? Convert.ToInt32(e.Attribute("RemoteFirePoints").Value) : 0;
			dto.DefenceModifier = (e.Attribute("DefenceModifier") != null) ? Convert.ToDouble(e.Attribute("DefenceModifier").Value) : 0;
			dto.MovementModifier = (e.Attribute("MovementModifier") != null) ? Convert.ToDouble(e.Attribute("MovementModifier").Value) : 0;
			dto.UnitWeightModifier = (e.Attribute("UnitWeightModifier") != null) ? Convert.ToDouble(e.Attribute("UnitWeightModifier").Value) : 0;
			dto.AllowableWeightModifier = (e.Attribute("AllowableWeightModifier") != null) ? Convert.ToDouble(e.Attribute("AllowableWeightModifier").Value) : 0;
			dto.UnitCostModifier = (e.Attribute("UnitCostModifier") != null) ? Convert.ToDouble(e.Attribute("UnitCostModifier").Value) : 0;
			dto.StealthModifier = (e.Attribute("StealthModifier") != null) ? Convert.ToDouble(e.Attribute("StealthModifier").Value) : 0;
		}

#endregion	
		

	}



}
