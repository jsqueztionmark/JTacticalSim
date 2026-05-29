using JTacticalSim.API.Component;
using JTacticalSim.API.Game;

namespace JTacticalSim.GUI.Render;

public sealed class MonoGameMainScreenRenderer : BaseScreenRenderer
{
    public MonoGameMainScreenRenderer(MonoGameRenderer baseRenderer)
        : base(baseRenderer)
    { }

    public override void RenderScreen()           { }
    public void LoadContent()                     { }
    public void UnloadContent()                   { }
    public void RenderBoardFrame()                { }
    public void RenderBoard()                     { }
    public void RenderMap(bool clear)             { }
    public void RenderFullMap(bool clear)         { }
    public void RenderTileUnitInfoArea()          { }
    public void RenderNode(INode node, int zoom)  { }
    public void RenderTile(ITile tile, int zoom)  { }
    public void DisplayNodeInfo(INode node)       { }
    public void DisplayUnitInfo(IUnit unit)       { }
    public void RenderUnit(IUnit unit, int zoom)  { }
    public int  RenderUnitStackInfo(IUnitStack stack) => 0;
    public void ResetTileDemographics(IEnumerable<INode> nodes) { }
}
