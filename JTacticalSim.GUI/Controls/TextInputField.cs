using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JTacticalSim.GUI.Controls;

/// <summary>
/// Single-line text entry control rendered via SpriteBatch.
/// Call Open() to position; Draw() each frame; wire events for confirm/cancel.
/// </summary>
public sealed class TextInputField
{
    // ── Layout ───────────────────────────────────────────────────────────────
    public int MaxLength { get; init; } = 30;
    public int PadX      { get; init; } = 8;

    // ── Style ────────────────────────────────────────────────────────────────
    public ControlStyle Style { get; init; } = ControlStyle.Default;

    // ── Content ──────────────────────────────────────────────────────────────
    public string Label { get; set; } = string.Empty;

    // ── Runtime state ────────────────────────────────────────────────────────
    public int    X      { get; private set; }
    public int    Y      { get; private set; }
    public int    Width  { get; private set; }
    public int    Height { get; private set; }
    public string Text   => _sb.ToString();

    private readonly StringBuilder _sb = new();

    // ── Events ───────────────────────────────────────────────────────────────
    /// <summary>Fired when the user presses Enter with non-empty text.</summary>
    public event Action<string> TextConfirmed;
    /// <summary>Fired when the user presses Escape.</summary>
    public event Action Cancelled;
    /// <summary>Optionally validate text before firing TextConfirmed. Return an error message or null.</summary>
    public Func<string, string> Validate { get; set; }

    private string _errorMessage;

    // ── API ──────────────────────────────────────────────────────────────────

    public void Open(int x, int y, int w, int h)
    {
        X = x; Y = y; Width = w; Height = h;
        _sb.Clear();
        _errorMessage = null;
    }

    public void HandleInput(KeyboardState cur, KeyboardState prev)
    {
        if (JustPressed(cur, prev, Keys.Escape))
        {
            _errorMessage = null;
            Cancelled?.Invoke();
            return;
        }

        if (JustPressed(cur, prev, Keys.Enter))
        {
            var text = _sb.ToString().Trim();
            if (string.IsNullOrEmpty(text)) return;

            var err = Validate?.Invoke(text);
            if (err != null) { _errorMessage = err; return; }

            _errorMessage = null;
            TextConfirmed?.Invoke(text);
            return;
        }

        if (JustPressed(cur, prev, Keys.Back) && _sb.Length > 0)
        {
            _sb.Remove(_sb.Length - 1, 1);
            _errorMessage = null;
            return;
        }

        if (_sb.Length >= MaxLength) return;

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
        if (sb == null || px == null) return;

        // Background + border
        sb.Draw(px, new Rectangle(X,             Y,              Width,  Height), Style.Background);
        sb.Draw(px, new Rectangle(X,             Y,              Width,  1),      Style.Border);
        sb.Draw(px, new Rectangle(X,             Y + Height - 1, Width,  1),      Style.Border);
        sb.Draw(px, new Rectangle(X,             Y,              1,      Height), Style.Border);
        sb.Draw(px, new Rectangle(X + Width - 1, Y,              1,      Height), Style.Border);

        if (fnt == null) return;

        float lh  = fnt.MeasureString("M").Y;
        float textY = Y + (Height - lh) / 2f;
        float x   = X + PadX;

        // Label
        if (!string.IsNullOrEmpty(Label))
        {
            sb.DrawString(fnt, Label + ": ", new Vector2(x, textY), Style.TitleText);
            x += fnt.MeasureString(Label + ": ").X;
        }

        // Typed text + blinking cursor placeholder
        sb.DrawString(fnt, _sb.ToString() + "_", new Vector2(x, textY), Style.ItemText);

        // Error message below the field
        if (!string.IsNullOrEmpty(_errorMessage))
        {
            sb.DrawString(fnt, _errorMessage,
                          new Vector2(X + PadX, Y + Height + 4),
                          new Color(220, 80, 80));
        }
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
            Keys.OemPeriod   => shift ? '>' : '.',
            Keys.OemComma    => shift ? '<' : ',',
            Keys.OemMinus    => shift ? '_' : '-',
            Keys.OemPlus     => shift ? '+' : '=',
            Keys.OemQuestion => shift ? '?' : '/',
            Keys.OemQuotes   => shift ? '"' : '\'',
            _                => null,
        };
    }
}
