using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.API;
using JTacticalSim.API.Service;
using JTacticalSim.Component.Data;
using JTacticalSim.Component.GameBoard;
using JTacticalSim.Component.AI;
using JTacticalSim.Component.Game;
using JTacticalSim.Component.World;
using JTacticalSim.API.InfoObjects;
using NUnit.Framework;
using JTacticalSim.LINQPad.Plugins;
using Moq;

namespace JTacticalSim.Test.Component
{
	/// <summary>
	/// Each test should verify Create, Update, Read, and Delete of the component
	/// for context and cache when a cached type
	/// </summary>
	[TestFixture]
	public class CRUDTests : BaseTest
	{
		// NOTE: TODO:
		// Currently, demographics are static. If we decide to make the changing of demographics a part of the game,
		// we'll need to add that integration testing here.

		[Test]
		public void Unit_CRUD()
		{
		// CREATE/SAVE

			var location = new Coordinate(1, 0, 0);
			var country = TheGame.JTSServices.GenericComponentService.GetByName<Country>("USA");
			var unitType = TheGame.JTSServices.UnitService.GetUnitTypeByName("LightArmor");
			var unitClass = TheGame.JTSServices.UnitService.GetUnitClassByName("Attack");
			var unitGroupType = TheGame.JTSServices.UnitService.GetUnitGroupTypeByName("Company");
			var unitInfo = new UnitInfo(unitType, unitClass, unitGroupType);

			var result = TheGame.JTSServices.UnitService.CreateUnit("testUnit", location, country, unitInfo);
			var newUnit = result.Result;
			newUnit.SetNextID();

			// Component was created with all attributes
			Verify_CRUD_Context_Operation<IUnit>(result, newUnit);
			Assert.AreSame(newUnit, result.SuccessfulObjects.First());
			Assert.AreSame(newUnit.Location, location);
			Assert.AreSame(newUnit.Country, country);
			Assert.AreSame(newUnit.UnitInfo, unitInfo);

			// Component is saved into context correctly
			var saveResult = TheGame.JTSServices.UnitService.SaveUnits(new List<IUnit> { newUnit });
			Verify_CRUD_Context_Operation<IUnit>(saveResult, newUnit);

			// Component is saved into cache correctly
			var cacheUnit = TheGame.Cache.UnitCache.TryFind(saveResult.SuccessfulObjects.First().UID);
			Assert.AreEqual(cacheUnit, newUnit);

		// UPDATE

			var newLocation = new Coordinate(2, 2, 0);
			var newUnitType = TheGame.JTSServices.UnitService.GetUnitTypeByName("HeavyArmor");
			var newUnitClass = TheGame.JTSServices.UnitService.GetUnitClassByName("Standard");
			var newUnitGroupType = TheGame.JTSServices.UnitService.GetUnitGroupTypeByName("Platoon");
			newUnit.Name = "newTestUnit";
			newUnit.Location = newLocation;
			newUnit.UnitInfo.UnitClass = newUnitClass;
			newUnit.UnitInfo.UnitType = newUnitType;
			newUnit.UnitInfo.UnitGroupType = newUnitGroupType;

			var updateResult = TheGame.JTSServices.UnitService.UpdateUnits(new List<IUnit> { newUnit });

			// Component is updated in context correctly
			Verify_CRUD_Context_Operation<IUnit>(updateResult, newUnit);
			Assert.AreSame(newUnit, updateResult.SuccessfulObjects.First());
			Assert.AreSame(newUnit.Location, newLocation);
			Assert.AreSame(newUnit.UnitInfo.UnitClass, newUnitClass);
			Assert.AreSame(newUnit.UnitInfo.UnitType, newUnitType);
			Assert.AreSame(newUnit.UnitInfo.UnitGroupType, newUnitGroupType);
			Assert.AreEqual(newUnit.Name, "newTestUnit");

			// Component is updated in cache correctly
			cacheUnit = TheGame.Cache.UnitCache.TryFind(updateResult.SuccessfulObjects.First().UID);
			Assert.AreEqual(cacheUnit, newUnit);

		// DELETE/REMOVE

			// Component is removed from context correctly
			var removeResult = TheGame.JTSServices.UnitService.RemoveUnits(new List<IUnit> { newUnit });
			Verify_CRUD_Context_Operation<IUnit>(removeResult, newUnit);

			// Component is removed from cache correctly
			cacheUnit = TheGame.Cache.UnitCache.TryFind(removeResult.SuccessfulObjects.First().UID);
			Assert.IsNull(cacheUnit);

			try
			{
				var getResult = TheGame.JTSServices.UnitService.GetUnitByID(newUnit.ID); 
			}
			catch (Exception ex)
			{
#pragma warning disable 612,618
				Assert.IsInstanceOf(typeof(ComponentNotFoundException), ex);
#pragma warning restore 612,618
			}
		}

