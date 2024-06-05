using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.API;
using JTacticalSim.Component.AI;
using JTacticalSim.Component.World;
using JTacticalSim.Component.GameBoard;
using JTacticalSim.Utility;
using NUnit.Framework;
using JTacticalSim.LINQPad.Plugins;

namespace JTacticalSim.Test.Service
{
	/// <summary>
	/// Tests for the AI Service
	/// </summary>
	[TestFixture]
	public class AIServiceTests : BaseTest
	{
		// Deploy from transports

		[Test]
		public void Deploy_Unit_From_NonWatercraft_Succeeds_Within_Deploy_Distance()
		{
			var destinationNode = TheGame.JTSServices.NodeService.GetNodeAt(2, 1, 0);
			var transport = ScriptTestUnits.HellCats;
			var deployUnits = new List<IUnit>{ScriptTestUnits.C_Infantry};
			var result = TheGame.JTSServices.AIService.DeployUnitsFromTransportToNode(transport, deployUnits, destinationNode);

			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
		}

		[Test]
		public void Deploy_Unit_From_NonWatercraft_Fails_Outside_Deploy_Distance()
		{
			var destinationNode = TheGame.JTSServices.NodeService.GetNodeAt(3, 1, 0);
			var transport = ScriptTestUnits.HellCats;
			var deployUnits = new List<IUnit>{ScriptTestUnits.C_Infantry};
			var result = TheGame.JTSServices.AIService.DeployUnitsFromTransportToNode(transport, deployUnits, destinationNode);

			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
		}

		[Test]
		public void Deploy_Unit_From_NonWatercraft_Fails_DueTo_Incompatible_Node()
		{
			var transport = ScriptTestUnits.ScoutCavA;
		    var destinationNode = TheGame.JTSServices.NodeService.GetNodeAt(0, 3, 0);
			var deployUnits = new List<IUnit>{ScriptTestUnits.LI_298};
	
			var result = TheGame.JTSServices.AIService.DeployUnitsFromTransportToNode(transport, deployUnits, destinationNode);

		    Assert.AreEqual(result.Status, ResultStatus.FAILURE);
		}

		[Test]
		public void Deploy_Unit_From_Watercraft_Fails_DueTo_Incompatible_Node()
		{
		    var destinationNode = TheGame.JTSServices.NodeService.GetNodeAt(1, 1, 0);
			var transport = ScriptTestUnits.Trans_A;
			var deployUnits = new List<IUnit>{ScriptTestUnits.B_Infantry};
	
			var result = TheGame.JTSServices.AIService.DeployUnitsFromTransportToNode(transport, deployUnits, destinationNode);

		    Assert.AreEqual(result.Status, ResultStatus.FAILURE);
		}

		[Test]
		public void Deploy_Unit_From_Watercraft_Succeeds_Within_Deploy_Distance()
		{
			var destinationNode = TheGame.JTSServices.NodeService.GetNodeAt(0, 1, 0);
			var transport = ScriptTestUnits.Trans_A;
			var deployUnits = new List<IUnit>{ScriptTestUnits.B_Infantry};
			var result = TheGame.JTSServices.AIService.DeployUnitsFromTransportToNode(transport, deployUnits, destinationNode);

			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
		}

		[Test]
		public void Deploy_Unit_From_Watercraft_Fails_Outside_Deploy_Distance()
		{
			var destinationNode = TheGame.JTSServices.NodeService.GetNodeAt(3, 0, 0);
			var transport = ScriptTestUnits.Trans_A;
			var deployUnits = new List<IUnit>{ScriptTestUnits.B_Infantry};
			var result = TheGame.JTSServices.AIService.DeployUnitsFromTransportToNode(transport, deployUnits, destinationNode);

			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
		}


		// Load To transports

		[Test]
		public void Load_Unit_To_NonWatercraft_Succeeds_Within_Load_Distance()
		{
			var transport = ScriptTestUnits.LOADTO_HELO;
			var loadUnits = new List<IUnit>{ScriptTestUnits.LOADME_LINFANTRY};
	
			var result = TheGame.JTSServices.AIService.LoadUnitsToTransport(transport, loadUnits);

			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
		}

		[Test]
		public void Load_Unit_To_NonWatercraft_Fails_Outside_Load_Distance()
		{	
			var transport = ScriptTestUnits.LOADTO_HELO;
			var loadUnits = new List<IUnit>{ScriptTestUnits.NinthMedical};
	
			var result = TheGame.JTSServices.AIService.LoadUnitsToTransport(transport, loadUnits);

			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
		}

