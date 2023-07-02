using System.Collections.Generic;
using System.Linq;
using JTacticalSim.API.Data;

namespace JTacticalSim.API.InfoObjects
{
	public class TableInfo : ICanGetNextID
	{
		public List<dynamic> Records { get; set; }

		public TableInfo()
		{
			Records = new List<dynamic>();
		}

		public int GetNextID()
		{
			return (Records.ToArray().Count() == 0) ? 0 : (Records.Max(r => r.ID) + 1);
		}

	}
}
