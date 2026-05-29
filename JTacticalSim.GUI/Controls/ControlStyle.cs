using Microsoft.Xna.Framework;

namespace JTacticalSim.GUI.Controls;

/// <summary>
/// Color/visual theme for a popup control. Construct with object initialiser to customise;
/// leave any property unset to use the default game theme.
/// </summary>
public sealed class ControlStyle
{
    public Color Background   { get; init; } = new Color(18, 26, 48);
    public Color Border       { get; init; } = new Color(60, 100, 160);
    public Color TitleText    { get; init; } = Color.White;
    public Color ItemText     { get; init; } = new Color(180, 210, 255);
    public Color SelectedBg   { get; init; } = new Color(50, 85, 155);
    public Color SelectedText { get; init; } = Color.White;
    public Color HintText     { get; init; } = new Color(100, 100, 100);
    public Color Separator    { get; init; } = new Color(60, 100, 160);

    /// <summary>Pixels from the top border to the title text.</summary>
    public int TitlePadTop { get; init; } = 5;
    /// <summary>Pixels of breathing room on each side of a horizontal rule.</summary>
    public int SectionGap  { get; init; } = 5;

    /// <summary>Default dark-navy game theme.</summary>
    public static ControlStyle Default { get; } = new();
}
