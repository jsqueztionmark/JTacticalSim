using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Text;
using System.Configuration;
using System.Transactions;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Component.Util;
using JTacticalSim.Component;
using JTacticalSim.Component.Data;
using JTacticalSim.Media.Sound;
using JTacticalSim.Service;
using JTacticalSim.Utility;
using ConsoleControls;

namespace JTacticalSim.ConsoleApp
{
	public sealed class TitleScreenRenderer : BaseScreenRenderer, IScreenRenderer
	{

#region Controls

		public ConsoleBox GameInfoBox;
		public PagedConsoleBox ScenarioInfoBox;
		public SelectListBox<ISavedGame> SavedGamesSelect;
		public SelectListBox<IScenario> ScenarioSelect;
		public TextBox NewGameTitle;

#endregion

		public TitleScreenRenderer(ConsoleRenderer baseRenderer)
		{
			TheGame().GameLoading += On_GameLoading;
			TheGame().MapLoading += On_MapLoading;
			_baseRenderer = baseRenderer;
		}

		public override void DisplayUserMessage(BoxDisplayType messageType, string message, Exception ex)
		{
			var display = new StringBuilder("{0}".F(message));

			if (ex != null)
				display.AppendLine(ex.Message);

			StatusDisplay.Display(display.ToString(), messageType);
			RenderScreen();
		}

		protected  override void InitializeControls()
		{
			// Screen board border
			MainBorder = new Screen
				{
					Height = (Console.WindowHeight / 2) + 20,
					Width = (Console.WindowWidth / 2),
					BorderForeColor = Global.Colors.ScreenBorderForeColor,
					BorderBackColor = Global.Colors.ScreenBorderBGColor,
					BackColor = Global.Colors.ScreenBGColor,
					ForeColor = Global.Colors.ScreenForeColor,
					Caption = "JTacticalSim Battle Simulator"
				};

			MainBorder.CenterPositionHorizontal();
			MainBorder.CenterPositionVertical();
			MainBorder.WindowClosePressed += On_CtlXPressed;

			GameInfoBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE)
				{
					Height = 7,
					Width = MainBorder.Width - 10,
					TopOrigin = MainBorder.TopOrigin + 3,
					LeftOrigin = MainBorder.LeftOrigin + 5,
					BorderForeColor = Global.Colors.SelectContainerBorderForeColor,
					BorderBackColor = Global.Colors.SelectContainerBorderBGColor,
					BackColor = Global.Colors.SelectContainerBGColor,
					ForeColor = Global.Colors.SelectContainerForeColor,
					DrawElements = new SingleLineBoxElements(),
					Text = SetGameInfoContent()
				};

			Controls.Push(GameInfoBox);

			SavedGamesSelect = new SelectListContainer<ISavedGame>(this)
				{
					Height = 10,
					Width = MainBorder.Width - 10,
					TopOrigin = (GameInfoBox.TopOrigin + GameInfoBox.Height) + 4,
					BorderForeColor = Global.Colors.SelectContainerBorderForeColor,
					BorderBackColor = Global.Colors.SelectContainerBorderBGColor,
					BackColor = Global.Colors.SelectContainerBGColor,
					ForeColor = Global.Colors.SelectContainerForeColor,
					PromptColor = ConsoleColor.Yellow,
					DrawElements = new SingleLineBoxElements(), 
					PageSize = 15,
					Caption = "Choose a Game",
					PromptText = "Scroll ▲/▼  Select:Enter"
				};

			FillSavedGamesSelect();
			SavedGamesSelect.CenterPositionHorizontal();
			SavedGamesSelect.ItemSelected += this.On_SavedGameItemSelected;
			//SavedGamesSelect.SelectionChanged += this.On_UnitTypeBoxSelectionChanged;
			SavedGamesSelect.EscapePressed += this.On_SavedGameEscapePressed;
			SavedGamesSelect.WindowClosePressed += this.On_CtlXPressed;

			Controls.Push(SavedGamesSelect);

			// Focus by default on load
			SavedGamesSelect.HasFocus = true;

			ScenarioSelect = new SelectListContainer<IScenario>(this)
				{
					Height = SavedGamesSelect.Height,
					Width = SavedGamesSelect.Width,
					TopOrigin = SavedGamesSelect.TopOrigin,
					BorderForeColor = Global.Colors.SelectContainerBorderForeColor,
					BorderBackColor = Global.Colors.SelectContainerBorderBGColor,
					BackColor = Global.Colors.SelectContainerBGColor,
					ForeColor = Global.Colors.SelectContainerForeColor,
					PromptColor = ConsoleColor.Yellow,
					DrawElements = new SingleLineBoxElements(), 
					PageSize = 15,
					Caption = "Choose a Scenario"
				};

			FillScenarioSelect();
			ScenarioSelect.CenterPositionHorizontal();
			ScenarioSelect.ItemSelected += this.On_ScenarioItemSelected;
			ScenarioSelect.SelectionChanged += On_ScenarioItemChanged;
			ScenarioSelect.EscapePressed += this.On_ScenarioEscapePressed;
			ScenarioSelect.WindowClosePressed += this.On_CtlXPressed;

			NewGameTitle = new TextBox
				{
					Height = 6,
					Width = ScenarioSelect.Width,
					TopOrigin = 47,
					BorderForeColor = Global.Colors.SelectContainerBorderForeColor,
					BorderBackColor = Global.Colors.SelectContainerBorderBGColor,
					BackColor = Global.Colors.SelectContainerForeColor,
					ForeColor = Global.Colors.ScreenBorderForeColor,
					PromptColor = Global.Colors.ScreenBorderForeColor,
					DrawElements = new SingleLineBoxElements(),
					MessageColor = ConsoleColor.Red,
					DropShadow = false,
					MaxCharacters = 30,
					Caption = "New Game Name"
				};

