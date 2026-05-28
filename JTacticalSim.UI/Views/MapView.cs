using System.Text;
using Terminal.Gui.Drawing;
using Terminal.Gui.Drivers;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.Utility;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace JTacticalSim.UI.Views;

public class MapView : View
{
    private const int CellWidth = 4;
    private const int CellHeight = 2;

    private int _viewportCol;
    private int _viewportRow;

    public MapView()
    {
        CanFocus = true;
    }

    public void CenterOnSelected()
    {
        var board = UIContext.Game?.GameBoard;
        if (board?.SelectedNode == null) return;

        var loc = board.SelectedNode.Location;
        int visCols = Viewport.Width / CellWidth;
        int visRows = Viewport.Height / CellHeight;
        _viewportCol = Math.Max(0, loc.X - visCols / 2);
        _viewportRow = Math.Max(0, loc.Y - visRows / 2);
        ClampViewport();
    }

    protected override bool OnDrawingContent(DrawContext context)
    {
        var game = UIContext.Game;
        if (game?.GameBoard == null) return true;

        var board = game.GameBoard;
        var attrs = board.DefaultAttributes;
        int visCols = Viewport.Width / CellWidth;
        int visRows = Viewport.Height / CellHeight;

        for (int vy = 0; vy < visRows; vy++)
        {
            for (int vx = 0; vx < visCols; vx++)
            {
                int nodeX = _viewportCol + vx;
                int nodeY = _viewportRow + vy;

                if (nodeX >= attrs.Width || nodeY >= attrs.Height)
                    continue;

                var node = game.JTSServices.NodeService.GetNodeAt(nodeX, nodeY, 0);
                if (node == null) continue;

                int screenX = vx * CellWidth;
                int screenY = vy * CellHeight;

                bool isSelected = board.SelectedNode != null
                    && board.SelectedNode.Location.X == nodeX
                    && board.SelectedNode.Location.Y == nodeY;

                bool isMoveable = board.AvailableMovementNodes != null
                    && board.AvailableMovementNodes.Any(n =>
                        n.Location.X == nodeX && n.Location.Y == nodeY);

                DrawTileCell(screenX, screenY, node, isSelected, isMoveable);
            }
        }

        return true;
    }

    private void DrawTileCell(int x, int y, INode node, bool isSelected, bool isMoveable)
    {
        var tile = node.DefaultTile;
        var (tileChar, fg, bg) = GetTileAppearance(tile);

        if (isSelected)
        {
            var tmp = fg;
            fg = bg;
            bg = tmp;
        }
        else if (isMoveable)
        {
            bg = new Color(60, 60, 180);
        }

        var attr = new Attribute(fg, bg);

        // Row 0: terrain fill
        for (int cx = 0; cx < CellWidth; cx++)
            SetChar(x + cx, y, tileChar, attr);

        // Row 1: unit indicator or terrain fill
        var units = GetVisibleUnits(node);
        if (units.Count > 0)
        {
            var unit = units[0];
            var unitFg = GetCountryColor(unit.Country);
            var unitAttr = isSelected
                ? new Attribute(bg, unitFg)
                : new Attribute(unitFg, bg);

            string label = units.Count == 1
                ? GetUnitSymbol(unit)
                : $"{GetUnitSymbol(unit)}{units.Count}";

            int pad = Math.Max(0, CellWidth - label.Length);
            int left = pad / 2;

            for (int cx = 0; cx < CellWidth; cx++)
            {
                int li = cx - left;
                char ch = (li >= 0 && li < label.Length) ? label[li] : tileChar;
                var a = (li >= 0 && li < label.Length) ? unitAttr : attr;
                SetChar(x + cx, y + 1, ch, a);
            }
        }
        else
        {
            for (int cx = 0; cx < CellWidth; cx++)
                SetChar(x + cx, y + 1, tileChar, attr);
        }
    }

    private void SetChar(int x, int y, char ch, Attribute attr)
    {
        if (x < 0 || x >= Viewport.Width || y < 0 || y >= Viewport.Height) return;
        SetAttribute(attr);
        Move(x, y);
        AddRune((Rune)ch);
    }

