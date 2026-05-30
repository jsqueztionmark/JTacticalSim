using System.Text;
using Microsoft.Xna.Framework.Graphics;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Game;
using JTacticalSim.Component.World;
using JTacticalSim.GUI.Controls;

namespace JTacticalSim.GUI.Render;

public class Renderer : BaseRenderer
{
    public SpriteBatch SpriteBatch { get; set; }
    public SpriteFont Font    { get; set; }
    public SpriteFont MapFont { get; set; }
    public GraphicsDevice GraphicsDevice { get; set; }
    public Texture2D Pixel { get; set; }

    public ModalOverlay Overlay { get; } = new();
    public DevCli DevCli       { get; } = new();

    // Two-pass confirm: first call shows the overlay and returns false;
    // after the user answers Y/N the stored result is returned on the retry.
    private bool? _pendingConfirmResult;

    private IBoard _board => TheGame().GameBoard;
    private IZoomHandler _zoomHandler => TheGame().ZoomHandler;
    private IMapModeHandler _mapModeHandler => TheGame().MapModeHandler;

    public TitleScreenRenderer TitleScreenRenderer { get; private set; }
    public MainScreenRenderer MainScreenRenderer { get; private set; }
    public BattleScreenRenderer BattleScreenRenderer { get; private set; }
    public ReinforcementsScreenRenderer ReinforcementsScreenRenderer { get; private set; }
    public QuickSelectRenderer QuickSelectRenderer { get; private set; }
    public ScenarioInfoScreenRenderer ScenarioInfoScreenRenderer { get; private set; }
    public HelpScreenRenderer HelpScreenRenderer { get; private set; }
    public GameOverScreenRenderer GameOverScreenRenderer { get; private set; }

    public Renderer()
    {
        TitleScreenRenderer = new TitleScreenRenderer(this);
        MainScreenRenderer = new MainScreenRenderer(this);
        BattleScreenRenderer = new BattleScreenRenderer(this);
        ReinforcementsScreenRenderer = new ReinforcementsScreenRenderer(this);
        QuickSelectRenderer = new QuickSelectRenderer(this);
        ScenarioInfoScreenRenderer = new ScenarioInfoScreenRenderer(this);
        HelpScreenRenderer = new HelpScreenRenderer(this);
        GameOverScreenRenderer = new GameOverScreenRenderer(this);
    }

    public override void LoadContent()
    {
        TheGame().ZoomHandler = new ZoomHandler();
        TheGame().MapModeHandler = new MapModeHandler();
        LoadZoomInfo();
        LoadMapModes();
        MainScreenRenderer.Reset();
    }

    public override void UnloadContent() { }

    private void LoadZoomInfo()
    {
        var zooms = new List<ZoomInfo>
        {
            new ZoomInfo { RowSpacing = 120, ColumnSpacing = 120, CurrentOrigin = new Coordinate(0,0,0), DrawHeight = 1, DrawWidth = 1, IsCurrent = true,  Level = ZoomLevel.FOUR  },
            new ZoomInfo { RowSpacing =  60, ColumnSpacing =  60, CurrentOrigin = new Coordinate(0,0,0), DrawHeight = 1, DrawWidth = 1, IsCurrent = false, Level = ZoomLevel.THREE },
            new ZoomInfo { RowSpacing =  40, ColumnSpacing =  40, CurrentOrigin = new Coordinate(0,0,0), DrawHeight = 1, DrawWidth = 1, IsCurrent = false, Level = ZoomLevel.TWO   },
        };
        TheGame().ZoomHandler.LoadZooms(zooms);
        // Actual DrawWidth/DrawHeight are set by MainScreenRenderer.RecalcZoomDrawCounts()
        // once the map panel pixel dimensions are known.
    }

