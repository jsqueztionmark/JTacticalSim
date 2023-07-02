using System;
using System.Runtime.Serialization;
using JTacticalSim.API.Data;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class BasePointValuesDTO : IBasePointValues
	{
		[DataMember]
		public int Movement { get; private set; }

		[DataMember]
		public int CombatRoll { get; private set; }

		[DataMember]
		public int CombatBase { get; private set; }

		[DataMember]
		public int StealthRoll { get; private set; }

		[DataMember]
		public int StealthBase { get; private set; }

		[DataMember]
		public double HiddenStealthThreshhold { get; private set; }

		[DataMember]
		public int MedicalSupportBase { get; private set; }

		[DataMember]
		public int WeightBase { get; private set; }

		[DataMember]
		public int CostBase { get; private set; }

		[DataMember]
		public int AIBaseRoll { get; private set; }

		[DataMember]
		public int AIAggressiveness { get; private set; }

		[DataMember]
		public int AIDefensiveness { get; private set; }

		[DataMember]
		public int AIIntelligence { get; private set; }

		[DataMember]
		public int MeterDistanceBase { get; private set; }

		[DataMember]
		public int ReinforcementCalcBaseCountry { get; private set; }
		
		[DataMember]
		public int ReinforcementCalcBaseFaction { get; private set; }
		
		[DataMember]
		public double ReinforcementCalcBaseVP { get; private set; }

		[DataMember]
		public double HQBonus { get; private set; }

		[DataMember]
		public double NotSuppliedPenalty { get; private set; }

		[DataMember]
		public int MaxSupplyDistance { get; private set; }

		[DataMember]
		public double TargetAttachedUnitBonus { get; private set; }

		[DataMember]
		public double TargetMedicalUnitBonus { get; private set; }

		[DataMember]
		public double TargetSupplyUnitBonus { get; private set; }

		[DataMember]
		public double BattlePostureBonus { get; private set; }

		public BasePointValuesDTO()
		{}

		public BasePointValuesDTO(	int movement, 
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
