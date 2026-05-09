using System;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.Service
{
	public interface IDemographicService
	{
		IEnumerable<IDemographic> GetDemographics();

		IResult<IDemographic, IDemographic> SaveDemographics(List<IDemographic> demographics);

		IResult<IDemographic, IDemographic> RemoveDemographics(List<IDemographic> demographics);

		IResult<IDemographic, IDemographic> UpdateDemographics(List<IDemographic> demographics);

		IDemographic GetDemographicByID(int id);

		IEnumerable<IDemographic> GetDemographicsByType(IDemographicType type);

		IDemographicClass GetDemographicClassByID(int id);

		IEnumerable<IDemographicClass> GetDemographicClasses();

		IResult<IDemographicClass, IDemographicClass> SaveDemographicClasses(List<IDemographicClass> demographicClasses);

		IResult<IDemographicClass, IDemographicClass> RemoveDemographicClasses(List<IDemographicClass> demographicClasses);

		IResult<IDemographicClass, IDemographicClass> UpdateDemographicClasses(List<IDemographicClass> demographicClasses);



		IDemographicType GetDemographicTypeByID(int id);

		IDemographicType GetDemographicTypeByName(string name);

		IEnumerable<IDemographicType> GetDemographicTypes();

		IResult<IDemographicType, IDemographicType> SaveDemographicTypes(List<IDemographicType> demographicTypes);

		IResult<IDemographicType, IDemographicType> RemoveDemographicTypes(List<IDemographicType> demographicTypes);

		IResult<IDemographicType, IDemographicType> UpdateDemographicTypes(List<IDemographicType> demographicTypes);
	}
}
