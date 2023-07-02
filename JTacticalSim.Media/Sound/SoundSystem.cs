using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Media.Sound;

namespace JTacticalSim.Media.Sound
{
	public sealed class SoundSystem : ISoundSystem
	{
		private readonly Dictionary<SoundType, string> _soundStore;
		private readonly ISoundHandler _handler;
		public event EventHandler PlayFinished;

		public SoundSystem()
		{
			_soundStore = new Dictionary<SoundType, string>();
			_handler = SoundHandlerFactory.Instance.GetSoundHandler(Utility.GetConfiguredSoundSourceType());
			_handler.PlayFinished += On_PlayFinished;
		}

		public IResult<SoundType, Tuple<SoundType, string>> AddSound(SoundType soundType, string name)
		{
			var r = new OperationResult<SoundType, Tuple<SoundType, string>>();

			if (_soundStore.ContainsKey(soundType))
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Sound already exists.");
				r.FailedObjects.Add(new Tuple<SoundType, string>(soundType, name));
				return r;
			}

			_soundStore.Add(soundType, name);

			r.Status = ResultStatus.SUCCESS;
			r.Messages.Add("Sound added.");
			r.SuccessfulObjects.Add(new Tuple<SoundType, string>(soundType, name));
			r.Result = soundType;
			return r;
		}

		public IResult<bool, SoundType> Exists(SoundType soundType)
		{
			var r = new OperationResult<bool, SoundType>();
			r.Result = _soundStore.ContainsKey(soundType) && !String.IsNullOrEmpty(_soundStore[soundType]);
			return r;
		}

		public void Play(SoundType soundType)
		{
			if (!_soundStore.ContainsKey(soundType))
				return;

			var soundName = _soundStore[soundType];
			var r = _handler.GetSound(soundName);

			if (r.Status == ResultStatus.SUCCESS)
			{
				_handler.PlaySound(r.Result);
			}							
		}

		public void PlayAsync(SoundType soundType)
		{
			if (!_soundStore.ContainsKey(soundType))
				return;

			var soundName = _soundStore[soundType];
			var r = _handler.GetSound(soundName);

			if (r.Status == ResultStatus.SUCCESS)
			{
				_handler.PlaySoundAsync(r.Result);
			}				
		}

		public void StopPlayback()
		{
			_handler.StopPlayback();
		}

		public void StopPlaybackAsync()
		{
			_handler.StopPlaybackAsync();
		}


		private void On_PlayFinished(object sender, EventArgs e)
		{
			if (PlayFinished != null) PlayFinished(sender, e);
		}
	}
}
