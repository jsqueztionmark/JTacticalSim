using System;
using System.Text;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Component;
using JTacticalSim.API;

namespace JTacticalSim.API.Game
{
	public interface IZoomHandler : IBaseGameObject
	{
		ZoomInfo CurrentZoom { get; }
		ZoomLevel MaxZoomLevel { get; }

		void ResetAllZoomLevels(int vOffSet, int hOffSet);
		void SyncAllZoomLevels();
		void SyncAllZoomLevelsFullArea();
		void LoadZooms(IEnumerable<ZoomInfo> zooms);
		ZoomInfo GetZoomLevelInfo(ZoomLevel zoomLevel);
		bool CycleZoomLevel(CycleDirection direction);
	}
}
