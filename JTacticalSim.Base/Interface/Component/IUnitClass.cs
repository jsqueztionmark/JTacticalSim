using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Media.Sound;

namespace JTacticalSim.API.Component
{
	public interface IUnitClass : ITextDisplayable, IStatModifier
	{
		ISoundSystem Sounds { get; }
		double ReinforcementCost();
	}
}
