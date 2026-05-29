using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using JTacticalSim.API.Component;
using JTacticalSim.API.Component.Util;
using JTacticalSim.API.Game;
using JTacticalSim.GUI.Controls;

namespace JTacticalSim.GUI.Render;

public sealed class QuickSelectRenderer : BaseScreenRenderer
{
    private static readonly Color ColOverlay = new(10, 15, 25);

    private const int MarginX = 30;
    private const int MarginY = 30;
    private const int Gap     = 12;

    // ── Controls ─────────────────────────────────────────────────────────────

    private readonly PopupList<IUnit> _unitList = new()
    {
        Title    = "Quick Unit Select",
        MinWidth = 520,
        MaxVisible = 36,
        HintText = "Up/Down: Navigate    Enter: Go To Unit    Esc: Close",
        Style    = ControlStyle.Default,
    };

    private readonly TextPanel _unitInfoPanel = new()
    {
        Style    = ControlStyle.Default,
        ItemHeight = 19,
        PadX     = 10,
    };

    private readonly TextPanel _nodeInfoPanel = new()
    {
        Style    = ControlStyle.Default,
        ItemHeight = 19,
        PadX     = 10,
    };

    private bool _initialized;

    // ── Constructor ──────────────────────────────────────────────────────────

    public QuickSelectRenderer(Renderer baseRenderer) : base(baseRenderer)
    {
        _unitList.ItemSelected    += OnUnitSelected;
        _unitList.SelectionChanged += OnSelectionChanged;
        _unitList.Closed          += () => CloseScreen();
    }

    // ── Lifecycle ────────────────────────────────────────────────────────────

    public override void CloseScreen()
    {
        _initialized = false;
        base.CloseScreen();
    }

    // ── Rendering ────────────────────────────────────────────────────────────

    public override void RenderScreen()
    {
        var sb  = _baseRenderer.SpriteBatch;
        var fnt = _baseRenderer.Font;
        var px  = _baseRenderer.Pixel;
        if (sb == null || fnt == null || px == null) return;

        var gd   = _baseRenderer.GraphicsDevice;
        int winW = gd.Viewport.Width;
        int winH = gd.Viewport.Height;

        if (!_initialized) Initialize(winW, winH, fnt);

        FillRect(sb, px, 0, 0, winW, winH, ColOverlay);

        _unitList.Draw(sb, fnt, px);
        _unitInfoPanel.Draw(sb, fnt, px);
        _nodeInfoPanel.Draw(sb, fnt, px);
    }

    // ── Input ────────────────────────────────────────────────────────────────

    public void HandleInput(KeyboardState cur, KeyboardState prev)
    {
        _unitList.HandleInput(cur, prev);
    }

    // ── Initialisation ───────────────────────────────────────────────────────

    private void Initialize(int winW, int winH, SpriteFont fnt)
    {
        var game = TheGame();
        var items = BuildUnitList(game);

        _unitList.Open(items, MarginX, MarginY, fnt);

        // Right-side panels share the space to the right of the list
        int listRight = _unitList.X + _unitList.Width + Gap;
        int rightW    = winW - listRight - MarginX;
        int rightH    = winH - MarginY * 2;
        int halfH     = (rightH - Gap) / 2;

        _unitInfoPanel.Open(listRight, MarginY,                   rightW, halfH, "Unit Info");
        _nodeInfoPanel.Open(listRight, MarginY + halfH + Gap,     rightW, halfH, "Unit Location Info");

        // Seed info panels with the initial selection
        UpdateInfoPanels(_unitList.SelectedItem.Value);

        _initialized = true;
    }

    private static List<ListItem<IUnit>> BuildUnitList(IGame game)
    {
        var allUnits = game.JTSServices.UnitService
            .GetAllUnits(game.CurrentTurn.Player.Country)
            .Where(u => !game.CurrentTurn.Player.UnplacedReinforcements.Contains(u))
            .ToList();

        var branches = new[] { "army", "navy", "airforce" };
        var items    = new List<ListItem<IUnit>>();

        foreach (var branch in branches)
        {
            var group = allUnits
                .Where(u => u.UnitInfo.UnitType.Branch.Name.ToLowerInvariant() == branch)
                .OrderBy(u => u.Name)
                .ToList();

            if (!group.Any()) continue;

            items.Add(new ListItem<IUnit>(group[0].UnitInfo.UnitType.Branch.Name, null, IsHeader: true));

            foreach (var u in group)
            {
                string label = $"{u.UnitInfo.UnitType.Name,-18} {u.Name}";
                items.Add(new ListItem<IUnit>(label, u));
            }
        }

        return items;
    }

    // ── Event handlers ───────────────────────────────────────────────────────

    private void OnUnitSelected(IUnit unit)
    {
        if (unit == null) return;

        var game = TheGame();
        game.GameBoard.ClearSelectedItems(true);
        unit.Select();
        var node = unit.GetNode();
        game.GameBoard.SelectedNode = node;

        var stack = node.DefaultTile
            .GetAllComponentStacks()
            .SingleOrDefault(cs => cs.Country.Equals(unit.Country));
        stack?.BringToTop(unit);

        game.GameBoard.CenterSelectedNode();
        game.Renderer.SetCurrentViewableArea();
        CloseScreen();
    }

    private void OnSelectionChanged(ListItem<IUnit> item)
    {
        UpdateInfoPanels(item.Value);
    }

    private void UpdateInfoPanels(IUnit unit)
    {
        if (unit == null)
        {
            _unitInfoPanel.SetContent(string.Empty);
            _nodeInfoPanel.SetContent(string.Empty);
            return;
        }

        _unitInfoPanel.SetContent(unit.TextInfo());

        var node = unit.GetNode();
        if (node != null)
        {
            var sb2 = new System.Text.StringBuilder();
            var names = node.DefaultTile.GetAllDemographicNames();
            if (!string.IsNullOrWhiteSpace(node.DefaultTile.Name))
                sb2.AppendLine(node.DefaultTile.Name);
            foreach (var n in names)
                sb2.AppendLine(n);
            sb2.AppendLine();
            sb2.Append(node.DefaultTile.TextInfo());
            _nodeInfoPanel.SetContent(sb2.ToString());
        }
        else
        {
            _nodeInfoPanel.SetContent(string.Empty);
        }
    }
}
