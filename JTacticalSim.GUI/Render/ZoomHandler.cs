using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.Component.World;

namespace JTacticalSim.GUI.Render;

public sealed class ZoomHandler : BaseGameObject, IZoomHandler
{
    private IBoard _board => TheGame().GameBoard;
    private List<ZoomInfo> _zoomLevels { get; set; }

    public ZoomInfo CurrentZoom => _zoomLevels.SingleOrDefault(z => z.IsCurrent);
    public ZoomLevel MaxZoomLevel { get; private set; }

    public ZoomHandler()
        : base(GameObjectType.HANDLER)
    {
        MaxZoomLevel = ZoomLevel.FOUR;
        _zoomLevels = new List<ZoomInfo>();
    }

    public void LoadZooms(IEnumerable<ZoomInfo> zooms)
    {
        foreach (var z in zooms)
            if (!_zoomLevels.Any(existing => existing.Level == z.Level))
                _zoomLevels.Add(z);
    }

    public ZoomInfo GetZoomLevelInfo(ZoomLevel zoomLevel) =>
        _zoomLevels.SingleOrDefault(z => z.Level == zoomLevel);

    public bool CycleZoomLevel(CycleDirection direction)
    {
        var old = CurrentZoom;
        if (direction == CycleDirection.IN && (int)CurrentZoom.Level > 1)
        {
            CurrentZoom.IsCurrent = false;
            _zoomLevels.Single(z => z.Level == old.Level - 1).IsCurrent = true;
            return true;
        }
        if (direction == CycleDirection.OUT && CurrentZoom.Level < MaxZoomLevel)
        {
            CurrentZoom.IsCurrent = false;
            _zoomLevels.Single(z => z.Level == old.Level + 1).IsCurrent = true;
            return true;
        }
        return false;
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
            if (_board.SelectedNode.Location.X > z.CurrentOrigin.X + z.DrawWidth - 1)  z.CurrentOrigin.X++;
            if (_board.SelectedNode.Location.Y > z.CurrentOrigin.Y + z.DrawHeight - 1) z.CurrentOrigin.Y++;
            if (_board.SelectedNode.Location.X < z.CurrentOrigin.X) z.CurrentOrigin.X--;
            if (_board.SelectedNode.Location.Y < z.CurrentOrigin.Y) z.CurrentOrigin.Y--;
        });
    }

    public void SyncAllZoomLevelsFullArea()
    {
        _zoomLevels.ForEach(z =>
        {
            if (z.Level == ZoomLevel.ONE) return;
            if (_board.SelectedNode.Location.X > z.CurrentOrigin.X + z.DrawWidth - 1)  z.CurrentOrigin.X += z.DrawWidth;
            if (_board.SelectedNode.Location.Y > z.CurrentOrigin.Y + z.DrawHeight - 1) z.CurrentOrigin.Y += z.DrawHeight;
            if (_board.SelectedNode.Location.X < z.CurrentOrigin.X) z.CurrentOrigin.X -= z.DrawWidth;
            if (_board.SelectedNode.Location.Y < z.CurrentOrigin.Y) z.CurrentOrigin.Y -= z.DrawHeight;
        });
    }
}