    private static (char ch, Color fg, Color bg) GetTileAppearance(ITile tile)
    {
        if (tile == null)
            return ('.', new Color(128, 128, 128), new Color(0, 0, 0));

        var allGeog = tile.AllGeography;
        if (allGeog == null || allGeog.Count == 0)
            return ('.', new Color(0, 180, 0), new Color(0, 80, 0));

        foreach (var demo in allGeog)
        {
            var className = demo.DemographicClass?.Name?.ToLowerInvariant() ?? "";

            if (className.Contains("water") || className.Contains("ocean") || className.Contains("sea"))
                return ('~', new Color(100, 150, 255), new Color(0, 0, 120));

            if (className.Contains("lake"))
                return ('~', new Color(80, 130, 230), new Color(0, 0, 100));

            if (className.Contains("mountain"))
                return ('^', new Color(180, 180, 180), new Color(80, 60, 40));

            if (className.Contains("hill"))
                return ('n', new Color(160, 140, 80), new Color(80, 100, 40));

            if (className.Contains("forest") || className.Contains("wood"))
                return ('#', new Color(0, 140, 0), new Color(0, 60, 0));

            if (className.Contains("marsh") || className.Contains("swamp"))
                return ('~', new Color(180, 180, 0), new Color(40, 80, 0));

            if (className.Contains("urban") || className.Contains("city"))
                return ('#', new Color(200, 200, 200), new Color(80, 80, 80));

            if (className.Contains("town"))
                return ('o', new Color(180, 140, 100), new Color(60, 40, 20));

            if (className.Contains("airport"))
                return ('=', new Color(200, 200, 200), new Color(60, 60, 60));

            if (className.Contains("military") || className.Contains("base"))
                return ('*', new Color(200, 200, 200), new Color(80, 60, 60));

            if (className.Contains("road") || className.Contains("track"))
                return ('+', new Color(160, 140, 100), new Color(0, 80, 0));

            if (className.Contains("bridge") || className.Contains("dam"))
                return ('=', new Color(160, 140, 100), new Color(0, 0, 100));

            if (className.Contains("nuclear") || className.Contains("wasteland"))
                return ('!', new Color(255, 0, 0), new Color(60, 0, 0));
        }

        return ('.', new Color(0, 180, 0), new Color(0, 80, 0));
    }

    private static Color GetCountryColor(ICountry country)
    {
        if (country == null) return new Color(200, 200, 200);

        var name = country.Name?.ToLowerInvariant() ?? "";
        int hash = Math.Abs(name.GetHashCode());
        int r = 120 + (hash % 136);
        int g = 120 + ((hash / 256) % 136);
        int b = 120 + ((hash / 65536) % 136);
        return new Color(r, g, b);
    }

    private static string GetUnitSymbol(IUnit unit)
    {
        var textZ3 = unit.UnitInfo?.UnitType?.TextDisplayZ3;
        if (!string.IsNullOrEmpty(textZ3))
            return textZ3.Substring(0, Math.Min(2, textZ3.Length));
        return "U";
    }

    private static List<IUnit> GetVisibleUnits(INode node)
    {
        try
        {
            var units = UIContext.Game.JTSServices.UnitService.GetAllUnitsAt(node.Location);
            return units?.Where(u => u != null).ToList() ?? new List<IUnit>();
        }
        catch
        {
            return new List<IUnit>();
        }
    }

    protected override bool OnKeyDown(Key key)
    {
        var board = UIContext.Game?.GameBoard;
        if (board == null) return base.OnKeyDown(key);

        Direction? dir = key.KeyCode switch
        {
            KeyCode.CursorUp    => Direction.NORTH,
            KeyCode.CursorDown  => Direction.SOUTH,
            KeyCode.CursorLeft  => Direction.WEST,
            KeyCode.CursorRight => Direction.EAST,
            _ => null
        };

        if (dir.HasValue && board.SelectedNode != null)
        {
            key.Handled = true;
            var next = board.SelectedNode.GetNodeInDirection(dir.Value, 1);
            if (next != null)
            {
                next.Select();
                EnsureSelectedVisible();
                SetNeedsDraw();
                OnSelectionChanged();
            }
            return true;
        }

        return base.OnKeyDown(key);
    }

    public event EventHandler SelectionChanged;

    private void OnSelectionChanged()
    {
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void EnsureSelectedVisible()
    {
        var selected = UIContext.Game?.GameBoard?.SelectedNode;
        if (selected == null) return;

        int visCols = Viewport.Width / CellWidth;
        int visRows = Viewport.Height / CellHeight;
        int margin = 1;

        if (selected.Location.X < _viewportCol + margin)
            _viewportCol = Math.Max(0, selected.Location.X - margin);
        else if (selected.Location.X >= _viewportCol + visCols - margin)
            _viewportCol = selected.Location.X - visCols + margin + 1;

        if (selected.Location.Y < _viewportRow + margin)
            _viewportRow = Math.Max(0, selected.Location.Y - margin);
        else if (selected.Location.Y >= _viewportRow + visRows - margin)
            _viewportRow = selected.Location.Y - visRows + margin + 1;

        ClampViewport();
    }

    private void ClampViewport()
    {
        var attrs = UIContext.Game?.GameBoard?.DefaultAttributes;
        if (attrs == null) return;

        int visCols = Viewport.Width / CellWidth;
        int visRows = Viewport.Height / CellHeight;
        _viewportCol = Math.Clamp(_viewportCol, 0, Math.Max(0, attrs.Width - visCols));
        _viewportRow = Math.Clamp(_viewportRow, 0, Math.Max(0, attrs.Height - visRows));
    }
}
