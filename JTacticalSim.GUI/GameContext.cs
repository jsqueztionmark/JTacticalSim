using JTacticalSim.API;
using JTacticalSim.API.Game;
using JTacticalSim.GUI.Render;
using JTacticalSim.GUI.CommandProcessor;
using JTacticalSim.Media.Sound;
using JTacticalSim.Service;

namespace JTacticalSim.GUI;

public class GameContext : BaseGameObject
{
    static readonly object padlock = new object();

    private static volatile GameContext _instance;
    public static GameContext Instance
    {
        get
        {
            if (_instance == null)
                lock (padlock)
                    if (_instance == null) _instance = new GameContext();
            return _instance;
        }
    }

    private GameContext()
        : base(GameObjectType.GAME_BASE)
    { }

    public void InitializeGame(bool startUp = false)
    {
        TheGame().NullGame();
        TheGame().Create(new ServiceDependencies(),
                         new MonoGameRenderer(),
                         new SoundSystem(),
                         new MonoGameCommandProcessor());

        TheGame().StateSystem.GameStateChanged += On_GameStateChangedHandler;

        if (startUp)
            TheGame().StateSystem.ChangeState(StateType.TITLE_MENU);
    }

    public void On_GameStateChangedHandler(object sender, EventArgs e)
    {
        // MonoGame renders every frame via MonoGameHost.Draw(); no action needed here.
    }
}
