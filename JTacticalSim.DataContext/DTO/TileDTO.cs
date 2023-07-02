using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class TileDTO : BaseBoardComponentDTO
	{
		[DataMember]
		public IEnumerable<DemographicDTO> Demographics { get; set; }

		[DataMember]
		public int VictoryPoints { get; set; }

		[DataMember]
		public bool IsPrimeTarget { get; set; }

		[DataMember]
		public bool IsGeographicChokePoint { get; set; }

	}
}
