using JTacticalSim.API.Component;

namespace JTacticalSim.API.Game
{
	public interface ICommandInterface
	{
		event CommandCancelHandler CancelCommand;
		void SetInputError<TResult, TObject>(IResult<TResult, TObject> result);
		void GetCommand();
		void HandleInput(StateType state);
		void RunCommand(Commands command);
	}
}
