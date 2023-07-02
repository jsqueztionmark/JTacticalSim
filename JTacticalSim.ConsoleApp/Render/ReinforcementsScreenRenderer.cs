using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.Component;
using JTacticalSim.Utility;
using ConsoleControls;

namespace JTacticalSim.ConsoleApp
{
	public sealed class ReinforcementsScreenRenderer : BaseScreenRenderer, IScreenRenderer
	{

#region Controls

		public ConsoleBox BuildOUnitBox;

		public SelectListBox<IUnitType> UnitTypeBox;
		public SelectListBox<IUnitClass> UnitClassBox;
		public SelectListBox<IUnitGroupType> UnitGroupTypeBox;
		public TextBox UnitNameBox;

#endregion

		public UnitBuilder UnitBuilder { get; private set; } 

		public ReinforcementsScreenRenderer(ConsoleRenderer baseRenderer)
		{
			_baseRenderer = baseRenderer;
		}

		protected override void InitializeControls()
		{
			// Screen border
			MainBorder = new Screen
				{
					Height = (Console.WindowHeight/2) + 10,
					Width = (Console.WindowWidth/2) - 11,
					BorderForeColor = Global.Colors.ScreenBorderForeColor,
					BorderBackColor = Global.Colors.ScreenBorderBGColor,
					BackColor = Global.Colors.ScreenBGColor,
					ForeColor = Global.Colors.ScreenForeColor,
					Caption = "Unit Reinforcements",
				};

			MainBorder.CenterPositionHorizontal(23);
			MainBorder.CenterPositionVertical();
			MainBorder.WindowClosePressed += On_CtlXPressed;

			BuildOUnitBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE)
				{
					Height = 3,
					Width = 82,
					LeftOrigin = MainBorder.LeftOrigin + 3,
					TopOrigin = MainBorder.TopOrigin + 3,
					BorderForeColor = Global.Colors.SelectContainerBorderForeColor,
					BorderBackColor = Global.Colors.SelectContainerBorderBGColor,
					BackColor = ConsoleColor.Black,
					ForeColor = ConsoleColor.White,
					DrawElements = new SingleLineBoxElements(),
					Caption = "Build O' Unit"
				};

			UnitTypeBox = new SelectListContainer<IUnitType>(this)
				{
					Height = MainBorder.Height - 40,
					Width = 40,
					LeftOrigin = MainBorder.LeftOrigin + 3,
					TopOrigin = BuildOUnitBox.TopOrigin + BuildOUnitBox.Height + 6,
					BorderForeColor = Global.Colors.SelectContainerBorderForeColor,
					BorderBackColor = Global.Colors.SelectContainerBorderBGColor,
					BackColor = Global.Colors.SelectContainerBGColor,
					ForeColor = Global.Colors.SelectContainerForeColor,
					PromptColor = ConsoleColor.Yellow,
					DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
					DrawElements = new SingleLineBoxElements(), 
					PageSize = 15,
					Caption = "Select A Unit Type"
				};

			FillUnitTypeSelect();
			UnitTypeBox.ItemSelected += this.On_UnitTypeBoxItemSelected;
			UnitTypeBox.SelectionChanged += this.On_UnitTypeBoxSelectionChanged;
			UnitTypeBox.EscapePressed += this.On_UnitTypeBoxEscapePressed;
			UnitTypeBox.WindowClosePressed += this.On_CtlXPressed;

			UnitClassBox = new SelectListContainer<IUnitClass>(this)
				{
					Height = MainBorder.Height - 40,
					Width = 40,
					LeftOrigin = (UnitTypeBox.LeftOrigin + UnitTypeBox.Width) - 20,
					TopOrigin = UnitTypeBox.TopOrigin + 3,
					BorderForeColor = Global.Colors.SelectContainerBorderForeColor,
					BorderBackColor = Global.Colors.SelectContainerBorderBGColor,
					BackColor = Global.Colors.SelectContainerBGColor,
					ForeColor = Global.Colors.SelectContainerForeColor,
					PromptColor = ConsoleColor.Yellow,
					DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
					DrawElements = new SingleLineBoxElements(),
					DropShadow = true,
					PageSize = 15,
					Caption = "Select A Unit Class"
				};

