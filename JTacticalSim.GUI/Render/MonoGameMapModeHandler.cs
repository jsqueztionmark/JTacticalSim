using JTacticalSim.API;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;

namespace JTacticalSim.GUI.Render;

public sealed class MonoGameMapModeHandler : BaseGameObject, IMapModeHandler
{
    private readonly MapMode MAX = Enum.GetValues(typeof(MapMode)).Cast<MapMode>().Max();
    private readonly MapMode MIN = Enum.GetValues(typeof(MapMode)).Cast<MapMode>().Min();

    private List<MapModeInfo> _mapModes { get; set; }
    public MapModeInfo CurrentMapMode => _mapModes.SingleOrDefault(mm => mm.IsCurrent);

    public MonoGameMapModeHandler()
        : base(GameObjectType.HANDLER)
    {
        _mapModes = new List<MapModeInfo>();
    }

    public void LoadMapModes(IEnumerable<MapModeInfo> mapModes)
    {
        foreach (var mode in mapModes)
            if (!_mapModes.Any(mm => mm.MapMode == mode.MapMode))
                _mapModes.Add(mode);
    }

    public bool CycleMapMode(CycleDirection direction)
    {
        var old = CurrentMapMode;
        if (direction == CycleDirection.UP && CurrentMapMode.MapMode != MAX)
        {
            CurrentMapMode.IsCurrent = false;
            _mapModes.Single(mm => mm.MapMode == old.MapMode + 1).IsCurrent = true;
            return true;
        }
        if (direction == CycleDirection.DOWN && CurrentMapMode.MapMode != MIN)
        {
            CurrentMapMode.IsCurrent = false;
            _mapModes.Single(mm => mm.MapMode == old.MapMode - 1).IsCurrent = true;
            return true;
        }
        return false;
    }
}
