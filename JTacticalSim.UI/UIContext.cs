using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using JTacticalSim.API;
using JTacticalSim.API.Game;
using JTacticalSim.Media.Sound;
using JTacticalSim.Service;
using JTacticalSim.UI.Input;
using JTacticalSim.UI.Render;

namespace JTacticalSim.UI;

public class UIContext : BaseGameObject
{
    static readonly object _lock = new();
    static volatile UIContext _instance;

    public static UIContext Instance
    {
        get
        {
            if (_instance == null)
                lock (_lock)
                    if (_instance == null)
                        _instance = new UIContext();
            return _instance;
        }
    }

    public static IGame Game => _instance?.TheGame();

    private TerminalGuiRenderer _renderer;
    private View _currentScreen;

    public IApplication App { get; private set; }
    public Window Shell { get; private set; }

    private UIContext() : base(GameObjectType.GAME_BASE) { }

    public void InitializeGame(bool startUp = false)
    {
        App = Application.Create().Init();
        Shell = new Window
        {
            X = 0, Y = 0,
            Width = Dim.Fill(), Height = Dim.Fill(),
            Title = "J Tactical Sim"
        };

        _renderer = new TerminalGuiRenderer();
        ResetGameEngine();

        if (startUp)
            TheGame().StateSystem.ChangeState(StateType.TITLE_MENU);
    }

    public void ResetGameEngine()
    {
        TheGame().NullGame();
        TheGame().Create(
            new ServiceDependencies(),
            _renderer,
            new SoundSystem(),
            new UICommandProcessor());

        TheGame().StateSystem.GameStateChanged += OnGameStateChanged;
    }

    public void LoadAndStartGame(string name)
    {
        ResetGameEngine();
        var result = TheGame().LoadGame(name);

        if (result == null || result.Status == ResultStatus.EXCEPTION)
        {
            _renderer.DisplayUserMessage(
                MessageDisplayType.ERROR,
                result?.Message ?? "Could not load game.",
                result?.ex);
            return;
        }

        TheGame().Start();
    }

    public void Run()
    {
        App.Run(Shell);
    }

    private void OnGameStateChanged(object sender, EventArgs e)
    {
        TheGame().StateSystem.Render();

        var newView = _renderer.CurrentView;
        if (newView == null) return;

        if (_currentScreen != null)
            Shell.Remove(_currentScreen);

        _currentScreen = newView;
        Shell.Add(_currentScreen);
    }
}
