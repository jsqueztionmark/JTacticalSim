using System;
using System.Text;

namespace JTacticalSim.API.InfoObjects
{
	public class CurrentMoveStatInfo
	{
		public int MovementPoints { get; set; }
		public int RemoteFirePoints { get; set; }
		public bool HasPerformedAction { get; set; }
	}
}