			UnitClassBox.ItemSelected += this.On_UnitClassBoxItemSelected;
			UnitClassBox.SelectionChanged += this.On_UnitClassBoxSelectionChanged;
			UnitClassBox.EscapePressed += this.On_UnitClassBoxEscapePressed;
			UnitClassBox.WindowClosePressed += this.On_CtlXPressed;

			UnitGroupTypeBox = new SelectListContainer<IUnitGroupType>(this)
				{
					Height = 8,
					Width = 40,
					LeftOrigin = (UnitClassBox.LeftOrigin + UnitClassBox.Width) - 20,
					TopOrigin = UnitClassBox.TopOrigin + 5,
					BorderForeColor = Global.Colors.SelectContainerBorderForeColor,
					BorderBackColor = Global.Colors.SelectContainerBorderBGColor,
					BackColor = Global.Colors.SelectContainerBGColor,
					ForeColor = Global.Colors.SelectContainerForeColor,
					PromptColor = ConsoleColor.Yellow,
					DrawElements = new SingleLineBoxElements(),
					DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
					DropShadow = true,
					PageSize = 5,
					Caption = "Select A Unit Group Type"
				};

			FillUnitGroupTypeSelect();
			UnitGroupTypeBox.ItemSelected += this.On_UnitGroupTypeBoxItemSelected;
			UnitGroupTypeBox.SelectionChanged += this.On_UnitGroupTypeBoxSelectionChanged;
			UnitGroupTypeBox.EscapePressed += this.On_UnitGroupTypeBoxEscapePressed;
			UnitGroupTypeBox.WindowClosePressed += this.On_CtlXPressed;

			UnitNameBox = new TextBox()
				{
					Height = 6,
					Width = 82,
					LeftOrigin = MainBorder.LeftOrigin + 3,
					TopOrigin = (MainBorder.TopOrigin + MainBorder.Height) - 8,
					BorderForeColor = Global.Colors.SelectContainerBorderForeColor,
					BorderBackColor = Global.Colors.SelectContainerBorderBGColor,
					BackColor = Global.Colors.SelectContainerForeColor,
					ForeColor = Global.Colors.ScreenBorderForeColor,
					PromptColor = Global.Colors.ScreenBorderForeColor,
					DrawElements = new SingleLineBoxElements(),
					MessageColor = ConsoleColor.Red,
					DropShadow = false,
					MaxCharacters = 30,
					Caption = "Unit Name"
				};

			UnitNameBox.EscapePressed += On_UnitNameBoxEscapePressed;
			UnitNameBox.TextEntered += On_UnitNameEntered;
			UnitNameBox.WindowClosePressed += this.On_CtlXPressed;

			// Push onto the stack
			Controls.Push(UnitTypeBox);
		}

		public override void RenderScreen()
		{
			UnitBuilder = new UnitBuilder();
			UnitBuilder.UnitSaved += On_ReinforcementSaved;
			UnitBuilder.CreateNewUnit();
			base.RenderScreen();
		}

		protected override void DrawOverlay()
		{
			UpdateAndDrawBuildOUnitBox();	
		}


		private void FillUnitTypeSelect()
		{
			UnitTypeBox.ClearItems();
			var unitTypes = TheGame().JTSServices.DataService.GetAllowedUnitTypes(TheGame().CurrentTurn.Player.Country)
											.OrderBy(ut => ut.Name)
											.ToList();

			unitTypes.ForEach(ut => UnitTypeBox.AddItem(new ListBoxItem<IUnitType>(ut, ut.Name)));
		}

		private void FillUnitClassSelect(IUnitType unitType)
		{
			UnitClassBox.ClearItems();
			var unitClass = TheGame().JTSServices.UnitService.GetAllowableUnitClassesForUnitType(unitType).Result
											.OrderBy(uc => uc.Name)
											.ToList();

			unitClass.ForEach(uc => UnitClassBox.AddItem(new ListBoxItem<IUnitClass>(uc, uc.Name)));
		}

