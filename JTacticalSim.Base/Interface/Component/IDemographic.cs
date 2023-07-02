using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.Utility;

namespace JTacticalSim.API.Component
{
	public interface IDemographic : IBaseComponent
	{
		IDemographicClass DemographicClass { get; set; }
		List<Direction> Orientation { get; set; }
		bool ProvidesMedical { get; set; }
		bool ProvidesSupply { get; set; }
		string Value { get; set; }
		string InstanceName { get; set; }

		bool IsDemographicType(string demographicTypeName);
		bool IsDemographicClass(string demographicClassName);
		bool IsHybrid();
	}
}
