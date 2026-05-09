using System;
using System.Runtime.Serialization;
using System.Text;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class DemographicTypeDTO : BaseGameComponentDTO
	{
		[DataMember]
		public int DisplayOrder { get; set; }
	}
}
