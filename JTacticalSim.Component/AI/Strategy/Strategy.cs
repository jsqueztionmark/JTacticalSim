using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.Game;

namespace JTacticalSim.Component.AI
{
	[Obsolete("Tactics are now the highest level container in the strategy stack.")]
	public class Strategy : AIStrategyComponentContainer<IStrategy, ITactic>, IStrategy
	{
		public event EventHandler Executing;
		public event EventHandler Executed;

		protected override string ChildTypeName {get { return "Tactic"; }}
		protected override string ComponentTypeName {get { return "Strategy"; }}

		public IPlayer Player { get; private set; }
		public StrategicalStance Stance { get; private set; }

		// ICompletable
		public event CompletedEvent Completed;
		public int TurnsToComplete { get { return ChildComponents.Sum(c => c.TurnsToComplete); }}
		public bool IsComplete { get { return ChildComponents.All(c => c.IsComplete); }}

		public Strategy(StrategicalStance stance, IPlayer player)
		{
			Stance = stance;
			Player = player;
		}

		public IResult<IStrategy, ITactic> Execute()
		{
			var r = new OperationResult<IStrategy, ITactic>();

			// Complete each Mission
			// Catalog the results

			return r;
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
