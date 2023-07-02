using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Dynamic;
using System.Threading.Tasks;
using JTacticalSim.Data.DTO;
using JTacticalSim.API.Game;
using JTacticalSim.API.Service;
using JTacticalSim.API.Component;
using JTacticalSim.API.Cache;
using JTacticalSim.API.AI;
using JTacticalSim.API;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Data;
using JTacticalSim.Component.GameBoard;
using JTacticalSim.Component.AI;
using JTacticalSim.Utility;


namespace JTacticalSim.DataContext.Repository
{
	public class XMLComponentRepository : IComponentRepository
	{
		private IGameCacheDependencies Cache
		{
			get { return JTacticalSim.Cache.GameCache.Instance; }
		}

		private static readonly object padlock = new object();

		private static volatile IComponentRepository _instance;

		public static IComponentRepository Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new XMLComponentRepository();
				}

				return _instance;
			}
		}

		private XMLComponentRepository()
		{
		}


		public IEnumerable<NodeDTO> GetNodes()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				var retVal = ctx.NodesTable.Records.Cast<NodeDTO>();
				return retVal;
			}
		}

		public IResult<INode, INode> SaveNodes(List<INode> nodes)
		{
			var result = new OperationResult<INode, INode>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Nodes saved.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var n in nodes)
				{
					try
					{
						//verify that a node doesn't exist at that location already
						if (ctx.NodesTable.Records.All(dto => dto.UID != n.UID))
							ctx.NodesTable.Records.Add(n.ToDTO());

						// Save to cache
						Cache.NodeCache.TryAdd(n.UID, n);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(n);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Nodes saved.");
						return result;
					}

					result.SuccessfulObjects.Add(n);

				}
			}

			return result;
		}

		public IResult<INode, INode> RemoveNodes(List<INode> nodes)
		{
			var result = new OperationResult<INode, INode>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Nodes removed.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in nodes)
				{
					try
					{
						if (ctx.NodesTable.Records.Any(dto => dto.UID == o.UID))
							ctx.NodesTable.Records.Remove(ctx.NodesTable.Records.Single(dto => dto.UID == o.UID));

						// Remove from cache
						Cache.NodeCache.TryRemove(o.UID);

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Nodes removed.");
					}
				}
			}

			return result;
		}

		public IResult<INode, INode> UpdateNodes(List<INode> nodes)
		{
			var result = new OperationResult<INode, INode>();

			result.Status = ResultStatus.SUCCESS;

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = RemoveNodes(nodes);
					if (removeResult.Status == ResultStatus.EXCEPTION) throw removeResult.ex;

					var saveResult = SaveNodes(nodes);
					if (saveResult.Status == ResultStatus.EXCEPTION) throw saveResult.ex;

					// Update in cache
					foreach (var n in nodes)
					{
						Cache.NodeCache.TryUpdate(n.UID, n);
						result.SuccessfulObjects.Add(n);
					}

				}
				catch (Exception ex)
				{
					nodes.ForEach(n => result.FailedObjects.Add(n));

					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("No Nodes were updated.");
					txn.Dispose();
					return result;
				}

				txn.Complete();
			}

			result.Messages.Add("All Nodes updated.");
			return result;
		}


		public IEnumerable<UnitDTO> GetUnits()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.UnitsTable.Records.Cast<UnitDTO>();
			}
		}

		public IResult<IUnit, IUnit> CreateUnit(string name,
		                                        ICoordinate coordinate,
		                                        ICountry country,
		                                        UnitInfo unitInfo,
		                                        ISubNodeLocation subNodeLocation)
		{
			var result = new OperationResult<IUnit, IUnit>();

			result.Status = ResultStatus.SUCCESS;

			try
			{
				var unit = new Unit(name, coordinate, unitInfo) {Country = country, SubNodeLocation = subNodeLocation};
				result.SuccessfulObjects.Add(unit);
			}
			catch (Exception ex)
			{
				result.ex = ex;
				result.Status = ResultStatus.EXCEPTION;
				result.Messages.Add("Unit not created");
				return result;
			}

			result.Messages.Add("Unit Created");
			return result;
		}

		public IResult<IUnit, IUnit> SaveUnits(List<IUnit> units)
		{
			var result = new OperationResult<IUnit, IUnit>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Units saved.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in units)
				{
					try
					{
						//verify that the unit doesn't exist already
						if (ctx.UnitsTable.Records.Cast<IBaseGameComponentDTO>().All(dto => dto.UID != o.UID))
							ctx.UnitsTable.Records.Add(o.ToDTO());

						// Add to cache
						// Save to cache
						Cache.UnitCache.TryAdd(o.UID, o);

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Units saved.");
					}
				}
			}

			return result;
		}

		public IResult<IUnit, IUnit> RemoveUnits(List<IUnit> units)
		{
			var result = new OperationResult<IUnit, IUnit>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Units removed.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in units)
				{
					try
					{
						using (var txn = new TransactionScope())
						{
							// Remove units
							if (ctx.UnitsTable.Records.Any(dto => dto.UID == o.UID))
								ctx.UnitsTable.Records.Remove(ctx.UnitsTable.Records.Single(dto => dto.UID == o.UID));

							// Remove from cache
							Cache.UnitCache.TryRemove(o.UID);

							txn.Complete();
						}

						result.SuccessfulObjects.Add(o);
						result.Result = o;
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Units removed.");
					}
				}
			}

			return result;
		}

		public IResult<IUnit, IUnit> UpdateUnits(List<IUnit> units)
		{
			var result = new OperationResult<IUnit, IUnit>();

			result.Status = ResultStatus.SUCCESS;

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = RemoveUnits(units);
					if (removeResult.Status == ResultStatus.EXCEPTION) throw removeResult.ex;

					var saveResult = SaveUnits(units);
					if (saveResult.Status == ResultStatus.EXCEPTION) throw saveResult.ex;

					// Update cache
					foreach (var u in units)
					{
						Cache.UnitCache.TryUpdate(u.UID, u);
						Cache.TurnMoveCache.TryUpdate(u.UID, u.CurrentMoveStats);
						result.SuccessfulObjects.Add(u);
					}
				}
				catch (Exception ex)
				{
					units.ForEach(u => result.FailedObjects.Add(u));

					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("No Units were updated.");
					txn.Dispose();
					return result;
				}

				txn.Complete();
			}

			result.Messages.Add("All Units updated.");
			return result;
		}


		public IEnumerable<UnitClassDTO> GetUnitClasses()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.UnitClassesTable.Records.Cast<UnitClassDTO>();
			}
		}

		public IResult<IUnitClass, IUnitClass> SaveUnitClasses(List<IUnitClass> unitClasses)
		{
			var result = new OperationResult<IUnitClass, IUnitClass>();
			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Unit Classes saved.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in unitClasses)
				{
					try
					{
						//verify that the unit class doesn't exist already
						if (ctx.UnitClassesTable.Records.Cast<IBaseGameComponentDTO>().All(dto => dto.UID != o.UID))
							ctx.UnitClassesTable.Records.Add(o.ToDTO());

					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Unit Classes saved.");
					}
				}
			}

			return result;
		}

		public IResult<IUnitClass, IUnitClass> RemoveUnitClasses(List<IUnitClass> unitClasses)
		{
			var result = new OperationResult<IUnitClass, IUnitClass>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Unit Classes removed.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in unitClasses)
				{
					try
					{
						using (var txn = new TransactionScope())
						{
							// Remove unit classes
							if (ctx.UnitClassesTable.Records.Any(dto => dto.UID == o.UID))
								ctx.UnitClassesTable.Records.Remove(ctx.UnitClassesTable.Records.Single(dto => dto.UID == o.UID));

							txn.Complete();
						}

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Unit Classes removed.");
					}
				}
			}

			return result;
		}

		public IResult<IUnitClass, IUnitClass> UpdateUnitClasses(List<IUnitClass> unitClasses)
		{
			var result = new OperationResult<IUnitClass, IUnitClass>();

			result.Status = ResultStatus.SUCCESS;

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = RemoveUnitClasses(unitClasses);
					if (removeResult.Status == ResultStatus.EXCEPTION) throw removeResult.ex;

					var saveResult = SaveUnitClasses(unitClasses);
					if (saveResult.Status == ResultStatus.EXCEPTION) throw saveResult.ex;

					unitClasses.ForEach(uc => result.SuccessfulObjects.Add(uc));

				}
				catch (Exception ex)
				{
					unitClasses.ForEach(uc => result.FailedObjects.Add(uc));

					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("No unit classes were updated.");
					txn.Dispose();
					return result;
				}

				txn.Complete();
			}

			result.Messages.Add("All Unit classes updated.");
			return result;
		}


		public IEnumerable<UnitTypeDTO> GetUnitTypes()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.UnitTypesTable.Records.Cast<UnitTypeDTO>();
			}
		}

		public IResult<IUnitType, IUnitType> SaveUnitTypes(List<IUnitType> unitTypes)
		{
			var result = new OperationResult<IUnitType, IUnitType>();
			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Unit Types saved.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in unitTypes)
				{
					try
					{
						if (ctx.UnitTypesTable.Records.Cast<IBaseGameComponentDTO>().All(dto => dto.UID != o.UID))
							ctx.UnitTypesTable.Records.Add(o.ToDTO());

					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Unit Types saved.");
					}
				}
			}

			return result;
		}

		public IResult<IUnitType, IUnitType> RemoveUnitTypes(List<IUnitType> unitTypes)
		{
			var result = new OperationResult<IUnitType, IUnitType>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Unit Types removed.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in unitTypes)
				{
					try
					{
						using (var txn = new TransactionScope())
						{
							if (ctx.UnitTypesTable.Records.Any(dto => dto.UID == o.UID))
								ctx.UnitTypesTable.Records.Remove(ctx.UnitTypesTable.Records.Single(dto => dto.UID == o.UID));

							txn.Complete();
						}

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Unit Types removed.");
					}
				}
			}

			return result;
		}

		public IResult<IUnitType, IUnitType> UpdateUnitTypes(List<IUnitType> unitTypes)
		{
			var result = new OperationResult<IUnitType, IUnitType>();

			result.Status = ResultStatus.SUCCESS;

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = RemoveUnitTypes(unitTypes);
					if (removeResult.Status == ResultStatus.EXCEPTION) throw removeResult.ex;

					var saveResult = SaveUnitTypes(unitTypes);
					if (saveResult.Status == ResultStatus.EXCEPTION) throw saveResult.ex;

					unitTypes.ForEach(uc => result.SuccessfulObjects.Add(uc));

				}
				catch (Exception ex)
				{
					unitTypes.ForEach(uc => result.FailedObjects.Add(uc));

					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("No unit types were updated.");
					txn.Dispose();
					return result;
				}

				txn.Complete();
			}

			result.Messages.Add("All unit types updated.");
			return result;
		}


		public IEnumerable<UnitBaseTypeDTO> GetUnitBaseTypes()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.UnitBaseTypesTable.Records.Cast<UnitBaseTypeDTO>();
			}
		}

		public IResult<IUnitBaseType, IUnitBaseType> SaveUnitBaseTypes(List<IUnitBaseType> unitBaseTypes)
		{
			var result = new OperationResult<IUnitBaseType, IUnitBaseType>();
			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Unit Base Types saved.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in unitBaseTypes)
				{
					try
					{
						if (ctx.UnitBaseTypesTable.Records.All(dto => dto.UID != o.UID))
							ctx.UnitBaseTypesTable.Records.Add(o.ToDTO());

					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Unit Base Types saved.");
					}
				}
			}

			return result;
		}

		public IResult<IUnitBaseType, IUnitBaseType> RemoveUnitBaseTypes(List<IUnitBaseType> unitBaseTypes)
		{
			var result = new OperationResult<IUnitBaseType, IUnitBaseType>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Unit Base Types removed.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in unitBaseTypes)
				{
					try
					{
						using (var txn = new TransactionScope())
						{
							if (ctx.UnitBaseTypesTable.Records.Any(dto => dto.UID == o.UID))
								ctx.UnitBaseTypesTable.Records.Remove(ctx.UnitBaseTypesTable.Records.Single(dto => dto.UID == o.UID));

							txn.Complete();
						}

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Unit Base Types removed.");
					}
				}
			}

			return result;
		}

		public IResult<IUnitBaseType, IUnitBaseType> UpdateUnitBaseTypes(List<IUnitBaseType> unitBaseTypes)
		{
			var result = new OperationResult<IUnitBaseType, IUnitBaseType>();

			result.Status = ResultStatus.SUCCESS;

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = RemoveUnitBaseTypes(unitBaseTypes);
					if (removeResult.Status == ResultStatus.EXCEPTION) throw removeResult.ex;

					var saveResult = SaveUnitBaseTypes(unitBaseTypes);
					if (saveResult.Status == ResultStatus.EXCEPTION) throw saveResult.ex;

					unitBaseTypes.ForEach(o => result.SuccessfulObjects.Add(o));
				}
				catch (Exception ex)
				{
					unitBaseTypes.ForEach(o => result.FailedObjects.Add(o));

					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("No unit base types were updated.");
					txn.Dispose();
					return result;
				}

				txn.Complete();
			}

			result.Messages.Add("All unit base types updated.");
			return result;
		}



		public IEnumerable<UnitGroupTypeDTO> GetUnitGroupTypes()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.UnitGroupTypesTable.Records.Cast<UnitGroupTypeDTO>();
			}
		}

		public IResult<IUnitGroupType, IUnitGroupType> SaveUnitGroupTypes(List<IUnitGroupType> unitGroupTypes)
		{
			var result = new OperationResult<IUnitGroupType, IUnitGroupType>();
			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Unit Group Types saved.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in unitGroupTypes)
				{
					try
					{
						if (ctx.UnitGroupTypesTable.Records.All(dto => dto.UID != o.UID))
							ctx.UnitGroupTypesTable.Records.Add(o.ToDTO());

					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Unit Group Types saved.");
					}
				}
			}

			return result;
		}

		public IResult<IUnitGroupType, IUnitGroupType> RemoveUnitGroupTypes(List<IUnitGroupType> unitGroupTypes)
		{
			var result = new OperationResult<IUnitGroupType, IUnitGroupType>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Unit Group Types removed.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in unitGroupTypes)
				{
					try
					{
						using (var txn = new TransactionScope())
						{
							if (ctx.UnitGroupTypesTable.Records.Any(dto => dto.UID == o.UID))
								ctx.UnitGroupTypesTable.Records.Remove(ctx.UnitGroupTypesTable.Records.Single(dto => dto.UID == o.UID));

							txn.Complete();
						}

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Unit Group Types removed.");
					}
				}
			}

			return result;
		}

		public IResult<IUnitGroupType, IUnitGroupType> UpdateUnitGroupTypes(List<IUnitGroupType> unitGroupTypes)
		{
			var result = new OperationResult<IUnitGroupType, IUnitGroupType>();

			result.Status = ResultStatus.SUCCESS;

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = RemoveUnitGroupTypes(unitGroupTypes);
					if (removeResult.Status == ResultStatus.EXCEPTION) throw removeResult.ex;

					var saveResult = SaveUnitGroupTypes(unitGroupTypes);
					if (saveResult.Status == ResultStatus.EXCEPTION) throw saveResult.ex;

					unitGroupTypes.ForEach(o => result.SuccessfulObjects.Add(o));
				}
				catch (Exception ex)
				{
					unitGroupTypes.ForEach(o => result.FailedObjects.Add(o));

					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("No unit group types were updated.");
					txn.Dispose();
					return result;
				}

				txn.Complete();
			}

			result.Messages.Add("All unit group types updated.");
			return result;
		}


		public IEnumerable<UnitGeogTypeDTO> GetUnitGeogTypes()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.UnitGeogTypesTable.Records.Cast<UnitGeogTypeDTO>();
			}
		}

		public IResult<IUnitGeogType, IUnitGeogType> SaveUnitGeogTypes(List<IUnitGeogType> unitGeogTypes)
		{
			var result = new OperationResult<IUnitGeogType, IUnitGeogType>();
			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Unit Geog Types saved.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in unitGeogTypes)
				{
					try
					{
						if (ctx.UnitGeogTypesTable.Records.All(dto => dto.UID != o.UID))
							ctx.UnitGeogTypesTable.Records.Add(o.ToDTO());

					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Unit Geog Types saved.");
					}
				}
			}

			return result;
		}

		public IResult<IUnitGeogType, IUnitGeogType> RemoveUnitGeogTypes(List<IUnitGeogType> unitGeogTypes)
		{
			var result = new OperationResult<IUnitGeogType, IUnitGeogType>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Unit Geog Types removed.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in unitGeogTypes)
				{
					try
					{
						using (var txn = new TransactionScope())
						{
							if (ctx.UnitGeogTypesTable.Records.Any(dto => dto.UID == o.UID))
								ctx.UnitGeogTypesTable.Records.Remove(ctx.UnitGeogTypesTable.Records.Single(dto => dto.UID == o.UID));

							txn.Complete();
						}

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Unit Geog Types removed.");
					}
				}
			}

			return result;
		}

		public IResult<IUnitGeogType, IUnitGeogType> UpdateUnitGeogTypes(List<IUnitGeogType> unitGeogTypes)
		{
			var result = new OperationResult<IUnitGeogType, IUnitGeogType>();

			result.Status = ResultStatus.SUCCESS;

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = RemoveUnitGeogTypes(unitGeogTypes);
					if (removeResult.Status == ResultStatus.EXCEPTION) throw removeResult.ex;

					var saveResult = SaveUnitGeogTypes(unitGeogTypes);
					if (saveResult.Status == ResultStatus.EXCEPTION) throw saveResult.ex;

					unitGeogTypes.ForEach(o => result.SuccessfulObjects.Add(o));
				}
				catch (Exception ex)
				{
					unitGeogTypes.ForEach(o => result.FailedObjects.Add(o));

					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("No unit geog types were updated.");
					txn.Dispose();
					return result;
				}

				txn.Complete();
			}

			result.Messages.Add("All unit geog types updated.");
			return result;
		}


		public IEnumerable<FactionDTO> GetFactions()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.FactionsTable.Records.Cast<FactionDTO>();
			}
		}

		public IResult<IFaction, IFaction> SaveFactions(List<IFaction> factions)
		{
			var result = new OperationResult<IFaction, IFaction>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Factions saved.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in factions)
				{
					try
					{
						//verify that the faction doesn't exist already
						if (ctx.FactionsTable.Records.All(dto => dto.UID != o.UID))
							ctx.FactionsTable.Records.Add(o.ToDTO());

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Factions could be saved.");
					}
				}
			}

			return result;
		}

		public IResult<IFaction, IFaction> RemoveFactions(List<IFaction> factions)
		{
			var result = new OperationResult<IFaction, IFaction>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Factions removed.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in factions)
				{
					try
					{
						if (ctx.FactionsTable.Records.Any(dto => dto.UID == o.UID))
							ctx.FactionsTable.Records.Remove(ctx.FactionsTable.Records.Single(dto => dto.UID == o.UID));

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Factions could be removed.");
					}
				}
			}

			return result;
		}

		public IResult<IFaction, IFaction> UpdateFactions(List<IFaction> factions)
		{
			var result = new OperationResult<IFaction, IFaction>();

			result.Status = ResultStatus.SUCCESS;

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = RemoveFactions(factions);
					if (removeResult.Status == ResultStatus.EXCEPTION) throw removeResult.ex;

					var saveResult = SaveFactions(factions);
					if (saveResult.Status == ResultStatus.EXCEPTION) throw saveResult.ex;

					factions.ForEach(o => result.SuccessfulObjects.Add(o));
				}
				catch (Exception ex)
				{
					factions.ForEach(o => result.FailedObjects.Add(o));

					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("No Factions were updated.");
					txn.Dispose();
					return result;
				}

				txn.Complete();
			}

			result.Messages.Add("All Factions updated.");
			return result;
		}


		public IEnumerable<CountryDTO> GetCountries()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.CountriesTable.Records.Cast<CountryDTO>();
			}
		}

		public IResult<ICountry, ICountry> SaveCountries(List<ICountry> countries)
		{
			var result = new OperationResult<ICountry, ICountry>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Countries saved.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in countries)
				{
					try
					{
						//verify that the faction doesn't exist already
						if (ctx.CountriesTable.Records.All(dto => dto.UID != o.UID))
							ctx.CountriesTable.Records.Add(o.ToDTO());

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Countries saved.");
					}
				}
			}

			return result;
		}

		public IResult<ICountry, ICountry> RemoveCountries(List<ICountry> countries)
		{
			var result = new OperationResult<ICountry, ICountry>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Countries removed.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in countries)
				{
					try
					{
						if (ctx.CountriesTable.Records.Any(dto => dto.UID == o.UID))
							ctx.CountriesTable.Records.Remove(ctx.CountriesTable.Records.Single(dto => dto.UID == o.UID));

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Countries removed.");
					}
				}
			}

			return result;
		}

		public IResult<ICountry, ICountry> UpdateCountries(List<ICountry> countries)
		{
			var result = new OperationResult<ICountry, ICountry>();

			result.Status = ResultStatus.SUCCESS;

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = RemoveCountries(countries);
					if (removeResult.Status == ResultStatus.EXCEPTION) throw removeResult.ex;

					var saveResult = SaveCountries(countries);
					if (saveResult.Status == ResultStatus.EXCEPTION) throw saveResult.ex;

					countries.ForEach(o => result.SuccessfulObjects.Add(o));
				}
				catch (Exception ex)
				{
					countries.ForEach(o => result.FailedObjects.Add(o));

					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("No Countries updated.");
					txn.Dispose();
					return result;
				}

				txn.Complete();
			}

			result.Messages.Add("All Countries updated.");
			return result;
		}


		public IEnumerable<TileDTO> GetTiles()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.TilesTable.Records.Cast<TileDTO>();
			}
		}

		public IResult<ITile, ITile> SaveTiles(List<ITile> tiles)
		{
			var result = new OperationResult<ITile, ITile>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Tiles saved.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in tiles)
				{
					try
					{
						//verify that the tile doesn't exist already
						if (ctx.TilesTable.Records.All(dto => dto.UID != o.UID))
						{
							ctx.TilesTable.Records.Add(o.ToDTO());
						}
							

						// Add to cache
						Cache.TileCache.TryAdd(o.UID, o);

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Tiles could be saved.");
					}
				}
			}

			return result;
		}

		public IResult<ITile, ITile> RemoveTiles(List<ITile> tiles)
		{
			var result = new OperationResult<ITile, ITile>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Tiles removed.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in tiles)
				{
					try
					{
						if (ctx.TilesTable.Records.Any(dto => dto.UID == o.UID))
							ctx.TilesTable.Records.Remove(ctx.TilesTable.Records.Single(dto => dto.UID == o.UID));

						// Remove from cache
						Cache.TileCache.TryRemove(o.UID);

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Tiles could be removed.");
					}
				}
			}

			return result;
		}

		public IResult<ITile, ITile> UpdateTiles(List<ITile> tiles)
		{
			var result = new OperationResult<ITile, ITile>();

			result.Status = ResultStatus.SUCCESS;

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = RemoveTiles(tiles);
					if (removeResult.Status == ResultStatus.EXCEPTION) throw removeResult.ex;

					var saveResult = SaveTiles(tiles);
					if (saveResult.Status == ResultStatus.EXCEPTION) throw saveResult.ex;


					// Update cache
					foreach (var t in tiles) 
					{
						Cache.TileCache.TryUpdate(t.UID, t);
						result.SuccessfulObjects.Add(t);
					}
				}
				catch (Exception ex)
				{
					tiles.ForEach(t => result.SuccessfulObjects.Add(t));
					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("No Tiles were not updated.");
					txn.Dispose();
					return result;
				}

				txn.Complete();
			}

			result.Messages.Add("All Tiles updated.");
			return result;
		}


		public IEnumerable<PlayerDTO> GetPlayers()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.PlayersTable.Records.Cast<PlayerDTO>();
			}
		}

		public IResult<IPlayer, IPlayer> SavePlayers(List<IPlayer> players)
		{
			var result = new OperationResult<IPlayer, IPlayer>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Players saved.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (IPlayer o in players)
				{
					try
					{
						//verify that the faction doesn't exist already
						if (ctx.PlayersTable.Records.All(dto => dto.UID != o.UID))
							ctx.PlayersTable.Records.Add(o.ToDTO());

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Players could be saved.");
					}
				}
			}

			return result;
		}

		public IResult<IPlayer, IPlayer> RemovePlayers(List<IPlayer> players)
		{
			var result = new OperationResult<IPlayer, IPlayer>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Players removed.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in players)
				{
					try
					{
						if (ctx.PlayersTable.Records.Any(dto => dto.UID == o.UID))
							ctx.PlayersTable.Records.Remove(ctx.PlayersTable.Records.Single(dto => dto.UID == o.UID));

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Players could be removed.");
					}
				}
			}

			return result;
		}

		public IResult<IPlayer, IPlayer> UpdatePlayers(List<IPlayer> players)
		{
			var result = new OperationResult<IPlayer, IPlayer>();

			result.Status = ResultStatus.SUCCESS;

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = RemovePlayers(players);
					if (removeResult.Status == ResultStatus.EXCEPTION) throw removeResult.ex;

					var saveResult = SavePlayers(players);
					if (saveResult.Status == ResultStatus.EXCEPTION) throw saveResult.ex;

					players.ForEach(o => result.SuccessfulObjects.Add(o));
				}
				catch (Exception ex)
				{
					players.ForEach(o => result.FailedObjects.Add(o));

					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("No Players were updated.");
					txn.Dispose();
					return result;
				}

				txn.Complete();
			}

			result.Messages.Add("All Players updated.");
			return result;
		}


		public IEnumerable<DemographicDTO> GetDemographics()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.DemographicsTable.Records.Cast<DemographicDTO>();
			}
		}

		public IResult<IDemographic, IDemographic> SaveDemographics(List<IDemographic> demographics)
		{
			var result = new OperationResult<IDemographic, IDemographic>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Demographics saved.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in demographics)
				{
					try
					{
						//verify that the demographic doesn't exist already
						if (ctx.DemographicsTable.Records.All(dto => dto.UID != o.UID))
							ctx.DemographicsTable.Records.Add(o.ToDTO());

						// Add to cache
						Cache.DemographicCache.TryAdd(o.UID, o);

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Demographics could be saved.");
					}
				}
			}

			return result;
		}

		public IResult<IDemographic, IDemographic> RemoveDemographics(List<IDemographic> demographics)
		{
			var result = new OperationResult<IDemographic, IDemographic>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Demographics removed.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in demographics)
				{
					try
					{
						if (ctx.DemographicsTable.Records.Any(dto => dto.UID == o.UID))
							ctx.DemographicsTable.Records.Remove(ctx.DemographicsTable.Records.Single(dto => dto.UID == o.UID));

						// Remove from cache
						Cache.DemographicCache.TryRemove(o.UID);

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Demographics could be removed.");
					}
				}
			}

			return result;
		}

		public IResult<IDemographic, IDemographic> UpdateDemographics(List<IDemographic> demographics)
		{
			var result = new OperationResult<IDemographic, IDemographic>();

			result.Status = ResultStatus.SUCCESS;

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = RemoveDemographics(demographics);
					if (removeResult.Status == ResultStatus.EXCEPTION) throw removeResult.ex;

					var saveResult = SaveDemographics(demographics);
					if (saveResult.Status == ResultStatus.EXCEPTION) throw saveResult.ex;

					// Update cache
					foreach (var d in demographics) 
					{
						Cache.DemographicCache.TryUpdate(d.UID, d);
						result.SuccessfulObjects.Add(d);
					}
				}
				catch (Exception ex)
				{
					demographics.ForEach(o => result.SuccessfulObjects.Add(o));

					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("No Demographics were updated.");
					txn.Dispose();
					return result;
				}

				txn.Complete();
			}

			result.Messages.Add("All Demographics updated.");
			return result;
		}


		public IEnumerable<DemographicClassDTO> GetDemographicClasses()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.DemographicClassesTable.Records.Cast<DemographicClassDTO>();
			}
		}

		public IResult<IDemographicClass, IDemographicClass> SaveDemographicClasses(List<IDemographicClass> demographicClasses)
		{
			var result = new OperationResult<IDemographicClass, IDemographicClass>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Demographic Classes saved.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in demographicClasses)
				{
					try
					{
						if (ctx.DemographicClassesTable.Records.All(dto => dto.UID != o.UID))
							ctx.DemographicClassesTable.Records.Add(o.ToDTO());

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Demographic Classes saved.");
					}
				}
			}

			return result;
		}

		public IResult<IDemographicClass, IDemographicClass> RemoveDemographicClasses(List<IDemographicClass> demographicClasses)
		{
			var result = new OperationResult<IDemographicClass, IDemographicClass>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Demographic Classes removed.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in demographicClasses)
				{
					try
					{
						if (ctx.DemographicClassesTable.Records.Any(dto => dto.UID == o.UID))
							ctx.DemographicClassesTable.Records.Remove(ctx.DemographicClassesTable.Records.Single(dto => dto.UID == o.UID));

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Demographic Classes removed.");
					}
				}
			}

			return result;
		}

		public IResult<IDemographicClass, IDemographicClass> UpdateDemographicClasses(List<IDemographicClass> demographicClasses)
		{
			var result = new OperationResult<IDemographicClass, IDemographicClass>();

			result.Status = ResultStatus.SUCCESS;

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = RemoveDemographicClasses(demographicClasses);
					if (removeResult.Status == ResultStatus.EXCEPTION) throw removeResult.ex;

					var saveResult = SaveDemographicClasses(demographicClasses);
					if (saveResult.Status == ResultStatus.EXCEPTION) throw saveResult.ex;

					demographicClasses.ForEach(o => result.SuccessfulObjects.Add(o));
				}
				catch (Exception ex)
				{
					demographicClasses.ForEach(o => result.FailedObjects.Add(o));

					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("No Demographic Classes were updated.");
					txn.Dispose();
					return result;
				}

				txn.Complete();
			}

			result.Messages.Add("All Demographic Classes updated.");
			return result;
		}


		public IEnumerable<DemographicTypeDTO> GetDemographicTypes()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.DemographicTypesTable.Records.Cast<DemographicTypeDTO>();
			}
		}

		public IResult<IDemographicType, IDemographicType> SaveDemographicTypes(List<IDemographicType> demographicTypes)
		{
			var result = new OperationResult<IDemographicType, IDemographicType>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Demographic Types saved.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in demographicTypes)
				{
					try
					{
						if (ctx.DemographicTypesTable.Records.All(dto => dto.UID != o.UID))
							ctx.DemographicTypesTable.Records.Add(o.ToDTO());

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Demographic Types saved.");
					}
				}
			}

			return result;
		}

		public IResult<IDemographicType, IDemographicType> RemoveDemographicTypes(List<IDemographicType> demographicTypes)
		{
			var result = new OperationResult<IDemographicType, IDemographicType>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Demographic Types removed.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in demographicTypes)
				{
					try
					{
						if (ctx.DemographicTypesTable.Records.Any(dto => dto.UID == o.UID))
							ctx.DemographicTypesTable.Records.Remove(ctx.DemographicTypesTable.Records.Single(dto => dto.UID == o.UID));

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Demographic Types removed.");
					}
				}
			}

			return result;
		}

		public IResult<IDemographicType, IDemographicType> UpdateDemographicTypes(List<IDemographicType> demographicTypes)
		{
			var result = new OperationResult<IDemographicType, IDemographicType>();

			result.Status = ResultStatus.SUCCESS;

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = RemoveDemographicTypes(demographicTypes);
					if (removeResult.Status == ResultStatus.EXCEPTION) throw removeResult.ex;

					var saveResult = SaveDemographicTypes(demographicTypes);
					if (saveResult.Status == ResultStatus.EXCEPTION) throw saveResult.ex;

					demographicTypes.ForEach(o => result.SuccessfulObjects.Add(o));
				}
				catch (Exception ex)
				{
					demographicTypes.ForEach(o => result.FailedObjects.Add(o));

					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("No Demographic Types were updated.");
					txn.Dispose();
					return result;
				}

				txn.Complete();
			}

			result.Messages.Add("All Demographic Types updated.");
			return result;
		}


		public IEnumerable<SavedGameDTO> GetSavedGames()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.SavedGames.Records.Cast<SavedGameDTO>();
			}
		}

		public IResult<ISavedGame, ISavedGame> SaveSavedGame(ISavedGame game)
		{
			var result = new OperationResult<ISavedGame, ISavedGame> { Status = ResultStatus.SUCCESS, Result = game };

			using (var ctx = XMLDataContext.Instance)
			{
				try
				{
					//verify that the saved game doesn't exist already
					if (ctx.SavedGames.Records.All(dto => dto.UID != game.UID))
						ctx.SavedGames.Records.Add(game.ToDTO());
				}
				catch (Exception ex)
				{
					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("Game save failed.");
					return result;
				}
			}

			result.Messages.Add("Game Saved.");
			return result;
		}

		public IResult<ISavedGame, ISavedGame> RemoveSavedGame(ISavedGame game)
		{
			var result = new OperationResult<ISavedGame, ISavedGame> { Status = ResultStatus.SUCCESS, Result = game };

			using (var ctx = XMLDataContext.Instance)
			{
				try
				{
					if (ctx.SavedGames.Records.Any(dto => dto.UID == game.UID))
						ctx.SavedGames.Records.Remove(ctx.SavedGames.Records.Single(dto => dto.UID == game.UID));

				}
				catch (Exception ex)
				{
					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("Game was not removed.");
					return result;
				}
			}

			result.Messages.Add("Game removed.");
			return result;
		}

		public IResult<ISavedGame, ISavedGame> UpdateSavedGame(ISavedGame game)
		{
			var result = new OperationResult<ISavedGame, ISavedGame> { Status = ResultStatus.SUCCESS, Result = game };

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = RemoveSavedGame(game);
					if (removeResult.Status == ResultStatus.EXCEPTION) throw removeResult.ex;

					var saveResult = SaveSavedGame(game);
					if (saveResult.Status == ResultStatus.EXCEPTION) throw saveResult.ex;

					result.SuccessfulObjects.Add(game);					
					
				}
				catch (Exception ex)
				{
					result.FailedObjects.Add(game);	

					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("Game failed to update.");
					txn.Dispose();
					return result;
				}

				txn.Complete();
			}

			result.Messages.Add("Game updated.");
			return result;
		}


		public IEnumerable<ScenarioDTO> GetScenarios()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.Scenarios.Records.Cast<ScenarioDTO>();
			}
		}

		public IResult<IScenario, IScenario> SaveScenario(IScenario scenario)
		{
			var result = new OperationResult<IScenario, IScenario> { Status = ResultStatus.SUCCESS, Result = scenario };

			using (var ctx = XMLDataContext.Instance)
			{
				try
				{
					if (ctx.Scenarios.Records.All(dto => dto.UID != scenario.UID))
						ctx.Scenarios.Records.Add(scenario.ToDTO());
				}
				catch (Exception ex)
				{
					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("Scenario save failed.");
					return result;
				}
			}

			result.Messages.Add("Scenario Saved.");
			return result;
		}

		public IResult<IScenario, IScenario> RemoveScenario(IScenario scenario)
		{
			var result = new OperationResult<IScenario, IScenario> { Status = ResultStatus.SUCCESS, Result = scenario };

			using (var ctx = XMLDataContext.Instance)
			{
				try
				{
					if (ctx.Scenarios.Records.Any(dto => dto.UID == scenario.UID))
						ctx.Scenarios.Records.Remove(ctx.Scenarios.Records.Single(dto => dto.UID == scenario.UID));

				}
				catch (Exception ex)
				{
					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("Scenario was not removed.");
					return result;
				}
			}

			result.Messages.Add("Scenario removed.");
			return result;
		}

		public IResult<IScenario, IScenario> UpdateScenario(IScenario scenario)
		{
			var result = new OperationResult<IScenario, IScenario> { Status = ResultStatus.SUCCESS, Result = scenario };

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = RemoveScenario(scenario);
					if (removeResult.Status == ResultStatus.EXCEPTION) throw removeResult.ex;

					var saveResult = SaveScenario(scenario);
					if (saveResult.Status == ResultStatus.EXCEPTION) throw saveResult.ex;

					result.SuccessfulObjects.Add(scenario);
				}
				catch (Exception ex)
				{
					result.FailedObjects.Add(scenario);

					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("Scenario failed to update.");
					txn.Dispose();
					return result;
				}

				txn.Complete();
			}

			result.Messages.Add("Scenario updated.");
			return result;
		}


		public IEnumerable<VictoryConditionTypeDTO> GetVictoryConditionTypes()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.VictoryConditionTypesTable.Records.Cast<VictoryConditionTypeDTO>();
			}
		}


