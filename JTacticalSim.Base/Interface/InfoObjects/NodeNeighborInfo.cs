using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.Utility;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.InfoObjects
{
	public struct NodeNeighborInfo
	{
		public Guid NodeReference { get; set; }
		public INode Node { get; set; }
		public Direction Direction { get; set; }
	}
}
