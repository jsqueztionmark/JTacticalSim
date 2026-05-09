using System;
using System.Text;

namespace JTacticalSim.API.Component
{
	public interface IGameFileCopyable : IBaseComponent
	{
		string GameFileDirectory { get; set; }
		bool IsScenario { get; }
	}
}
