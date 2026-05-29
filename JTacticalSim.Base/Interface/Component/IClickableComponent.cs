using System;
using System.Text;

namespace JTacticalSim.API.Component
{
	public interface IClickableComponent
	{
		// Events
		event ComponentClickedEvent ComponentClicked;
		void On_ComponentClicked(object sender, ComponentClickedEventArgs e);
	}
}
