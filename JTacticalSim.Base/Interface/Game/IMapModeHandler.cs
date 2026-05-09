using System;
using System.Text;
using JTacticalSim.API.InfoObjects;

namespace JTacticalSim.API.Game
{
	public interface IMapModeHandler : IBaseGameObject
	{
		MapModeInfo CurrentMapMode { get ; }
		void LoadMapModes(IEnumerable<MapModeInfo> mapModes);
		bool CycleMapMode(API.CycleDirection direction);
	}
}
