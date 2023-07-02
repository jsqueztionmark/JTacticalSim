using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleControls
{
	public class Screen : ConsoleBox
	{
		public Screen()
			: base(BoxDisplayType.DISPLAY, PromptType.NONE)
		{
			HasFocus = true;
			CloseWindowX = true;
			FillElement = '░';
			DrawElements = new DoubleLineBoxElements();
		}

		protected override void HandlePrompt()
		{
			ConsoleKeyInfo keyInfo = Console.ReadKey(true);

			if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0)
			{
				if (keyInfo.Key == ConsoleKey.X)
				{
					On_WindowClosePressed();
					return;
				}
			}
		}
	}
}
