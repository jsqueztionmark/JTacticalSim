using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.Utility;
using JTacticalSim.API.Component;

namespace JTacticalSim.DataContext
{
	internal class XMLDataFileInfo : DataFileInfoBase<XDocument>
	{
		private const string FILE_TYPE_DIRECTORY = "XML";

		private string _gameFileDirectory { get; set; }

		public override XDocument SynopsisFile { get { return XDocument.Load(_synopsisFilePath); }}
		public override XDocument ComponentDataFile { get { return XDocument.Load(_componentDataFilePath); }}
		public override XDocument LookupDataFile { get { return XDocument.Load(_lookupDataFilePath); }}
		public override XDocument GameDataFile { get { return XDocument.Load(_gameDataFilePath); }}
		public override XDocument BoardDataFile { get { return XDocument.Load(_boardDataFilePath); }}
		public override XDocument UnitDataFile { get { return XDocument.Load(_unitDataFilePath); }}
		public override XDocument CacheDataFile	{ get { return XDocument.Load(_cacheDataFilePath); }
		}

		public override string ComponentDirectory { get { return "{0}\\{1}".F(FilePathComponentData, FILE_TYPE_DIRECTORY); }}
		public override string GameSaveRootDirectory { get {return "{0}\\{1}".F(FilePathGameSave, _gameFileDirectory);}}
		public override string GameSaveDirectory { get { return "{0}\\{1}".F(GameSaveRootDirectory, FILE_TYPE_DIRECTORY); }}
		public override string ScenarioDirectory { get { return "{0}\\{1}\\{2}".F(FilePathScenario, _gameFileDirectory, FILE_TYPE_DIRECTORY); }}

		public XMLDataFileInfo(string gameFileDirectory, IComponentSet componentSet,  bool IsScenario)
			: base(componentSet)
		{
			var basePath = (IsScenario) ? FilePathScenario : FilePathGameSave;

			_gameFileDirectory = gameFileDirectory;
			_componentDataFilePath = "{0}\\{1}\\050_ComponentData.xml".F(FilePathComponentData, FILE_TYPE_DIRECTORY);
			_lookupDataFilePath = "{0}\\{1}\\060_LookupData.xml".F(FilePathComponentData, FILE_TYPE_DIRECTORY);
			_gameDataFilePath = "{0}\\{1}\\{2}\\100_GameData.xml".F(basePath, gameFileDirectory, FILE_TYPE_DIRECTORY);
			_boardDataFilePath = "{0}\\{1}\\{2}\\200_BoardMap.xml".F(basePath, gameFileDirectory, FILE_TYPE_DIRECTORY);
			_unitDataFilePath = "{0}\\{1}\\{2}\\300_UnitData.xml".F(basePath, gameFileDirectory, FILE_TYPE_DIRECTORY);
			_cacheDataFilePath = "{0}\\{1}\\{2}\\400_Cache.xml".F(basePath, gameFileDirectory, FILE_TYPE_DIRECTORY);
			_synopsisFilePath = "{0}\\{1}\\{2}\\500_Synopsis.xml".F(basePath, gameFileDirectory, FILE_TYPE_DIRECTORY);
		}
	}
}
