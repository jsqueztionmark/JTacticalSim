using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.GUI.Controls;

namespace JTacticalSim.GUI.Render;

public sealed class MainScreenRenderer : BaseScreenRenderer
{
    // ── Layout ───────────────────────────────────────────────────────────────

    private const int MenuBarH   = 22;   // top: [M]ain Menu  [H]elp
    private const int PlayerBarH = 22;   // bottom: country / faction / RP / VP
    private const int MapLeft    = 4;
    private const int MapTop     = MenuBarH + 2;
    private const int InfoPanelW  = 380;
    private const int ReinfPanelH = 140;   // reserved at bottom of right column for available reinforcements
    private const int LineH       = 18;

    private int WindowW;
    private int WindowH;
    private int InfoPanelLeft;
    private int MapWidth;
    private int MapHeight;
    private int InfoPanelTop;
    private int InfoPanelH;

    // ── Colors ──────────────────────────────────────────────────────────────

    private static readonly Color ColStatusBar   = new(20, 30, 50);
    private static readonly Color ColStatusText   = new(200, 210, 220);
    private static readonly Color ColMapBg        = new(10, 18, 10);
    private static readonly Color ColPanelBg      = new(12, 22, 42);
    private static readonly Color ColPanelBorder  = new(60, 100, 160);
    private static readonly Color ColCaption       = Color.White;
    private static readonly Color ColText          = new(200, 200, 200);
    private static readonly Color ColTextDim       = new(100, 100, 100);
    private static readonly Color ColTextHi        = Color.Yellow;
    private static readonly Color ColSelected      = Color.Yellow;
    private static readonly Color ColSelectedFill = Color.FromNonPremultiplied(200, 200, 0, 30);
    private static readonly Color ColRoute         = new(255, 180, 60);
    private static readonly Color ColRouteFill     = Color.FromNonPremultiplied(200, 120, 0, 30);

    // terrain
    private static readonly Color ColLand          = new(40, 90, 30);
    private static readonly Color ColWater         = new(30, 60, 140);
    private static readonly Color ColMountain      = new(120, 110, 100);    
    private static readonly Color ColHill          = new(80, 100, 65);
    private static readonly Color ColForest        = new(20, 70, 20);
    private static readonly Color ColMarsh         = new(50, 80, 60);
    private static readonly Color ColUrban         = new(110, 100, 90);
    private static readonly Color ColRoad          = new(70, 65, 55);
    private static readonly Color ColBridge        = new(90, 85, 75);
    private static readonly Color ColAirport       = new(90, 90, 100);
    private static readonly Color ColMilBase       = new(130, 120, 50);
    private static readonly Color ColNuke          = new(140, 30, 30);
    private static readonly Color ColHighContrast  = new(50, 55, 60);

    // units
    private static readonly Color ColFriendlyUnit  = new(60, 180, 220);
    private static readonly Color ColEnemyUnit     = new(220, 70, 60);
    private static readonly Color ColNeutralUnit   = new(180, 180, 180);
    private static readonly Color ColUnitBg        = new(20, 20, 30, 200);

    private KeyboardState _previousKeyState;
    private bool _initialized;

    // ── Menu state ───────────────────────────────────────────────────────────

    private enum ActiveMenu { None, Main, Help }
    private ActiveMenu _activeMenu = ActiveMenu.None;
    private int _menuBarMainX;
    private int _menuBarHelpX;

    private readonly PopupList<Commands> _nodeActionPopup;
    private readonly PopupList<Commands> _menuPopup;

    private static readonly (string Label, Commands Command)[] MainMenuItems =
    {
        ("Title Screen",    Commands.DISPLAY_TITLE_SCREEN),
        ("Scenario Info",   Commands.DISPLAY_SCENARIO_INFO_SCREEN),
        ("Save Game",       Commands.SAVE_GAME),
        ("Exit",            Commands.EXIT),
    };

    private static readonly (string Label, Commands Command)[] HelpMenuItems =
    {
        ("Help Screen",     Commands.DISPLAY_HELP_SCREEN),
        ("Refresh Board",   Commands.REFRESH_BOARD),
    };

    public MainScreenRenderer(Renderer baseRenderer)
        : base(baseRenderer)
    {
        _previousKeyState = Keyboard.GetState();

        _nodeActionPopup = new PopupList<Commands>
        {
            Title  = "Node Action",
            Style  = ControlStyle.Default,
        };
        _nodeActionPopup.ItemSelected += cmd => TheGame().CommandProcessor.ProcessCommand(cmd);

        _menuPopup = new PopupList<Commands>
        {
            HintText = string.Empty,
            Style    = ControlStyle.Default,
        };
        _menuPopup.ItemSelected += cmd => TheGame().CommandProcessor.ProcessCommand(cmd);
        _menuPopup.Closed       += ()  => _activeMenu = ActiveMenu.None;
    }

    // ── Lifecycle ────────────────────────────────────────────────────────────

    public void LoadContent()   { }
    public void UnloadContent() { }
    public void Reset()         { _initialized = false; }

    /// <summary>Map panel bounds in screen pixels. Empty until first EnsureData() call.</summary>
    public Rectangle MapBounds =>
        _initialized ? new Rectangle(MapLeft, MapTop, MapWidth, MapHeight) : Rectangle.Empty;

    private void EnsureData()
    {
        if (_initialized) return;
        if (TheGame()?.GameBoard?.SelectedNode == null) return;

        var gd = _baseRenderer.GraphicsDevice;
        if (gd == null) return;

        WindowW = gd.Viewport.Width;
        WindowH = gd.Viewport.Height;
        InfoPanelLeft = WindowW - InfoPanelW - 4;
        MapWidth = InfoPanelLeft - MapLeft - 4;
        MapHeight = WindowH - MapTop - PlayerBarH - 2;
        InfoPanelTop = MapTop;
        InfoPanelH = MapHeight;

        _baseRenderer.RecalcZoomDrawCounts(MapWidth, MapHeight);
        TheGame().Renderer.SetCurrentViewableArea();
        _initialized = true;
    }

    // ── Main render entry ───────────────────────────────────────────────────

