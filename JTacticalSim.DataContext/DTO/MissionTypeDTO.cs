using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.Data.DTO
{
	public class MissionTypeDTO : BaseGameComponentDTO
	{
		public int Priority { get; set; }
		public int TurnOrder { get; set; }
		public bool CanceledByMove { get; set; }
	}
}
