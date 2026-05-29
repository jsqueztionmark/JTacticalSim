using System.Transactions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.Component.Data;

namespace JTacticalSim.GUI.Render;

public sealed class MonoGameTitleScreenRenderer : BaseScreenRenderer
{
    private enum TitlePhase { SelectGame, SelectScenario, EnterName }

    private TitlePhase _phase = TitlePhase.SelectGame;
    private List<ISavedGame> _savedGames;
    private List<IScenario> _scenarios;
    private IScenario _selectedScenario;

    private int _gameIndex = 0;
    private int _gameScrollOffset = 0;
    private int _scenarioIndex = 0;
    private int _scenarioScrollOffset = 0;
    private string _newGameName = string.Empty;
    private string _validationMessage = string.Empty;

    // Layout (1280x800 window)
    private const int BoxWidth    = 500;
    private const int BoxLeft     = (1280 - BoxWidth) / 2;
    private const int CreditsTop  = 60;
    private const int CreditsHeight = 96;
    private const int ListTop     = 185;
    private const int ListHeight  = 370;
    private const int NameTop     = 575;
    private const int NameHeight  = 96;
    private const int LineHeight  = 20;
    private const int PageSize    = 15;

    private static readonly Color BoxFill    = new Color(12, 22, 42);
    private static readonly Color BoxBorder  = new Color(60, 100, 160);
    private static readonly Color CaptionCol = Color.White;
    private static readonly Color TextNormal = new Color(200, 200, 200);
    private static readonly Color TextHilight = Color.Yellow;
    private static readonly Color TextDim    = new Color(100, 100, 100);
    private static readonly Color RowHilight = new Color(35, 55, 95);
    private static readonly Color TextError  = new Color(220, 60, 60);

    public MonoGameTitleScreenRenderer(MonoGameRenderer baseRenderer)
        : base(baseRenderer)
    { }

    // ── Data ────────────────────────────────────────────────────────────────────

    private void EnsureData()
    {
        if (_savedGames != null) return;
        _savedGames = TheGame().JTSServices.GameService.GetSavedGames()
            .OrderBy(sg => sg.Name).ToList();
        _scenarios = TheGame().JTSServices.GameService.GetScenarios()
            .OrderBy(s => s.Name).ToList();
    }

    private void ReloadData()
    {
        _savedGames = null;
        EnsureData();
    }

    // ── Rendering ───────────────────────────────────────────────────────────────

    public override void RenderScreen()
    {
        EnsureData();

        var sb  = _baseRenderer.SpriteBatch;
        var fnt = _baseRenderer.Font;
        var px  = _baseRenderer.Pixel;

        if (sb == null || fnt == null || px == null) return;

        DrawCreditsBox(sb, fnt, px);

        switch (_phase)
        {
            case TitlePhase.SelectGame:
                DrawGameList(sb, fnt, px);
                break;
            case TitlePhase.SelectScenario:
                DrawScenarioList(sb, fnt, px);
                break;
            case TitlePhase.EnterName:
                DrawScenarioList(sb, fnt, px);
                DrawNameInput(sb, fnt, px);
                break;
        }
    }

    private void DrawCreditsBox(SpriteBatch sb, SpriteFont fnt, Texture2D px)
    {
        DrawBox(sb, px, fnt, BoxLeft, CreditsTop, BoxWidth, CreditsHeight, "JTacticalSim Battle Simulator");
        int y = CreditsTop + 24;
        DrawText(sb, fnt, $"JTacticalSim  (c)2013-{System.DateTime.Now.Year}", BoxLeft + 12, y, TextNormal);
        y += LineHeight;
        DrawText(sb, fnt, "Design and Development - Jeff Storm", BoxLeft + 12, y, TextNormal);
        y += LineHeight;
        DrawText(sb, fnt, "jsqueztionmark@gmail.com", BoxLeft + 12, y, TextNormal);
    }

    private void DrawGameList(SpriteBatch sb, SpriteFont fnt, Texture2D px)
    {
        // Items: index 0 = "Create New Game", index 1 = separator, index 2+ = saved games
        int totalItems = 2 + _savedGames.Count;
        DrawBox(sb, px, fnt, BoxLeft, ListTop, BoxWidth, ListHeight, "Choose a Game");

        int y = ListTop + 24;
        int end = Math.Min(_gameScrollOffset + PageSize, totalItems);

        for (int i = _gameScrollOffset; i < end; i++)
        {
            string label = GameItemLabel(i);
            if (string.IsNullOrEmpty(label)) { y += LineHeight; continue; }

            bool selected = (i == _gameIndex);
            if (selected)
                sb.Draw(px, new Rectangle(BoxLeft + 2, y - 1, BoxWidth - 4, LineHeight + 1), RowHilight);

            DrawText(sb, fnt, label, BoxLeft + 12, y, selected ? TextHilight : TextNormal);
            y += LineHeight;
        }

        if (totalItems > PageSize)
            DrawText(sb, fnt, "Up/Down: scroll   Enter: select", BoxLeft + 12, ListTop + ListHeight - 22, TextDim);
    }

