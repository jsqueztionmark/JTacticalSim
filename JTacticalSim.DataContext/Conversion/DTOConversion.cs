using System.Linq;
using JTacticalSim.API.Game;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.Data;
using JTacticalSim.Component.Data;
using JTacticalSim.Data.DTO;
using JTacticalSim.Utility;
using JTacticalSim.DataContext.Repository;

namespace JTacticalSim.DataContext
{
	public static class DTOConversion
	{
		public static ComponentSetDTO ToDTO(this IComponentSet c)
		{
			if (c == null) return null;

			var dto = new ComponentSetDTO();

			SetBaseDTOValues(dto, c);
			dto.Path = c.Path;

			return dto;
		}

		public static ScenarioDTO ToDTO(this IScenario c)
		{
			if (c == null) return null;

			var dto = new ScenarioDTO();

			SetBaseDTOValues(dto, c);
			dto.Author = c.Author;
			dto.GameFileDirectory = c.GameFileDirectory;
			dto.ComponentSet = c.ComponentSet.ID;
			dto.Countries = c.Countries.Select(country => country.ToDTO()).ToList();
			dto.Factions = c.Factions.Select(faction => faction.ToDTO()).ToList();
			dto.VictoryConditions = c.VictoryConditions.Select(vc => vc.ToDTO()).ToList();
			dto.Synopsis = c.Synopsis;

			return dto;
		}

		public static SavedGameDTO ToDTO(this ISavedGame c)
		{
			if (c == null) return null;

			var dto = new SavedGameDTO();

			SetBaseDTOValues(dto, c);

			dto.GameFileDirectory = c.GameFileDirectory;
			dto.LastPlayed = c.LastPlayed;
			dto.Scenario = c.Scenario.ID;

			return dto;
		}

		public static PlayerDTO ToDTO(this IPlayer c)
		{
			if (c == null) return null;

			var dto = new PlayerDTO();

			SetBaseDTOValues(dto, c);

			dto.Country = c.Country.ID;
			dto.ReinforcementPoints = c.TrackedValues.ReinforcementPoints;
			dto.NuclearCharges = c.TrackedValues.NuclearCharges;
			dto.IsCurrentPlayer = c.IsCurrentPlayer;
			dto.IsAIPlayer = c.IsAIPlayer;
			c.UnplacedReinforcements.ForEach(u => dto.UnplacedReinforcements.Add(u.ID));

			return dto;
		}

		public static CoordinateDTO ToDTO(this ICoordinate c)
		{
			if (c == null) return null;

			return new CoordinateDTO
						{
							X = c.X, 
							Y = c.Y, 
							Z = c.Z
						};
		}

		public static NodeDTO ToDTO(this INode c)
		{
			if (c == null) return null;

			var dto = new NodeDTO();

			SetBaseDTOValues(dto, c);

			dto.Location = c.Location.ToDTO();
			dto.Country = c.Country.ID;
			dto.DefaultTile = c.DefaultTile.ToDTO();
			

			return dto;
		}

		public static TileDTO ToDTO(this ITile c)
		{
			if (c == null) return null;

			var dto = new TileDTO();
			
			SetBaseDTOValues(dto, c);

			dto.Location = c.Location.ToDTO();
			dto.VictoryPoints = c.VictoryPoints;
			dto.IsGeographicChokePoint = c.IsGeographicChokePoint;
			dto.IsPrimeTarget = c.IsPrimeTarget;

			dto.Demographics = c.GetAllDemographics().EnsureUnique().Select(d => d.ToDTO());

			return dto;
		}

		public static UnitDTO ToDTO(this IUnit c)
		{
			if (c == null) return null;

			var dto = new UnitDTO();

			SetBaseDTOValues(dto, c);

			dto.SubNodeLocation = c.SubNodeLocation.Value;
			dto.Posture = (int) c.Posture;
			dto.Country = c.Country.ID;
			dto.StackOrder = c.StackOrder;
			dto.Location = c.Location.ToDTO();
			dto.UnitType = c.UnitInfo.UnitType.ID;
			dto.UnitClass = c.UnitInfo.UnitClass.ID;
			dto.UnitGroupType = c.UnitInfo.UnitGroupType.ID;
			dto.CurrentHasPerformedAction = c.CurrentMoveStats.HasPerformedAction;
			dto.CurrentMovementPoints = c.CurrentMoveStats.MovementPoints;
			dto.CurrentRemoteFirePoints = c.CurrentMoveStats.RemoteFirePoints;
			dto.CurrentFuelRange = c.CurrentFuelRange;
			dto.UnitsTransported = c.GetTransportedUnits().Select(u => u.ID).ToList();

			return dto;
		}

