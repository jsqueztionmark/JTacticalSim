using System;
using System.Runtime.Serialization;
using JTacticalSim.API.Service;

namespace JTacticalSim.Service
{
	[Serializable, DataContract]
	public class ServiceDependencies : IServiceDependencies
	{
		static readonly object padlock = new object();

		private static volatile IServiceDependencies _instance;
		public static IServiceDependencies Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new ServiceDependencies();
				}

				return _instance;
			}
		}

		// *********************************************************
		// Services -- to be abstracted to a service provider
		// *********************************************************

		[DataMember]
		public IGenericComponentService GenericComponentService { get { return Service.GenericComponentService.Instance; } }

		[DataMember]
		public IGameService GameService { get { return Service.GameService.Instance; } }

		[DataMember]
		public INodeService NodeService { get { return Service.NodeService.Instance; } }

		[DataMember]
		public IUnitService UnitService { get { return Service.UnitService.Instance; } }

		[DataMember]
		public ITileService TileService { get { return Service.TileService.Instance; } }

		[DataMember]
		public IDemographicService DemographicService { get { return Service.DemographicService.Instance; } }

		[DataMember]
		public IDataService DataService { get { return Service.DataService.Instance; } }

		[DataMember]
		public IRulesService RulesService { get { return Service.RulesService.Instance; } }

		[DataMember]
		public IAIService AIService { get { return Service.AIService.Instance; } }

		[DataMember]
		public IMediaService GraphicsService { get { return Service.MediaService.Instance; } }
	}
}
