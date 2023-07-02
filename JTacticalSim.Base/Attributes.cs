using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Game.State;

namespace JTacticalSim.API
{
	/// <summary>
	/// Used to determine which commands are appropriate for which game states
	/// </summary>
	public class CommandAvailable : Attribute
	{
		public IEnumerable<StateType> AvailableInStates { get; set; }

		public CommandAvailable(params object[] availableInStates)
		{
			if (availableInStates.Any(ais => ais.GetType() != typeof(StateType)))
				throw new ArgumentException("Parameter is not valid StateType enum.");
			
			AvailableInStates = availableInStates.Cast<StateType>();
		}
	}

	/// <summary>
	/// Used to decorate the Commands so they can be parsed for the user
	/// </summary>
	public class Command : Attribute
	{
		public Commands CommandIdentifier { get; private set; }
		public string CommandName { get; private set; }
		public string DisplayName { get; private set; }
		public string Alias { get; private set; }
		public CommandType Type { get; private set; }
		public bool RefreshScreen { get; private set; }
		public string Args { get; private set; }
		public bool HideMe { get; private set; }

		public Command(Commands commandIdentifier,
						string commandName, 
						string displayName,
						string alias, 
						CommandType type, 
						bool refreshScreen, 
						string args,
						bool hideMe)
		{
			CommandIdentifier = commandIdentifier;
			CommandName = commandName;
			DisplayName = displayName;
			Alias = alias;
			Type = type;
			RefreshScreen = refreshScreen;
			Args = args;
			HideMe = hideMe;
		}
	}

	public class TableRecognizable : Attribute
	{
		public Type ComponentType { get; set; }
		public Type RecordType { get; set; }

		public TableRecognizable(Type componentType, Type recordType)
		{
			ComponentType = componentType;
			RecordType = recordType;
		}
	}
}
