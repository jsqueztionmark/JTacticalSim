using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Data;

namespace JTacticalSim.Component.Data
{
	public class BasePointValues : IBasePointValues
	{
		public int Movement { get; private set; }
		public int CombatRoll { get; private set; }
		public int CombatBase { get; private set; }
		public int StealthRoll { get; private set; }
		public int StealthBase { get; private set; }
		public double HiddenStealthThreshhold { get; private set; }
		public int MedicalSupportBase { get; private set; }
		public int WeightBase { get; private set; }
		public int CostBase { get; private set; }
		public int AIBaseRoll { get; private set; }
		public int AIAggressiveness { get; private set; }
		public int AIDefensiveness { get; private set; }
		public int AIIntelligence { get; private set; }
		public int MeterDistanceBase { get; private set; }
		public int ReinforcementCalcBaseCountry { get; private set; }
		public int ReinforcementCalcBaseFaction { get; private set; }
		public double ReinforcementCalcBaseVP { get; private set; }
		public double HQBonus { get; private set; }
		public double NotSuppliedPenalty { get; private set; }
		public int MaxSupplyDistance { get; private set; }
		public double TargetAttachedUnitBonus { get; private set; }
		public double TargetMedicalUnitBonus { get; private set; }
		public double TargetSupplyUnitBonus { get; private set; }
		public double BattlePostureBonus { get; private set; }

		public BasePointValues(	int movement, 
								int combatRoll, 
								int combatBase,
								int stealthRoll,
								int stealthBase,
								double hiddenStealthThreshhold,
								int medicalSupportBase,
								int weightBase,
								int costBase,
								int aIBaseRoll,
								int aIAggressiveness,
								int aIDefensiveness,
								int aIIntelligence,
								int meterDistanceBase,
								int reinforcementCalcBaseCountry,
								int reinforcementCalcBaseFaction,
								double reinforcementCalcBaseVP,
								double hqBonus,
								double notSuppliedPenalty,
								int maxSupplyDistance,
								double targetAttachedUnitBonus,
								double targetMedicalUnitBonus,
								double targetSupplyUnitBonus,
								double battlePostureBonus)
		{
			Movement = movement;
			CombatBase = combatBase;
			CombatRoll = combatRoll;
			StealthRoll = stealthRoll;
			StealthBase = stealthBase;
			HiddenStealthThreshhold = hiddenStealthThreshhold;
			MedicalSupportBase = medicalSupportBase;
			WeightBase = weightBase;
			CostBase = costBase;
			AIBaseRoll = aIBaseRoll;
			AIAggressiveness = aIAggressiveness;
			AIDefensiveness = aIDefensiveness;
			AIIntelligence = aIIntelligence;
			MeterDistanceBase = meterDistanceBase;
			ReinforcementCalcBaseCountry = reinforcementCalcBaseCountry;
			ReinforcementCalcBaseFaction = reinforcementCalcBaseFaction;
			ReinforcementCalcBaseVP = reinforcementCalcBaseVP;
			HQBonus = hqBonus;
			NotSuppliedPenalty = notSuppliedPenalty;
			MaxSupplyDistance = maxSupplyDistance;
			TargetAttachedUnitBonus = targetAttachedUnitBonus;
			TargetMedicalUnitBonus = targetMedicalUnitBonus;
			TargetSupplyUnitBonus = targetSupplyUnitBonus;
			BattlePostureBonus = battlePostureBonus;
		}
	}
}
