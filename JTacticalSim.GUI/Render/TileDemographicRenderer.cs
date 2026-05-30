using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JTacticalSim.API.Component;
using JTacticalSim.Utility;

namespace JTacticalSim.GUI.Render;

public static class TileDemographicRenderer
{
    private static readonly Color ColWater = new(30, 60, 140);

    // ── Infrastructure overlay ────────────────────────────────────────────────

    public static void DrawInfrastructureOverlay(ITile tile, SpriteBatch sb, Texture2D px, int x, int y, int w, int h)
    {
        var center = new Vector2(x + w / 2f, y + h / 2f);

        // Creeks first — drawn under roads; offset laterally when co-located
        var geo = tile?.AllGeography;
        if (geo != null)
        {
            foreach (var demo in geo)
            {
                if (demo?.Orientation == null || !demo.Orientation.Any()) continue;
                if (!demo.IsDemographicClass("Creek")) continue;
                foreach (var dir in demo.Orientation)
                {
                    var perp = CreekPerpendicularOffset(dir, 4f);
                    var edge = DirectionToEdgePoint(dir, x, y, w, h);
                    PrimitiveDrawSprites.DrawLine(sb, px, center + perp, edge + perp, ColWater, 2);
                }
            }
        }

        // Roads, bridges, dams, tracks — drawn on top of creeks
        var infra = tile?.Infrastructure;
        if (infra == null || infra.Count == 0) return;

        foreach (var demo in infra)
        {
            if (demo?.Orientation == null || !demo.Orientation.Any()) continue;

            bool isRoad   = demo.IsDemographicClass("Road");
            bool isBridge = demo.IsDemographicClass("Bridge");
            bool isDam    = demo.IsDemographicClass("Dam");
            bool isTracks = demo.IsDemographicClass("TrainTrack");

            if (!isRoad && !isBridge && !isDam && !isTracks) continue;

            Color lineColor = isRoad   ? Color.Black
                            : isBridge ? new Color(90, 90, 90)
                            : isDam    ? new Color(100, 130, 160)
                            :            new Color(80, 55, 35);
            int thickness = isRoad || isBridge ? 5 : 2;

            foreach (var dir in demo.Orientation)
            {
                var edge = DirectionToEdgePoint(dir, x, y, w, h);
                PrimitiveDrawSprites.DrawLine(sb, px, center, edge, lineColor, thickness);
                if (isRoad)
                    PrimitiveDrawSprites.DrawDashedLine(sb, px, center, edge, Color.White, 1, 6, 5);
            }
        }
    }

    private static Vector2 CreekPerpendicularOffset(Direction dir, float amount) => dir switch
    {
        Direction.NORTH or Direction.SOUTH             => new Vector2(amount, 0),
        Direction.EAST  or Direction.WEST              => new Vector2(0, amount),
        Direction.NORTHWEST or Direction.SOUTHEAST     => new Vector2(amount * 0.7f,  amount * 0.7f),
        Direction.NORTHEAST or Direction.SOUTHWEST     => new Vector2(amount * 0.7f, -amount * 0.7f),
        _                                              => Vector2.Zero,
    };

    // ── Demographic sprites ───────────────────────────────────────────────────

    public static void DrawDemographicSprites(ITile tile, SpriteBatch sb, Texture2D px, int x, int y, int w, int h, int zoomLevel)
    {
        var rh = tile?.ConsoleRenderHelper;
        if (rh == null) return;

        if (rh.IsNuclearWasteland && zoomLevel >= 2)
            DrawNuclearSymbol(sb, px, x, y, w, h);

        if (rh.HasMountain && zoomLevel >= 3)
            DrawMountainPeak(sb, px, x, y, w, h);

        if (rh.HasMilitaryBase && zoomLevel >= 2)
            DrawMilitaryBase(sb, px, x, y, w, h);

        if (rh.HasCommandPost && zoomLevel >= 2)
            DrawCommandPost(sb, px, x, y, w, h);

        if (rh.HasAirports && zoomLevel >= 3)
        {
            var demo = tile.Infrastructure.SingleOrDefault(d => d.IsDemographicClass("Airport"));
            if (demo != null) DrawAirport(sb, px, x, y, w, h, demo.Orientation);
        }

        if (rh.HasCities)
            DrawUrban(sb, px, x, y, w, h);

        if (rh.HasIndustrial)
            DrawIndustrial(sb, px, x, y, w, h);

        if (rh.HasTown && zoomLevel >= 3)
        {
            var demo = tile.Infrastructure.SingleOrDefault(d => d.IsDemographicClass("Town"));
            if (demo != null) DrawTown(sb, px, x, y, w, h, demo.Orientation);
        }
    }

