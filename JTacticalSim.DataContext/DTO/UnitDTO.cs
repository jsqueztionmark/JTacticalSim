using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using JTacticalSim.API;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class UnitDTO : BaseBoardComponentDTO
	{
		[DataMember]
		public int StackOrder { get; set; }

		[DataMember]
		public int UnitClass {get; set;}

		[DataMember]
		public int UnitType { get; set; }

		[DataMember]
		public int UnitGroupType { get; set; }

		[DataMember]
		public int? CurrentMovementPoints { get; set; }

		[DataMember]
		public int? Posture { get; set; }

		[DataMember]
		public int? CurrentRemoteFirePoints { get; set; }

		[DataMember]
		public bool? CurrentHasPerformedAction { get; set; }

		[DataMember]
		public int? CurrentFuelRange { get; set; }

		[DataMember]
		public List<int> UnitsTransported { get; set; }

		public UnitDTO()
		{
			UnitsTransported = new List<int>();
		}
	}
}
