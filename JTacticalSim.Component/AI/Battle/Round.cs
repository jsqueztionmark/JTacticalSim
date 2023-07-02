using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;

namespace JTacticalSim.Component.AI
{
	/// <summary>
	/// Represents a full round of battle 
	/// </summary>
	public class Round : GameComponentBase, IRound
	{
		public event BattleRoundStart RoundStart;
		public event BattleRoundEnd RoundEnd;

		public ISkirmish CurrentSkirmish { get; private set; }
		public int CurrentSkirmishCount { get { return Skirmishes.IndexOf(CurrentSkirmish) + 1; }}
		private IBattle _battle { get; set; }
		public List<ISkirmish> Skirmishes { get; private set; }

		public Round(IBattle battle)
		{
			_battle = battle;
			Skirmishes = new List<ISkirmish>();
		}

		public void AddSkirmish(ISkirmish skirmish)
		{
			Skirmishes.Add(skirmish);
		}

		public IEnumerable<IUnit> GetDefeatedUnits() { return Skirmishes.SelectMany(s => s.Destroyed); }
		
		public void DoBattle() 
		{
			On_RoundStart(new EventArgs());
			Skirmishes.ForEach(s => 
				{
					// Set the current skirmish
					CurrentSkirmish = ((ISkirmish)s);
					Render();
					s.DoBattle();
				});

			On_RoundEnd(new EventArgs());
		}

		public void Render()
		{
			TheGame().Renderer.On_RoundPreRender(new EventArgs());
			TheGame().Renderer.RenderBattleRound(this);
			TheGame().Renderer.On_RoundPostRender(new EventArgs());
		}

	// Event Handlers

		public void On_RoundStart(EventArgs e)
		{
			CurrentSkirmish = null;
			if (RoundStart != null) RoundStart(this, e);
		}

		public void On_RoundEnd(EventArgs e)
		{
			if (RoundEnd != null) RoundEnd(this, e);
		}
	}
}