		public static CountryDTO ToDTO(this ICountry c)
		{
			if (c == null) return null;

			var dto = new CountryDTO
							{
								Faction = c.Faction.ToDTO(),
                                Color = c.ColorString,
								BGColor = c.BGColorString,
								TextDisplayColor = c.TextDisplayColorString,
								FlagBGColor = c.FlagBGColorString,
								FlagColorA = c.FlagColorAString,
								FlagColorB = c.FlagColorBString,
                                FlagDisplayTextA = c.FlagDisplayTextA,
								FlagDisplayTextB = c.FlagDisplayTextB
							};

			SetBaseDTOValues(dto, c);

			return dto;

		}

		public static FactionDTO ToDTO(this IFaction c)
		{
			if (c == null) return null;

			var dto = new FactionDTO();
			SetBaseDTOValues(dto, c);

			return dto;
		}

		public static DemographicDTO ToDTO(this IDemographic c)
		{
			if (c == null) return null;

			var dto = new DemographicDTO
							{
								DemographicClass = c.DemographicClass.ID,
								Value = c.Value,
								ProvidesSupply = c.ProvidesSupply,
								ProvidesMedical = c.ProvidesMedical,
								InstanceName = c.InstanceName,
								Orientation = Orienting.ConvertDirectionsToString(c.Orientation, new char[] {','})
							};

			SetBaseDTOValues(dto, c);

			return dto;
		}



		/// <summary>
		/// General DTO converter for all base data types
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public static BaseGameComponentDTO ToDTO(this IBaseComponent c)
		{
			if (c == null) return null;

			var dto = new BaseGameComponentDTO();
			SetBaseDTOValues(dto as IBaseBoardComponentDTO, c);
			return dto;
		}

		public static UnitBaseTypeDTO ToDTO(this IUnitBaseType c)
		{
			if (c == null) return null;

			var dto = new UnitBaseTypeDTO();
			SetBaseDTOValues(dto, c);

			dto.CanReceiveMedicalSupport = c.CanReceiveMedicalSupport;
			dto.CanBeSupplied = c.CanBeSupplied;
			dto.NuclearAffected = c.NuclearAffected;
			dto.OutOfFuelMoveResultMessage = c.OutOfFuelMoveResultMessage;

			return dto;
		}

		public static UnitClassDTO ToDTO(this IUnitClass c)
		{
			if (c == null) return null;

			var retVal = new UnitClassDTO
								{
									TextDisplayZ1 = c.TextDisplayZ1,
									TextDisplayZ2 = c.TextDisplayZ2,
									TextDisplayZ3 = c.TextDisplayZ3,
									TextDisplayZ4 = c.TextDisplayZ4,
								};

			ComponentConversion.SetModifierValues(retVal, c);
			SetBaseDTOValues(retVal, c);

			return retVal;
		}

		public static UnitBranchDTO ToComponent(this IUnitBranch c)
		{
			if (c == null) return null;
			var dto = new UnitBranchDTO();
			SetBaseDTOValues(dto, c);
			return dto;
		}

		public static UnitTypeDTO ToDTO(this IUnitType c)
		{
			if (c == null) return null;

			var retVal = new UnitTypeDTO
								{
									TextDisplayZ1 = c.TextDisplayZ1,
									TextDisplayZ2 = c.TextDisplayZ2,
									TextDisplayZ3 = c.TextDisplayZ3,
									TextDisplayZ4 = c.TextDisplayZ4,
									UnitBaseTypeID = c.BaseType.ID,
									UnitBranchID = c.Branch.ID,
									FuelConsumer = c.FuelConsumer,
									FuelRange = c.FuelRange,
									Nuclear = c.Nuclear
								};

			ComponentConversion.SetModifierValues(retVal, c);
			SetBaseDTOValues(retVal, c);

			return retVal;
		}

		public static UnitGroupTypeDTO ToDTO(this IUnitGroupType c)
		{
			if (c == null) return null;

			var dto = new UnitGroupTypeDTO
							{
								TextDisplayZ1 = c.TextDisplayZ1,
								TextDisplayZ2 = c.TextDisplayZ2,
								TextDisplayZ3 = c.TextDisplayZ3,
								TextDisplayZ4 = c.TextDisplayZ4,
								Level = c.Level,
								MaxDirectAssignedUnits = c.MaxDirectAssignedUnits
							};

			SetBaseDTOValues(dto, c);

			return dto;
		}

		public static UnitGeogTypeDTO ToDTO(this IUnitGeogType c)
		{
			if (c == null) return null;
			var dto = new UnitGeogTypeDTO();
			SetBaseDTOValues(dto, c);
			return dto;
		}

