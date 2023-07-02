using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using JTacticalSim.Component;
using JTacticalSim.API.Service;
using JTacticalSim.API.Component;
using JTacticalSim.API;
using JTacticalSim.Data.DTO;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.DataContext;
using JTacticalSim.Utility;
using ctxUtil = JTacticalSim.DataContext.Utility;

namespace JTacticalSim.Service
{
	[ServiceBehavior]
	public sealed class GenericComponentService : BaseGameService, IGenericComponentService
	{
		static readonly object padlock = new object();

		private static volatile IGenericComponentService _instance;
		public static IGenericComponentService Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new GenericComponentService();
				}

				return _instance;
			}
		}

		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		private GenericComponentService()
		{}

#region Generic Get Methods

		[OperationBehavior]
		public bool ExistsInContext(IBaseComponent component)
		{
			dynamic table = ctxUtil.GetComponentTable(component);

			if (table == null) return false;

			foreach (IBaseGameComponentDTO dto in table.Records)
			{
				if (dto.UID == component.UID) return true;
			}

			return false;
		}

		[OperationBehavior]
		public object GetDTOForComponent(IBaseComponent component)
		{
			var matches = new List<IBaseGameComponentDTO>();

			// Get all dto matches from all tables (SHOULD be only one.. if not something's wrong)
			dynamic tableInfo = ctxUtil.GetComponentTable(component);

			if (tableInfo == null) return null;

			foreach (IBaseGameComponentDTO dto in tableInfo.Records)
			{
				if (component.UID == dto.UID) matches.Add(dto);
			}

			// Check for more than one match or no match. If so, throw exception
			if (matches.Count == 0)
				throw new DTONotFoundException("No DTO was found that matches component with UID {0}.".F(component.UID));
			if (matches.Count > 1)
				throw new DTONotFoundException("More than one DTO found that matches component with UID {0}.".F(component.UID));

			object retVal = matches.FirstOrDefault();

			return retVal;

		}

		[OperationBehavior]
		public int GetNextID(IBaseComponent component)
		{
			TableInfo table = ctxUtil.GetComponentTable(component);
			//var getMethod = table.GetType().GetMethod("GetNextID");
			//int retVal = Convert.ToInt32(getMethod.Invoke(table, null));
			var retVal = table.GetNextID();
			return retVal;
		}


		[OperationBehavior]
		public TableInfo GetComponentTable<TComponent>()
			where TComponent : class, IBaseComponent
		{
			var table = ctxUtil.GetAllTableInfos().SingleOrDefault(ti => ti.Key == typeof(TComponent));
			dynamic retVal = table.Value.Item2;
			return retVal;
		}


		// Cool, but HIGHLY Unperformant!!!!!
		// Don't use these for rendering!!!!

		[OperationBehavior]
		public TComponent GetByName<TComponent>(string name)
			where TComponent : class, IBaseComponent
		{
			var dto = GetComponentTable<TComponent>().Records.SingleOrDefault(r => r.Name.ToLowerInvariant() == name.ToLowerInvariant());
			return ConvertToComponent<TComponent>(dto);
		}

		[OperationBehavior]
		public TComponent GetByID<TComponent>(int id)
			where TComponent : class, IBaseComponent
		{
			dynamic dto = GetComponentTable<TComponent>().Records.SingleOrDefault(r => r.ID == id);
			return ConvertToComponent<TComponent>(dto);
		}

		[OperationBehavior]
		public List<TComponent> GetAll<TComponent>()
			where TComponent : class, IBaseComponent
		{
			var dtos = GetComponentTable<TComponent>().Records;

			return dtos.Select(dto => ConvertToComponent<TComponent>((object)dto)).ToList();
		}

#endregion

		private TComponent ConvertToComponent<TComponent>(object dto)
			where TComponent : class, IBaseComponent
		{
			if (dto == null) return null;

			var method = typeof(ComponentConversion).GetMethod("ToComponent", new[] { dto.GetType() });
			dynamic retVal = method.Invoke(dto, new[] { dto });
			return retVal;
		}

		private dtoT ConvertToDTO<TComponent, dtoT>(TComponent component)
			where TComponent : IBaseComponent
			where dtoT : IBaseGameComponentDTO
		{
			var method = typeof(DTOConversion).GetMethod("ToDTO", new[] { typeof(dtoT) });
			dynamic retVal = method.Invoke(component, new object[] { component });
			return retVal;
		}

#region Service Result helpers

		[OperationBehavior]
		public IResult<TResult, TObject> ConvertServiceResultDataToComponentResult<TResult, TObject>(IResult<TResult, TObject> serviceResult)
		{
			var componentResult = new OperationResult<TResult, TObject>
			{
				Status = serviceResult.Status,
				ex = serviceResult.ex,
				FailedObjects = serviceResult.FailedObjects,
				SuccessfulObjects = serviceResult.SuccessfulObjects,
				Messages = serviceResult.Messages,
				Result = serviceResult.Result
			};

			return componentResult;
		}

		[OperationBehavior]
		public IResult<TResult, TObject> CreateNewComponentResult<TResult, TObject>()
		{
			return new OperationResult<TResult, TObject>();
		}

#endregion

	}
}
