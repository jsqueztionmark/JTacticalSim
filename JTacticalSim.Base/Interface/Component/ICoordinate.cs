using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.Component
{
	public interface ICoordinate
	{
		int X { get; set; }
		int Y { get; set; }
		int Z { get; set; }

		/// <summary>
		/// Returns the coordinate in the format x_y_z
		/// </summary>
		/// <returns></returns>
		string ToStringForName();
	}
}
