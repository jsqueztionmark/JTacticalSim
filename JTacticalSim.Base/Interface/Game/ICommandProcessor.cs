using System;
using System.Text;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.Game
{
	public interface ICommandProcessor
	{
		void SetInputError<TResult, TObject>(IResult<TResult, TObject> result);
		void ProcessInput(StateType state);
		void ProcessCommand(Commands command);
	}
}