		[Test]
		public void Load_Unit_To_Watercraft_Succeeds_Within_Load_Distance()
		{	
			var transport = ScriptTestUnits.LOADTO_TRANSPORT;
			var loadUnits = new List<IUnit>{ScriptTestUnits.LOADME_HARMOR};
	
			var result = TheGame.JTSServices.AIService.LoadUnitsToTransport(transport, loadUnits);

			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
		}

		[Test]
		public void Load_Unit_To_Watercraft_Fails_Outside_Load_Distance()
		{
			var transport = ScriptTestUnits.LOADTO_TRANSPORT;
			var loadUnits = new List<IUnit>{ScriptTestUnits.LI_298};	
			var result = TheGame.JTSServices.AIService.LoadUnitsToTransport(transport, loadUnits);

			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
		}

		[Test]
		public void Load_Unit_To_Transport_Fails_DueTo_Weight()
		{	
			var transport = ScriptTestUnits.LOADTO_HELO;
			var loadUnits = new List<IUnit>{ScriptTestUnits.LOADME_LINFANTRY, ScriptTestUnits.LOADME_MINFANTRY};
	
			var result = TheGame.JTSServices.AIService.LoadUnitsToTransport(transport, loadUnits);

			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
		}

		[Test]
		public void Load_Units_To_Transport_Succeeds_For_Compatible_Unit_Only()
		{	
			var transport = ScriptTestUnits.LOADTO_HELO;
			var loadUnits = new List<IUnit>{ScriptTestUnits.LOADME_HARMOR, ScriptTestUnits.LOADME_LINFANTRY, ScriptTestUnits.LOADME_FIGHTER};
	
			var result = TheGame.JTSServices.AIService.LoadUnitsToTransport(transport, loadUnits);

			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
			Assert.AreEqual(result.SuccessfulObjects.Count, 1);
			Assert.AreEqual(result.SuccessfulObjects.Single().Name, "LOADME_LINFANTRY");
		}


		// Attach Units
		
		[Test]
		public void Unit_Attach_To_Unit_Fails_ParentToHighInChain()
		{
			var unit = ScriptTestUnits.BigGuns;		// Platoon
			var parent = ScriptTestUnits.Btl101;	// Battalion
			var result = TheGame.JTSServices.AIService.AttachUnitToUnit(parent, unit);

			Assert.AreEqual(result.Status, ResultStatus.FAILURE);	
		}

		[Test]
		public void Unit_Attach_To_Unit_Fails_ParentToLowInChain()
		{
			var unit = ScriptTestUnits.D_Infantry;		// Company
			var parent = ScriptTestUnits.BigGuns;		// Platoon
			var result = TheGame.JTSServices.AIService.AttachUnitToUnit(parent, unit);

			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
		}

		[Test]
		public void Unit_Attach_To_Unit_Fails_UnitAlreadyAttached()
		{
			var unit = ScriptTestUnits.C_Tank;
			var parent = ScriptTestUnits.Btl101;	
			var result = TheGame.JTSServices.AIService.AttachUnitToUnit(parent, unit);

			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
		}

		[Test]
		public void Unit_Attach_To_Unit_Succeeds()
		{
			var unit = ScriptTestUnits.BigGuns;
			var parent = ScriptTestUnits.D_Infantry;	
			var result = TheGame.JTSServices.AIService.AttachUnitToUnit(parent, unit);

			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
			Assert.IsTrue(parent.GetAllAttachedUnits().Contains(unit));
		}

		// Detach Units

		[Test]
		public void Unit_Detach_From_Unit_Fails_NotAttached()
		{
			var unit = ScriptTestUnits.BigGuns;	
			var result = TheGame.JTSServices.AIService.DetachUnitFromUnit(unit);

			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
		}

		[Test]
		public void Unit_Detach_From_Unit_Succeeds()
		{
			var unit = ScriptTestUnits.C_Infantry;
			var parent = unit.AttachedToUnit;
			var result = TheGame.JTSServices.AIService.DetachUnitFromUnit(unit);

			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
			Assert.IsFalse(parent.GetAllAttachedUnits().Contains(unit));
		}


		// Path Finding

