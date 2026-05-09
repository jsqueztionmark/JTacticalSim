using System;
using System.Text;

namespace JTacticalSim.API.Component
{
	public interface IComponentSet : IBaseComponent
	{
		string Path { get; set; }
	}
}
