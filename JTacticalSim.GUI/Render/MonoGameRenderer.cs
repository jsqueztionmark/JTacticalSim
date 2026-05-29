using System.Text;
using Microsoft.Xna.Framework.Graphics;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Game;
using JTacticalSim.Component.World;

namespace JTacticalSim.GUI.Render;

public class MonoGameRenderer : BaseRenderer
{
    public SpriteBatch SpriteBatch { get; set; }
    public SpriteFont Font { get; set; }
    public GraphicsDevice GraphicsDevice { get; set; }
    public Texture2D Pixel { get; set; }

    private IBoard _board => TheGame().GameBoard;
    private IZoomHandler _zoomHandler => TheGame().ZoomHandler;
    private IMapModeHandler _mapModeHandler => TheGame().MapModeHandler;

    public MonoGameTitleScreenRenderer TitleScreenRenderer { get; private set; }
    public MonoGameMainScreenRenderer MainScreenRenderer { get; private set; }
    public MonoGameBattleScreenRenderer BattleScreenRenderer { get; private set; }
    public MonoGameReinforcementsScreenRenderer ReinforcementsScreenRenderer { get; private set; }
    public MonoGameQuickSelectRenderer QuickSelectRenderer { get; private set; }
    public MonoGameScenarioInfoScreenRenderer ScenarioInfoScreenRenderer { get; private set; }
    public MonoGameHelpScreenRenderer HelpScreenRenderer { get; private set; }
    public MonoGameGameOverScreenRenderer GameOverScreenRenderer { get; private set; }

    public MonoGameRenderer()
    {
        TitleScreenRenderer = new MonoGameTitleScreenRenderer(this);
        MainScreenRenderer = new MonoGameMainScreenRenderer(this);
        BattleScreenRenderer = new MonoGameBattleScreenRenderer(this);
        ReinforcementsScreenRenderer = new MonoGameReinforcementsScreenRenderer(this);
        QuickSelectRenderer = new MonoGameQuickSelectRenderer(this);
        ScenarioInfoScreenRenderer = new MonoGameScenarioInfoScreenRenderer(this);
        HelpScreenRenderer = new MonoGameHelpScreenRenderer(this);
        GameOverScreenRenderer = new MonoGameGameOverScreenRenderer(this);
    }

    public override void LoadContent()
    {
        TheGame().ZoomHandler = new MonoGameZoomHandler();
        TheGame().MapModeHandler = new MonoGameMapModeHandler();
        LoadZoomInfo();
        LoadMapModes();
    }

    public override void UnloadContent() { }

    private void LoadZoomInfo()
    {
        var zooms = new List<ZoomInfo>
        {
            new ZoomInfo { RowSpacing = 64, ColumnSpacing = 96, CurrentOrigin = new Coordinate(0,0,0), DrawHeight = 9,  DrawWidth = 12, IsCurrent = true,  Level = ZoomLevel.FOUR  },
            new ZoomInfo { RowSpacing = 48, ColumnSpacing = 80, CurrentOrigin = new Coordinate(0,0,0), DrawHeight = 12, DrawWidth = 14, IsCurrent = false, Level = ZoomLevel.THREE },
            new ZoomInfo { RowSpacing = 32, ColumnSpacing = 48, CurrentOrigin = new Coordinate(0,0,0), DrawHeight = 18, DrawWidth = 24, IsCurrent = false, Level = ZoomLevel.TWO   },
            new ZoomInfo { RowSpacing = 16, ColumnSpacing = 16, CurrentOrigin = new Coordinate(0,0,0), DrawHeight = 16, DrawWidth = 25, IsCurrent = false, Level = ZoomLevel.ONE   },
        };
        TheGame().ZoomHandler.LoadZooms(zooms);
        TheGame().GameBoard.DefaultAttributes.DrawHeight = _zoomHandler.GetZoomLevelInfo(ZoomLevel.FOUR).DrawHeight;
        TheGame().GameBoard.DefaultAttributes.DrawWidth  = _zoomHandler.GetZoomLevelInfo(ZoomLevel.FOUR).DrawWidth;
    }