		[Test]
		public void Tile_CRUD()
		{
		// CREATE/SAVE

			var location = new Coordinate(1, 0, 0);
			var country = TheGame.JTSServices.GenericComponentService.GetByName<Country>("USA");

			var newComponent = new Tile(location, null)
			{
				 Name = "testTile",
				 Country = country, 
				 VictoryPoints = 5 				  
			};

			newComponent.SetNextID();

			// Component is saved into context correctly
			var saveResult = TheGame.JTSServices.TileService.SaveTiles(new List<ITile> { newComponent });
			Verify_CRUD_Context_Operation<ITile>(saveResult, newComponent);

			// Component is saved into cache correctly
			var cacheComponent = TheGame.Cache.TileCache.TryFind(saveResult.SuccessfulObjects.First().UID);
			Assert.AreEqual(cacheComponent, newComponent);

		// UPDATE

			var newLocation = new Coordinate(2, 2, 0);
			newComponent.Name = "newTestTile";
			newComponent.Location = newLocation;

			var updateResult = TheGame.JTSServices.TileService.UpdateTiles(new List<ITile> { newComponent });

			// Component is updated in context correctly
			Verify_CRUD_Context_Operation<ITile>(updateResult, newComponent);
			Assert.AreSame(newComponent, updateResult.SuccessfulObjects.First());
			Assert.AreSame(newComponent.Location, newLocation);
			Assert.AreEqual(newComponent.Name, "newTestTile");

			// Component is updated in cache correctly
			cacheComponent = TheGame.Cache.TileCache.TryFind(updateResult.SuccessfulObjects.First().UID);
			Assert.AreEqual(cacheComponent, newComponent);

		// DELETE/REMOVE

			// Component is removed from context correctly
			var removeResult = TheGame.JTSServices.TileService.RemoveTiles(new List<ITile> { newComponent });
			Verify_CRUD_Context_Operation<ITile>(removeResult, newComponent);

			// Component is removed from cache correctly
			cacheComponent = TheGame.Cache.TileCache.TryFind(removeResult.SuccessfulObjects.First().UID);
			Assert.IsNull(cacheComponent);

			try
			{
				var getResult = TheGame.JTSServices.TileService.GetTiles().Where(t => t.ID == newComponent.ID).SingleOrDefault();
			}
			catch (Exception ex)
			{
#pragma warning disable 612,618
				Assert.IsInstanceOf(typeof(ComponentNotFoundException), ex);
#pragma warning restore 612,618
			}
		}

