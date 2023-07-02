using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTacticalSim.API.Cache;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API;
using JTacticalSim.API.Game;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.Service;
using JTacticalSim.API.Data;
using JTacticalSim.Component.World;
using JTacticalSim.Data.DTO;
using JTacticalSim.Component.Data;
using JTacticalSim.Component.AI;
using JTacticalSim.Component.GameBoard;
using JTacticalSim.Component.Game;
using JTacticalSim.Utility;
using JTacticalSim.DataContext.Repository;

namespace JTacticalSim.DataContext
{
	public static class ComponentConversion
	{

		public static IComponentSet ToComponent(this ComponentSetDTO o)
		{
			if (o == null) return null;

			var componentSet = new ComponentSet();

			SetBaseComponentValues(componentSet, o);
			componentSet.Path = o.Path;

			return componentSet;
		}

		public static IScenario ToComponent(this ScenarioDTO o)
		{
			if (o == null) return null;

			var scenario = new Scenario();

			SetBaseComponentValues(scenario, o);
			scenario.GameFileDirectory = o.GameFileDirectory;
			scenario.Author = o.Author;
			scenario.ComponentSet = Game.Instance.JTSServices.GenericComponentService.GetByID<ComponentSet>(o.ComponentSet);
			scenario.Countries = o.Countries.Select(c => c.ToComponent()).ToList();
			scenario.Factions = o.Factions.Select(f => f.ToComponent()).ToList();
			scenario.VictoryConditions = o.VictoryConditions.Select(vc => vc.ToComponent()).ToList();
			
			if (!string.IsNullOrWhiteSpace(o.Synopsis))
				scenario.Synopsis =  o.Synopsis;

			return scenario;
		}

		public static ISavedGame ToComponent(this SavedGameDTO o)
		{
			if (o == null) return null;

			var savedGame = new SavedGame();
			
			SetBaseComponentValues(savedGame, o);
			savedGame.GameFileDirectory = o.GameFileDirectory;
			savedGame.LastPlayed = o.LastPlayed;
			savedGame.Scenario = Game.Instance.JTSServices.GenericComponentService.GetByID<Scenario>(o.Scenario);

			return savedGame;
		}		

		public static IPlayer ToComponent(this PlayerDTO o)
		{
			if (o == null) return null;

			var player = new Player(Game.Instance.JTSServices.GameService.GetCountryByID(o.Country));
			
			SetBaseComponentValues(player, o);
			player.TrackedValues.ReinforcementPoints = o.ReinforcementPoints;
			player.TrackedValues.NuclearCharges = o.NuclearCharges;
			player.IsAIPlayer = o.IsAIPlayer;

			o.UnplacedReinforcements.ForEach(i => player.UnplacedReinforcements.Add(Game.Instance.JTSServices.UnitService.GetUnitByID(i)));

			return player;
		}

		public static ICoordinate ToComponent(this CoordinateDTO o)
		{
			if (o == null) return null;

			return new Coordinate(o.X, o.Y, o.Z);
		}		

		public static INode ToComponent(this NodeDTO o)
		{
			if (o == null) return null;

			var node = new Node(o.Location.ToComponent(), o.DefaultTile.ToComponent());
			
			SetBaseComponentValues(node, o);

			node.Location = o.Location.ToComponent();
			node.Country = Game.Instance.JTSServices.GameService.GetCountryByID(o.Country);

			return node;
		}		

		public static ITile ToComponent(this TileDTO o)
		{
			if (o == null) return null;

			// Check cache first
			var tile = Utility.Cache.TileCache.TryFind(o.UID);
			if (tile != null) return tile;

			// Demographics are set below
			tile = new Tile(o.Location.ToComponent(), null);

			SetBaseComponentValues(tile, o);

			tile.VictoryPoints = o.VictoryPoints;
			tile.IsGeographicChokePoint = o.IsGeographicChokePoint;
			tile.IsPrimeTarget = o.IsPrimeTarget;
			
			foreach (var d in o.Demographics)
			{
				var component = Game.Instance.JTSServices.DemographicService.GetDemographicByID(d.ID);
				component.InstanceName = d.InstanceName;
				component.Orientation = Orienting.ParseOrientationString(d.Orientation, new char[] {','});
				tile.AddDemographic(component);
			}				

			// get unit stacks
			var countries = Game.Instance.JTSServices.GameService.GetCountries();
			var units = Game.Instance.JTSServices.UnitService.GetUnitsAt(tile.Location, countries);
			if (units.Any())
			    tile.AddComponentsToStacks(units);

			// Add to cache
			Utility.Cache.TileCache.TryAdd(o.UID, tile as ITile);
			return tile;
		}		

