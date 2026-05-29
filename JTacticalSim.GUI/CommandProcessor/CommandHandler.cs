using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.Component;
using JTacticalSim.Utility;
using Microsoft.Xna.Framework.Input;

namespace JTacticalSim.GUI.CommandProcessor;

internal class CommandHandler : InputCommandHandlerBase
{
    private KeyboardState    _previousKeyState;
    private ICommandInterface _pendingCi;

    public CommandHandler()
    {
        _previousKeyState = Keyboard.GetState();
    }

    // ── Input polling ────────────────────────────────────────────────────────

    public override void GetCommandInput(ICommandInterface ci)
    {
        // MonoGame polls input each frame; no blocking read needed
    }

    // ── Per-state input handlers ─────────────────────────────────────────────

    protected override void Handle_TITLE_MENU_Input(ICommandInterface ci)
    {
        var current = Keyboard.GetState();
        if (!HandleOverlay(current)) Renderer().TitleScreenRenderer.HandleInput(current, _previousKeyState);
        _previousKeyState = current;
    }

    protected override void Handle_GAME_IN_PLAY_Input(ICommandInterface ci)
    {
        var current  = Keyboard.GetState();
        var renderer = Renderer();

        if (HandleOverlay(current))         { _previousKeyState = current; return; }
        if (HandleDevCli(current, ci))      { _previousKeyState = current; return; }

        // Tab opens the dev CLI
        if (JustPressed(current, _previousKeyState, Keys.Tab))
        {
            var cli = renderer.DevCli;
            cli.Open(renderer.GraphicsDevice.Viewport.Width,
                     renderer.GraphicsDevice.Viewport.Height);
            // Subscribe fresh each open to avoid double-firing on reopen
            cli.CommandSubmitted -= OnDevCliCommandWrapper;
            cli.CommandSubmitted += OnDevCliCommandWrapper;
            _pendingCi = ci;
            _previousKeyState = current;
            return;
        }

        renderer.MainScreenRenderer.HandleInput(current, _previousKeyState);
        _previousKeyState = current;
    }

    protected override void Handle_AI_IN_PLAY_Input(ICommandInterface ci)
    {
        // AI handles its own turn; no player input processed
    }

    protected override void Handle_BATTLE_Input(ICommandInterface ci)
    {
        var current = Keyboard.GetState();
        HandleOverlay(current);
        _previousKeyState = current;
    }

    protected override void Handle_REINFORCE_Input(ICommandInterface ci)
    {
        var current = Keyboard.GetState();
        if (!HandleOverlay(current)) Renderer().ReinforcementsScreenRenderer.HandleInput(current, _previousKeyState);
        _previousKeyState = current;
    }

    protected override void Handle_QUICK_SELECT_Input(ICommandInterface ci)
    {
        var current = Keyboard.GetState();
        if (!HandleOverlay(current)) Renderer().QuickSelectRenderer.HandleInput(current, _previousKeyState);
        _previousKeyState = current;
    }

    protected override void Handle_GAME_OVER_Input(ICommandInterface ci)
    {
        var current = Keyboard.GetState();
        if (!HandleOverlay(current)) Renderer().GameOverScreenRenderer.HandleInput(current, _previousKeyState);
        _previousKeyState = current;
    }

    protected override void Handle_HELP_Input(ICommandInterface ci)
    {
        var current = Keyboard.GetState();
        if (!HandleOverlay(current)) Renderer().HelpScreenRenderer.HandleInput(current, _previousKeyState);
        _previousKeyState = current;
    }

    protected override void Handle_SCENARIO_INFO_Input(ICommandInterface ci)
    {
        var current = Keyboard.GetState();
        if (!HandleOverlay(current)) Renderer().ScenarioInfoScreenRenderer.HandleInput(current, _previousKeyState);
        _previousKeyState = current;
    }

    protected override void Handle_SETTINGS_MENU_Input(ICommandInterface ci)   { }
    protected override void Handle_SPLASH_SCREEN_Input(ICommandInterface ci)   { }

    // ── Console-era interaction stubs (replaced by MonoGame input/UI) ────────

