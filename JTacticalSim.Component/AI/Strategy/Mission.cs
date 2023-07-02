using System;
using System.Linq;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.Utility;

namespace JTacticalSim.Component.AI
{
	public class Mission : AIStrategyComponentContainer<IMission, IUnitTask>, IMission, IComparable
	{
		public event EventHandler Executing;
		public event EventHandler Executed;

		// ICompletable
		public event CompletedEvent Completed;
		public int TurnsToComplete { get { return ChildComponents.Sum(c => c.TurnsToComplete); }}
		public bool IsComplete { get { return ChildComponents.All(c => c.IsComplete); }}

		protected override string ChildTypeName { get { return "Task"; }}
		protected override string ComponentTypeName { get { return "Mission"; }}

		public event EventHandler MissionAssigned;
		public event EventHandler NextTaskAssigned;

		private int _unitID { get; set; }
		public IMissionType MissionType { get; private set; }
		public IUnitTask CurrentTask { get; private set; }

		public Mission(IMissionType missionType, int unitID)
		{
			_unitID = unitID;
			MissionType = missionType;
		}

		public IResult<ITactic, ITactic> GetCurrentTactic()
		{
			var serviceResult = TheGame().JTSServices.AIService.GetCurrentTacticForMission(this);
			return serviceResult;
		}

		public IUnit GetAssignedUnit()
		{
			var r = TheGame().JTSServices.UnitService.GetUnitByID(_unitID);
			return r;
		}

		public void SortTasks()
		{
			ChildComponents.Sort();
		}

		public IResult<IMission, IUnitTask> Execute()
		{
			var r = new OperationResult<IMission, IUnitTask>();
			
			if (CurrentTask == null)
				SetCurrentTask(); // On first execute, assure the current task

			if (CurrentTask == null)
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("There are no current tasks for mission.");
				return r;
			}

			On_TaskExecuting(this, new EventArgs());
			CurrentTask.Execute();
			On_TaskExecuted(this, new EventArgs());

			// If all tasks are complete, fire our completed event
			if (IsComplete)
				On_TaskCompleted(this, new EventArgs());

			SetCurrentTask(); // Set the next task for the next turn

			return r;
		}

		public int DecrementTurnsToComplete()
		{
			throw new NotImplementedException();
		}

		public IUnitTask SetCurrentTask()
		{
			SortTasks();
			CurrentTask = ChildComponents.FirstOrDefault(task => !task.IsComplete);
			On_NextTaskAssigned(new EventArgs());
			return CurrentTask;
		}

		public new int CompareTo(object obj)
		{
			var o = obj as Mission;

			var PriorityLHS = MissionType.Priority;
			var PriorityRHS = o.MissionType.Priority;

			if (PriorityLHS > PriorityRHS) return 1;
			if (PriorityLHS < PriorityRHS) return -1;

			return 0;
		}

#region Event Handlers

		public void On_TaskCompleted(object sender, EventArgs e)
		{
			var mission = sender as IMission;
			TheGame().CurrentTurn.TaskExecutionReport.AppendLine("{0}'s mission '{1}' is completed!".F(mission.GetAssignedUnit().Name, mission.MissionType.Name));

			if (Completed != null) Completed(sender, e);
		}

		public void On_TaskExecuting(object sender, EventArgs e)
		{
			if (Executing != null) Executing(this, e);
		}

		public void On_TaskExecuted(object sender, EventArgs e)
		{
			if (Executed != null) Executed(this, e);
		}

		private void On_MissionAssigned(EventArgs e)
		{
			if (MissionAssigned != null) MissionAssigned(this, e);
		}

		private void On_NextTaskAssigned(EventArgs e)
		{
			if (NextTaskAssigned != null) NextTaskAssigned(CurrentTask, e);
		}

#endregion
	}
}
