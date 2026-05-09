using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Xml.Linq;
using JTacticalSim.API.Data;
using JTacticalSim.API.Component;
using JTacticalSim.Utility;

namespace JTacticalSim.DataContext
{
	internal abstract class DataFileInfoBase<fileType> : IDataFileInfo<fileType>
	{
		protected string FilePathRoot { get; set; }
		protected string FilePathGameSave { get; set; }
		protected string FilePathScenario { get; set; }
		protected string FilePathComponentData { get; set; }
		protected string FilePathComponentSet { get; set; }

		protected string _scenarioDataFilePath;
		protected string _savedGameDataFilePath;
		protected string _componentSetDataFilePath;
		protected string _componentDataFilePath;
		protected string _lookupDataFilePath;
		protected string _gameDataFilePath;
		protected string _cacheDataFilePath;
		protected string _synopsisFilePath;
		protected string _boardDataFilePath;
		protected string _unitDataFilePath;

		public string ScenarioDataFilePath { get { return _scenarioDataFilePath; }}
		public string SavedGameDataFilePath { get { return _savedGameDataFilePath; } }
		public string ComponentSetDataFilePath { get { return _componentSetDataFilePath; } }
		public string ComponentDataFilePath { get { return _componentDataFilePath; } }
		public string LookupDataFilePath { get { return _lookupDataFilePath; } }
		public string GameDataFilePath { get { return _gameDataFilePath; } }
		public string BoardDataFilePath { get { return _boardDataFilePath; } }
		public string UnitDataFilePath { get { return _unitDataFilePath; } }
		public string CacheDataFilePath { get { return _cacheDataFilePath; } }
		public string SynopsisFilePath {get { return _synopsisFilePath; }}

		public XDocument SavedGameDataFile { get { return XDocument.Load(_savedGameDataFilePath); } }
		public XDocument ScenarioDataFile {get { return XDocument.Load(_scenarioDataFilePath); }}
		public XDocument ComponentSetDataFile { get { return XDocument.Load(_componentSetDataFilePath); } }

		public abstract fileType ComponentDataFile { get; }
		public abstract fileType LookupDataFile { get; }
		public abstract fileType GameDataFile { get; }
		public abstract fileType BoardDataFile { get; }
		public abstract fileType UnitDataFile { get; }
		public abstract fileType CacheDataFile { get; }
		public abstract fileType SynopsisFile { get; }
		public abstract string GameSaveRootDirectory { get; }
		public abstract string GameSaveDirectory { get; }
		public abstract string ScenarioDirectory { get; }
		public abstract string ComponentDirectory { get; }

		public List<String> ScenarioFilePaths 
		{ 
			get
			{
				return new List<string>
				{
					GameDataFilePath,
					BoardDataFilePath,
					UnitDataFilePath,
					CacheDataFilePath,
					SynopsisFilePath
				};
			}
		}

		public List<fileType> ScenarioFiles
		{
			get
			{
				return new List<fileType>
				{
					GameDataFile,
					BoardDataFile,
					UnitDataFile,
					CacheDataFile,
					SynopsisFile
				};
			}
		}

		protected DataFileInfoBase(IComponentSet componentSet)
		{
			FilePathComponentSet = (componentSet != null) ? componentSet.Path : ConfigurationManager.AppSettings["default_component_set"];
			FilePathRoot = ToAbsolutePath(ConfigurationManager.AppSettings["datafilepathDefault"]);
			FilePathComponentData = Path.Combine(FilePathRoot, FilePathComponentSet);
			FilePathGameSave = ToAbsolutePath(ConfigurationManager.AppSettings["datafilepathGameSaveDefault"]);
			FilePathScenario = ToAbsolutePath(ConfigurationManager.AppSettings["datafilepathScenarioDefault"]);

			_savedGameDataFilePath = Path.Combine(FilePathRoot, "010_SavedGames.xml");
			_scenarioDataFilePath = Path.Combine(FilePathRoot, "005_Scenarios.xml");
			_componentSetDataFilePath = Path.Combine(FilePathRoot, "002_ComponentSets.xml");
		}

		private static string ToAbsolutePath(string configPath)
		{
			var normalized = configPath.Replace('\\', Path.DirectorySeparatorChar);
			return Path.IsPathRooted(normalized)
				? normalized
				: Path.Combine(Directory.GetCurrentDirectory(), normalized);
		}
	}
}
