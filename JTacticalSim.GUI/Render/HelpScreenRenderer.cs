using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using JTacticalSim.GUI.Controls;

namespace JTacticalSim.GUI.Render;

public sealed class HelpScreenRenderer : BaseScreenRenderer
{
    private static readonly Color ColOverlay  = new(10, 15, 25);
    private static readonly Color ColPanelBg  = new(18, 28, 50);
    private static readonly Color ColBorder   = new(60, 100, 160);
    private static readonly Color ColTitle    = Color.White;
    private static readonly Color ColHeading  = Color.Yellow;
    private static readonly Color ColLabel    = new(200, 210, 230);
    private static readonly Color ColKey      = new(255, 200, 100);
    private static readonly Color ColHint     = new(120, 120, 140);

    public HelpScreenRenderer(Renderer baseRenderer) : base(baseRenderer) { }

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

        int panX = 50, panY = 50;
        int panW = winW - 100, panH = winH - 100;
        DrawPanel(sb, px, panX, panY, panW, panH, ColPanelBg, ColBorder);

        int lh = GetLineH(fnt);
        DrawTextCentered(sb, fnt, "Help", panX, panW, panY + 10, ColTitle);

        int sepY = panY + lh + 16;
        FillRect(sb, px, panX + 1, sepY, panW - 2, 1, ColBorder);

        int colY  = sepY + 14;
        int col1X = panX + 30;
        int col2X = panX + panW / 2 + 20;

        DrawKeyboardColumn(sb, fnt, col1X, colY, lh);
        DrawPopupColumn(sb, fnt, col2X, colY, lh);

        // Hint at bottom
        int hintSepY = panY + panH - lh - 14;
        FillRect(sb, px, panX + 1, hintSepY, panW - 2, 1, ColBorder);
        DrawTextCentered(sb, fnt, "Esc: Close", panX, panW, hintSepY + 6, ColHint);
    }

    public void HandleInput(KeyboardState cur, KeyboardState prev)
    {
        if (JustPressed(cur, prev, Keys.Escape))
            CloseScreen();
    }

    // ── Column renderers ─────────────────────────────────────────────────────

    private void DrawKeyboardColumn(SpriteBatch sb, SpriteFont fnt, int x, int y, int lh)
    {
        const int KeyTab = 340;

        DrawText(sb, fnt, "Main Game Keyboard Commands", x, y, ColHeading);
        y += lh + 4;

        void Row(string label, string key, bool gap = false)
        {
            if (gap) y += lh / 2;
            DrawText(sb, fnt, label, x, y, ColLabel);
            DrawText(sb, fnt, key,   x + KeyTab, y, ColKey);
            y += lh;
        }

        Row("Move cursor",                      "NumPad 1/2/3/4/6/7/8/9");
        Row("Node action / map menu",            "NumPad 5  or  Enter");
        Row("Zoom in / out",                     "+  /  -",           gap: true);
        Row("Cycle map mode",                    "Shift + +/-");
        Row("Scroll viewport",                   "Shift + Arrows",    gap: true);
        Row("Select top unit / scroll stack",    "Space",             gap: true);
        Row("Select unit w/ attached",           "Ctrl + Alt + Space");
        Row("Select all units at location",      "Ctrl + Shift + Space");
        Row("Unselect all units",                "Shift + Space");
        Row("Open reinforcements screen",        "Ctrl + R",          gap: true);
        Row("Open unit quick-select screen",     "Ctrl + U");
        Row("Open main menu",                    "M",                 gap: true);
        Row("Open help",                         "H");
        Row("End turn",                          "Ctrl + End",        gap: true);
        Row("Quit",                              "Ctrl + Q");
    }

    private void DrawPopupColumn(SpriteBatch sb, SpriteFont fnt, int x, int y, int lh)
    {
        const int KeyTab = 260;

        DrawText(sb, fnt, "Popup / Menu Controls", x, y, ColHeading);
        y += lh + 4;

        void Row(string label, string key, bool gap = false)
        {
            if (gap) y += lh / 2;
            DrawText(sb, fnt, label, x, y, ColLabel);
            DrawText(sb, fnt, key,   x + KeyTab, y, ColKey);
            y += lh;
        }

        Row("Navigate items",    "Up / Down");
        Row("Confirm selection", "Enter  or  NumPad 5");
        Row("Cancel / close",    "Esc");

        y += lh;
        DrawText(sb, fnt, "Main Menu Items", x, y, ColHeading);
        y += lh + 4;

        DrawText(sb, fnt, "Title Screen", x, y, ColLabel); y += lh;
        DrawText(sb, fnt, "Scenario Info", x, y, ColLabel); y += lh;
        DrawText(sb, fnt, "Save Game", x, y, ColLabel); y += lh;
        DrawText(sb, fnt, "Exit", x, y, ColLabel);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static bool JustPressed(KeyboardState cur, KeyboardState prev, Keys key)
        => cur.IsKeyDown(key) && !prev.IsKeyDown(key);
}