    private void LoadMapModes()
    {
        var modes = new List<MapModeInfo>
        {
            new MapModeInfo(MapMode.GEOGRAPHICAL, "Geography")    { IsCurrent = true  },
            new MapModeInfo(MapMode.POLITICAL,     "Political")   { IsCurrent = false },
            new MapModeInfo(MapMode.HIGH_CONTRAST, "High Contrast") { IsCurrent = false },
        };
        _mapModeHandler.LoadMapModes(modes);
    }

    // ── Main screen ──────────────────────────────────────────────────────────

    public override void RenderMainScreen()        => MainScreenRenderer.RenderScreen();
    public override void RenderBoardFrame()        => MainScreenRenderer.RenderBoardFrame();
    public override void RenderBoard()             => MainScreenRenderer.RenderBoard();
    public override void RenderMap(bool clear)     => MainScreenRenderer.RenderMap(clear);
    public override void RenderFullMap(bool clear) => MainScreenRenderer.RenderFullMap(clear);
    public override void RenderTileUnitInfoArea()  => MainScreenRenderer.RenderTileUnitInfoArea();
    public override void RenderNode(INode node, int zoomLevel)    => MainScreenRenderer.RenderNode(node, zoomLevel);
    public override void RenderTile(ITile tile, int zoomLevel)    => MainScreenRenderer.RenderTile(tile, zoomLevel);
    public override void DisplayNodeInfo(INode node)              => MainScreenRenderer.DisplayNodeInfo(node);
    public override void DisplayTileInfo(ITile tile)              { }
    public override void DisplayPlayerInfo(IPlayer player)        { }
    public override void DisplayLegend()                          { }
    public override void ResetTileDemographics(IEnumerable<INode> nodes) => MainScreenRenderer.ResetTileDemographics(nodes);

    public override void SetCurrentViewableArea()
    {
        var maxX = _zoomHandler.CurrentZoom.CurrentOrigin.X + _zoomHandler.CurrentZoom.DrawWidth;
        var maxY = _zoomHandler.CurrentZoom.CurrentOrigin.Y + _zoomHandler.CurrentZoom.DrawHeight;
        TheGame().GameBoard.CurrentViewableAreaNodes = TheGame().JTSServices.NodeService.GetAllNodes()
            .Where(n =>
                n.Location.X >= _zoomHandler.CurrentZoom.CurrentOrigin.X && n.Location.X < maxX &&
                n.Location.Y >= _zoomHandler.CurrentZoom.CurrentOrigin.Y && n.Location.Y < maxY)
            .Where(n => n != null);
    }

    public override void RefreshActiveNodes()
    {
        if (_board.LastSelectedNode == null)
            _board.LastSelectedNode = _board.SelectedNode;
        RefreshNodes(new[] { _board.LastSelectedNode });
        RefreshNodes(new[] { _board.SelectedNode });
        TheGame().Renderer.RenderTileUnitInfoArea();
    }

    public override void RefreshActiveRoute()
    {
        if (_board.CurrentRoute == null) return;
        foreach (var node in _board.CurrentRoute.Nodes)
            if (_board.NodeIsInViewableArea(node.GetNode()))
                TheGame().Renderer.RenderNode(node.GetNode(), (int)_zoomHandler.CurrentZoom.Level);
    }

    public override void RefreshNodes(IEnumerable<INode> nodes)
    {
        foreach (var n in nodes)
        {
            if (n == null) continue;
            if (TheGame().GameBoard.NodeIsInViewableArea(n))
                TheGame().Renderer.RenderNode(n, (int)_zoomHandler.CurrentZoom.Level);
        }
    }

