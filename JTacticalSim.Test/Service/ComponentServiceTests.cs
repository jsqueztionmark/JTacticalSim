using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.Utility;
using JTacticalSim.API;
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
	/// Tests for Component Service
	/// </summary>
	[TestFixture]
	public class ComponentServiceTests : BaseTest
	{
		[Test]
		public void Get_Game_Victory_Conditions_By_Faction_IRAN()
		{
			var f = TheGame.JTSServices.GenericComponentService.GetByName<Faction>("IRAN");
			var result = TheGame.JTSServices.GameService.GetVictoryConditionsByFaction(f).Select(vc => vc.ConditionType).ToList();

			Assert.Contains(GameVictoryCondition.ENEMY_UNITS_REMAINING, result);
			Assert.Contains(GameVictoryCondition.VICTORY_POINTS_HELD, result);
			Assert.AreEqual(result.Count, 2);
		}

		[Test]
		public void Get_Game_Victory_Conditions_By_Faction_USA()
		{
			var f = TheGame.JTSServices.GenericComponentService.GetByName<Faction>("USA");
			var result = TheGame.JTSServices.GameService.GetVictoryConditionsByFaction(f).Select(vc => vc.ConditionType).ToList();

			Assert.Contains(GameVictoryCondition.ENEMY_UNITS_REMAINING, result);
			Assert.AreEqual(result.Count, 1);
		}

		[Test]
		public void Get_Allowable_Neighbor_Nodes_For_Grid()
		{
			var unit = ScriptTestUnits.NinthMedical;
			var map = unit.GetAllowableMovements();
	
			var result = TheGame.JTSServices.NodeService.GetAllowableNeighborNodesForGrid(unit, unit.GetNode(), map);

			var expect = new List<ICoordinate>
				{
					new Coordinate(0, 1, 0),
					new Coordinate(1, 2, 0)
				};

			var coordinates = result.Select(n => n.Item1.Location);
	
			Assert.AreEqual(result.Count(), 2);
			Assert.IsTrue(coordinates.All(c => expect.Contains(c)));
		}

		// Not currently using extended nodes.
		[TestCase(false, false, 14)]
		[TestCase(true, false, 15)]
		public void Get_All_Nodes_Within_Max_Distance(bool includeCurrent, bool includeExtended, int resultCount)
		{
			var unit = ScriptTestUnits.NinthMedical;
			var result = TheGame.JTSServices.NodeService.GetAllNodesWithinMaxDistance(unit, includeCurrent, includeExtended);
	
			Assert.AreEqual(result.Count(), resultCount);
		}


		[Test]
		public void Get_Allowable_Movement_Nodes_For_Unit()
		{
			var unit = ScriptTestUnits.A_Tank;
			var result = TheGame.JTSServices.NodeService.GetAllowableMovementNodesForUnit(unit);

			var expect = new List<ICoordinate>
				{
					new Coordinate(0, 0, 0),
					new Coordinate(1, 0, 0),
					new Coordinate(2, 0, 0),
					new Coordinate(3, 0, 0),
					new Coordinate(0, 1, 0),
					new Coordinate(2, 1, 0),
					new Coordinate(0, 2, 0),
					new Coordinate(3, 1, 0),
					new Coordinate(1, 2, 0),
					new Coordinate(4, 0, 0),
					new Coordinate(2, 2, 0),
					new Coordinate(3, 2, 0),
					new Coordinate(4, 1, 0),
					new Coordinate(4, 2, 0)
				};

			var coordinates = result.Select(n => n.Location);

			Assert.AreEqual(result.Count, 14);
			Assert.IsTrue(coordinates.All(c => expect.Contains(c)));
		}

		[TestCase(1, 5)]
		[TestCase(2, 6)]
		public void Get_All_Nodes_At_Distance(int distance, int resultCount)
		{
			var unit = ScriptTestUnits.A_Tank;
			var result = TheGame.JTSServices.NodeService.GetAllNodesAtDistance(unit.GetNode(), distance, false);

			Assert.AreEqual(result.Count(), resultCount);
		}

		// All eight directions and self
		[TestCase(0, 0, Direction.NORTHWEST)]
		[TestCase(1, 0, Direction.NORTH)]
		[TestCase(2, 0, Direction.NORTHEAST)]
		[TestCase(2, 1, Direction.EAST)]
		[TestCase(2, 2, Direction.SOUTHEAST)]
		[TestCase(1, 2, Direction.SOUTH)]
		[TestCase(0, 2, Direction.SOUTHWEST)]
		[TestCase(0, 1, Direction.WEST)]
		[TestCase(1, 1, Direction.NONE)]
		public void Get_Node_Direction_From_SourceNode_NODEISNEIGHBOR(int targetX, int targetY, Direction resultDirection)
		{
			var sourceNode = TheGame.JTSServices.NodeService.GetNodeAt(1, 1, 0);
			var targetNode = TheGame.JTSServices.NodeService.GetNodeAt(targetX, targetY, 0);

			var result = TheGame.JTSServices.NodeService.GetNodeDirectionFromNeighborSourceNode(targetNode, sourceNode);

			Assert.AreEqual(result, resultDirection);
		}

		[Test]
		public void Get_Node_Direction_From_SourceNode_NODEISNOTNEIGHBOR()
		{
			var sourceNode = TheGame.JTSServices.NodeService.GetNodeAt(1, 1, 0);
			var targetNode = TheGame.JTSServices.NodeService.GetNodeAt(3, 0, 0);

			var result = TheGame.JTSServices.NodeService.GetNodeDirectionFromNeighborSourceNode(targetNode, sourceNode);

			Assert.AreEqual(result, Direction.NONE);
		}

		[Test]
		public void Get_Units_By_Unit_Assignment()
		{
			var unit = ScriptTestUnits.Btl101;
			var result = TheGame.JTSServices.UnitService.GetUnitsByUnitAssignment(unit.ID);

			Assert.AreEqual(result.Count, 5);
		}

		[Test]
		public void Get_Units_By_Unit_Assignment_Recursive()
		{
			var unit = ScriptTestUnits.Btl101;
			var result = TheGame.JTSServices.UnitService.GetUnitsByUnitAssignmentRecursive(unit.ID);

			Assert.AreEqual(result.Count, 6);
		}

		[Test]
		public void Get_Units_By_Transport()
		{
			var unit = ScriptTestUnits.HellCats;
			var transported = ScriptTestUnits.C_Infantry;
			var result = TheGame.JTSServices.UnitService.GetUnitsByTransport(unit.ID);

			Assert.AreEqual(result.Count, 1);
			Assert.IsTrue(result.Contains(transported));

		}

		[Test]
		public void Get_Unit_Assigned_To_Unit()
		{
			var unit = ScriptTestUnits.C_Infantry;
			var assignedTo = ScriptTestUnits.A_Infantry;
			var result = TheGame.JTSServices.UnitService.GetUnitAssignedToUnit(unit.ID);
	
			Assert.IsNotNull(result);
			Assert.AreEqual(result, assignedTo);
		}

		[Test]
		public void Get_Allowable_Unit_Classes_For_Unit()
		{
			var unit = ScriptTestUnits.C_Infantry;
			var result = TheGame.JTSServices.UnitService.GetAllowableUnitClassesForUnit(unit);

			var expected = new List<IUnitClass>
				{
					TheGame.JTSServices.GenericComponentService.GetByName<UnitClass>("Standard"),
					TheGame.JTSServices.GenericComponentService.GetByName<UnitClass>("Supply"),
					TheGame.JTSServices.GenericComponentService.GetByName<UnitClass>("Engineer"),
					TheGame.JTSServices.GenericComponentService.GetByName<UnitClass>("Medical"),
					TheGame.JTSServices.GenericComponentService.GetByName<UnitClass>("SpecialForces"),
					TheGame.JTSServices.GenericComponentService.GetByName<UnitClass>("Defence"),
					TheGame.JTSServices.GenericComponentService.GetByName<UnitClass>("Airborne"),
					TheGame.JTSServices.GenericComponentService.GetByName<UnitClass>("HQ"),
					TheGame.JTSServices.GenericComponentService.GetByName<UnitClass>("Scout"),
					TheGame.JTSServices.GenericComponentService.GetByName<UnitClass>("Sniper"),
				};

			Assert.AreEqual(result.Result.Count(), 10);
			Assert.IsTrue(result.Result.All(uc => expected.Contains(uc)));
		}

		[TestCase("Platoon", "Company")]
		[TestCase("Company", "Battalion")]
		[TestCase("Battalion", "Brigade")]
		public void Get_Next_Highest_Unit_Group_Type_Succeeds(string groupTypeName, string resultTypeName)
		{
			var ugt = TheGame.JTSServices.UnitService.GetUnitGroupTypeByName(groupTypeName);
			var result = TheGame.JTSServices.UnitService.GetNextHighestUnitGroupType(ugt);

			Assert.AreEqual(result.Name, resultTypeName);
		}

		[Test]
		public void Get_Next_Highest_Unit_Group_Type_IS_NULL_FOR_HIGHEST()
		{
			var ugt = TheGame.JTSServices.UnitService.GetUnitGroupTypeByName("Brigade");
			var result = TheGame.JTSServices.UnitService.GetNextHighestUnitGroupType(ugt);

			Assert.IsNull(result);
		}

		[Test]
		public void Get_Unit_Tasks_For_Mission_GETS_ALL_TASKS()
		{
			var missionType = TheGame.JTSServices.GenericComponentService.GetByName<MissionType>("AirbornAttack");
			var result = TheGame.JTSServices.DataService.LookupUnitTaskTypesForMissionType(missionType);

			var expected = new List<int>{0, 2, 3, 4};

			Assert.AreEqual(result.Result.Count(), 4);
			Assert.IsTrue(result.Result.All(r => expected.Contains(r.ID)));
		}

		[Test]
		public void Mission_Unit_Tasks_SORT_BY_SORTORDER()
		{
			var missionType = TheGame.JTSServices.GenericComponentService.GetByName<MissionType>("AirbornAttack");
			var result = TheGame.JTSServices.DataService.LookupUnitTaskTypesForMissionType(missionType);
			var unit = ScriptTestUnits.A_Infantry;

			// build the strategy components
			var tactic = new Tactic(StrategicalStance.OFFENSIVE, TheGame.GetPlayers().First());
			var mission = new Mission(missionType, unit.ID);			
			result.Result.ForEach(t =>
				{
					var task = new UnitTask(t, mission, null, 1);
					mission.AddChildComponent(task);
				});

			tactic.AddChildComponent(mission);

			TheGame.JTSServices.AIService.SaveTactic(tactic);

			mission.SortTasks();
			var tasks = mission.GetChildComponents();

			var expected = new List<int> { 3, 2, 4, 0 };

			for (var i = 0; i <= result.Result.Count - 1; i++)
			{
				Assert.AreEqual(tasks[i].TaskType.ID, expected[i]);
			}
		}

		[Test]
		public void Add_Reinforcement_Unit_To_Player()
		{
			var player = TheGame.GetPlayers().First();
			Assert.IsTrue(!player.UnplacedReinforcements.Any());
			
			var unit = ScriptTestUnits.Btl101;
			var result = TheGame.JTSServices.UnitService.SaveReinforcementUnitToPlayer(player, unit);

			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
			Assert.IsTrue(player.UnplacedReinforcements.Any());
		}

		[Test]
		public void Remove_Reinforcement_Unit_From_Player()
		{
			var player = TheGame.GetPlayers().First();
			var unit = ScriptTestUnits.Btl101;
			var result = TheGame.JTSServices.UnitService.SaveReinforcementUnitToPlayer(player, unit);

			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
			Assert.IsTrue(player.UnplacedReinforcements.Any());

			var removeResult = TheGame.JTSServices.UnitService.RemoveReinforcementUnitFromPlayer(player, unit);

			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
			Assert.IsTrue(!player.UnplacedReinforcements.Any());

		}

		[Test]
		public void TestGenericTemp()
		{
			var unit = ScriptTestUnits.Btl101;

			var typeArgument = unit.GetType();

			var method = TheGame.JTSServices.GenericComponentService.GetType().GetMethod("GetByID").MakeGenericMethod(typeArgument);
			var obj = method.Invoke(TheGame.JTSServices.GenericComponentService, new object[] {3}); 
		}

	}

	sealed class Generic<T>
	{
		public Generic() { }
	}
}
