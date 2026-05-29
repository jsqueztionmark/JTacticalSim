using JTacticalSim.API;
using JTacticalSim.API.Game;

namespace JTacticalSim.GUI.Render;

public abstract class BaseScreenRenderer : BaseGameObject, IScreenRenderer
{
    protected MonoGameRenderer _baseRenderer { get; }

    protected BaseScreenRenderer(MonoGameRenderer baseRenderer)
        : base(GameObjectType.RENDER)
    {
        _baseRenderer = baseRenderer;
    }

    public abstract void RenderScreen();

    public virtual void CloseScreen()
    {
        TheGame().StateSystem.ChangeState(StateType.GAME_IN_PLAY);
    }
}
