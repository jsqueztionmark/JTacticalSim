using JTacticalSim.API;
using JTacticalSim.API.Game;
using JTacticalSim.GUI.Render;
using JTacticalSim.GUI.CommandProcessor;
using JTacticalSim.GUI.Sound;
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
        // Register MonoGameSoundHandler for all SoundSystem instances — including
        // per-component ones created by GameComponentBase (unit classes, unit types, etc.)
        SoundHandlerFactory.Instance.RegisterHandlerFactory(() => new MonoGameSoundHandler());

        TheGame().NullGame();
        TheGame().Create(new ServiceDependencies(),
                         new Renderer(),
                         new SoundSystem(),
                         new CommandProcessor.CommandProcessor());

        TheGame().StateSystem.GameStateChanged += On_GameStateChangedHandler;

        if (startUp)
            TheGame().StateSystem.ChangeState(StateType.TITLE_MENU);
    }

    public void On_GameStateChangedHandler(object sender, EventArgs e)
    {
        // MonoGame renders every frame via MonoGameHost.Draw(); no action needed here.
    }
}
