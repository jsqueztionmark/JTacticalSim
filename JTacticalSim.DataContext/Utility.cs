using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using JTacticalSim.API;
using JTacticalSim.API.Game;
using JTacticalSim.API.Component;
using JTacticalSim.API.Cache;
using JTacticalSim.API.Service;
using JTacticalSim.API.InfoObjects;


namespace JTacticalSim.DataContext
{
	public static class Utility
	{
		public static IGameCacheDependencies Cache = JTacticalSim.Cache.GameCache.Instance;
		public static DataFactory DataFactory = DataFactory.Instance;

		public static DataSourceType GetDataSourceType()
		{
			var sourceType = ConfigurationManager.AppSettings["datasourcetype"];

			switch (sourceType)
			{
				case "memory":
					return DataSourceType.MEMORY;
				case "rdbms":
					return DataSourceType.SQL;
				case "XML":
					return DataSourceType.XML;
				default:
					return DataSourceType.UNKNOWN;
			}
		}


		public static Type GetDataFileType()
		{
			switch (GetDataSourceType())
			{
				case DataSourceType.XML:
					return typeof(XDocument);
				default:
					throw new Exception("DataSourceType not configured or configured type is not a file type");
			}
		}

		/// <summary>
		/// Returns all context table objects with the TableRecognizable attribute
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<Type, Tuple<Type, TableInfo>>> GetAllTableInfos()
		{
			var tables = new Dictionary<Type, Tuple<Type, TableInfo>>();

			using (var ctx = DataFactory.GetDataContext())
			{
				PropertyInfo[] props = typeof(BaseDataContext).GetProperties();

				foreach (PropertyInfo p in props)
				{
					var attr = Attribute.GetCustomAttribute(p, typeof(TableRecognizable), false) as TableRecognizable;
					if (attr == null) continue;
					tables.Add(attr.ComponentType, new Tuple<Type, TableInfo>(attr.RecordType, p.GetValue(ctx, null) as TableInfo));
				}
			}

			return tables;
		}

		/// <summary>
		/// Returns tableInfo object for given type
		/// </summary>
		/// <returns></returns>
		public static TableInfo GetComponentTable(IBaseComponent component)
		{
			var tables = GetAllTableInfos();
			var retVal = tables.SingleOrDefault(ti => ti.Key == component.GetType()).Value.Item2;
			return retVal;
		}

		/// <summary>
		/// Returns tableInfo object for given type
		/// </summary>
		/// <returns></returns>
		public static object GetComponentTable(Type type)
		{
			return GetAllTableInfos().SingleOrDefault(ti => ti.Key.Equals(type)).Value.Item2;
		}

	}
}