    private static void DrawMountainPeak(SpriteBatch sb, Texture2D px, int x, int y, int w, int h)
    {
        int cx = x + w / 2, cy = y + h / 2;
        int u    = Math.Max(3, w / 15);
        int core = Math.Max(8, w / 6);

        var ice  = new Color(155, 168, 178);
        var snow = new Color(240, 245, 255);

        sb.Draw(px, new Rectangle(cx - u*2, cy - u*2, u*4, u*4), ice);
        sb.Draw(px, new Rectangle(cx - u*4, cy - u,   u*2, u*2), ice);
        sb.Draw(px, new Rectangle(cx + u*2, cy - u,   u,   u*2), ice);
        sb.Draw(px, new Rectangle(cx - u,   cy - u*4, u*2, u*2), ice);
        sb.Draw(px, new Rectangle(cx - u,   cy + u*2, u*3, u*2), ice);
        sb.Draw(px, new Rectangle(cx - u*3, cy - u*3, u*2, u),   ice);
        sb.Draw(px, new Rectangle(cx - u*2, cy + u*2, u,   u),   ice);
        sb.Draw(px, new Rectangle(cx + u*2, cy + u,   u,   u),   ice);
        sb.Draw(px, new Rectangle(cx - core / 2, cy - core / 2, core, core), snow);
    }

    private static void DrawMilitaryBase(SpriteBatch sb, Texture2D px, int x, int y, int w, int h)
    {
        const int b = 10;
        int m  = w / 8;
        int bx = x + m, by = y + m, bw = w - m * 2, bh = h - m * 2;

        var olive  = new Color(95,  85,  25);
        var border = new Color(130, 125, 115);
        var tank   = new Color(25,  22,  12);

        sb.Draw(px, new Rectangle(bx,          by,          bw, bh), olive);
        sb.Draw(px, new Rectangle(bx,          by,          bw, b),  border);
        sb.Draw(px, new Rectangle(bx,          by + bh - b, bw, b),  border);
        sb.Draw(px, new Rectangle(bx,          by,          b,  bh), border);
        sb.Draw(px, new Rectangle(bx + bw - b, by,          b,  bh), border);

        int ibx = bx + b, iby = by + b, ibw = bw - b * 2, ibh = bh - b * 2;
        int tw = Math.Max(8, ibw / 4), th = Math.Max(4, ibh / 5);
        int ty = iby + ibh / 2 - th / 2;
        int tx1 = ibx + ibw / 4 - tw / 2, tx2 = ibx + ibw * 3 / 4 - tw / 2;
        int turH = Math.Max(2, th / 2), turW = Math.Max(3, tw / 3);

        sb.Draw(px, new Rectangle(tx1, ty, tw, th), tank);
        sb.Draw(px, new Rectangle(tx1 + tw / 4, ty - turH, turW, turH), tank);
        sb.Draw(px, new Rectangle(tx2, ty, tw, th), tank);
        sb.Draw(px, new Rectangle(tx2 + tw / 4, ty - turH, turW, turH), tank);
    }

    private static void DrawCommandPost(SpriteBatch sb, Texture2D px, int x, int y, int w, int h)
    {
        const int b = 10;
        int m  = w / 8;
        int bx = x + m, by = y + m, bw = w - m * 2, bh = h - m * 2;

        var olive  = new Color(95,  85,  25);
        var border = new Color(130, 125, 115);

        sb.Draw(px, new Rectangle(bx,          by,          bw, bh), olive);
        sb.Draw(px, new Rectangle(bx,          by,          bw, b),  border);
        sb.Draw(px, new Rectangle(bx,          by + bh - b, bw, b),  border);
        sb.Draw(px, new Rectangle(bx,          by,          b,  bh), border);
        sb.Draw(px, new Rectangle(bx + bw - b, by,          b,  bh), border);

        int cx = bx + bw / 2, cy = by + bh / 2;
        int armL = Math.Max(4, bw / 4), armT = Math.Max(2, bh / 8);
        sb.Draw(px, new Rectangle(cx - armL,     cy - armT / 2, armL * 2, armT), Color.White);
        sb.Draw(px, new Rectangle(cx - armT / 2, cy - armL,     armT, armL * 2), Color.White);
    }

    private static void DrawAirport(SpriteBatch sb, Texture2D px, int x, int y, int w, int h, List<Direction> orientation)
    {
        bool isNS = orientation.Any(d => d == Direction.NORTH || d == Direction.SOUTH);
        int cx = x + w / 2, cy = y + h / 2;
        int rLen = w - 8, rWid = w / 4;
        int rw = isNS ? rWid : rLen, rh = isNS ? rLen : rWid;

        var asphalt = new Color(50, 50, 55);
        var marking = new Color(215, 210, 185);

        sb.Draw(px, new Rectangle(cx - rw / 2, cy - rh / 2, rw, rh), asphalt);

        if (isNS)
        {
            sb.Draw(px, new Rectangle(cx - rw / 2, cy - rh / 2 + 2, rw, 2), marking);
            sb.Draw(px, new Rectangle(cx - rw / 2, cy + rh / 2 - 4, rw, 2), marking);
            PrimitiveDrawSprites.DrawDashedLine(sb, px, new Vector2(cx, cy - rh / 2), new Vector2(cx, cy + rh / 2), marking, 1, 5, 4);
        }
        else
        {
            sb.Draw(px, new Rectangle(cx - rw / 2 + 2, cy - rh / 2, 2, rh), marking);
            sb.Draw(px, new Rectangle(cx + rw / 2 - 4, cy - rh / 2, 2, rh), marking);
            PrimitiveDrawSprites.DrawDashedLine(sb, px, new Vector2(cx - rw / 2, cy), new Vector2(cx + rw / 2, cy), marking, 1, 5, 4);
        }
    }

