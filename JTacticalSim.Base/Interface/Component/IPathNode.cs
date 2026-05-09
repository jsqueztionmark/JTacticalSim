using System;
using System.Text;

namespace JTacticalSim.API.Component
{
	public interface IPathNode : IPathableObject
	{
		bool HasOverrideInDirection { get; set; }
	}
}
