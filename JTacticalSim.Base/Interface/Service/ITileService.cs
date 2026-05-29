using System;
using JTacticalSim.API.Component;
using JTacticalSim.API.Data;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.API.Service;
using JTacticalSim.Utility;

namespace JTacticalSim.API.Service
{
	public interface ITileService
	{
		IEnumerable<ITile> GetTiles();

		IResult<ITile, ITile> SaveTiles(List<ITile> tiles);

		IResult<ITile, ITile> RemoveTiles(List<ITile> tiles);

		IResult<ITile, ITile> UpdateTiles(List<ITile> tiles);

		/// <summary>
		/// Applies demographic affects when a tile location is nuked. 
		/// Removes any demographics that are not geography classes. Adds the nukeDemographic to the tile.
		/// </summary>
		/// <returns></returns>
		IResult<ITile, ITile> NukeAffectTile(ITile tile, IDemographic nukeDemographic);

		ICoordinate CreateCoordinate(int X, int Y, int Z);

		ICoordinate CreateCoordinateForDirection(ICoordinate location, Direction direction, int distance);

		/// <summary>
		/// Returns a collection of directions [Orientation] Allowable for a demographic class
		/// Checks if the provided demographic class is already represented by any demographics existing on the tile 
		/// </summary>
		/// <param name="baseDemographic"></param>
		/// <param name="newDemographicClass"></param>
		/// <returns></returns>
		IEnumerable<Direction> GetOrientationAllowableForDemographicClassByTile(ITile tile, IDemographicClass demographicClass);


		IUnitStack GetCurrentStack(IMoveableComponent component);
	}
}
