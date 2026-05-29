using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.AI;
using JTacticalSim.API.Component;

namespace JTacticalSim.GUI.Render;

/// <summary>
/// Accumulates battle data during the synchronous engine battle loop (which runs entirely
/// within a single Update() frame) and shows a summary via ModalOverlay once combat ends.
/// RenderScreen() is effectively dead code: the game state transitions back to GAME_IN_PLAY
/// within the same Update() that started the battle, so the draw loop never renders it.
/// </summary>
public sealed class BattleScreenRenderer : BaseScreenRenderer
{
    private readonly List<string> _lines    = new();
    private int                   _lastRound = 0;

    public BattleScreenRenderer(Renderer baseRenderer) : base(baseRenderer) { }

    // ── Called during Update() — store only, no SpriteBatch drawing ──────────

    public void RenderBattle(IBattle battle)
    {
        _lines.Clear();
        _lastRound = 0;

        _lines.Add($"Type:     {FriendlyBattleType(battle.BattleType)}");
        _lines.Add($"Attacker: {battle.AttackerFaction?.Name ?? "Unknown"}");
        _lines.Add($"Defender: {battle.DefenderFaction?.Name ?? "Unknown"}");
        _lines.Add(string.Empty);
    }

    public void RenderBattleRound(IRound round)
    {
        int roundNum = TheGame().CurrentBattle?.CurrentRoundCount ?? 0;
        if (roundNum == _lastRound) return;

        _lastRound = roundNum;
        string roundType = round.CurrentSkirmish?.Type switch
        {
            SkirmishType.AIR_DEFENCE      => "Air Defence",
            SkirmishType.MISSILE_DEFENCE  => "Missile Defence",
            _                             => "Full Combat",
        };
        _lines.Add($"Round {roundNum} - {roundType}");
    }

    public void RenderBattleSkirmish(ISkirmish skirmish)
    {
        string atk    = FormatUnit(skirmish.Attacker);
        string def    = FormatUnit(skirmish.Defender);
        string action = skirmish.Type == SkirmishType.FULL ? "attacks" : "fires on";
        _lines.Add($"  {atk} {action} {def}");
    }

    public void RenderBattleSkirmishOutcome(ISkirmish skirmish)
    {
        try
        {
            var result  = skirmish.GetSkirmishResults();
            string msg  = !string.IsNullOrWhiteSpace(result?.Message)
                              ? result.Message
                              : skirmish.VictoryCondition.ToString();
            if (_lines.Count > 0)
                _lines[_lines.Count - 1] += $"  [{msg}]";
        }
        catch { /* rules service may be unavailable for certain skirmish types */ }
    }

    public void RenderBattleOutcome(IBattle battle)
    {
        _lines.Add(string.Empty);
        _lines.Add($"Result: {FriendlyVictoryCondition(battle.VictoryCondition)}");

        var sb = new StringBuilder();
        foreach (var line in _lines)
            sb.AppendLine(line);

        ((Renderer)TheGame().Renderer).Overlay.ShowReport("Combat Report", sb.ToString());
    }

    public void RenderBattleRetreat(IBattle battle)
    {
        // RenderRetreat is defined on IBattle but not called anywhere in the current
        // engine. No-op for now; if wired in future, use ConfirmAction two-pass pattern
        // and set battle.VictoryCondition = BattleVictoryCondition.RETREAT on confirm.
    }

    // ── RenderScreen: BattleState.Render() target ────────────────────────────

    public override void RenderScreen()
    {
        // The battle resolves entirely within a single Update() frame, so state
        // transitions back to GAME_IN_PLAY before Draw() fires. This is effectively
        // unreachable under normal play.
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string FormatUnit(IUnit unit)
    {
        if (unit == null) return "Unknown";
        string type = unit.UnitInfo?.UnitType?.Name ?? "?";
        return $"{type} {unit.Name}";
    }

    private static string FriendlyBattleType(BattleType t) => t switch
    {
        BattleType.LOCAL             => "Local Combat",
        BattleType.BARRAGE           => "Barrage",
        BattleType.NUCLEAR           => "Nuclear Strike",
        BattleType.FORCED_ENGAGEMENT => "Forced Engagement",
        _                            => t.ToString(),
    };

    private static string FriendlyVictoryCondition(BattleVictoryCondition v) => v switch
    {
        BattleVictoryCondition.ATTACKERS_VICTORIOUS => "Attackers Victorious",
        BattleVictoryCondition.DEFENDERS_VICTORIOUS => "Defenders Victorious",
        BattleVictoryCondition.ALL_DESTROYED        => "All Units Destroyed",
        BattleVictoryCondition.STALEMATE            => "Stalemate",
        BattleVictoryCondition.SURRENDER            => "Surrender",
        BattleVictoryCondition.RETREAT              => "Retreat",
        BattleVictoryCondition.EVADED               => "Evaded",
        _                                           => v.ToString(),
    };
}
