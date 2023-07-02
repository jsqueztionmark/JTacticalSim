using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.InfoObjects
{
	/// <summary>
	/// Encapsulates any necessary information about a movement route
	/// </summary>
	public class RouteInfo
	{
		public int Priority { get; set; }
		public IEnumerable<IPathNode> Nodes { get; private set; }
		public INode Source { get; private set; }
		public INode Target { get; private set; }

		public RouteInfo(IEnumerable<IPathNode> nodes, INode source, INode target)
		{
			Nodes = nodes;
			Source = source;
			Target = target;
		}
	}
}
