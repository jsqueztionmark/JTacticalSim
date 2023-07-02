using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.Game
{
	public interface IInputCommandContainer
	{
		// Board
		void ZoomMap(CycleDirection direction);
		void CycleMapMode(CycleDirection direction);
		void DisplayCurrentNode();
		void SetCurrentNode();
		void SetSelectedUnit();
		void SetSelectedUnits();
		void SetSelectedUnitWithAttached();
		void UnselectAllUnits();
		void MoveUnitToSelectedNode();
		void MoveUnitsToSelectedNode();
		void CycleUnits();
		void GetReinforcements();

		// Battle
		void BarrageLocation();
		void NukeLocation();
		void DoBattleAtLocation();

		// Unit
		void AddUnit();
		void RemoveUnit();
		void EditUnit();
		void DisplayUnit();
		void DisplayUnits();
		void AttachUnit();
		void AttachUnits();
		void DetachUnit();
		void DetachUnits();
		void LoadUnit();
		void SetBattlePosture(BattlePosture posture);
		void DeployUnit();
		void DisplayAssignedUnits();
		void BuildInfrastructure();
		void DestroyInfrastructure();

		// Player
		void DisplayPlayer();
		void AddReinforcementUnit();
		void PlaceReinforcementUnit();
		void EndTurn();

		// Screens
		void RefreshScreen();
		void DisplayReinforcementsScreen();
		void DisplayMainBoardScreen();
		void DisplayTitleScreen();
		void DisplayUnitQuickSelectScreen();
		void DisplayScenarioInfoScreen();
		void DisplayHelpScreen();

		// Utility
		void Exit();
		void Play();
		void LoadGame();
		void NewGame();
		void SaveGame();
		void SaveGameAs();
		void DeleteGame();
		void ScenarioEditor();
		void DisplayCommandList();
	}
}
