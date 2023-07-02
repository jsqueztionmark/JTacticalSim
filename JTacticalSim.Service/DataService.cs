using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.Service;
using JTacticalSim.API.Data;
using JTacticalSim.API.Game;
using JTacticalSim.DataContext;

namespace JTacticalSim.Service
{
	[ServiceBehavior]
	public sealed class DataService : BaseGameService, IDataService
	{
		static readonly object padlock = new object();

		private static volatile IDataService _instance;
		public static IDataService Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new DataService();
				}

				return _instance;
			}
		}

		private DataService()
		{ }

#region Service Methods

	// Game

		[OperationBehavior]
	    public IResult<string, string> LoadSavedGameData(string gameDataDirectory, bool IsScenario)
	    {
	        using (var ctx = DataFactory.GetDataContext())
	        {
	            var r = ctx.LoadSavedGameData(gameDataDirectory, IsScenario);
		        return r;
	        }
	    }

		[OperationBehavior]
	    public IResult<string, string> LoadData(string gameFileDirectory, IComponentSet componentSet, bool IsScenario)
	    {
	        using (var ctx = DataFactory.GetDataContext())
	        {
	            var r = ctx.LoadData(gameFileDirectory, componentSet, IsScenario);
		        return r;
	        }
	    }

		[OperationBehavior]
	    public IResult<IGameFileCopyable, IGameFileCopyable> SaveData(IGameFileCopyable currentData)
	    {	
			using (var ctx = DataFactory.GetDataContext())
	        {
	            var r = ctx.SaveData(currentData);
				return r;
	        }			
	    }

		[OperationBehavior]
		public IResult<IGameFileCopyable, IGameFileCopyable> SaveDataAs(IGameFileCopyable currentData, IGameFileCopyable newData)
		{
			using (var ctx = DataFactory.GetDataContext())
	        {
	             var r = ctx.SaveDataAs(currentData, newData);
				return r;
	        }
		}

		[OperationBehavior]
		public IResult<IGameFileCopyable, IGameFileCopyable> RemoveSavedData(IGameFileCopyable delData)
		{
			using (var ctx = DataFactory.GetDataContext())
	        {
	             var r = ctx.RemoveSavedGameData(delData);
				return r;
	        }
		}

		[OperationBehavior]
		public void ResetDataContext()
		{
			using (var ctx = DataFactory.GetDataContext())
	        {
	            ctx.Reset();
	        }
		}

		[OperationBehavior]
		public IBasePointValues GetBasePointValues()
		{
			return BaseGamePointValues;
		}

	// Unit

		[OperationBehavior]
		public IEnumerable<dynamic> GetUnitAssignments()
		{
			return DataRepository.GetUnitAssignments();
		}

		[OperationBehavior]
		public IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> SaveUnitAssignment(IUnit unit, IUnit assignToUnit)
		{
			return DataRepository.SaveUnitAssignment(unit, assignToUnit);
		}

		[OperationBehavior]
		public IResult<IUnit, IUnit> RemoveUnitAssignmentsFromUnit(IUnit unit)
		{
			return DataRepository.RemoveUnitAssignmentsFromUnit(unit);
		}

		[OperationBehavior]
		public IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> RemoveUnitAssignment(IUnit unit, IUnit assignedToUnit)
		{
			return DataRepository.RemoveUnitAssignment(unit, assignedToUnit);
		}


		[OperationBehavior]
		public IEnumerable<dynamic> GetUnitTransports()
		{
			return DataRepository.GetUnitTransports();
		}

		[OperationBehavior]
		public IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> SaveUnitTransport(IUnit unit, IUnit transport)
		{
			return DataRepository.SaveUnitTransport(unit, transport);
		}

		[OperationBehavior]
		public IResult<Tuple<IUnit, IUnit>, Tuple<IUnit, IUnit>> RemoveUnitTransport(IUnit unit, IUnit transport)
		{
			return DataRepository.RemoveUnitTransport(unit, transport);
		}


		[OperationBehavior]
		public IEnumerable<IUnitType> GetAllowedUnitTypes(ICountry country)
		{
			var unitTypeIDs = DataRepository.GetAllowedUnitTypes()
												.Where(type => Convert.ToInt32(type.Country) == country.ID || Convert.ToInt32(type.Country) == 9999)
												.Select(type => Convert.ToInt32(type.UnitType));
			var r = ComponentRepository.GetUnitTypes().Where(ut => unitTypeIDs.Contains(ut.ID)).Select(dto => dto.ToComponent());
			return r;
		}

		[OperationBehavior]
		public IEnumerable<IUnitGroupType> GetAllowedUnitGroupTypes()
		{
			var unitGroupTypeIDs = DataRepository.GetAllowedUnitGroupTypes().Select(type => Convert.ToInt32(type.UnitGroupType));
			var r = ComponentRepository.GetUnitGroupTypes().Where(ut => unitGroupTypeIDs.Contains(ut.ID)).Select(dto => dto.ToComponent());
			return r;
		}

		[OperationContract]
		public IEnumerable<dynamic> GetFactionVictoryConditions()
		{
			return DataRepository.GetFactionVictoryConditions();
		}

		[OperationContract]
		public IResult<Tuple<IFaction, IVictoryCondition>, Tuple<IFaction, IVictoryCondition>> SaveFactionVictoryConditions(IFaction faction, IVictoryCondition victoryCondition)
		{
			return DataRepository.SaveFactionVictoryConditions(faction, victoryCondition);
		}

		[OperationContract]
		public IResult<Tuple<IFaction, IVictoryCondition>, Tuple<IFaction, IVictoryCondition>> RemoveFactionVictoryConditions(IFaction faction, IVictoryCondition victoryCondition)
		{
			return DataRepository.RemoveFactionVictoryConditions(faction, victoryCondition);
		}



		[OperationBehavior]
		public IEnumerable<dynamic> GetUnitBaseTypeUnitGeogTypesLookup()
		{
			return DataRepository.GetUnitBaseTypeUnitGeogTypesLookup();
		}

		[OperationBehavior]
		public IEnumerable<dynamic> GetUnitGeogTypeDemographicClassesLookup()
		{
			return DataRepository.GetUnitGeogTypeDemographicClassesLookup();
		}

		[OperationBehavior]
		public IEnumerable<dynamic> GetUnitBaseTypeUnitClassesLookup()
		{
			return DataRepository.GetUnitBaseTypeUnitClassesLookup();
		}

		[OperationBehavior]
		public IEnumerable<dynamic> GetUnitTaskUnitClassesLookup()
		{
			return DataRepository.GetUnitTaskTypeUnitClassesLookup();
		}


	#region Lookups

		[OperationBehavior]
		public IEnumerable<int> LookupUnitGeogTypesByBaseTypes(IEnumerable<int> baseTypeids)
		{
			return DataRepository.GetUnitBaseTypeUnitGeogTypesLookup()
						.Where(o => baseTypeids.Contains((int)o.UnitBaseType))
						.Select(o => Convert.ToInt32(o.UnitGeogType as int?));		
		}

		[OperationBehavior]
		public IEnumerable<int> LookupDemographicClassesByUnitGeogTypes(IEnumerable<int> unitGeogTypeids)
		{
			return DataRepository.GetUnitGeogTypeDemographicClassesLookup()
							.Where(o => unitGeogTypeids.Contains((int)o.UnitGeogType))
							.Select(o => Convert.ToInt32(o.DemographicClass as int?));
		}

		[OperationBehavior]
		public IEnumerable<int> LookupAllowableUnitClassesByUnitBaseType(int unitBaseTypeid)
		{
			return DataRepository.GetUnitBaseTypeUnitClassesLookup()
							.Where(o => (int)o.UnitBaseType == unitBaseTypeid || (int)o.UnitBaseType == 9999)
							.Select(o => Convert.ToInt32(o.UnitClass as int?));
		}

		[OperationBehavior]
		public IResult<int, int> LookupUnitTaskTypeStepOrderForMissionType(IUnitTaskType unitTaskType, IMissionType missionType)
		{
			var r = new OperationResult<int, int> {Status = ResultStatus.SUCCESS};
			
			try
			{
				r.Result = DataRepository.GetMissionTypeUnitTaskTypes()
								.Where(mout => (int)mout.MissionType == missionType.ID && (int)mout.UnitTaskType == unitTaskType.ID)
								.Select(mout => Convert.ToInt32(mout.StepOrder as int?)).SingleOrDefault();
				return r;
			}
			catch (ComponentNotFoundException)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Unit task type not associated with the given mission type.");
				return r;
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				return r;
			}	

		}

		[OperationBehavior]
		public IResult<List<IUnitTaskType>, IUnitTaskType> LookupUnitTaskTypesForMissionType(IMissionType missionType)
		{
			var r = new OperationResult<List<IUnitTaskType>, IUnitTaskType> { Status = ResultStatus.SUCCESS };

			try
			{
				var unitTaskIDs = DataRepository.GetMissionTypeUnitTaskTypes()
										.Where(mout => (int)mout.MissionType == missionType.ID).ToList()
										.Select(mout => Convert.ToInt32(mout.UnitTaskType));

				r.Result = ComponentRepository.GetUnitTaskTypes().Where(ut => unitTaskIDs.Contains(ut.ID)).Select(ut => ut.ToComponent()).ToList();

				return r;

			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				return r;
			}
		}

	#endregion

#endregion

	}
}
