using System;
using JTacticalSim.API;
using JTacticalSim.API.Component;

namespace JTacticalSim.DataContext
{
	public class DataFileFactory
	{
		private static volatile DataFileFactory _instance = null;
		static readonly object padlock = new object();

		public static DataFileFactory Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new DataFileFactory();
				}

				return _instance;
			}
		}


		public IDataFileInfo<fileType> GetDataFiles<fileType>(string gameFileDirectory, IComponentSet componentSet, bool IsScenario)
		{
			switch (Utility.GetDataSourceType())
			{
				case DataSourceType.XML:
					return new XMLDataFileInfo(gameFileDirectory, componentSet, IsScenario) as IDataFileInfo<fileType>;
				default:
					{
						throw new Exception("No data source type found for current configuration or current data source configuration is not a file type");	
					}
			}
		}
	}
}
