using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;

namespace JTacticalSim.Component.GameBoard
{
	public class PathNode : GameComponentBase, IPathNode
	{
		public virtual ICoordinate Location { get; set; }
		public virtual ISubNodeLocation SubNodeLocation { get; set; }
        public bool HasOverrideInDirection { get; set; }

		// Pathfinding Heuristic criteria
		public double? H {get { return TheGame().JTSServices.RulesService.CalculateMovementHeuristic(this); }}
		public int? G { get { return (Parent == null || Parent.G == null) ? 0 : Parent.G + 1; } }
		public double? F { get { return G + H; } }

		public IPathableObject Parent { get; set; }
		public IPathableObject Target { get; set; }
		public IPathableObject Source { get; set; }

		public INode GetNode()
		{
			return TheGame().JTSServices.NodeService.GetNodeAt(Location);
		}
	}
}
