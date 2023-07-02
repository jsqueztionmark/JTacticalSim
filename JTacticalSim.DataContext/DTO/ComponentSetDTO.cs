using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using JTacticalSim.Data.DTO;

namespace JTacticalSim.DataContext
{
	public class ComponentSetDTO : BaseGameComponentDTO
	{
		[DataMember]
		public string Path { get; set; }
	}
}
