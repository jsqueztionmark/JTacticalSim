using System;
using System.Text;
using JTacticalSim.Utility;
using JTacticalSim.API.InfoObjects;

namespace JTacticalSim.API.Component
{
	public interface IDemographicClass : ITextDisplayable, IStatModifier, ISpriteContainer
	{
		IDemographicType DemographicType { get; set; }
		bool MovementHinderanceConfigured { get; set; }
		BuildInfo BuildInfo { get; set; }
	}
}
