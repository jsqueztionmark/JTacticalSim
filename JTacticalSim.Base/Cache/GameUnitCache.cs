using System;
using System.Collections.Generic;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.Cache;

namespace JTacticalSim.Cache
{
	class GameUnitCache : BaseCache<IUnit>, IGameUnitCache
	{
		// Instance
		static readonly object padlock = new object();

		private static volatile IGameUnitCache _instance = null;
		public static IGameUnitCache Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new GameUnitCache();
				}

				return _instance;
			}
		}

		private GameUnitCache() {}

		public override void TryAdd(Guid uid, IUnit unit)
		{
			if (!objects.ContainsKey(uid)) objects.Add(uid, unit);
		}

		public override void TryRemove(Guid uid)
		{
			if (objects.ContainsKey(uid)) objects.Remove(uid);		
		}

	}
}
