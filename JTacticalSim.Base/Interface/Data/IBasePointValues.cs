using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.Data
{
	public interface IBasePointValues
	{
		int Movement { get; }
		int CombatRoll { get; }
		int CombatBase { get; }
		int StealthRoll { get; }
		int StealthBase { get; }
		double HiddenStealthThreshhold { get; }
		int MedicalSupportBase { get; }
		int WeightBase { get; }
		int CostBase { get; }
		int AIBaseRoll { get; }
		int AIAggressiveness { get; }
		int AIDefensiveness { get; }
		int AIIntelligence { get; }
		int MeterDistanceBase { get; }
		int ReinforcementCalcBaseCountry { get; }
		int ReinforcementCalcBaseFaction { get; }
		double ReinforcementCalcBaseVP { get; }
		double HQBonus { get; }
		double NotSuppliedPenalty { get; }
		int MaxSupplyDistance { get; }
		double TargetAttachedUnitBonus { get; }
		double TargetMedicalUnitBonus { get; }
		double TargetSupplyUnitBonus { get; }
		double BattlePostureBonus { get; }
	}
}
