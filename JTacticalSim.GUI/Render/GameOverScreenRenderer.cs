using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using JTacticalSim.API;
using JTacticalSim.API.Game;
using JTacticalSim.API.Component.Util;
using JTacticalSim.GUI.Controls;

namespace JTacticalSim.GUI.Render;

public sealed class GameOverScreenRenderer : BaseScreenRenderer
{
    private static readonly Color ColOverlay = new(5, 5, 10);
    private static readonly Color ColPanelBg = new(25, 15, 15);
    private static readonly Color ColBorder  = new(180, 30, 30);
    private static readonly Color ColTitle   = new(255, 100, 100);
    private static readonly Color ColText    = new(220, 200, 200);
    private static readonly Color ColHint    = new(150, 120, 120);

    public GameOverScreenRenderer(Renderer baseRenderer) : base(baseRenderer) { }

    public override void CloseScreen()
    {
        TheGame().StateSystem.ChangeState(StateType.TITLE_MENU);
    }

    public override void RenderScreen()
    {
        var sb  = _baseRenderer.SpriteBatch;
        var fnt = _baseRenderer.Font;
        var px  = _baseRenderer.Pixel;
        if (sb == null || fnt == null || px == null) return;

        var gd   = _baseRenderer.GraphicsDevice;
        int winW = gd.Viewport.Width;
        int winH = gd.Viewport.Height;

        FillRect(sb, px, 0, 0, winW, winH, ColOverlay);

        const int panW = 800;
        int panH = 360;
        int panX = (winW - panW) / 2;
        int panY = (winH - panH) / 2;
        DrawPanel(sb, px, panX, panY, panW, panH, ColPanelBg, ColBorder);

        int lh = GetLineH(fnt);
        DrawTextCentered(sb, fnt, "Game Over!!!", panX, panW, panY + 12, ColTitle);

        int sepY = panY + lh + 18;
        FillRect(sb, px, panX + 1, sepY, panW - 2, 1, ColBorder);

        int y = sepY + 16;

        // Scenario name
        var scenario = TheGame()?.LoadedScenario;
        if (scenario != null)
        {
            DrawTextCentered(sb, fnt, scenario.Name, panX, panW, y, ColText);
            y += lh + lh;
        }

        // Faction VP standings
        var players = TheGame()?.GetPlayers();
        if (players != null)
        {
            var factions = players
                .Select(p => p.Country.Faction)
                .DistinctBy(f => f.ID)
                .ToList();

            foreach (var f in factions)
            {
                int? vp = f.GetCurrentVictoryPoints();
                string line = $"{f.Name}  —  VP: {vp?.ToString() ?? "—"}";
                DrawTextCentered(sb, fnt, line, panX, panW, y, ColText);
                y += lh;
            }
        }

        // Hint
        int hintSepY = panY + panH - lh - 14;
        FillRect(sb, px, panX + 1, hintSepY, panW - 2, 1, ColBorder);
        DrawTextCentered(sb, fnt, "Esc: Return to Title Screen", panX, panW, hintSepY + 6, ColHint);
    }

    public void HandleInput(KeyboardState cur, KeyboardState prev)
    {
        if (JustPressed(cur, prev, Keys.Escape))
            CloseScreen();
    }

    private static bool JustPressed(KeyboardState cur, KeyboardState prev, Keys key)
        => cur.IsKeyDown(key) && !prev.IsKeyDown(key);
}
