using System.Text;
using System.Xml.Linq;
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

		public override string ComponentDirectory { get { return Path.Combine(FilePathComponentData, FILE_TYPE_DIRECTORY); }}
		public override string GameSaveRootDirectory { get { return Path.Combine(FilePathGameSave, _gameFileDirectory); }}
		public override string GameSaveDirectory { get { return Path.Combine(GameSaveRootDirectory, FILE_TYPE_DIRECTORY); }}
		public override string ScenarioDirectory { get { return Path.Combine(FilePathScenario, _gameFileDirectory, FILE_TYPE_DIRECTORY); }}

		public XMLDataFileInfo(string gameFileDirectory, IComponentSet componentSet,  bool IsScenario)
			: base(componentSet)
		{
			var basePath = (IsScenario) ? FilePathScenario : FilePathGameSave;

			_gameFileDirectory = gameFileDirectory;
			_componentDataFilePath = Path.Combine(FilePathComponentData, FILE_TYPE_DIRECTORY, "050_ComponentData.xml");
			_lookupDataFilePath = Path.Combine(FilePathComponentData, FILE_TYPE_DIRECTORY, "060_LookupData.xml");
			_gameDataFilePath = Path.Combine(basePath, gameFileDirectory, FILE_TYPE_DIRECTORY, "100_GameData.xml");
			_boardDataFilePath = Path.Combine(basePath, gameFileDirectory, FILE_TYPE_DIRECTORY, "200_BoardMap.xml");
			_unitDataFilePath = Path.Combine(basePath, gameFileDirectory, FILE_TYPE_DIRECTORY, "300_UnitData.xml");
			_cacheDataFilePath = Path.Combine(basePath, gameFileDirectory, FILE_TYPE_DIRECTORY, "400_Cache.xml");
			_synopsisFilePath = Path.Combine(basePath, gameFileDirectory, FILE_TYPE_DIRECTORY, "500_Synopsis.xml");
		}
	}
}
