using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using JTacticalSim.API.Game;
using JTacticalSim.API.Component.Util;
using JTacticalSim.GUI.Controls;

namespace JTacticalSim.GUI.Render;

public sealed class ScenarioInfoScreenRenderer : BaseScreenRenderer
{
    private static readonly Color ColOverlay = new(10, 15, 25);
    private static readonly Color ColPanelBg = new(18, 28, 50);
    private static readonly Color ColBorder  = new(60, 100, 160);
    private static readonly Color ColTitle   = Color.White;

    private const int PanelW = 900;
    private const int PanelH = 620;

    private readonly TextPanel _contentPanel = new()
    {
        Style    = ControlStyle.Default,
        ItemHeight = 20,
        PadX     = 12,
    };

    private bool _initialized;

    public ScenarioInfoScreenRenderer(Renderer baseRenderer) : base(baseRenderer) { }

    public override void CloseScreen()
    {
        _initialized = false;
        base.CloseScreen();
    }

    public override void RenderScreen()
    {
        var sb  = _baseRenderer.SpriteBatch;
        var fnt = _baseRenderer.Font;
        var px  = _baseRenderer.Pixel;
        if (sb == null || fnt == null || px == null) return;

        var gd = _baseRenderer.GraphicsDevice;
        int winW = gd.Viewport.Width;
        int winH = gd.Viewport.Height;

        if (!_initialized) Initialize(winW, winH);

        // Dark overlay
        FillRect(sb, px, 0, 0, winW, winH, ColOverlay);

        // Outer panel border (the content panel draws its own background/border)
        int panX = (winW - PanelW) / 2;
        int panY = (winH - PanelH) / 2;
        DrawPanel(sb, px, panX, panY, PanelW, PanelH, ColPanelBg, ColBorder);

        // Title bar
        int lh = GetLineH(fnt);
        DrawTextCentered(sb, fnt, TheGame()?.LoadedScenario?.Name ?? "Scenario Info",
                         panX, PanelW, panY + 10, ColTitle);
        FillRect(sb, px, panX + 1, panY + lh + 16, PanelW - 2, 1, ColBorder);

        // Scrollable content
        _contentPanel.Draw(sb, fnt, px);
    }

    public void HandleInput(KeyboardState cur, KeyboardState prev)
    {
        if (JustPressed(cur, prev, Keys.Escape))
            CloseScreen();
        else
            _contentPanel.HandleInput(cur, prev);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void Initialize(int winW, int winH)
    {
        var scenario = TheGame()?.LoadedScenario;
        string text  = scenario?.TextInfo() ?? string.Empty;

        int panX = (winW - PanelW) / 2;
        int panY = (winH - PanelH) / 2;

        // Content panel sits below the title bar inside the outer panel
        int contentTop = panY + 42;
        _contentPanel.Open(panX + 8, contentTop, PanelW - 16, PanelH - 42 - 8,
                           string.Empty, text);

        _initialized = true;
    }

    private static bool JustPressed(KeyboardState cur, KeyboardState prev, Keys key)
        => cur.IsKeyDown(key) && !prev.IsKeyDown(key);
}
