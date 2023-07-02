using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Dynamic;
using JTacticalSim.API.Game;
using JTacticalSim.API.Service;
using JTacticalSim.API.Component;
using JTacticalSim.API;
using JTacticalSim.API.Cache;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Data;
using JTacticalSim.Component.GameBoard;

namespace JTacticalSim.DataContext.Repository
{
	public class XMLDataRepository : IDataRepository
	{
		private IGameCacheDependencies Cache { get { return JTacticalSim.Cache.GameCache.Instance; } }

		static readonly object padlock = new object();

		private static volatile IDataRepository _instance;
		public static IDataRepository Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new XMLDataRepository();
				}	
			
				return _instance;
			}
		}

		private XMLDataRepository()
		{ }


		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


		// Code Data

		public IBasePointValues GetGameBasePointValues()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.BasePointValues;
			}
		}

		public GameboardAttributeInfo GetGameboardAttributes()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return new GameboardAttributeInfo
				{
					Title = ctx.Board.Title,
					Height = ctx.Board.Height,
					Width = ctx.Board.Width,
					DrawHeight = ctx.Board.DrawHeight,
					DrawWidth = ctx.Board.DrawWidth,
					CellSize = ctx.Board.CellSize,
					CellMeters = ctx.Board.CellMeters,
					CellMaxUnits = ctx.Board.CellMaxUnits
				};
			}
		}


	// Lookups

		public List<int> GetHybridDemographicsClasses()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.HybridDemographicClasses;
			}
		}


		public IEnumerable<dynamic> GetUnitAssignments()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.UnitAssignments;
			}
		}

		public IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> SaveUnitAssignment(IUnit unit, IUnit assignToUnit)
		{
			var result = new OperationResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>>();
			var currentResultObject = new Tuple<IUnit, IUnit>(unit, assignToUnit);

			result.Status = ResultStatus.SUCCESS;

			using (var ctx = XMLDataContext.Instance)
			{
				try
				{
					//verify that the assignment doesn't exist already
					if (!ctx.UnitAssignments.Any(o => o.Unit == unit.ID && o.AssignedToUnit == assignToUnit.ID))
					{
						dynamic d = new ExpandoObject();
						d.Unit = unit.ID;
						d.AssignedToUnit = assignToUnit.ID;
						ctx.UnitAssignments.Add(d);
					}

					result.SuccessfulObjects.Add(currentResultObject);

				}
				catch (Exception ex)
				{
					result.ex = ex;
					result.FailedObjects.Add(currentResultObject);
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("Unit assignment was not saved.");
					return result;
				}
			}

			result.Messages.Add("Unit Assignment saved.");
			return result;
		}

		public IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> RemoveUnitAssignment(IUnit unit, IUnit assignedToUnit)
		{
			var result = new OperationResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>>();
			var currentResultObject = new Tuple<IUnit, IUnit>(unit, assignedToUnit);

			result.Status = ResultStatus.SUCCESS;

			using (var ctx = XMLDataContext.Instance)
			{
				try
				{
					var objToRemove = ctx.UnitAssignments
											.SingleOrDefault(ua => ua.Unit == unit.ID &&
																	ua.AssignedToUnit == assignedToUnit.ID);

					if (objToRemove == null) throw new Exception("Unit Assignment not found");

					ctx.UnitAssignments.Remove(objToRemove);
					result.SuccessfulObjects.Add(currentResultObject);

				}
				catch (Exception ex)
				{
					result.ex = ex;
					result.FailedObjects.Add(currentResultObject);
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("Unit assignment was not removed.");
					return result;
				}
			}

			result.Messages.Add("Unit Assignment removed.");
			return result;
		}

		public IResult<IUnit, IUnit> RemoveUnitAssignmentsFromUnit(IUnit unit)
		{
			var result = new OperationResult<IUnit, IUnit>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("Unit Assignments removed.");

			using (var ctx = XMLDataContext.Instance)
			{
				try
				{
					var objsToRemove = ctx.UnitAssignments.Where(ua => ua.AssignedToUnit == unit.ID).ToList();

					foreach (var u in objsToRemove)
					{
						try
						{
							ctx.UnitAssignments.Remove(u);
							result.SuccessfulObjects.Add(unit);
						}
						catch (Exception)
						{
							result.FailedObjects.Add(unit);
							result.Status = ResultStatus.FAILURE;
							result.Messages.Add("Some unit assignments were not removed. Check FailedObjects.");
						}
					}
				}
				catch (Exception ex)
				{
					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("Unit assignments were not removed.");
				}
			}

			return result;
		}

		
		public IEnumerable<dynamic> GetUnitTransports()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.UnitTransports;
			}
		}

		public IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> SaveUnitTransport(IUnit unit, IUnit transport)
		{
			var result = new OperationResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>>();
			var currentResultObject = new Tuple<IUnit, IUnit>(unit, transport);

			result.Status = ResultStatus.SUCCESS;

			using (var ctx = XMLDataContext.Instance)
			{
				try
				{
					//verify that the transport record doesn't exist already
					if (!ctx.UnitTransports.Any(o => o.Unit == unit.ID && o.TransportUnit == transport.ID))
					{
						dynamic d = new ExpandoObject();
						d.Unit = unit.ID;
						d.TransportUnit = transport.ID;
						ctx.UnitTransports.Add(d);
					}

					result.SuccessfulObjects.Add(currentResultObject);

				}
				catch (Exception ex)
				{
					result.ex = ex;
					result.FailedObjects.Add(currentResultObject);
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("UnitTransport failed to save.");
					return result;
				}
			}

			result.Messages.Add("Unit Transport saved.");
			return result;
		}

		public IResult<Tuple<IUnit, IUnit>,Tuple<IUnit, IUnit>> RemoveUnitTransport(IUnit unit, IUnit transport)
		{
			var result = new OperationResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>>();
			var currentResultObject = new Tuple<IUnit, IUnit>(unit, transport);

			result.Status = ResultStatus.SUCCESS;

			using (var ctx = XMLDataContext.Instance)
			{
				try
				{
					var objToRemove = ctx.UnitTransports
											.SingleOrDefault(ua => ua.Unit == unit.ID &&
																	ua.TransportUnit == transport.ID);

					if (objToRemove == null) throw new Exception("Unit transport not found");

					ctx.UnitTransports.Remove(objToRemove);
					result.SuccessfulObjects.Add(currentResultObject);

				}
				catch (Exception ex)
				{
					result.ex = ex;
					result.FailedObjects.Add(currentResultObject);
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("Unit transport record was not removed.");
					return result;
				}
			}

			result.Messages.Add("Unit transport record removed.");
			return result;
		}


		public IEnumerable<dynamic> GetAllowedUnitTypes()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.AllowedUnitTypes;
			}
		}

		public IEnumerable<dynamic> GetAllowedUnitGroupTypes()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.AllowedUnitGroupTypes;
			}
		}


		public IEnumerable<dynamic> GetFactionVictoryConditions()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.FactionVictoryConditions;
			}
		}

		public IResult<Tuple<IFaction, IVictoryCondition>, Tuple<IFaction, IVictoryCondition>> SaveFactionVictoryConditions(IFaction faction, IVictoryCondition victoryCondition)
		{
			var result = new OperationResult<Tuple<IFaction, IVictoryCondition>, Tuple<IFaction, IVictoryCondition>>();
			var currentResultObject = new Tuple<IFaction, IVictoryCondition>(faction, victoryCondition);

			result.Status = ResultStatus.SUCCESS;

			using (var ctx = XMLDataContext.Instance)
			{
				try
				{
					if (!ctx.FactionVictoryConditions.Any(o => o.FactionID == faction.ID && o.ConditionType == victoryCondition.VictoryConditionType.ID))
					{
						dynamic d = new ExpandoObject();
						d.FactionID = faction.ID;
						d.ConditionType = victoryCondition.VictoryConditionType.ID;
						d.Value = victoryCondition.Value;
						ctx.FactionVictoryConditions.Add(d);

						result.Messages.Add("FactionVictoryCondition saved.");
						result.SuccessfulObjects.Add(currentResultObject);
					}
					else
					{
						result.FailedObjects.Add(currentResultObject);
						result.Status = ResultStatus.FAILURE;
						result.Messages.Add("FactionVictoryCondition must be unique by faction/condition type.");
					}

				}
				catch (Exception ex)
				{
					result.ex = ex;
					result.FailedObjects.Add(currentResultObject);
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("FactionVictoryCondition failed to save.");
					return result;
				}
			}

			return result;
		}

		public IResult<Tuple<IFaction, IVictoryCondition>, Tuple<IFaction, IVictoryCondition>> RemoveFactionVictoryConditions(IFaction faction, IVictoryCondition victoryCondition)
		{
			var result = new OperationResult<Tuple<IFaction, IVictoryCondition>, Tuple<IFaction, IVictoryCondition>>();
			var currentResultObject = new Tuple<IFaction, IVictoryCondition>(faction, victoryCondition);

			result.Status = ResultStatus.SUCCESS;

			using (var ctx = XMLDataContext.Instance)
			{
				try
				{
					var objToRemove = ctx.FactionVictoryConditions
											.SingleOrDefault(fvc => fvc.FactionID == faction.ID &&
																	fvc.ConditionType == victoryCondition.VictoryConditionType.ID);

					if (objToRemove == null) throw new Exception("FactionVictoryCondition not found");

					ctx.FactionVictoryConditions.Remove(objToRemove);
					result.SuccessfulObjects.Add(currentResultObject);

				}
				catch (Exception ex)
				{
					result.ex = ex;
					result.FailedObjects.Add(currentResultObject);
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("UFactionVictoryCondition was not removed.");
					return result;
				}
			}

			result.Messages.Add("FactionVictoryCondition removed.");
			return result;
		}



		public IEnumerable<dynamic> GetUnitBaseTypeUnitGeogTypesLookup()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.UnitBaseTypeUnitGeogTypesLookup;
			}
		}
		
		public IEnumerable<dynamic> GetUnitGeogTypeDemographicClassesLookup()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.UnitGeogTypeDemographicClassesLookup;
			}
		}

		public IEnumerable<dynamic> GetUnitBaseTypeUnitClassesLookup()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.UnitBaseTypeUnitClassesLookup;
			}
		}

		public IEnumerable<dynamic> GetUnitTaskTypeUnitClassesLookup()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.UnitTaskTypeUnitClassesLookup;
			}
		}

		public IEnumerable<dynamic> GetUnitGroupTypeUnitTaskTypeLookup()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.UnitGroupTypeUnitTaskTypeLookup;
			}
		}

		public IEnumerable<dynamic> GetUnitGeogTypeMovementOverrides()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.UnitGeogTypeMovementOverrides;
			}
		}

		public IEnumerable<dynamic> GetUnitBattleEffectiveLookup()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.UnitBattleEffectiveLookup;
			}
		}

		public IEnumerable<dynamic> GetUnitTransportUnitTypeUnitClasses()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.UnitTransportUnitTypeUnitClasses;
			}
		}

		public IEnumerable<dynamic> GetMissionTypeUnitTaskTypes()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.MissionTypeUnitTaskType;
			}
		}

		public IEnumerable<dynamic> GetMovementHinderanceInDirection()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.MovementHinderanceInDirection;
			}
		}
	}
}
