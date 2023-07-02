using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleControls
{
	public interface IConsoleControl
	{
		int LeftOrigin { get; set; }
		int TopOrigin { get; set; }
		int Height { get; set; }
		int Width { get; set; }
		ConsoleColor BackColor { get; set; }
		ConsoleColor ForeColor { get; set; }
		bool DropShadow { get; set; }
		bool HasFocus { get; set; }

		int Draw();
		void Fill(ConsoleColor bgColor);
		void Fill();
		int FillText();
		void Clear();
		int ClearAndRedraw();
		void Erase();
	}
}
