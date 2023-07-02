using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace JTacticalSim.DataContext
{
	public interface IDataFileInfo<fileType>
	{
		XDocument SavedGameDataFile { get; }
		XDocument ScenarioDataFile { get; }
		XDocument ComponentSetDataFile { get; }

		List<string> ScenarioFilePaths { get; }
		List<fileType> ScenarioFiles { get; }

		fileType SynopsisFile { get; }
		fileType ComponentDataFile { get; }
		fileType LookupDataFile { get; }
		fileType GameDataFile { get; }
		fileType BoardDataFile { get; }
		fileType UnitDataFile { get; }
		fileType CacheDataFile { get; }

		string SynopsisFilePath { get; }
		string ScenarioDataFilePath { get; }
		string SavedGameDataFilePath { get; }
		string ComponentSetDataFilePath { get; }
		string ComponentDataFilePath { get; }
		string LookupDataFilePath { get; }
		string GameDataFilePath { get; }
		string BoardDataFilePath { get; }
		string UnitDataFilePath { get; }
		string CacheDataFilePath { get; }

		string GameSaveRootDirectory { get; }
		string GameSaveDirectory { get; }
		string ScenarioDirectory { get; }
		string ComponentDirectory { get; }
	}
}
