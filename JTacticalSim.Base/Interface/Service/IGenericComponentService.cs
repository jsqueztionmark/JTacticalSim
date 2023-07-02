using System;
using System.Collections.Generic;
using System.ServiceModel;
using JTacticalSim.API.Component;
using JTacticalSim.API.InfoObjects;

namespace JTacticalSim.API.Service
{
	public interface IGenericComponentService
	{
		/// <summary>
		/// Determines whether a component is currently still available in the data context
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		[OperationContract]
		bool ExistsInContext(IBaseComponent component);

		/// <summary>
		/// Returns the associated DTO for a given component's UID
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		[OperationContract]
		object GetDTOForComponent(IBaseComponent component);

		/// <summary>
		/// Returns a component based on ID
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="dtoT"></typeparam>
		/// <param name="id"></param>
		/// <returns></returns>
		[OperationContract]
		TComponent GetByID<TComponent>(int id)
			where TComponent : class, IBaseComponent;

		/// <summary>
		/// Returns a component based on Name
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="dtoT"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		[OperationContract]
		TComponent GetByName<TComponent>(string name)
			where TComponent : class, IBaseComponent;

		/// <summary>
		/// Returns all components of a specified type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="dtoT"></typeparam>
		/// <returns></returns>
		[OperationContract]
		List<TComponent> GetAll<TComponent>()
			where TComponent : class, IBaseComponent;


		/// <summary>
		/// Returns the associated table object for a component type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="dtoT"></typeparam>
		/// <returns></returns>
		[OperationContract]
		TableInfo GetComponentTable<TComponent>()
			where TComponent : class, IBaseComponent;

		/// <summary>
		/// Returns the next available ID (auto-increment) from the appropriate data context table
		/// T must be IAutoIncrementable
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		int GetNextID(IBaseComponent component);

		/// <summary>
		/// Returns a new ComponentResult of T with the data from a service result">
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <typeparam name="TObject"></typeparam>
		/// <param name="serviceResult"></param>
		/// <returns></returns>
		[OperationContract]
		IResult<TResult, TObject> ConvertServiceResultDataToComponentResult<TResult, TObject>(IResult<TResult, TObject> serviceResult);

		/// <summary>
		/// Returns an empty, new ComponentResult
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <typeparam name="TObject"></typeparam>
		/// <returns></returns>
		[OperationContract]
		IResult<TResult, TObject> CreateNewComponentResult<TResult, TObject>();
	}
}
