using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.Utility;
using JTacticalSim.API.Data;

namespace JTacticalSim.Component.Data
{
	public class DemographicClass : ComponentStatModifierContainer, IDemographicClass
	{
		public IDemographicType DemographicType { get; set; }
		public BuildInfo BuildInfo { get; set; }
		public string TextDisplayZ1 { get; set; }
		public string TextDisplayZ2 { get; set; }
		public string TextDisplayZ3 { get; set; }
		public string TextDisplayZ4 { get; set; }
		public bool MovementHinderanceConfigured { get; set; }
		public string SpriteName { get {return "dem_{0}".F(this.Name); }}
	}
}
