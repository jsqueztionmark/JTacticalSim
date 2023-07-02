using System.Collections.Generic;
using JTacticalSim.API.Component;
using JTacticalSim.Utility;

namespace JTacticalSim.Component.GameBoard
{
	public class Demographic : GameComponentBase, IDemographic
	{
		public IDemographicClass DemographicClass { get; set; }
		public List<Direction> Orientation { get; set; }
		public bool ProvidesMedical { get; set; }
		public bool ProvidesSupply { get; set; }
		public string Value { get; set; }
		public string InstanceName { get; set; }
		
		public Demographic(){}

	// Rules

		public bool IsDemographicType(string demographicTypeName)
		{
			var dt = TheGame().JTSServices.DemographicService.GetDemographicTypeByID(DemographicClass.DemographicType.ID);
			return (dt.Name.ToLowerInvariant() == demographicTypeName.ToLowerInvariant());
		}

		public bool IsDemographicClass(string demographicClassName)
		{
			var dc = TheGame().JTSServices.DemographicService.GetDemographicClassByID(DemographicClass.ID);
			return (dc.Name.ToLowerInvariant() == demographicClassName.ToLowerInvariant());
		}

		public bool IsHybrid()
		{
			return TheGame().JTSServices.RulesService.DemographicIsHybrid(this);
		}
	}
}
