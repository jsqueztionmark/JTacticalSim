using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.Utility
{
    public static class Render
    {
        public static ConsoleColor GetConsoleColorByString(string color)
        {
            switch (color.ToLowerInvariant())
            {
				case "green":
					return ConsoleColor.Green;
                case "darkgreen":
                    return ConsoleColor.DarkGreen;
                case "red":
                    return ConsoleColor.Red;
				case "darkred":
                    return ConsoleColor.DarkRed;
				case "magenta":
					return ConsoleColor.Magenta;
				case "darkmagenta":
					return ConsoleColor.DarkMagenta;
                case "blue":
                    return ConsoleColor.Blue;
				case "darkblue":
					return ConsoleColor.DarkBlue;
				case "yellow":
                    return ConsoleColor.Yellow;
				case "darkyellow":
		            return ConsoleColor.DarkYellow;
				case "white":
					return ConsoleColor.White;
				case "gray":
		            return ConsoleColor.Gray;
				case "darkgray":
					return ConsoleColor.DarkGray;				
				case "cyan":
					return ConsoleColor.Cyan;
				case "darkcyan":
					return ConsoleColor.DarkCyan;
                default:
                    return ConsoleColor.Black;

            }
        }
    }
}
