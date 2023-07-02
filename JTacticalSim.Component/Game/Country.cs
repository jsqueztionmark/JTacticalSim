using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.Utility;

namespace JTacticalSim.Component.Game
{
	public class Country : GameComponentBase, ICountry
	{
		public string SpriteName { get { return "flag_{0}".F(this.Name); }}
        public string FlagDisplayTextA { get; set; }
		public string FlagDisplayTextB { get; set; }
        public string ColorString { get; set; }
		public string BGColorString { get; set; }
		public string TextDisplayColorString { get; set; }
		public string FlagColorAString { get; set; }
		public string FlagColorBString { get; set; }
		public string FlagBGColorString { get; set; }
        public ConsoleColor Color { get; set; }
		public ConsoleColor BGColor { get; set; }
		public ConsoleColor TextDisplayColor { get; set; }
		public ConsoleColor FlagBGColor { get; set; }
		public ConsoleColor FlagColorA { get; set; }
		public ConsoleColor FlagColorB { get; set; }

		public IFaction Faction { get; set; }
	}
}