    private void DrawScenarioList(SpriteBatch sb, SpriteFont fnt, Texture2D px)
    {
        DrawBox(sb, px, fnt, BoxLeft, ListTop, BoxWidth, ListHeight, "Choose a Scenario");

        int y = ListTop + 24;
        int end = Math.Min(_scenarioScrollOffset + PageSize, _scenarios.Count);

        for (int i = _scenarioScrollOffset; i < end; i++)
        {
            bool selected = (i == _scenarioIndex);
            if (selected)
                sb.Draw(px, new Rectangle(BoxLeft + 2, y - 1, BoxWidth - 4, LineHeight + 1), RowHilight);

            DrawText(sb, fnt, _scenarios[i].Name, BoxLeft + 12, y, selected ? TextHilight : TextNormal);
            y += LineHeight;
        }

        if (_scenarios.Count > PageSize)
            DrawText(sb, fnt, "Up/Down: scroll   Enter: select   Esc: back", BoxLeft + 12, ListTop + ListHeight - 22, TextDim);
        else
            DrawText(sb, fnt, "Enter: select   Esc: back", BoxLeft + 12, ListTop + ListHeight - 22, TextDim);
    }

    private void DrawNameInput(SpriteBatch sb, SpriteFont fnt, Texture2D px)
    {
        DrawBox(sb, px, fnt, BoxLeft, NameTop, BoxWidth, NameHeight, "New Game Name");
        DrawText(sb, fnt, _newGameName + "|", BoxLeft + 12, NameTop + 28, Color.White);
        if (!string.IsNullOrEmpty(_validationMessage))
            DrawText(sb, fnt, _validationMessage, BoxLeft + 12, NameTop + 52, TextError);
        DrawText(sb, fnt, "Enter: confirm   Esc: back", BoxLeft + 12, NameTop + NameHeight - 22, TextDim);
    }

    // ── Input ───────────────────────────────────────────────────────────────────

    public void HandleInput(KeyboardState current, KeyboardState previous)
    {
        EnsureData();
        switch (_phase)
        {
            case TitlePhase.SelectGame:     HandleGameListInput(current, previous);    break;
            case TitlePhase.SelectScenario: HandleScenarioListInput(current, previous); break;
            case TitlePhase.EnterName:      HandleNameInput(current, previous);        break;
        }
    }

    private void HandleGameListInput(KeyboardState current, KeyboardState previous)
    {
        int totalItems = 2 + _savedGames.Count;

        if (JustPressed(current, previous, Keys.Up))
        {
            _gameIndex = Math.Max(0, _gameIndex - 1);
            if (_gameIndex == 1) _gameIndex = 0;
            if (_gameIndex < _gameScrollOffset) _gameScrollOffset = _gameIndex;
        }
        else if (JustPressed(current, previous, Keys.Down))
        {
            _gameIndex = Math.Min(totalItems - 1, _gameIndex + 1);
            if (_gameIndex == 1) _gameIndex = 2;
            if (_gameIndex >= totalItems) _gameIndex = totalItems - 1;
            if (_gameIndex >= _gameScrollOffset + PageSize) _gameScrollOffset++;
        }
        else if (JustPressed(current, previous, Keys.Enter) && _gameIndex != 1)
        {
            if (_gameIndex == 0)
            {
                _phase = TitlePhase.SelectScenario;
                _scenarioIndex = 0;
                _scenarioScrollOffset = 0;
            }
            else
            {
                LoadAndStartGame(_savedGames[_gameIndex - 2].Name);
            }
        }
    }

    private void HandleScenarioListInput(KeyboardState current, KeyboardState previous)
    {
        if (_scenarios.Count == 0) return;

        if (JustPressed(current, previous, Keys.Up))
        {
            _scenarioIndex = Math.Max(0, _scenarioIndex - 1);
            if (_scenarioIndex < _scenarioScrollOffset) _scenarioScrollOffset = _scenarioIndex;
        }
        else if (JustPressed(current, previous, Keys.Down))
        {
            _scenarioIndex = Math.Min(_scenarios.Count - 1, _scenarioIndex + 1);
            if (_scenarioIndex >= _scenarioScrollOffset + PageSize) _scenarioScrollOffset++;
        }
        else if (JustPressed(current, previous, Keys.Enter))
        {
            _selectedScenario = _scenarios[_scenarioIndex];
            _newGameName = string.Empty;
            _validationMessage = string.Empty;
            _phase = TitlePhase.EnterName;
        }
        else if (JustPressed(current, previous, Keys.Escape))
        {
            _phase = TitlePhase.SelectGame;
        }
    }

