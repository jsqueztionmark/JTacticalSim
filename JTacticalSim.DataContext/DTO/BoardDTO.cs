using System;
using System.Runtime.Serialization;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class BoardDTO
	{
		[DataMember]
		public string Title { get; set; }

		[DataMember]
		public int Height { get; set; }

		[DataMember]
		public int Width { get; set; }

		[DataMember]
		public int DrawHeight { get; set; }

		[DataMember]
		public int DrawWidth { get; set; }

		[DataMember]
		public int CellSize { get; set; }

		[DataMember]
		public int CellMeters { get; set; }

		[DataMember]
		public int CellMaxUnits { get; set; }
	}
}
