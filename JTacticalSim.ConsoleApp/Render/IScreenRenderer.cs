using System;
using System.Text;
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
