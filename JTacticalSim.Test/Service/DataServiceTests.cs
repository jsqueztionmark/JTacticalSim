using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using JTacticalSim.API.Component;
using JTacticalSim.API.AI;
using JTacticalSim.API.Game;
using JTacticalSim.API;
using JTacticalSim.Component.AI;
using JTacticalSim.Component.World;
using NUnit.Framework;
using JTacticalSim.LINQPad.Plugins;

namespace JTacticalSim.Test.Service
{
	/// <summary>
	/// Tests for the Data Service
	/// </summary>
	[TestFixture]
	public class DataServiceTests : BaseTest
	{
		// Lookups
		[TestCase(0, 4, 3)]
		[TestCase(1, 1, 4)]
		public void Lookup_Unit_Task_Step_Order_For_Mission(int Mission, int unitTask, int stepOrder)
		{
			var missionType = TheGame.JTSServices.GenericComponentService.GetByID<MissionType>(Mission);
			var taskType = TheGame.JTSServices.GenericComponentService.GetByID<UnitTaskType>(unitTask);

			var result = TheGame.JTSServices.DataService.LookupUnitTaskTypeStepOrderForMissionType(taskType, missionType);

			Assert.AreEqual(result.Result, stepOrder);
		}
	}
}