		private void FillUnitGroupTypeSelect()
		{
			UnitGroupTypeBox.ClearItems();
			var unitGroupTypes = TheGame().JTSServices.DataService.GetAllowedUnitGroupTypes()
											.OrderBy(ut => ut.Name)
											.ToList();

			unitGroupTypes.ForEach(ugt => UnitGroupTypeBox.AddItem(new ListBoxItem<IUnitGroupType>(ugt, ugt.Name)));
		}

		private void UpdateAndDrawBuildOUnitBox()
		{
			BuildOUnitBox.ClearAndRedraw();

			if (UnitBuilder.WorkingUnit.UnitInfo.UnitGroupType != null)
			{
				Console.SetCursorPosition(BuildOUnitBox.LeftOrigin + 2, BuildOUnitBox.TopOrigin + 2);
				DrawBuildUnitValue("Unit Group Type:", UnitBuilder.WorkingUnit.UnitInfo.UnitGroupType.TextDisplayZ4);
			}
			if (UnitBuilder.WorkingUnit.UnitInfo.UnitClass != null)
			{
				Console.SetCursorPosition(BuildOUnitBox.LeftOrigin + 25, BuildOUnitBox.TopOrigin + 2);
				DrawBuildUnitValue("Unit Class:", UnitBuilder.WorkingUnit.UnitInfo.UnitClass.TextDisplayZ4);
			}
			if (UnitBuilder.WorkingUnit.UnitInfo.UnitType != null)
			{
				Console.SetCursorPosition(BuildOUnitBox.LeftOrigin + 42, BuildOUnitBox.TopOrigin + 2);
				DrawBuildUnitValue("Unit Type:", UnitBuilder.WorkingUnit.UnitInfo.UnitType.TextDisplayZ4);
			}

			// Show reinforcement points cost
			Console.SetCursorPosition(BuildOUnitBox.LeftOrigin + 65, BuildOUnitBox.TopOrigin + 2);
			DrawPointCost();
		}

		private void DrawBuildUnitValue(string caption, string value)
		{
			Console.BackgroundColor = BuildOUnitBox.BackColor;
			Console.ForegroundColor = BuildOUnitBox.ForeColor;
			Console.Write(caption);
			Console.Write(" ");
			Console.ForegroundColor = TheGame().CurrentTurn.Player.Country.TextDisplayColor;
			Console.Write(value);
		}

		private void DrawPointCost()
		{
			Console.BackgroundColor = BuildOUnitBox.BackColor;
			Console.ForegroundColor = BuildOUnitBox.ForeColor;
			Console.Write("Point Cost: ");
			Console.BackgroundColor = BuildOUnitBox.BackColor;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write(UnitBuilder.WorkingUnit.UnitCost);
			Console.ResetColor();
		}

#region Event Handlers

		private void On_ReinforcementSaved(object sender, EventArgs e)
		{			
		}

	// Unit Type Select

		private void On_UnitTypeBoxItemSelected(object sender, EventArgs e)
		{
			FillUnitClassSelect(UnitBuilder.WorkingUnit.UnitInfo.UnitType);
			UnitClassBox.ClearSelection();
			Controls.Push(UnitClassBox);
			
			RefreshScreen();
		}

		private void On_UnitTypeBoxSelectionChanged(object sender, EventArgs e)
		{
			UnitBuilder.WorkingUnit.UnitInfo.UnitType = UnitTypeBox.HighlightedItem.Value;
			if (UnitBuilder.WorkingUnit.UnitInfo.UnitType == null)
				return;

			UpdateAndDrawBuildOUnitBox();
		}

		private void On_UnitTypeBoxEscapePressed(object sender, EventArgs e)
		{
			RefreshScreen();
		}

	// Unit Class Select

		private void On_UnitClassBoxItemSelected(object sender, EventArgs e)
		{
			UnitGroupTypeBox.ClearSelection();
			Controls.Push(UnitGroupTypeBox);

			RefreshScreen();
		}

		private void On_UnitClassBoxSelectionChanged(object sender, EventArgs e)
		{
			UnitBuilder.WorkingUnit.UnitInfo.UnitClass = UnitClassBox.HighlightedItem.Value;
			if (UnitBuilder.WorkingUnit.UnitInfo.UnitClass == null)
				return;

			UpdateAndDrawBuildOUnitBox();
		}

