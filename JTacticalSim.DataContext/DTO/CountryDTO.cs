using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using JTacticalSim.API.Component;

namespace JTacticalSim.Data.DTO
{
	[Serializable, DataContract]
	public class CountryDTO : BaseGameComponentDTO
	{
		[DataMember]
		public FactionDTO Faction { get; set; }

        [DataMember]
        public string FlagDisplayTextA { get; set; }

		[DataMember]
		public string FlagDisplayTextB { get; set; }

        [DataMember]
        public string Color { get; set; }

		[DataMember]
		public string BGColor { get; set; }

		[DataMember]
		public string TextDisplayColor { get; set; }

		[DataMember]
		public string FlagBGColor { get; set; }
		
		[DataMember]
		public string FlagColorA { get; set; }
		
		[DataMember]
		public string FlagColorB { get; set; }
	}
}
