using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.Game
{
	public class TileConsoleRenderHelper
	{

		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		// console rendering helper properties
		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		// geog/terrain

		public bool HasMountains { get; set; }
		public bool HasMountain { get; set; }
		public bool HasTown { get; set; }
		public bool HasHills { get; set; }
		public bool HasLakes { get; set; }
		public bool HasRivers { get; set; }
		public bool HasCreeks { get; set; }
		public bool HasShoreLineNorth { get; set; }
		public bool HasShoreLineSouth { get; set; }
		public bool HasShoreLineEast { get; set; }
		public bool HasShoreLineWest { get; set; }
		public bool HasShoreLineNorthWest { get; set; }
		public bool HasShoreLineSouthWest { get; set; }
		public bool HasShoreLineNorthEast { get; set; }
		public bool HasShoreLineSouthEast { get; set; }

		public bool IsRiver { get; set; }
		public bool IsSea { get; set; }
		public bool IsNuclearWasteland { get; set; }

		// flora 

		public bool HasForests { get; set; }
		public bool HasWoodlands { get; set; }
		public bool HasMarsh { get; set; }
		public bool HasTrees { get; set; }

		// infrastructure

		public bool HasMilitaryBase { get; set; }
		public bool HasCommandPost { get; set; }
		public bool HasAirports { get; set; }
		public bool HasCities { get; set; }
		public bool HasIndustrial { get; set; }
		public bool HasRoad { get; set; }
		public bool HasBridge { get; set; }
		public bool HasDam { get; set; }
		public bool HasTracks { get; set; }
	}
}
