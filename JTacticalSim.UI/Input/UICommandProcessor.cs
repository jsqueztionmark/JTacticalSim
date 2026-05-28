using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;

namespace JTacticalSim.UI.Input;

public class UICommandProcessor : BaseGameObject, ICommandProcessor
{
    public UICommandProcessor() : base(GameObjectType.HANDLER) { }

    // Input flows through Terminal.Gui key events on individual views.
    public void ProcessInput(StateType state) { }

    public void ProcessCommand(Commands command) { }

    public void SetInputError<TResult, TObject>(IResult<TResult, TObject> result) { }
}
