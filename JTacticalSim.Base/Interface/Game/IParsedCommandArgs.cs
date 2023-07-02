using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.Game
{
	public interface IParsedCommandArgs
	{
		string Command { get; set; }
		string[] Args { get; set; }
		List<string> Switches { get; set; }
		bool Cancel { get; set; }
	}
}
