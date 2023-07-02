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
	public sealed class ConsoleZoomHandler : BaseGameObject, IZoomHandler
	{
		private IBoard _board { get { return TheGame().GameBoard; } }
		private List<ZoomInfo> _zoomLevels { get; set; }
		public ZoomInfo CurrentZoom { get { return _zoomLevels.SingleOrDefault(z => z.IsCurrent); }}
		public ZoomLevel MaxZoomLevel { get; private set; }

		public ConsoleZoomHandler()
			: base(GameObjectType.HANDLER)
		{
			MaxZoomLevel = (ZoomLevel)Convert.ToInt32(ConfigurationManager.AppSettings["max_zoom_level"]);
			_zoomLevels = new List<ZoomInfo>();
		}

		public void LoadZooms(IEnumerable<ZoomInfo> zooms)
		{
			foreach (var z in zooms)
				AddZoomInfo(z);
		}

		private void AddZoomInfo(ZoomInfo zoomInfo)
		{
			if (!_zoomLevels.Any(z => z.Level == zoomInfo.Level))
				_zoomLevels.Add(zoomInfo);
		}

		public void ResetAllZoomLevels(int vOffSet, int hOffSet)
		{
			_zoomLevels.ForEach(z =>
				{
					if (z.Level <= ZoomLevel.ONE) return;

					z.CurrentOrigin.X = _board.SelectedNode.Location.X - hOffSet;
					z.CurrentOrigin.Y = _board.SelectedNode.Location.Y - vOffSet;
				});
		}

		public void SyncAllZoomLevels()
		{
			_zoomLevels.ForEach(z =>
				{
					if (z.Level == ZoomLevel.ONE) return;

					if (_board.SelectedNode.Location.X > (z.CurrentOrigin.X + z.DrawWidth - 1))
						z.CurrentOrigin.X++;
					if (_board.SelectedNode.Location.Y > (z.CurrentOrigin.Y + z.DrawHeight - 1))
						z.CurrentOrigin.Y++;
					if (_board.SelectedNode.Location.X < z.CurrentOrigin.X)
						z.CurrentOrigin.X--;
					if (_board.SelectedNode.Location.Y < z.CurrentOrigin.Y)
						z.CurrentOrigin.Y--;
				});
		}

		public void SyncAllZoomLevelsFullArea()
		{
			_zoomLevels.ForEach(z =>
				{
					if (z.Level == ZoomLevel.ONE) return;

					if (_board.SelectedNode.Location.X > (z.CurrentOrigin.X + z.DrawWidth - 1))
						z.CurrentOrigin.X = z.CurrentOrigin.X + z.DrawWidth;
					if (_board.SelectedNode.Location.Y > (z.CurrentOrigin.Y + z.DrawHeight - 1))
						z.CurrentOrigin.Y = z.CurrentOrigin.Y + z.DrawHeight;
					if (_board.SelectedNode.Location.X < z.CurrentOrigin.X)
						z.CurrentOrigin.X = z.CurrentOrigin.X - z.DrawWidth;
					if (_board.SelectedNode.Location.Y < z.CurrentOrigin.Y)
						z.CurrentOrigin.Y = z.CurrentOrigin.Y - z.DrawHeight;
				});
		}

		public ZoomInfo GetZoomLevelInfo(ZoomLevel zoomLevel)
		{
			return _zoomLevels.SingleOrDefault(zl => zl.Level == zoomLevel);
		}

		public bool CycleZoomLevel(CycleDirection direction)
		{
			var oldCurrent = CurrentZoom;
			var cycled = false;

			if (direction == API.CycleDirection.IN && (int)CurrentZoom.Level > 1)
			{	
				CurrentZoom.IsCurrent = false;
				_zoomLevels.Single(z => z.Level == oldCurrent.Level- 1).IsCurrent = true;
				cycled = true;
			}
			if (direction == API.CycleDirection.OUT && CurrentZoom.Level < MaxZoomLevel)
			{
				CurrentZoom.IsCurrent = false;
				_zoomLevels.Single(z => z.Level == oldCurrent.Level + 1).IsCurrent = true;
				cycled = true;
			}

			return cycled;
		}
	}
}
