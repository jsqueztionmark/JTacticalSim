using System;
using System.Runtime.Serialization;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class SavedGameDTO : BaseGameComponentDTO
	{
		[DataMember]
		public string GameFileDirectory { get; set; }

		[DataMember]
		public int Scenario { get; set; }

		[DataMember]
		public bool LastPlayed { get; set; }
	}
}
