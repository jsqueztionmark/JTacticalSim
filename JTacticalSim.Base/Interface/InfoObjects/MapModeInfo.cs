using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTacticalSim.API.InfoObjects;

namespace JTacticalSim.API.InfoObjects
{
	public class MapModeInfo
	{
		public MapMode MapMode { get; private set; }
		public string Name { get; private set; }
		public bool IsCurrent { get; set; }

		public MapModeInfo(MapMode mapMode, string name)
		{
			MapMode = mapMode;
			Name = name;
		}
	}
}