		private void On_UnitClassBoxEscapePressed(object sender, EventArgs e)
		{
			UnitBuilder.WorkingUnit.UnitInfo.UnitClass = null;
			Controls.Pop();
			RefreshScreen();	
		}

	// Unit Group Type Select

		private void On_UnitGroupTypeBoxItemSelected(object sender, EventArgs e)
		{
			Controls.Push(UnitNameBox);
			RefreshScreen();
		}

		private void On_UnitGroupTypeBoxSelectionChanged(object sender, EventArgs e)
		{
			UnitBuilder.WorkingUnit.UnitInfo.UnitGroupType = UnitGroupTypeBox.HighlightedItem.Value;
			if (UnitBuilder.WorkingUnit.UnitInfo.UnitGroupType == null)
				return;

			UpdateAndDrawBuildOUnitBox();
		}

		private void On_UnitGroupTypeBoxEscapePressed(object sender, EventArgs e)
		{
			UnitBuilder.WorkingUnit.UnitInfo.UnitGroupType = null;
			Controls.Pop();
			RefreshScreen();
		}

	// Unit Name Box

		private void On_UnitNameBoxEscapePressed(object sender, EventArgs e)
		{
			Controls.Pop();
			RefreshScreen();
		}

		private void On_UnitNameEntered(object sender, EventArgs e)
		{
			if (UnitNameBox.Text == null)
				return;

			// The end of the process - where the business happens
			// Validate the name
			var unitName = UnitNameBox.Text;
			var validateResult = InputValidation.IsValidUnitName(unitName, TheGame());

			if (!validateResult.Result)
			{
				UnitNameBox.Message = validateResult.Message;
				RefreshScreen();
				return;
			}

			UnitBuilder.WorkingUnit.Name = unitName;
			var saveResult = UnitBuilder.SaveUnit();
			//TODO: Handle errors

			var r = TheGame().CurrentTurn.Player.AddReinforcementUnit(UnitBuilder.WorkingUnit);
			//TODO: Handle errors

			TheGame().Renderer.RenderBoardFrame();
			RenderScreen();
		}

#endregion

	}

	public class UnitBuilder : BaseGameObject
	{
		public event EventHandler UnitSaved;

		private IUnit _workingUnit { get; set; }
		public IUnit WorkingUnit { get { return _workingUnit; }}
		public int RunningTotalVP
		{
			get { return 0; }
		}

		public UnitBuilder()
			: base(GameObjectType.HANDLER)
		{
		}

		/// <summary>
		/// Replaces the current working unit with a new unit
		/// </summary>
		public void CreateNewUnit()
		{
			var ui = new UnitInfo(null, null, null);

			IUnit unit = TheGame().JTSServices.UnitService.CreateUnit(string.Empty, null, 
																	TheGame().CurrentTurn.Player.Country, 
																	ui).SuccessfulObjects.First();
			unit.UnitInfo = ui;
			unit.Description = unit.Name;

			_workingUnit = unit;
		}

		/// <summary>
		/// Saves the current working unit to context
		/// </summary>
		/// <returns></returns>
		public IResult<IUnit, IUnit> SaveUnit()
		{
			// Get geog types
			_workingUnit.UnitInfo.UnitType.BaseType.SupportedUnitGeographyTypes = 
					TheGame().JTSServices.DataService.LookupUnitGeogTypesByBaseTypes(new [] {_workingUnit.UnitInfo.UnitType.BaseType.ID});

			// Determine global override value
			_workingUnit.UnitInfo.UnitType.HasGlobalMovementOverride =
					TheGame().JTSServices.RulesService.UnitHasGlobalMovementOverride(_workingUnit).Result;

			_workingUnit.CurrentFuelRange = _workingUnit.UnitInfo.UnitType.FuelRange;

			_workingUnit.SetNextID();
			var sResult = TheGame().JTSServices.UnitService.SaveUnits(new List<IUnit> {_workingUnit});
			return sResult;
		}

		private void On_UnitSaved()
		{
			if (UnitSaved != null) UnitSaved(this, new EventArgs());
		}

	}
}
