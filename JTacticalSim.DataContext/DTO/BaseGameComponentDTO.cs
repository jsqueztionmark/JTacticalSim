using System;
using System.Runtime.Serialization;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class BaseGameComponentDTO : IBaseGameComponentDTO
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string Description { get; set; }

		[DataMember]
		public int ID { get; set; }

		[DataMember]
		public Guid UID { get; set; }
	}
}
