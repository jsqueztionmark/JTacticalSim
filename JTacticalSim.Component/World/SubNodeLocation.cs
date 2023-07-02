using System;
using System.Collections.Generic;
using System.Text;
using JTacticalSim.API.Component;

namespace JTacticalSim.Component.World
{
	public class SubNodeLocation : ISubNodeLocation
	{
		public int Value { get; private set; }

		public SubNodeLocation(int value)
		{
			Value = value;
		}
	}
}