#region StrategyCache

		
		public IEnumerable<UnitTaskTypeDTO> GetUnitTaskTypes()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.UnitTaskTypesTable.Records.Cast<UnitTaskTypeDTO>();
			}
		}

		public IResult<IUnitTaskType, IUnitTaskType> SaveUnitTaskTypes(List<IUnitTaskType> unitTaskTypes)
		{
			var result = new OperationResult<IUnitTaskType, IUnitTaskType>();
			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Unit Task Types saved.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in unitTaskTypes)
				{
					try
					{
						if (ctx.UnitTaskTypesTable.Records.All(dto => dto.UID != o.UID))
							ctx.UnitTaskTypesTable.Records.Add(o.ToDTO());

					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Unit Task Types saved.");
					}
				}
			}

			return result;
		}

		public IResult<IUnitTaskType, IUnitTaskType> RemoveUnitTaskTypes(List<IUnitTaskType> unitTaskTypes)
		{
			var result = new OperationResult<IUnitTaskType, IUnitTaskType>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Unit Task Types removed.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in unitTaskTypes)
				{
					try
					{
						using (var txn = new TransactionScope())
						{
							if (ctx.UnitTaskTypesTable.Records.Any(dto => dto.UID == o.UID))
								ctx.UnitTaskTypesTable.Records.Remove(ctx.UnitTaskTypesTable.Records.Single(dto => dto.UID == o.UID));

							txn.Complete();
						}

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Unit Task Types removed.");
					}
				}
			}

			return result;
		}

		public IResult<IUnitTaskType, IUnitTaskType> UpdateUnitTaskTypes(List<IUnitTaskType> unitTaskTypes)
		{
			var result = new OperationResult<IUnitTaskType, IUnitTaskType>();

			result.Status = ResultStatus.SUCCESS;

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = RemoveUnitTaskTypes(unitTaskTypes);
					if (removeResult.Status == ResultStatus.EXCEPTION) throw removeResult.ex;

					var saveResult = SaveUnitTaskTypes(unitTaskTypes);
					if (saveResult.Status == ResultStatus.EXCEPTION) throw saveResult.ex;

					unitTaskTypes.ForEach(o => result.SuccessfulObjects.Add(o));
				}
				catch (Exception ex)
				{
					unitTaskTypes.ForEach(o => result.FailedObjects.Add(o));

					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("No unit task types were updated.");
					txn.Dispose();
					return result;
				}

				txn.Complete();
			}

			result.Messages.Add("All unit task types updated.");
			return result;
		}


		public IEnumerable<MissionTypeDTO> GetMissionTypes()
		{
			using (var ctx = XMLDataContext.Instance)
			{
				return ctx.MissionTypesTable.Records.Cast<MissionTypeDTO>();
			}
		}

		public IResult<IMissionType, IMissionType> SaveMissionTypes(List<IMissionType> MissionTypes)
		{
			var result = new OperationResult<IMissionType, IMissionType>();
			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Mission Types saved.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in MissionTypes)
				{
					try
					{
						if (ctx.MissionTypesTable.Records.All(dto => dto.UID != o.UID))
							ctx.MissionTypesTable.Records.Add(o.ToDTO());

					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Mission Types saved.");
					}
				}
			}

			return result;
		}

		public IResult<IMissionType, IMissionType> RemoveMissionTypes(List<IMissionType> MissionTypes)
		{
			var result = new OperationResult<IMissionType, IMissionType>();

			result.Status = ResultStatus.SUCCESS;
			result.Messages.Add("All Mission Types removed.");

			using (var ctx = XMLDataContext.Instance)
			{
				foreach (var o in MissionTypes)
				{
					try
					{
						using (var txn = new TransactionScope())
						{
							if (ctx.MissionTypesTable.Records.Any(dto => dto.UID == o.UID))
								ctx.MissionTypesTable.Records.Remove(ctx.MissionTypesTable.Records.Single(dto => dto.UID == o.UID));

							txn.Complete();
						}

						result.SuccessfulObjects.Add(o);
					}
					catch (Exception ex)
					{
						result.ex = ex;
						result.FailedObjects.Add(o);
						result.Status = ResultStatus.EXCEPTION;
						result.Messages.Clear();
						result.Messages.Add("Not all Mission Types removed.");
					}
				}
			}

			return result;
		}

		public IResult<IMissionType, IMissionType> UpdateMissionTypes(List<IMissionType> MissionTypes)
		{
			var result = new OperationResult<IMissionType, IMissionType>();

			result.Status = ResultStatus.SUCCESS;

			using (var txn = new TransactionScope())
			{
				try
				{
					var removeResult = RemoveMissionTypes(MissionTypes);
					if (removeResult.Status == ResultStatus.EXCEPTION) throw removeResult.ex;

					var saveResult = SaveMissionTypes(MissionTypes);
					if (saveResult.Status == ResultStatus.EXCEPTION) throw saveResult.ex;

					MissionTypes.ForEach(o => result.SuccessfulObjects.Add(o));
				}
				catch (Exception ex)
				{
					MissionTypes.ForEach(o => result.FailedObjects.Add(o));

					result.ex = ex;
					result.Status = ResultStatus.EXCEPTION;
					result.Messages.Add("No Mission Objetive Types were updated.");
					txn.Dispose();
					return result;
				}

				txn.Complete();
			}

			result.Messages.Add("All Mission Objetive Types updated.");
			return result;
		}


		public IEnumerable<ITactic> GetTactics()
		{
			return Cache.TurnStrategyCache.GetAll();
		}

		public IResult<ITactic, ITactic> SaveTactic(ITactic tactic)
		{
			var result = new OperationResult<ITactic, ITactic> { Status = ResultStatus.SUCCESS, Result = tactic };

			try
			{
				Cache.TurnStrategyCache.TryAdd(tactic.UID, tactic);
			}
			catch (Exception ex)
			{
				result.ex = ex;
				result.Status = ResultStatus.EXCEPTION;
				result.Messages.Add("Tactic save failed.");
				return result;
			}


			result.Messages.Add("Tactic Saved.");
			return result;
		}

		public IResult<ITactic, ITactic> RemoveTactic(ITactic tactic)
		{
			var result = new OperationResult<ITactic, ITactic> { Status = ResultStatus.SUCCESS, Result = tactic };

			try
			{
				Cache.TurnStrategyCache.TryRemove(tactic.UID);
			}
			catch (Exception ex)
			{
				result.ex = ex;
				result.Status = ResultStatus.EXCEPTION;
				result.Messages.Add("Tactic remove failed.");
				return result;
			}


			result.Messages.Add("Tactic Removed.");
			return result;
		}

		public IResult<ITactic, ITactic> UpdateTactic(ITactic tactic)
		{
			var result = new OperationResult<ITactic, ITactic> { Status = ResultStatus.SUCCESS, Result = tactic };

			try
			{
				Cache.TurnStrategyCache.TryUpdate(tactic.UID, tactic);
			}
			catch (Exception ex)
			{
				result.ex = ex;
				result.Status = ResultStatus.EXCEPTION;
				result.Messages.Add("Tactic update failed.");
				return result;
			}


			result.Messages.Add("Tactic Updated.");
			return result;
		}

		public IResult<ITactic, ITactic> CreateTacticFromContext(TacticDTO tacticDTO, IEnumerable<MissionDTO> missionDTOs, IEnumerable<UnitTaskDTO> unitTaskDTOs)
		{
			var result = new OperationResult<ITactic, ITactic>();

			var missions = new List<IMission>();

			// Create Missions
			foreach (var dto in missionDTOs)
			{
				missions.Add(new Mission
								(
									GetMissionTypes().SingleOrDefault(mt => mt.ID == dto.MissionType).ToComponent(),
									dto.Unit
								));

			}

			// Create UnitTasks
			foreach (var dto in unitTaskDTOs)
			{
				var mission = missions.SingleOrDefault(m => m.ID == dto.Mission);
				var task = new UnitTask
								(
									GetUnitTaskTypes().SingleOrDefault(utt => utt.ID == dto.UnitTaskType).ToComponent(),
									mission,
									null,
									dto.TurnsToComplete
								);

				mission.AddChildComponent(task);
			}

			result.Result = new Tactic
								(
									(StrategicalStance)tacticDTO.Stance,
									GetPlayers().SingleOrDefault(p => p.ID == tacticDTO.Player).ToComponent()
								);

			result.Result.AddChildComponents(missions);

			return result;
		}


