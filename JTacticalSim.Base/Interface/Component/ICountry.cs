using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.Component
{
	public interface ICountry : IBaseComponent, ISpriteContainer
	{
		IFaction Faction { get; set; }

        string FlagDisplayTextA { get; set; }
		string FlagDisplayTextB { get; set; }
        string ColorString { get; set; }
		string BGColorString { get; set; }
		string TextDisplayColorString { get; set; }
		string FlagColorAString { get; set; }
		string FlagColorBString { get; set; }
		string FlagBGColorString { get; set; }
        ConsoleColor Color { get; set; }
		ConsoleColor BGColor { get; set; }
		ConsoleColor TextDisplayColor { get; set; }
		ConsoleColor FlagBGColor { get; set; }
		ConsoleColor FlagColorA { get; set; }
		ConsoleColor FlagColorB { get; set; }
	}
}
