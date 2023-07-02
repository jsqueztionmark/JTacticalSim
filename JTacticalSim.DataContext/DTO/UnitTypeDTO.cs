using System;
using System.Runtime.Serialization;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class UnitTypeDTO : DTOStatModifierContainer, ITextDisplayableDTO
	{
		[DataMember]
		public int UnitBaseTypeID { get; set; }

		[DataMember]
		public int UnitBranchID { get; set; }

		[DataMember]
		public string TextDisplayZ1 { get; set; }

		[DataMember]
		public string TextDisplayZ2 { get; set; }

		[DataMember]
		public string TextDisplayZ3 { get; set; }

		[DataMember]
		public string TextDisplayZ4 { get; set; }

		[DataMember]
		public string Sound_Fire { get; set; }

		[DataMember]
		public string Sound_Move { get; set; }

		[DataMember]
		public bool Nuclear { get; set; }

		[DataMember]
		public bool FuelConsumer { get; set; }

		[DataMember]
		public int FuelRange { get; set; }
	}
}
