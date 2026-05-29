using System;
using System.Text;
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