    protected override string HandleCMDInput(string message) => string.Empty;
    protected override Command GetNodeAction() => null;
    protected override Command GetMainMenuAction() => null;
    protected override Command GetHelpMenuAction() => null;
    protected override IUnit SelectUnit(IEnumerable<IUnit> units) => null;
    protected override IDemographic SelectDemographic(IEnumerable<IDemographic> demographics, string action) => null;
    protected override IResult<Direction, Direction> SelectOrientation(IEnumerable<Direction> directions) => null;
    protected override string GetSaveGameAsTitle() => string.Empty;
    protected override bool IsValidRow(string input) => false;
    protected override bool IsValidColumn(string input) => false;
    protected override bool IsValidLocationEntered() => false;
    protected override MouseButton GetValidMouseButtonClick(string input) => MouseButton.LEFT;

    protected override void On_MenuClickAction(object sender, EventArgs e)          { }
    protected override void On_MapMenuErased(object sender, EventArgs e)            { }
    protected override void On_MainMenuErased(object sender, EventArgs e)           { }
    protected override void On_CmdBoxErased(object sender, EventArgs e)             { }
    protected override void On_CmdBoxEscapePressed(object sender, EventArgs e)      { }
    protected override void On_SaveAsGameTitleEscapePressed(object sender, EventArgs e) { }
    protected override void On_SaveAsGameTitleEntered(object sender, EventArgs e)   { }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Routes input to the modal overlay if it is visible. Returns true if consumed.</summary>
    private bool HandleOverlay(KeyboardState current)
    {
        var overlay = Renderer().Overlay;
        if (!overlay.IsVisible) return false;
        overlay.HandleInput(current, _previousKeyState);
        return true;
    }

    /// <summary>Routes input to the dev CLI if open. Returns true if consumed.</summary>
    private bool HandleDevCli(KeyboardState current, ICommandInterface ci)
    {
        var cli = Renderer().DevCli;
        if (!cli.IsOpen) return false;
        cli.HandleInput(current, _previousKeyState);
        return true;
    }

    // ── Dev CLI command execution ─────────────────────────────────────────────

    private void OnDevCliCommandWrapper(string input) => OnDevCliCommand(input, _pendingCi);

    private void OnDevCliCommand(string input, ICommandInterface ci)
    {
        var parts    = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var command  = parts.Length > 0 ? parts[0].ToLowerInvariant() : string.Empty;
        var switches = System.Array.FindAll(parts, p => p.StartsWith("--"));
        var args     = System.Array.FindAll(parts, p => !p.StartsWith("--") && p != command);
        string msg   = args.Length > 0 ? string.Join(" ", args) : "Test message from dev CLI.";

        switch (command)
        {
            case "show-modal-overlay":
            case "smo":
                string sw = switches.Length > 0 ? switches[0].ToLowerInvariant() : "--info";
                switch (sw)
                {
                    case "--error":
                        TheGame().Renderer.DisplayUserMessage(API.MessageDisplayType.ERROR,   msg, null); break;
                    case "--warning":
                        TheGame().Renderer.DisplayUserMessage(API.MessageDisplayType.WARNING, msg, null); break;
                    case "--confirm":
                        TheGame().Renderer.ConfirmAction(msg); break;
                    case "--report":
                        TheGame().Renderer.DisplayTaskExecutionReport(
                            new System.Text.StringBuilder(msg)); break;
                    default:
                        TheGame().Renderer.DisplayUserMessage(API.MessageDisplayType.INFO,    msg, null); break;
                }
                break;

            case "show-game-over":
            case "sgo":
                TheGame().StateSystem.ChangeState(API.StateType.GAME_OVER);
                break;

            case "show-battle-report":
            case "sbr":
                TheGame().Renderer.DisplayTaskExecutionReport(
                    new System.Text.StringBuilder(
                        "Type:     Local Combat\nAttacker: Germany\nDefender: USA\n\n" +
                        "Round 1 - Air Defence\n  (no air defence skirmishes)\n\n" +
                        "Round 2 - Full Combat\n  Armor 1st Panzer attacks Infantry 3rd Infantry  [Attacker wins]\n" +
                        "  Artillery 5th Arty fires on Armor 2nd Armored  [Defender holds]\n\n" +
                        "Result: Attackers Victorious"));
                break;
            case "show-battle-screen":
            case "sbs":
                TheGame().StateSystem.ChangeState(API.StateType.BATTLE);
                break;

            default:
                // Fall through to engine command dispatch
                ParseCommandArgs(input);
                RunCommand(ci);
                break;
        }
    }

    private static bool JustPressed(KeyboardState cur, KeyboardState prev, Keys key)
        => cur.IsKeyDown(key) && !prev.IsKeyDown(key);

    private JTacticalSim.GUI.Render.Renderer Renderer()
        => (JTacticalSim.GUI.Render.Renderer)TheGame().Renderer;
}
