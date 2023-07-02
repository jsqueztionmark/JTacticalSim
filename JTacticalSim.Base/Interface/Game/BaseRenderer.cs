using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.Service;
using JTacticalSim.API.Game;

namespace JTacticalSim.API.Game
{
	public abstract class BaseRenderer : BaseGameObject, IRenderer
	{
		protected BaseRenderer()
			: base(GameObjectType.RENDER)
		{}

		public event BattlePreRenderEvent BattlePreRender;
		public event BattlePostRenderEvent BattlePostRender;
		public event BattleRoundPreRenderEvent RoundPreRender;
		public event BattleRoundPostRenderEvent RoundPostRender;
		public event BattleSkirmishPreRenderEvent SkirmishPreRender;
		public event BattleSkirmishPostRenderEvent SkirmishPostRender;
		public event BoardPreRenderEvent BoardPreRender;
		public event BoardPostRenderEvent BoardPostRender;
		public event NodePreRenderEvent NodePreRender;
		public event NodePostRenderEvent NodePostRender;
		public event TilePreRenderEvent TilePreRender;
		public event TilePostRenderEvent TilePostRender;
		public event UnitPreRenderEvent UnitPreRender;
		public event UnitPostRenderEvent UnitPostRender;

#region IRenderer Members

		public abstract void RenderBoard();
		public abstract void RenderBoardFrame();
		public abstract void RenderMap(bool clear);
		public abstract void RenderFullMap(bool clear);
		public abstract void ZoomMap(CycleDirection direction);
		public abstract void CycleMapMode(CycleDirection direction);
		public abstract void RenderTileUnitInfoArea();
		public abstract void RenderMainScreen();
		public abstract void RenderReinforcementScreen();
		public abstract void RenderQuickSelectScreen();
		public abstract void RenderScenarioInfoScreen();
		public abstract void RenderTitleScreen();
		public abstract void RenderBattleScreen();
		public abstract void RenderHelpScreen();
		public abstract void RenderGameOverScreen();
		public abstract void RenderNode(INode node, int zoomLevel);
		public abstract void RenderTile(ITile tile, int zoomLevel);
		public abstract void RenderUnit(IUnit unit, int zoomLevel);
		public abstract void CenterSelectedNode();
		public abstract void RefreshActiveRoute();
		public abstract void RefreshActiveNodes();
		public abstract void SetCurrentViewableArea();
		public abstract void RefreshNodes(IEnumerable<INode> nodes);
		public abstract void ResetTileDemographics(IEnumerable<INode> nodes);
		public abstract int RenderUnitStackInfo(IUnitStack stack);
		public abstract void RenderUnitName(IUnit unit);
		public abstract void DisplayTileInfo(ITile tile);
		public abstract void DisplayUnits(List<IUnit> units);
		public abstract void DisplayUnitInfo(IUnit unit);
		public abstract void DisplayNodeInfo(INode node);
		public abstract void DisplayPlayerInfo(IPlayer player);
		public abstract void DisplayLegend();

		public abstract void RenderBattle(IBattle battle);
		public abstract void RenderBattleRound(IRound round);
		public abstract void RenderBattleSkirmish(ISkirmish skirmish);
		public abstract void RenderBattleRetreat(IBattle battle);
		public abstract void RenderBattleOutcome(IBattle battle);
		public abstract void RenderBattleSkirmishOutcome(ISkirmish skirmish);

		public abstract void LoadContent();
		public abstract void UnloadContent();
		public abstract void DisplayUserMessage(MessageDisplayType messageType, string message, Exception ex);
		public abstract void DisplayTaskExecutionReport(StringBuilder report);
		public abstract bool ConfirmAction(string message);

	// Event Handlers

		public virtual void On_BattlePreRender(EventArgs e)
		{
			if (BattlePreRender != null) BattlePreRender(this, e);
		}

		public virtual void On_BattlePostRender(EventArgs e)
		{
			if (BattlePostRender != null) BattlePostRender(this, e);
		}

		public virtual void On_RoundPreRender(EventArgs e)
		{
			if (RoundPreRender != null) RoundPreRender(this, e);
		}

		public virtual void On_RoundPostRender(EventArgs e)
		{
			if (RoundPostRender != null) RoundPostRender(this, e);
		}

		public void On_SkirmishPreRender(EventArgs e)
		{
			if (SkirmishPreRender != null) SkirmishPreRender(this, e);
		}

		public void On_SkirmishPostRender(EventArgs e)
		{
			if (SkirmishPostRender != null) SkirmishPostRender(this, e);
		}

		public virtual void On_BoardPreRender(EventArgs e)
		{
			if (BoardPreRender != null) BoardPreRender(this, e);
		}

		public virtual void On_BoardPostRender(EventArgs e)
		{
			if (BoardPostRender != null) BoardPostRender(this, e);
		}

		public virtual void On_NodePreRender(EventArgs e)
		{
			if (NodePreRender != null) NodePreRender(this, e);
		}

		public virtual void On_NodePostRender(EventArgs e)
		{
			if (NodePostRender != null) NodePostRender(this, e);
		}

		public virtual void On_TilePreRender(EventArgs e)
		{
			if (TilePreRender != null) TilePreRender(this, e);
		}

		public virtual void On_TilePostRender(EventArgs e)
		{
			if (TilePostRender != null) TilePostRender(this, e);
		}

		public virtual void On_UnitPreRender(EventArgs e)
		{
			if (UnitPreRender != null) UnitPreRender(this, e);
		}

		public virtual void On_UnitPostRender(EventArgs e)
		{
			if (UnitPostRender != null) UnitPostRender(this, e);
		}

#endregion

	}
}
