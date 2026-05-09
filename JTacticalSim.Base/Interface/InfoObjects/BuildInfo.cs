using System;
using System.Text;

namespace JTacticalSim.API.InfoObjects
{
	public class BuildInfo
	{
		public bool Buildable { get; set; }
		public bool Destroyable { get; set; }
		public int BuildTurns { get; set; }
		public int DestroyTurns { get; set; }
	}
}
