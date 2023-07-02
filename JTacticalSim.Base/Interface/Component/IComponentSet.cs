using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.Component
{
	public interface IComponentSet : IBaseComponent
	{
		string Path { get; set; }
	}
}
