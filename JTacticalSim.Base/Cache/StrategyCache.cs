using System;
using System.Collections.Generic;
using JTacticalSim.API.Cache;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;

namespace JTacticalSim.Cache
{
	public class StrategyCache : BaseCache<ITactic>, IStrategyCache
	{
		// Instance
		static readonly object padlock = new object();

		private static volatile IStrategyCache _instance = null;
		public static IStrategyCache Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new StrategyCache();
				}

				return _instance;
			}
		}

		private StrategyCache() {}

		public override void TryAdd(Guid uid, ITactic tactic)
		{
			if (!objects.ContainsKey(uid)) objects.Add(uid, tactic);
		}

		public override void TryRemove(Guid uid)
		{
			if (objects.ContainsKey(uid)) objects.Remove(uid);		
		}

		public IEnumerable<ITactic> Clean()
		{
			var removed = new List<ITactic>();

			foreach (var kvp in objects)
			{
				var tactic = kvp.Value as ITactic;
				if (tactic.IsComplete)
				{
					TryRemove(tactic.UID);
					removed.Add(tactic);
				}					
			}

			return removed;
		}
	}
}
