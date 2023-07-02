using System;
using System.Runtime.Serialization;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class DemographicDTO : BaseGameComponentDTO
	{
		[DataMember]
		public int DemographicClass { get; set; }

		[DataMember]
		public string Orientation { get; set; }

		[DataMember]
		public bool ProvidesMedical { get; set; }

		[DataMember]
		public bool ProvidesSupply { get; set; }

		[DataMember]
		public string InstanceName { get; set; }

		[DataMember]
		public string Value { get; set; }
	}
}