		public static IUnit ToComponent(this UnitDTO o)
		{
			if (o == null) return null;

			// Try cache first
			var unit = Utility.Cache.UnitCache.TryFind(o.UID);
			if (unit != null) return unit;

			var ui = new UnitInfo(	Game.Instance.JTSServices.UnitService.GetUnitTypeByID(o.UnitType),
									Game.Instance.JTSServices.UnitService.GetUnitClassByID(o.UnitClass),
									Game.Instance.JTSServices.UnitService.GetUnitGroupTypeByID(o.UnitGroupType));


			// Account for unplaced units with no location
			var c = (o.Location == null) ? null : o.Location.ToComponent();

			unit = new Unit(o.Name, c, ui);
			SetBaseComponentValues(unit, o);

			// Be sure this is called before we set the default move stats as this sets the unit's HasPerformedAction to true
			unit.Posture = (o.Posture != null) ? (BattlePosture)o.Posture : BattlePosture.STANDARD;

			// Account for unit data files with no movement stats
			// not required for new scenarios or base scenarios
			var cmsi = new CurrentMoveStatInfo
				{
					HasPerformedAction = o.CurrentHasPerformedAction ?? false,
					MovementPoints = o.CurrentMovementPoints ?? unit.MovementPoints,
					RemoteFirePoints = o.CurrentRemoteFirePoints ?? unit.RemoteFirePoints,
				};

			unit.CurrentMoveStats = cmsi;
			unit.CurrentFuelRange = o.CurrentFuelRange ?? unit.UnitInfo.UnitType.FuelRange;
			unit.SubNodeLocation = new SubNodeLocation(o.SubNodeLocation);
			unit.Country = Game.Instance.JTSServices.GameService.GetCountryByID(o.Country);
			unit.StackOrder = o.StackOrder;

			// Set in cache
			Utility.Cache.UnitCache.TryUpdate(o.UID, unit as IUnit);

			return unit;
		}		

		public static ICountry ToComponent(this CountryDTO o)
		{
			if (o == null) return null;
			
			var retVal = new Country();

			SetBaseComponentValues(retVal, o);
			retVal.Faction = o.Faction.ToComponent();
            retVal.ColorString = o.Color;
			retVal.BGColorString = o.BGColor;
            retVal.Color = Render.GetConsoleColorByString(o.Color);
			retVal.BGColor = Render.GetConsoleColorByString(o.BGColor);
			retVal.TextDisplayColor = Render.GetConsoleColorByString(o.TextDisplayColor);
			retVal.FlagBGColor = Render.GetConsoleColorByString(o.FlagBGColor);
			retVal.FlagColorA = Render.GetConsoleColorByString(o.FlagColorA);
			retVal.FlagColorB = Render.GetConsoleColorByString(o.FlagColorB);
            retVal.FlagDisplayTextA = o.FlagDisplayTextA;
			retVal.FlagDisplayTextB = o.FlagDisplayTextB;
			return retVal;
		}		

		public static IFaction ToComponent(this FactionDTO o)
		{
			if (o == null) return null;

			var retVal = new Faction();
			SetBaseComponentValues(retVal, o);
			return retVal;
		}		

		public static IDemographic ToComponent(this DemographicDTO o)
		{
			if (o == null) return null;

			// Check cache first
			var retVal = Utility.Cache.DemographicCache.TryFind(o.UID);
			if (retVal != null) return retVal;

			retVal = new Demographic
						{
							Value = o.Value,
							InstanceName = o.InstanceName,
							ProvidesMedical = o.ProvidesMedical,
							ProvidesSupply = o.ProvidesSupply,
							DemographicClass = Game.Instance.JTSServices.DemographicService.GetDemographicClassByID(o.DemographicClass),
							Orientation = Orienting.ParseOrientationString(o.Orientation, new char[] {','})
						};

			SetBaseComponentValues(retVal, o);

			// Set in cache
			Utility.Cache.DemographicCache.TryAdd(o.UID, o as IDemographic);

			return retVal;
		}		


