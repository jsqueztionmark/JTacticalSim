namespace JTacticalSim.GUI;

internal class Program
{
    public static void Main(string[] args)
    {
        var ctx = GameContext.Instance;
        ctx.InitializeGame(true);
        using var host = new MonoGameHost(ctx);
        host.Run();
    }
}
