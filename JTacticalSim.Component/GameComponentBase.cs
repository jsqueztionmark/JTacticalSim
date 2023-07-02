using System;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Media.Sound;
using JTacticalSim.Media.Sound;

namespace JTacticalSim.Component
{
	public abstract class GameComponentBase : BaseGameObject, IBaseComponent, IComparable, ISoundPlayable
	{
		public virtual event EventHandler PlaySoundFinished;

		public virtual ISoundSystem Sounds { get; private set; }
		public virtual string Name { get; set; }
		public string Description { get; set; }
		public int ID { get; set; }
		public Guid UID { get; set; }

		protected GameComponentBase()
			: base(GameObjectType.COMPONENT)
		{
			Sounds = new SoundSystem();
			UID = Guid.NewGuid();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is IBaseComponent) || obj == null || obj is DBNull) return false;
			
			var rhs = (IBaseComponent)obj;
			if (rhs == null) return false;
			return (this.UID == rhs.UID);
		}

		public static bool operator == (GameComponentBase rhs, IBoardComponent lhs)
		{
			if (lhs == null) return false;
			return rhs.Equals(lhs);
		}

		public static bool operator != (GameComponentBase rhs, IBoardComponent lhs)
		{
			return !rhs.Equals(lhs.Location);
		}

		public override int GetHashCode()
		{
			return ID.GetHashCode() ^ UID.GetHashCode();
		}

#region IComparable Members

		public int CompareTo(object obj)
		{
			if (this.UID == ((IBaseComponent)obj).UID) return 1;			
			return -1;

		}

#endregion

#region Sounds

		public virtual void PlaySound(SoundType soundType)
		{
			Sounds.Play(soundType);
		}

		public virtual void PlaySoundAsync(SoundType soundType)
		{
			Sounds.PlayAsync(soundType);
		}

		public virtual void StopSoundPlayback()
		{
			Sounds.StopPlayback();
		}

		public virtual void StopSoundPlaybackAsync()
		{
			Sounds.StopPlaybackAsync();
		}

#endregion

		public void SetNextID()
		{
			this.ID = TheGame().JTSServices.GenericComponentService.GetNextID(this);
		}

		protected void On_PlaySoundFinished(object sender, EventArgs e)
		{
			if (PlaySoundFinished != null) PlaySoundFinished(this, e);
		}
	}
}
