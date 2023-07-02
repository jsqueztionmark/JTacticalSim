using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.Component
{
	public interface IUnitBaseType : IBaseComponent
	{
		bool CanReceiveMedicalSupport { get; set; }
		bool CanBeSupplied { get; set; }
		bool NuclearAffected { get; set; }
		IEnumerable<int> SupportedUnitGeographyTypes { get; set; }
		string OutOfFuelMoveResultMessage { get; set; }
	}
}