		[Test]
		public void Node_CRUD()
		{
			// CREATE/SAVE

			var location = new Coordinate(1, 0, 0);
			var country = TheGame.JTSServices.GenericComponentService.GetByName<Country>("USA");

			var defaultTile = new Tile(location, null);

			var newComponent = new Node(location, defaultTile)
			{
				Name = "testNode",
				Country = country
			};

			newComponent.SetNextID();

			// Component is saved into context correctly
			var saveResult = TheGame.JTSServices.NodeService.SaveNodes(new List<INode> { newComponent });
			Verify_CRUD_Context_Operation<INode>(saveResult, newComponent);

			// Component is saved into cache correctly
			var cacheComponent = TheGame.Cache.NodeCache.TryFind(saveResult.SuccessfulObjects.First().UID);
			Assert.AreEqual(cacheComponent, newComponent);

			// UPDATE

			var newLocation = new Coordinate(2, 2, 0);
			newComponent.Name = "newTestNode";
			newComponent.Location = newLocation;

			var updateResult = TheGame.JTSServices.NodeService.UpdateNodes(new List<INode> { newComponent });

			// Component is updated in context correctly
			Verify_CRUD_Context_Operation<INode>(updateResult, newComponent);
			Assert.AreSame(newComponent, updateResult.SuccessfulObjects.First());
			Assert.AreSame(newComponent.Location, newLocation);
			Assert.AreEqual(newComponent.Name, "newTestNode");

			// Component is updated in cache correctly
			cacheComponent = TheGame.Cache.NodeCache.TryFind(updateResult.SuccessfulObjects.First().UID);
			Assert.AreEqual(cacheComponent, newComponent);

			// DELETE/REMOVE

			// Component is removed from context correctly
			var removeResult = TheGame.JTSServices.NodeService.RemoveNodes(new List<INode> { newComponent });
			Verify_CRUD_Context_Operation<INode>(removeResult, newComponent);

			// Component is removed from cache correctly
			cacheComponent = TheGame.Cache.NodeCache.TryFind(removeResult.SuccessfulObjects.First().UID);
			Assert.IsNull(cacheComponent);

			try
			{
				var getResult = TheGame.JTSServices.NodeService.GetAllNodes().Where(n => n.ID == newComponent.ID).SingleOrDefault();
			}
			catch (Exception ex)
			{
#pragma warning disable 612,618
				Assert.IsInstanceOf(typeof(ComponentNotFoundException), ex);
#pragma warning restore 612,618
			}
		}

		[Test]
		public void Faction_CRUD()
		{
			// CREATE/SAVE

			var newComponent = new Faction()
			{
				Name = "GoodGuys"
			};

			newComponent.SetNextID();

			// Component is saved into context correctly
			var saveResult = TheGame.JTSServices.GameService.SaveFactions(new List<IFaction> { newComponent });
			Verify_CRUD_Context_Operation<IFaction>(saveResult, newComponent);

			// UPDATE

			newComponent.Name = "BadGuys";

			var updateResult = TheGame.JTSServices.GameService.UpdateFactions(new List<IFaction> { newComponent });

			// Component is updated in context correctly
			Verify_CRUD_Context_Operation<IFaction>(updateResult, newComponent);
			Assert.AreSame(newComponent, updateResult.SuccessfulObjects.First());
			Assert.AreEqual(newComponent.Name, "BadGuys");

			// DELETE/REMOVE

			// Component is removed from context correctly
			var removeResult = TheGame.JTSServices.GameService.RemoveFactions(new List<IFaction> { newComponent });
			Verify_CRUD_Context_Operation<IFaction>(removeResult, newComponent);

			try
			{
				var getResult = TheGame.JTSServices.GameService.GetAllFactions().Where(f => f.ID == newComponent.ID);
			}
			catch (Exception ex)
			{
#pragma warning disable 612,618
				Assert.IsInstanceOf(typeof(ComponentNotFoundException), ex);
#pragma warning restore 612,618
			}
		}

		[Test]
		public void Country_CRUD()
		{
			// CREATE/SAVE

			var faction = new Faction();

			var newComponent = new Country()
			{
				Name = "Canada", 
				Faction = faction
			};

			newComponent.SetNextID();

			// Component is saved into context correctly
			var saveResult = TheGame.JTSServices.GameService.SaveCountries(new List<ICountry> { newComponent });
			Verify_CRUD_Context_Operation<ICountry>(saveResult, newComponent);

			// UPDATE

			newComponent.Name = "Germany";

			var updateResult = TheGame.JTSServices.GameService.UpdateCountries(new List<ICountry> { newComponent });

			// Component is updated in context correctly
			Verify_CRUD_Context_Operation<ICountry>(updateResult, newComponent);
			Assert.AreSame(newComponent, updateResult.SuccessfulObjects.First());
			Assert.AreEqual(newComponent.Name, "Germany");

			// DELETE/REMOVE

			// Component is removed from context correctly
			var removeResult = TheGame.JTSServices.GameService.RemoveCountries(new List<ICountry> { newComponent });
			Verify_CRUD_Context_Operation<ICountry>(removeResult, newComponent);

			try
			{
				var getResult = TheGame.JTSServices.GameService.GetCountries().Where(c => c.ID == newComponent.ID).SingleOrDefault();
			}
			catch (Exception ex)
			{
#pragma warning disable 612,618
				Assert.IsInstanceOf(typeof(ComponentNotFoundException), ex);
#pragma warning restore 612,618
			}
		}

