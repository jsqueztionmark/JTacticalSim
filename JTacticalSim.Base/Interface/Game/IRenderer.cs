using System;
using System.Text;
using System.Collections.Generic;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;

namespace JTacticalSim.API.Game
{
	public interface IRenderer
	{
		// Events
		event BattlePreRenderEvent BattlePreRender;
		event BattlePostRenderEvent BattlePostRender;
		event BattleRoundPreRenderEvent RoundPreRender;
		event BattleRoundPostRenderEvent RoundPostRender;
		event BattleSkirmishPreRenderEvent SkirmishPreRender;
		event BattleSkirmishPostRenderEvent SkirmishPostRender;
		event BoardPreRenderEvent BoardPreRender;
		event BoardPostRenderEvent BoardPostRender;
		event NodePreRenderEvent NodePreRender;
		event NodePostRenderEvent NodePostRender;
		event TilePreRenderEvent TilePreRender;
		event TilePostRenderEvent TilePostRender;
		event UnitPreRenderEvent UnitPreRender;
		event UnitPostRenderEvent UnitPostRender;

		// Event Handlers
		void On_BattlePreRender(EventArgs e);
		void On_BattlePostRender(EventArgs e);
		void On_RoundPreRender(EventArgs e);
		void On_RoundPostRender(EventArgs e);
		void On_SkirmishPreRender(EventArgs e);
		void On_SkirmishPostRender(EventArgs e);
		void On_BoardPreRender(EventArgs e);
		void On_BoardPostRender(EventArgs e);
		void On_NodePreRender(EventArgs e);
		void On_NodePostRender(EventArgs e);
		void On_TilePreRender(EventArgs e);
		void On_TilePostRender(EventArgs e);
		void On_UnitPreRender(EventArgs e);
		void On_UnitPostRender(EventArgs e);

		void RenderBoard();
		void RenderBoardFrame();
		void RenderMap(bool clear);
		void RenderFullMap(bool clear);
		void ZoomMap(CycleDirection direction);
		void CycleMapMode(CycleDirection direction);
		void RenderTileUnitInfoArea();
		void RenderMainScreen();
		void RenderReinforcementScreen();
		void RenderQuickSelectScreen();
		void RenderScenarioInfoScreen();
		void RenderTitleScreen();
		void RenderBattleScreen();
		void RenderHelpScreen();
		void RenderGameOverScreen();
		void RenderNode(INode node, int zoomLevel);
		void RenderTile(ITile tile, int zoomLevel);
		void RenderUnit(IUnit unit, int zoomLevel);
		void CenterSelectedNode();
		void RefreshActiveRoute();
		void RefreshActiveNodes();
		void SetCurrentViewableArea();
		void RefreshNodes(IEnumerable<INode> nodes);
		int RenderUnitStackInfo(IUnitStack stack);
		void ResetTileDemographics(IEnumerable<INode> nodes);
		void RenderUnitName(IUnit unit);
		void DisplayUnits(List<IUnit> units);
		void DisplayUnitInfo(IUnit unit);
		void DisplayNodeInfo(INode node);
		void DisplayTileInfo(ITile tile);
		void DisplayPlayerInfo(IPlayer player);
		void DisplayLegend();

		void RenderBattle(IBattle battle);
		void RenderBattleRound(IRound round);
		void RenderBattleSkirmish(ISkirmish skirmish);
		void RenderBattleSkirmishOutcome(ISkirmish skirmish);
		void RenderBattleRetreat(IBattle battle);
		void RenderBattleOutcome(IBattle battle);

		void LoadContent();
		void UnloadContent();
		void DisplayUserMessage(MessageDisplayType messageType, string message, Exception ex);
		void DisplayTaskExecutionReport(StringBuilder report);
		bool ConfirmAction(string message);
	}
}
