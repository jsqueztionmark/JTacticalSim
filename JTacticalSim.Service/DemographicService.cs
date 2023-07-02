using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using JTacticalSim.API.Component;
using JTacticalSim.API.Service;
using JTacticalSim.API;
using JTacticalSim.DataContext;
using JTacticalSim.Utility;
using ctxUtil = JTacticalSim.DataContext.Utility;

namespace JTacticalSim.Service
{
	[ServiceBehavior]
	public sealed class DemographicService : BaseGameService, IDemographicService
	{
		static readonly object padlock = new object();

		private static volatile IDemographicService _instance;
		public static IDemographicService Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new DemographicService();
				}

				return _instance;
			}
		}

		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		private DemographicService()
		{ }

#region Service Methods

		[OperationBehavior]
		public IEnumerable<IDemographic> GetDemographics()
		{
			return ComponentRepository.GetDemographics().Select(d => d.ToComponent());
		}

		[OperationBehavior]
		public IResult<IDemographic, IDemographic> SaveDemographics(List<IDemographic> demographics)
		{
			return ComponentRepository.SaveDemographics(demographics);
		}

		[OperationBehavior]
		public IResult<IDemographic, IDemographic> RemoveDemographics(List<IDemographic> demographics)
		{
			return ComponentRepository.RemoveDemographics(demographics);
		}

		[OperationBehavior]
		public IResult<IDemographic, IDemographic> UpdateDemographics(List<IDemographic> demographics)
		{
			return ComponentRepository.UpdateDemographics(demographics);
		}

		[OperationBehavior]
		public IDemographic GetDemographicByID(int id)
		{
			var dto = ComponentRepository.GetDemographics().SingleOrDefault(d => d.ID == id);

			if (dto == null)
				throw new ComponentNotFoundException("No demographic found with id {0}".F(id));

			return dto.ToComponent();
		}

		[OperationBehavior]
		public IEnumerable<IDemographic> GetDemographicsByType(IDemographicType type)
		{
			var demoClasses = ComponentRepository.GetDemographicClasses()
									.Where(dc => dc.DemographicType.Equals(type.ID))
									.Select(dc => dc.ID).ToList(); 

			var retVal = ComponentRepository.GetDemographics()
							.Where(d => demoClasses.Contains(d.DemographicClass))
							.Select(d => d.ToComponent());

			return retVal;
		}


		[OperationBehavior]
		public IDemographicClass GetDemographicClassByID(int id)
		{
			var dto = ComponentRepository.GetDemographicClasses().SingleOrDefault(dc => dc.ID == id);

			if (dto == null)
				throw new ComponentNotFoundException("No demographic class found with id {0}".F(id));

			return dto.ToComponent();
		}

		[OperationBehavior]
		public IEnumerable<IDemographicClass> GetDemographicClasses()
		{
			return ComponentRepository.GetDemographicClasses().Select(dc => dc.ToComponent());
		}

		[OperationBehavior]
		public IResult<IDemographicClass, IDemographicClass> SaveDemographicClasses(List<IDemographicClass> demographicClasses)
		{
			return ComponentRepository.SaveDemographicClasses(demographicClasses);
		}

		[OperationBehavior]
		public IResult<IDemographicClass, IDemographicClass> RemoveDemographicClasses(List<IDemographicClass> demographicClasses)
		{
			return ComponentRepository.RemoveDemographicClasses(demographicClasses);
		}

		[OperationBehavior]
		public IResult<IDemographicClass, IDemographicClass> UpdateDemographicClasses(List<IDemographicClass> demographicClasses)
		{
			return ComponentRepository.UpdateDemographicClasses(demographicClasses);
		}


		[OperationBehavior]
		public IDemographicType GetDemographicTypeByID(int id)
		{
			var dto = ComponentRepository.GetDemographicTypes().SingleOrDefault(dt => dt.ID == id);

			if (dto == null)
				throw new ComponentNotFoundException("No demographic type found with id {0}".F(id));

			return dto.ToComponent();
		}

		[OperationBehavior]
		public IDemographicType GetDemographicTypeByName(string name)
		{
			var dto =
				ComponentRepository.GetDemographicTypes()
									.SingleOrDefault(dt => dt.Name.ToLowerInvariant() == name.ToLowerInvariant());

			if (dto == null)
				throw new ComponentNotFoundException("No demographic type found with name {0}".F(name));

			return dto.ToComponent();
		}

		[OperationBehavior]
		public IEnumerable<IDemographicType> GetDemographicTypes()
		{
			return ComponentRepository.GetDemographicTypes().Select(dt => dt.ToComponent());
		}

		[OperationBehavior]
		public IResult<IDemographicType, IDemographicType> SaveDemographicTypes(List<IDemographicType> demographicTypes)
		{
			return ComponentRepository.SaveDemographicTypes(demographicTypes);
		}

		[OperationBehavior]
		public IResult<IDemographicType, IDemographicType> RemoveDemographicTypes(List<IDemographicType> demographicTypes)
		{
			return ComponentRepository.RemoveDemographicTypes(demographicTypes);
		}

		[OperationBehavior]
		public IResult<IDemographicType, IDemographicType> UpdateDemographicTypes(List<IDemographicType> demographicTypes)
		{
			return ComponentRepository.UpdateDemographicTypes(demographicTypes);
		}

#endregion

	}
}
