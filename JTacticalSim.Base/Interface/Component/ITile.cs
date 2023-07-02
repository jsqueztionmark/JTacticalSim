using System.Collections.Generic;
using System.Drawing;
using JTacticalSim.API.Game;
using JTacticalSim.API.InfoObjects;
using JTacticalSim.Utility;

namespace JTacticalSim.API.Component
{
	public interface ITile : IBoardComponent, IInfoDisplayable
	{
		List<IDemographic> BaseGeography { get; }
		List<IDemographic> Terrain { get; }
		List<IDemographic> AllGeography { get; }
		List<IDemographic> Infrastructure { get; }
		List<IDemographic> Flora { get; }
		List<IDemographic> Population { get; }

		double NetStealthAdjustment { get; set; }
		double NetAttackAdjustment { get; set; }
		double NetDefenceAdjustment { get; set; }
		int NetMovementAdjustment { get; set; }

		TileConsoleRenderHelper ConsoleRenderHelper { get; }

		/// <summary>
		/// Tile, map level override giving a location a higher net strategic value
		/// </summary>
		bool IsPrimeTarget { get; set; }
		bool IsDestroyed { get; set; }
		bool IsGeographicChokePoint { get; set; }
		int VictoryPoints { get; set; }		
		int TotalUnitCount { get; }

		void Render(int zoomLevel);

		/// <summary>
		/// Returns the count of stacks for this tile that have any visible components
		/// </summary>
		/// <returns></returns>
		int VisibleStackCount();

		List<IDemographic> GetAllDemographics();
		List<IDemographic> GetAllHybridDemographics(); 
			
		/// <summary>
		/// Returns the component stack for a given country
		/// </summary>
		/// <param name="country"></param>
		/// <returns></returns>
		IUnitStack GetCountryComponentStack(ICountry country);

		/// <summary>
		/// Returns all component stacks at this tile
		/// </summary>
		/// <returns></returns>
		List<IUnitStack> GetAllComponentStacks();

		/// <summary>
		/// Attempts to populate the appropriate component stacks with the given components
		/// </summary>
		/// <param name="components"></param>
		/// <returns></returns>
		IResult<IUnit, IUnit> AddComponentsToStacks(IEnumerable<IUnit> components);

		/// <summary>
		/// Attempts to remove the given components from the appropriate unit stacks
		/// </summary>
		/// <param name="components"></param>
		/// <returns></returns>
		IResult<IUnit, IUnit> RemoveComponentsFromStacks(IEnumerable<IUnit> components);

		void SetTileName();

		IResult<IDemographic, IDemographic> AddDemographic(IDemographic demographic);
		IResult<IDemographic, IDemographic> RemoveDemographic(IDemographic demographic);
		IResult<IDemographic, IDemographic> RemoveDirectionFromDemographicOrientation(IDemographic demographic, Direction direction);

		/// <summary>
		/// Returns all all demographic instance names in a list
		/// </summary>
		/// <returns></returns>
		List<string> GetAllDemographicNames();

		/// <summary>
		/// Re-populates the component stacks from the data store
		/// </summary>
		/// <returns></returns>
		IResult<IUnit, IUnit> RefreshComponentStacks();

		/// <summary>
		/// Resets the calculated tile info including net modifier values
		/// </summary>
		/// <returns></returns>
		IResult<ITile, ITile> ReCalculateTileInfo();

		/// <summary>
		/// Returns an info object categorizing the strategy assessments of the tile.
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		StrategicAssessmentInfo GetStrategicValues();

		/// <summary>
		/// Returns the average strategy assessment rating of the aggragated assessment ratings of the tile.
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		StrategicAssessmentRating GetNetStrategicValue();

		/// <summary>
		/// Determines whether infrastructure of the given class can currently be built on this tile
		/// </summary>
		/// <param name="demographicClass"></param>
		/// <returns></returns>
		bool CanSupportInfrastructureBuilding(IDemographic demographic);

		///// <summary>
		///// Returns the specific texture image for the tile
		///// </summary>
		///// <param name="o"></param>
		///// <returns></returns>
		//Image GetDemographicTextureImage();

		///// <summary>
		///// Returns the default texture image for the first BaseGeography of the tile
		///// </summary>
		///// <param name="o"></param>
		///// <returns></returns>
		//Image GetDefaultDemographicTextureImage();

		void ResetComponentStackDisplayOrder();
	}
}
