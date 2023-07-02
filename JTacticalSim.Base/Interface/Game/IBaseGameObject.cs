using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Cache;

namespace JTacticalSim.API.Game
{
	public interface IBaseGameObject
	{	
		GameObjectType GOType { get; }
		IGame TheGame();
		IGameCacheDependencies Cache();
	}
}
