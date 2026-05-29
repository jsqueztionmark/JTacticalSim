using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using JTacticalSim.API;

namespace JTacticalSim.GUI.Controls;

/// <summary>
/// Full-screen modal dialog rendered over the current game state.
/// Supports three prompt modes: any-key message, Y/N confirmation, and scrollable report.
/// </summary>
public sealed class ModalOverlay
{
    // ── Types ────────────────────────────────────────────────────────────────

    public enum PromptMode { Message, Confirm, Report }

    // ── Colors per message type ──────────────────────────────────────────────

    private static readonly (Color Bg, Color Border, Color Text, Color Caption, string Title) StyleError =
        (new Color(140, 25, 25), Color.White,         Color.White, Color.White, "Error!");

    private static readonly (Color Bg, Color Border, Color Text, Color Caption, string Title) StyleWarning =
        (new Color(200, 175, 0),  new Color(80, 60, 0), new Color(30, 20, 0), new Color(60, 40, 0), "Warning!");

    private static readonly (Color Bg, Color Border, Color Text, Color Caption, string Title) StyleInfo =
        (new Color(0, 110, 130),  Color.White,          Color.White, Color.White, "Info!");

    private static readonly Color ColOverlay  = new(0, 0, 0, 160);
    private static readonly Color ColHint     = new(200, 200, 200, 200);

    // ── Layout ───────────────────────────────────────────────────────────────

    private const int PanelW    = 720;
    private const int PadX      = 16;
    private const int TitleH    = 28;
    private const int LineH     = 20;
    private const int HintH     = 26;

    // ── Runtime state ────────────────────────────────────────────────────────

    public bool       IsVisible   { get; private set; }
    public bool?      Result      { get; private set; }   // null = waiting, true/false = answered
    public PromptMode Mode        { get; private set; }

    public event Action Dismissed;

    private (Color Bg, Color Border, Color Text, Color Caption, string Title) _style;
    private string       _caption;
    private string       _rawText;
    private List<string> _wrappedLines = new();
    private bool         _needsWrap    = true;
    private int          _scrollOffset;
    private int          _maxBodyLines;

    // ── API ──────────────────────────────────────────────────────────────────

    public void ShowMessage(MessageDisplayType type, string message, Exception ex = null)
    {
        var display = ex != null ? $"{message}\n{ex.Message}" : message;
        Open(TypeStyle(type), PromptMode.Message, null, display);
    }

    public void ShowConfirm(string message)
    {
        Open(StyleWarning, PromptMode.Confirm, null, message);
    }

    public void ShowReport(string title, string reportText)
    {
        Open(StyleInfo, PromptMode.Report, title, reportText);
    }

    public void HandleInput(KeyboardState cur, KeyboardState prev)
    {
        if (!IsVisible) return;

        if (Mode == PromptMode.Confirm)
        {
            if (JustPressed(cur, prev, Keys.Y))             { Result = true;  Dismiss(); }
            else if (JustPressed(cur, prev, Keys.N))        { Result = false; Dismiss(); }
            else if (JustPressed(cur, prev, Keys.Escape))   { Result = false; Dismiss(); }
            return;
        }

        // Report: scroll with Up/Down, any other key dismisses
        if (Mode == PromptMode.Report)
        {
            if (JustPressed(cur, prev, Keys.Up)   || JustPressed(cur, prev, Keys.NumPad8))
                { if (_scrollOffset > 0) _scrollOffset--; return; }
            if (JustPressed(cur, prev, Keys.Down) || JustPressed(cur, prev, Keys.NumPad2))
                { if (_scrollOffset + _maxBodyLines < _wrappedLines.Count) _scrollOffset++; return; }
        }

        // Any non-modifier key dismisses
        foreach (var key in cur.GetPressedKeys())
        {
            if (!JustPressed(cur, prev, key)) continue;
            if (IsModifier(key)) continue;
            Dismiss();
            return;
        }
    }

