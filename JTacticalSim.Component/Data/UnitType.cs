using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.Service;
using JTacticalSim.API.Media.Sound;
using JTacticalSim.Media.Sound;

namespace JTacticalSim.Component.Data
{
	public class UnitType : ComponentStatModifierContainer, IUnitType
	{
		public IUnitBaseType BaseType { get; set; }
		public IUnitBranch Branch { get; set; }
		public bool HasGlobalMovementOverride { get; set; }
		public bool Nuclear { get; set; }
		public bool FuelConsumer { get; set; }
		public int FuelRange { get; set; }
		public string TextDisplayZ1 { get; set; }
		public string TextDisplayZ2 { get; set; }
		public string TextDisplayZ3 { get; set; }
		public string TextDisplayZ4 { get; set; }

		public UnitType()
		{
		}

		public double ReinforcementCost()
		{
			return (UnitCostModifier * TheGame().BasePointValues.CostBase);
		}
	}
}
