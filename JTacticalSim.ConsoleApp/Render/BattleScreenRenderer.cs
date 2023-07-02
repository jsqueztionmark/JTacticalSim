using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using JTacticalSim.API;
using JTacticalSim.API.AI;
using JTacticalSim.API.Component;
using JTacticalSim.API.Component.Util;
using JTacticalSim.API.Game;
using JTacticalSim.Component.World;
using JTacticalSim.Utility;
using ConsoleControls;

namespace JTacticalSim.ConsoleApp
{
	public sealed class BattleScreenRenderer : BaseScreenRenderer, IScreenRenderer
	{

#region Controls

		private ConsoleBox RoundDisplay;
		private ConsoleBox SkirmishDisplay;

#endregion

		private int _roundCount { get; set; }

		public BattleScreenRenderer(ConsoleRenderer baseRenderer)
		{
			_baseRenderer = baseRenderer;
		}


		protected override void InitializeControls()
		{

			// Screen box
			MainBorder = new Screen
				{
					Height = (Console.WindowHeight/2) + 10,
					Width = (Console.WindowWidth/2) - 11,					
					BorderForeColor = ConsoleColor.White,
					BorderBackColor = Global.Colors.ScreenBGColor,
					BackColor = ConsoleColor.White,
					ForeColor = Global.Colors.ScreenForeColor,
					DrawElements = new SingleLineBoxElements(),
					CloseWindowX = false,
					HasFocus = false,
					Caption = "Battle"
				};

			MainBorder.CenterPositionHorizontal(23);
			MainBorder.CenterPositionVertical();
			MainBorder.WindowClosePressed += On_CtlXPressed;

			RoundDisplay = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE)
				{
					Height = 3,
					Width = MainBorder.Width - 2,
					LeftOrigin = (MainBorder.LeftOrigin + 1),
					TopOrigin = (MainBorder.TopOrigin + 1),
					BorderForeColor = ConsoleColor.DarkGray,
					BorderBackColor = ConsoleColor.Black,
					BackColor = ConsoleColor.Black,
					ForeColor = ConsoleColor.White,
					DrawElements = new SingleLineBoxElements(),
					Border = true,
					CloseWindowX = false,
					HasFocus = false,
				};

			SkirmishDisplay = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.NONE)
				{
					Height = (MainBorder.Height - RoundDisplay.Height) - 4,
					Width = MainBorder.Width - 2,
					LeftOrigin = (MainBorder.LeftOrigin + 1),
					TopOrigin = RoundDisplay.TopOrigin + 5,
					BorderForeColor = ConsoleColor.DarkGray,
					BorderBackColor = ConsoleColor.Black,
					BackColor = ConsoleColor.Black,
					ForeColor = ConsoleColor.White,
					Border = true,
					CloseWindowX = false,
					DrawElements = new SingleLineBoxElements(),
					HasFocus = false
				};
		}

		public override void RenderScreen()
		{
			_roundCount = 0;
			base.RenderScreen();
		}

		public override void RefreshScreen()
		{
			MainBorder.Fill();
			MainBorder.Draw();
			RoundDisplay.Fill();
			RoundDisplay.Draw();
			SkirmishDisplay.Fill();
			SkirmishDisplay.Draw();
		}


