using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.InfoObjects
{
	public class GameboardAttributeInfo
	{
		/// <summary>
		/// Displayable name for the board
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Number of cells (nodes) in y axis
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// Number of cells (nodes) in x axis
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Number of cells to draw in height at any given time
		/// </summary>
		public int DrawHeight { get; set; }

		/// <summary>
		/// Number of cells to draw in width at any given time
		/// </summary>
		public int DrawWidth { get; set; }

		/// <summary>
		/// The pixel length of a cell vertex
		/// </summary>
		public int CellSize { get; set; }

		/// <summary>
		/// Real world measurement of cell vertex
		/// </summary>
		public int CellMeters { get; set; }

		/// <summary>
		/// Max number of units that can occupy a cell at any one time
		/// Should be set to correspond to the real world size of a cell
		/// </summary>
		public int CellMaxUnits { get; set; }
	}
}
