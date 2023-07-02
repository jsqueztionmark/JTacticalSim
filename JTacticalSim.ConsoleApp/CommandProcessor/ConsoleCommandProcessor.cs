using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Runtime.InteropServices;
using JTacticalSim.API;
using JTacticalSim.API.Game;
using JTacticalSim.API.Component;
using System.IO;
using System.Globalization;
using System.Text;
using JTacticalSim.ConsoleApp.CommandLineUtil;

namespace JTacticalSim.ConsoleApp
{
	public class ConsoleCommandProcessor : BaseGameObject, ICommandProcessor
	{
		private ICommandInterface _commandInterface { get; set;}

		public ConsoleCommandProcessor()
			: base(GameObjectType.HANDLER)
		{
			_commandInterface = new CommandInterface(new ConsoleCommandHandler());

			// Allows for LINQPad to run this with no console handle
			try
			{
				var winWidth = Convert.ToInt32(ConfigurationManager.AppSettings["window_width"]);
				var winHeight = Convert.ToInt32(ConfigurationManager.AppSettings["window_height"]);

				Console.OutputEncoding = Encoding.Unicode;
				Console.Title = "JTacticalSim";
				Console.SetWindowSize(winWidth, winHeight);
                Console.SetBufferSize(winWidth, winHeight);
				Console.CursorVisible = false;
				ConsoleUtils.CenterConsole();
			}
			catch
			{ }
			
		}

#region Interface Implementation

		[Obsolete("Errors are handled through the renderer")]
		public void SetInputError<TResult, TObject>(IResult<TResult, TObject> result)
		{
			_commandInterface.SetInputError(result);
		}

		public void ProcessInput(StateType state)
		{
			_commandInterface.HandleInput(state);	
		}

		public void ProcessCommand(Commands command)
		{
			_commandInterface.RunCommand(command);
		}

		public void CancelCommand()
		{
			System.Console.Out.Flush();
			_commandInterface.GetCommand();
		}


#endregion


	}
}
