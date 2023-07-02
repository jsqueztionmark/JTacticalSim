using System;
using System.Collections.Generic;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.Cache;

namespace JTacticalSim.Cache
{
	public sealed class GameNodeCache : BaseCache<INode>, IGameNodeCache
	{
		// Instance
		static readonly object padlock = new object();

		private static volatile IGameNodeCache _instance = null;
		public static IGameNodeCache Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new GameNodeCache();
				}

				return _instance;
			}
		}

		private GameNodeCache() {}

		public override void TryAdd(Guid uid, INode node)
		{
			if (!objects.ContainsKey(uid)) objects.Add(uid, node);
		}

		public override void TryRemove(Guid uid)
		{
			if (objects.ContainsKey(uid)) objects.Remove(uid);		
		}
	}
}
