using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows.Threading;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Media.Sound;
using JTacticalSim.Utility;

namespace JTacticalSim.Media.Sound
{
	internal class WavSoundHandler : DispatcherObject, ISoundHandler
	{
		public event EventHandler FileLoaded;
		public event EventHandler PlayFinished;
		private SoundPlayer _soundPlayer;

		public WavSoundHandler()
		{
			_soundPlayer = new SoundPlayer();
		}

		public void PlaySoundAsync(FileStream fs)
		{
			Action play = () =>	
			{
				var thread = new Thread(PlaySoundThread);
				thread.SetApartmentState(ApartmentState.STA);
				thread.IsBackground = true;
				thread.Start(fs);
			};
			Dispatcher.Invoke(play); 
		}

		public void PlaySound(FileStream fs)
		{
			PlaySoundThread(fs);
		}

		public void StopPlaybackAsync()
		{
			throw new NotImplementedException();
		}

		public void StopPlayback()
		{
			_soundPlayer.Stop();
			On_PlayFinished(this, new EventArgs());
		}

		public IResult<FileStream, FileStream> GetSound(string name)
		{
			var r = new OperationResult<FileStream, FileStream>();

			var curDir = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());

			try
			{
				var fileDir = "{0}{1}\\{2}\\{3}\\".F(curDir, 
													ConfigurationManager.AppSettings["mediafilepathDefault"], 
													Game.Instance.LoadedScenario.ComponentSet.Name, 
													ConfigurationManager.AppSettings["soundsourcetype"]);
	
				var fs = new FileStream("{0}{1}.wav".F(fileDir, name), FileMode.Open, FileAccess.Read);

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
			// Since SoundPlayer has no play end event, we'll run this on another thread
			// and run synchronously so we can fire our own event.
			var fs = obj as FileStream;

			if (fs == null)
				throw new Exception("Object type must be FileStream");

			_soundPlayer.Stream = fs;
			_soundPlayer.LoadCompleted += On_FileLoaded;
			_soundPlayer.LoadAsync();
			_soundPlayer.PlaySync();
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
