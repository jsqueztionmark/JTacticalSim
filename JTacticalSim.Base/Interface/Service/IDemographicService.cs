using System;
using System.Collections.Generic;
using System.ServiceModel;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.Service
{
	[ServiceContract]
	public interface IDemographicService
	{
		[OperationContract]
		IEnumerable<IDemographic> GetDemographics();

		[OperationContract]
		IResult<IDemographic, IDemographic> SaveDemographics(List<IDemographic> demographics);

		[OperationContract]
		IResult<IDemographic, IDemographic> RemoveDemographics(List<IDemographic> demographics);

		[OperationContract]
		IResult<IDemographic, IDemographic> UpdateDemographics(List<IDemographic> demographics);

		[OperationContract]
		IDemographic GetDemographicByID(int id);

		[OperationContract]
		IEnumerable<IDemographic> GetDemographicsByType(IDemographicType type);

		[OperationContract]
		IDemographicClass GetDemographicClassByID(int id);

		[OperationContract]
		IEnumerable<IDemographicClass> GetDemographicClasses();

		[OperationContract]
		IResult<IDemographicClass, IDemographicClass> SaveDemographicClasses(List<IDemographicClass> demographicClasses);

		[OperationContract]
		IResult<IDemographicClass, IDemographicClass> RemoveDemographicClasses(List<IDemographicClass> demographicClasses);

		[OperationContract]
		IResult<IDemographicClass, IDemographicClass> UpdateDemographicClasses(List<IDemographicClass> demographicClasses);



		[OperationContract]
		IDemographicType GetDemographicTypeByID(int id);

		[OperationContract]
		IDemographicType GetDemographicTypeByName(string name);

		[OperationContract]
		IEnumerable<IDemographicType> GetDemographicTypes();

		[OperationContract]
		IResult<IDemographicType, IDemographicType> SaveDemographicTypes(List<IDemographicType> demographicTypes);

		[OperationContract]
		IResult<IDemographicType, IDemographicType> RemoveDemographicTypes(List<IDemographicType> demographicTypes);

		[OperationContract]
		IResult<IDemographicType, IDemographicType> UpdateDemographicTypes(List<IDemographicType> demographicTypes);
	}
}
