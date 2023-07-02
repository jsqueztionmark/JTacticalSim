using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public abstract class BaseBoardComponentDTO : BaseGameComponentDTO, IBaseBoardComponentDTO
	{
		[DataMember]
		public CoordinateDTO Location { get; set; }

		[DataMember]
		public int SubNodeLocation { get; set; }

		[DataMember]
		public int Country { get; set; }
	}
}
