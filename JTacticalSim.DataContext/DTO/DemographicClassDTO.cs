using System;
using System.Runtime.Serialization;
using JTacticalSim.API.InfoObjects;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class DemographicClassDTO : DTOStatModifierContainer, ITextDisplayableDTO
	{
		[DataMember]
		public int DemographicType { get; set; }

		[DataMember]
		public BuildInfo BuildInfo { get; set; }

		[DataMember]
		public string TextDisplayZ1 { get; set; }

		[DataMember]
		public string TextDisplayZ2 { get; set; }

		[DataMember]
		public string TextDisplayZ3 { get; set; }

		[DataMember]
		public string TextDisplayZ4 { get; set; }

		[DataMember]
		public bool MovementHinderanceConfigured { get; set; }
	}
}
