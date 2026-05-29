using System;
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
