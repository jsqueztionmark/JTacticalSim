using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;
using JTacticalSim.API.InfoObjects;

namespace JTacticalSim.API.Game
{
	public interface IPlayer : IBaseComponent, IInfoDisplayable
	{
		ICountry Country { get; }
		List<IUnit> UnplacedReinforcements { get; }
		PlayerTrackedValuesInfo TrackedValues { get; set; }
		
		bool IsCurrentPlayer { get; }
		bool IsAIPlayer { get; set; }

		IResult<IUnit, IUnit> AddReinforcementUnit(IUnit unit);
		IResult<IUnit, IUnit> RemoveReinforcementUnit(IUnit unit);
	}
}
