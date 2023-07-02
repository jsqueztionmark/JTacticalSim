using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