    public void Draw(SpriteBatch sb, SpriteFont fnt, Texture2D px, int winW, int winH,
                     Microsoft.Xna.Framework.Rectangle mapArea = default)
    {
        if (!IsVisible || sb == null || px == null) return;

        // Dim the full screen
        sb.Draw(px, new Rectangle(0, 0, winW, winH), ColOverlay);

        if (fnt == null) return;

        if (_needsWrap) BuildWrappedLines(fnt);

        int bodyLines = Math.Min(_maxBodyLines, _wrappedLines.Count);
        int panelH    = TitleH + 8 + bodyLines * LineH + 12 + HintH + 8;

        // Centre within the map area when available, otherwise the full window
        int areaX = mapArea.IsEmpty ? 0     : mapArea.X;
        int areaY = mapArea.IsEmpty ? 0     : mapArea.Y;
        int areaW = mapArea.IsEmpty ? winW  : mapArea.Width;
        int areaH = mapArea.IsEmpty ? winH  : mapArea.Height;

        int panelX = areaX + (areaW - PanelW) / 2;
        int panelY = areaY + (areaH - panelH) / 2;

        // Panel background + border
        sb.Draw(px, new Rectangle(panelX, panelY, PanelW, panelH), _style.Bg);
        DrawBorder(sb, px, panelX, panelY, PanelW, panelH, _style.Border);

        // Caption / title
        string cap = _caption ?? _style.Title;
        float capW = fnt.MeasureString(cap).X;
        sb.DrawString(fnt, cap,
                      new Vector2(panelX + (PanelW - capW) / 2f, panelY + 6),
                      _style.Caption);

        // Separator
        int sepY = panelY + TitleH;
        sb.Draw(px, new Rectangle(panelX + 1, sepY, PanelW - 2, 1), _style.Border);

        // Body text
        int textY = sepY + 8;
        int end   = Math.Min(_scrollOffset + _maxBodyLines, _wrappedLines.Count);
        for (int i = _scrollOffset; i < end; i++)
            sb.DrawString(fnt, _wrappedLines[i],
                          new Vector2(panelX + PadX, textY + (i - _scrollOffset) * LineH),
                          _style.Text);

        // Hint separator + hint text
        int hintSepY = panelY + panelH - HintH - 4;
        sb.Draw(px, new Rectangle(panelX + 1, hintSepY, PanelW - 2, 1), _style.Border);

        string hint = Mode switch
        {
            PromptMode.Confirm => "Y: Confirm    N: Cancel    Esc: Cancel",
            PromptMode.Report  => "Up/Down: Scroll    Any key: Close",
            _                  => "Press any key to continue",
        };
        float hintW = fnt.MeasureString(hint).X * 0.72f;
        sb.DrawString(fnt, hint,
                      new Vector2(panelX + (PanelW - hintW) / 2f, hintSepY + 6),
                      ColHint,
                      0f, Vector2.Zero, 0.72f, SpriteEffects.None, 0f);
    }

    // ── Internals ────────────────────────────────────────────────────────────

    private void Open(
        (Color Bg, Color Border, Color Text, Color Caption, string Title) style,
        PromptMode mode, string captionOverride, string text)
    {
        _style        = style;
        Mode          = mode;
        _caption      = captionOverride;
        _rawText      = Sanitize(text ?? string.Empty);
        _needsWrap    = true;
        _wrappedLines.Clear();
        _scrollOffset = 0;
        _maxBodyLines = mode == PromptMode.Report ? 14 : 5;
        Result        = null;
        IsVisible     = true;
    }

    private void Dismiss()
    {
        IsVisible = false;
        Dismissed?.Invoke();
    }

    private void BuildWrappedLines(SpriteFont fnt)
    {
        int maxPx = PanelW - PadX * 2;
        _wrappedLines.Clear();

        foreach (var raw in _rawText.Split('\n'))
        {
            var line = raw.TrimEnd('\r');
            if (string.IsNullOrEmpty(line)) { _wrappedLines.Add(string.Empty); continue; }
            if (fnt.MeasureString(line).X <= maxPx) { _wrappedLines.Add(line); continue; }

            var words   = line.Split(' ');
            var current = new StringBuilder();
            foreach (var word in words)
            {
                if (current.Length == 0) { current.Append(word); continue; }
                string candidate = current + " " + word;
                if (fnt.MeasureString(candidate).X <= maxPx)
                    current.Append(' ').Append(word);
                else
                {
                    _wrappedLines.Add(current.ToString());
                    current.Clear().Append(word);
                }
            }
            if (current.Length > 0) _wrappedLines.Add(current.ToString());
        }

        _needsWrap = false;
    }

    private static (Color, Color, Color, Color, string) TypeStyle(MessageDisplayType t) => t switch
    {
        MessageDisplayType.ERROR   => StyleError,
        MessageDisplayType.WARNING => StyleWarning,
        _                          => StyleInfo,
    };

    private static void DrawBorder(SpriteBatch sb, Texture2D px, int x, int y, int w, int h, Color c)
    {
        sb.Draw(px, new Rectangle(x,         y,         w, 1), c);
        sb.Draw(px, new Rectangle(x,         y + h - 1, w, 1), c);
        sb.Draw(px, new Rectangle(x,         y,         1, h), c);
        sb.Draw(px, new Rectangle(x + w - 1, y,         1, h), c);
    }

    private static bool IsModifier(Keys k) =>
        k is Keys.LeftShift or Keys.RightShift or
             Keys.LeftControl or Keys.RightControl or
             Keys.LeftAlt or Keys.RightAlt;

    private static bool JustPressed(KeyboardState cur, KeyboardState prev, Keys key)
        => cur.IsKeyDown(key) && !prev.IsKeyDown(key);

    private static string Sanitize(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var sb = new StringBuilder(s.Length);
        foreach (char ch in s)
        {
            if (ch >= 32 && ch <= 126 || ch == '\n') { sb.Append(ch); continue; }
            switch (ch)
            {
                case '\r': break;
                case '→': sb.Append('>'); break;
                case '←': sb.Append('<'); break;
                case '—': sb.Append('-'); break;
                case '–': sb.Append('-'); break;
            }
        }
        return sb.ToString();
    }
}
