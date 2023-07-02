using System;
using JTacticalSim.API.Component;
using JTacticalSim.API.Cache;

namespace JTacticalSim.Cache
{
	public class GameTileCache : BaseCache<ITile>, IGameTileCache
	{
		// Instance
		static readonly object padlock = new object();

		private static volatile IGameTileCache _instance = null;
		public static IGameTileCache Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new GameTileCache();
				}

				return _instance;
			}
		}

		private GameTileCache() {}

		public override void TryAdd(Guid uid, ITile tile)
		{
			if (!objects.ContainsKey(uid)) objects.Add(uid, tile);
		}

		public override void TryRemove(Guid uid)
		{
			if (objects.ContainsKey(uid)) objects.Remove(uid);		
		}
	}
}
