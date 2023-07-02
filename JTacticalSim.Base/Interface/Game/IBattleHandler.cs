using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;

namespace JTacticalSim.API.Game
{
	public interface IBattleHandler : IBaseGameObject
	{
		IResult<IBattle, IBattle> BarrageUnitsAtLocation(INode target);
		IResult<IBattle, IBattle> DoBattleAtLocation(INode target);
		IResult<IBattle, IBattle> NukeLocation(INode target);
		
		/// <summary>
		/// Puts the moving faction on the defensive while moving through enemy territory for configured number of rounds. 
		/// </summary>
		/// <param name="locationNode"></param>
		/// <returns></returns>
		IResult<IBattle, IBattle> ForceMovementBattleEngagement(INode target);
	}
}
