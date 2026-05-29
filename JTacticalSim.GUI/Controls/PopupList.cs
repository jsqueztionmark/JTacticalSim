using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JTacticalSim.GUI.Controls;

/// <summary>
/// Reusable SpriteBatch-rendered popup with a scrollable item list.
/// The caller owns Begin/End; this control only calls Draw/DrawString inside Draw().
/// Items with IsHeader=true are rendered as non-selectable section dividers.
/// </summary>
public sealed class PopupList<T>
{
    // ── Private state ────────────────────────────────────────────────────────
    private List<ListItem<T>> _items = new();
    private int _selectedIndex;
    private int _scrollOffset;

    // ── Layout (set via object initialiser before first use) ─────────────────
    /// <summary>Computed at Open() time from content; never set this directly.</summary>
    public int Width       { get; private set; } = 250;
    /// <summary>Minimum width regardless of content.</summary>
    public int MinWidth    { get; init; } = 140;
    public int MaxVisible  { get; init; } = 12;
    public int ItemHeight  { get; init; } = 20;
    public int PadX        { get; init; } = 8;
    public int TitleHeight { get; init; } = 22;
    public int HintHeight  { get; init; } = 18;

    // ── Content ──────────────────────────────────────────────────────────────
    public string Title    { get; set; } = string.Empty;

    /// <summary>Hint shown at the bottom. Set to null/empty to hide the hint bar entirely.</summary>
    public string HintText { get; set; } = "^/v:Scroll  Enter:Select  Esc:Close";

    // ── Style ────────────────────────────────────────────────────────────────
    public ControlStyle Style { get; init; } = ControlStyle.Default;

    // ── Runtime state ────────────────────────────────────────────────────────
    public bool IsOpen { get; private set; }
    public int X       { get; private set; }
    public int Y       { get; private set; }

    public ListItem<T> SelectedItem => _items.Count > 0 ? _items[_selectedIndex] : default;

    private int EffectiveHintH => string.IsNullOrEmpty(HintText) ? 0 : HintHeight;

    public int TotalHeight =>
        TitleHeight                                     // title text area
        + Style.SectionGap                              // below title text, before separator
        + 1                                             // title separator pixel
        + Style.SectionGap                              // after title separator, before items
        + Math.Min(MaxVisible, _items.Count) * ItemHeight
        + Style.SectionGap                              // below items, before hint separator
        + EffectiveHintH
        + 4;                                            // bottom margin

    // ── Events ───────────────────────────────────────────────────────────────
    /// <summary>Fired when the player confirms a selection (non-header items only).</summary>
    public event Action<T> ItemSelected;

    /// <summary>Fired when the highlighted item changes.</summary>
    public event Action<ListItem<T>> SelectionChanged;

    /// <summary>Fired when the popup closes without a selection (Escape or Close()).</summary>
    public event Action Closed;

    // ── API ──────────────────────────────────────────────────────────────────

    public void Open(IEnumerable<ListItem<T>> items, int x, int y, SpriteFont fnt)
    {
        _items = items.ToList();
        if (_items.Count == 0) return;
        _scrollOffset  = 0;
        X = x; Y = y;
        Width = ComputeWidth(fnt);
        IsOpen = true;

        // Advance past any leading headers
        _selectedIndex = 0;
        while (_selectedIndex < _items.Count - 1 && _items[_selectedIndex].IsHeader)
            _selectedIndex++;
    }

    private int ComputeWidth(SpriteFont fnt)
    {
        if (fnt == null) return MinWidth;

        int w = MinWidth;

        foreach (var item in _items)
            w = Math.Max(w, (int)fnt.MeasureString(item.Label).X + PadX * 2);

        if (!string.IsNullOrEmpty(Title))
            w = Math.Max(w, (int)fnt.MeasureString(Title).X + PadX * 2);

        if (!string.IsNullOrEmpty(HintText))
            w = Math.Max(w, (int)(fnt.MeasureString(HintText).X * 0.72f) + PadX * 2);

        return w;
    }

    public void Close()
    {
        IsOpen = false;
        Closed?.Invoke();
    }

