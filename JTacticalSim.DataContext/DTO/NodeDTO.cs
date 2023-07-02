using System;
using System.Runtime.Serialization;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class NodeDTO : BaseBoardComponentDTO
	{
		[DataMember]
		public TileDTO DefaultTile { get; set; }
	}
}
