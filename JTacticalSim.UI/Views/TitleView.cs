using System.Collections.ObjectModel;
using Terminal.Gui.App;
using Terminal.Gui.Drivers;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Component.Util;
using JTacticalSim.Component.Data;

namespace JTacticalSim.UI.Views;

public class GameTitleView : View
{
    private ListView _savedGamesList;
    private ListView _scenarioList;
    private Label _scenarioInfoLabel;

    private ObservableCollection<string> _savedGamesSource = new();
    private ObservableCollection<string> _scenariosSource = new();

    // index 0 in _savedGamesSource is the "Create New Game" sentinel — no backing object
    private List<ISavedGame> _savedGames = new();
    private List<IScenario> _scenarios = new();

    public GameTitleView()
    {
        X = 0; Y = 0;
        Width = Dim.Fill(); Height = Dim.Fill();
        CanFocus = true;

        BuildLayout();
        LoadData();
    }

    private void BuildLayout()
    {
        Add(new Label
        {
            Text = "J TACTICAL SIM — Battle Simulator",
            X = Pos.Center(), Y = 1
        });

        Add(new Label
        {
            Text = "Design and Development: Jeff Storm",
            X = Pos.Center(), Y = 2
        });

        var savedGamesFrame = new FrameView
        {
            Title = "Choose a Game",
            X = Pos.Percent(2), Y = 4,
            Width = Dim.Percent(35), Height = Dim.Percent(65)
        };
        _savedGamesList = new ListView
        {
            X = 0, Y = 0,
            Width = Dim.Fill(), Height = Dim.Fill(),
            CanFocus = true
        };
        _savedGamesList.SetSource(_savedGamesSource);
        savedGamesFrame.Add(_savedGamesList);
        Add(savedGamesFrame);

        var scenariosFrame = new FrameView
        {
            Title = "Choose a Scenario",
            X = Pos.Right(savedGamesFrame) + 1, Y = 4,
            Width = Dim.Percent(35), Height = Dim.Percent(65)
        };
        _scenarioList = new ListView
        {
            X = 0, Y = 0,
            Width = Dim.Fill(), Height = Dim.Fill(),
            CanFocus = true
        };
        _scenarioList.SetSource(_scenariosSource);
        _scenarioList.ValueChanged += OnScenarioSelectionChanged;
        _scenarioList.Enabled = false;
        scenariosFrame.Add(_scenarioList);
        Add(scenariosFrame);

        var infoFrame = new FrameView
        {
            Title = "Scenario Info",
            X = Pos.Right(scenariosFrame) + 1, Y = 4,
            Width = Dim.Fill(2), Height = Dim.Percent(65)
        };
        _scenarioInfoLabel = new Label
        {
            X = 0, Y = 0,
            Width = Dim.Fill(), Height = Dim.Fill()
        };
        infoFrame.Add(_scenarioInfoLabel);
        Add(infoFrame);

        Add(new Label
        {
            Text = "[Enter] Select  [Tab] Switch panel  [Ctrl+Q] Quit",
            X = Pos.Center(), Y = Pos.AnchorEnd(1)
        });
    }

    private void LoadData()
    {
        try
        {
            _savedGamesSource.Add(">> Create New Game <<");

            _savedGames = UIContext.Game.JTSServices.GameService
                .GetSavedGames()
                .OrderBy(g => g.Name)
                .ToList();

            foreach (var g in _savedGames)
                _savedGamesSource.Add(g.Name);

            _scenarios = UIContext.Game.JTSServices.GameService
                .GetScenarios()
                .OrderBy(s => s.Name)
                .ToList();

            foreach (var s in _scenarios)
                _scenariosSource.Add(s.Name);
        }
        catch (Exception ex)
        {
            _savedGamesSource.Add($"(error loading: {ex.Message})");
        }

        _savedGamesList.SetFocus();
    }

    private void OnScenarioSelectionChanged(object sender, ValueChangedEventArgs<int?> args)
    {
        int idx = args.NewValue ?? -1;
        if (idx >= 0 && idx < _scenarios.Count)
            _scenarioInfoLabel.Text = _scenarios[idx].TextInfo() ?? string.Empty;
    }

    protected override bool OnKeyDown(Key key)
    {
        // Ctrl+Q — quit
        if (key.KeyCode == (KeyCode.Q | KeyCode.CtrlMask))
        {
            key.Handled = true;
            UIContext.Instance.App.RequestStop(UIContext.Instance.Shell);
            return true;
        }

        // Enter on saved-games list
        if (key.KeyCode == KeyCode.Enter && _savedGamesList.HasFocus)
        {
            key.Handled = true;
            int idx = _savedGamesList.SelectedItem ?? -1;
            if (idx == 0)
            {
                _scenarioList.Enabled = true;
                _scenarioList.SetFocus();
            }
            else if (idx > 0 && idx - 1 < _savedGames.Count)
                LoadGame(_savedGames[idx - 1].Name);
            return true;
        }

        // Enter on scenario list — prompt for game name
        if (key.KeyCode == KeyCode.Enter && _scenarioList.HasFocus)
        {
            key.Handled = true;
            int idx = _scenarioList.SelectedItem ?? -1;
            if (idx >= 0 && idx < _scenarios.Count)
                PromptNewGameName(_scenarios[idx]);
            return true;
        }

        // Esc on scenario list — back to saved-games list, reset scenario state
        if (key.KeyCode == KeyCode.Esc && _scenarioList.HasFocus)
        {
            key.Handled = true;
            _scenarioList.SelectedItem = null;
            _scenarioList.Enabled = false;
            _scenarioInfoLabel.Text = string.Empty;
            _savedGamesList.SetFocus();
            return true;
        }

        return base.OnKeyDown(key);
    }

    private void PromptNewGameName(IScenario scenario)
    {
        var dialog = new Dialog
        {
            Title = "New Game Name",
            Width = 50, Height = 8
        };

        var nameField = new TextField
        {
            X = 1, Y = 1,
            Width = Dim.Fill(1)
        };
        dialog.Add(nameField);

        var ok = new Button { Text = "OK", X = Pos.Center() - 6, Y = 4 };
        var cancel = new Button { Text = "Cancel", X = Pos.Center() + 2, Y = 4 };

        ok.Accepted += (s, e) =>
        {
            var name = nameField.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(name))
            {
                UIContext.Instance.App.RequestStop(dialog);
                CreateNewGame(scenario, name);
            }
        };
        cancel.Accepted += (s, e) => UIContext.Instance.App.RequestStop(dialog);

        dialog.Add(ok, cancel);
        UIContext.Instance.App.Run(dialog);
    }

    private void CreateNewGame(IScenario scenario, string gameName)
    {
        var newGame = new SavedGame
        {
            Name = gameName,
            GameFileDirectory = gameName,
            LastPlayed = false,
            Scenario = scenario
        };

        newGame.SetNextID();

        var sResult = UIContext.Game.JTSServices.GameService.SaveSavedGame(newGame);
        if (sResult.Status == ResultStatus.EXCEPTION)
        {
            MessageBox.ErrorQuery(UIContext.Instance.App, "Error", sResult.Message, "OK");
            return;
        }

        var cResult = UIContext.Game.SaveAs(scenario, newGame);
        if (cResult.Status == ResultStatus.EXCEPTION)
        {
            MessageBox.ErrorQuery(UIContext.Instance.App, "Error", cResult.Message, "OK");
            return;
        }

        UIContext.Game.StateSystem.ChangeState(StateType.TITLE_MENU);
    }

    private void LoadGame(string name)
    {
        UIContext.Instance.LoadAndStartGame(name);
    }
}
