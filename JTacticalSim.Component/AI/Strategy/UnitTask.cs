using System;
using System.Transactions;
using System.Collections.Generic;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API;
using JTacticalSim.Utility;
using JTacticalSim.AI;

namespace JTacticalSim.Component.AI
{
	public class UnitTask : GameComponentBase, IUnitTask, IComparable
	{
		public event EventHandler Executing;
		public event EventHandler Executed;

		// ICompletable
		public event CompletedEvent Completed;
		public bool IsComplete { get { return TurnsToComplete == 0; }}
		private int _turnsToComplete;
		public int TurnsToComplete { get { return _turnsToComplete; } }

		public IUnitTaskType TaskType { get; private set; }
		private IMission _mission { get; set; }
		public IEnumerable<TaskExecutionArgument> Args { get; private set; }


		public UnitTask(IUnitTaskType taskType, 
						IMission mission,
 						IEnumerable<TaskExecutionArgument> args,
						int turnsToComplete)
		{
			if (taskType == null)
				throw new ArgumentNullException("taskType is a required parameter.");

			TaskType = taskType;
			_mission = mission;
			Args = args;
			_turnsToComplete = turnsToComplete;
		}

		public IUnit GetUnitAssigned()
		{
			return _mission.GetAssignedUnit();
		}

		public IResult<IMission, IMission> GetCurrentMission()
		{
			var serviceResult = TheGame().JTSServices.AIService.GetCurrentMissionForUnitTask(this);
			return serviceResult;
		}

		public IResult<int, int> GetStepOrderForCurrentMission()
		{
			var serviceResult = TheGame().JTSServices.DataService.LookupUnitTaskTypeStepOrderForMissionType(this.TaskType, _mission.MissionType);
			return serviceResult;
		}

		public IResult<IUnitTask, IUnitTask> Execute()
		{
			using (var txn = new TransactionScope())
			{
				var taskHandler = new UnitTaskHandler();
				On_TaskExecuting(this, new EventArgs());
				var r = taskHandler.ExecuteTask(this, Args);

				if (r.Status == ResultStatus.SUCCESS)
				{
					r.Messages.Add("Task Executed. {0} turns left to complete.".F(TurnsToComplete));
					r.SuccessfulObjects.Add(this);

					On_TaskExecuted(this, new EventArgs());

					if (IsComplete)
						On_TaskCompleted(this, new EventArgs());
					
					txn.Complete();
					return r;					
				}
				if (r.Status == ResultStatus.EXCEPTION || r.Status == ResultStatus.FAILURE)
				{
					r.Messages.Add("Task Not Executed. {0} turns left to complete.".F(TurnsToComplete));
					r.FailedObjects.Add(this);
					return r;
				}

				return r;
			}
		}

		public int DecrementTurnsToComplete()
		{
			if (_turnsToComplete > 0) _turnsToComplete--;
			return _turnsToComplete;
		}

		// Using the cache to grab the parent mission does not work as the cache is not updated
		// between sort operations. This COULD be remedied, but it's not really necessary as the
		// tasks lifecycle never modifies the parent mission anyway. Maybe a 'TODO'. I'm leaving
		// the other code intact.
		public new int CompareTo(object obj)
		{
			var o = obj as UnitTask;

			var stepOrderLHS = GetStepOrderForCurrentMission().Result;
			var stepOrderRHS = o.GetStepOrderForCurrentMission().Result;

			if (stepOrderLHS > stepOrderRHS) return 1;
			if (stepOrderLHS < stepOrderRHS) return -1;

			return 0;
		}

#region Event Handlers

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
			var task = sender as IUnitTask;

			if (!task.IsComplete)
			{
				TheGame().CurrentTurn.TaskExecutionReport.AppendLine("{0} performed task {1}. {2} more turns to complete.".F(task.GetUnitAssigned().Name, task.TaskType.Name, task.TurnsToComplete));
			}
			if (Executed != null) Executed(sender, e);
		}

#endregion
		

	}
}