#endregion



		//public IEnumerable<IStrategy> GetStrategies()
		//{
		//	return Cache.TurnStrategyCache.GetAll();
		//}

		//public IResult<IStrategy, IStrategy> SaveStrategy(IStrategy strategy)
		//{
		//	var result = new OperationResult<IStrategy, IStrategy> { Status = ResultStatus.SUCCESS, Result = strategy };

		//	try
		//	{
		//		Cache.TurnStrategyCache.TryAdd(strategy.UID, strategy);
		//	}
		//	catch (Exception ex)
		//	{
		//		result.ex = ex;
		//		result.Status = ResultStatus.EXCEPTION;
		//		result.Messages.Add("Strategy save failed.");
		//		return result;
		//	}


		//	result.Messages.Add("Strategy Saved.");
		//	return result;
		//}

		//public IResult<IStrategy, IStrategy> RemoveStrategy(IStrategy strategy)
		//{
		//	var result = new OperationResult<IStrategy, IStrategy> { Status = ResultStatus.SUCCESS, Result = strategy };

		//	try
		//	{
		//		Cache.TurnStrategyCache.TryRemove(strategy.UID);
		//	}
		//	catch (Exception ex)
		//	{
		//		result.ex = ex;
		//		result.Status = ResultStatus.EXCEPTION;
		//		result.Messages.Add("Strategy remove failed.");
		//		return result;
		//	}


		//	result.Messages.Add("Strategy Removed.");
		//	return result;
		//}

		//public IResult<IStrategy, IStrategy> UpdateStrategy(IStrategy strategy)
		//{
		//	var result = new OperationResult<IStrategy, IStrategy> { Status = ResultStatus.SUCCESS, Result = strategy };

		//	try
		//	{
		//		Cache.TurnStrategyCache.TryUpdate(strategy.UID, strategy);
		//	}
		//	catch (Exception ex)
		//	{
		//		result.ex = ex;
		//		result.Status = ResultStatus.EXCEPTION;
		//		result.Messages.Add("Strategy update failed.");
		//		return result;
		//	}


		//	result.Messages.Add("Strategy Updated.");
		//	return result;
		//}

	}
}