		[Test]
		public void Player_CRUD()
		{
			// CREATE/SAVE

			var country = new Mock<ICountry>();
			var faction = new Mock<IFaction>();
			country.Object.Faction = faction.Object;
			var unit = ScriptTestUnits.BigGuns;

			var newComponent = new Player(country.Object)
			{
				Name = "testPlayer", 
				IsAIPlayer = false, 
			};

			newComponent.TrackedValues.ReinforcementPoints = 100;
			newComponent.AddReinforcementUnit(unit);
			newComponent.SetNextID();

			// Caught off-guard. The adding of the reinforcement decrements the available reinforcement points.
			// Gotta allow for that
			var netPoints = (100 - unit.UnitCost);

			// Component is saved into context correctly
			var saveResult = TheGame.JTSServices.GameService.SavePlayers(new List<IPlayer> { newComponent });
			Verify_CRUD_Context_Operation<IPlayer>(saveResult, newComponent);
			Assert.AreSame(newComponent, saveResult.SuccessfulObjects.First());
			Assert.AreEqual(newComponent.Name, "testPlayer");
			Assert.IsTrue(newComponent.UnplacedReinforcements.Any());
			Assert.IsFalse(newComponent.IsAIPlayer);
			Assert.AreEqual(newComponent.TrackedValues.ReinforcementPoints, netPoints);

			// UPDATE

			newComponent.Name = "newTestPlayer";
			newComponent.RemoveReinforcementUnit(unit);
			newComponent.IsAIPlayer = true;
			newComponent.TrackedValues.ReinforcementPoints = 500;

			var updateResult = TheGame.JTSServices.GameService.UpdatePlayers(new List<IPlayer> { newComponent });

			// Component is updated in context correctly
			Verify_CRUD_Context_Operation<IPlayer>(updateResult, newComponent);
			Assert.AreSame(newComponent, updateResult.SuccessfulObjects.First());
			Assert.AreEqual(newComponent.Name, "newTestPlayer");
			Assert.IsTrue(!newComponent.UnplacedReinforcements.Any());
			Assert.IsTrue(newComponent.IsAIPlayer);
			Assert.AreEqual(newComponent.TrackedValues.ReinforcementPoints, 500);

			// DELETE/REMOVE

			// Component is removed from context correctly
			var removeResult = TheGame.JTSServices.GameService.RemovePlayers(new List<IPlayer> { newComponent });
			Verify_CRUD_Context_Operation<IPlayer>(removeResult, newComponent);

			try
			{
				var getResult = TheGame.JTSServices.GameService.GetPlayers().SingleOrDefault(p => p.ID == newComponent.ID);
			}
			catch (Exception ex)
			{
#pragma warning disable 612,618
				Assert.IsInstanceOf(typeof(ComponentNotFoundException), ex);
#pragma warning restore 612,618
			}
		}

		[Test]
		public void UnitTransports_CRUD()
		{
		// CREATE/SAVE

			var unit = ScriptTestUnits.LOADME_LINFANTRY;
			var transport = ScriptTestUnits.LOADTO_HELO;

			var checkValue = new Tuple<IUnit, IUnit>(unit, transport);

			// data is saved into context correctly
			var saveResult = TheGame.JTSServices.DataService.SaveUnitTransport(unit, transport);
			Assert.AreEqual(saveResult.Status, ResultStatus.SUCCESS);
			Assert.AreEqual(saveResult.SuccessfulObjects.Count(), 1);
			Assert.AreEqual(saveResult.FailedObjects.Count(), 0);
			Assert.IsTrue(saveResult.SuccessfulObjects.Contains(checkValue));
			Assert.AreEqual(transport.GetTransportedUnits().Count(), 1);
			Assert.AreEqual(transport.GetTransportedUnits().SingleOrDefault(), unit);

		// DELETE/REMOVE

			// Component is removed from context correctly
			var removeResult = TheGame.JTSServices.DataService.RemoveUnitTransport(unit, transport);
			Assert.AreEqual(saveResult.Status, ResultStatus.SUCCESS);
			Assert.AreEqual(saveResult.SuccessfulObjects.Count(), 1);
			Assert.AreEqual(saveResult.FailedObjects.Count(), 0);
			Assert.IsTrue(saveResult.SuccessfulObjects.Contains(checkValue));
			Assert.IsTrue(!transport.GetTransportedUnits().Any());			
		}

