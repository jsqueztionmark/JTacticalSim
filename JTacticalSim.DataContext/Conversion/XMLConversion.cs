using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.AI;
using JTacticalSim.Data.DTO;
using JTacticalSim.Component.AI;
using System.Xml.Linq;

namespace JTacticalSim.DataContext
{
	public static class XMLConversion
	{
		public static XElement ToXML(this NodeDTO dto)
		{
			XElement x = new XElement("Node");
			x.Add(new XAttribute("ID", dto.ID));
			XElement l = new XElement("Location");
			l.Add(dto.Location.ToXML());
			x.Add(l);
			
			XElement c = new XElement("Country");
			c.Add(new XAttribute("ID", dto.Country));
			x.Add(c);

			XElement dt = new XElement("DefaultTile");
			dt.Add(dto.DefaultTile.ToXML());
			x.Add(dt);

			return x;
		}

		public static XElement ToXML(this UnitDTO dto)
		{
			XElement x = new XElement("Unit");
			x.Add(new XAttribute("ID", dto.ID));
			x.Add(new XAttribute("Name", dto.Name));
			x.Add(new XAttribute("Description", dto.Description));
			x.Add(new XAttribute("StackOrder", dto.StackOrder));
			x.Add(new XAttribute("CurrentFuelRange", Convert.ToInt32(dto.CurrentFuelRange)));

			XElement l = new XElement("Location");

			// Null location indicates an unplaced unit
			if (dto.Location != null)
			{				
				l.Add(dto.Location.ToXML());		
			}

			XElement snl = new XElement("SubNodeLocation");
			snl.Add(new XAttribute("Value", dto.SubNodeLocation));
			l.Add(snl);	
			x.Add(l);
			

			XElement ui = new XElement("UnitInfo");
			ui.Add(new XAttribute("UnitType", dto.UnitType));
			ui.Add(new XAttribute("UnitClass", dto.UnitClass));
			ui.Add(new XAttribute("UnitGroupType", dto.UnitGroupType));
			x.Add(ui);

			XElement p = new XElement("Posture");
			p.Add(new XAttribute("ID", dto.Posture ?? (int)BattlePosture.STANDARD));
			x.Add(p);

			XElement ms = new XElement("MovementStats");
			ms.Add(new XAttribute("CurrentMovementPoints", Convert.ToInt32(dto.CurrentMovementPoints)));
			ms.Add(new XAttribute("CurrentHasPerformedAction", Convert.ToBoolean(dto.CurrentHasPerformedAction)));
			ms.Add(new XAttribute("CurrentRemoteFirePoints", Convert.ToInt32(dto.CurrentRemoteFirePoints)));
			x.Add(ms);

			XElement c = new XElement("Country");
			c.Add(new XAttribute("ID", dto.Country));
			x.Add(c);

			return x;
		}

		public static XElement ToXML(this CoordinateDTO dto)
		{
			XElement x = new XElement("Coordinate");
			x.Add(new XAttribute("X", dto.X));
			x.Add(new XAttribute("Y", dto.Y));
			x.Add(new XAttribute("Z", dto.Z));

			return x;
		}

		public static XElement ToXML(this CountryDTO dto)
		{
			XElement x = new XElement("Country");
			x.Add(new XAttribute("ID", dto.ID));
			x.Add(new XAttribute("Name", dto.Name));
			x.Add(new XAttribute("Description", dto.Description));
            x.Add(new XAttribute("Color", dto.Color));
			x.Add(new XAttribute("BGColor", dto.BGColor));
			x.Add(new XAttribute("TextDisplayColor", dto.TextDisplayColor));
            x.Add(new XAttribute("FlagDisplayTextA", dto.FlagDisplayTextA));
			x.Add(new XAttribute("FlagDisplayTextB", dto.FlagDisplayTextB));
			x.Add(new XAttribute("FlagBGColor", dto.FlagBGColor));
			x.Add(new XAttribute("FlagColorA", dto.FlagColorA));
			x.Add(new XAttribute("FlagColorB", dto.FlagColorB));
			x.Add(new XAttribute("Faction", dto.Faction.ID));

			return x;
		}

		public static XElement ToXML(this FactionDTO dto)
		{
			XElement x = new XElement("Faction");
			x.Add(new XAttribute("ID", dto.ID));
			x.Add(new XAttribute("Name", dto.Name));
			x.Add(new XAttribute("Description", dto.Description));

			return x;
		}

