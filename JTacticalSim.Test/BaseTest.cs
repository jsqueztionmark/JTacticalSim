using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using JTacticalSim.API.Component;
using JTacticalSim.GameState;
using NUnit.Framework;
using JTacticalSim.API;
using JTacticalSim.API.Game;
using JTacticalSim.API.Cache;
using JTacticalSim.Service;
using JTacticalSim.LINQPad.Plugins;
using JTacticalSim.Cache;

namespace JTacticalSim.Test
{
	public class BaseTest
	{
		protected IGame TheGame;

		[SetUp]
		public void Init()
		{
			TheGame = ComponentUtilities.CreateNewGameInstance(ConfigurationManager.AppSettings["TEST_GAME"]);
		}

		[TearDown]
		public void Dispose()
		{
			
		}

		protected IEnumerable<IPathableObject> GetBoardMap()
		{
			return TheGame.JTSServices.NodeService.GetAllNodes();
		}
	}
}
