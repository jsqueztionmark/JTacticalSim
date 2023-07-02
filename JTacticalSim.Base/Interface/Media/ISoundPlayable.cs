using System;

namespace JTacticalSim.API.Media.Sound
{
	public interface ISoundPlayable
	{
		event EventHandler PlaySoundFinished;

		void PlaySound(SoundType soundType);
		void PlaySoundAsync(SoundType soundType);
		void StopSoundPlayback();
		void StopSoundPlaybackAsync();
	}
}
