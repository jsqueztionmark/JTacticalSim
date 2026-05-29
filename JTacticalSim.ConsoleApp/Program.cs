using JTacticalSim.API;

namespace JTacticalSim.ConsoleApp
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			var ctx = GameContext.Instance;
			ctx.InitializeGame(true);
			ctx.GameLoop(0.0);
		}
	}
}
