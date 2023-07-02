using System;
using System.Runtime.Serialization;
using JTacticalSim.Utility;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class CoordinateDTO
	{
		[DataMember]
		public int X { get; set; }

		[DataMember]
		public int Y { get; set; }

		[DataMember]
		public int Z { get; set; }

		public override string ToString()
		{
			return "{0}, {1}, {2}".F(X, Y, Z);
		}
	}
}
