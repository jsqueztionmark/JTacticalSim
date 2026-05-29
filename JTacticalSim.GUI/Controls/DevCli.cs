using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JTacticalSim.GUI.Controls;

/// <summary>
/// Developer CLI bar. Tab opens, Enter submits, Esc/Tab cancels.
/// Positioned at the bottom of the screen via Open(winW, winH).
/// </summary>
public sealed class DevCli
{
    // ── Style ────────────────────────────────────────────────────────────────

    private static readonly Color ColBg     = new(12, 18, 35);
    private static readonly Color ColBorder = new(80, 140, 220);
    private static readonly Color ColPrompt = new(80, 140, 220);
    private static readonly Color ColText   = new(220, 230, 255);
    private static readonly Color ColHint   = new(80, 90, 110);

    private const int Height = 36;
    private const int PadX   = 10;

    // ── State ────────────────────────────────────────────────────────────────

    public bool IsOpen { get; private set; }

    private readonly StringBuilder _sb  = new();
    private int _x, _y, _w;

    // ── Events ───────────────────────────────────────────────────────────────

    public event Action<string> CommandSubmitted;
    public event Action         Cancelled;

    // ── API ──────────────────────────────────────────────────────────────────

    public void Open(int winW, int winH)
    {
        _x    = 0;
        _w    = winW;
        _y    = winH - Height;
        _sb.Clear();
        IsOpen = true;
    }

    public void Close()
    {
        IsOpen = false;
        _sb.Clear();
    }

    public void HandleInput(KeyboardState cur, KeyboardState prev)
    {
        if (!IsOpen) return;

        if (JustPressed(cur, prev, Keys.Escape) || JustPressed(cur, prev, Keys.Tab))
        {
            IsOpen = false;
            Cancelled?.Invoke();
            return;
        }

        if (JustPressed(cur, prev, Keys.Enter))
        {
            string cmd = _sb.ToString().Trim();
            IsOpen = false;
            if (!string.IsNullOrEmpty(cmd))
                CommandSubmitted?.Invoke(cmd);
            else
                Cancelled?.Invoke();
            return;
        }

        if (JustPressed(cur, prev, Keys.Back) && _sb.Length > 0)
        {
            _sb.Remove(_sb.Length - 1, 1);
            return;
        }

        bool shift = cur.IsKeyDown(Keys.LeftShift) || cur.IsKeyDown(Keys.RightShift);
        foreach (var key in cur.GetPressedKeys())
        {
            if (!JustPressed(cur, prev, key)) continue;
            char? ch = KeyToChar(key, shift);
            if (ch.HasValue) _sb.Append(ch.Value);
        }
    }

    public void Draw(SpriteBatch sb, SpriteFont fnt, Texture2D px)
    {
        if (!IsOpen || sb == null || px == null) return;

        // Background + border
        sb.Draw(px, new Rectangle(_x, _y, _w, Height), ColBg);
        sb.Draw(px, new Rectangle(_x, _y, _w, 1),      ColBorder);

        if (fnt == null) return;

        float lh   = fnt.MeasureString("M").Y;
        float textY = _y + (Height - lh) / 2f;

        string prompt = "> ";
        sb.DrawString(fnt, prompt, new Vector2(_x + PadX, textY), ColPrompt);
        float promptW = fnt.MeasureString(prompt).X;

        sb.DrawString(fnt, _sb.ToString() + "_", new Vector2(_x + PadX + promptW, textY), ColText);

        string hint = "Enter: Execute    Esc/Tab: Cancel";
        float hintW = fnt.MeasureString(hint).X * 0.65f;
        sb.DrawString(fnt, hint,
                      new Vector2(_x + _w - hintW - PadX, textY),
                      ColHint, 0f, Vector2.Zero, 0.65f, SpriteEffects.None, 0f);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static bool JustPressed(KeyboardState cur, KeyboardState prev, Keys key)
        => cur.IsKeyDown(key) && !prev.IsKeyDown(key);

    private static char? KeyToChar(Keys key, bool shift)
    {
        if (key >= Keys.A && key <= Keys.Z)
            return shift ? (char)('A' + (key - Keys.A)) : (char)('a' + (key - Keys.A));
        if (key >= Keys.D0 && key <= Keys.D9 && !shift)
            return (char)('0' + (key - Keys.D0));
        return key switch
        {
            Keys.Space       => ' ',
            Keys.OemMinus    => shift ? '_' : '-',
            Keys.OemPlus     => shift ? '+' : '=',
            Keys.OemPeriod   => shift ? '>' : '.',
            Keys.OemComma    => shift ? '<' : ',',
            Keys.OemQuestion => shift ? '?' : '/',
            Keys.OemQuotes   => shift ? '"' : '\'',
            _                => null,
        };
    }
}
