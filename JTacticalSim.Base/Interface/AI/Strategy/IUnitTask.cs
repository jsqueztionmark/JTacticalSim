using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.AI;

namespace JTacticalSim.API.AI
{
	public interface IUnitTask : IBaseComponent, ICompletable, IExecutableTask<IUnitTask, IUnitTask>
	{
		IUnitTaskType TaskType { get; }
		IEnumerable<TaskExecutionArgument> Args { get; }

		/// <summary>
		/// Returns the step order for the task based on the task's current parent mission
		/// </summary>
		/// <returns></returns>
		IResult<int, int> GetStepOrderForCurrentMission();

		/// <summary>
		/// Returns the Mission that this task is a child of
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		IResult<IMission, IMission> GetCurrentMission();

		/// <summary>
		/// Returns the Mission's assigned unit
		/// </summary>
		/// <returns></returns>
		IUnit GetUnitAssigned();
	}
}
