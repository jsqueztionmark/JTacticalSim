using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.Utility
{
	public static class Enums
	{
		public static int? LowestEnumValue<T>(T e)
			where T : struct, IConvertible
		{
			if (!typeof (T).IsEnum)
			{
				throw new ArgumentException("T must be an enumerated type");
			}

			var items = Enum.GetValues(typeof (T));
			return Convert.ToInt32(items.GetValue(items.GetLowerBound(0)));
		}

		public static int? HighestEnumValue<T>(T e)
			where T : struct, IConvertible
		{
			if (!typeof (T).IsEnum)
			{
				throw new ArgumentException("T must be an enumerated type");
			}

			var items = Enum.GetValues(typeof (T));
			return Convert.ToInt32(items.GetValue(items.GetUpperBound(0)));
		}
	}
}
