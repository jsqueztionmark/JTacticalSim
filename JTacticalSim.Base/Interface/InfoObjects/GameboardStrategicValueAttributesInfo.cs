using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.InfoObjects
{
	/// <summary>
	/// Carries Minimum and Maximum values for strategic node values on the game board
	/// </summary>
	public class GameboardStrategicValueAttributesInfo
	{
		public StrategicAttributeInfo Defense { get; set; }
		public StrategicAttributeInfo Offense { get; set; }
		public StrategicAttributeInfo Stealth { get; set; }
		public StrategicAttributeInfo Movement { get; set; }
		public StrategicAttributeInfo VictoryPoints { get; set; }
	}
}
