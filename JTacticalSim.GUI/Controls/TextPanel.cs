using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JTacticalSim.GUI.Controls;

/// <summary>
/// Bordered panel that displays scrollable read-only text lines.
/// Call Open() to position and load content; Draw() each frame.
/// Long lines are word-wrapped to fit the panel width on first Draw().
/// </summary>
public sealed class TextPanel
{
    // ── Layout ───────────────────────────────────────────────────────────────
    public int ItemHeight  { get; init; } = 20;
    public int PadX        { get; init; } = 8;
    public int TitleHeight { get; init; } = 22;

    // ── Style ────────────────────────────────────────────────────────────────
    public ControlStyle Style { get; init; } = ControlStyle.Default;

    // ── Runtime state ────────────────────────────────────────────────────────
    public int X      { get; private set; }
    public int Y      { get; private set; }
    public int Width  { get; private set; }
    public int Height { get; private set; }

    public string Title { get; set; } = string.Empty;

    private List<string> _rawLines  = new();   // lines as split from source text
    private List<string> _dispLines = new();   // word-wrapped lines for display
    private bool _needsWrap = true;

    private int _scrollOffset;
    private int _maxVisible;

    // ── API ──────────────────────────────────────────────────────────────────

    public void Open(int x, int y, int w, int h, string title, string text = "")
    {
        X = x; Y = y; Width = w; Height = h;
        Title = title;
        _scrollOffset = 0;
        int headerH = TitleHeight + Style.SectionGap + 1 + Style.SectionGap;
        _maxVisible  = Math.Max(1, (h - headerH - 4) / ItemHeight);
        SetLines(text);
    }

    public void SetContent(string text)
    {
        _scrollOffset = 0;
        SetLines(text);
    }

    public void ScrollUp()
    {
        if (_scrollOffset > 0) _scrollOffset--;
    }

    public void ScrollDown()
    {
        if (_scrollOffset + _maxVisible < _dispLines.Count) _scrollOffset++;
    }

    public void HandleInput(KeyboardState cur, KeyboardState prev)
    {
        if (JustPressed(cur, prev, Keys.Up)   || JustPressed(cur, prev, Keys.NumPad8)) ScrollUp();
        if (JustPressed(cur, prev, Keys.Down) || JustPressed(cur, prev, Keys.NumPad2)) ScrollDown();
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

        // Build wrapped lines once per content change
        if (_needsWrap) BuildWrappedLines(fnt);

        // Title (centred)
        if (!string.IsNullOrEmpty(Title))
        {
            float tw = fnt.MeasureString(Title).X;
            sb.DrawString(fnt, Title, new Vector2(X + (Width - tw) / 2f, Y + Style.TitlePadTop), Style.TitleText);
        }

        // Separator below title
        int sepY     = Y + TitleHeight + Style.SectionGap;
        int contentY = sepY + 1 + Style.SectionGap;
        sb.Draw(px, new Rectangle(X + 1, sepY, Width - 2, 1), Style.Separator);

        // Text lines
        int end = Math.Min(_scrollOffset + _maxVisible, _dispLines.Count);
        for (int i = _scrollOffset; i < end; i++)
            sb.DrawString(fnt, _dispLines[i], new Vector2(X + PadX, contentY + (i - _scrollOffset) * ItemHeight), Style.ItemText);
    }

    // ── Word-wrap ─────────────────────────────────────────────────────────────

    private void BuildWrappedLines(SpriteFont fnt)
    {
        int maxPx = Width - PadX * 2;
        _dispLines = new List<string>(_rawLines.Count);

        foreach (var line in _rawLines)
        {
            if (string.IsNullOrEmpty(line)) { _dispLines.Add(string.Empty); continue; }

            // If the line fits, add it directly
            if (fnt.MeasureString(line).X <= maxPx) { _dispLines.Add(line); continue; }

            // Word-wrap
            var words   = line.Split(' ');
            var current = new StringBuilder();

            foreach (var word in words)
            {
                if (current.Length == 0)
                {
                    current.Append(word);
                    continue;
                }

                string candidate = current + " " + word;
                if (fnt.MeasureString(candidate).X <= maxPx)
                {
                    current.Append(' ').Append(word);
                }
                else
                {
                    _dispLines.Add(current.ToString());
                    current.Clear().Append(word);
                }
            }

            if (current.Length > 0) _dispLines.Add(current.ToString());
        }

        _needsWrap = false;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void SetLines(string text)
    {
        _rawLines = (text ?? string.Empty)
                        .Split('\n')
                        .Select(l => SanitizeLine(l.TrimEnd('\r')))
                        .ToList();
        _needsWrap = true;
    }

    private static string SanitizeLine(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var sb = new StringBuilder(s.Length);
        foreach (char ch in s)
        {
            if (ch >= 32 && ch <= 126) { sb.Append(ch); continue; }
            switch (ch)
            {
                case '→': sb.Append('>');  break;
                case '←': sb.Append('<');  break;
                case '—': sb.Append('-');  break;
                case '–': sb.Append('-');  break;
                case '‘':
                case '’': sb.Append('\''); break;
                case '“':
                case '”': sb.Append('"');  break;
            }
        }
        return sb.ToString();
    }

    private static bool JustPressed(KeyboardState cur, KeyboardState prev, Keys key)
        => cur.IsKeyDown(key) && !prev.IsKeyDown(key);
}
