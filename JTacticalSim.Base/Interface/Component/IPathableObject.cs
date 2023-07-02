
namespace JTacticalSim.API.Component
{
	public interface IPathableObject
	{
		ICoordinate Location { get; set; }
		ISubNodeLocation SubNodeLocation { get; set; }

		// Pathfinding heuristic criteria
		IPathableObject Parent { get; set; }
		IPathableObject Target { get; set; }
		IPathableObject Source { get; set; }
		
		/// <summary>
		/// Integer given by heuristic
		/// </summary>
		double? H { get; } 

		/// <summary>
		/// Distance traveled from the starting point
		/// </summary>
		int? G { get; }

		/// <summary>
		/// G + H : Total 'cost' of travel
		/// </summary>
		double? F { get; }

		INode GetNode();
	}
}
