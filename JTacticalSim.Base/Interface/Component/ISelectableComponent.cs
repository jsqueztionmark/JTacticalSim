using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.Component
{
	public interface ISelectableComponent : IClickableComponent
	{
		// Events
		event ComponentSelectedEvent ComponentSelected;
		event ComponentUnSelectedEvent ComponentUnSelected;
		void On_ComponentSelected(ComponentSelectedEventArgs e);
		void On_ComponentUnSelected(ComponentUnSelectedEventArgs e);

		void Select();
		void UnSelect();
	}
}