    public void HandleInput(KeyboardState current, KeyboardState previous)
    {
        if (!IsOpen) return;

        if (JustPressed(current, previous, Keys.Up) || JustPressed(current, previous, Keys.NumPad8))
        {
            int next = _selectedIndex - 1;
            while (next > 0 && _items[next].IsHeader) next--;
            if (next >= 0 && !_items[next].IsHeader)
            {
                _selectedIndex = next;
                if (_selectedIndex < _scrollOffset) _scrollOffset = _selectedIndex;
                SelectionChanged?.Invoke(_items[_selectedIndex]);
            }
        }
        else if (JustPressed(current, previous, Keys.Down) || JustPressed(current, previous, Keys.NumPad2))
        {
            int next = _selectedIndex + 1;
            while (next < _items.Count - 1 && _items[next].IsHeader) next++;
            if (next < _items.Count && !_items[next].IsHeader)
            {
                _selectedIndex = next;
                if (_selectedIndex >= _scrollOffset + MaxVisible) _scrollOffset++;
                SelectionChanged?.Invoke(_items[_selectedIndex]);
            }
        }
        else if (JustPressed(current, previous, Keys.Enter) || JustPressed(current, previous, Keys.NumPad5))
        {
            if (!_items[_selectedIndex].IsHeader)
            {
                IsOpen = false;
                ItemSelected?.Invoke(_items[_selectedIndex].Value);
            }
        }
        else if (JustPressed(current, previous, Keys.Escape))
        {
            Close();
        }
    }

    public void Draw(SpriteBatch sb, SpriteFont fnt, Texture2D px)
    {
        if (!IsOpen || sb == null || fnt == null || px == null) return;

        int h = TotalHeight;
        int w = Width;

        // Background
        sb.Draw(px, new Rectangle(X, Y, w, h), Style.Background);

        // Border
        sb.Draw(px, new Rectangle(X,         Y,         w, 1), Style.Border);
        sb.Draw(px, new Rectangle(X,         Y + h - 1, w, 1), Style.Border);
        sb.Draw(px, new Rectangle(X,         Y,         1, h), Style.Border);
        sb.Draw(px, new Rectangle(X + w - 1, Y,         1, h), Style.Border);

        // Title with scroll indicators
        bool canUp   = _scrollOffset > 0;
        bool canDown = _scrollOffset + MaxVisible < _items.Count;
        string title = Title + (canUp ? " ^" : "") + (canDown ? " v" : "");
        if (!string.IsNullOrEmpty(title))
        {
            var sz = fnt.MeasureString(title);
            sb.DrawString(fnt, title,
                          new Vector2(X + (w - (int)sz.X) / 2, Y + Style.TitlePadTop),
                          Style.TitleText);
        }

        // Separator below title
        int separatorY = Y + TitleHeight + Style.SectionGap;
        sb.Draw(px, new Rectangle(X + 1, separatorY, w - 2, 1), Style.Separator);
        int contentY = separatorY + 1 + Style.SectionGap;

        // Item rows
        int end = Math.Min(_scrollOffset + MaxVisible, _items.Count);
        for (int i = _scrollOffset; i < end; i++)
        {
            int itemY  = contentY + (i - _scrollOffset) * ItemHeight;
            bool isHdr = _items[i].IsHeader;
            bool sel   = i == _selectedIndex && !isHdr;

            if (sel)
                sb.Draw(px, new Rectangle(X + 1, itemY, w - 2, ItemHeight), Style.SelectedBg);

            Color textColor = isHdr ? Style.TitleText : (sel ? Style.SelectedText : Style.ItemText);
            sb.DrawString(fnt, _items[i].Label, new Vector2(X + PadX, itemY + 2), textColor);
        }

        // Hint bar
        if (EffectiveHintH > 0)
        {
            int hintSepY = Y + h - EffectiveHintH - 4;
            sb.Draw(px, new Rectangle(X + 1, hintSepY, w - 2, 1), Style.Separator);
            sb.DrawString(fnt, HintText,
                          new Vector2(X + PadX, hintSepY + Style.SectionGap - 1),
                          Style.HintText,
                          0f, Vector2.Zero, 0.72f, SpriteEffects.None, 0f);
        }
    }

    private static bool JustPressed(KeyboardState current, KeyboardState previous, Keys key)
        => current.IsKeyDown(key) && !previous.IsKeyDown(key);
}
