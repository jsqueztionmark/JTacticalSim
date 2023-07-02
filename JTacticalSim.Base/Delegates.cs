using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API
{

	public delegate void CommandCancelHandler();
	public delegate void ToComponentHandler();
	public delegate void ToDTOHandler();

	public delegate void GameStateChangedEvent(object sender, EventArgs e);

	public delegate void ComponentSelectedEvent(object sender, ComponentSelectedEventArgs e);
	public delegate void ComponentUnSelectedEvent(object sender, ComponentUnSelectedEventArgs e);
	public delegate void ComponentClickedEvent(object sender, ComponentClickedEventArgs e);

	public delegate void BoardPreRenderEvent(object sender, EventArgs e);
	public delegate void BoardPostRenderEvent(object sender, EventArgs e);

	public delegate void TilePreRenderEvent(object sender, EventArgs e);
	public delegate void TilePostRenderEvent(object sender, EventArgs e);

	public delegate void NodePreRenderEvent(object sender, EventArgs e);
	public delegate void NodePostRenderEvent(object sender, EventArgs e);

	public delegate void UnitPreRenderEvent(object sender, EventArgs e);
	public delegate void UnitPostRenderEvent(object sender, EventArgs e);
	public delegate void UnitAttachedEvent(object sender, EventArgs e);
	public delegate void UnitDetachedEvent(object sender, EventArgs e);
	public delegate void UnitsLoadedEvent(object sender, UnitsLoadedEventArgs e);
	public delegate void UnitsDeployedEvent(object sender, UnitsDeployedEventArgs e);

	public delegate void ComponentMoveEvent(object sender, ComponentMoveEventArgs e);
	public delegate void WaypointReachedEvent(object sender, ComponentMoveEventArgs e);

	public delegate void TurnEndedEvent(object sender, EventArgs e);
	public delegate void TurnStartedEvent(object sender, PlayerTurnStartEventArgs e);

	public delegate void BattleRoundStart(object sender, EventArgs e);
	public delegate void BattleRoundEnd(object sender, EventArgs e);
	public delegate void BattleRoundPreRenderEvent(object sender, EventArgs e);
	public delegate void BattleRoundPostRenderEvent(object sender, EventArgs e);
	public delegate void BattleStart(object sender, EventArgs e);
	public delegate void BattleEnd(object sender, EventArgs e);
	public delegate void BattlePreRenderEvent(object sender, EventArgs e);
	public delegate void BattlePostRenderEvent(object sender, EventArgs e);
	public delegate void BattleSkirmishStart(object sender, EventArgs e);
	public delegate void BattleSkirmishEnd(object sender, EventArgs e);
	public delegate void BattleSkirmishPreRenderEvent(object sender, EventArgs e);
	public delegate void BattleSkirmishPostRenderEvent(object sender, EventArgs e);

	public delegate void CompletedEvent(object sender, EventArgs e);

}
