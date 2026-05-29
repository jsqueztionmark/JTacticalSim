using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.Component;
using JTacticalSim.Utility;
using Microsoft.Xna.Framework.Input;

namespace JTacticalSim.GUI.CommandProcessor;

internal class MonoGameCommandHandler : InputCommandHandlerBase
{
    private KeyboardState _previousKeyState;

    public MonoGameCommandHandler()
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
        var renderer = (JTacticalSim.GUI.Render.MonoGameRenderer)TheGame().Renderer;
        renderer.TitleScreenRenderer.HandleInput(current, _previousKeyState);
        _previousKeyState = current;
    }

    protected override void Handle_GAME_IN_PLAY_Input(ICommandInterface ci)
    {
        // TODO: map keys/mouse to game commands
        _previousKeyState = Keyboard.GetState();
    }

    protected override void Handle_AI_IN_PLAY_Input(ICommandInterface ci)
    {
        // AI handles its own turn; no player input processed
    }

    protected override void Handle_BATTLE_Input(ICommandInterface ci)         { }
    protected override void Handle_REINFORCE_Input(ICommandInterface ci)       { }
    protected override void Handle_QUICK_SELECT_Input(ICommandInterface ci)    { }
    protected override void Handle_GAME_OVER_Input(ICommandInterface ci)       { }
    protected override void Handle_HELP_Input(ICommandInterface ci)            { }
    protected override void Handle_SCENARIO_INFO_Input(ICommandInterface ci)   { }
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
}