		[Test]
		public void UnitAssignments_CRUD()
		{
		// CREATE/SAVE

			var unit = ScriptTestUnits.BigGuns;
			var assignToUnit = ScriptTestUnits.Btl101;

			var checkValue = new Tuple<IUnit, IUnit>(unit, assignToUnit);

			// data is saved into context correctly
			var saveResult = TheGame.JTSServices.DataService.SaveUnitAssignment(unit, assignToUnit);
			Assert.AreEqual(saveResult.Status, ResultStatus.SUCCESS);
			Assert.AreEqual(saveResult.SuccessfulObjects.Count(), 1);
			Assert.AreEqual(saveResult.FailedObjects.Count(), 0);
			Assert.IsTrue(saveResult.SuccessfulObjects.Contains(checkValue));
			Assert.IsTrue(assignToUnit.GetDirectAttachedUnits().Contains(unit));
			Assert.AreEqual(unit.AttachedToUnit, assignToUnit);

		// DELETE/REMOVE

			// Component is removed from context correctly
			var removeResult = TheGame.JTSServices.DataService.RemoveUnitAssignment(unit, assignToUnit);
			Assert.AreEqual(saveResult.Status, ResultStatus.SUCCESS);
			Assert.AreEqual(saveResult.SuccessfulObjects.Count(), 1);
			Assert.AreEqual(saveResult.FailedObjects.Count(), 0);
			Assert.IsTrue(saveResult.SuccessfulObjects.Contains(checkValue));
			Assert.IsTrue(!assignToUnit.GetDirectAttachedUnits().Contains(unit));
			Assert.AreEqual(unit.AttachedToUnit, null);	
		}

		[Test]
		public void FactionVictoryConditions_CRUD()
		{
		// CREATE/SAVE

			var faction = new Faction();
			var victoryCondition = new VictoryCondition{VictoryConditionType = TheGame.JTSServices.GenericComponentService.GetByID<VictoryConditionType>(0)};
			faction.SetNextID();

			var checkValue = new Tuple<IFaction, IVictoryCondition>(faction, victoryCondition);

			// data is saved into context correctly
			var saveResult = TheGame.JTSServices.DataService.SaveFactionVictoryConditions(faction, victoryCondition);
			Assert.AreEqual(saveResult.Status, ResultStatus.SUCCESS);
			Assert.AreEqual(saveResult.SuccessfulObjects.Count(), 1);
			Assert.AreEqual(saveResult.FailedObjects.Count(), 0);
			Assert.IsTrue(saveResult.SuccessfulObjects.Contains(checkValue));

			var tmp = TheGame.JTSServices.DataService.GetFactionVictoryConditions()
				.SingleOrDefault(fvc => fvc.FactionID == faction.ID && fvc.ConditionType == victoryCondition.VictoryConditionType.ID);

			Assert.IsNotNull(tmp);

		// DELETE/REMOVE

			// Component is removed from context correctly
			var removeResult = TheGame.JTSServices.DataService.RemoveFactionVictoryConditions(faction, victoryCondition);
			Assert.AreEqual(removeResult.Status, ResultStatus.SUCCESS);
			Assert.AreEqual(removeResult.SuccessfulObjects.Count(), 1);
			Assert.AreEqual(removeResult.FailedObjects.Count(), 0);
			Assert.IsTrue(removeResult.SuccessfulObjects.Contains(checkValue));

			tmp = TheGame.JTSServices.DataService.GetFactionVictoryConditions()
				.SingleOrDefault(fvc => fvc.FactionID == faction.ID && fvc.ConditionType == victoryCondition.VictoryConditionType.ID);

			Assert.IsNull(tmp);
		}

		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~`

		private void Verify_CRUD_Context_Operation<T>(IResult<T, T> result, T component)
		{
			Assert.AreEqual(result.Status, ResultStatus.SUCCESS);
			Assert.AreEqual(result.SuccessfulObjects.Count(), 1);
			Assert.AreEqual(result.FailedObjects.Count(), 0);
			Assert.IsTrue(result.SuccessfulObjects.Contains(component));
		}

	}
}
