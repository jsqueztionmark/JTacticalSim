using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using JTacticalSim.API.Cache;
using JTacticalSim.API.Component;
using JTacticalSim.API.InfoObjects;

namespace JTacticalSim.Cache
{
	public sealed class MoveStatCache : BaseCache<CurrentMoveStatInfo>, IMoveStatCache
	{
		// Instance
		static readonly object padlock = new object();

		private static volatile IMoveStatCache _instance = null;
		public static IMoveStatCache Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new MoveStatCache();
				}

				return _instance;
			}
		}

		private MoveStatCache() {}

		public override void TryAdd(Guid uid, CurrentMoveStatInfo stats)
		{
			if (!objects.ContainsKey(uid)) objects.Add(uid, stats);
		}

		public override void TryRemove(Guid uid)
		{
			if (objects.ContainsKey(uid)) objects.Remove(uid);		
		}


		public override void Refresh()
		{			
			var factions = Game.Instance.JTSServices.GameService.GetAllFactions();
			var units = Game.Instance.JTSServices.UnitService.GetAllUnits(factions).ToArray();

			base.Refresh();

			// We don't want to reset the movement stats when we're loading new game data
			// as we may have saved stat data
			// We also will be storing global move stats here as well, e.g. CurrentFuelRange. We don't want to reset these.
			foreach (var u in units)
			{
				TryUpdate(u.UID, new CurrentMoveStatInfo 
													{
														MovementPoints = u.MovementPoints,
														RemoteFirePoints = u.RemoteFirePoints,
														HasPerformedAction = false
													});
			}

			Game.Instance.JTSServices.UnitService.UpdateUnits(units.ToList());

		}
 	}
}
