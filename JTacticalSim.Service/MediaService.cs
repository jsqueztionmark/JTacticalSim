using System.ServiceModel;
using JTacticalSim.API.Service;


namespace JTacticalSim.Service
{
	[ServiceBehavior]
	public class MediaService : BaseGameService, IMediaService
	{
		//private readonly string _curDrive = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());
		static readonly object padlock = new object();

		private static volatile IMediaService _instance;
		public static IMediaService Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new MediaService();
				}

				return _instance;
			}
		}

		private MediaService()
		{ }

#region Service Methods


#endregion

	}
}
