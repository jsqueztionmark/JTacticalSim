using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Drawing;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.Game;
using JTacticalSim.API.Service;
using JTacticalSim.Utility;
using JTacticalSim.API.InfoObjects;


namespace JTacticalSim.API.Component
{
	public static class Extension
	{
		/// <summary>
		/// Returns all units at a given tile
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public static IEnumerable<IUnit> GetAllUnits(this IBoardComponent o)
		{
			var factions = o.TheGame().JTSServices.GameService.GetAllFactions();
			return o.TheGame().JTSServices.UnitService.GetUnitsAt(o.Location, factions);
		}

		/// <summary>
		/// Ensures that all demographics are unique by class.
		/// Zips orientations for non-unique demographics into one
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		public static IEnumerable<IDemographic> EnsureUnique(this IEnumerable<IDemographic> demographics)
		{
			var zipUs = demographics.GroupBy(d => d.ID).Select(d => new {Demographics = d}).ToList();
			var zipped = new List<IDemographic>();
	
			zipUs.ForEach(item =>
				{
					if (item.Demographics.Count() == 1)
					{
						zipped.Add(item.Demographics.SingleOrDefault());
						return;
					}
					var orientation = item.Demographics.SelectMany(d => d.Orientation).Distinct().ToList();
					var keep = item.Demographics.First();
					keep.Orientation = orientation;
					zipped.Add(keep);
				});

			return zipped;
		}

		public static bool HasMaxUnits(this ITile o, IFaction faction)
		{
			var maxUnits = o.TheGame().GameBoard.DefaultAttributes.CellMaxUnits;
			return o.TheGame().JTSServices.RulesService.TileHasMaxUnitsForFaction(o, faction, maxUnits).Result;
		}

		public static bool WillExceedMaxUnits(this ITile o, IEnumerable<IUnit> movingUnits)
		{
			var maxUnits = o.TheGame().GameBoard.DefaultAttributes.CellMaxUnits;
			return o.TheGame().JTSServices.RulesService.TileWillExceedMaxUnitsForFaction(o, movingUnits, maxUnits).Result;
		}

		public static bool IsBeingTransported(this IMoveableComponent o)
		{
			return o.TheGame().JTSServices.RulesService.ComponentIsBeingTransported(o).Result;
		}

		public static bool IsVisible(this IMoveableComponent o)
		{
			return o.TheGame().JTSServices.RulesService.ComponentIsVisible(o).Result;
		}

		public static IUnitStack GetCurrentStack(this IMoveableComponent o)
		{
			return o.TheGame().JTSServices.TileService.GetCurrentStack(o);
		}

		public static string GetTextDisplayForZoom(this ITextDisplayable o, ZoomLevel zoomLevel)
		{
			switch (zoomLevel)
			{
				case ZoomLevel.ONE:
					{
						return o.TextDisplayZ1;
					}
				case ZoomLevel.TWO:
					{
						return o.TextDisplayZ2;
					}
				case ZoomLevel.THREE:
					{
						return o.TextDisplayZ3;
					}
				case ZoomLevel.FOUR:
					{
						return o.TextDisplayZ4;
					}
				default:
					{
						return string.Empty;
					}
			}
		}

		public static List<IPathableObject> PruneMapForPathFinding(this List<IPathableObject> map, INode target, INode source, int distance)
		{
			if (target.Location.Y > source.Location.Y)
				map.RemoveAll(n => n.Location.Y < (target.Location.Y - distance));
			if (target.Location.Y < source.Location.Y)
				map.RemoveAll(n => n.Location.Y > (target.Location.Y + distance));
			if (target.Location.X > source.Location.X)
				map.RemoveAll(n => n.Location.X < (target.Location.X - distance));
			if (target.Location.X < source.Location.X)
				map.RemoveAll(n => n.Location.X > (target.Location.X + distance));

			return map;
		}


		/// <summary>
		/// Converts an object of type IPathableObject to another type of IPathableObject
		/// retaining only the IPathableObject members
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="o"></param>
		/// <returns></returns>
		public static T ToPathableObject<T>(this IPathableObject o)
			where T : IPathableObject, new()
		{
			var retVal = new T();			
			
			Type sourceType = o.GetType();
			Type destinationType = retVal.GetType();

			foreach (var p in sourceType.GetProperties())
			{
				var destinationP = destinationType.GetProperty(p.Name);
				if (destinationP != null && destinationP.CanWrite)
				{
					destinationP.SetValue(retVal, p.GetValue(o, null), null);
				}
			}
			
			return retVal;
		}

		public static void DisplayInfo(this List<IUnit> units)
		{
		    units.First().TheGame().Renderer.DisplayUnits(units);
		}	

	// Utility

		public static bool NullOrNone(this List<IBaseComponent> o)
		{
			return (o == null || o.Count == 0);
		}

		public static bool ExistsInContext(this IBaseComponent o)
		{
			return o.TheGame().JTSServices.GenericComponentService.ExistsInContext(o);
		}


		// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


		/// <summary>
		/// Converts base data from one IResult to another
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <typeparam name="TObject"></typeparam>
		/// <typeparam name="TResult2"></typeparam>
		/// <typeparam name="TObject2"></typeparam>
		/// <param name="result1"></param>
		/// <param name="result2"></param>
		public static void ConvertResultData(this IBaseResult result1, IBaseResult result2)
		{
			result1.Messages = result2.Messages;
			result1.Status = result2.Status;
			result1.ex = result2.ex;
		}

		/// <summary>
		/// Aggragates statuses, messages and exceptions into a single result
		/// </summary>
		/// <param name="results"></param>
		/// <returns></returns>
		public static IResult<object, Exception> AggragateResultsData(this IEnumerable<IBaseResult> results)
		{
			var retVal = new OperationResult<object, Exception> {Status = ResultStatus.SUCCESS};

			foreach (var r in results)
			{
				retVal.Messages.Add(r.Message);
				if (r.Status == ResultStatus.EXCEPTION)
					retVal.FailedObjects.Add(r.ex);
			}

			if (results.Any(r => r.Status == ResultStatus.FAILURE || r.Status == ResultStatus.SOME_FAILURE))
				retVal.Status = ResultStatus.FAILURE;

			if (results.Any(r => r.Status == ResultStatus.EXCEPTION))
				retVal.Status = ResultStatus.EXCEPTION;

			return retVal;
		}

		/// <summary>
		/// Returns a summary of all messages in a results collection
		/// </summary>
		/// <param name="results"></param>
		/// <returns></returns>
		public static string MessageSummary(this IEnumerable<IBaseResult> results)
		{
			var sb = new StringBuilder();
			foreach (var result in results)
			{
				sb.AppendLine(result.Message);
			}
			return sb.ToString();
		}
	}
}
