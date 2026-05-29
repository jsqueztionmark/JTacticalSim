using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.Component;
using JTacticalSim.Component.GameBoard;
using JTacticalSim.GUI.Controls;

namespace JTacticalSim.GUI.Render;

public sealed class ReinforcementsScreenRenderer : BaseScreenRenderer
{
    // ── Colors ───────────────────────────────────────────────────────────────

    private static readonly Color ColOverlay   = new(10, 15, 25);
    private static readonly Color ColPanelBg   = new(18, 28, 50);
    private static readonly Color ColBorder    = new(60, 100, 160);
    private static readonly Color ColLabel     = new(160, 180, 210);
    private static readonly Color ColValue     = Color.Yellow;
    private static readonly Color ColCostOk    = new(255, 120, 100);
    private static readonly Color ColCostOver  = new(220, 30, 30);   // cost > remaining RP
    private static readonly Color ColRpLabel   = new(180, 220, 180);
    private static readonly Color ColRpValue   = new(100, 220, 100);

    // ── Layout ───────────────────────────────────────────────────────────────

    private const int MarginY  = 30;
    private const int PreviewH = 52;
    private const int ListY    = MarginY + PreviewH + 10;
    private const int Col1X    = 30;
    private const int Col2X    = 530;
    private const int Col3X    = 1030;

    // ── Wizard state ─────────────────────────────────────────────────────────

    private enum Step { UnitType, UnitClass, UnitGroupType, Name }
    private Step _currentStep = Step.UnitType;

    private IUnit _workingUnit;
    private bool  _initialized;

    // ── Controls ─────────────────────────────────────────────────────────────

    private readonly PopupList<IUnitType>      _typeList = new()
    {
        Title      = "Unit Type",
        MinWidth   = 240,
        MaxVisible = 18,
        HintText   = "Up/Down: Navigate    Enter: Select    Esc: Cancel",
        Style      = ControlStyle.Default,
    };

    private readonly PopupList<IUnitClass>     _classList = new()
    {
        Title      = "Unit Class",
        MinWidth   = 240,
        MaxVisible = 18,
        HintText   = "Up/Down: Navigate    Enter: Select    Esc: Back",
        Style      = ControlStyle.Default,
    };

    private readonly PopupList<IUnitGroupType> _groupList = new()
    {
        Title      = "Unit Group Type",
        MinWidth   = 240,
        MaxVisible = 12,
        HintText   = "Up/Down: Navigate    Enter: Select    Esc: Back",
        Style      = ControlStyle.Default,
    };

    private readonly TextInputField _nameField = new()
    {
        Label     = "Unit Name",
        MaxLength = 30,
        Style     = ControlStyle.Default,
        PadX      = 10,
    };

    // ── Constructor ──────────────────────────────────────────────────────────

    public ReinforcementsScreenRenderer(Renderer baseRenderer) : base(baseRenderer)
    {
        // Real-time preview: update working unit as the player navigates each list
        _typeList.SelectionChanged  += item => { if (_workingUnit != null) _workingUnit.UnitInfo.UnitType      = item.Value; };
        _classList.SelectionChanged += item => { if (_workingUnit != null) _workingUnit.UnitInfo.UnitClass     = item.Value; };
        _groupList.SelectionChanged += item => { if (_workingUnit != null) _workingUnit.UnitInfo.UnitGroupType = item.Value; };

        _typeList.ItemSelected  += OnTypeSelected;
        _typeList.Closed        += () => CloseScreen();

        _classList.ItemSelected += OnClassSelected;
        _classList.Closed       += () => GoToStep(Step.UnitType);

        _groupList.ItemSelected += OnGroupTypeSelected;
        _groupList.Closed       += () => GoToStep(Step.UnitClass);

        _nameField.TextConfirmed += OnNameConfirmed;
        _nameField.Cancelled     += () => GoToStep(Step.UnitGroupType);
        _nameField.Validate      = ValidateName;
    }

    // ── Lifecycle ────────────────────────────────────────────────────────────

    public override void CloseScreen()
    {
        _initialized = false;
        base.CloseScreen();
    }

    // ── Rendering ────────────────────────────────────────────────────────────