		public static IUnitBaseType ToComponent(this UnitBaseTypeDTO o)
		{
			if (o == null) return null;

			var retVal = new UnitBaseType();
			SetBaseComponentValues(retVal, o);

			retVal.CanReceiveMedicalSupport = o.CanReceiveMedicalSupport;
			retVal.CanBeSupplied = o.CanBeSupplied;
			retVal.NuclearAffected = o.NuclearAffected;
			retVal.OutOfFuelMoveResultMessage = o.OutOfFuelMoveResultMessage;

			return retVal;
		}		

		public static IUnitClass ToComponent(this UnitClassDTO o)
		{
			if (o == null) return null;

			var unitClass = new UnitClass();

			SetBaseComponentValues(unitClass, o);
			SetModifierValues(unitClass, o);

			unitClass.TextDisplayZ1 = o.TextDisplayZ1;
			unitClass.TextDisplayZ2 = o.TextDisplayZ2;
			unitClass.TextDisplayZ3 = o.TextDisplayZ3;
			unitClass.TextDisplayZ4 = o.TextDisplayZ4;
			unitClass.Sounds.AddSound(SoundType.FIRE, o.Sound_Fire);
			unitClass.Sounds.AddSound(SoundType.MOVE, o.Sound_Move);

			return unitClass;
		}	
	
		public static IUnitBranch ToComponent(this UnitBranchDTO o)
		{
			if (o == null) return null;
			var c = new UnitBranch();
			SetBaseComponentValues(c, o);
			return c;
		}

		public static IUnitType ToComponent(this UnitTypeDTO o)
		{
			if (o == null) return null;

			var unitType = new UnitType();

			unitType.TextDisplayZ1 = o.TextDisplayZ1;
			unitType.TextDisplayZ2 = o.TextDisplayZ2;
			unitType.TextDisplayZ3 = o.TextDisplayZ3;
			unitType.TextDisplayZ4 = o.TextDisplayZ4;
			unitType.FuelConsumer = o.FuelConsumer;
			unitType.Nuclear = o.Nuclear;
			unitType.FuelRange = o.FuelRange;
			unitType.BaseType = Game.Instance.JTSServices.GenericComponentService.GetByID<UnitBaseType>(o.UnitBaseTypeID);
			unitType.Branch = Game.Instance.JTSServices.GenericComponentService.GetByID<UnitBranch>(o.UnitBranchID);
			unitType.Sounds.AddSound(SoundType.FIRE, o.Sound_Fire);
			unitType.Sounds.AddSound(SoundType.MOVE, o.Sound_Move);

			SetModifierValues(unitType, o);
			SetBaseComponentValues(unitType, o);

			return unitType;
		}		

		public static IUnitGroupType ToComponent(this UnitGroupTypeDTO o)
		{
			if (o == null) return null;

			var unitGroupType = new UnitGroupType();

			unitGroupType.TextDisplayZ1 = o.TextDisplayZ1;
			unitGroupType.TextDisplayZ2 = o.TextDisplayZ2;
			unitGroupType.TextDisplayZ3 = o.TextDisplayZ3;
			unitGroupType.TextDisplayZ4 = o.TextDisplayZ4;
			unitGroupType.Level = o.Level;
			unitGroupType.MaxDirectAssignedUnits = o.MaxDirectAssignedUnits;

			SetBaseComponentValues(unitGroupType, o);

			return unitGroupType;
		}

		public static IUnitGeogType ToComponent(this UnitGeogTypeDTO o)
		{
			if (o == null) return null;
			var unitGeogType = new UnitGeogType();
			SetBaseComponentValues(unitGeogType, o);
			return unitGeogType;
		}

		public static IDemographicClass ToComponent(this DemographicClassDTO o)
		{
			if (o == null) return null;

			var demographicClass = new DemographicClass();

			demographicClass.TextDisplayZ1 = o.TextDisplayZ1;
			demographicClass.TextDisplayZ2 = o.TextDisplayZ2;
			demographicClass.TextDisplayZ3 = o.TextDisplayZ3;
			demographicClass.TextDisplayZ4 = o.TextDisplayZ4;
			demographicClass.MovementHinderanceConfigured = o.MovementHinderanceConfigured;
			demographicClass.DemographicType = Game.Instance.JTSServices.DemographicService.GetDemographicTypeByID(o.DemographicType);
			demographicClass.BuildInfo = o.BuildInfo;

			SetModifierValues(demographicClass, o);
			SetBaseComponentValues(demographicClass, o);

			return demographicClass;
		}		

