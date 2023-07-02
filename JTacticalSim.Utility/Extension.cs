using System;
using System.Collections.Generic;
using System.Text;

namespace JTacticalSim.Utility
{
	public static class Extension
	{
		public static string F(this string fmt, params object[] args)
		{
			return string.Format(fmt, args);
		}

		public static Direction Reverse(this Direction direction)
		{
			return Orienting.GetOppositeDirection(direction);
		}
	}
}
