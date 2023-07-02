using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using JTacticalSim.API.Component;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.Utility;
using ConsoleControls;

namespace JTacticalSim.ConsoleApp
{
	public static class Extension
	{
		public static string FullTabbedDisplayName(this IUnit unit)
		{
				var groupTypeIndent = 4;
				var unitTypeIndent = 3;

				var sb = new StringBuilder();
				var groupOffset = groupTypeIndent - unit.UnitInfo.UnitGroupType.TextDisplayZ4.Length;
				var unitTypeOffset = unitTypeIndent - unit.UnitInfo.UnitType.TextDisplayZ4.Length;

				sb.Append(unit.UnitInfo.UnitGroupType.TextDisplayZ4);

				for (var i = 0; i < groupOffset; i++)
					sb.Append(" ");

				sb.Append("{0} ".F(string.IsNullOrWhiteSpace(unit.UnitInfo.UnitClass.TextDisplayZ4) ? " " : unit.UnitInfo.UnitClass.TextDisplayZ4));

				sb.Append(unit.UnitInfo.UnitType.TextDisplayZ4);

				for (var i = 0; i < unitTypeOffset; i++)
					sb.Append(" ");

				sb.Append(unit.Name);

				return sb.ToString();				
		}

		public static int TotalDisplayLength(this IUnit unit)
		{
			// TODO: This may need to change by zoome level but I think it really needs to get the MAX length
			return unit.Name.Length +	((unit.UnitInfo.UnitClass != null) ? unit.UnitInfo.UnitClass.TextDisplayZ4.Length : 0) + 
										((unit.UnitInfo.UnitType != null) ? unit.UnitInfo.UnitType.TextDisplayZ4.Length : 0) + 
										((unit.UnitInfo.UnitGroupType != null) ? unit.UnitInfo.UnitGroupType.TextDisplayZ4.Length : 0); 
		
		}

		public static bool IsWithinConsoleControlArea(this INode node, ConsoleControl control, ZoomInfo zoom, int leftOffset, int topOffset)
		{
			// determine the node's current absolute position
			var nodeX = ((node.Location.X - zoom.CurrentOrigin.X) * zoom.ColumnSpacing) + (leftOffset + 1);
			var nodeY = ((node.Location.Y - zoom.CurrentOrigin.Y) * zoom.RowSpacing) + (topOffset + 1);
			var controlBottom = control.TopOrigin + control.Height + 2; // allow for dropshadow
			var controlRight = control.LeftOrigin + control.Width;	// allow for dropshadow

			return nodeX >= control.LeftOrigin - zoom.ColumnSpacing &&
					nodeX <= controlRight &&
					nodeY >= control.TopOrigin - zoom.RowSpacing &&
					nodeY <= controlBottom;
		}

		public static void RedrawControlAffectedNodes(this ConsoleControl control)
		{
			var game = Game.Instance;
			var zoom = game.ZoomHandler.CurrentZoom;
			var leftOffset = Global.Measurements.WESTMARGIN;
			var rightOffset = Global.Measurements.NORTHMARGIN;
			var nodes = game.GameBoard.CurrentViewableAreaNodes
												.Where(n => n.IsWithinConsoleControlArea(control, zoom, leftOffset, rightOffset));
			game.Renderer.RefreshNodes(nodes);
		}
	}
}