			NewGameTitle.CenterPositionHorizontal();

			NewGameTitle.EscapePressed += On_NewGameTitleEscapePressed;
			NewGameTitle.TextEntered += On_NewGameTitleEntered;
			NewGameTitle.WindowClosePressed += this.On_CtlXPressed;
		}

		private void FillSavedGamesSelect()
		{
			var newGameSelect = new SavedGame();
			newGameSelect.Name = "Create New Game";
			SavedGamesSelect.AddItem(new ListBoxItem<ISavedGame>(newGameSelect, newGameSelect.Name));
			SavedGamesSelect.AddItem(new ListBoxItem<ISavedGame>(null, string.Empty));			

			var savedGames = TheGame().JTSServices.GameService.GetSavedGames()
											//.Where(sg => sg.Scenario.ID != 0)
											.OrderBy(sg => sg.Name).ToList();
			savedGames.ForEach(sg => SavedGamesSelect.AddItem(new ListBoxItem<ISavedGame>(sg, sg.Name)));
			
		}

		private void FillScenarioSelect()
		{
			var scenarios = TheGame().JTSServices.GameService.GetScenarios().OrderBy(s => s.Name).ToList();
			scenarios.ForEach(s => ScenarioSelect.AddItem(new ListBoxItem<IScenario>(s, s.Name)));
		}

		private string SetGameInfoContent()
		{
			var sb = new StringBuilder();
			sb.AppendLine("JTacticalSim v.{0}  {1}".F(ConfigurationManager.AppSettings["gameversion"], "(c)2013-2014"));
			sb.AppendLine("");
			sb.AppendLine("Design and Development - Jeff Storm");
			sb.AppendLine("jsqueztionmark@gmail.com");
			return sb.ToString();
		}

		private void CreateNewGame(IScenario scenario, string newGameName)
		{
			var newGame = new SavedGame
				{
					Name = newGameName,
					GameFileDirectory = newGameName,
					LastPlayed = false,
					Scenario = scenario
				};

			newGame.SetNextID();

			using (var txn = new TransactionScope())
			{
				// Save the saved game record.
				var sResult = TheGame().JTSServices.GameService.SaveSavedGame(newGame);

				// Save the saved game data.
				var cResult = TheGame().SaveAs(scenario, newGame);
				txn.Complete();
			}

			TheGame().StateSystem.ChangeState(StateType.TITLE_MENU);
		}

		private void CreateNewScenarioInfoBox(string text)
		{
			ScenarioInfoBox = new PagedConsoleBox()
				{
					Height = 20,
					Width = ScenarioSelect.Width,
					TopOrigin = NewGameTitle.TopOrigin,
					BorderForeColor = Global.Colors.SelectContainerBorderForeColor,
					BorderBackColor = Global.Colors.SelectContainerBorderBGColor,
					BackColor = Global.Colors.SelectContainerBGColor,
					ForeColor = Global.Colors.ScenarioInfoTextColor,
					DrawElements = new SingleLineBoxElements(), 
					Caption = "Scenario Info",
					HasFocus = false,
					Text = text
				};

			ScenarioInfoBox.CenterPositionHorizontal();
			ScenarioInfoBox.ClearAndRedraw();
		}


#region Event Handlers

		private void On_GameLoading(object sender, EventArgs e)
		{
			Console.Clear();
			Console.WriteLine("Loading Game ...");
			Thread.Sleep(500);
		}

		private void On_MapLoading(object sender, EventArgs e)
		{
			Console.WriteLine("Creating Map ...");
		}

		private void On_SavedGameEscapePressed(object sender, EventArgs e)
		{
			RefreshScreen();
		}

		private void On_SavedGameItemSelected(object sender, EventArgs e)
		{
			// Handle new game select
			if (SavedGamesSelect.SelectedItem.Value.Name.ToLowerInvariant() == "Create New Game".ToLowerInvariant())
			{
				Controls.Push(ScenarioSelect);
				RefreshScreen();
			}
			else
			{
				LoadNewGame(SavedGamesSelect.SelectedItem.Value.Name);
			}
		}

		private void On_ScenarioEscapePressed(object sender, EventArgs e)
		{
			Controls.Pop();
			RefreshScreen();
		}

		private void On_ScenarioItemSelected(object sender, EventArgs e)
		{
			Controls.Push(NewGameTitle);
			RefreshScreen();	
		}

		private void On_ScenarioItemChanged(object sender, EventArgs e)
		{
			var scenario = ScenarioSelect.HighlightedItem.Value;
			if (scenario == null) return;

			CreateNewScenarioInfoBox(scenario.TextInfo());				
		}

		private void On_NewGameTitleEscapePressed(object sender, EventArgs e)
		{
			Controls.Pop();
			RefreshScreen();
		}

		private void On_NewGameTitleEntered(object sender, EventArgs e)
		{
			var scenario = ScenarioSelect.SelectedItem.Value;
			var gameTitle = NewGameTitle.Text;

			if (string.IsNullOrWhiteSpace(gameTitle))
				return;

			var gResult = InputValidation.IsValidGameTitle(gameTitle, TheGame());

			if (!gResult.Result)
			{
				NewGameTitle.Message = gResult.Message;
				NewGameTitle.ClearAndRedraw();
				return;
			}

			CreateNewGame(scenario, gameTitle);
		}


#endregion

		
	}
}
