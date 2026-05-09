using System;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;

namespace JTacticalSim.API.Cache
{
	/// <summary>
	/// Holdes strategy components for the turn
	/// </summary>
	public interface IStrategyCache : IBaseCache<ITactic>
	{
		/// <summary>
		/// Removes completed strategies
		/// Returns a collection of the removed strategies
		/// </summary>
		IEnumerable<ITactic> Clean();
	}
}