		public static DemographicClassDTO ToDTO(this IDemographicClass c)
		{
			if (c == null) return null;

			var retVal = new DemographicClassDTO
								{
									TextDisplayZ1 = c.TextDisplayZ1,
									TextDisplayZ2 = c.TextDisplayZ2,
									TextDisplayZ3 = c.TextDisplayZ3,
									TextDisplayZ4 = c.TextDisplayZ4,
									MovementHinderanceConfigured = c.MovementHinderanceConfigured,
									DemographicType = c.DemographicType.ID,
									BuildInfo = c.BuildInfo
								};

			ComponentConversion.SetModifierValues(retVal, c);
			SetBaseDTOValues(retVal, c);

			return retVal;
		}

		public static DemographicTypeDTO ToDTO(this IDemographicType c)
		{
			if (c == null) return null;

			var dto = new DemographicTypeDTO();
			SetBaseDTOValues(dto, c);
			dto.DisplayOrder = c.DisplayOrder;
			return dto;
		}

		public static BasePointValuesDTO ToDTO(this IBasePointValues c)
		{
			if (c == null) return null;
			return new BasePointValuesDTO(	c.Movement, 
											c.CombatRoll, 
											c.CombatBase,
											c.StealthRoll,
											c.StealthBase,
											c.HiddenStealthThreshhold,
 											c.MedicalSupportBase,
											c.WeightBase, 
											c.CostBase,
											c.AIBaseRoll,
											c.AIAggressiveness,
											c.AIDefensiveness,
											c.AIIntelligence,
											c.MeterDistanceBase,
											c.ReinforcementCalcBaseCountry,
											c.ReinforcementCalcBaseFaction,
											c.ReinforcementCalcBaseVP,
											c.HQBonus,
											c.NotSuppliedPenalty,
											c.MaxSupplyDistance,
											c.TargetAttachedUnitBonus,
											c.TargetMedicalUnitBonus,
											c.TargetSupplyUnitBonus,
											c.BattlePostureBonus);
		}

		public static VictoryConditionDTO ToDTO(this IVictoryCondition c)
		{
			if (c == null) return null;

			var dto = new VictoryConditionDTO();
			SetBaseDTOValues(dto, c);

			dto.Faction = c.Faction.ToDTO();
			dto.Value = c.Value;
			dto.VictoryConditionType = c.VictoryConditionType.ToDTO();

			return dto;
		}

		public static VictoryConditionTypeDTO ToDTO(this IVictoryConditionType c)
		{
			if (c == null) return null;

			var dto = new VictoryConditionTypeDTO();
			SetBaseDTOValues(dto, c);

			return dto;
		}

#region StrategyCache

		public static UnitTaskTypeDTO ToDTO(this IUnitTaskType c)
		{
			if (c == null) return null;

			var dto = new UnitTaskTypeDTO();
			SetBaseDTOValues(dto, c);
			return dto;
		}

		public static MissionTypeDTO ToDTO(this IMissionType c)
		{
			if (c == null) return null;

			var dto = new MissionTypeDTO();
			SetBaseDTOValues(dto, c);
			dto.Priority = c.Priority;
			dto.TurnOrder = c.TurnOrder;
			dto.CanceledByMove = c.CanceledByMove;
			return dto;
		}

		public static UnitTaskDTO ToDTO(this IUnitTask c, IMission mission)
		{
			if (c == null) return null;

			var dto = new UnitTaskDTO();
			SetBaseDTOValues(dto, c);
			dto.UnitTaskType = c.TaskType.ID;
			dto.TurnsToComplete = c.TurnsToComplete;
			dto.Mission = mission.ID; 
			return dto;
		}

		public static MissionDTO ToDTO(this IMission c, ITactic tactic)
		{
			if (c == null) return null;

			var dto = new MissionDTO();
			SetBaseDTOValues(dto, c);
			dto.CurrentTask = c.CurrentTask.ID;
			dto.MissionType = c.MissionType.ID;
			dto.Unit = c.GetAssignedUnit().ID;
			dto.Tactic = tactic.ID;
			return dto;
		}

		public static TacticDTO ToDTO(this ITactic c)
		{
			if (c == null) return null;

			var dto = new TacticDTO();
			SetBaseDTOValues(dto, c);
			dto.Player = c.Player.ID;
			dto.Stance = (int)c.Stance;
			return dto;
		}

#endregion	


		private static void SetBaseDTOValues(IBaseGameComponentDTO dto, IBaseComponent component)
		{
			dto.ID = component.ID;
			dto.UID = component.UID;
			dto.Name = component.Name;
			dto.Description = component.Description;
		}

	}

}
