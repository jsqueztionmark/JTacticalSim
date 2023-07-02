using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;

namespace JTacticalSim.API.Game
{
	public interface IStrategyHandler : IBaseGameObject
	{
		/// <summary>
		/// Handles the saving of a tactic in cache and hooks up events
		/// </summary>
		/// <param name="tactic"></param>
		/// <returns></returns>
		IResult<ITactic, ITactic> SetUpTactic(ITactic tactic);

		/// <summary>
		/// Handles the creation of a single task mission for a unit. Stores in Strategy Cache.
		/// </summary>
		/// <param name="mission"></param>
		/// <param name="task"></param>
		/// <param name="stance"></param>
		IResult<IMission, IMission> SetUpUnitMission(IMission mission, IUnitTask task, StrategicalStance stance, IPlayer player);

		/// <summary>
		/// Executes all current strategies for a player
		/// </summary>
		/// <param name="player"></param>
		void ExecuteStrategiesForPlayer(IPlayer player);
	}
}
