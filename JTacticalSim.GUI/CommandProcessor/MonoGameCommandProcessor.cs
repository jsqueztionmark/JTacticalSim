using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;

namespace JTacticalSim.GUI.CommandProcessor;

public class MonoGameCommandProcessor : BaseGameObject, ICommandProcessor
{
    private readonly ICommandInterface _commandInterface;

    public MonoGameCommandProcessor()
        : base(GameObjectType.HANDLER)
    {
        _commandInterface = new CommandInterface(new MonoGameCommandHandler());
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
