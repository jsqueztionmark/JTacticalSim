using System;
using System.Runtime.Serialization;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class UnitGroupTypeDTO : BaseGameComponentDTO, ITextDisplayableDTO
	{
		[DataMember]
		public string TextDisplayZ1 { get; set; }

		[DataMember]
		public string TextDisplayZ2 { get; set; }

		[DataMember]
		public string TextDisplayZ3 { get; set; }

		[DataMember]
		public string TextDisplayZ4 { get; set; }

		[DataMember]
		public int Level { get; set; }

		[DataMember]
		public int MaxDirectAssignedUnits { get; set; }
	}
}