#region Renderer Pass-through

		public void RenderBattle(IBattle battle)
		{
			MainBorder.BorderForeColor = ConsoleColor.Red;
			MainBorder.CaptionColor = ConsoleColor.Red;
			RoundDisplay.BorderBackColor = ConsoleColor.Red;
			SkirmishDisplay.BorderBackColor = ConsoleColor.Red;
			MainBorder.Caption = "Combat Engaged!!!";
			RefreshScreen();
		}

		public void RenderBattleRound(IRound round)
		{
			if (!round.Skirmishes.Any()) return;			
			DisplayRoundSkirmishCountInfo(TheGame().CurrentBattle.CurrentRoundCount, round.CurrentSkirmishCount, round.CurrentSkirmish.Type);

			_baseRenderer.RenderTileUnitInfoArea();
			RefreshScreen();
		}

		public void RenderBattleSkirmish(ISkirmish skirmish)
		{
			if (skirmish.Type == SkirmishType.FULL)
			{
				DisplaySkirmishInfo(skirmish.Attacker, skirmish.Defender, "attacks");
			}
			else
			{
				DisplaySkirmishInfo(skirmish.Defender, skirmish.Attacker, "defends against");
			}
		}

		private void DisplaySkirmishInfo(IUnit unitA, IUnit unitB, string actionText)
		{
			Console.BackgroundColor = SkirmishDisplay.BackColor;
			Console.ForegroundColor = SkirmishDisplay.ForeColor;

			var lineNum = 3;
			Console.SetCursorPosition(SkirmishDisplay.LeftOrigin + 5, SkirmishDisplay.TopOrigin + lineNum++);

			// Full skirmish
			Console.ForegroundColor = unitA.Country.TextDisplayColor;
			Console.Write("{0} {1}", unitA.UnitInfo.UnitType.TextDisplayZ4, unitA.Name);
			Console.ForegroundColor = SkirmishDisplay.ForeColor;
			Console.Write("  {0}  ".F(actionText));
			Console.ForegroundColor = unitB.Country.TextDisplayColor;
			Console.Write("{0} {1}", unitB.UnitInfo.UnitType.TextDisplayZ4, unitB.Name);
			Console.ForegroundColor = SkirmishDisplay.ForeColor;
		}

		public void RenderSkirmishOutcome(ISkirmish skirmish)
		{
			var skirmishOutcomeBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.PRESS_ANY_KEY)
				{
					Height = 1,
					Width = 60,
					LeftOrigin = SkirmishDisplay.LeftOrigin + (SkirmishDisplay.Width / 2) - 31,
					TopOrigin = SkirmishDisplay.TopOrigin + (SkirmishDisplay.Height / 2) - 1,
					BorderForeColor = ConsoleColor.White,
					BorderBackColor = ConsoleColor.DarkCyan,
					BackColor = ConsoleColor.DarkCyan,
					ForeColor = ConsoleColor.White,
					PromptColor = ConsoleColor.Yellow,
					CloseWindowX = false,
					DrawElements = new SingleLineBoxElements(),
					DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
					DropShadow = true,
					Text = "{0}".F(skirmish.GetSkirmishResults().Message)
				};

			skirmishOutcomeBox.ClearAndRedraw();
			RefreshScreen();
		}

		public void RenderBattleOutcome(IBattle battle)
		{
			var battleOutcomeBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.PRESS_ANY_KEY)
				{
					Height = 1,
					Width = 60,
					LeftOrigin = SkirmishDisplay.LeftOrigin + (SkirmishDisplay.Width / 2) - 30,
					TopOrigin = SkirmishDisplay.TopOrigin + (SkirmishDisplay.Height / 2) - 3,
					BorderForeColor = ConsoleColor.White,
					BorderBackColor = ConsoleColor.DarkCyan,
					BackColor = ConsoleColor.DarkCyan,
					ForeColor = ConsoleColor.White,
					PromptColor = ConsoleColor.Yellow,
					CloseWindowX = false,
					DrawElements = new SingleLineBoxElements(),
					DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
					DropShadow = true,
					Caption = "Battle Outcome",
					Text = "{0}!!!".F(TheGame().CurrentBattle.VictoryCondition.ToString())
				};

			battleOutcomeBox.ClearAndRedraw();
		}

		public void RenderBattleRetreat(IBattle battle)
		{
			var retreat = false;

			var RetreatBox = new ConsoleBox(BoxDisplayType.DISPLAY, PromptType.YES_NO)
				{
					Height = 2,
					Width = 60,
					LeftOrigin = SkirmishDisplay.LeftOrigin + (SkirmishDisplay.Width / 2) - 30,
					TopOrigin = SkirmishDisplay.TopOrigin + (SkirmishDisplay.Height / 2) - 3,
					BorderForeColor = ConsoleColor.DarkGray,
					BorderBackColor = ConsoleColor.Red,
					BackColor = ConsoleColor.Red,
					ForeColor = ConsoleColor.White,
					PromptColor = ConsoleColor.White,
					CloseWindowX = false,
					DrawElements = new SingleLineBoxElements(),
					DropShadowColor = Global.Colors.BASE_DROPSHADOW_COLOR,
					DropShadow = true,
					Text = "Call Retreat?"
				};


			// Handle retreat
			// No retreat for forced engagements
			if (battle.VictoryCondition == BattleVictoryCondition.NO_VICTOR && battle.BattleType != BattleType.FORCED_ENGAGEMENT)
			{
				RetreatBox.HasFocus = true;
				RetreatBox.ClearAndRedraw();
				retreat = Convert.ToBoolean(RetreatBox.Prompt);

				if (retreat)
					battle.VictoryCondition = BattleVictoryCondition.RETREAT;
			}

			RefreshScreen();
		}

#endregion

		private void DisplayRoundSkirmishCountInfo(int round, int skirmish, SkirmishType type)
		{
			Thread.Sleep(500);
			RoundDisplay.Text = "   Round {0}  /  Skirmish {1}     Combat Type : {2}".F(round, skirmish, type.ToString());
			RoundDisplay.ClearAndRedraw();
			Thread.Sleep(500);
		}


#region Event Handlers

#endregion

	}
}
