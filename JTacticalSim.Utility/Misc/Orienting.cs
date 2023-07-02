using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.Utility
{
	public static class Orienting
	{
		private static IDictionary<int, IDictionary<Direction, IEnumerable<int>>> _adjacentSubnodesMatrix;

		public static IDictionary<int, IDictionary<Direction, IEnumerable<int>>> AdjacentSubnodesMatrix
		{
			get
			{
				if (_adjacentSubnodesMatrix == null)
				{
					_adjacentSubnodesMatrix = new Dictionary<int, IDictionary<Direction, IEnumerable<int>>>();

					_adjacentSubnodesMatrix.Add(1, new Dictionary<Direction, IEnumerable<int>>()
						{
							{Direction.NONE, new[] {2,5,4}},
							{Direction.NORTH, new[] {7,8}},
							{Direction.NORTHWEST, new[] {9}},
							{Direction.WEST, new[] {3,6}}
						});
					_adjacentSubnodesMatrix.Add(2, new Dictionary<Direction, IEnumerable<int>>()
						{
							{Direction.NONE, new[] {1,4,5,6,3}},
							{Direction.NORTH, new[] {7,8,9}},
						});
					_adjacentSubnodesMatrix.Add(3, new Dictionary<Direction, IEnumerable<int>>()
						{
							{Direction.NONE, new[] {2,5,6}},
							{Direction.NORTH, new[] {8,9}},
							{Direction.NORTHEAST, new[] {7}},
							{Direction.EAST, new[] {1,4}}
						});
					_adjacentSubnodesMatrix.Add(4, new Dictionary<Direction, IEnumerable<int>>()
						{
							{Direction.NONE, new[] {1,2,5,8,7}},
							{Direction.WEST, new[] {3,6,9}},
						});
					_adjacentSubnodesMatrix.Add(5, new Dictionary<Direction, IEnumerable<int>>()
						{
							{Direction.NONE, new[] {1,2,3,4,6,7,8,9}}
						});
					_adjacentSubnodesMatrix.Add(6, new Dictionary<Direction, IEnumerable<int>>()
						{
							{Direction.NONE, new[] {3,2,5,8,9}},
							{Direction.EAST, new[] {1,4,7}},
						});
					_adjacentSubnodesMatrix.Add(7, new Dictionary<Direction, IEnumerable<int>>()
						{
							{Direction.NONE, new[] {4,5,8}},
							{Direction.WEST, new[] {6,9}},
							{Direction.SOUTHWEST, new[] {3}},
							{Direction.SOUTH, new[] {1,2}}
						});
					_adjacentSubnodesMatrix.Add(8, new Dictionary<Direction, IEnumerable<int>>()
						{
							{Direction.NONE, new[] {7,4,5,6,9}},
							{Direction.SOUTH, new[] {1,2,3}},
						});
					_adjacentSubnodesMatrix.Add(9, new Dictionary<Direction, IEnumerable<int>>()
						{
							{Direction.NONE, new[] {8,5,6}},
							{Direction.EAST, new[] {4,7}},
							{Direction.SOUTHEAST, new[] {1}},
							{Direction.SOUTH, new[] {2,3}}
						});
							

				}
				
				return _adjacentSubnodesMatrix;
			}
			
		}

		/// <summary>
		/// Returns a direction enum based on an input string. Returns Direction.NONE if no match is found
		/// </summary>
		/// <param name="direction"></param>
		/// <returns></returns>
		public static Direction ConvertStringToDirection(string direction)
		{
			switch (direction.ToLowerInvariant())
			{
				case "2" :
				case "n" :
				case "north" :
					return Direction.NORTH;
				case "8" :
				case "s" :
				case "south" :
					return Direction.SOUTH;
				case "6 " :
				case "e" :
				case "east" :
					return Direction.EAST;
				case "4" :
				case "w" :
				case "west" :
					return Direction.WEST;
				case "1" :
				case "nw" :
				case "northwest" :
					return Direction.NORTHWEST;
				case "3" :
				case "ne" :
				case "northeast" :
					return Direction.NORTHEAST;
				case "9" :
				case "se" :
				case "southeast" :
					return Direction.SOUTHEAST;
				case "7" :
				case "sw" :
				case "southwest" :
					return Direction.SOUTHWEST;
				default:
					return Direction.NONE;
			}
		}

		public static int ConvertDirectionToSubNodeLocation(Direction direction)
		{
			switch (direction)
			{
				case Direction.NORTH :
					return 2;
				case Direction.SOUTH :
					return 8;
				case Direction.WEST :
					return 4;
				case Direction.EAST :
					return 6;
				case Direction.NORTHWEST :
					return 1;
				case Direction.NORTHEAST :
					return 3;
				case Direction.SOUTHWEST :
					return 7;
				case Direction.SOUTHEAST:
					return 9;
				default :
					return 5;
			}
		}

		/// <summary>
		/// Returns a delimited string representing the given directions
		/// </summary>
		/// <param name="directions"></param>
		/// <returns></returns>
		public static string ConvertDirectionsToString(IEnumerable<Direction> directions, char[] delimiter)
		{
			directions = directions.ToArray();
			if (!directions.Any()) return string.Empty;

			var sb = new StringBuilder();
			var delimiterString = new string(delimiter);

			foreach(var d in directions)
			{
				sb.Append("{0}{1}".F(d.ToString().ToLowerInvariant(), delimiterString));
			}

			// trim off the trailing delimiter
			sb.Remove(sb.ToString().LastIndexOf(delimiterString), 1);

			return sb.ToString();
		}

		/// <summary>
		/// Returns a list of directions based on a delimited input string
		/// </summary>
		/// <param name="directionString"></param>
		/// <param name="delimiter"></param>
		/// <returns></returns>
		public static List<Direction> ParseOrientationString(string directionString, char[] delimiter)
		{	
			var retVal = new List<Direction>();

			if (string.IsNullOrWhiteSpace(directionString))
			{
				retVal.Add(Direction.NONE);
				return retVal;
			}

			directionString = directionString.Replace(" ", "");
			var directions = directionString.Split(delimiter);

			retVal.AddRange(directions.Select(d => ConvertStringToDirection(d)));

			return retVal;
		}

		/// <summary>
		/// Returns the opposite direction for a given direction
		/// </summary>
		/// <param name="direction"></param>
		/// <returns></returns>
		public static Direction GetOppositeDirection(Direction direction)
		{
			switch (direction)
			{
				case Direction.NORTH :
					return Direction.SOUTH;
				case Direction.SOUTH :
					return Direction.NORTH;
				case Direction.EAST :
					return Direction.WEST;
				case Direction.WEST :
					return Direction.EAST;
				case Direction.NORTHWEST :
					return Direction.SOUTHEAST;
				case Direction.SOUTHEAST :
					return Direction.NORTHWEST;
				case Direction.NORTHEAST :
					return Direction.SOUTHWEST;
				case Direction.SOUTHWEST :
					return Direction.NORTHEAST;
				default:
					return Direction.NONE;
			}
		}

		public static IEnumerable<Direction> GetAllDirections()
		{
			var retVal = new List<Direction>
				{
					Direction.NORTH,
					Direction.SOUTH,
					Direction.EAST,
					Direction.WEST,
					Direction.NORTHWEST,
					Direction.NORTHEAST,
					Direction.SOUTHWEST,
					Direction.SOUTHEAST
				};


			return retVal;
		}

	}

	public static class NodeEdges
	{
		public static NodeEdge NorthNodeEdge = new NodeEdge {Direction = Direction.NORTH, SubNodeIDs = new[] {1, 2, 3}};
		public static NodeEdge SouthNodeEdge = new NodeEdge {Direction = Direction.SOUTH, SubNodeIDs = new[] {7, 8, 9}};
		public static NodeEdge EastNodeEdge = new NodeEdge {Direction = Direction.EAST, SubNodeIDs = new[] {3, 6, 9}};
		public static NodeEdge WestNodeEdge = new NodeEdge {Direction = Direction.WEST,SubNodeIDs = new[] {1, 4, 7}};
		public static NodeEdge NorthWestNodeEdge = new NodeEdge {Direction = Direction.NORTHWEST, SubNodeIDs = new[] {9}};
		public static NodeEdge NorthEastNodeEdge = new NodeEdge {Direction = Direction.NORTHEAST, SubNodeIDs = new[] {7}};
		public static NodeEdge SouthWestNodeEdge = new NodeEdge {Direction = Direction.SOUTHWEST, SubNodeIDs = new[] {3}};
		public static NodeEdge SouthEastNodeEdge = new NodeEdge {Direction = Direction.SOUTHEAST, SubNodeIDs = new[] {1}};
	}

	public class NodeEdge
	{
		public Direction Direction { get; set; }
		public IEnumerable<int> SubNodeIDs { get; set; }
	}

	public class MoveInDirectionResult
	{
		public bool HasMovementOverrideInDirection { get; set; }
		public bool CanMoveInDirection { get; set; }
	}

}
