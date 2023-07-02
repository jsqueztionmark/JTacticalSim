using System;
using System.Runtime.Serialization;
using JTacticalSim.API.Component;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class DTOStatModifierContainer : BaseGameComponentDTO, IStatModifier
	{
		[DataMember]
		public double MovementModifier { get; set; }

		[DataMember]
		public double AttackModifier { get; set; }

		[DataMember]
		public double AttackDistanceModifier { get; set; }

		[DataMember]
		public int RemoteFirePoints { get; set; }

		[DataMember]
		public double DefenceModifier { get; set; }

		[DataMember]
		public double UnitCostModifier { get; set; }

		[DataMember]
		public double UnitWeightModifier { get; set; }

		[DataMember]
		public double AllowableWeightModifier { get; set; }

		[DataMember]
		public double StealthModifier { get; set; }
	}
}
