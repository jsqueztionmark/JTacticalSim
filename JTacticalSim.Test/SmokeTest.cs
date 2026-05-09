using NUnit.Framework;

namespace JTacticalSim.Test
{
	[TestFixture]
	public class SmokeTest
	{
		[Test]
		public void True_Is_True() => Assert.That(true, Is.True);
	}
}