    private static void DrawUrban(SpriteBatch sb, Texture2D px, int x, int y, int w, int h)
    {
        var building = new Color(72, 68, 62);
        var accent   = new Color(135, 42, 42);

        int baseY = y + h - h / 5;
        int[] widths  = { w / 9,  w / 11, w / 8  };
        int[] heights = { h / 3,  h * 2 / 5, h / 4 };
        int[] offsets = { w / 5,  w / 2,  w * 3 / 4 };

        for (int i = 0; i < 3; i++)
        {
            int bx = x + offsets[i] - widths[i] / 2;
            sb.Draw(px, new Rectangle(bx, baseY - heights[i], widths[i], heights[i]), building);
            sb.Draw(px, new Rectangle(bx, baseY - 3,          widths[i], 2),          accent);
        }
    }

    private static void DrawIndustrial(SpriteBatch sb, Texture2D px, int x, int y, int w, int h)
    {
        DrawUrban(sb, px, x, y, w, h);

        int cx    = x + w / 2;
        int ch    = h / 5;
        int cw    = Math.Max(3, w / 14);
        int baseY = y + h - h / 5;
        sb.Draw(px, new Rectangle(cx - cw / 2, baseY - h / 3 - ch, cw, ch), new Color(90, 88, 84));
    }

    private static void DrawTown(SpriteBatch sb, Texture2D px, int x, int y, int w, int h, List<Direction> orientation)
    {
        _ = orientation;

        int hw = Math.Max(8, w / 7), hh = Math.Max(5, h / 9), rh = Math.Max(3, hh / 2);
        var wall  = new Color(190, 172, 130);
        var roofA = new Color(145, 65, 52);
        var roofB = new Color(120, 85, 65);

        void House(int hx, int hy, Color rc)
        {
            sb.Draw(px, new Rectangle(hx,        hy,        hw,      hh),    wall);
            sb.Draw(px, new Rectangle(hx,        hy - rh,   hw,      rh / 2), rc);
            sb.Draw(px, new Rectangle(hx + hw/5, hy - rh/2, hw*3/5, rh/2),   rc);
        }

        House(x + w/5   - hw/2, y + h/3   - hh/2, roofA);
        House(x + w*3/5 - hw/2, y + h/4   - hh/2, roofB);
        House(x + w/4   - hw/2, y + h*2/3 - hh/2, roofB);
        House(x + w*3/5 - hw/2, y + h*3/5 - hh/2, roofA);
    }

    private static void DrawNuclearSymbol(SpriteBatch sb, Texture2D px, int x, int y, int w, int h)
    {
        int cx = x + w / 2, cy = y + h / 2;
        int r      = Math.Max(5,  w / 7);
        int bladeW = Math.Max(3,  w / 20);
        int dot    = Math.Max(4,  w / 18);

        var yellow = new Color(220, 195, 0);

        float[] angles = { -MathF.PI / 2f, MathF.PI / 6f, MathF.PI * 5f / 6f };
        foreach (float a in angles)
        {
            var tip = new Vector2(cx + MathF.Cos(a) * r, cy + MathF.Sin(a) * r);
            PrimitiveDrawSprites.DrawLine(sb, px, new Vector2(cx, cy), tip, yellow, bladeW);
        }
        sb.Draw(px, new Rectangle(cx - dot / 2, cy - dot / 2, dot, dot), Color.Black);
    }

    // ── Geometry helpers ─────────────────────────────────────────────────────

    public static Vector2 DirectionToEdgePoint(Direction dir, int x, int y, int w, int h) => dir switch
    {
        Direction.NORTH     => new Vector2(x + w / 2f, y),
        Direction.SOUTH     => new Vector2(x + w / 2f, y + h),
        Direction.EAST      => new Vector2(x + w,       y + h / 2f),
        Direction.WEST      => new Vector2(x,            y + h / 2f),
        Direction.NORTHWEST => new Vector2(x,            y),
        Direction.NORTHEAST => new Vector2(x + w,        y),
        Direction.SOUTHWEST => new Vector2(x,            y + h),
        Direction.SOUTHEAST => new Vector2(x + w,        y + h),
        _                   => new Vector2(x + w / 2f,   y + h / 2f),
    };
}
