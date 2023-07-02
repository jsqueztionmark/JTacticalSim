using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Media.Sound;

namespace JTacticalSim.API.Component
{
	public interface IUnitType : ITextDisplayable, IStatModifier
	{
		IUnitBaseType BaseType { get; set; }
		IUnitBranch Branch { get; set; }
		ISoundSystem Sounds { get; }
		bool Nuclear { get; set; }
		bool FuelConsumer { get; set; }
		int FuelRange { get; set; }
		double ReinforcementCost();
		bool HasGlobalMovementOverride { get; set; }
	}
}
