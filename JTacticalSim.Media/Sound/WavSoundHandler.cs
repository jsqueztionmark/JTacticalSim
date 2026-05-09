using System.Media;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Media.Sound;
using JTacticalSim.Utility;

namespace JTacticalSim.Media.Sound
{
	internal class WavSoundHandler : ISoundHandler
	{
		public event EventHandler FileLoaded;
		public event EventHandler PlayFinished;
		private SoundPlayer _soundPlayer;

		public WavSoundHandler() { }

		private SoundPlayer Player => _soundPlayer ??= OperatingSystem.IsWindows() ? new SoundPlayer() : null;

		public void PlaySoundAsync(FileStream fs)
		{
			if (Player == null) return;
			var thread = new Thread(PlaySoundThread);
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Start(fs);
		}

		public void PlaySound(FileStream fs)
		{
			if (Player == null) return;
			PlaySoundThread(fs);
		}

		public void StopPlaybackAsync()
		{
			throw new NotImplementedException();
		}

		public void StopPlayback()
		{
			Player?.Stop();
			On_PlayFinished(this, new EventArgs());
		}

		public IResult<FileStream, FileStream> GetSound(string name)
		{
			var r = new OperationResult<FileStream, FileStream>();

			try
			{
				var mediaRoot = ConfigurationManager.AppSettings["mediafilepathDefault"]
					.Replace('\\', Path.DirectorySeparatorChar);
				if (!Path.IsPathRooted(mediaRoot))
					mediaRoot = Path.Combine(Directory.GetCurrentDirectory(), mediaRoot);

				var fileDir = Path.Combine(mediaRoot,
					Game.Instance.LoadedScenario.ComponentSet.Name,
					ConfigurationManager.AppSettings["soundsourcetype"]);

				var fs = new FileStream(Path.Combine(fileDir, name + ".wav"), FileMode.Open, FileAccess.Read);

				r.Result = fs;
				r.SuccessfulObjects.Add(fs);
				r.Status = ResultStatus.SUCCESS;
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.Messages.Add("Could not load sound file '{0}' from the configured media directory.".F(name));
				r.ex = ex;
			}

			return r;
		}

		private void PlaySoundThread(Object obj)
		{
			var fs = obj as FileStream;
			if (fs == null) throw new Exception("Object type must be FileStream");

			var player = Player;
			if (player == null) return;

			player.Stream = fs;
			player.LoadCompleted += On_FileLoaded;
			player.LoadAsync();
			player.PlaySync();
			On_PlayFinished(this, new EventArgs());
		}

		private void On_FileLoaded(object sender, EventArgs e)
		{
			if (FileLoaded != null) FileLoaded(sender, e);
		}

		private void On_PlayFinished(object sender, EventArgs e)
		{
			if (PlayFinished != null) PlayFinished(sender, e);
		}
	}
}
