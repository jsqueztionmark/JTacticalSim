using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTacticalSim.API.Game;
using JTacticalSim.Component.Data;

namespace JTacticalSim.ConsoleApp
{
	public static class InputValidation
	{
		public static SimpleValidationResult<bool> IsValidUnitName(string unitName, IGame theGame)
		{
			//Restrict to 30 characters to not f-up the board
			if ((unitName.Length > 30 || unitName.Length == 0))
				return new SimpleValidationResult<bool> { Result = false, Message = "Unit Name must be less than 30 characters" };
			if (!theGame.JTSServices.RulesService.UnitNameIsUnique(unitName).Result)
				return new SimpleValidationResult<bool> { Result = false, Message = "Unit Name already in use" };

			return new SimpleValidationResult<bool> { Result = true, Message = "" };
		}

		public static SimpleValidationResult<bool> IsValidGameTitle(string gameTitle, IGame theGame)
		{
			if (string.IsNullOrWhiteSpace(gameTitle))
				return new SimpleValidationResult<bool> { Result = false, Message = "Game title can not be null" };

			var sResult = theGame.JTSServices.RulesService.NameIsValid<SavedGame>(gameTitle);

			if (!sResult.Result)
				return new SimpleValidationResult<bool> { Result = false, Message = sResult.Message };
		
			return new SimpleValidationResult<bool> { Result = true, Message = "" };
		}
	}

	public struct SimpleValidationResult<T>
	{
		public T Result { get; set; }
		public string Message { get; set; }
	}
}
