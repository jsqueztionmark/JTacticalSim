using System.Collections.Generic;
using System.Reflection;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Media.Sound;

namespace JTacticalSim.API.Game
{
	public interface IInputCommandHandler : IInputCommandContainer
	{
		void GetCommandInput(ICommandInterface ci);
		void HandleInput(ICommandInterface ci, StateType state);
		void SetInputError(string message);
	}
}
