using System;
using System.Collections.Generic;
using System.Text;
using JTacticalSim.API.Cache;

namespace JTacticalSim.Cache
{
	public class GameCache : IGameCacheDependencies
	{
		static readonly object padlock = new object();

		private static volatile IGameCacheDependencies _instance = null;
		public static IGameCacheDependencies Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new GameCache();
				}

				return _instance;
			}
		}

		public IGameNodeCache NodeCache { get { return GameNodeCache.Instance; }}
		public IGameUnitCache UnitCache { get { return GameUnitCache.Instance; } }
		public IGameDemographicCache DemographicCache { get { return GameDemographicCache.Instance; } }
		public IGameTileCache TileCache { get { return GameTileCache.Instance; } }
		public IMoveStatCache TurnMoveCache { get { return MoveStatCache.Instance; } }
		public IStrategyCache TurnStrategyCache { get { return StrategyCache.Instance; } }

		public void ClearAll()
		{
			NodeCache.Clear();
			UnitCache.Clear();
			DemographicCache.Clear();
			TileCache.Clear();
			TurnMoveCache.Clear();
			TurnStrategyCache.Clear();
		}
	}
}
