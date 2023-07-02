using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.DataContext
{
	public class DataFileHandlerFactory
	{
		private static volatile DataFileHandlerFactory _instance = null;
		static readonly object padlock = new object();

		public static DataFileHandlerFactory Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new DataFileHandlerFactory();
				}

				return _instance;
			}
		}

		public IDataFileHandler<fileType> GetDataFileHandler<fileType>(bool IsScenario, IDataFileInfo<fileType> dataFiles)
		{
			if (IsScenario)
			{
				return new ScenarioFileHandler<fileType>(dataFiles);
			}

			return new GameFileHandler<fileType>(dataFiles);
			
		}
	}
}
