using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.InfoObjects
{
	public class ZoomInfo
	{
		public int RowSpacing { get; set; }
 		public int ColumnSpacing { get; set; }
		public ZoomLevel Level { get; set; }
		public int DrawWidth { get; set; }
		public int DrawHeight { get; set; }
		public ICoordinate CurrentOrigin { get; set; }
		public bool IsCurrent { get; set; }
	}
}
