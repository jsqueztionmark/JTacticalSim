using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.AI
{
	public interface IMissionType : IBaseComponent
	{
		int Priority { get; set; }
		int TurnOrder { get; set; }
		bool CanceledByMove { get; set; }
	}
}