    public void RecalcZoomDrawCounts(int mapPixelWidth, int mapPixelHeight)
    {
        foreach (var level in new[] { ZoomLevel.TWO, ZoomLevel.THREE, ZoomLevel.FOUR })
        {
            var z = _zoomHandler.GetZoomLevelInfo(level);
            if (z == null) continue;
            z.DrawWidth  = mapPixelWidth  / z.ColumnSpacing;
            z.DrawHeight = mapPixelHeight / z.RowSpacing;
        }

        var cur = _zoomHandler.CurrentZoom;
        TheGame().GameBoard.DefaultAttributes.DrawHeight = cur.DrawHeight;
        TheGame().GameBoard.DefaultAttributes.DrawWidth  = cur.DrawWidth;
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

    public override void RenderMainScreen()        { if (IsInDrawPhase) MainScreenRenderer.RenderScreen(); }
    public override void RenderBoardFrame()        { if (IsInDrawPhase) MainScreenRenderer.RenderBoardFrame(); }
    public override void RenderBoard()             { if (IsInDrawPhase) MainScreenRenderer.RenderBoard(); }
    public override void RenderMap(bool clear)     { if (IsInDrawPhase) MainScreenRenderer.RenderMap(clear); }
    public override void RenderFullMap(bool clear) { if (IsInDrawPhase) MainScreenRenderer.RenderFullMap(clear); }
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

    // Guards against SpriteBatch.Draw() being called during Update() before Begin().
    // Set true only inside MonoGameHost.Draw() between Begin/End.
    internal bool IsInDrawPhase { get; set; }

    // These are no-ops: the full screen re-renders every frame in Draw(), so any state
    // change made during Update() is reflected automatically on the next draw call.
    // Calling SpriteBatch.Draw() here (during Update) would crash with "Begin not called".
    public override void RefreshActiveNodes()  { }
    public override void RefreshActiveRoute()  { }
    public override void RefreshNodes(IEnumerable<INode> nodes) { }

    public override void CenterSelectedNode()
    {
        var node  = _board.SelectedNode;
        var zoom  = _zoomHandler.CurrentZoom;
        int boardW = _board.DefaultAttributes.Width;
        int boardH = _board.DefaultAttributes.Height;

        // Desired top-left origin that places the node at the centre of the viewport
        int originX = node.Location.X - zoom.DrawWidth  / 2;
        int originY = node.Location.Y - zoom.DrawHeight / 2;

        // Clamp so we never scroll past map edges
        originX = Math.Max(0, Math.Min(originX, boardW - zoom.DrawWidth));
        originY = Math.Max(0, Math.Min(originY, boardH - zoom.DrawHeight));

        // ResetAllZoomLevels expects offsets from the node position (origin = node - offset)
        int hOffset = node.Location.X - originX;
        int vOffset = node.Location.Y - originY;

        _zoomHandler.ResetAllZoomLevels(vOffset, hOffset);
        SetCurrentViewableArea();
    }

    public override void ZoomMap(CycleDirection direction)
    {
        if (direction == CycleDirection.IN && _zoomHandler.CurrentZoom.Level <= ZoomLevel.TWO) return;
        if (_zoomHandler.CycleZoomLevel(direction))
        {
            var cur = _zoomHandler.CurrentZoom;
            TheGame().GameBoard.DefaultAttributes.DrawHeight = cur.DrawHeight;
            TheGame().GameBoard.DefaultAttributes.DrawWidth  = cur.DrawWidth;
            SetCurrentViewableArea();
        }
    }

    public override void CycleMapMode(CycleDirection direction)
    {
        if (_mapModeHandler.CycleMapMode(direction))
            SetCurrentViewableArea();
    }

    // ── Units ────────────────────────────────────────────────────────────────

    public override void DisplayUnits(List<IUnit> units)       { }
    public override void DisplayUnitInfo(IUnit unit)           => MainScreenRenderer.DisplayUnitInfo(unit);
    public override void RenderUnit(IUnit unit, int zoomLevel) => MainScreenRenderer.RenderUnit(unit, zoomLevel);
    public override void RenderUnitName(IUnit unit)            { }
    public override int  RenderUnitStackInfo(IUnitStack stack) => MainScreenRenderer.RenderUnitStackInfo(stack);

    // ── Secondary screens ────────────────────────────────────────────────────

    public override void RenderReinforcementScreen() { if (IsInDrawPhase) ReinforcementsScreenRenderer.RenderScreen(); }
    public override void RenderTitleScreen()         { if (IsInDrawPhase) TitleScreenRenderer.RenderScreen(); }
    public override void RenderQuickSelectScreen()   { if (IsInDrawPhase) QuickSelectRenderer.RenderScreen(); }
    public override void RenderScenarioInfoScreen()  { if (IsInDrawPhase) ScenarioInfoScreenRenderer.RenderScreen(); }
    public override void RenderHelpScreen()          { if (IsInDrawPhase) HelpScreenRenderer.RenderScreen(); }
    public override void RenderGameOverScreen()      { if (IsInDrawPhase) GameOverScreenRenderer.RenderScreen(); }

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
        Overlay.ShowMessage(messageType, message, ex);
    }

    public override void DisplayTaskExecutionReport(StringBuilder report)
    {
        if (string.IsNullOrWhiteSpace(report?.ToString())) return;
        Overlay.ShowReport("Current Missions Report", report.ToString());
    }

    public override bool ConfirmAction(string message)
    {
        // If the overlay just answered this question, consume and return the result.
        if (_pendingConfirmResult.HasValue)
        {
            bool result = _pendingConfirmResult.Value;
            _pendingConfirmResult = null;
            return result;
        }

        // First call: show the overlay, cancel the current action so the user
        // can read the warning. The engine will retry when they re-trigger.
        Overlay.ShowConfirm(message);
        Overlay.Dismissed += OnConfirmDismissed;
        return false;
    }

    private void OnConfirmDismissed()
    {
        Overlay.Dismissed -= OnConfirmDismissed;
        _pendingConfirmResult = Overlay.Result;
    }
}
