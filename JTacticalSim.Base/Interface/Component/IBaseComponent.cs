using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Game;
using JTacticalSim.API.Media.Sound;

namespace JTacticalSim.API.Component
{
	public interface IBaseComponent : IBaseGameObject, ISoundPlayable
	{
		int ID { get; set; }
		string Name { get; set; }
		string Description { get; set; }
		Guid UID { get; set; }

		void SetNextID();
	}
}
