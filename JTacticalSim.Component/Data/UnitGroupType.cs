using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.Service;

namespace JTacticalSim.Component.Data
{
	public class UnitGroupType : GameComponentBase, IUnitGroupType, IComparable
	{
		public string TextDisplayZ1 { get; set; }
		public string TextDisplayZ2 { get; set; }
		public string TextDisplayZ3 { get; set; }
		public string TextDisplayZ4 { get; set; }
		public int Level { get; set; }
		public int MaxDirectAssignedUnits { get; set; }

		public IUnitGroupType NextHighestGroupType
		{
			get { return TheGame().JTSServices.UnitService.GetNextHighestUnitGroupType(this); }
		}

		public UnitGroupType()
		{}

		public new int CompareTo(object obj)
		{
			var o = obj as UnitGroupType;

			if (Level > o.Level) return 1;
			if (Level < o.Level) return -1;

			return 0;
		}
	}
}
