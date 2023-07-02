using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.AI
{
	public interface IMission : IAIStrategyObjectContainer<IUnitTask>, ICompletable, IExecutableTask<IMission, IUnitTask>
	{
		event EventHandler MissionAssigned;
		event EventHandler NextTaskAssigned;

		IMissionType MissionType { get; }
		IUnitTask CurrentTask { get; }

		/// <summary>
		/// Returns the Tactic that this mission is a child of
		/// </summary>
		/// <returns></returns>
		IResult<ITactic, ITactic> GetCurrentTactic();
		IUnit GetAssignedUnit();
		void SortTasks();
		IUnitTask SetCurrentTask();
	}
}
