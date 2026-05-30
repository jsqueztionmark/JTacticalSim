using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JTacticalSim.GUI.Render;

public static class PrimitiveDrawSprites
{
    public static void DrawLine(SpriteBatch sb, Texture2D px, Vector2 from, Vector2 to, Color color, int thickness)
    {
        var diff = to - from;
        float len = diff.Length();
        if (len < 1f) return;
        float angle = MathF.Atan2(diff.Y, diff.X);
        sb.Draw(px, from, null, color, angle, new Vector2(0, 0.5f), new Vector2(len, thickness), SpriteEffects.None, 0f);
    }

    public static void DrawDashedLine(SpriteBatch sb, Texture2D px, Vector2 from, Vector2 to,
                                      Color color, int thickness, int dashLen, int gapLen)
    {
        var diff = to - from;
        float totalLen = diff.Length();
        if (totalLen < 1f) return;

        float angle   = MathF.Atan2(diff.Y, diff.X);
        var   dir     = Vector2.Normalize(diff);
        float pos     = 0f;
        bool  drawing = true;

        while (pos < totalLen)
        {
            float segLen = Math.Min(drawing ? dashLen : gapLen, totalLen - pos);
            if (drawing && segLen > 0)
                sb.Draw(px, from + dir * pos, null, color, angle, new Vector2(0, 0.5f),
                        new Vector2(segLen, thickness), SpriteEffects.None, 0f);
            pos += segLen;
            drawing = !drawing;
        }
    }

    public static Color ConsoleColorToXna(System.ConsoleColor cc) => cc switch
    {
        System.ConsoleColor.Black       => new Color(20,  20,  20),
        System.ConsoleColor.DarkBlue    => new Color(0,   0,   139),
        System.ConsoleColor.DarkGreen   => new Color(0,   100, 0),
        System.ConsoleColor.DarkCyan    => new Color(0,   139, 139),
        System.ConsoleColor.DarkRed     => new Color(139, 0,   0),
        System.ConsoleColor.DarkMagenta => new Color(139, 0,   139),
        System.ConsoleColor.DarkYellow  => new Color(180, 160, 0),
        System.ConsoleColor.Gray        => new Color(169, 169, 169),
        System.ConsoleColor.DarkGray    => new Color(105, 105, 105),
        System.ConsoleColor.Blue        => new Color(50,  80,  180),
        System.ConsoleColor.Green       => new Color(0,   180, 0),
        System.ConsoleColor.Cyan        => new Color(0,   200, 200),
        System.ConsoleColor.Red         => new Color(220, 50,  50),
        System.ConsoleColor.Magenta     => new Color(200, 0,   200),
        System.ConsoleColor.Yellow      => new Color(220, 220, 50),
        System.ConsoleColor.White       => new Color(240, 240, 240),
        _                               => new Color(120, 120, 120),
    };
}
