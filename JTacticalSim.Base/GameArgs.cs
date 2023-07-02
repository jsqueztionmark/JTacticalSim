using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;

namespace JTacticalSim.API
{
	public class ComponentMoveEventArgs : EventArgs
	{
		public INode SourceNode { get; private set; }
		public INode TargetNode { get; private set; }

		public ComponentMoveEventArgs(INode sourceNode, INode targetNode)
		{
			SourceNode = sourceNode;
			TargetNode = targetNode;
		}
	}

	public class ComponentClickedEventArgs : EventArgs
	{
		public MouseButton ButtonClicked { get; private set; }

		public ComponentClickedEventArgs(MouseButton buttonClicked)
		{
			ButtonClicked = buttonClicked;
		}
	}

	public class ComponentSelectedEventArgs : EventArgs
	{
		public ComponentSelectedEventArgs()
		{}
	}

	public class ComponentUnSelectedEventArgs : EventArgs
	{
		public ComponentUnSelectedEventArgs()
		{ }
	}

	public class UnitsLoadedEventArgs : EventArgs
	{
		public List<IUnit> UnitsLoaded { get; private set; }

		public UnitsLoadedEventArgs(List<IUnit> unitsLoaded)
		{
			UnitsLoaded = unitsLoaded;
		}
	}

	public class UnitsDeployedEventArgs : EventArgs
	{
		public List<IUnit> UnitsDeployed { get; private set; }

		public UnitsDeployedEventArgs(List<IUnit> unitsDeployed)
		{
			UnitsDeployed = unitsDeployed;
		}
	}

	public class GameStateChangedEventArgs : EventArgs
	{
		public StateType NewStateType { get; private set; }

		public GameStateChangedEventArgs(StateType newStateType)
		{
			NewStateType = newStateType;
		}
	}

	public class PlayerTurnStartEventArgs : EventArgs
	{
		/// <summary>
		/// Determines Whether the event is firing on first load
		/// </summary>
		public bool IsGameLoad { get; private set; }

		public PlayerTurnStartEventArgs(bool isGameLoad)
		{
			IsGameLoad = isGameLoad;
		}
	}

	public class RoundEndedEventArgs : EventArgs
	{
		/// <summary>
		/// Allows for battle scripts to be run repeatedly from the same context
		/// </summary>
		public bool RemoveUnitsFromGame { get; private set; }

		public RoundEndedEventArgs(bool removeUnitsFromGame)
		{
			RemoveUnitsFromGame = removeUnitsFromGame;
		}
	}

	public class TaskExecutionArgument
	{
		public string Type { get; private set; }
		public string Assembly { get; private set; }
		public string Name { get; private set; }
		public IEnumerable<string> Values { get; private set; }
		public bool IsCollection { get { return Values.Count() > 1; } }
		public bool HasValues { get { return Values.Any(); } }

		public TaskExecutionArgument(string type, string assembly, string name, IEnumerable<string> values)
		{
			Type = type;
			Assembly = assembly;
			Name = name;
			Values = values;
		}
	}

}