    public override void RenderScreen()
    {
        var sb  = _baseRenderer.SpriteBatch;
        var fnt = _baseRenderer.Font;
        var px  = _baseRenderer.Pixel;
        if (sb == null || fnt == null || px == null) return;

        var gd   = _baseRenderer.GraphicsDevice;
        int winW = gd.Viewport.Width;
        int winH = gd.Viewport.Height;

        if (!_initialized) Initialize(fnt);

        FillRect(sb, px, 0, 0, winW, winH, ColOverlay);
        DrawPreviewBar(sb, fnt, px, winW);

        _typeList.Draw(sb, fnt, px);
        if (_currentStep >= Step.UnitClass)     _classList.Draw(sb, fnt, px);
        if (_currentStep >= Step.UnitGroupType) _groupList.Draw(sb, fnt, px);
        if (_currentStep == Step.Name)          _nameField.Draw(sb, fnt, px);
    }

    // ── Input ────────────────────────────────────────────────────────────────

    public void HandleInput(KeyboardState cur, KeyboardState prev)
    {
        switch (_currentStep)
        {
            case Step.UnitType:      _typeList.HandleInput(cur, prev);   break;
            case Step.UnitClass:     _classList.HandleInput(cur, prev);  break;
            case Step.UnitGroupType: _groupList.HandleInput(cur, prev);  break;
            case Step.Name:          _nameField.HandleInput(cur, prev);  break;
        }
    }

    // ── Initialisation ───────────────────────────────────────────────────────

    private void Initialize(SpriteFont fnt)
    {
        StartNewUnit();
        GoToStep(Step.UnitType, fnt);
        _initialized = true;
    }

    private void StartNewUnit()
    {
        var game = TheGame();
        var ui   = new UnitInfo(null, null, null);
        _workingUnit = game.JTSServices.UnitService
            .CreateUnit(string.Empty, null, game.CurrentTurn.Player.Country, ui)
            .SuccessfulObjects.First();
        _workingUnit.UnitInfo = ui;
    }

    private void GoToStep(Step step, SpriteFont fnt = null)
    {
        fnt ??= _baseRenderer.Font;
        _currentStep = step;

        var game = TheGame();

        switch (step)
        {
            case Step.UnitType:
            {
                // Clear class/grouptype so cost preview resets cleanly
                if (_workingUnit != null)
                {
                    _workingUnit.UnitInfo.UnitClass     = null;
                    _workingUnit.UnitInfo.UnitGroupType = null;
                }
                var types = game.JTSServices.DataService
                    .GetAllowedUnitTypes(game.CurrentTurn.Player.Country)
                    .OrderBy(t => t.Name)
                    .Select(t => new ListItem<IUnitType>(t.Name, t))
                    .ToList();
                _typeList.Open(types, Col1X, ListY, fnt);
                // Seed working unit with the initial highlighted type for live cost preview
                if (types.Any() && _workingUnit != null)
                    _workingUnit.UnitInfo.UnitType = types[0].Value;
                break;
            }

            case Step.UnitClass:
            {
                if (_workingUnit != null) _workingUnit.UnitInfo.UnitGroupType = null;
                var classes = game.JTSServices.UnitService
                    .GetAllowableUnitClassesForUnitType(_workingUnit.UnitInfo.UnitType).Result
                    .OrderBy(c => c.Name)
                    .Select(c => new ListItem<IUnitClass>(c.Name, c))
                    .ToList();
                _classList.Open(classes, Col2X, ListY, fnt);
                if (classes.Any() && _workingUnit != null)
                    _workingUnit.UnitInfo.UnitClass = classes[0].Value;
                break;
            }

            case Step.UnitGroupType:
            {
                var groups = game.JTSServices.DataService
                    .GetAllowedUnitGroupTypes()
                    .OrderBy(g => g.Name)
                    .Select(g => new ListItem<IUnitGroupType>(g.Name, g))
                    .ToList();
                _groupList.Open(groups, Col3X, ListY, fnt);
                if (groups.Any() && _workingUnit != null)
                    _workingUnit.UnitInfo.UnitGroupType = groups[0].Value;
                break;
            }

            case Step.Name:
            {
                var gd = _baseRenderer.GraphicsDevice;
                _nameField.Open(Col1X, gd.Viewport.Height - 80, gd.Viewport.Width - Col1X * 2, 46);
                break;
            }
        }
    }

