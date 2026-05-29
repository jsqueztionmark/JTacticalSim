namespace JTacticalSim.GUI.Controls;

/// <summary>A labelled item in a PopupList or SelectPanel.</summary>
/// <param name="IsHeader">When true the item is a non-selectable section header.</param>
public readonly record struct ListItem<T>(string Label, T Value, bool IsHeader = false);
