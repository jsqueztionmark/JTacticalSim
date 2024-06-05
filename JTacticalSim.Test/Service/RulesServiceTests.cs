using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.Game;
using JTacticalSim.API;
using JTacticalSim.Utility;
using NUnit.Framework;
using JTacticalSim.LINQPad.Plugins;
using JTacticalSim.Component.GameBoard;
using JTacticalSim.Component.Game;
using JTacticalSim.Component.World;
using JTacticalSim.Component.AI;
using JTacticalSim.Component.Data;

namespace JTacticalSim.Test.Service
{
	/// <summary>
	/// Tests for the AI Service
	/// </summary>
	[TestFixture]
	public class RulesServiceTests : BaseTest
	{
		// Node/Tile

		[TestCase("iran", "True")]
		[TestCase("usa", "False")]
		public void Node_Is_Faction_Node(params string[] args)
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(2, 2, 0); // shorline east/Iran
			var faction = TheGame.JTSServices.GenericComponentService.GetByName<Faction>(args[0]);
	
			var result = TheGame.JTSServices.RulesService.NodeIsFactionNode(node.Location, faction);

			Assert.AreEqual(result.Result, Convert.ToBoolean(args[1]));
		}

		[Test]
		public void Node_Is_Valid_For_Move_Succeeds()
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(0, 1, 0); 
			var unit = ScriptTestUnits.LOADME_HARMOR;
	
			var result = TheGame.JTSServices.RulesService.NodeIsValidForMove(unit, node);

