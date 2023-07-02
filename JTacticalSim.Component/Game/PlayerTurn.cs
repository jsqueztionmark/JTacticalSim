using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;


namespace JTacticalSim.Component.Game
{
	public class PlayerTurn : GameComponentBase, IPlayerTurn
	{
		public event TurnEndedEvent TurnEnded;
		public event TurnStartedEvent TurnStarted;

		public IPlayer Player { get; private set; }
		public StringBuilder TaskExecutionReport { get; set; }

		public PlayerTurn(IPlayer player)
		{
			Player = player;
			TaskExecutionReport = new StringBuilder();
		}

		public void Start()
		{
			Start(false);	
		}

		public void Start(bool isGameLoad)
		{
			On_TurnStarted(new PlayerTurnStartEventArgs(isGameLoad));
		}

		public void End()
		{
			On_TurnEnded(new EventArgs());
		}


	// Event Handlers

		public void On_TurnEnded(EventArgs e)
		{
			var specialUnitTurnEndResult = TheGame().JTSServices.AIService.HandleSpecialTurnEndUnitManagement();
			HandleResultDisplay(specialUnitTurnEndResult, true);

			// Collect reinforcement points
			var pointsToAdd = Convert.ToInt32(TheGame().JTSServices.RulesService.CalculateReinforcementPointsForTurn(Player).Result);
			Player.TrackedValues.ReinforcementPoints += pointsToAdd;

			// Save Player
			TheGame().JTSServices.GameService.UpdatePlayers(new List<IPlayer> { Player });

			if (TurnEnded != null) TurnEnded(this, e);
		}

		public void On_TurnStarted(PlayerTurnStartEventArgs e)
		{		
			//TODO: Evaluate game and create strategies for AI players
			if (TurnStarted != null) TurnStarted(this, e);

			if (TaskExecutionReport.Length > 0)
			{
				TheGame().Renderer.DisplayTaskExecutionReport(TaskExecutionReport);
				TaskExecutionReport = null;
			}			
		}
	}
}