    private void HandleNameInput(KeyboardState current, KeyboardState previous)
    {
        if (JustPressed(current, previous, Keys.Escape))
        {
            _phase = TitlePhase.SelectScenario;
            _validationMessage = string.Empty;
            return;
        }

        if (JustPressed(current, previous, Keys.Back))
        {
            if (_newGameName.Length > 0)
                _newGameName = _newGameName[..^1];
            _validationMessage = string.Empty;
            return;
        }

        if (JustPressed(current, previous, Keys.Enter))
        {
            if (string.IsNullOrWhiteSpace(_newGameName))
            {
                _validationMessage = "Game name cannot be empty";
                return;
            }
            var v = TheGame().JTSServices.RulesService.NameIsValid<SavedGame>(_newGameName);
            if (!v.Result)
            {
                _validationMessage = v.Message;
                return;
            }
            CreateAndLoadGame(_selectedScenario, _newGameName);
            return;
        }

        if (_newGameName.Length >= 30) return;

        bool shift = current.IsKeyDown(Keys.LeftShift) || current.IsKeyDown(Keys.RightShift);
        foreach (var key in current.GetPressedKeys())
        {
            if (previous.IsKeyDown(key)) continue;
            char ch = KeyToChar(key, shift);
            if (ch != '\0') _newGameName += ch;
        }
    }

    // ── Game loading/creation ────────────────────────────────────────────────────

    private void LoadAndStartGame(string name)
    {
        TheGame().LoadGame(name);
        TheGame().Start();
    }

    private void CreateAndLoadGame(IScenario scenario, string name)
    {
        var newGame = new SavedGame
        {
            Name = name,
            GameFileDirectory = name,
            LastPlayed = false,
            Scenario = scenario
        };
        newGame.SetNextID();

        using (var txn = new TransactionScope())
        {
            TheGame().JTSServices.GameService.SaveSavedGame(newGame);
            TheGame().SaveAs(scenario, newGame);
            txn.Complete();
        }

        ReloadData();
        _phase = TitlePhase.SelectGame;
        _gameIndex = 0;
        _gameScrollOffset = 0;
        TheGame().StateSystem.ChangeState(StateType.TITLE_MENU);
    }

    // ── Drawing helpers ──────────────────────────────────────────────────────────

    private void DrawBox(SpriteBatch sb, Texture2D px, SpriteFont fnt,
                         int x, int y, int w, int h, string caption)
    {
        sb.Draw(px, new Rectangle(x, y, w, h), BoxFill);
        sb.Draw(px, new Rectangle(x,         y,         w, 1), BoxBorder);
        sb.Draw(px, new Rectangle(x,         y + h - 1, w, 1), BoxBorder);
        sb.Draw(px, new Rectangle(x,         y,         1, h), BoxBorder);
        sb.Draw(px, new Rectangle(x + w - 1, y,         1, h), BoxBorder);

        if (fnt != null && !string.IsNullOrEmpty(caption))
        {
            var sz = fnt.MeasureString(caption);
            int cx = x + (w - (int)sz.X) / 2;
            sb.DrawString(fnt, caption, new Vector2(cx, y + 4), CaptionCol);
        }
    }

    private static void DrawText(SpriteBatch sb, SpriteFont fnt, string text, int x, int y, Color color)
    {
        if (fnt == null || string.IsNullOrEmpty(text)) return;
        sb.DrawString(fnt, text, new Vector2(x, y), color);
    }

    // ── Utilities ────────────────────────────────────────────────────────────────

    private string GameItemLabel(int index)
    {
        if (index == 0) return "  Create New Game";
        if (index == 1) return null;
        return "  " + _savedGames[index - 2].Name;
    }

    private static bool JustPressed(KeyboardState current, KeyboardState previous, Keys key)
        => current.IsKeyDown(key) && !previous.IsKeyDown(key);

    private static char KeyToChar(Keys key, bool shift)
    {
        if (key >= Keys.A && key <= Keys.Z)
        {
            char c = (char)('a' + (key - Keys.A));
            return shift ? char.ToUpper(c) : c;
        }
        if (key >= Keys.D0 && key <= Keys.D9)
        {
            if (!shift) return (char)('0' + (key - Keys.D0));
            return ")!@#$%^&*("[key - Keys.D0];
        }
        if (key == Keys.Space)    return ' ';
        if (key == Keys.OemMinus) return shift ? '_' : '-';
        if (key == Keys.OemPeriod) return '.';
        return '\0';
    }
}
