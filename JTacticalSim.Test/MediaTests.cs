using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Media.Sound;
using JTacticalSim.Media.Sound;
using Moq;
using NUnit.Framework;

namespace JTacticalSim.Test
{
	/// <summary>
	/// Tests for direct media handling
	/// </summary>
	[TestFixture]
	public class MediaTests : BaseTest
	{
		[Test]
		public void SoundHandlerFactory_Returns_Handler()
		{
			var handler = SoundHandlerFactory.Instance.GetSoundHandler(SoundSourceType.WAV);
			Assert.IsNotNull(handler);
			Assert.IsInstanceOf(typeof(ISoundHandler), handler);
		}

		[TestCase("Boat_Move", ResultStatus.SUCCESS)]
		[TestCase("Bad_File_Name", ResultStatus.EXCEPTION)]
		public void SoundHandler_GetSound_Returns_FilesStream_Success_Fail(string fileName, ResultStatus status)
		{
			var handler = SoundHandlerFactory.Instance.GetSoundHandler(SoundSourceType.WAV);
			var result = handler.GetSound(fileName);
			Assert.AreEqual(result.Status, status);
		}
	}
}
