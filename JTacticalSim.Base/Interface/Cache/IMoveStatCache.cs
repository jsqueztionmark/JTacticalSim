using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.InfoObjects;

namespace JTacticalSim.API.Cache
{
	/// <summary>
	/// Holds all the move stats for each unit held by the current player for the current turn
	/// </summary>
	public interface IMoveStatCache : IBaseCache<CurrentMoveStatInfo>
	{
	}
}
