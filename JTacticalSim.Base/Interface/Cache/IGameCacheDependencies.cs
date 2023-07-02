using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.Cache
{
	public interface IGameCacheDependencies
	{
		IGameNodeCache NodeCache { get; }
		IGameUnitCache UnitCache { get; }
		IGameDemographicCache DemographicCache { get; }
		IGameTileCache TileCache { get; }
		IMoveStatCache TurnMoveCache { get; }
		IStrategyCache TurnStrategyCache { get; }

		void ClearAll();
	}
}
