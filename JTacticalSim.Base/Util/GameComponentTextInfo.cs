using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.Utility;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;

namespace JTacticalSim.API.Component.Util
{
	public static class GameComponentTextInfo
	{
		public static string TextInfo(this INode node)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Node:  {0}_{1}_{2}".F(node.Location.X, node.Location.Y, node.Location.Z));
			sb.AppendLine("Name:  {0}".F(node.Name));
			sb.AppendLine("ID:  {0}".F(node.ID));
			sb.AppendLine("UID:  {0}".F(node.UID));
			sb.AppendLine("  IsSouthOuterBoundary:  {0}".F(node.IsSouthOuterBoundary.ToString()));
			sb.AppendLine("  IsNorthOuterBoundary:  {0}".F(node.IsNorthOuterBoundary.ToString()));
			sb.AppendLine("  IsWestOuterBoundary:  {0}".F(node.IsWestOuterBoundary.ToString()));
			sb.AppendLine("  IsEastOuterBoundary:  {0}".F(node.IsEastOuterBoundary.ToString()));
			sb.AppendLine(string.Empty);

			return sb.ToString();
		}

		public static string TextInfo(this ITile tile)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(string.Empty);
			sb.AppendLine("Held By:  {0}".F(tile.Country.Name));
			sb.AppendLine("Victory Points:  {0}".F(tile.VictoryPoints.ToString()));
			sb.AppendLine("IsGeographicChokepoint:  {0}".F(tile.IsGeographicChokePoint.ToString()));
			sb.AppendLine("IsPrimeTarget: {0}".F(tile.IsPrimeTarget));
			sb.AppendLine(string.Empty);

			// Strategic Assessments Values
			sb.AppendLine("NetAttackAdjusment:  {0}".F(tile.NetAttackAdjustment));
			sb.AppendLine("NetDefenceAdjusment:  {0}".F(tile.NetDefenceAdjustment));
			sb.AppendLine("NetMovementAdjusment:  {0}".F(tile.NetMovementAdjustment));
			sb.AppendLine("NetStealthAdjustment:  {0}".F(tile.NetStealthAdjustment));

			sb.AppendLine(string.Empty);

			var sv = tile.GetStrategicValues();			
			sb.AppendLine("StrategicOffensiveRating:  {0}".F(sv.OffensibleRating.ToString()));
			sb.AppendLine("StrategicDefensiveRating:  {0}".F(sv.DefensibleRating.ToString()));
			sb.AppendLine("StrategicMovementRating:  {0}".F(sv.MovementRating.ToString()));
			sb.AppendLine("StrategicStealthRating:  {0}".F(sv.StealthRating.ToString()));
			sb.AppendLine("VictoryPointRating:  {0}".F(sv.VictoryPointsRating.ToString()));
			sb.AppendLine("OtherStrategicRating:  {0}".F(sv.OtherAggragateRating.ToString()));
			sb.AppendLine("NetStrategicRating:  {0}".F(tile.GetNetStrategicValue()));
			
			sb.AppendLine(string.Empty);

