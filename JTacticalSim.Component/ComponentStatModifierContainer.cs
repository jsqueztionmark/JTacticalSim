using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;

namespace JTacticalSim.Component
{
	public class ComponentStatModifierContainer : GameComponentBase, IStatModifier
	{
		public double MovementModifier { get; set; }
		public double AttackModifier { get; set; }
		public double AttackDistanceModifier { get; set; }
		public int RemoteFirePoints { get; set; }
		public double DefenceModifier { get; set; }
		public double UnitCostModifier { get; set; }
		public double UnitWeightModifier { get; set; }
		public double AllowableWeightModifier { get; set; }
		public double StealthModifier { get; set; }
	}
}
