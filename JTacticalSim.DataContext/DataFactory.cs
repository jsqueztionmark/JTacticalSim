using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using JTacticalSim.Data;
using JTacticalSim.API;
using JTacticalSim.API.Data;
using JTacticalSim.DataContext.Repository;

namespace JTacticalSim.DataContext
{
	public class DataFactory
	{
		private static volatile DataFactory _instance = null;
		static readonly object padlock = new object();

		public static DataFactory Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new DataFactory();
				}

				return _instance;
			}
		}

		public IDataContext GetDataContext()
		{
			switch (Utility.GetDataSourceType())
			{
				case DataSourceType.XML:
					return XMLDataContext.Instance;
				default:
					{
						throw new Exception("No data source type found for current configuration.");	
					}
			}
		}

		public IComponentRepository GetComponentRepository()
		{
			switch (Utility.GetDataSourceType())
			{
				case DataSourceType.XML:
					return XMLComponentRepository.Instance;
				default:
					{
						throw new Exception("No component repository found for current configuration.");
					}
			}
		}

		public IDataRepository GetDataRepository()
		{
			switch (Utility.GetDataSourceType())
			{
				case DataSourceType.XML:
					return XMLDataRepository.Instance;
				default:
					{
						throw new Exception("No data repository found for current configuration.");
					}
			}
		}
		
	}
}
