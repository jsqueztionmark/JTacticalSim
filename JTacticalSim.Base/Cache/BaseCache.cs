using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using JTacticalSim.API.Cache;
using JTacticalSim.API.Component;

namespace JTacticalSim.Cache
{
	public abstract class BaseCache<T> : IBaseCache<T>
	{
		//Cache collection
		protected Dictionary<Guid, object> objects;

		protected BaseCache()
		{
			objects = new Dictionary<Guid, object>();
		}

		public virtual T TryFind(Guid uid)
		{
			var r = (objects.ContainsKey(uid)) ? objects[uid] : null;			
			return (T)r;
		}

		public virtual IEnumerable<T> GetAll()
		{
			var r = (objects.Values.Select(o => (T)o));
			return r;
		}

		public abstract void TryAdd(Guid uid, T obj);

		public abstract void TryRemove(Guid uid);

		public virtual void TryUpdate(Guid uid, T obj)
		{
			TryRemove(uid);
			TryAdd(uid, obj);
		}

		public virtual void Clear()
		{
			objects.Clear();
		}

		public virtual void Refresh() { Clear(); }
	}
}