    public override void RenderScreen()
    {
        EnsureData();

        var sb  = _baseRenderer.SpriteBatch;
        var fnt = _baseRenderer.Font;
        var px  = _baseRenderer.Pixel;
        if (sb == null || fnt == null || px == null) return;

        var board = TheGame()?.GameBoard;
        if (board == null) return;

        DrawMenuBar(sb, fnt, px);
        DrawMapBackground(sb, px);
        DrawMapLabel(sb, fnt);
        RenderMap(false);
        DrawInfoPanel(sb, fnt, px);
        DrawPlayerBar(sb, fnt, px);

        if (_nodeActionPopup.IsOpen)
            _nodeActionPopup.Draw(sb, fnt, px);
        else if (_menuPopup.IsOpen)
            _menuPopup.Draw(sb, fnt, px);
    }

    // ── Menu bar (top) ──────────────────────────────────────────────────────

    private static readonly Color ColMenuBg        = new(15, 20, 35);
    private static readonly Color ColMenuItem      = new(180, 210, 255);
    private static readonly Color ColMenuKey       = Color.Yellow;
    private static readonly Color ColMenuBarActive = new(40, 65, 120);

    private void DrawMenuBar(SpriteBatch sb, SpriteFont fnt, Texture2D px)
    {
        sb.Draw(px, new Rectangle(0, 0, WindowW, MenuBarH), ColMenuBg);
        if (fnt == null) return;

        int textH = (int)fnt.MeasureString("M").Y;
        int y = (MenuBarH - textH) / 2;
        int x = 8;

        _menuBarMainX = x;
        int mainW = (int)fnt.MeasureString("[M]ain Menu").X;
        if (_activeMenu == ActiveMenu.Main)
            sb.Draw(px, new Rectangle(x - 4, 0, mainW + 8, MenuBarH), ColMenuBarActive);
        x = DrawMenuBarItem(sb, fnt, "[M]", "ain Menu", x, y);
        x += 32;

        _menuBarHelpX = x;
        int helpW = (int)fnt.MeasureString("[H]elp").X;
        if (_activeMenu == ActiveMenu.Help)
            sb.Draw(px, new Rectangle(x - 4, 0, helpW + 8, MenuBarH), ColMenuBarActive);
        DrawMenuBarItem(sb, fnt, "[H]", "elp", x, y);
    }

    private int DrawMenuBarItem(SpriteBatch sb, SpriteFont fnt, string key, string rest, int x, int y)
    {
        sb.DrawString(fnt, key,  new Vector2(x, y), ColMenuKey);
        int keyW = (int)fnt.MeasureString(key).X;
        sb.DrawString(fnt, rest, new Vector2(x + keyW, y), ColMenuItem);
        return x + keyW + (int)fnt.MeasureString(rest).X;
    }

    // ── Node action popup ────────────────────────────────────────────────────

    private void OnNodeAction()
    {
        var node = TheGame()?.GameBoard?.SelectedNode;
        if (node == null) return;

        var items = CommandInterface.GetAvailableCommandsForNode(node, TheGame())
            .Where(c => c != null)
            .Select(c => new ListItem<Commands>(c.DisplayName, c.CommandIdentifier));

        var (x, y) = GetNodeActionPopupPosition(_nodeActionPopup.Width, _nodeActionPopup.TotalHeight);
        _nodeActionPopup.Open(items, x, y, _baseRenderer.Font);
    }

    private (int x, int y) GetNodeActionPopupPosition(int popupW, int popupH)
    {
        var zoom    = TheGame().ZoomHandler.CurrentZoom;
        var origin  = zoom.CurrentOrigin;
        var node    = TheGame().GameBoard.SelectedNode;
        int col     = node.Location.X - origin.X;
        int row     = node.Location.Y - origin.Y;
        var (offX, offY) = MapGridOffset(zoom);
        int nodePixX = MapLeft + offX + col * zoom.ColumnSpacing;
        int nodePixY = MapTop  + offY + row * zoom.RowSpacing;

        int x = nodePixX + zoom.ColumnSpacing + 2;
        if (x + popupW > InfoPanelLeft - 4)
            x = nodePixX - popupW - 2;

        int y = nodePixY;
        if (y + popupH > MapTop + MapHeight - 4)
            y = MapTop + MapHeight - popupH - 4;
        if (y < MapTop + 4)
            y = MapTop + 4;

        return (x, y);
    }

    // ── Dropdown menus ───────────────────────────────────────────────────────

    private void ToggleMenu(ActiveMenu menu)
    {
        if (_activeMenu == menu && _menuPopup.IsOpen)
        {
            _menuPopup.Close();
            return;
        }
        _activeMenu = menu;
        var data = menu == ActiveMenu.Main ? MainMenuItems : HelpMenuItems;
        var items = data.Select(d => new ListItem<Commands>(d.Label, d.Command));
        int anchorX = (menu == ActiveMenu.Main ? _menuBarMainX : _menuBarHelpX) - 4;
        _menuPopup.Title = string.Empty;
        _menuPopup.Open(items, anchorX, MenuBarH, _baseRenderer.Font);
    }

    // ── Map label (inside map panel, top-left) ───────────────────────────────

