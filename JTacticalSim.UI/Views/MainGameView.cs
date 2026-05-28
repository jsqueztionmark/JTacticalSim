using System.Text;
using Terminal.Gui.Drawing;
using Terminal.Gui.Drivers;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using JTacticalSim.API;
using JTacticalSim.API.Component;

namespace JTacticalSim.UI.Views;

public class MainGameView : View
{
    private readonly MapView _mapView;
    private readonly FrameView _mapFrame;
    private readonly FrameView _infoFrame;
    private readonly FrameView _reinforcementsFrame;
    private readonly Label _playerInfoLabel;
    private readonly Label _nodeInfoLabel;
    private readonly Label _unitsLabel;
    private readonly Label _reinforcementsLabel;
    private readonly Label _menuLabel;

    public MainGameView()
    {
        X = 0; Y = 0;
        Width = Dim.Fill(); Height = Dim.Fill();
        CanFocus = true;

        // Top menu bar
        _menuLabel = new Label
        {
            Text = "[M]enu  [H]elp  [S]ave  [+/-]Zoom  [Space]Select Unit  [Enter]Move  [E]nd Turn  [Ctrl+Q]Quit",
            X = 0, Y = 0,
            Width = Dim.Fill(), Height = 1
        };
        Add(_menuLabel);

        // Map frame — left side, takes most of the width
        _mapFrame = new FrameView
        {
            Title = "Map",
            X = 0, Y = 1,
            Width = Dim.Percent(65), Height = Dim.Fill(4)
        };
        _mapView = new MapView
        {
            X = 0, Y = 0,
            Width = Dim.Fill(), Height = Dim.Fill()
        };
        _mapView.SelectionChanged += OnMapSelectionChanged;
        _mapFrame.Add(_mapView);
        Add(_mapFrame);

        // Info frame — right side, top portion
        _infoFrame = new FrameView
        {
            Title = "Location / Unit Info",
            X = Pos.Right(_mapFrame), Y = 1,
            Width = Dim.Fill(), Height = Dim.Percent(50)
        };
        _nodeInfoLabel = new Label
        {
            X = 0, Y = 0,
            Width = Dim.Fill(), Height = Dim.Percent(40)
        };
        _unitsLabel = new Label
        {
            X = 0, Y = Pos.Bottom(_nodeInfoLabel),
            Width = Dim.Fill(), Height = Dim.Fill()
        };
        _infoFrame.Add(_nodeInfoLabel, _unitsLabel);
        Add(_infoFrame);

        // Reinforcements frame — right side, bottom portion
        _reinforcementsFrame = new FrameView
        {
            Title = "Available Reinforcements",
            X = Pos.Right(_mapFrame), Y = Pos.Bottom(_infoFrame),
            Width = Dim.Fill(), Height = Dim.Fill(4)
        };
        _reinforcementsLabel = new Label
        {
            X = 0, Y = 0,
            Width = Dim.Fill(), Height = Dim.Fill()
        };
        _reinforcementsFrame.Add(_reinforcementsLabel);
        Add(_reinforcementsFrame);

        // Player info bar — bottom
        _playerInfoLabel = new Label
        {
            X = 0, Y = Pos.AnchorEnd(3),
            Width = Dim.Fill(), Height = 3
        };
        Add(_playerInfoLabel);

        RefreshPlayerInfo();
        RefreshNodeInfo();
        RefreshReinforcements();
        _mapView.CenterOnSelected();
    }

    private void OnMapSelectionChanged(object sender, EventArgs e)
    {
        RefreshNodeInfo();
        SetNeedsDraw();
    }

    private void RefreshPlayerInfo()
    {
        var game = UIContext.Game;
        if (game?.CurrentTurn?.Player == null)
        {
            _playerInfoLabel.Text = "No active game";
            return;
        }

        var player = game.CurrentTurn.Player;
        var country = player.Country;
        var faction = country?.Faction;
        var tracked = player.TrackedValues;

        var sb = new StringBuilder();
        sb.Append($" {country?.Name ?? "?"}");
        if (faction != null)
            sb.Append($"  |  {faction.Name}");
        sb.AppendLine();

        sb.Append($" Nuclear: {tracked?.NuclearCharges ?? 0}");
        sb.Append($"  |  Reinf: {tracked?.ReinforcementPoints ?? 0}");
        if (faction != null)
            sb.Append($"  |  VP: {faction.GetCurrentVictoryPoints() ?? 0}");

        _playerInfoLabel.Text = sb.ToString();
    }

