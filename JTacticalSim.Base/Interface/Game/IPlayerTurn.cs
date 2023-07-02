using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Cache;

namespace JTacticalSim.API.Game
{
	/// <summary>
	/// Holds all the pertinent dynamic information for the scope of a player turn
	/// </summary>
	public interface IPlayerTurn
	{
		event TurnEndedEvent TurnEnded;
		event TurnStartedEvent TurnStarted;

		IPlayer Player { get; }

		/// <summary>
		/// Summary of assigned task actions that occurred when the turn started
		/// </summary>
		StringBuilder TaskExecutionReport { get; set; }
		
		/// <summary>
		/// Starts the turn
		/// </summary>
		void Start();

		/// <summary>
		/// Starts the turn. Let's the caching know if this is the first load of the game.
		/// </summary>
		/// <param name="isGameLoad"></param>
		void Start(bool isGameLoad);

		/// <summary>
		/// Ends the turn
		/// </summary>
		void End();
	}
}
