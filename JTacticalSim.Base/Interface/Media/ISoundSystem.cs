using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTacticalSim.API;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.Media.Sound
{
	public interface ISoundSystem
	{
		IResult<SoundType, Tuple<SoundType, string>> AddSound(SoundType soundType, string name);
		IResult<bool, SoundType> Exists(SoundType soundType);
		void Play(SoundType soundType);
		void PlayAsync(SoundType soundType);
		void StopPlayback();
		void StopPlaybackAsync();

		event EventHandler PlayFinished;
	}
}
