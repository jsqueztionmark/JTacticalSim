using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Game;

namespace JTacticalSim.Component
{
	public struct ParsedCommandArgs : IParsedCommandArgs
	{
		public string Command { get; set; }
		public string[] Args { get; set; }
		public List<string> Switches { get; set; }
		public bool Cancel { get; set; }
	}
}
