using System;
using System.Collections.Generic;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.Cache;

namespace JTacticalSim.Cache
{
	public class GameDemographicCache : BaseCache<IDemographic>, IGameDemographicCache
	{
		// Instance
		static readonly object padlock = new object();

		private static volatile IGameDemographicCache _instance = null;
		public static IGameDemographicCache Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new GameDemographicCache();
				}

				return _instance;
			}
		}

		private GameDemographicCache() {}

		public override void TryAdd(Guid uid, IDemographic demographic)
		{
			if (!objects.ContainsKey(uid)) objects.Add(uid, demographic);
		}

		public override void TryRemove(Guid uid)
		{
			if (objects.ContainsKey(uid)) objects.Remove(uid);		
		}
	}
}
