using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.InfoObjects
{
	/// <summary>
	/// Carries Minimum and Maximum values for a strategic node metric type
	/// Used to determine actual relational strategic metrics for a scenario/board
	/// </summary>
	public class StrategicAttributeInfo
	{		
		/// <summary>
		/// The minimum metric value represented on the board
		/// </summary>
		public double Min { get; private set; }

		/// <summary>
		/// The maximum metric value represented on the board
		/// </summary>
		public double Max { get; private set; }

		public double Range { get { return GetRangeForMinMaxValues(Max, Min); }}

		public StrategicAttributeInfo(double min, double max)
		{
			Max = max;
			Min = min;
		}

		private double GetRangeForMinMaxValues(double max, double min)
		{
			return (max - min);
			//return (!(max > 0)) ? 0 : max - min;
		}
	}
}
