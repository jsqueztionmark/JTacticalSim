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
using JTacticalSim.API.Component.Util;
using JTacticalSim.Component;
using JTacticalSim.Utility;
using ConsoleControls;

namespace JTacticalSim.ConsoleApp
{
	public sealed class QuickSelectRenderer : BaseScreenRenderer, IScreenRenderer
	{

#region Controls

		public SelectListBox<IUnit> UnitBox;
		public ConsoleBox UnitInfoBox;
		public ConsoleBox NodeInfoBox;

#endregion

		public QuickSelectRenderer(ConsoleRenderer baseRenderer)
		{
			_baseRenderer = baseRenderer;
		}

		protected override void InitializeControls()
		{
			// Screen border
			MainBorder = new Screen
				{
					Height = Console.WindowHeight - 25,
					Width = (Console.WindowWidth / 2),
					BorderForeColor = Global.Colors.ScreenBorderForeColor,
					BorderBackColor = Global.Colors.ScreenBorderBGColor,
					BackColor = Global.Colors.ScreenBGColor,
					ForeColor = Global.Colors.ScreenForeColor,
					Caption = "Quick Unit Select",
					HasFocus = false
				};

			MainBorder.CenterPositionHorizontal(23);
			MainBorder.CenterPositionVertical();
			MainBorder.WindowClosePressed += On_CtlXPressed;

			UnitBox = new SelectListContainer<IUnit>(this)
				{
					Height = MainBorder.Height - 4,
					Width = 48,
					LeftOrigin = MainBorder.LeftOrigin + 3,
					TopOrigin = MainBorder.TopOrigin + 2,
					BorderForeColor = Global.Colors.SelectContainerBorderForeColor,
					BorderBackColor = Global.Colors.SelectContainerBorderBGColor,
					BackColor = Global.Colors.SelectContainerBGColor,
					ForeColor = Global.Colors.SelectContainerForeColor,
					PromptColor = ConsoleColor.Yellow,
					DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
					DrawElements = new SingleLineBoxElements(), 
					PageSize = 50,
					Caption = "Select A Unit",
					CaptionColor = Global.Colors.SelectContainerBorderForeColor
				};

			FillUnitSelect();
			UnitBox.ItemSelected += On_UnitBoxItemSelected;
			UnitBox.SelectionChanged += On_UnitBoxSelectionChanged;
			UnitBox.EscapePressed += On_UnitBoxEscapePressed;
			UnitBox.WindowClosePressed += On_CtlXPressed;

			Controls.Push(UnitBox);

			UnitInfoBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE)
				{
					Height = (UnitBox.Height / 2) - 2,
					Width = 42,
					LeftOrigin = UnitBox.LeftOrigin + UnitBox.Width + 4,
					TopOrigin = UnitBox.TopOrigin,
					BorderForeColor = Global.Colors.SelectContainerBorderForeColor,
					BorderBackColor = Global.Colors.SelectContainerBorderBGColor,
					BackColor = Global.Colors.SelectContainerBGColor,
					ForeColor = Global.Colors.SelectContainerForeColor,
					PromptColor = ConsoleColor.Yellow,
					DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
					DrawElements = new SingleLineBoxElements(), 
					Caption = "Unit Info",
					CaptionColor = Global.Colors.SelectContainerBorderForeColor
				};

			UnitInfoBox.HasFocus = false;

			NodeInfoBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE)
				{
					Height = (UnitBox.Height / 2) - 2,
					Width = 42,
					LeftOrigin = UnitBox.LeftOrigin + UnitBox.Width + 4,
					TopOrigin = UnitInfoBox.TopOrigin + UnitInfoBox.Height + 4,
					BorderForeColor = Global.Colors.SelectContainerBorderForeColor,
					BorderBackColor = Global.Colors.SelectContainerBorderBGColor,
					BackColor = Global.Colors.SelectContainerBGColor,
					ForeColor = Global.Colors.SelectContainerForeColor,
					PromptColor = ConsoleColor.Yellow,
					DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
					DrawElements = new SingleLineBoxElements(), 
					Caption = "Unit Location Info",
					CaptionColor = Global.Colors.SelectContainerBorderForeColor
				};

