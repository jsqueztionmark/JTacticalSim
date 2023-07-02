using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using JTacticalSim.API;
using JTacticalSim.API.Game;
using JTacticalSim.API.Component;
using JTacticalSim.API.Cache;
using JTacticalSim.API.Service;
using JTacticalSim.API.InfoObjects;


namespace JTacticalSim.Media
{
	public static class Utility
	{
		public static SoundSourceType GetConfiguredSoundSourceType()
		{
			var sourceType = ConfigurationManager.AppSettings["soundsourcetype"];

			switch (sourceType)
			{
				case "WAV":
					return SoundSourceType.WAV;
				default:
					return SoundSourceType.UNKNOWN;
			}
		}
	}
}
