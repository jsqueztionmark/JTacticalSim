using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;

namespace JTacticalSim.Component.Data
{
	public class UnitBaseType : GameComponentBase, IUnitBaseType
	{
		public bool CanReceiveMedicalSupport { get; set; }
		public bool CanBeSupplied { get; set; }
		public bool NuclearAffected { get; set; }
		public IEnumerable<int> SupportedUnitGeographyTypes { get; set; }
		public string OutOfFuelMoveResultMessage { get; set; }
	}
}