    private void DrawMapLabel(SpriteBatch sb, SpriteFont fnt)
    {
        if (fnt == null) return;
        string label = GetMapLabel();
        if (string.IsNullOrEmpty(label)) return;

        float scale = 0.78f;
        sb.DrawString(fnt, label, new Vector2(MapLeft + 4, MapTop + 3), ColTextDim,
                      0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private string GetMapLabel()
    {
        var zoom    = TheGame().ZoomHandler?.CurrentZoom;
        var mapMode = TheGame().MapModeHandler?.CurrentMapMode;
        if (zoom == null || mapMode == null) return string.Empty;

        double w = zoom.DrawWidth  * TheGame().GameBoard.DefaultAttributes.CellMeters;
        double h = zoom.DrawHeight * TheGame().GameBoard.DefaultAttributes.CellMeters;
        string unit = "m";
        if (w > 1000 && h > 1000) { w /= 1000; h /= 1000; unit = "km"; }

        int level = Math.Max(0, (int)zoom.Level - 1);
        return $"Area Map : Level {level}  @  {w:F1} x {h:F1} {unit}  :  {mapMode.Name} mode";
    }

    // ── Map background ──────────────────────────────────────────────────────

    private void DrawMapBackground(SpriteBatch sb, Texture2D px)
    {
        sb.Draw(px, new Rectangle(MapLeft, MapTop, MapWidth, MapHeight), ColMapBg);
    }

    // ── Map rendering ───────────────────────────────────────────────────────

    public void RenderMap(bool clear)
    {
        var sb  = _baseRenderer.SpriteBatch;
        var fnt = _baseRenderer.Font;
        var px  = _baseRenderer.Pixel;
        if (sb == null || fnt == null || px == null) return;

        var board = TheGame()?.GameBoard;
        if (board?.CurrentViewableAreaNodes == null) return;

        if (clear)
            sb.Draw(px, new Rectangle(MapLeft, MapTop, MapWidth, MapHeight), ColMapBg);

        var zoom = TheGame().ZoomHandler.CurrentZoom;
        int zoomLevel = (int)zoom.Level;

        foreach (var node in board.CurrentViewableAreaNodes)
        {
            if (node == null) continue;
            RenderNode(node, zoomLevel);
        }
    }

    public void RenderFullMap(bool clear)
    {
        RenderMap(clear);
    }

    public void RenderBoard()
    {
        RenderMap(true);
    }

    public void RenderBoardFrame()
    {
        var sb = _baseRenderer.SpriteBatch;
        var px = _baseRenderer.Pixel;
        if (sb == null || px == null) return;
        DrawMapBackground(sb, px);
    }

    // ── Node rendering ──────────────────────────────────────────────────────

    public void RenderNode(INode node, int zoomLevel)
    {
        if (node == null) return;

        var sb  = _baseRenderer.SpriteBatch;
        var fnt = _baseRenderer.Font;
        var px  = _baseRenderer.Pixel;
        if (sb == null || fnt == null || px == null) return;

        var zoom = TheGame().ZoomHandler.CurrentZoom;
        var origin = zoom.CurrentOrigin;

        int col = node.Location.X - origin.X;
        int row = node.Location.Y - origin.Y;
        if (col < 0 || row < 0) return;

        int cellW = zoom.ColumnSpacing;
        int cellH = zoom.RowSpacing;

        var (offX, offY) = MapGridOffset(zoom);
        int px1 = MapLeft + offX + col * cellW;
        int py1 = MapTop  + offY + row * cellH;

        if (px1 + cellW > MapLeft + MapWidth || py1 + cellH > MapTop + MapHeight) return;

        RenderTile(node.DefaultTile, zoomLevel, px1, py1, cellW, cellH);
        RenderUnitsOnNode(node, zoomLevel, px1, py1, cellW, cellH);
        RenderNodeOverlays(node, px1, py1, cellW, cellH);

    }

    // ── Tile rendering ──────────────────────────────────────────────────────

    public void RenderTile(ITile tile, int zoomLevel)
    {
        // Called via the interface — full node context not available, so no-op.
        // Actual rendering goes through the overload with pixel coordinates.
    }

    private void RenderTile(ITile tile, int zoomLevel, int x, int y, int w, int h)
    {
        var sb = _baseRenderer.SpriteBatch;
        var px = _baseRenderer.Pixel;
        if (sb == null || px == null || tile == null) return;

        Color tileColor = GetTileColor(tile);
        sb.Draw(px, new Rectangle(x, y, w, h), tileColor);

        // Country ownership dot — top-left corner; skipped in POLITICAL mode (whole tile is already country-colored)
        var mapMode = TheGame().MapModeHandler?.CurrentMapMode;
        if (tile.Country != null && mapMode?.MapMode != MapMode.POLITICAL)
        {
            int dotSize = Math.Max(4, Math.Min(8, w / 12));
            Color dotColor = ConsoleColorToXna(tile.Country.Color);
            sb.Draw(px, new Rectangle(x + 3, y + 3, dotSize, dotSize), dotColor);
        }

        // Victory points — top-right corner
        var fnt = _baseRenderer.Font;
        if (tile.VictoryPoints > 0 && fnt != null)
        {
            float vpScale = 1.0f;
            string vpLabel = tile.VictoryPoints.ToString();
            var vpSize = fnt.MeasureString(vpLabel) * vpScale;
            int vpX = x + w - (int)vpSize.X - 3;
            int vpY = y + 2;
            sb.DrawString(fnt, vpLabel, new Vector2(vpX, vpY), ColTextHi, 0f, Vector2.Zero, vpScale, SpriteEffects.None, 0f);
        }

        // At higher zoom levels, render demographic text labels
        if (zoomLevel >= 3 && fnt != null && w >= 48)
        {
            string label = GetTileLabel(tile);
            if (!string.IsNullOrEmpty(label))
            {
                float scale = 1.0f;
                var textSize = fnt.MeasureString(label) * scale;
                int tx = x + (w - (int)textSize.X) / 2;
                int ty = y + h - (int)textSize.Y - 2;
                sb.DrawString(fnt, label, new Vector2(tx, ty), ColTextDim, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
        }
    }

    private Color GetTileColor(ITile tile)
    {
        var mapMode = TheGame().MapModeHandler?.CurrentMapMode;

        if (mapMode?.MapMode == MapMode.POLITICAL && tile.Country != null)
            return ConsoleColorToXna(tile.Country.Color);

        if (mapMode?.MapMode == MapMode.HIGH_CONTRAST)
            return ColHighContrast;

        var h = tile.ConsoleRenderHelper;
        if (h == null) return ColLand;

        if (h.IsNuclearWasteland)  return ColNuke;
        if (h.IsSea || h.IsRiver) return ColWater;
        if (h.HasLakes)           return ColWater;
        if (h.HasMountains || h.HasMountain) return ColMountain;
        if (h.HasHills)           return ColHill;
        if (h.HasForests || h.HasWoodlands || h.HasTrees) return ColForest;
        if (h.HasMarsh)           return ColMarsh;
        if (h.HasCities || h.HasTown) return ColUrban;
        if (h.HasIndustrial)      return ColUrban;
        if (h.HasMilitaryBase || h.HasCommandPost) return ColMilBase;
        if (h.HasAirports)        return ColAirport;
        if (h.HasBridge)          return ColBridge;
        if (h.HasRoad || h.HasTracks) return ColRoad;

        return ColLand;
    }

    private static string GetTileLabel(ITile tile)
    {
        var h = tile?.ConsoleRenderHelper;
        if (h == null) return null;

        if (h.IsNuclearWasteland) return "NUKE";
        if (h.IsSea)             return "SEA";
        if (h.IsRiver)           return "RVR";
        if (h.HasMountains || h.HasMountain) return "MTN";
        if (h.HasHills)          return "HILL";
        if (h.HasForests)        return "FOR";
        if (h.HasWoodlands)      return "WDS";
        if (h.HasMarsh)          return "MSH";
        if (h.HasCities)         return "CTY";
        if (h.HasTown)           return "TWN";
        if (h.HasMilitaryBase)   return "MIL";
        if (h.HasAirports)       return "AIR";
        if (h.HasLakes)          return "LKE";
        return null;
    }

    // ── Unit rendering ──────────────────────────────────────────────────────

    private void RenderUnitsOnNode(INode node, int zoomLevel, int x, int y, int cellW, int cellH)
    {
        if (node.DefaultTile == null) return;

        var sb  = _baseRenderer.SpriteBatch;
        var fnt = _baseRenderer.Font;
        var px  = _baseRenderer.Pixel;

        var stacks = node.DefaultTile.GetAllComponentStacks();
        if (stacks == null || stacks.Count == 0) return;

        var visible = stacks.Where(s => s != null && s.HasVisibleComponents).ToList();
        if (visible.Count == 0) return;

        if (cellW >= 48 && cellH >= 24)
        {
            // Large cells: labeled tokens stacked vertically, group centered in the cell
            float scale = zoomLevel >= 4 ? 0.85f : 0.75f;

            // Measure all tokens up front so we can center the block
            var tokens = visible
                .Select(stack =>
                {
                    var topUnit = stack.GetFirstVisibleUnit();
                    if (topUnit == null) return null;
                    int count = stack.GetAllUnits().Count;
                    string label = GetUnitToken(topUnit, count, zoomLevel);
                    var textSize = fnt.MeasureString(label) * scale;
                    int tw = Math.Min(Math.Max((int)textSize.X + 4, 20), cellW - 4);
                    int th = (int)textSize.Y + 2;
                    return new { label, tw, th, color = GetUnitColor(topUnit) };
                })
                .Where(t => t != null)
                .ToList();

            int totalH = tokens.Sum(t => t.th) + (tokens.Count - 1);
            int tokenY = y + Math.Max(2, (cellH - totalH) / 2);

            foreach (var token in tokens)
            {
                if (tokenY + token.th > y + cellH - 2) break;

                int tx = x + (cellW - token.tw) / 2;

                sb.Draw(px, new Rectangle(tx, tokenY, token.tw, token.th), ColUnitBg);
                sb.Draw(px, new Rectangle(tx, tokenY, token.tw, 1), token.color);
                sb.Draw(px, new Rectangle(tx, tokenY + token.th - 1, token.tw, 1), token.color);
                sb.Draw(px, new Rectangle(tx, tokenY, 1, token.th), token.color);
                sb.Draw(px, new Rectangle(tx + token.tw - 1, tokenY, 1, token.th), token.color);

                sb.DrawString(fnt, token.label, new Vector2(tx + 2, tokenY + 1), token.color,
                    0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

                tokenY += token.th + 1;
            }
        }
        else
        {
            // Small cells: one dot per stack, row centered in the cell
            // Multiple factions appear side-by-side
            int dotSize = Math.Max(4, cellW / 4);
            int gap     = 1;
            int totalW  = visible.Count * (dotSize + gap) - gap;
            int dotX    = x + (cellW - totalW) / 2;
            int dotY    = y + (cellH - dotSize) / 2;

            foreach (var stack in visible)
            {
                var topUnit = stack.GetFirstVisibleUnit();
                if (topUnit == null) continue;
                sb.Draw(px, new Rectangle(dotX, dotY, dotSize, dotSize), GetUnitColor(topUnit));
                dotX += dotSize + gap;
            }
        }
    }

    public void RenderUnit(IUnit unit, int zoomLevel)
    {
        if (unit == null) return;
        var node = unit.GetNode();
        if (node != null)
            RenderNode(node, zoomLevel);
    }

    public int RenderUnitStackInfo(IUnitStack stack)
    {
        return stack?.GetAllUnits()?.Count ?? 0;
    }

    private static string GetUnitToken(IUnit unit, int count, int zoomLevel)
    {
        string name = unit.Name ?? "?";
        if (zoomLevel >= 4)
        {
            string abbrev = name.Length > 8 ? name[..8] : name;
            return count > 1 ? $"{abbrev} x{count}" : abbrev;
        }
        string shortName = name.Length > 4 ? name[..4] : name;
        return count > 1 ? $"{shortName}x{count}" : shortName;
    }

    private Color GetUnitColor(IUnit unit)
    {
        var currentPlayer = TheGame().CurrentTurn?.Player;
        if (currentPlayer == null) return ColNeutralUnit;

        if (unit.Country?.Faction != null && currentPlayer.Country?.Faction != null)
        {
            if (unit.Country.Faction.ID == currentPlayer.Country.Faction.ID)
                return ColFriendlyUnit;
            return ColEnemyUnit;
        }
        return ColNeutralUnit;
    }

    // ── Node overlays (selection, routes, move radius) ──────────────────────

    private void RenderNodeOverlays(INode node, int x, int y, int w, int h)
    {
        var sb = _baseRenderer.SpriteBatch;
        var px = _baseRenderer.Pixel;
        var board = TheGame().GameBoard;

        // Selected units: fill on the node(s) where selected units live
        if (board.SelectedUnits != null)
        {
            foreach (var u in board.SelectedUnits)
            {
                var un = u?.GetNode();
                if (un != null && un.Location.X == node.Location.X && un.Location.Y == node.Location.Y)
                {
                    sb.Draw(px, new Rectangle(x + 1, y + 1, w - 2, h - 2), ColSelectedFill);
                    break;
                }
            }
        }

        // Cursor node: yellow border only, no fill
        if (board.SelectedNode != null && node.Location.X == board.SelectedNode.Location.X && node.Location.Y == board.SelectedNode.Location.Y)
        {
            int bw = 2;
            sb.Draw(px, new Rectangle(x, y, w, bw), ColSelected);
            sb.Draw(px, new Rectangle(x, y + h - bw, w, bw), ColSelected);
            sb.Draw(px, new Rectangle(x, y, bw, h), ColSelected);
            sb.Draw(px, new Rectangle(x + w - bw, y, bw, h), ColSelected);
        }

        // Route overlay: semi-transparent fill + amber top/bottom lines
        if (board.CurrentRoute?.Nodes != null)
        {
            foreach (var pathNode in board.CurrentRoute.Nodes)
            {
                var routeNode = pathNode.GetNode();
                if (routeNode != null && routeNode.Location.X == node.Location.X && routeNode.Location.Y == node.Location.Y)
                {
                    sb.Draw(px, new Rectangle(x + 1, y + 1, w - 2, h - 2), ColRouteFill);
                    sb.Draw(px, new Rectangle(x + 1, y + 1, w - 2, 2), ColRoute);
                    sb.Draw(px, new Rectangle(x + 1, y + h - 3, w - 2, 2), ColRoute);
                    break;
                }
            }
        }

        // Movement radius overlay intentionally omitted: engine uses simple distance which
        // does not account for the pathfinding algorithm (CanMoveOntoNodeFromDirection, etc.).
    }

    // ── Info panel ───────────────────────────────────────────────────────────

    private void DrawInfoPanel(SpriteBatch sb, SpriteFont fnt, Texture2D px)
    {
        int tileInfoH = InfoPanelH - ReinfPanelH - 4;
        DrawBox(sb, px, fnt, InfoPanelLeft, InfoPanelTop, InfoPanelW, tileInfoH, "Tile / Unit Info");
        DrawReinforcementsPanel(sb, fnt, px);

        var board = TheGame()?.GameBoard;
        if (board?.SelectedNode == null) return;

        int y = InfoPanelTop + 24;
        var node = board.SelectedNode;

        DisplayNodeInfo(node, sb, fnt, px, ref y);

        // Units at this location
        var tile = node.DefaultTile;
        if (tile == null) return;

        var stacks = tile.GetAllComponentStacks();
        if (stacks == null || stacks.Count == 0) return;

        y += 8;
        DrawText(sb, fnt, "Units:", InfoPanelLeft + 8, y, ColCaption);
        y += LineH;

        var selectedUnits = board.SelectedUnits;

        int panelBottom = InfoPanelTop + InfoPanelH - ReinfPanelH - 12;

        foreach (var stack in stacks)
        {
            if (stack == null) continue;
            foreach (var unit in stack.GetAllUnits())
            {
                if (unit == null) continue;
                if (unit.IsBeingTransported()) continue;   // rendered as child of its transport below
                if (y + LineH > panelBottom) return;

                bool isSelected = selectedUnits != null && selectedUnits.Any(su => su.ID == unit.ID);
                Color col = isSelected ? ColTextHi : ColText;
                string marker = isSelected ? "> " : "  ";

                Color unitCol = GetUnitColor(unit);
                sb.Draw(px, new Rectangle(InfoPanelLeft + 8, y + 4, 8, 8), unitCol);
                DrawText(sb, fnt, $"{marker}{unit.Name}", InfoPanelLeft + 20, y, col);
                y += LineH;

                if (isSelected && y + LineH * 6 < InfoPanelTop + InfoPanelH - 8)
                    DisplayUnitDetail(unit, sb, fnt, ref y);

                // Transported units — indented with arrow
                foreach (var carried in unit.GetTransportedUnits())
                {
                    if (carried == null) continue;
                    if (y + LineH > panelBottom) return;

                    Color carriedCol = GetUnitColor(carried);
                    //sb.Draw(px, new Rectangle(InfoPanelLeft + 20, y + 4, 6, 6), carriedCol);
                    DrawText(sb, fnt, $"=>", InfoPanelLeft + 30, y, carriedCol);
                    DrawText(sb, fnt, carried.Name, InfoPanelLeft + 55, y, ColTextDim);
                    y += LineH;
                }
            }
        }
    }

    private void DrawReinforcementsPanel(SpriteBatch sb, SpriteFont fnt, Texture2D px)
    {
        int panelY = InfoPanelTop + InfoPanelH - ReinfPanelH;
        DrawBox(sb, px, fnt, InfoPanelLeft, panelY, InfoPanelW, ReinfPanelH, "Available Reinforcements");

        var player = TheGame()?.CurrentTurn?.Player;
        if (player == null) return;

        var units = player.UnplacedReinforcements;
        int y = panelY + 24;
        int bottom = panelY + ReinfPanelH - 4;

        if (!units.Any())
        {
            DrawText(sb, fnt, "(none)", InfoPanelLeft + 8, y, ColTextDim);
            return;
        }

        foreach (var unit in units)
        {
            if (y + LineH > bottom) break;

            Color col = GetUnitColor(unit);
            sb.Draw(px, new Rectangle(InfoPanelLeft + 8, y + 4, 8, 8), col);

            string label = $"{unit.UnitInfo?.UnitType?.Name ?? "?"} {unit.Name}";
            DrawText(sb, fnt, label, InfoPanelLeft + 20, y, ColText);
            y += LineH;
        }
    }

    public void DisplayNodeInfo(INode node)
    {
        // Called externally — actual rendering happens in DrawInfoPanel each frame
    }

    private void DisplayNodeInfo(INode node, SpriteBatch sb, SpriteFont fnt, Texture2D px, ref int y)
    {
        string name = node.DefaultTile?.Country?.Name ?? "Unknown";
        DrawText(sb, fnt, node.DisplayName ?? $"({node.Location.X},{node.Location.Y})", InfoPanelLeft + 8, y, ColCaption);
        y += LineH;

        DrawText(sb, fnt, $"Location: {node.Location.X}, {node.Location.Y}, {node.Location.Z}", InfoPanelLeft + 8, y, ColText);
        y += LineH;

        DrawText(sb, fnt, $"Country: {name}", InfoPanelLeft + 8, y, ColText);
        y += LineH;

        var tile = node.DefaultTile;
        if (tile == null) return;

        string terrain = DescribeTerrain(tile);
        if (!string.IsNullOrEmpty(terrain))
        {
            DrawText(sb, fnt, terrain, InfoPanelLeft + 8, y, ColTextDim);
            y += LineH;
        }

        if (tile.VictoryPoints > 0)
        {
            DrawText(sb, fnt, $"Victory Points: {tile.VictoryPoints}", InfoPanelLeft + 8, y, ColText);
            y += LineH;
        }
    }

    public void DisplayUnitInfo(IUnit unit)
    {
        // Called externally — actual rendering happens in DrawInfoPanel each frame
    }

    private void DisplayUnitDetail(IUnit unit, SpriteBatch sb, SpriteFont fnt, ref int y)
    {
        int left = InfoPanelLeft + 28;
        float scale = 0.85f;
        int lineStep = (int)(LineH * 0.9f);

        if (unit.UnitInfo?.UnitClass != null)
        {
            sb.DrawString(fnt, $"Class: {unit.UnitInfo.UnitClass.Name}", new Vector2(left, y), ColTextDim, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            y += lineStep;
        }
        if (unit.UnitInfo?.UnitType != null)
        {
            sb.DrawString(fnt, $"Type: {unit.UnitInfo.UnitType.Name}", new Vector2(left, y), ColTextDim, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            y += lineStep;
        }
        if (unit.UnitInfo?.UnitGroupType != null)
        {
            sb.DrawString(fnt, $"Group: {unit.UnitInfo.UnitGroupType.Name}", new Vector2(left, y), ColTextDim, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            y += lineStep;
        }

        sb.DrawString(fnt, $"Move: {unit.MovementPoints}  Fuel: {unit.FuelLevelPercent:F0}%", new Vector2(left, y), ColTextDim, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        y += lineStep;

        sb.DrawString(fnt, $"Posture: {unit.Posture}", new Vector2(left, y), ColTextDim, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        y += lineStep;

        Color supplyCol = unit.IsSupplied() ? new Color(80, 180, 80) : new Color(220, 80, 80);
        string supplyText = unit.IsSupplied() ? "Supplied" : "NOT Supplied";
        sb.DrawString(fnt, supplyText, new Vector2(left, y), supplyCol, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        y += lineStep;

        y += 4;
    }

    public void RenderTileUnitInfoArea()
    {
        // Info panel re-renders every frame via DrawInfoPanel
    }

    // ── Player info bar (bottom) ─────────────────────────────────────────────

    private static readonly Color ColPlayerBarBg  = new(15, 20, 35);
    private static readonly Color ColPlayerText   = new(200, 210, 220);
    private static readonly Color ColRpColor      = new(80, 200, 80);
    private static readonly Color ColRpPerTurn    = new(0, 160, 160);
    private static readonly Color ColVpColor      = new(220, 200, 60);
    private static readonly Color ColNukeColor    = new(220, 80, 80);

    private void DrawPlayerBar(SpriteBatch sb, SpriteFont fnt, Texture2D px)
    {
        int barY = WindowH - PlayerBarH;
        sb.Draw(px, new Rectangle(0, barY, WindowW, PlayerBarH), ColPlayerBarBg);
        // top separator line
        sb.Draw(px, new Rectangle(0, barY, WindowW, 1), ColPanelBorder);

        if (fnt == null) return;

        var player  = TheGame().CurrentTurn?.Player;
        if (player == null) return;

        var country = player.Country;
        var faction = TheGame().CurrentPlayerFaction;
        int textY   = barY + (PlayerBarH - (int)fnt.MeasureString("A").Y) / 2;
        int x       = 8;

        // Country name (colored by country)
        Color countryColor = country != null ? ConsoleColorToXna(country.TextDisplayColor) : ColPlayerText;
        string countryName = country?.Name ?? "???";
        sb.DrawString(fnt, countryName, new Vector2(x, textY), countryColor);
        x += (int)fnt.MeasureString(countryName).X + 24;

        // Faction
        if (faction != null)
        {
            sb.DrawString(fnt, "Faction Alignment: ", new Vector2(x, textY), ColPlayerText);
            x += (int)fnt.MeasureString("Faction Alignment: ").X;
            sb.DrawString(fnt, faction.Name, new Vector2(x, textY), ColMenuKey);
            x += (int)fnt.MeasureString(faction.Name).X + 24;
        }

        // Nuclear charges
        if (player.TrackedValues?.NuclearCharges != null)
        {
            sb.DrawString(fnt, "Nuclear Charges: ", new Vector2(x, textY), ColPlayerText);
            x += (int)fnt.MeasureString("Nuclear Charges: ").X;
            sb.DrawString(fnt, player.TrackedValues.NuclearCharges.ToString(), new Vector2(x, textY), ColNukeColor);
            x += (int)fnt.MeasureString(player.TrackedValues.NuclearCharges.ToString()).X + 24;
        }

        // RP (current / per turn) — right-aligned group
        if (player.TrackedValues?.ReinforcementPoints != null)
        {
            int rp       = player.TrackedValues.ReinforcementPoints ?? 0;
            int rpPerTurn = 0;
            try { rpPerTurn = TheGame().JTSServices.RulesService.CalculateReinforcementPointsForTurn(player).Result; }
            catch { }

            string rpLabel = "Reinforcement Points: ";
            string rpVal   = $"{rp}";
            string rpSep   = " / ";
            string rpPT    = $"{rpPerTurn}";

            // position from right anchor
            float vpBlockW  = fnt.MeasureString("Victory Points:  888").X;
            float rpBlockW  = fnt.MeasureString(rpLabel).X + fnt.MeasureString(rpVal).X +
                              fnt.MeasureString(rpSep).X  + fnt.MeasureString(rpPT).X;
            int rpX = WindowW - (int)vpBlockW - (int)rpBlockW - 32;

            sb.DrawString(fnt, rpLabel, new Vector2(rpX, textY), ColPlayerText);
            rpX += (int)fnt.MeasureString(rpLabel).X;
            sb.DrawString(fnt, rpVal,   new Vector2(rpX, textY), ColRpColor);
            rpX += (int)fnt.MeasureString(rpVal).X;
            sb.DrawString(fnt, rpSep,   new Vector2(rpX, textY), ColPlayerText);
            rpX += (int)fnt.MeasureString(rpSep).X;
            sb.DrawString(fnt, rpPT,    new Vector2(rpX, textY), ColRpPerTurn);

            // VP — rightmost
            var vp = faction?.GetCurrentVictoryPoints();
            if (vp.HasValue)
            {
                string vpLabel = "Victory Points: ";
                string vpVal   = $" {vp.Value}";
                int vpX = WindowW - (int)fnt.MeasureString(vpLabel + vpVal).X - 8;
                sb.DrawString(fnt, vpLabel, new Vector2(vpX, textY), ColPlayerText);
                vpX += (int)fnt.MeasureString(vpLabel).X;
                // highlight box behind VP value
                int vpValW = (int)fnt.MeasureString(vpVal).X;
                sb.Draw(px, new Rectangle(vpX, barY + 2, vpValW, PlayerBarH - 4), ColVpColor * 0.25f);
                sb.DrawString(fnt, vpVal, new Vector2(vpX, textY), ColVpColor);
            }
        }
    }

    // ── Input handling ──────────────────────────────────────────────────────

    public void HandleInput(KeyboardState current, KeyboardState previous)
    {
        EnsureData();

        // Node action popup takes priority
        if (_nodeActionPopup.IsOpen)
        {
            _nodeActionPopup.HandleInput(current, previous);
            return;
        }

        // Menu dropdown
        if (_menuPopup.IsOpen)
        {
            _menuPopup.HandleInput(current, previous);
            // also allow M/H to switch or close
            if (JustPressed(current, previous, Keys.M))
                ToggleMenu(ActiveMenu.Main);
            else if (JustPressed(current, previous, Keys.H))
                ToggleMenu(ActiveMenu.Help);
            return;
        }

        var board = TheGame()?.GameBoard;
        if (board?.SelectedNode == null) return;

        bool ctrl  = current.IsKeyDown(Keys.LeftControl)  || current.IsKeyDown(Keys.RightControl);
        bool shift = current.IsKeyDown(Keys.LeftShift)    || current.IsKeyDown(Keys.RightShift);
        bool alt   = current.IsKeyDown(Keys.LeftAlt)      || current.IsKeyDown(Keys.RightAlt);

        if (ctrl)
        {
            if (JustPressed(current, previous, Keys.Up))
                ZoomMap(CycleDirection.OUT);
            else if (JustPressed(current, previous, Keys.Down))
                ZoomMap(CycleDirection.IN);
            else if (JustPressed(current, previous, Keys.Left))
                CycleMapMode(CycleDirection.DOWN);
            else if (JustPressed(current, previous, Keys.Right))
                CycleMapMode(CycleDirection.UP);
            else if (JustPressed(current, previous, Keys.End))
                TheGame().CommandProcessor.ProcessCommand(Commands.END_TURN);
            else if (JustPressed(current, previous, Keys.Q))
                TheGame().CommandProcessor.ProcessCommand(Commands.EXIT);
            else if (JustPressed(current, previous, Keys.R))
                TheGame().CommandProcessor.ProcessCommand(Commands.DISPLAY_REINFORCEMENTS_SCREEN);
            else if (JustPressed(current, previous, Keys.U))
                TheGame().CommandProcessor.ProcessCommand(Commands.DISPLAY_UNIT_QUICK_SELECT_SCREEN);
            else if (JustPressed(current, previous, Keys.Space))
            {
                if (shift)
                    TheGame().CommandProcessor.ProcessCommand(Commands.SET_SELECTED_UNITS);
                else if (alt)
                    TheGame().CommandProcessor.ProcessCommand(Commands.SET_SELECTED_UNITS_W_ATTACHED);
            }
            return;
        }

        // Top menu shortcuts (no modifier)
        if (JustPressed(current, previous, Keys.M))
        {
            ToggleMenu(ActiveMenu.Main);
            return;
        }
        if (JustPressed(current, previous, Keys.H))
        {
            ToggleMenu(ActiveMenu.Help);
            return;
        }

        if (shift)
        {
            // Scroll viewport without moving the cursor
            if (JustPressed(current, previous, Keys.NumPad8) || JustPressed(current, previous, Keys.Up))
                ScrollViewport(0, -1);
            else if (JustPressed(current, previous, Keys.NumPad2) || JustPressed(current, previous, Keys.Down))
                ScrollViewport(0, 1);
            else if (JustPressed(current, previous, Keys.NumPad4) || JustPressed(current, previous, Keys.Left))
                ScrollViewport(-1, 0);
            else if (JustPressed(current, previous, Keys.NumPad6) || JustPressed(current, previous, Keys.Right))
                ScrollViewport(1, 0);
            else if (JustPressed(current, previous, Keys.Add))
                CycleMapMode(CycleDirection.UP);
            else if (JustPressed(current, previous, Keys.Subtract))
                CycleMapMode(CycleDirection.DOWN);
            else if (JustPressed(current, previous, Keys.Space))
                TheGame().CommandProcessor.ProcessCommand(Commands.UNSELECT_ALL_UNITS);
            return;
        }

        // Navigation
        if (JustPressed(current, previous, Keys.Up)      || JustPressed(current, previous, Keys.NumPad8))
            MoveSelection(0, -1);
        else if (JustPressed(current, previous, Keys.Down)  || JustPressed(current, previous, Keys.NumPad2))
            MoveSelection(0, 1);
        else if (JustPressed(current, previous, Keys.Left)  || JustPressed(current, previous, Keys.NumPad4))
            MoveSelection(-1, 0);
        else if (JustPressed(current, previous, Keys.Right) || JustPressed(current, previous, Keys.NumPad6))
            MoveSelection(1, 0);
        // Diagonal movement (numpad corners)
        else if (JustPressed(current, previous, Keys.NumPad7))
            MoveSelection(-1, -1);
        else if (JustPressed(current, previous, Keys.NumPad9))
            MoveSelection(1, -1);
        else if (JustPressed(current, previous, Keys.NumPad1))
            MoveSelection(-1, 1);
        else if (JustPressed(current, previous, Keys.NumPad3))
            MoveSelection(1, 1);
        // NumPad5 / Enter — node action popup
        else if (JustPressed(current, previous, Keys.NumPad5) || JustPressed(current, previous, Keys.Enter))
            OnNodeAction();
        // Zoom
        else if (JustPressed(current, previous, Keys.OemPlus)   || JustPressed(current, previous, Keys.Add))
            ZoomMap(CycleDirection.OUT);
        else if (JustPressed(current, previous, Keys.OemMinus)  || JustPressed(current, previous, Keys.Subtract))
            ZoomMap(CycleDirection.IN);
        else if (JustPressed(current, previous, Keys.Space))
            TheGame().CommandProcessor.ProcessCommand(Commands.CYCLE_UNITS);
    }

    private void MoveSelection(int dx, int dy)
    {
        var board = TheGame().GameBoard;
        var cur = board.SelectedNode;
        if (cur == null) return;

        var target = TheGame().JTSServices.NodeService.GetNodeAt(
            cur.Location.X + dx,
            cur.Location.Y + dy,
            cur.Location.Z);

        if (target == null) return;

        target.Select();

        var zoom = TheGame().ZoomHandler.CurrentZoom;
        var origin = zoom.CurrentOrigin;

        bool needsScroll =
            target.Location.X < origin.X ||
            target.Location.Y < origin.Y ||
            target.Location.X >= origin.X + zoom.DrawWidth ||
            target.Location.Y >= origin.Y + zoom.DrawHeight;

        if (needsScroll)
        {
            if (dx < 0 && origin.X > 0) zoom.CurrentOrigin.X--;
            if (dx > 0) zoom.CurrentOrigin.X++;
            if (dy < 0 && origin.Y > 0) zoom.CurrentOrigin.Y--;
            if (dy > 0) zoom.CurrentOrigin.Y++;

            TheGame().Renderer.SetCurrentViewableArea();
            TheGame().ZoomHandler.SyncAllZoomLevels();
        }
    }

    private void ScrollViewport(int dx, int dy)
    {
        var zoom = TheGame().ZoomHandler.CurrentZoom;
        var origin = zoom.CurrentOrigin;

        if (dx < 0 && origin.X > 0) zoom.CurrentOrigin.X--;
        if (dx > 0) zoom.CurrentOrigin.X++;
        if (dy < 0 && origin.Y > 0) zoom.CurrentOrigin.Y--;
        if (dy > 0) zoom.CurrentOrigin.Y++;

        TheGame().Renderer.SetCurrentViewableArea();
        TheGame().ZoomHandler.SyncAllZoomLevels();
    }

    private void ZoomMap(CycleDirection direction)
    {
        var zh = TheGame().ZoomHandler;
        if (direction == CycleDirection.IN && zh.CurrentZoom.Level <= ZoomLevel.TWO) return;
        if (zh.CycleZoomLevel(direction))
        {
            _baseRenderer.RecalcZoomDrawCounts(MapWidth, MapHeight);

            // Re-center the new zoom level on the selected node so the cursor is always visible.
            var zoom  = zh.CurrentZoom;
            var node  = TheGame().GameBoard.SelectedNode;
            var attrs = TheGame().GameBoard.DefaultAttributes;

            int ox = Math.Max(0, Math.Min(node.Location.X - zoom.DrawWidth  / 2, attrs.Width  - zoom.DrawWidth));
            int oy = Math.Max(0, Math.Min(node.Location.Y - zoom.DrawHeight / 2, attrs.Height - zoom.DrawHeight));
            zoom.CurrentOrigin.X = ox;
            zoom.CurrentOrigin.Y = oy;

            TheGame().Renderer.SetCurrentViewableArea();
        }
    }

    private void CycleMapMode(CycleDirection direction)
    {
        var mh = TheGame().MapModeHandler;
        if (mh.CycleMapMode(direction))
            TheGame().Renderer.SetCurrentViewableArea();
    }

    // ── Demographics reset ──────────────────────────────────────────────────

    public void ResetTileDemographics(IEnumerable<INode> nodes)
    {
        if (nodes == null) return;
        foreach (var node in nodes)
            RenderNode(node, (int)TheGame().ZoomHandler.CurrentZoom.Level);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    // Centers the tile grid within the map panel — distributes dead pixels as equal margins.
    private (int offsetX, int offsetY) MapGridOffset(ZoomInfo zoom) =>
        ((MapWidth  - zoom.DrawWidth  * zoom.ColumnSpacing) / 2,
         (MapHeight - zoom.DrawHeight * zoom.RowSpacing)    / 2);

    private static string DescribeTerrain(ITile tile)
    {
        var parts = new List<string>();
        var h = tile.ConsoleRenderHelper;
        if (h == null) return string.Empty;

        if (h.IsSea) parts.Add("Sea");
        else if (h.IsRiver) parts.Add("River");
        if (h.HasLakes) parts.Add("Lake");
        if (h.HasMountains || h.HasMountain) parts.Add("Mountain");
        if (h.HasHills) parts.Add("Hills");
        if (h.HasForests) parts.Add("Forest");
        if (h.HasWoodlands) parts.Add("Woodland");
        if (h.HasMarsh) parts.Add("Marsh");
        if (h.HasCities) parts.Add("City");
        if (h.HasTown) parts.Add("Town");
        if (h.HasRoad) parts.Add("Road");
        if (h.HasBridge) parts.Add("Bridge");
        if (h.HasMilitaryBase) parts.Add("Military Base");
        if (h.HasAirports) parts.Add("Airport");
        if (h.IsNuclearWasteland) parts.Add("Nuclear Wasteland");

        return parts.Count > 0 ? string.Join(", ", parts) : "Open";
    }

    private static Color ConsoleColorToXna(System.ConsoleColor cc)
    {
        return cc switch
        {
            System.ConsoleColor.Black       => new Color(20, 20, 20),
            System.ConsoleColor.DarkBlue    => new Color(0, 0, 139),
            System.ConsoleColor.DarkGreen   => new Color(0, 100, 0),
            System.ConsoleColor.DarkCyan    => new Color(0, 139, 139),
            System.ConsoleColor.DarkRed     => new Color(139, 0, 0),
            System.ConsoleColor.DarkMagenta => new Color(139, 0, 139),
            System.ConsoleColor.DarkYellow  => new Color(180, 160, 0),
            System.ConsoleColor.Gray        => new Color(169, 169, 169),
            System.ConsoleColor.DarkGray    => new Color(105, 105, 105),
            System.ConsoleColor.Blue        => new Color(50, 80, 180),
            System.ConsoleColor.Green       => new Color(0, 180, 0),
            System.ConsoleColor.Cyan        => new Color(0, 200, 200),
            System.ConsoleColor.Red         => new Color(220, 50, 50),
            System.ConsoleColor.Magenta     => new Color(200, 0, 200),
            System.ConsoleColor.Yellow      => new Color(220, 220, 50),
            System.ConsoleColor.White       => new Color(240, 240, 240),
            _                               => new Color(120, 120, 120),
        };
    }

    private void DrawBox(SpriteBatch sb, Texture2D px, SpriteFont fnt,
                         int x, int y, int w, int h, string caption)
    {
        sb.Draw(px, new Rectangle(x, y, w, h), ColPanelBg);
        sb.Draw(px, new Rectangle(x,         y,         w, 1), ColPanelBorder);
        sb.Draw(px, new Rectangle(x,         y + h - 1, w, 1), ColPanelBorder);
        sb.Draw(px, new Rectangle(x,         y,         1, h), ColPanelBorder);
        sb.Draw(px, new Rectangle(x + w - 1, y,         1, h), ColPanelBorder);

        if (fnt != null && !string.IsNullOrEmpty(caption))
        {
            var sz = fnt.MeasureString(caption);
            int cx = x + (w - (int)sz.X) / 2;
            sb.DrawString(fnt, caption, new Vector2(cx, y + 4), ColCaption);
        }
    }

    private static void DrawText(SpriteBatch sb, SpriteFont fnt, string text, int x, int y, Color color)
    {
        if (fnt == null || string.IsNullOrEmpty(text)) return;
        sb.DrawString(fnt, text, new Vector2(x, y), color);
    }

    private static bool JustPressed(KeyboardState current, KeyboardState previous, Keys key)
        => current.IsKeyDown(key) && !previous.IsKeyDown(key);
}
