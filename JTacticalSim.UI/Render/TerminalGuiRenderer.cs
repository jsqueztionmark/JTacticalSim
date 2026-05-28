using System.Text;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using JTacticalSim.API;
using JTacticalSim.API.AI;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.UI.Views;

namespace JTacticalSim.UI.Render;

public class TerminalGuiRenderer : BaseRenderer
{
    public View CurrentView { get; private set; }

    // ── Screen routing ────────────────────────────────────────────────────

    public override void RenderTitleScreen()    => CurrentView = new GameTitleView();
    public override void RenderMainScreen()     => CurrentView = new MainGameView();
    public override void RenderBattleScreen()   => CurrentView = new MainGameView(); // placeholder
    public override void RenderHelpScreen()     => CurrentView = new MainGameView(); // placeholder
    public override void RenderReinforcementScreen()  => CurrentView = new MainGameView(); // placeholder
    public override void RenderQuickSelectScreen()    => CurrentView = new MainGameView(); // placeholder
    public override void RenderScenarioInfoScreen()   => CurrentView = new MainGameView(); // placeholder
    public override void RenderGameOverScreen()       => CurrentView = new MainGameView(); // placeholder

    // ── Dialogs ───────────────────────────────────────────────────────────

    public override void DisplayUserMessage(MessageDisplayType messageType, string message, Exception ex)
    {
        var text = ex != null ? $"{message}\n{ex.Message}" : message;
        if (messageType == MessageDisplayType.ERROR)
            MessageBox.ErrorQuery(UIContext.Instance.App, "Error", text, "OK");
        else
            MessageBox.Query(UIContext.Instance.App, "Info", text, "OK");
    }

    public override void DisplayTaskExecutionReport(StringBuilder report)
        => MessageBox.Query(UIContext.Instance.App, "Report", report.ToString(), "OK");

    public override bool ConfirmAction(string message)
    {
        int? result = MessageBox.Query(UIContext.Instance.App, "Confirm", message, "Yes", "No");
        return result == 0;
    }

    // ── Stubs — implemented when screens are built ────────────────────────

    public override void RenderBoard() { }
    public override void RenderBoardFrame() { }
    public override void RenderMap(bool clear) { }
    public override void RenderFullMap(bool clear) { }
    public override void ZoomMap(CycleDirection direction) { }
    public override void CycleMapMode(CycleDirection direction) { }
    public override void RenderTileUnitInfoArea() { }
    public override void RenderNode(INode node, int zoomLevel) { }
    public override void RenderTile(ITile tile, int zoomLevel) { }
    public override void RenderUnit(IUnit unit, int zoomLevel) { }
    public override void CenterSelectedNode() { }
    public override void RefreshActiveRoute() { }
    public override void RefreshActiveNodes() { }
    public override void SetCurrentViewableArea() { }
    public override void RefreshNodes(IEnumerable<INode> nodes) { }
    public override void ResetTileDemographics(IEnumerable<INode> nodes) { }
    public override int RenderUnitStackInfo(IUnitStack stack) => 0;
    public override void RenderUnitName(IUnit unit) { }
    public override void DisplayTileInfo(ITile tile) { }
    public override void DisplayUnits(List<IUnit> units) { }
    public override void DisplayUnitInfo(IUnit unit) { }
    public override void DisplayNodeInfo(INode node) { }
    public override void DisplayPlayerInfo(IPlayer player) { }
    public override void DisplayLegend() { }
    public override void RenderBattle(IBattle battle) { }
    public override void RenderBattleRound(IRound round) { }
    public override void RenderBattleSkirmish(ISkirmish skirmish) { }
    public override void RenderBattleRetreat(IBattle battle) { }
    public override void RenderBattleOutcome(IBattle battle) { }
    public override void RenderBattleSkirmishOutcome(ISkirmish skirmish) { }
    public override void LoadContent() { }
    public override void UnloadContent() { }
}
