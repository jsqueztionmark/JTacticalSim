using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API;
using JTacticalSim.API.Game;

namespace JTacticalSim.Component.AI
{
	public class Tactic : AIStrategyComponentContainer<ITactic, IMission>, ITactic
	{
		public event EventHandler Executing;
		public event EventHandler Executed;

		protected override string ChildTypeName {get { return "Mission"; }}
		protected override string ComponentTypeName {get { return "Tactic"; }}
		
		public IPlayer Player { get; private set; }
		public StrategicalStance Stance { get; private set; }
		
		// ICompletable
		public event CompletedEvent Completed;
		public int TurnsToComplete { get { return ChildComponents.Max(c => c.TurnsToComplete); } }
		public bool IsComplete { get { return ChildComponents.All(c => c.IsComplete); }}

		public Tactic(StrategicalStance stance, IPlayer player)
		{
			Player = player;
			Stance = stance;
		}

		public IResult<ITactic, IMission> Execute()
		{
			var result = new OperationResult<ITactic, IMission> { Status = ResultStatus.SUCCESS };
			
			// NOTE: We're only sorting on Priority - not sure what we'll do with TurnOrder (necessary?)
			// If so - it will only matter for the Player AI
			ChildComponents.Sort();

			foreach (var mission in ChildComponents)
			{
				var mResult = mission.Execute();
				result.Messages.Add(mResult.Message);

				if (mResult.Status != ResultStatus.SUCCESS)
				{
					result.SuccessfulObjects.Add(mission);
				}
				else
				{
					result.FailedObjects.Add(mission); 
					result.Status = (result.SuccessfulObjects.Any()) ? ResultStatus.SOME_FAILURE : ResultStatus.FAILURE;
				}
			}

			if (IsComplete)
			{
				// If the full tactic is complete, remove the tactic from cache
				var removeResult = TheGame().JTSServices.AIService.RemoveTactic(this);

				//TODO: handle result

				On_TaskCompleted(this, new EventArgs());
			}

			return result;
		}

		public int DecrementTurnsToComplete()
		{
			throw new NotImplementedException();
		}


		public void On_TaskCompleted(object sender, EventArgs e)
		{
			if (Completed != null) Completed(sender, e);
		}

		public void On_TaskExecuting(object sender, EventArgs e)
		{
			if (Executing != null) Executing(sender, e);
		}

		public void On_TaskExecuted(object sender, EventArgs e)
		{
			if (Executed != null) Executed(sender, e);
		}

	}
}
