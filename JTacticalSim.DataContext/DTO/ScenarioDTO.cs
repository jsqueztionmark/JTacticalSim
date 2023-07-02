using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.Data.DTO;

namespace JTacticalSim.DataContext
{
	public class ScenarioDTO : BaseGameComponentDTO
	{
		[DataMember]
		public string GameFileDirectory { get; set; }

		[DataMember]
		public string Author { get; set; }

		[DataMember]
		public int ComponentSet { get; set; }

		[DataMember]
		public List<CountryDTO> Countries { get; set; }

		[DataMember]
		public List<FactionDTO> Factions { get; set; }

		[DataMember]
 		public List<VictoryConditionDTO> VictoryConditions { get; set; } 

		[DataMember]
		public string Synopsis { get; set; }

		public ScenarioDTO()
		{
			Countries = new List<CountryDTO>();
			Factions = new List<FactionDTO>();
			VictoryConditions = new List<VictoryConditionDTO>();
		}
	}
}
