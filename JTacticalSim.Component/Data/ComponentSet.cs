using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;

namespace JTacticalSim.Component.Data
{
	public class ComponentSet : GameComponentBase, IComponentSet
	{
		public string Path { get; set; }
	}
}
