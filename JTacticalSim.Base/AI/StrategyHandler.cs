using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Game;
using JTacticalSim.API.AI;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.Utility;

namespace JTacticalSim.AI
{
	public class StrategyHandler : BaseGameObject, IStrategyHandler
	{
		public StrategyHandler()
			: base(GameObjectType.HANDLER)
		{}

#region Strategies

		// Create Strategies
		// Update Strategies
		// Remove Strategies
		// Execute Strategies
		// Re-evaluate Strategies

		public IResult<ITactic, ITactic> SetUpTactic(ITactic tactic)
		{
			var result = new OperationResult<ITactic, ITactic>();

			try
			{
				var sResult = TheGame().JTSServices.AIService.SaveTactic(tactic);
				result.ConvertResultData(sResult);
			}
			catch (Exception ex)
			{
				result.ex = ex;
				result.Status = ResultStatus.EXCEPTION;
			}

			return result;
		}

		public IResult<IMission, IMission> SetUpUnitMission(IMission mission, IUnitTask task, StrategicalStance stance, IPlayer player)
		{
			var result = new OperationResult<IMission, IMission>();

			try
			{
				var tactic = TheGame().JTSServices.AIService.CreateNewTactic(new[] {mission}, player, stance).Result;

				mission.AddChildComponent(task);
				mission.SetCurrentTask();
				tactic.AddChildComponent(mission);

				var sResult = TheGame().JTSServices.AIService.SaveTactic(tactic);
				result.ConvertResultData(sResult);
			}
			catch (Exception ex)
			{
				result.ex = ex;
				result.Status = ResultStatus.EXCEPTION;
			}

			DisplayTaskAssignmentResults(result, task);
			return result;
		}

		public void ExecuteStrategiesForPlayer(IPlayer player)
		{
			var tactics = Cache().TurnStrategyCache.GetAll().Where(t => t.Player.Equals(player)).ToList();
			var sResult = TheGame().JTSServices.AIService.ExecuteStrategies(tactics);
			
			// TODO: Create and display report to user
		}


		private void DisplayTaskAssignmentResults<TResult, TObject>(IResult<TResult, TObject> result, IUnitTask task)
		{
			if (result.Status == ResultStatus.EXCEPTION)
			{
				TheGame().Renderer.DisplayUserMessage(MessageDisplayType.ERROR, 
														"Task {0} could not be assigned to {1}.".F(task.TaskType.Name, task.GetUnitAssigned().Name), 
														result.ex);
			}

			if (result.Status == ResultStatus.FAILURE)
			{
				TheGame().Renderer.DisplayUserMessage(MessageDisplayType.WARNING, 
														"Task {0} could not be assigned to {1}. {2}".F(task.TaskType.Name, task.GetUnitAssigned().Name, result.Message), 
														null);
			}

			if (result.Status == ResultStatus.SUCCESS)
			{
				TheGame().Renderer.DisplayUserMessage(MessageDisplayType.INFO, 
														"Task {0} assigned to {1}. Will complete in {2} turns.".F(task.TaskType.Name, task.GetUnitAssigned().Name, task.TurnsToComplete), 
														null);
			}
		}


#region Task Event Handlers

		private void On_TacticCompleted(object sender, EventArgs e)
		{

		}

		private void On_MissionCompleted(object sender, EventArgs e)
		{
		}

		private void On_TaskCompleted(object sender, EventArgs e)
		{

		}

		private void On_TaskExecuted(object sender, EventArgs e)
		{
		}

#endregion
		


#endregion

		
	}
}
