using System;
using JTacticalSim.API;
using JTacticalSim.API.Media.Sound;

namespace JTacticalSim.Media.Sound
{
	public class SoundHandlerFactory
	{
		private static volatile SoundHandlerFactory _instance = null;
		static readonly object padlock = new object();

		public static SoundHandlerFactory Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (padlock)
						if (_instance == null) _instance = new SoundHandlerFactory();
				}

				return _instance;
			}
		}


		public ISoundHandler GetSoundHandler(SoundSourceType sourceType)
		{
			switch (sourceType)
			{
				case SoundSourceType.WAV:
				default:
					{
						return new WavSoundHandler();
					}
			}
		}
	}
}
