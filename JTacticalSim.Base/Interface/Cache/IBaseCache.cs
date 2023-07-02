using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.Cache
{
	public interface IBaseCache<T>
	{
		/// <summary>
		/// Returns an object of type T from the cache if it exists in the cache
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="uid"></param>
		/// <returns></returns>
		T TryFind(Guid uid);

		/// <summary>
		/// Returns all cache objects of type T
		/// </summary>
		/// <returns></returns>
		IEnumerable<T> GetAll();

		/// <summary>
		/// Adds an object of type T to the cache if it does not yet exist
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="uid"></param>
		/// /// <param name="obj"></param>
		void TryAdd(Guid uid, T obj) ;

		/// <summary>
		/// Removes an object from the cache if it exists
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="uid"></param>
		void TryRemove(Guid uid);

		/// <summary>
		/// Updates an object in the cache if it exists
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="uid"></param>
		/// /// <param name="obj"></param>
		void TryUpdate(Guid uid, T obj);

		/// <summary>
		/// Clears the cache
		/// </summary>
		void Clear();

		/// <summary>
		/// Refreshes the cache. Clears then re-loads
		/// </summary>
		void Refresh();
	}
}