    private void RefreshNodeInfo()
    {
        var board = UIContext.Game?.GameBoard;
        if (board?.SelectedNode == null)
        {
            _nodeInfoLabel.Text = "No node selected";
            _unitsLabel.Text = "";
            return;
        }

        var node = board.SelectedNode;
        var tile = node.DefaultTile;
        var loc = node.Location;

        var sb = new StringBuilder();
        sb.AppendLine($"Location: ({loc.X}, {loc.Y}, {loc.Z})");

        if (tile != null)
        {
            if (!string.IsNullOrEmpty(node.DisplayName))
                sb.AppendLine(node.DisplayName);

            if (tile.Country != null)
                sb.AppendLine($"Held by: {tile.Country.Name}");

            var demos = tile.AllGeography;
            if (demos != null && demos.Count > 0)
            {
                var names = demos
                    .Where(d => d.DemographicClass != null)
                    .Select(d => d.DemographicClass.Name)
                    .Distinct();
                sb.AppendLine(string.Join(", ", names));
            }
        }

        _nodeInfoLabel.Text = sb.ToString();

        // Units at this location
        var unitsSb = new StringBuilder();
        try
        {
            var units = UIContext.Game.JTSServices.UnitService.GetAllUnitsAt(loc);
            if (units != null && units.Count > 0)
            {
                unitsSb.AppendLine($"Units ({units.Count}):");
                foreach (var unit in units)
                {
                    bool selected = board.SelectedUnits != null
                        && board.SelectedUnits.Contains(unit);
                    string marker = selected ? ">> " : "   ";
                    unitsSb.AppendLine($"{marker}{unit.Name}");
                }
            }
        }
        catch { }

        _unitsLabel.Text = unitsSb.ToString();
    }

    private void RefreshReinforcements()
    {
        var player = UIContext.Game?.CurrentTurn?.Player;
        if (player == null)
        {
            _reinforcementsLabel.Text = "";
            return;
        }

        var reinf = player.UnplacedReinforcements;
        if (reinf == null || reinf.Count == 0)
        {
            _reinforcementsLabel.Text = "None";
            return;
        }

        var sb = new StringBuilder();
        foreach (var unit in reinf)
            sb.AppendLine($"  {unit.Name}");

        _reinforcementsLabel.Text = sb.ToString();
    }

    protected override bool OnKeyDown(Key key)
    {
        if (key.KeyCode == (KeyCode.Q | KeyCode.CtrlMask))
        {
            key.Handled = true;
            UIContext.Instance.App.RequestStop(UIContext.Instance.Shell);
            return true;
        }

        // End turn
        if (key.KeyCode == KeyCode.E)
        {
            key.Handled = true;
            EndTurn();
            return true;
        }

        // Save game
        if (key.KeyCode == KeyCode.S)
        {
            key.Handled = true;
            SaveGame();
            return true;
        }

        // Help
        if (key.KeyCode == KeyCode.H)
        {
            key.Handled = true;
            UIContext.Game?.StateSystem.ChangeState(StateType.HELP);
            return true;
        }

        // Select/deselect units at current node
        if (key.KeyCode == KeyCode.Space)
        {
            key.Handled = true;
            ToggleUnitSelection();
            return true;
        }

        // Move selected units
        if (key.KeyCode == KeyCode.Enter)
        {
            key.Handled = true;
            MoveSelectedUnits();
            return true;
        }

        return base.OnKeyDown(key);
    }

    private void EndTurn()
    {
        var game = UIContext.Game;
        if (game?.CurrentTurn == null) return;

        if (!UIContext.Game.Renderer.ConfirmAction("End your turn?"))
            return;

        game.CurrentTurn.End();
        RefreshPlayerInfo();
        RefreshNodeInfo();
        RefreshReinforcements();
        _mapView.CenterOnSelected();
        _mapView.SetNeedsDraw();
    }

    private void SaveGame()
    {
        var game = UIContext.Game;
        if (game?.LoadedGame == null) return;

        var result = game.Save(game.LoadedGame);
        game.Renderer.DisplayUserMessage(
            result.Status == ResultStatus.SUCCESS
                ? MessageDisplayType.INFO
                : MessageDisplayType.ERROR,
            result.Message,
            result.ex);
    }

    private void ToggleUnitSelection()
    {
        var board = UIContext.Game?.GameBoard;
        if (board?.SelectedNode == null) return;

        var units = UIContext.Game.JTSServices.UnitService.GetAllUnitsAt(board.SelectedNode.Location);
        if (units == null || units.Count == 0) return;

        if (board.SelectedUnits != null && board.SelectedUnits.Count > 0)
        {
            board.SelectedUnits.Clear();
            board.ClearCurrentRoute();
        }
        else
        {
            var friendly = units.Where(u => u.Country?.Faction != null
                && UIContext.Game.CurrentTurn?.Player?.Country?.Faction != null
                && u.Country.Faction.ID == UIContext.Game.CurrentTurn.Player.Country.Faction.ID)
                .ToList();

            if (friendly.Count > 0)
            {
                board.SelectedUnits = friendly;
                UIContext.Game.GameBoard.SetCurrentRoute(RouteType.FASTESTUNIT);
            }
        }

        RefreshNodeInfo();
        _mapView.SetNeedsDraw();
    }

    private void MoveSelectedUnits()
    {
        var board = UIContext.Game?.GameBoard;
        if (board?.SelectedUnits == null || board.SelectedUnits.Count == 0) return;
        if (board.CurrentRoute == null) return;

        foreach (var unit in board.SelectedUnits.ToList())
        {
            var sourceNode = UIContext.Game.JTSServices.NodeService.GetNodeAt(unit.Location);
            var moveResult = unit.MoveToLocation(board.SelectedNode, sourceNode);
            if (moveResult.Status == ResultStatus.EXCEPTION)
            {
                UIContext.Game.Renderer.DisplayUserMessage(
                    MessageDisplayType.ERROR, moveResult.Message, moveResult.ex);
                break;
            }
        }

        board.SelectedUnits.Clear();
        board.ClearCurrentRoute();
        RefreshNodeInfo();
        _mapView.SetNeedsDraw();
    }
}
