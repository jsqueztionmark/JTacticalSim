using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Cache;

namespace JTacticalSim.API.Service
{
	public interface IServiceDependencies
	{
		IGenericComponentService GenericComponentService { get; }
		IGameService GameService { get; }
		IDemographicService DemographicService { get; }
		INodeService NodeService { get; }
		ITileService TileService { get; }
		IUnitService UnitService { get; }
		IDataService DataService { get; }
		IRulesService RulesService { get; }
		IAIService AIService { get; }
		IMediaService GraphicsService { get; }
	}
}
