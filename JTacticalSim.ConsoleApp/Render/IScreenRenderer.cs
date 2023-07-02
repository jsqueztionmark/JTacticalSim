using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleControls;

namespace JTacticalSim.ConsoleApp
{
	public interface IScreenRenderer
	{
		void RenderScreen();
		void RefreshScreen();
		void CloseScreen();
		void DisplayUserMessage(BoxDisplayType messageType, string Message, Exception ex);
	}
}
