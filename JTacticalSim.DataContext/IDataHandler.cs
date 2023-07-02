using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JTacticalSim.API.Component;

namespace JTacticalSim.DataContext
{
	public interface IDataHandler
	{
		IResult<string, string> LoadSavedGameData(string fileDirectory, bool IsScenario);
		IResult<string, string> LoadData(string fileDirectory, IComponentSet componentSet, bool IsScenario);
		IResult<IGameFileCopyable, IGameFileCopyable> SaveData(IGameFileCopyable currentGame);

		/// <summary>
		/// Creates a new game/scenario from a base context : 
		/// Accepts either a scenario or current game to create from
		/// </summary>
		/// <param name="current"></param>
		/// <param name="newGame"></param>
		/// <returns></returns>
		IResult<IGameFileCopyable, IGameFileCopyable> SaveDataAs(IGameFileCopyable current, IGameFileCopyable newGame);

		/// <summary>
		/// Removes the files for a saved game
		/// </summary>
		/// <param name="delGame"></param>
		/// <returns></returns>
		IResult<IGameFileCopyable, IGameFileCopyable> RemoveSavedGameData(IGameFileCopyable delGame);

		void Reset();
	}
}