		[TestCase("0", "0", "0")]
		[TestCase("0", "1", "0")]
		[TestCase("0", "2", "0")]
		[TestCase("2", "0", "0")]
		[TestCase("3", "0", "0")]
		public void Find_Path_Succeeds_Within_Move_Radius(params string[] args)
		{
			var spec = new PathSpec(new Coordinate(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2])), TheGame);
			var result = TheGame.JTSServices.AIService.FindPath(spec.SourceNode, spec.TargetNode, spec.NodeMap, spec.Unit);
			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
		}

		[TestCase("1", "1", "0")]
		public void Find_Path_Fails_For_Incompatible_Nodes(params string[] args)
		{
			var spec = new PathSpec(new Coordinate(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2])), TheGame);
			var result = TheGame.JTSServices.AIService.FindPath(spec.SourceNode, spec.TargetNode, spec.NodeMap, spec.Unit);
			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
		}

		[TestCase("5", "0", "0")]
		public void Find_Path_Fails_Outside_Move_Radius(params string[] args)
		{
			var spec = new PathSpec(new Coordinate(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2])), TheGame);
			var result = TheGame.JTSServices.AIService.FindPath(spec.SourceNode, spec.TargetNode, spec.NodeMap, spec.Unit);
			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
		}

		[TestCase("6", "4")]
		public void Node_Count_To_Node_Succeeds_Within_Max_Distance(params string[] args)
		{
			var sourceNode = TheGame.JTSServices.NodeService.GetNodeAt(4, 2, 0);
			var targetNode = TheGame.JTSServices.NodeService.GetNodeAt(8, 2, 0);
			var result = TheGame.JTSServices.AIService.CalculateNodeCountToNode(sourceNode, targetNode, Convert.ToInt32(args[0]));
			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
			Assert.AreEqual(result.Result, Convert.ToInt32(args[1]));
		}

		[TestCase("3", null)]
		public void Node_Count_To_Node_Fails_Outside_Max_Distance(params string[] args)
		{
			var sourceNode = TheGame.JTSServices.NodeService.GetNodeAt(4, 2, 0);
			var targetNode = TheGame.JTSServices.NodeService.GetNodeAt(8, 2, 0);
			var result = TheGame.JTSServices.AIService.CalculateNodeCountToNode(sourceNode, targetNode, Convert.ToInt32(args[0]));
			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
			Assert.AreEqual(result.Result, args[1]);
		}


		// Battle

		[Test]
		public void Get_Prime_Target_Selects_Highest_UnitTargetDesirabilityFactor_LOCAL()
		{
			var UTDF_A = ScriptTestUnits.LOADME_LINFANTRY.GetUnitTargetDesirabilityFactor();
			var UTDF_B = ScriptTestUnits.LOADME_MINFANTRY.GetUnitTargetDesirabilityFactor();
			var UTDF_C = ScriptTestUnits.HellCats.GetUnitTargetDesirabilityFactor();

			Assert.IsTrue(UTDF_C > UTDF_B);
			Assert.IsTrue(UTDF_B > UTDF_A);

			var attacker = ScriptTestUnits.C_Tank;
			var candidates = new List<IUnit>{ScriptTestUnits.LOADME_LINFANTRY, ScriptTestUnits.LOADME_MINFANTRY, ScriptTestUnits.HellCats};
			var result = TheGame.JTSServices.AIService.GetPrimeUnitTargetForUnit(candidates, attacker, BattleType.LOCAL);

			Assert.AreEqual(result.Result.GetUnitTargetDesirabilityFactor(), UTDF_C);
		}

		[Test]
		public void Get_Prime_Target_Selects_RemoteFireCapable_First_BARRAGE()
		{
			var UTDF_A = ScriptTestUnits.LOADME_LINFANTRY.GetUnitTargetDesirabilityFactor();
			var UTDF_B = ScriptTestUnits.LOADME_MINFANTRY.GetUnitTargetDesirabilityFactor();
			var UTDF_C = ScriptTestUnits.HellCats.GetUnitTargetDesirabilityFactor();
			var UTDF_D = ScriptTestUnits.BigGuns.GetUnitTargetDesirabilityFactor();

			Assert.IsTrue(UTDF_C > UTDF_B);
			Assert.IsTrue(UTDF_B > UTDF_A); 

			var attacker = ScriptTestUnits.Sandy_MLRS;
			var candidates = new List<IUnit>{ScriptTestUnits.LOADME_LINFANTRY, 
											ScriptTestUnits.LOADME_MINFANTRY, 
											ScriptTestUnits.BigGuns,
											ScriptTestUnits.HellCats};
			var result = TheGame.JTSServices.AIService.GetPrimeUnitTargetForUnit(candidates, attacker, BattleType.BARRAGE);

			Assert.AreEqual(result.Result.GetUnitTargetDesirabilityFactor(), UTDF_D);
		}

		[Test]
		public void Create_Full_Combat_Skirmish_Uses_All_Possible_Units_LOCAL()
		{
			var Attackers = new List<IUnit>();
			var Defenders = new List<IUnit>();
	
			Attackers.Add(ScriptTestUnits.HellCats);
			Attackers.Add(ScriptTestUnits.A_Tank);
			Attackers.Add(ScriptTestUnits.MI_HQ);	// HQ should NOT be able to attack based on rules
			Attackers.Add(ScriptTestUnits.ScoutCavA);
	
			Defenders.Add(ScriptTestUnits.FighterSquad_A);
			Defenders.Add(ScriptTestUnits.Sam_Site);
			Defenders.Add(ScriptTestUnits.Fadaykin_SF);
			Defenders.Add(ScriptTestUnits.A_SandyTanks);
	
			var battle = TheGame.JTSServices.GameService.CreateNewBattle(Attackers, Defenders, BattleType.LOCAL).Result;
			ScriptTestUnits.Sam_Site_A.GetNode().Select();
	
			var result = TheGame.JTSServices.AIService.CreateFullCombatSkirmishes(battle);

			Assert.IsTrue(result.Result.Count == 3);
		}

		[Test]
		public void Create_Full_Combat_Skirmish_Uses_Compatible_Defenders_Only_LOCAL()
		{
			var Attackers = new List<IUnit>();
			var Defenders = new List<IUnit>();
	
			Attackers.Add(ScriptTestUnits.HellCats);
			Attackers.Add(ScriptTestUnits.A_Tank);
			Attackers.Add(ScriptTestUnits.ScoutCavA);
	
			Defenders.Add(ScriptTestUnits.FighterSquad_A);
			Defenders.Add(ScriptTestUnits.Nuc_Sub_A);
			Defenders.Add(ScriptTestUnits.Fadaykin_SF);
			Defenders.Add(ScriptTestUnits.A_SandyTanks);

			var battle = TheGame.JTSServices.GameService.CreateNewBattle(Attackers, Defenders, BattleType.LOCAL).Result;
			ScriptTestUnits.Sam_Site_A.GetNode().Select();
	
			var result = TheGame.JTSServices.AIService.CreateFullCombatSkirmishes(battle);

			Assert.IsTrue(result.Result.Count == 3);

			var usedUnits = result.Result.Select(s => s.Attacker).Concat(result.Result.Select(s => s.Defender));

			// A Sub would not be a compatible defender
			Assert.AreEqual(usedUnits.Count(), 6);
			Assert.IsTrue(usedUnits.All(u => u.Name != "Nuc_Sub_A"));
		}

		[Test]
		public void Create_Full_Combat_Skirmish_Uses_RemoteFireCapableAttackers_Only_BARRAGE()
		{
			var Attackers = new List<IUnit>();
			var Defenders = new List<IUnit>();
	
			Attackers.Add(ScriptTestUnits.HellCats);
			Attackers.Add(ScriptTestUnits.BigGuns);
			Attackers.Add(ScriptTestUnits.ScoutCavA);
	
			Defenders.Add(ScriptTestUnits.FighterSquad_A);
			Defenders.Add(ScriptTestUnits.Nuc_Sub_A);
			Defenders.Add(ScriptTestUnits.Fadaykin_SF);

			var battle = TheGame.JTSServices.GameService.CreateNewBattle(Attackers, Defenders, BattleType.BARRAGE).Result;
			ScriptTestUnits.Sam_Site_A.GetNode().Select();
	
			var result = TheGame.JTSServices.AIService.CreateFullCombatSkirmishes(battle);

			Assert.IsTrue(result.Result.Count == 1);
	
			var usedUnits = result.Result.Select(s => s.Attacker).Concat(result.Result.Select(s => s.Defender));

			// Not remote fire capable
			Assert.IsTrue(usedUnits.All(u => u.Name != "HellCats"));
			Assert.IsTrue(usedUnits.All(u => u.Name != "ScoutCavA"));
		}

		[Test]
		public void Create_Full_Combat_Skirmish_Uses_Compatible_Units_Only_BARRAGE()
		{
			var Attackers = new List<IUnit>();
			var Defenders = new List<IUnit>();
	
			Attackers.Add(ScriptTestUnits.HellCats);
			Attackers.Add(ScriptTestUnits.BigGuns);
			Attackers.Add(ScriptTestUnits.ScoutCavA);
	
			// Artillery effective against land and water surface units only
			Defenders.Add(ScriptTestUnits.FighterSquad_A);
			Defenders.Add(ScriptTestUnits.Nuc_Sub_A);

			var battle = TheGame.JTSServices.GameService.CreateNewBattle(Attackers, Defenders, BattleType.BARRAGE).Result;
			ScriptTestUnits.Sam_Site_A.GetNode().Select();
	
			var result = TheGame.JTSServices.AIService.CreateFullCombatSkirmishes(battle);
	
			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
		}

		[Test]
		public void Create_AirDefence_Combat_Skirmish_Uses_Compatible_Units_Only()
		{
			var Attackers = new List<IUnit>();
			var Defenders = new List<IUnit>();
	
			Attackers.Add(ScriptTestUnits.HellCats);
			Attackers.Add(ScriptTestUnits.A_Tank);
			Attackers.Add(ScriptTestUnits.ScoutCavA);

			Defenders.Add(ScriptTestUnits.FighterSquad_A);
			Defenders.Add(ScriptTestUnits.Nuc_Sub_A);
			Defenders.Add(ScriptTestUnits.Fadaykin_SF);
			Defenders.Add(ScriptTestUnits.Sam_Site);

			var battle = TheGame.JTSServices.GameService.CreateNewBattle(Attackers, Defenders, BattleType.LOCAL).Result;
			ScriptTestUnits.Sam_Site_A.GetNode().Select();
	
			var result = TheGame.JTSServices.AIService.CreateAirDefenceSkirmishes(battle);

			Assert.IsTrue(result.Result.Count == 1);
	
			var usedUnits = result.Result.Select(s => s.Attacker).Concat(result.Result.Select(s => s.Defender));

			Assert.IsTrue(usedUnits.Any(u => u.Name == "HellCats") && usedUnits.Any(u => u.Name == "sam_site"));
		}

		[Test]
		public void Create_AirDefence_Combat_Skirmish_Uses_All_Possible_Units()
		{
			var Attackers = new List<IUnit>();
			var Defenders = new List<IUnit>();
	
			Attackers.Add(ScriptTestUnits.HellCats);
			Attackers.Add(ScriptTestUnits.A_Tank);
			Attackers.Add(ScriptTestUnits.ScoutCavA);

			Defenders.Add(ScriptTestUnits.FighterSquad_A);
			Defenders.Add(ScriptTestUnits.Nuc_Sub_A);
			Defenders.Add(ScriptTestUnits.Sam_Site_A);
			Defenders.Add(ScriptTestUnits.Sam_Site);

			var battle = TheGame.JTSServices.GameService.CreateNewBattle(Attackers, Defenders, BattleType.LOCAL).Result;
			ScriptTestUnits.Sam_Site_A.GetNode().Select();
	
			var result = TheGame.JTSServices.AIService.CreateAirDefenceSkirmishes(battle);

			Assert.IsTrue(result.Result.Count == 2);
		}


		// Other

		[Test]
		public void Claim_Node_For_Victor_Faction_Succeeds_With_Appropriate_Unit_UNOCCUPIED()
		{
			var nodeToClaim = TheGame.JTSServices.NodeService.GetNodeAt(2, 2, 0); // shorline east/Iran
			var units = new List<IUnit>();
	
			units.Add(ScriptTestUnits.B_Tank);
	
			var result = TheGame.JTSServices.AIService.ClaimNodeForVictorFaction(units, nodeToClaim);	

			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
		}

		[Test]
		public void Claim_Node_For_Victor_Faction_Fails_With_InAppropriate_Unit_UNOCCUPIED()
		{
			var nodeToClaim = TheGame.JTSServices.NodeService.GetNodeAt(2, 2, 0); // shorline east/Iran
			var units = new List<IUnit>();
	
			units.Add(ScriptTestUnits.ScoutCavA);
	
			var result = TheGame.JTSServices.AIService.ClaimNodeForVictorFaction(units, nodeToClaim);	

			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
		}

		[Test]
		public void Claim_Node_For_Victor_Faction_Fails_With_Appropriate_Unit_ENEMYOCCUPIED()
		{
			var nodeToClaim = TheGame.JTSServices.NodeService.GetNodeAt(2, 4, 0); // land/Iran
			var units = new List<IUnit>();
	
			units.Add(ScriptTestUnits.A_Infantry);
	
			var result = TheGame.JTSServices.AIService.ClaimNodeForVictorFaction(units, nodeToClaim);
			Assert.AreEqual(result.Status, ResultStatus.FAILURE);
		}


		[Test]
		public void Cancel_Mission_Removes_Tactic_With_Single_Mission()
		{
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

			var missionType = TheGame.JTSServices.AIService.GetMissionTypes().SingleOrDefault(mot => mot.Name == "Build");
			var mission = new Mission(missionType, unit.ID);
			var buildTaskType = TheGame.JTSServices.AIService.GetUnitTaskTypes().SingleOrDefault(utt => utt.Name == "BuildInfrastructure");			
			var buildTask = new UnitTask(buildTaskType, mission, buildArgs, demographic.DemographicClass.BuildInfo.BuildTurns);
			mission.AddChildComponent(buildTask);
			TheGame.StrategyHandler.SetUpUnitMission(mission, buildTask, StrategicalStance.ADMINISTRATIVE, TheGame.CurrentTurn.Player);

			Assert.IsNotNull(unit.GetCurrentMission());

			var r = TheGame.JTSServices.AIService.CancelMission(mission);

			Assert.IsNull(unit.GetCurrentMission());
			Assert.IsFalse(TheGame.JTSServices.AIService.GetTactics().Any());
		}

		// Fuel consumption on turn end

		[TestCase(100)]		//not enough fuel
		public void Handle_Special_TurnEnd_Unit_Management_Bomber_Removes(int fuel)
		{
			// Cause to run out of fuel
			var unit = ScriptTestUnits.LOADME_BOMBER;
			unit.CurrentFuelRange = fuel;
			var result = TheGame.JTSServices.AIService.HandleSpecialTurnEndUnitManagement();
			var unitResult = ScriptTestUnits.LOADME_BOMBER;

			// Bomber is removed
			Assert.IsTrue(result.Status == ResultStatus.SUCCESS);
			Assert.IsNull(unitResult);

		}

		[TestCase(1000)]	//enough fuel
		public void Handle_Special_TurnEnd_Unit_Management_Bomber_NotRemoves(int fuel)
		{
			var unit = ScriptTestUnits.LOADME_BOMBER;
			unit.CurrentFuelRange = fuel;
			var result = TheGame.JTSServices.AIService.HandleSpecialTurnEndUnitManagement();
			var unitResult = ScriptTestUnits.LOADME_BOMBER;

			// Bomber is removed
			Assert.IsTrue(result.Status == ResultStatus.SUCCESS);
			Assert.NotNull(unitResult);

		}

		[TestCase(100)]		//not enough fuel
		public void Handle_Special_TurnEnd_Unit_Management_Fighter_Removes(int fuel)
		{
			// Cause to run out of fuel
			var unit = ScriptTestUnits.LOADME_FIGHTER;
			unit.CurrentFuelRange = fuel;
			var result = TheGame.JTSServices.AIService.HandleSpecialTurnEndUnitManagement();
			var unitResult = ScriptTestUnits.LOADME_FIGHTER;

			// Bomber is removed
			Assert.IsTrue(result.Status == ResultStatus.SUCCESS);
			Assert.IsNull(unitResult);

		}

		[TestCase(1000)]	//enough fuel
		public void Handle_Special_TurnEnd_Unit_Management_Fighter_NotRemoves(int fuel)
		{
			var unit = ScriptTestUnits.LOADME_FIGHTER;
			unit.CurrentFuelRange = fuel;
			var result = TheGame.JTSServices.AIService.HandleSpecialTurnEndUnitManagement();
			var unitResult = ScriptTestUnits.LOADME_FIGHTER;

			// Bomber is removed
			Assert.IsTrue(result.Status == ResultStatus.SUCCESS);
			Assert.NotNull(unitResult);

		}

		[TestCase(1000)]	//enough fuel
		[TestCase(1000)]	//not enough fuel
		public void Handle_Special_TurnEnd_Unit_Management_Helicopter_NotRemoves(int fuel)
		{
			var unit = ScriptTestUnits.LOADTO_HELO;
			unit.CurrentFuelRange = fuel;
			var result = TheGame.JTSServices.AIService.HandleSpecialTurnEndUnitManagement();
			var unitResult = ScriptTestUnits.LOADTO_HELO;

			// Bomber is removed
			Assert.IsTrue(result.Status == ResultStatus.SUCCESS);
			Assert.NotNull(unitResult);

		}
	}
}