    // ── Preview bar ──────────────────────────────────────────────────────────

    private void DrawPreviewBar(SpriteBatch sb, SpriteFont fnt, Texture2D px, int winW)
    {
        DrawPanel(sb, px, Col1X, MarginY, winW - Col1X * 2, PreviewH, ColPanelBg, ColBorder);

        int lh = GetLineH(fnt);
        int y  = MarginY + (PreviewH - lh) / 2;
        int x  = Col1X + 12;
        const int TabW = 280;

        void Field(string label, string value)
        {
            DrawText(sb, fnt, label, x, y, ColLabel);
            DrawText(sb, fnt, value ?? "-", x + (int)fnt.MeasureString(label + " ").X, y, ColValue);
            x += TabW;
        }
        
        Field("Unit Type:",   _workingUnit?.UnitInfo?.UnitType?.Name);
        Field("Unit Class:",  _workingUnit?.UnitInfo?.UnitClass?.Name);
        Field("Group Type:", _workingUnit?.UnitInfo?.UnitGroupType?.Name);

        // Point cost vs. accumulated RP budget
        double cost       = _workingUnit?.UnitCost ?? 0;
        int    remaining  = TheGame().CurrentTurn.Player.TrackedValues?.ReinforcementPoints ?? 0;
        bool   overBudget = cost > remaining;

        DrawText(sb, fnt, "Cost:", x, y, ColLabel);
        x += (int)fnt.MeasureString("Cost: ").X;
        DrawText(sb, fnt, cost.ToString("F0"), x, y, overBudget ? ColCostOver : ColCostOk);
        x += (int)fnt.MeasureString("999  ").X;

        DrawText(sb, fnt, "RP Remaining:", x, y, ColRpLabel);
        x += (int)fnt.MeasureString("RP Remaining: ").X;
        DrawText(sb, fnt, remaining.ToString(), x, y, ColRpValue);
    }

    // ── RP check helper ──────────────────────────────────────────────────────

    private bool CostExceedsRP()
    {
        double cost      = _workingUnit?.UnitCost ?? 0;
        int    remaining = TheGame().CurrentTurn.Player.TrackedValues?.ReinforcementPoints ?? 0;
        return cost > remaining;
    }

    // ── Event handlers ───────────────────────────────────────────────────────

    private void OnTypeSelected(IUnitType type)
    {
        _workingUnit.UnitInfo.UnitType = type;
        if (CostExceedsRP()) return;   // silently block; UX notification deferred
        GoToStep(Step.UnitClass);
    }

    private void OnClassSelected(IUnitClass cls)
    {
        _workingUnit.UnitInfo.UnitClass = cls;
        if (CostExceedsRP()) return;
        GoToStep(Step.UnitGroupType);
    }

    private void OnGroupTypeSelected(IUnitGroupType groupType)
    {
        _workingUnit.UnitInfo.UnitGroupType = groupType;
        if (CostExceedsRP()) return;
        GoToStep(Step.Name);
    }

    private void OnNameConfirmed(string name)
    {
        _workingUnit.Name        = name;
        _workingUnit.Description = name;
        SaveWorkingUnit();
        StartNewUnit();
        GoToStep(Step.UnitType);
    }

    private string ValidateName(string name)
    {
        var result = TheGame().JTSServices.RulesService.NameIsValid<Unit>(name);
        return result.Status == API.ResultStatus.SUCCESS ? null : result.Message;
    }

    private void SaveWorkingUnit()
    {
        var game = TheGame();
        var unit = _workingUnit;

        unit.UnitInfo.UnitType.BaseType.SupportedUnitGeographyTypes =
            game.JTSServices.DataService
                .LookupUnitGeogTypesByBaseTypes(new[] { unit.UnitInfo.UnitType.BaseType.ID });

        unit.UnitInfo.UnitType.HasGlobalMovementOverride =
            game.JTSServices.RulesService.UnitHasGlobalMovementOverride(unit).Result;

        unit.CurrentFuelRange = unit.UnitInfo.UnitType.FuelRange;
        unit.SetNextID();

        game.JTSServices.UnitService.SaveUnits(new List<IUnit> { unit });
        game.CurrentTurn.Player.AddReinforcementUnit(unit);
    }
}