			return sb.ToString();
		}

		public static string TextInfo(this IPlayer player)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(string.Empty);
			sb.AppendLine("Name : {0} - AIPlayer : {1}".F(player.Name, player.IsAIPlayer.ToString()));
			sb.AppendLine("Description : {0}".F(player.Description));
			sb.AppendLine("Faction : {0}".F(player.Country.Faction.Name));
			sb.AppendLine("ID : {0}".F(player.ID));
			sb.AppendLine("UID : {0}".F(player.UID));
			sb.AppendLine("  Reinforcement Points : {0}".F(player.TrackedValues.ReinforcementPoints.ToString()));
			sb.AppendLine("  Nuclear Charges : {0}".F(player.TrackedValues.NuclearCharges.ToString()));
			sb.AppendLine("  Victory Points : {0}".F(player.Country.Faction.GetCurrentVictoryPoints().ToString()));
			sb.AppendLine(string.Empty);

			return sb.ToString();
		}

		public static string TextInfo(this IScenario scenario)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(string.Empty);
			sb.AppendLine("{0} - {1}".F(scenario.Name, scenario.ComponentSet.Name));
			sb.AppendLine(string.Empty);
			sb.AppendLine("Countries (Country - Faction)");
			foreach (var country in scenario.Countries)
				sb.AppendLine("   → {0} - {1}".F(country.Name, country.Faction.Name));
			sb.AppendLine(string.Empty);

			sb.AppendLine("Victory Conditions (Faction - Condition : Value)");
			foreach (var vc in scenario.VictoryConditions)
				sb.AppendLine("   → {0} - {1} : {2}".F(vc.Faction.Name, vc.VictoryConditionType.Name, vc.Value));	
			sb.AppendLine(string.Empty);

			sb.AppendLine(scenario.Synopsis);

			return sb.ToString();
		}

		public static string TextInfo(this IUnit unit)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("{0}".F(unit.UnitInfo.UnitType.Branch.Name));
			sb.AppendLine("{0}".F(unit.Description));
			sb.AppendLine("____________________________________");
			sb.AppendLine("Current Posture:  {0}".F(unit.Posture.ToString()));
			sb.AppendLine("Current Mission:  {0}".F((unit.GetCurrentMission() != null) ? unit.GetCurrentMission().MissionType.Name : "None"));
			if (unit.GetCurrentMission() != null)
				sb.AppendLine("     * {0} turns to complete *".F(unit.GetCurrentMission().TurnsToComplete));
			sb.AppendLine("____________________________________");
			sb.AppendLine(string.Empty);
			sb.AppendLine("Unit Class:  {0}".F(unit.UnitInfo.UnitClass.Name));
			sb.AppendLine("Unit Type:  {0}".F(unit.UnitInfo.UnitType.Name));
			sb.AppendLine("Unit BaseType:  {0}".F(unit.UnitInfo.UnitType.BaseType.Name));
			sb.AppendLine("Unit Group Type:  {0}".F(unit.UnitInfo.UnitGroupType.Name));
			sb.AppendLine(string.Empty);
			sb.AppendLine("Is Supplied:  {0}".F(unit.IsSupplied().ToString()));
			sb.AppendLine("Has Medical Support:  {0}".F(unit.HasMedicalSupport().ToString()));
			sb.AppendLine("Remaining Movement Points:  {0}".F(unit.CurrentMoveStats.MovementPoints.ToString()));
			sb.AppendLine("Remaining Remote Fire Points:  {0}".F(unit.CurrentMoveStats.RemoteFirePoints.ToString()));
			sb.AppendLine("Is Nuclear Capable: {0}".F(unit.IsNuclearCapable().ToString()));
			if (unit.UnitInfo.UnitType.FuelConsumer)
				sb.AppendLine("Fuel Remaining:  {0}%".F(unit.FuelLevelPercent.ToString()));
			sb.AppendLine("Has Performed Action:  {0}".F(unit.CurrentMoveStats.HasPerformedAction.ToString()));
			sb.AppendLine("Can attack this turn:  {0}".F((unit.CanDoBattleThisTurn().ToString())));
			sb.AppendLine(string.Empty);
			sb.AppendLine("Attack Roll (at location):  {0}".F(unit.GetFullNetAttackValue()));
			sb.AppendLine("Defence Roll (at location):  {0}".F(unit.GetFullNetDefenceValue()));
			sb.AppendLine("Stealth Roll (at location):  {0}".F(unit.GetFullNetStealthValue()));
			sb.AppendLine("Attack Distance:  {0}".F(unit.RemoteAttackDistance));
			sb.AppendLine("Transport Weight Allowable:  {0}".F(unit.GetAllowableTransportWeight()));
			sb.AppendLine("Total Unit Weight:  {0}".F(unit.GetWeight()));

			return sb.ToString();
		}
		
	}
}
