using System;
using System.Text;
using JTacticalSim.API.AI;

namespace JTacticalSim.Component.AI
{
	public class MissionType : GameComponentBase, IMissionType
	{
		public int Priority { get; set; }
		public int TurnOrder { get; set; }
		public bool CanceledByMove { get; set; }
	}
}