		public static XElement ToXML(this PlayerDTO dto)
		{
			XElement x = new XElement("Player");
			x.Add(new XAttribute("ID", dto.ID));
			x.Add(new XAttribute("Name", dto.Name));
			x.Add(new XAttribute("Description", dto.Description));
			x.Add(new XAttribute("Country", dto.Country));
			x.Add(new XAttribute("ReinforcementPoints", dto.ReinforcementPoints ?? 0));
			x.Add(new XAttribute("IsCurrentPlayer", dto.IsCurrentPlayer));
			x.Add(new XAttribute("IsAIPlayer", dto.IsAIPlayer));
			
			XElement tv = new XElement("TrackedValues");
			tv.Add(new XAttribute("ReinforcementPoints", dto.ReinforcementPoints));
			tv.Add(new XAttribute("NuclearCharges", dto.NuclearCharges));
			x.Add(tv);

			XElement unrfcmts = new XElement("UnplacedReinforcements");
			foreach (var i in dto.UnplacedReinforcements)
			{
				XElement unit = new XElement("Unit");
				unit.Add(new XAttribute("ID", i));
				unrfcmts.Add(unit);
			}
			x.Add(unrfcmts);
			
			return x;
		}

		public static XElement ToXML(this TileDTO dto)
		{
			XElement x = new XElement("Tile");
			x.Add(new XAttribute("VictoryPoints", dto.VictoryPoints));
			x.Add(new XAttribute("IsPrimeTarget", dto.IsPrimeTarget.ToString()));
			x.Add(new XAttribute("Name", dto.Name));
			x.Add(new XAttribute("ID", dto.ID));

			XElement ds = new XElement("Demographics");

			dto.Demographics.ToList().ForEach(d =>
				{
					ds.Add(d.ToXML());
				});

			x.Add(ds);

			return x;
		}

		public static XElement ToXML(this DemographicDTO dto)
		{
			XElement x = new XElement("Demographic");
			x.Add(new XAttribute("ID", dto.ID));
			x.Add(new XAttribute("InstanceName", dto.InstanceName ?? string.Empty));
			x.Add(new XAttribute("Orientation", dto.Orientation));

			return x;
		}

		public static XElement ToXML(this ScenarioDTO dto)
		{
			XElement x = new XElement("Scenario");
			x.Add(new XAttribute("ID", dto.ID));
			x.Add(new XAttribute("Name", dto.Name));
			x.Add(new XAttribute("GameFileDirectory", dto.GameFileDirectory));
			x.Add(new XAttribute("ComponentSet", dto.ComponentSet));
			x.Add(new XAttribute("Author", dto.Author));

			return x;
		}

		public static XElement ToXML(this SavedGameDTO dto)
		{
			XElement x = new XElement("SavedGame");
			x.Add(new XAttribute("ID", dto.ID));
			x.Add(new XAttribute("Name", dto.Name));
			x.Add(new XAttribute("GameFileDirectory", dto.GameFileDirectory));
			x.Add(new XAttribute("LastPlayed", dto.LastPlayed));
			x.Add(new XAttribute("Scenario", dto.Scenario));

			return x;
		}

#region StrategyCache

		// Cache is stored in components, not dtos

		public static XElement ToXml(this ITactic c)
		{
			XElement x = new XElement("Tactic");
			x.Add(new XAttribute("Player", c.Player.ID));
			x.Add(new XAttribute("Stance", (int)c.Stance));

			XElement missions = new XElement("Missions");

			c.GetChildComponents().ToList().ForEach(mission =>
				{
					missions.Add(mission.ToXML());
				});

			x.Add(missions);

			foreach (var mission in c.GetChildComponents())
			{
				XElement xMission = mission.ToXML();
			}

			return x;
		}

		public static XElement ToXML(this IMission c)
		{
			XElement x = new XElement("Mission");
			x.Add(new XAttribute("CurrentTaskTaskType", c.CurrentTask.TaskType.ID));	// Tasks do not have an ID. We'll assume unique tasktypes per mission for now			
			x.Add(new XAttribute("MissionType", c.MissionType.ID));
			x.Add(new XAttribute("Unit", c.GetAssignedUnit().ID));

			XElement tasks = new XElement("UnitTasks");

			c.GetChildComponents().ToList().ForEach(task =>
				{
					tasks.Add(task.ToXML());
				});

			x.Add(tasks);

			return x;
		}

		public static XElement ToXML(this IUnitTask c)
		{
			XElement x = new XElement("UnitTask");
			x.Add(new XAttribute("TurnsToComplete", c.TurnsToComplete));
			x.Add(new XAttribute("TaskType", c.TaskType.ID));;

			XElement args = new XElement("Args");

			c.Args.ToList().ForEach(arg =>
				{
					args.Add(arg.ToXML());
				});

			x.Add(args);

			return x;
		}

		public static XElement ToXML(this TaskExecutionArgument c)
		{
			XElement x = new XElement("TaskExecutionArgument");
			x.Add(new XAttribute("Assembly", c.Assembly ?? string.Empty));
			x.Add(new XAttribute("Type", c.Type));
			x.Add(new XAttribute("Name", c.Name));

			c.Values.ToList().ForEach(v => 
							{
								var xValue = new XElement("ArgumentValue");
								xValue.Add(new XAttribute("Value", v.ToString()));
								x.Add(xValue);
							});	
			return x;
		}

#endregion

	}
}
