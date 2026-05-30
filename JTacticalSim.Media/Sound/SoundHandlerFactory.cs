using System;
using System.Diagnostics.CodeAnalysis;
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

		// Allows the GUI layer to register a platform-specific handler factory without
		// creating a circular dependency (JTacticalSim.Media cannot reference JTacticalSim.GUI).
		private Func<ISoundHandler> _handlerFactory;

		public void RegisterHandlerFactory(Func<ISoundHandler> factory)
		{
			_handlerFactory = factory;
		}

		[SuppressMessage("Interoperability", "CA1416", Justification = "WavSoundHandler guards all Windows-only calls at runtime")]
		public ISoundHandler GetSoundHandler(SoundSourceType sourceType)
		{
			if (_handlerFactory != null)
				return _handlerFactory();

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
