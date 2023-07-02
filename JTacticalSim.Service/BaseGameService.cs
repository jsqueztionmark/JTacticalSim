using JTacticalSim.API.Service;
using JTacticalSim.API.Data;
using JTacticalSim.API.Cache;
using JTacticalSim.Cache;
using JTacticalSim.DataContext.Repository;
using JTacticalSim.DataContext;
using ctxUtil = JTacticalSim.DataContext.Utility;

namespace JTacticalSim.Service
{
	public abstract class BaseGameService : BaseGameObject
	{
		protected DataFactory DataFactory { get { return DataFactory.Instance; } }
		protected IBasePointValues BaseGamePointValues { get { return DataFactory.Instance.GetDataRepository().GetGameBasePointValues(); } }

		protected IComponentRepository ComponentRepository { get { return DataFactory.Instance.GetComponentRepository(); } }
		protected IDataRepository DataRepository { get { return DataFactory.Instance.GetDataRepository(); } }

		protected BaseGameService()
			: base(API.GameObjectType.SERVICE)
		{}
	}
}