		public static IDemographicType ToComponent(this DemographicTypeDTO o)
		{
			if (o == null) return null;

			var demographicType = new DemographicType();
			SetBaseComponentValues(demographicType, o);
			demographicType.DisplayOrder = o.DisplayOrder;
			return demographicType;
		}

		public static IBasePointValues ToComponent(this IBasePointValues o)
		{
			if (o == null) return null;
			return new BasePointValues(	o.Movement, 
										o.CombatRoll, 
										o.CombatBase,
										o.StealthBase,
										o.StealthBase,
										o.HiddenStealthThreshhold,
 										o.MedicalSupportBase,
										o.WeightBase,
										o.CostBase,
										o.AIBaseRoll,
										o.AIAggressiveness,
										o.AIDefensiveness,
										o.AIIntelligence,
										o.MeterDistanceBase,
										o.ReinforcementCalcBaseCountry,
										o.ReinforcementCalcBaseFaction,
										o.ReinforcementCalcBaseVP,
										o.HQBonus,
										o.NotSuppliedPenalty,
										o.MaxSupplyDistance,
										o.TargetAttachedUnitBonus,
										o.TargetMedicalUnitBonus,
										o.TargetSupplyUnitBonus,
										o.BattlePostureBonus);
		}

		public static IVictoryCondition ToComponent(this VictoryConditionDTO o)
		{			
		    if (o == null) return null;

		    var victoryCondition = new VictoryCondition();
		    SetBaseComponentValues(victoryCondition, o);

			victoryCondition.Value = o.Value;
			victoryCondition.Faction = o.Faction.ToComponent();
			victoryCondition.VictoryConditionType = o.VictoryConditionType.ToComponent();

		    return victoryCondition;
		}


		public static IVictoryConditionType ToComponent(this VictoryConditionTypeDTO o)
		{
			if (o == null) return null;
			var victoryConditionType = new VictoryConditionType();
			SetBaseComponentValues(victoryConditionType, o);
			return victoryConditionType;
		}

#region StrategyCache

		public static IUnitTaskType ToComponent(this UnitTaskTypeDTO o)
		{
			if (o == null) return null;

		    var unitTaskType = new UnitTaskType();
			SetBaseComponentValues(unitTaskType, o);
			return unitTaskType;
		}

		public static IMissionType ToComponent(this MissionTypeDTO o)
		{
			if (o == null) return null;

			var MissionType = new MissionType();
			SetBaseComponentValues(MissionType, o);
			MissionType.Priority = o.Priority;
			MissionType.TurnOrder = o.TurnOrder;
			MissionType.CanceledByMove = o.CanceledByMove;
			return MissionType;
		}


#endregion		


		private static void SetBaseComponentValues(IBaseComponent component, IBaseGameComponentDTO dto)
		{
			component.ID = dto.ID;
			component.UID = dto.UID;
			component.Name = dto.Name;
			component.Description = dto.Description;
		}

		public static void SetModifierValues(IStatModifier toStatModifierContainer, IStatModifier fromStatModiferContainer)
		{
			toStatModifierContainer.AttackDistanceModifier = fromStatModiferContainer.AttackDistanceModifier;
			toStatModifierContainer.RemoteFirePoints = fromStatModiferContainer.RemoteFirePoints;
			toStatModifierContainer.AttackModifier = fromStatModiferContainer.AttackModifier;
			toStatModifierContainer.StealthModifier = fromStatModiferContainer.StealthModifier;
			toStatModifierContainer.DefenceModifier = fromStatModiferContainer.DefenceModifier;
			toStatModifierContainer.MovementModifier = fromStatModiferContainer.MovementModifier;
			toStatModifierContainer.AllowableWeightModifier = fromStatModiferContainer.AllowableWeightModifier;
			toStatModifierContainer.UnitWeightModifier = fromStatModiferContainer.UnitWeightModifier;
			toStatModifierContainer.UnitCostModifier = fromStatModiferContainer.UnitCostModifier;
		}
	}
}
