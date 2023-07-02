using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.Component
{
	/// <summary>
	/// A game component that holds statistics used in calculating game rules
	/// </summary>
	public interface IStatModifier
	{
		double MovementModifier { get; set; }
		double AttackModifier { get; set; }
		double AttackDistanceModifier { get; set; }
		int RemoteFirePoints { get; set; }
		double DefenceModifier { get; set; }
		double UnitCostModifier { get; set; }
		double UnitWeightModifier { get; set; }
		double AllowableWeightModifier { get; set; }
		double StealthModifier { get; set; }
	}
}