			NodeInfoBox.HasFocus = false;
		}

		protected override void DrawOverlay()
		{
			UnitInfoBox.ClearAndRedraw();
			NodeInfoBox.ClearAndRedraw();
			base.DrawOverlay();
		}

		private void FillUnitSelect()
		{
			var units = TheGame().JTSServices.UnitService
												.GetAllUnits(TheGame().CurrentTurn.Player.Country)
												.Where(u => !TheGame().CurrentTurn.Player.UnplacedReinforcements.Contains(u))
												.ToList();

			var allUnitsByBranch = new List<List<IUnit>>
			{
				units.Where(u => u.UnitInfo.UnitType.Branch.Name.ToLowerInvariant() == "army").ToList(),
				units.Where(u => u.UnitInfo.UnitType.Branch.Name.ToLowerInvariant() == "navy").ToList(),
				units.Where(u => u.UnitInfo.UnitType.Branch.Name.ToLowerInvariant() == "airforce").ToList(),
			};

			allUnitsByBranch.Where(ul => ul.Any()).ToList().ForEach(ul =>
				{
					ul.Sort((u1, u2) => u1.Name.CompareTo(u2.Name));
					var branchName = ul.First().UnitInfo.UnitType.Branch.Name;

					var spaceItem = new ListBoxItem<IUnit>(null, string.Empty);
					UnitBox.AddItem(spaceItem);
					
					var titleItem = new ListBoxItem<IUnit>(null, branchName);
					titleItem.ItemColor = ConsoleColor.Green;
					UnitBox.AddItem(titleItem);

					ul.ForEach(u =>
						{
							var item = new ListBoxItem<IUnit>(u, u.FullTabbedDisplayName());
				
							if (u.CurrentMoveStats.HasPerformedAction)
								item.ItemColor = ConsoleColor.Gray;

							if (TheGame().GameBoard.SelectedUnits.Any(unit => unit.Equals(u)))
								item.ItemColor = Global.Colors.UnitSelectedColor;

							UnitBox.AddItem(item);
						});
				});
		}

		private Command GetUnitAction(IUnit unit)
		{
			var unitActionBox = new UnitSelectMenuBox<Command>("Choose Action");
			//unitActionBox.Erased += On_MapMenuErased;
			//unitActionBox.EscapePressed += On_MenuClickAction;
			//unitActionBox.ItemSelected += On_MenuClickAction;

			foreach (var cmd in CommandInterface.GetAvailableCommandsForUnitQuickSelect(unit, TheGame()))
			{
				unitActionBox.AddItem(new ListBoxItem<Command>(cmd, cmd.DisplayName));
			}

			if (unitActionBox.ItemCount == 0) return null;
			unitActionBox.ClearAndRedraw();
			return (unitActionBox.SelectedItem != null) ? unitActionBox.SelectedItem.Value : null;
		}

		/// <summary>
		/// Creates a pop-up style box on the map adjacent to the currently selected node
		/// </summary>
		/// <typeparam name="T"></typeparam>
		private class UnitSelectMenuBox<T> : SelectListBox<T>
		{
			public UnitSelectMenuBox(string caption)
			{
				var leftOrigin = 50;
				var topOrigin = 50;

				Height = Global.Measurements.NODE_ACTION_SELECT_HEIGHT;
				Width = Global.Measurements.NODE_ACTION_SELECT_WIDTH;
				LeftOrigin = leftOrigin;
				TopOrigin = topOrigin;
				BorderForeColor = Global.Colors.MapMenuBorderForeColor;
				BorderBackColor = Global.Colors.MapMenuBorderBackColor;
				DrawElements = new SingleLineBoxElements();
				BackColor = Global.Colors.MapMenuBackColor;;
				DropShadow = true;
				DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR;
				PageSize = 15;
				Bullet = "∙";
				Caption = caption;
				CaptionColor = ConsoleColor.DarkRed;

			}
		}

#region Event Handlers
	
		private void On_UnitBoxItemSelected(object sender, EventArgs e)
		{
			var selectList = sender as SelectListContainer<IUnit>;
			var unit = selectList.SelectedItem.Value;
			TheGame().GameBoard.ClearSelectedItems(true);
			unit.Select();
			var r = TheGame().GameBoard.SelectedNode = unit.GetNode();
			var unitStack = r.DefaultTile.GetAllComponentStacks().SingleOrDefault(cs => cs.Country.Equals(unit.Country));
			unitStack.BringToTop(unit);
			TheGame().GameBoard.CenterSelectedNode();
			TheGame().Renderer.SetCurrentViewableArea();
			CloseScreen();
		}

		private void On_UnitBoxSelectionChanged(object sender, EventArgs e)
		{
			var unit = UnitBox.HighlightedItem.Value;

			if (unit != null)
			{
				var demNames = unit.GetNode().DefaultTile.GetAllDemographicNames();
				var sb = new StringBuilder();
				
				if (!string.IsNullOrWhiteSpace(unit.GetNode().DefaultTile.Name))
					sb.AppendLine(unit.GetNode().DefaultTile.Name);
				
				foreach (var name in demNames)
					sb.AppendLine(name);

				sb.AppendLine("");
				sb.Append(unit.GetNode().DefaultTile.TextInfo());

				UnitInfoBox.Text = unit.TextInfo();
				NodeInfoBox.Text = sb.ToString();
				DrawOverlay();
			}
		}

		private void On_UnitBoxEscapePressed(object sender, EventArgs e)
		{
			RefreshScreen();
		}

#endregion

	}
}
