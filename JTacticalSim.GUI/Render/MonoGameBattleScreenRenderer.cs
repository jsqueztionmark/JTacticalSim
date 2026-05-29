using JTacticalSim.API.AI;

namespace JTacticalSim.GUI.Render;

public sealed class MonoGameBattleScreenRenderer : BaseScreenRenderer
{
    public MonoGameBattleScreenRenderer(MonoGameRenderer baseRenderer)
        : base(baseRenderer)
    { }

    public override void RenderScreen()                            { }
    public void RenderBattle(IBattle battle)                       { }
    public void RenderBattleRound(IRound round)                    { }
    public void RenderBattleSkirmish(ISkirmish skirmish)           { }
    public void RenderBattleSkirmishOutcome(ISkirmish skirmish)    { }
    public void RenderBattleOutcome(IBattle battle)                { }
    public void RenderBattleRetreat(IBattle battle)                { }
}
