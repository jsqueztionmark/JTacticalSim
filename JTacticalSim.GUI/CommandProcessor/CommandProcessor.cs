using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;

namespace JTacticalSim.GUI.CommandProcessor;

public class CommandProcessor : BaseGameObject, ICommandProcessor
{
    private readonly ICommandInterface _commandInterface;

    public CommandProcessor()
        : base(GameObjectType.HANDLER)
    {
        _commandInterface = new CommandInterface(new CommandHandler());
    }

    public void SetInputError<TResult, TObject>(IResult<TResult, TObject> result)
    {
        _commandInterface.SetInputError(result);
    }

    public void ProcessInput(StateType state)
    {
        _commandInterface.HandleInput(state);
    }

    public void ProcessCommand(Commands command)
    {
        _commandInterface.RunCommand(command);
    }
}
