using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Component.Util;
using JTacticalSim.API.Game;
using JTacticalSim.Component.World;
using JTacticalSim.Utility;
using ConsoleControls;

namespace JTacticalSim.Console_OLD
{
	public class ConsoleRenderer : BaseRenderer
	{
		public ConsoleRenderer()
		{
		}

		public override void LoadContent()
		{
			// Do Nothing
		}

		private void LoadMapModes()
		{
			// Do nothing
		}

		private void LoadZooms()
		{
			// Do Nothing
		}

		public override void UnloadContent()
		{
			// Do nothing - no graphics content for console
		}

		// Handle errors

		public override void DisplayUserMessage(MessageDisplayType messageType, string message, Exception ex)
		{
			// Do nothing
		}

		public override void DisplayTaskExecutionReport(StringBuilder report)
		{
			// Do nothing
		}

		public override bool ConfirmAction(string message)
		{
			throw new NotImplementedException();
		}

		public void DisplayError(string error)
		{
			// Do Nothing
		}


		// Screens

		public override void RenderMainScreen()
		{
			RenderBoard();
		}

		public override void RenderReinforcementScreen()
		{
			throw new NotImplementedException();
		}

		public override void RenderHelpScreen()
		{
			throw new NotImplementedException();
		}

		public override void RenderQuickSelectScreen()
		{
			throw new NotImplementedException();
		}

		public override void RenderScenarioInfoScreen()
		{
			throw new NotImplementedException();
		}

		public override void RenderTitleScreen()
		{
			throw new NotImplementedException();
		}

		public override void RenderBattleScreen()
		{
			throw new NotImplementedException();
		}

		public override void RenderGameOverScreen()
		{
			throw new NotImplementedException();
		}


		// Board

		public override void RenderBoard()
		{
			throw new NotImplementedException();
		}

		public override void RenderBoardFrame()
		{
			throw new NotImplementedException();
		}

		public override void RenderMap(bool clear)
		{
			throw new NotImplementedException();
		}

		public override void RenderFullMap(bool clear)
		{
			throw new NotImplementedException();
		}

		public override void RenderTileUnitInfoArea()
		{
			throw new NotImplementedException();
		}

		public override void RenderNode(INode node, int zoomLevel)
		{
			throw new NotImplementedException();
		}

		public override void RenderTile(ITile tile, int zoomLevel)
		{
			throw new NotImplementedException();
		}

		public override void ResetTileDemographics(IEnumerable<INode> nodes)
		{
			throw new NotImplementedException();
		}

		public override void SetCurrentViewableArea()
		{
			throw new NotImplementedException();
		}

		public override void RefreshActiveNodes()
		{
			throw new NotImplementedException();
		}

		public override void RefreshActiveRoute()
		{
			throw new NotImplementedException();
		}

		public override void CenterSelectedNode()
		{
			throw new NotImplementedException();
		}

		public override void CycleMapMode(API.CycleDirection direction)
		{
			throw new NotImplementedException();
		}

		public override void ZoomMap(API.CycleDirection direction)
		{
			throw new NotImplementedException();
		}

		public override void RefreshNodes(IEnumerable<INode> nodes)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Only pertinent to console rendering
		/// </summary>
		/// <param name="tile"></param>
		private void RenderTileDemographics(ITile tile)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///  Only pertinent to console rendering
		/// </summary>
		/// <param name="nodes"></param>
		private void RenderTopTileBaseGeog(IEnumerable<INode> nodes)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Only pertinent to console rendering
		/// </summary>
		/// <param name="nodes"></param>
		private void RenderBottomTileBaseGeog(IEnumerable<INode> nodes)
		{
			throw new NotImplementedException();
		}

		public override int RenderUnitStackInfo(IUnitStack stack)
		{
			throw new NotImplementedException();
		}

		public override void DisplayPlayerInfo(IPlayer player)
		{
			throw new NotImplementedException();
		}

		public override void DisplayNodeInfo(INode node)
		{
			throw new NotImplementedException();
		}

		public override void DisplayTileInfo(ITile tile)
		{
			throw new NotImplementedException();
		}

		public override void DisplayLegend()
		{
			throw new NotImplementedException();
		}

	// Units

		public override void DisplayUnits(List<IUnit> units)
		{
			if (!units.Any())
				return;

			System.Console.WriteLine("Units : ");
			foreach (IUnit u in units)
			{
				System.Console.WriteLine("----------------------------------------------- |");
				u.DisplayInfo();
			}
		}

		public override void DisplayUnitInfo(IUnit unit)
		{
			System.Console.WriteLine(unit.TextInfo());
		}

		public override void RenderUnit(IUnit unit, int zoomLevel)
		{
			RenderUnitName(unit);
		}

		public override void RenderUnitName(IUnit unit)
		{
			throw new NotImplementedException();
		}

	// Battle

		public override void RenderBattle(IBattle battle)
		{
			// Do Nothing
		}

		public override void RenderBattleRound(IRound round)
		{
			// Do Nothing
		}

		public override void RenderBattleSkirmish(ISkirmish skirmish)
		{
			// Do Nothing
		}

		public override void RenderBattleSkirmishOutcome(ISkirmish skirmish)
		{
			// Do Nothing
		}

		public override void RenderBattleOutcome(IBattle battle)
		{
			// Do Nothing
		}

		public override void RenderBattleRetreat(IBattle battle)
		{
			// Do Nothing
		}

	// Reinforcements


#region Event Handlers

		public override void On_BoardPostRender(EventArgs e)
		{
			throw new NotImplementedException();
		}

		public override void On_BoardPreRender(EventArgs e)
		{}

#endregion


#region DrawUtilities

		private void DrawLine()
		{
			System.Console.Write("________________________________________________________");
		}

#endregion

	}
}
