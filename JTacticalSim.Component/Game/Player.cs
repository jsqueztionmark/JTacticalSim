using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using JTacticalSim.API;
using JTacticalSim.API.Game;
using JTacticalSim.API.Component;
using JTacticalSim.Utility;
using JTacticalSim.API.InfoObjects;

namespace JTacticalSim.Component.Game
{
	public class Player : GameComponentBase, IPlayer, IInfoDisplayable
	{
		public ICountry Country { get; private set; }
		public List<IUnit> UnplacedReinforcements { get; private set; }

		public PlayerTrackedValuesInfo TrackedValues { get; set; }

		public bool IsCurrentPlayer { get { return this.TheGame().CurrentTurn.Player.Equals(this); }}
		public bool IsAIPlayer { get; set; }

		public Player(ICountry country)
		{
			Country = country;
			UnplacedReinforcements = new List<IUnit>();
			TrackedValues = new PlayerTrackedValuesInfo();
		}

		public void DisplayInfo()
		{
			TheGame().Renderer.DisplayPlayerInfo(this);
		}

		public IResult<IUnit, IUnit> AddReinforcementUnit(IUnit unit)
		{
			var result = TheGame().JTSServices.UnitService.SaveReinforcementUnitToPlayer(this, unit);
			return result;
		}

		public IResult<IUnit, IUnit> RemoveReinforcementUnit(IUnit unit)
		{
			var result = TheGame().JTSServices.UnitService.RemoveReinforcementUnitFromPlayer(this, unit);
			return result;
		}
	}
}
