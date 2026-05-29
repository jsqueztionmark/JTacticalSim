using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JTacticalSim.API;
using JTacticalSim.API.Game;

namespace JTacticalSim.GUI.Render;

public abstract class BaseScreenRenderer : BaseGameObject, IScreenRenderer
{
    protected Renderer _baseRenderer { get; }

    protected BaseScreenRenderer(Renderer baseRenderer)
        : base(GameObjectType.RENDER)
    {
        _baseRenderer = baseRenderer;
    }

    public abstract void RenderScreen();

    public virtual void CloseScreen()
    {
        TheGame().StateSystem.ChangeState(StateType.GAME_IN_PLAY);
    }

    // ── Shared draw helpers ──────────────────────────────────────────────────

    protected static void FillRect(SpriteBatch sb, Texture2D px, int x, int y, int w, int h, Color c)
        => sb.Draw(px, new Rectangle(x, y, w, h), c);

    protected static void DrawBorder(SpriteBatch sb, Texture2D px, int x, int y, int w, int h, Color c, int t = 1)
    {
        sb.Draw(px, new Rectangle(x,         y,         w, t), c);
        sb.Draw(px, new Rectangle(x,         y + h - t, w, t), c);
        sb.Draw(px, new Rectangle(x,         y,         t, h), c);
        sb.Draw(px, new Rectangle(x + w - t, y,         t, h), c);
    }

    protected static void DrawPanel(SpriteBatch sb, Texture2D px, int x, int y, int w, int h, Color bg, Color border)
    {
        FillRect(sb, px, x, y, w, h, bg);
        DrawBorder(sb, px, x, y, w, h, border);
    }

    protected static void DrawText(SpriteBatch sb, SpriteFont fnt, string text, float x, float y, Color c, float scale = 1f)
    {
        if (string.IsNullOrEmpty(text)) return;
        text = Sanitize(text);
        if (scale == 1f)
            sb.DrawString(fnt, text, new Vector2(x, y), c);
        else
            sb.DrawString(fnt, text, new Vector2(x, y), c, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    protected static void DrawTextCentered(SpriteBatch sb, SpriteFont fnt, string text, int panelX, int panelW, float y, Color c)
    {
        if (string.IsNullOrEmpty(text) || fnt == null) return;
        text = Sanitize(text);
        float tw = fnt.MeasureString(text).X;
        sb.DrawString(fnt, text, new Vector2(panelX + (panelW - tw) / 2f, y), c);
    }

    protected static int GetLineH(SpriteFont fnt) => fnt == null ? 18 : (int)fnt.MeasureString("M").Y + 2;

    /// <summary>
    /// Strip/replace characters outside the font's ASCII 32-126 range.
    /// Engine TextInfo() methods emit Unicode arrows and dashes that the spritefont can't resolve.
    /// </summary>
    protected internal static string Sanitize(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var sb = new System.Text.StringBuilder(text.Length);
        foreach (char ch in text)
        {
            if (ch >= 32 && ch <= 126) { sb.Append(ch); continue; }
            switch (ch)
            {
                case '\n': sb.Append('\n'); break;
                case '\r': break;
                case '→': sb.Append('>');  break;  // →
                case '←': sb.Append('<');  break;  // ←
                case '—': sb.Append('-');  break;  // —
                case '–': sb.Append('-');  break;  // –
                case '‘':
                case '’': sb.Append('\''); break;  // ' '
                case '“':
                case '”': sb.Append('"');  break;  // " "
                // skip everything else silently
            }
        }
        return sb.ToString();
    }
}
