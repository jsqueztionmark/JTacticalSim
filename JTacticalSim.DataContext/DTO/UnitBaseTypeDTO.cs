using System;
using System.Runtime.Serialization;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class UnitBaseTypeDTO : BaseGameComponentDTO
	{
		[DataMember]
		public bool CanReceiveMedicalSupport { get; set; }

		[DataMember]
		public bool CanBeSupplied { get; set; }

		[DataMember]
		public bool NuclearAffected { get; set; }

		[DataMember]
		public string OutOfFuelMoveResultMessage { get; set; }
	}
}