			Assert.IsTrue(result.Result);
			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
		}

		[Test]
		public void Node_Is_Valid_For_Move_Fails_DueTo_Tile_Incompatibility()
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(1, 1, 0); 
			var unit = ScriptTestUnits.LOADME_HARMOR;
	
			var result = TheGame.JTSServices.RulesService.NodeIsValidForMove(unit, node);

			Assert.IsFalse(result.Result);
			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
		}

		[Test]
		public void Node_Is_Valid_For_Move_Indeterminate_DueTo_Range()
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(4, 4, 0); 
			var unit = ScriptTestUnits.LOADME_HARMOR;
	
			var result = TheGame.JTSServices.RulesService.NodeIsValidForMove(unit, node);

			Assert.IsFalse(result.Result);
			Assert.AreEqual(result.Status, ResultStatus.OTHER);
		}

		[TestCase(2, true)]			// Over
		[TestCase(3, true)]			// Equals
		[TestCase(4, false)]		// Under
		public void Tile_Has_Max_Units_For_Faction(int max, bool expected)
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(0, 0, 0);
			var tile = node.DefaultTile;
			var faction = TheGame.JTSServices.GameService.GetFactionByID(0);

			var r = TheGame.JTSServices.RulesService.TileHasMaxUnitsForFaction(tile, faction, max);

			Assert.AreEqual(r.Result, expected);
		}

		//[TestCase(7, 2, true)]		// Node is a mountain pass	:CURRENTLY NOT ON MAP
		[TestCase(7, 3, false)]		// Node itself has mountains
		[TestCase(5, 1, false)]		// Node is not between mountains
		public void Tile_Is_Pass_Through_Restricted_Movement(int x, int y, bool expected)
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(x, y, 0);
			var neighborPairs = TheGame.JTSServices.NodeService.GetNeighborNodesOppositeTilePairs(node);
			var r = TheGame.JTSServices.RulesService.TileIsPassThroughRestrictedMovement(node.DefaultTile, neighborPairs);

			Assert.AreEqual(r.Result, expected);
		}

		[TestCase(7, 2, false)]		// Node is a mountain pass but is not bound by base geography
		[TestCase(4, 2, false)]		// Node is land surrounded by land
		[TestCase(1, 2, true)]		// Hybrid with hybrids on both sides
		//[TestCase(4, 5, true)]		// Water with land on one side, null on other	:CURRENTLY NOT ON MAP
		//[TestCase(7, 6, true)]		// Water with hybrid on one side, null on other	:CURRENTLY NOT ON MAP
		[TestCase(0, 0, true)]		// Corner
		[TestCase(1, 4, false)]		// Water with land on one side, water on the other
		[TestCase(5, 0, false)]		// Land with land on one side, null on the other
		public void Tile_Is_Narrow_Geography(int x, int y, bool expected)
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(x, y, 0);
			var neighborPairs = TheGame.JTSServices.NodeService.GetNeighborNodesOppositeTilePairs(node);
			var r = TheGame.JTSServices.RulesService.TileIsNarrowGeography(node.DefaultTile, neighborPairs);

			Assert.AreEqual(r.Result, expected);
		}

		[Test]
		public void Tile_Is_Chokepoint_SUCCEEDS_FOR_BRIDGE()
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(1, 4, 0);
			var r = TheGame.JTSServices.RulesService.TileIsChokepoint(node.DefaultTile);

			Assert.AreEqual(r.Result, true);
		}

		[TestCase("0", "0", "0", "true")]		// Land
		[TestCase("0", "3", "0", "false")]		// Water
		[TestCase("0", "4", "0", "true")]		// Water with bridge (override)	
		public void Tile_Is_Allowable_For_Unit_Succeeds_Fails(params string[] args)
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2])); // shorline east/Iran
			var unit = ScriptTestUnits.LOADME_HARMOR;
			var result = TheGame.JTSServices.RulesService.TileIsAllowableForUnit(unit, node.DefaultTile);

			Assert.AreEqual(result.Result, Convert.ToBoolean(args[3]));
			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
		}

		[TestCase("0", "3", "0", "false")]		// Water
		[TestCase("0", "4", "0", "true")]		// Water with bridge
		public void Tile_Has_Movement_Override_For_Unit_Succeeds_Fails(params string[] args)
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2])); // shorline east/Iran
			var unit = ScriptTestUnits.LOADME_HARMOR;
			var result = TheGame.JTSServices.RulesService.TileHasMovementOverrideForUnit(unit, node.DefaultTile, Direction.EAST);

			Assert.AreEqual(result.Result, Convert.ToBoolean(args[3]));
			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
		}

		// Unit

		[Test]
		public void Get_Allowable_Deploy_Distance_For_Transport_WATERCRAFT()
		{
			var transport = ScriptTestUnits.LOADTO_TRANSPORT;
			var result = TheGame.JTSServices.RulesService.GetAllowableDeployDistanceForTransport(transport);

			Assert.AreEqual(result.Result, 1);
		}

		[Test]
		public void Get_Allowable_Deploy_Distance_For_Transport_OTHER()
		{
			var transport = ScriptTestUnits.LOADTO_HELO;
			var result = TheGame.JTSServices.RulesService.GetAllowableDeployDistanceForTransport(transport);

			Assert.AreEqual(result.Result, 0);
		}

		[Test]
		public void Get_Allowable_Load_Distance_For_Transport_WATERCRAFT()
		{
			var transport = ScriptTestUnits.LOADTO_TRANSPORT;
			var result = TheGame.JTSServices.RulesService.GetAllowableDeployDistanceForTransport(transport);

			Assert.AreEqual(result.Result, 1);
		}

		[Test]
		public void Get_Allowable_Load_Distance_For_Transport_OTHER()
		{
			var transport = ScriptTestUnits.LOADTO_HELO;
			var result = TheGame.JTSServices.RulesService.GetAllowableDeployDistanceForTransport(transport);

			Assert.AreEqual(result.Result, 0);
		}

		[TestCase("b_infantry", "false")]
		[TestCase("foo_bar", "true")]
		public void Unit_Name_Is_Unique(params string[] args)
		{
			var result = TheGame.JTSServices.RulesService.UnitNameIsUnique(args[0]);	

			Assert.AreEqual(result.Result, Convert.ToBoolean(args[1]));
		}

		[Test]
		public void Units_Can_Do_Battle_With_Units_Fails_None_Compatible_LOCAL()
		{
			var attackers = new List<IUnit>
				{ 
					ScriptTestUnits.LOADTO_HELO,
					ScriptTestUnits.A_Tank
				};
		
			var defenders = new List<IUnit>
				{
					ScriptTestUnits.SubSquad_A
				};
	
			var result = TheGame.JTSServices.RulesService.UnitsCanDoBattleWithUnits(attackers, defenders, BattleType.LOCAL);
	
			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
			Assert.IsFalse(result.Result);
		}

		[Test]
		public void Units_Can_Do_Battle_With_Units_Succeeds_AtLeastOne_Compatible_LOCAL()
		{
			var attackers = new List<IUnit>
				{ 
					ScriptTestUnits.LOADTO_HELO,
					ScriptTestUnits.A_Tank,
					ScriptTestUnits.Navy_HQ
				};
		
			var defenders = new List<IUnit>
				{
					ScriptTestUnits.SubSquad_A
				};
	
			var result = TheGame.JTSServices.RulesService.UnitsCanDoBattleWithUnits(attackers, defenders, BattleType.LOCAL);
	
			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
			Assert.IsTrue(result.Result);
		}

		[TestCase("Navy_HQ", "SubSquad_A", "true")]				// destroyer against subs/ not ANTI-SUB, but natively allowed by unit type
		[TestCase("A_Tank", "SubSquad_A", "false")]				// ground units against subs			
		[TestCase("A_Tank", "FighterSquad_A", "false")]			// ground units should not be able to attack high altitude units
		[TestCase("FighterSquad_A", "A_Tank", "true")]			// high altitude should be able to attack ground units
		[TestCase("antisub_fighter", "SubSquad_A", "true")]		// anti-sub class units can do battle with subs
		[TestCase("FighterSquad_A", "SubSquad_A", "false")]		// non-anti-sub units can not do battle with subs
		public void Unit_Can_Do_Battle_With_Unit_LOCAL(params string[] args)
		{
			var attacker = TheGame.JTSServices.GenericComponentService.GetByName<Unit>(args[0]);
			var defender = TheGame.JTSServices.GenericComponentService.GetByName<Unit>(args[1]);
			var result = TheGame.JTSServices.RulesService.UnitCanDoBattleWithUnit(attacker, defender, BattleType.LOCAL);

			Assert.AreEqual(result.Result, Convert.ToBoolean(args[2]));
		}

		[TestCase("BigGuns", "Fadaykin_SF", "true")]			// Artillery can barrage
		[TestCase("Fadaykin_SF", "BigGuns", "false")]			// Infantry can not barrage
		public void Unit_Can_Do_Battle_With_Unit_BARRAGE(params string[] args)
		{
			var attacker = TheGame.JTSServices.GenericComponentService.GetByName<Unit>(args[0]);
			var defender = TheGame.JTSServices.GenericComponentService.GetByName<Unit>(args[1]);
			var result = TheGame.JTSServices.RulesService.UnitCanDoBattleWithUnit(attacker, defender, BattleType.BARRAGE);

			Assert.AreEqual(result.Result, Convert.ToBoolean(args[2]));
		}


		// Rules Calculations... may not continue. These are highly dynamic and data biased

		//[Test]
		//public void Calculate_Total_Unit_Weight_Sums_HQ_And_All_Attached_Units()
		//{
		//	var unit = ScriptTestUnits.Btl101; 
		//	var allUnits = unit.GetAllAttachedUnits().ToList();	
		//	allUnits.Add(unit);

		//	// Make sure we still have all the units attached
		//	Assert.Contains(ScriptTestUnits.A_Tank, allUnits);
		//	Assert.Contains(ScriptTestUnits.C_Tank, allUnits);
		//	Assert.Contains(ScriptTestUnits.B_Tank, allUnits);
		//	Assert.Contains(ScriptTestUnits.B_Infantry, allUnits);
		//	Assert.Contains(ScriptTestUnits.A_Infantry, allUnits);
		//	Assert.Contains(ScriptTestUnits.C_Infantry, allUnits);
		//	Assert.Contains(ScriptTestUnits.Btl101, allUnits);
		//	Assert.IsTrue(allUnits.Count() == 7);

		//	var result = TheGame.JTSServices.RulesService.CalculateTotalUnitWeight(unit);

		//	Assert.AreEqual(result.Result, 65.5);
		//}

		//[Test]
		//public void Calculate_Total_Unit_Weight_Returns_Unit_Weight_With_No_Attached_Units()
		//{
		//	var unit = ScriptTestUnits.B_Infantry; 
		//	var allUnits = unit.GetAllAttachedUnits().ToList();	
		//	allUnits.Add(unit);

		//	// Make sure we have only the one unit
		//	Assert.Contains(ScriptTestUnits.B_Infantry, allUnits);
		//	Assert.IsTrue(allUnits.Count() == 1);

		//	var result = TheGame.JTSServices.RulesService.CalculateTotalUnitWeight(unit);

		//	Assert.AreEqual(result.Result, 8);
		//}

		//[Test]
		//public void Calculate_Unit_Weight_Returns_Weight_For_Unit_Only()
		//{
		//	var unit = ScriptTestUnits.Btl101; 

		//	var result = TheGame.JTSServices.RulesService.CalculateUnitWeight(unit);

		//	Assert.AreEqual(result.Result, 8);
		//}

		//[TestCase("b_infantry", "0")]
		//[TestCase("loadto_helo", "15")]
		//public void Calculate_Allowable_Transport_Weight(params string[] args)
		//{
		//	var unit = TheGame.JTSServices.GenericComponentService.GetByName<Unit>(args[0]);
		//	var result = TheGame.JTSServices.RulesService.CalculateAllowableTransportWeight(unit);

		//	Assert.AreEqual(result.Result, Convert.ToDouble(args[1]));
		//}

		[TestCase("b_infantry", "true")]
		[TestCase("loadto_helo", "false")]
		[TestCase("FighterSquad_A", "false")]
		public void Unit_Can_Claim_Node_For_Faction(params string[] args)
		{
			var unit = TheGame.JTSServices.GenericComponentService.GetByName<Unit>(args[0]);
			var result = TheGame.JTSServices.RulesService.UnitCanClaimNodeForFaction(unit);

			Assert.AreEqual(result.Result, Convert.ToBoolean(args[1]));
		}

		[TestCase("BigGuns", "101btl", "false")]			// squad to battalion
		[TestCase("b_infantry", "101btl", "true")]			// company to battalion
		[TestCase("b_infantry", "BigGuns", "false")]		// company to platoon
		[TestCase("bigguns", "b_infantry", "true")]			// squad to company
		public void Unit_Can_Attach_To_Unit(params string[] args)
		{
			var unit = TheGame.JTSServices.GenericComponentService.GetByName<Unit>(args[0]);
			var attachToUnit = TheGame.JTSServices.GenericComponentService.GetByName<Unit>(args[1]);
			var result = TheGame.JTSServices.RulesService.UnitCanAttachToUnit(unit, attachToUnit);

			Assert.AreEqual(result.Result, Convert.ToBoolean(args[2]));
		}

		[TestCase("BigGuns", "true")]			// Within max distance
		[TestCase("TestCav", "false")]			// Outside max distance 3
		[TestCase("LOADME_FIGHTER", "false")]	// within max distance, but plane UnitBaseType can not be supplied
		public void Unit_Is_Supplied(params string[] args)
		{
			var unit = TheGame.JTSServices.GenericComponentService.GetByName<Unit>(args[0]);
			var result = TheGame.JTSServices.RulesService.UnitIsSupplied(unit);

			Assert.AreEqual(result.Result, Convert.ToBoolean(args[1]));
		}

		[Test]
		public void Unit_Has_Medical_Support_Fails_Incompatible_UnitBaseType()
		{
			var unit = ScriptTestUnits.FighterSquad_Z;	
			var result = TheGame.JTSServices.RulesService.UnitHasMedicalSupport(unit);

			Assert.IsFalse(result.Result);
		}

		[TestCase("medsp_tanks", "true")]
		[TestCase("a_infantry", "false")]
		public void Unit_Has_Medical_Support_SucceedsFails_Due_To_Available_Medical_Unit(params string[] args)
		{
			var unit = TheGame.JTSServices.GenericComponentService.GetByName<Unit>(args[0]);	
			var result = TheGame.JTSServices.RulesService.UnitHasMedicalSupport(unit);

			Assert.AreEqual(result.Result, Convert.ToBoolean(args[1]));
		}

		[TestCase("3", "1", "0", "true")]   // airport
		[TestCase("2", "1", "0", "false")]	// military base
		[TestCase("0", "5", "0", "true")]	// aircraft carrier
		[TestCase("0", "1", "0", "false")]	// multiple non-compatible transport units
		[TestCase("3", "0", "0", "false")]	// nothing
		public void Unit_Can_Refuel_At_Location_Fighter(params string[] args)
		{
			var unit = ScriptTestUnits.LOADME_FIGHTER;
			var node = TheGame.JTSServices.NodeService.GetNodeAt(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));

			var result = TheGame.JTSServices.RulesService.UnitCanRefuelAtLocation(unit, node);

			Assert.AreEqual(result.Result, Convert.ToBoolean(args[3]));
		}

		[TestCase("3", "1", "0", "true")]   // airport
		[TestCase("2", "1", "0", "false")]	// military base
		[TestCase("0", "5", "0", "false")]	// aircraft carrier
		[TestCase("0", "1", "0", "false")]	// multiple non-compatible transport units
		[TestCase("3", "0", "0", "false")]	// nothing
		public void Unit_Can_Refuel_At_Location_Bomber(params string[] args)
		{
			var unit = ScriptTestUnits.LOADME_BOMBER;
			var node = TheGame.JTSServices.NodeService.GetNodeAt(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));

			var result = TheGame.JTSServices.RulesService.UnitCanRefuelAtLocation(unit, node);

			Assert.AreEqual(result.Result, Convert.ToBoolean(args[3]));
		}

		[TestCase("3", "1", "0", "true")]   // airport
		[TestCase("2", "1", "0", "true")]	// military base
		[TestCase("0", "5", "0", "true")]	// aircraft carrier
		[TestCase("0", "1", "0", "true")]	// multiple non-compatible transport units, but is supplied
		[TestCase("3", "0", "0", "true")]	// nothing - but is supplied
		public void Unit_Can_Refuel_At_Location_Helicopter(params string[] args)
		{
			var unit = ScriptTestUnits.LOADTO_HELO;
			var node = TheGame.JTSServices.NodeService.GetNodeAt(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));

			var result = TheGame.JTSServices.RulesService.UnitCanRefuelAtLocation(unit, node);

			Assert.AreEqual(result.Result, Convert.ToBoolean(args[3]));
		}


		[TestCase("specialforces", "false")]
		[TestCase("attack", "true")]
		public void Unit_Is_UnitClass(params string[] args)
		{
			var unit = ScriptTestUnits.A_SandyTanks;
			var result = TheGame.JTSServices.RulesService.UnitIsUnitClass(unit, args[0]);

			Assert.AreEqual(result.Result, Convert.ToBoolean(args[1]));
		}

		[TestCase("infantry", "false")]
		[TestCase("armor", "true")]
		public void Unit_Is_UnitBaseType(params string[] args)
		{
			var unit = ScriptTestUnits.A_SandyTanks;
			var result = TheGame.JTSServices.RulesService.UnitIsUnitBaseType(unit, args[0]);

			Assert.AreEqual(result.Result, Convert.ToBoolean(args[1]));
		}

		[Test]
		public void Unit_Is_Deployable_To_Node_Succeeds()
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(0, 1, 0); 
			var unit = ScriptTestUnits.LOADME_HARMOR;
	
			var result = TheGame.JTSServices.RulesService.UnitIsDeployableToNode(unit, node);

			Assert.IsTrue(result.Result);
			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
		}

		[Test]
		public void Unit_Is_Deployable_To_Node_Fails_DueTo_Tile_Incompatibility()
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(1, 1, 0); 
			var unit = ScriptTestUnits.LOADME_HARMOR;
	
			var result = TheGame.JTSServices.RulesService.UnitIsDeployableToNode(unit, node);

			Assert.IsFalse(result.Result);
			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
		}

		[Test]
		public void Unit_Is_Deployable_To_Node_Indeterminate_DueTo_Range()
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(4, 4, 0); 
			var unit = ScriptTestUnits.LOADME_HARMOR;
	
			var result = TheGame.JTSServices.RulesService.UnitIsDeployableToNode(unit, node);

			Assert.IsFalse(result.Result);
			Assert.AreEqual(result.Status, ResultStatus.OTHER);
		}

		[Test]
		public void Unit_Can_Reinforce_At_Location_Succeeds_NonHQ_HQ_Present()
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(0, 0, 0);
			var unit = ScriptTestUnits.A_Infantry;	
			var result = TheGame.JTSServices.RulesService.UnitCanReinforceAtLocation(unit, node);

			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
			Assert.IsTrue(result.Result);
		}

		[Test]
		public void Unit_Can_Reinforce_At_Location_Succeeds_HQ_NoHQPresent_NoMilitaryInstallation()
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(0, 1, 0);
			var unit = ScriptTestUnits.Btl101;	
			var result = TheGame.JTSServices.RulesService.UnitCanReinforceAtLocation(unit, node);

			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
			Assert.IsFalse(result.Result);
		}

		[Test]
		public void Unit_Can_Reinforce_At_Location_Fails_NonHQ_No_HQ_Present()
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(0, 1, 0);
			var unit = ScriptTestUnits.A_Infantry;	
			var result = TheGame.JTSServices.RulesService.UnitCanReinforceAtLocation(unit, node);

			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
			Assert.IsFalse(result.Result);
		}

		[Test]
		public void Unit_CanReinforce_At_Location_Fails_NoAirportForPlane()
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(0, 1, 0);
			var unit = ScriptTestUnits.BomberSquad_A;	
			var result = TheGame.JTSServices.RulesService.UnitCanReinforceAtLocation(unit, node);

			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
			Assert.IsFalse(result.Result);
		}

		[Test]
		public void Unit_CanReinforce_At_Location_Fails_AirportForPlane_NoHQPresent()
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(0, 0, 0);
			var unit = ScriptTestUnits.BomberSquad_A;	
			var result = TheGame.JTSServices.RulesService.UnitCanReinforceAtLocation(unit, node);

			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
			Assert.IsFalse(result.Result);
		}

		[Test]
		public void Unit_CanReinforce_At_Location_Succeeds_AirportForPlane_HQPresent()
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(0, 0, 0);
			var unit = ScriptTestUnits.LOADME_FIGHTER;	
			var result = TheGame.JTSServices.RulesService.UnitCanReinforceAtLocation(unit, node);

			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
			Assert.IsTrue(result.Result);
		}

		[Test]
		public void Unit_Can_Reinforce_At_Location_Fails_Non_Friendly_Node()
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(2, 2, 0);
			var unit = ScriptTestUnits.Btl101;	
			var result = TheGame.JTSServices.RulesService.UnitCanReinforceAtLocation(unit, node);

			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
			Assert.False(result.Result);
		}

		[TestCase("101Btl", "Attack", false)]
		[TestCase("101Btl", "Defend", true)]
		[TestCase("101Btl", "ProduceUnits", true)]
		[TestCase("A_Infantry", "Attack", true)]
		[TestCase("A_Infantry", "ProduceUnits", false)]
		public void Unit_Can_Perform_Task(string unitName, string taskTypeName, bool expectedResult)
		{
			var unit = TheGame.JTSServices.GenericComponentService.GetByName<Unit>(unitName);
			var task = TheGame.JTSServices.GenericComponentService.GetByName<UnitTaskType>(taskTypeName);
			var result = TheGame.JTSServices.RulesService.UnitCanPerformTask(unit, task);

			Assert.AreEqual(expectedResult, result.Result);
		}

		[TestCase(4, 7, 4)]
		[TestCase(5, 8, 5)]
		[TestCase(1, 7, 0)]
		public void Calculate_Threat_Distance(int factor, int baseValue, int expected)
		{
			var r = TheGame.JTSServices.RulesService.CalculateThreatDistance(factor, baseValue);
			Assert.AreEqual(r, expected);
		}

		[TestCase("A_Infantry", false)]	// In use
		[TestCase("A&Infantry", false)]	// Special Characters
		[TestCase("A Infantry", true)]	// Spaces
		[TestCase("", false)]			// empty string
		[TestCase("newName", true)]		
		public void Name_Is_Valid(string name, bool expected)
		{
			var r = TheGame.JTSServices.RulesService.NameIsValid<Unit>(name);
			Assert.AreEqual(r.Result, expected);
		}

		[TestCase("USA", true)]
		public void Country_Has_Dependant_Components(string countryName, bool expected)
		{
			var country = TheGame.JTSServices.GenericComponentService.GetByName<Country>(countryName);
			var r = TheGame.JTSServices.RulesService.CountryHasDependantComponents(country);
			Assert.AreEqual(r.Result, expected);
		}

		[TestCase("0", "2", "0", "false", "12")]	// Shorline West With Bridge East
		[TestCase("3", "0", "0", "true", "11")]		// Land with existing roads east/west
		[TestCase("2", "1", "0", "true", "11")]		// Shorline East with no roads or bridges - road
		[TestCase("2", "1", "0", "true", "12")]		// Shorline East with no roads or bridges - bridge
		public void Tile_Can_Support_Infrastructure_Building(params string[] args)
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));
			var demographic = TheGame.JTSServices.DemographicService.GetDemographicByID(Convert.ToInt32(args[4]));
			var result = TheGame.JTSServices.RulesService.TileCanSupportInfrastructureBuilding(node.DefaultTile, demographic);

			Assert.AreEqual(result.Result, Convert.ToBoolean(args[3]));
			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
		}

		[TestCase("0", "2", "0", "false", "14")]	// demographic not infrastructure (trees)
		public void Tile_Can_Support_Infrastructure_Building_FAIL_BAD_PARAMETER(params string[] args)
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));
			var demographic = TheGame.JTSServices.DemographicService.GetDemographicByID(Convert.ToInt32(args[4]));
			var result = TheGame.JTSServices.RulesService.TileCanSupportInfrastructureBuilding(node.DefaultTile, demographic);

			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
		}

		[TestCase("3", "0", "0", "true", "11", "n,s")]		// Land with existing roads east/west
		[TestCase("2", "1", "0", "true", "11", "n,s,e")]	// Shorline East with no roads or bridges - road
		[TestCase("2", "1", "0", "true", "12", "w")]		// Shorline East with no roads or bridges - bridge
		public void Get_Orientation_Allowable_For_Demographic_Class_By_Tile(params string[] args)
		{
			var node = TheGame.JTSServices.NodeService.GetNodeAt(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));
			var demographic = TheGame.JTSServices.DemographicService.GetDemographicByID(Convert.ToInt32(args[4]));
			var result = TheGame.JTSServices.TileService.GetOrientationAllowableForDemographicClassByTile(node.DefaultTile, demographic.DemographicClass);

			var expectedOrientation = Orienting.ParseOrientationString(args[5], new char[] {','});

			Assert.IsTrue(result.Count() == expectedOrientation.Count);
 			Assert.IsTrue(!expectedOrientation.Except(result).Any());

		}

		//[TestCase(StrategicAssessmentRating.HIGH, StrategicAssessmentRating.LOW, StrategicAssessmentRating.VERYLOW)]
		//[TestCase(StrategicAssessmentRating.LOW, StrategicAssessmentRating.HIGH, StrategicAssessmentRating.MEDIUM)]
		//[TestCase(StrategicAssessmentRating.VERYHIGH, StrategicAssessmentRating.LOW, StrategicAssessmentRating.HIGH)]
		//[TestCase(StrategicAssessmentRating.HIGH, StrategicAssessmentRating.VERYHIGH, StrategicAssessmentRating.HIGH)]
		//public void Get_Overall_Rating_For_Strategic_Assessment(StrategicAssessmentRating defensive,
		//                                                        StrategicAssessmentRating offensive,
		//                                                        StrategicAssessmentRating expected)
		//{
		//    var assessment = new API.InfoObjects.StrategicAssessmentInfo();
	
		//    assessment.DefensibleRating = defensive;
		//    assessment.OffensibleRating = offensive;
	
		//    var r = TheGame.JTSServices.RulesService.GetOverallRatingForStrategicAssessment(assessment);

		//    Assert.AreEqual(r.Result, expected);
		//}

		[Test]
		public void Mission_Attack_Canceled_By_Move()
		{
			var unit = TheGame.JTSServices.GenericComponentService.GetByName<Unit>("A_Infantry");				
			var missionType = TheGame.JTSServices.AIService.GetMissionTypes().SingleOrDefault(mot => mot.Name == "LocationAttack");
			var mission = new Mission(missionType, unit.ID);
			var unitTaskType = TheGame.JTSServices.AIService.GetUnitTaskTypes().SingleOrDefault(utt => utt.Name == "Attack");			
			var task = new UnitTask(unitTaskType, mission, new List<TaskExecutionArgument>(), 1);
			mission.AddChildComponent(task);
			mission.SetCurrentTask();

			var r = TheGame.JTSServices.RulesService.MissionCanceledByMove(mission);
			Assert.IsTrue(r.Result);
		}

		[Test]
		public void Mission_Build_Canceled_By_Move()
		{
			var unit = TheGame.JTSServices.GenericComponentService.GetByName<Unit>("A_Infantry");				
			var missionType = TheGame.JTSServices.AIService.GetMissionTypes().SingleOrDefault(mot => mot.Name == "Build");
			var mission = new Mission(missionType, unit.ID);
			var unitTaskType = TheGame.JTSServices.AIService.GetUnitTaskTypes().SingleOrDefault(utt => utt.Name == "BuildInfrastructure");			
			var task = new UnitTask(unitTaskType, mission, new List<TaskExecutionArgument>(), 1);
			mission.AddChildComponent(task);
			mission.SetCurrentTask();

			var r = TheGame.JTSServices.RulesService.MissionCanceledByMove(mission);
			Assert.IsTrue(r.Result);
		}

		[Test]
		public void Mission_BuildWithMove_NOT_Canceled_By_Move()
		{
			var sourceNode = TheGame.JTSServices.NodeService.GetNodeAt(6, 2, 0);
			var targetNode = TheGame.JTSServices.NodeService.GetNodeAt(7, 2, 0);
			var unit = TheGame.JTSServices.GenericComponentService.GetByName<Unit>("A_Infantry");
			var demographic = TheGame.JTSServices.DemographicService.GetDemographics().FirstOrDefault(d => d.DemographicClass.BuildInfo.Buildable);
			
			var buildArgs = new List<TaskExecutionArgument>();

			buildArgs.Add(new TaskExecutionArgument(	unit.GetNode().DefaultTile.GetType().FullName,
														unit.GetNode().DefaultTile.GetType().Assembly.FullName, 
														"tile", 
														new[] { unit.GetNode().DefaultTile.ID.ToString() }));

			buildArgs.Add(new TaskExecutionArgument(	demographic.GetType().FullName,
														demographic.GetType().Assembly.FullName, 
														"demographic", 
														new[] { demographic.ID.ToString() }));

			buildArgs.Add(new TaskExecutionArgument(	"int",
														null, 
														"direction", 
														new[] { ((int)Direction.EAST).ToString() }));
			
			var moveArgs = new List<TaskExecutionArgument>();

			moveArgs.Add(new TaskExecutionArgument(	sourceNode.GetType().FullName,
														sourceNode.GetType().Assembly.FullName, 
														"sourceNode", 
														new[] { sourceNode.DefaultTile.ID.ToString() }));

			moveArgs.Add(new TaskExecutionArgument(	targetNode.GetType().FullName,
														targetNode.GetType().Assembly.FullName, 
														"targetNode", 
														new[] { targetNode.ID.ToString() }));

			var missionType = TheGame.JTSServices.AIService.GetMissionTypes().SingleOrDefault(mot => mot.Name == "Build");
			var mission = new Mission(missionType, unit.ID);
			var moveTaskType = TheGame.JTSServices.AIService.GetUnitTaskTypes().SingleOrDefault(utt => utt.Name == "MoveToLocation");			
			var moveTask = new UnitTask(moveTaskType, mission, moveArgs, 1);
			var buildTaskType = TheGame.JTSServices.AIService.GetUnitTaskTypes().SingleOrDefault(utt => utt.Name == "BuildInfrastructure");			
			var buildTask = new UnitTask(buildTaskType, mission, buildArgs, demographic.DemographicClass.BuildInfo.BuildTurns);
			mission.AddChildComponent(moveTask);
			mission.AddChildComponent(buildTask);
			mission.SortTasks();
			mission.SetCurrentTask();

			var r = TheGame.JTSServices.RulesService.MissionCanceledByMove(mission);
			Assert.IsFalse(r.Result);
		}

		[Test]
		public void Mission_BuildWithMove_MoveComplete_Canceled_By_Move()
		{
			var sourceNode = TheGame.JTSServices.NodeService.GetNodeAt(6, 2, 0);
			var targetNode = TheGame.JTSServices.NodeService.GetNodeAt(7, 2, 0);
			var unit = TheGame.JTSServices.GenericComponentService.GetByName<Unit>("A_Infantry");
			var demographic = TheGame.JTSServices.DemographicService.GetDemographics().FirstOrDefault(d => d.DemographicClass.BuildInfo.Buildable);
			
			var buildArgs = new List<TaskExecutionArgument>();

			buildArgs.Add(new TaskExecutionArgument(	unit.GetNode().DefaultTile.GetType().FullName,
														unit.GetNode().DefaultTile.GetType().Assembly.FullName, 
														"tile", 
														new[] { unit.GetNode().DefaultTile.ID.ToString() }));

			buildArgs.Add(new TaskExecutionArgument(	demographic.GetType().FullName,
														demographic.GetType().Assembly.FullName, 
														"demographic", 
														new[] { demographic.ID.ToString() }));

			buildArgs.Add(new TaskExecutionArgument(	"int",
														null, 
														"direction", 
														new[] { ((int)Direction.EAST).ToString() }));
			
			var moveArgs = new List<TaskExecutionArgument>();

			moveArgs.Add(new TaskExecutionArgument(	sourceNode.GetType().FullName,
														sourceNode.GetType().Assembly.FullName, 
														"sourceNode", 
														new[] { sourceNode.ID.ToString() }));

			moveArgs.Add(new TaskExecutionArgument(	targetNode.GetType().FullName,
														targetNode.GetType().Assembly.FullName, 
														"targetNode", 
														new[] { targetNode.ID.ToString() }));
			
			var missionType = TheGame.JTSServices.AIService.GetMissionTypes().SingleOrDefault(mot => mot.Name == "Build");
			var mission = new Mission(missionType, unit.ID);
			var moveTaskType = TheGame.JTSServices.AIService.GetUnitTaskTypes().SingleOrDefault(utt => utt.Name == "MoveToLocation");			
			var moveTask = new UnitTask(moveTaskType, mission, moveArgs, 1);
			var buildTaskType = TheGame.JTSServices.AIService.GetUnitTaskTypes().SingleOrDefault(utt => utt.Name == "BuildInfrastructure");			
			var buildTask = new UnitTask(buildTaskType, mission, buildArgs, demographic.DemographicClass.BuildInfo.BuildTurns);
			mission.AddChildComponent(moveTask);
			mission.AddChildComponent(buildTask);
			mission.Execute();

			var r = TheGame.JTSServices.RulesService.MissionCanceledByMove(mission);
			Assert.IsTrue(r.Result);
		}
	}
}
