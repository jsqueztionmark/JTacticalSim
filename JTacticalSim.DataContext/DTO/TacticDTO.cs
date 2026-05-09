using System;
using System.Text;
using System.Runtime.Serialization;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class TacticDTO : BaseGameComponentDTO
	{
		[DataMember]
		public int Stance { get; set; }

		[DataMember]
		public int Player { get; set; }
	}
}
