using System;
using System.Text;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.Media.Sound
{
	public interface ISoundHandler
	{
		void PlaySound(FileStream fs);
		void PlaySoundAsync(FileStream fs);
		void StopPlayback();
		void StopPlaybackAsync();
		IResult<FileStream, FileStream> GetSound(string name);

		event EventHandler FileLoaded;
		event EventHandler PlayFinished;
	}
}
