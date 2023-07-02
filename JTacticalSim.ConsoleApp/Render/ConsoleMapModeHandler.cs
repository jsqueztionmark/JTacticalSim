using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Component;
using JTacticalSim.API;
using JTacticalSim.API.Game;
using JTacticalSim.Component.World;

namespace JTacticalSim.ConsoleApp
{
	public sealed class ConsoleMapModeHandler : BaseGameObject, IMapModeHandler
	{
		public MapMode MAX = Enum.GetValues(typeof(MapMode)).Cast<MapMode>().Max();
		public MapMode MIN = Enum.GetValues(typeof(MapMode)).Cast<MapMode>().Min();

		private List<MapModeInfo> _mapModes { get; set; }
		public MapModeInfo CurrentMapMode { get { return _mapModes.SingleOrDefault(mm => mm.IsCurrent); }}

		public ConsoleMapModeHandler()
			: base(GameObjectType.HANDLER)
		{
			_mapModes = new List<MapModeInfo>();
		}

		public void LoadMapModes(IEnumerable<MapModeInfo> mapModes)
		{
			foreach (var mode in mapModes)
				AddMapModeInfo(mode);
		}

		private void AddMapModeInfo(MapModeInfo mapModeInfo)
		{
			if (!_mapModes.Any(mm => mm.MapMode == mapModeInfo.MapMode))
				_mapModes.Add(mapModeInfo);
		}

		public bool CycleMapMode(API.CycleDirection direction)
		{
			var oldCurrent = CurrentMapMode;
			var cycled = false;

			if (direction == API.CycleDirection.UP && CurrentMapMode.MapMode != MAX)
			{			
				CurrentMapMode.IsCurrent = false;
				_mapModes.Single(mm => mm.MapMode == oldCurrent.MapMode + 1).IsCurrent = true;
				cycled = true;
			}
			if (direction == API.CycleDirection.DOWN && CurrentMapMode.MapMode != MIN)
			{
				CurrentMapMode.IsCurrent = false;
				_mapModes.Single(mm => mm.MapMode == oldCurrent.MapMode - 1).IsCurrent = true;
				cycled = true;
			}

			return cycled;
			
		}
	}
}