    public override void CenterSelectedNode()
    {
        var vOffSet = 0;
        var hOffSet = 0;
        if (_board.SelectedNode.Location.X > _zoomHandler.CurrentZoom.DrawWidth / 2 &&
            _board.SelectedNode.Location.X < (_board.DefaultAttributes.Width - 1) - _zoomHandler.CurrentZoom.DrawWidth)
            hOffSet = _zoomHandler.CurrentZoom.DrawWidth / 2;
        if (_board.SelectedNode.Location.Y > _zoomHandler.CurrentZoom.DrawHeight / 2 &&
            _board.SelectedNode.Location.Y < (_board.DefaultAttributes.Height - 1) - _zoomHandler.CurrentZoom.DrawHeight)
            vOffSet = _zoomHandler.CurrentZoom.DrawHeight / 2;
        SetCurrentViewableArea();
        _zoomHandler.ResetAllZoomLevels(vOffSet, hOffSet);
    }

    public override void ZoomMap(CycleDirection direction)
    {
        if (direction == CycleDirection.IN && _zoomHandler.CurrentZoom.Level <= ZoomLevel.TWO) return;
        if (_zoomHandler.CycleZoomLevel(direction))
        {
            SetCurrentViewableArea();
            RenderBoardFrame();
            RenderMap(true);
        }
    }

    public override void CycleMapMode(CycleDirection direction)
    {
        if (_mapModeHandler.CycleMapMode(direction))
        {
            SetCurrentViewableArea();
            RenderBoardFrame();
            RenderMap(true);
        }
    }

    // ── Units ────────────────────────────────────────────────────────────────

    public override void DisplayUnits(List<IUnit> units)       { }
    public override void DisplayUnitInfo(IUnit unit)           => MainScreenRenderer.DisplayUnitInfo(unit);
    public override void RenderUnit(IUnit unit, int zoomLevel) => MainScreenRenderer.RenderUnit(unit, zoomLevel);
    public override void RenderUnitName(IUnit unit)            { }
    public override int  RenderUnitStackInfo(IUnitStack stack) => MainScreenRenderer.RenderUnitStackInfo(stack);

    // ── Secondary screens ────────────────────────────────────────────────────

    public override void RenderReinforcementScreen() => ReinforcementsScreenRenderer.RenderScreen();
    public override void RenderTitleScreen()         => TitleScreenRenderer.RenderScreen();
    public override void RenderQuickSelectScreen()   => QuickSelectRenderer.RenderScreen();
    public override void RenderScenarioInfoScreen()  => ScenarioInfoScreenRenderer.RenderScreen();
    public override void RenderHelpScreen()          => HelpScreenRenderer.RenderScreen();
    public override void RenderGameOverScreen()      => GameOverScreenRenderer.RenderScreen();

    // ── Battle ───────────────────────────────────────────────────────────────

    public override void RenderBattleScreen()                            => BattleScreenRenderer.RenderScreen();
    public override void RenderBattle(IBattle battle)                    => BattleScreenRenderer.RenderBattle(battle);
    public override void RenderBattleRound(IRound round)                 => BattleScreenRenderer.RenderBattleRound(round);
    public override void RenderBattleSkirmish(ISkirmish skirmish)        => BattleScreenRenderer.RenderBattleSkirmish(skirmish);
    public override void RenderBattleSkirmishOutcome(ISkirmish skirmish) => BattleScreenRenderer.RenderBattleSkirmishOutcome(skirmish);
    public override void RenderBattleOutcome(IBattle battle)             => BattleScreenRenderer.RenderBattleOutcome(battle);
    public override void RenderBattleRetreat(IBattle battle)             => BattleScreenRenderer.RenderBattleRetreat(battle);

    // ── Feedback ─────────────────────────────────────────────────────────────

    public override void DisplayUserMessage(MessageDisplayType messageType, string message, Exception ex)
    {
        // TODO: overlay dialog
        System.Console.WriteLine($"[{messageType}] {message}{(ex != null ? $"\n{ex.Message}" : "")}");
    }

    public override void DisplayTaskExecutionReport(StringBuilder report)
    {
        // TODO: overlay dialog
        System.Console.WriteLine(report.ToString());
    }

    public override bool ConfirmAction(string message)
    {
        // TODO: yes/no overlay dialog; defaults to false until implemented
        System.Console.WriteLine($"[Confirm] {message}");
        return false;
    }
}
