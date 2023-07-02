using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.InfoObjects
{
	public class UnitInfo
	{
		public IUnitType UnitType { get; set; }
		public IUnitClass UnitClass { get; set; }
		public IUnitGroupType UnitGroupType { get; set; }

		public UnitInfo(IUnitType unitType, 
						IUnitClass unitClass, 
						IUnitGroupType unitGroupType)
		{
			UnitType = unitType;
			UnitClass = unitClass;
			UnitGroupType = unitGroupType;
		}
	}
}
