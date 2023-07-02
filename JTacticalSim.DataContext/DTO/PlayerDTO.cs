using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class PlayerDTO : BaseGameComponentDTO
	{
		[DataMember]
		public int Country { get; set; }

		[DataMember]
		public int? ReinforcementPoints { get; set; }

		[DataMember]
		public int? NuclearCharges { get; set; }

		[DataMember]
		public List<int> UnplacedReinforcements { get; set; } 

		[DataMember]
		public bool IsCurrentPlayer { get; set; }

		[DataMember]
		public bool IsAIPlayer { get; set; }

		public PlayerDTO()
		{
			UnplacedReinforcements = new List<int>();
		}
	}
}
