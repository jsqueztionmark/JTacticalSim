using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using JTacticalSim.API.Component;
using JTacticalSim.API;
using JTacticalSim.API.Service;
using JTacticalSim.Utility;

namespace JTacticalSim.Component.GameBoard
{
	public abstract class BoardComponentBase : GameComponentBase, IBoardComponent
	{		
		public virtual ICoordinate Location { get; set; }
		public virtual ISubNodeLocation SubNodeLocation { get; set; }
		public virtual ICountry Country { get; set; }
		public virtual string SpriteName { get { return this.Name; } }

		// Pathfinding Heuristic criteria
		public double? H {get { return TheGame().JTSServices.RulesService.CalculateMovementHeuristic(this); }}
		public int? G { get { return (Parent == null || Parent.G == null) ? 0 : Parent.G + 1; } }
		public double? F { get { return G + H; } }

		public IPathableObject Parent { get; set; }
		public IPathableObject Target { get; set; }
		public IPathableObject Source { get; set; }

		protected BoardComponentBase()
		{}

		protected BoardComponentBase(IPathableObject parent, 
										IPathableObject source, 
										IPathableObject target)
		{
			Parent = parent;
			Target = target;
			Source = source;
		}

		public INode GetNode()
		{
			return TheGame().JTSServices.NodeService.GetNodeAt(Location);
		}

		public virtual bool LocationEquals(ICoordinate rhs)
		{
			return this.Location.Equals(rhs);
		}

		//public bool IsClicked(int mouseX, int mouseY)
		//{
		//	return Bounds.Contains(mouseX, mouseY);
		//}

		public bool IsFriendly()
		{
			return Country.Faction.Equals(TheGame().CurrentPlayerFaction);
		}

		//public abstract IEnumerable<Rectangle?> GetSprites(SpriteSheet sheet);

		// IComparable

		public int CompareTo(IBoardComponent o)
		{
			if (F < o.F) return 1;
			if (F > o.F) return -1;
			
			return 0;
		}
	}
}
