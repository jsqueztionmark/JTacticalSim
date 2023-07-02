using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class VictoryConditionDTO : BaseGameComponentDTO
	{
		[DataMember]
		public int FactionID { get; set; }

		[DataMember]
		public FactionDTO Faction { get; set; }

		[DataMember]
		public int ConditionType { get; set; }

		[DataMember]
		public VictoryConditionTypeDTO VictoryConditionType { get; set; }

		[DataMember]
		public int Value { get; set; }
	}
}
