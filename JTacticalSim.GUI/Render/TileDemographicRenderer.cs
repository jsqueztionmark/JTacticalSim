using JTacticalSim.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JTacticalSim.API.Component;
using JTacticalSim.Utility;

namespace JTacticalSim.GUI.Render;

public static class TileDemographicRenderer
{
    private static readonly Color ColWater = new(30, 60, 140);

    // ── Shoreline ─────────────────────────────────────────────────────────────

    public static void DrawShoreline(ITile tile, SpriteBatch sb, Texture2D px, int x, int y, int w, int h)
    {
        var rh = tile?.ConsoleRenderHelper;
        if (rh == null) return;

        bool any = rh.HasShoreLineNorth || rh.HasShoreLineSouth ||
                   rh.HasShoreLineEast  || rh.HasShoreLineWest  ||
                   rh.HasShoreLineNorthWest || rh.HasShoreLineNorthEast ||
                   rh.HasShoreLineSouthWest || rh.HasShoreLineSouthEast;
        if (!any) return;

        var sand = new Color(205, 185, 135);
        const int t = 3;
        int mx = x + w / 2, my = y + h / 2;

        // Sand straddles the tile edge it's drawn on (t/2 each side)
        void HLine(int x1, int y1, int len) => sb.Draw(px, new Rectangle(x1, y1 - t / 2, len, t), sand);
        void VLine(int x1, int y1, int len) => sb.Draw(px, new Rectangle(x1 - t / 2, y1, t, len), sand);

        // Cardinal — full line along the tile edge that faces water
        if (rh.HasShoreLineNorth) HLine(x,     y,     w);   // water above
        if (rh.HasShoreLineSouth) HLine(x,     y + h, w);   // water below
        if (rh.HasShoreLineEast)  VLine(x + w, y,     h);   // water to the right
        if (rh.HasShoreLineWest)  VLine(x,     y,     h);   // water to the left

        // Corner — L at the tile edges bounding the single land quadrant
        if (rh.HasShoreLineNorthWest) { HLine(x,     y,     mx - x);     VLine(x,     y,     my - y);     }
        if (rh.HasShoreLineNorthEast) { HLine(mx,    y,     x + w - mx); VLine(x + w, y,     my - y);     }
        if (rh.HasShoreLineSouthWest) { HLine(x,     y + h, mx - x);     VLine(x,     my,    y + h - my); }
        if (rh.HasShoreLineSouthEast) { HLine(mx,    y + h, x + w - mx); VLine(x + w, my,    y + h - my); }
    }

    // ── Geographic / flora character overlay (MapFont) ───────────────────────
    // Rendered between base quadrant fills and infrastructure, matching console render stack:
    // flora → geography terrain → creeks → roads/bridges → bases → airports → large mountain → nuclear

    public static void DrawGeographicOverlay(ITile tile, SpriteBatch sb, SpriteFont mapFont,
                                             int x, int y, int w, int h, int zoomLevel)
    {
        if (mapFont == null) return;
        var rh = tile?.ConsoleRenderHelper;
        if (rh == null) return;

        var zoom = (ZoomLevel)zoomLevel;
        int spc  = Math.Max(8, w / 6);
        var land = GetLandQuadrants(tile);

        bool hasMountains = rh.HasMountains || rh.HasMountain;

        // Flora — suppressed on mountain tiles (hills keep flora)
        if (!hasMountains)
        {
            int denseSpc = Math.Max(6, w / 8);   // denser spacing for forested
            if (rh.HasForests)   TileChars(tile.Flora, "Forested", sb, mapFont, x, y, w, h, zoom, new Color(20,  110, 20),  denseSpc, land, 1.5f);
            if (rh.HasTrees)     TileChars(tile.Flora, "Trees",    sb, mapFont, x, y, w, h, zoom, new Color(160, 85,  30),  spc,      land, 1.5f);
            if (rh.HasWoodlands) TileChars(tile.Flora, "Woodland", sb, mapFont, x, y, w, h, zoom, new Color(25,  100, 25), spc, land, 1.5f);
            if (rh.HasMarsh)     TileChars(tile.Flora, "Marsh",    sb, mapFont, x, y, w, h, zoom, new Color(80,  105, 72), spc, land, 1.5f);
        }

        // Geography terrain — hills/mountains rendered larger
        if (rh.HasHills)     TileChars(tile.AllGeography, "Hills",     sb, mapFont, x, y, w, h, zoom, new Color(115, 132, 92),  spc, land, 2.0f);
        if (rh.HasMountains) TileChars(tile.AllGeography, "Mountains", sb, mapFont, x, y, w, h, zoom, new Color(160, 152, 142), spc, land, 2.2f);
        if (rh.HasLakes)     TileChars(tile.AllGeography, "Lake",      sb, mapFont, x, y, w, h, zoom, new Color(18,  48,  138), spc, (false, false, false, false), 1.5f);
    }

    // Mirrors GetTileQuadrantColors land/water logic — tells TileChars which quadrants to draw on
    private static (bool tl, bool tr, bool bl, bool br) GetLandQuadrants(ITile tile)
    {
        var h = tile?.ConsoleRenderHelper;
        if (h == null) return (true, true, true, true);
        if (h.HasShoreLineNorth)     return (true,  true,  false, false);
        if (h.HasShoreLineSouth)     return (false, false, true,  true);
        if (h.HasShoreLineWest)      return (true,  false, true,  false);
        if (h.HasShoreLineEast)      return (false, true,  false, true);
        if (h.HasShoreLineSouthWest) return (false, false, true,  false);
        if (h.HasShoreLineSouthEast) return (false, false, false, true);
        if (h.HasShoreLineNorthWest) return (true,  false, false, false);
        if (h.HasShoreLineNorthEast) return (false, true,  false, false);
        if (h.IsRiver || h.IsSea || h.HasLakes) return (false, false, false, false);
        return (true, true, true, true);
    }

    private static void TileChars(IEnumerable<IDemographic> demographics, string className,
                                   SpriteBatch sb, SpriteFont fnt,
                                   int x, int y, int w, int h,
                                   ZoomLevel zoom, Color color, int spacing,
                                   (bool tl, bool tr, bool bl, bool br) landQ,
                                   float scale = 1.5f)
    {
        var demo = demographics?.FirstOrDefault(d => d.IsDemographicClass(className));
        if (demo == null) return;

        string display = demo.DemographicClass.GetTextDisplayForZoom(zoom);
        if (string.IsNullOrEmpty(display)) return;

        var    parts  = display.Split(',');
        string chA    = parts[0].Trim();
        string chB    = parts.Length > 1 ? parts[1].Trim() : chA;
        if (string.IsNullOrEmpty(chA)) return;

        var sizeA = fnt.MeasureString(chA) * scale;

        int midX = x + w / 2;
        int midY = y + h / 2;
        int cols = Math.Max(1, w / spacing);
        int rows = Math.Max(1, h / spacing);

        for (int row = 0; row < rows; row++)
        {
            float xOff = row % 2 == 1 ? spacing / 2f : 0f;
            for (int col = 0; col < cols; col++)
            {
                float cx = x + col * spacing + xOff;
                float cy = y + row * spacing;

                // Quadrant bounds check — skip if this position is on a water quadrant
                bool isLeft = cx < midX;
                bool isTop  = cy < midY;
                bool isLand = isTop  &&  isLeft  ? landQ.tl
                            : isTop  && !isLeft  ? landQ.tr
                            : !isTop &&  isLeft  ? landQ.bl
                            :                      landQ.br;
                if (!isLand) continue;

                if (cx + sizeA.X > x + w || cy + sizeA.Y > y + h) continue;

                string ch = (row + col) % 2 == 0 ? chA : chB;
                if (string.IsNullOrEmpty(ch)) continue;

                sb.DrawString(fnt, ch, new Vector2(cx, cy), color,
                              0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
        }
    }

    // ── Infrastructure overlay ────────────────────────────────────────────────

    public static void DrawInfrastructureOverlay(ITile tile, SpriteBatch sb, Texture2D px, int x, int y, int w, int h)
    {
        var center = new Vector2(x + w / 2f, y + h / 2f);

        // Creeks first — drawn under roads, centered on tile so segments meet cleanly
        var geo = tile?.AllGeography;
        if (geo != null)
        {
            foreach (var demo in geo)
            {
                if (demo?.Orientation == null || !demo.Orientation.Any()) continue;
                if (!demo.IsDemographicClass("Creek")) continue;
                foreach (var dir in demo.Orientation)
                    PrimitiveDrawSprites.DrawLine(sb, px, center, DirectionToEdgePoint(dir, x, y, w, h), ColWater, 2);
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

    // ── Demographic sprites ───────────────────────────────────────────────────

    // Pre-road terrain sprites (mountain peak, nuclear)
    public static void DrawDemographicSprites(ITile tile, SpriteBatch sb, Texture2D px, int x, int y, int w, int h, int zoomLevel)
    {
        var rh = tile?.ConsoleRenderHelper;
        if (rh == null) return;

        if (rh.IsNuclearWasteland && zoomLevel >= 2)
            DrawNuclearSymbol(sb, px, x, y, w, h);

        if (rh.HasMountain && zoomLevel >= 3)
            DrawMountainPeak(sb, px, x, y, w, h);
    }

    // Airport — rendered below urban sprites; caller applies co-location offset
    public static void DrawAirportSprite(ITile tile, SpriteBatch sb, Texture2D px, int x, int y, int w, int h, int zoomLevel)
    {
        var rh = tile?.ConsoleRenderHelper;
        if (rh == null || !rh.HasAirports || zoomLevel < 3) return;
        var demo = tile.Infrastructure.SingleOrDefault(d => d.IsDemographicClass("Airport"));
        if (demo != null) DrawAirport(sb, px, x, y, w, h, demo.Orientation);
    }

    // Top-layer facility sprites — rendered over everything (military base, CP only)
    public static void DrawFacilitySprites(ITile tile, SpriteBatch sb, Texture2D px, int x, int y, int w, int h, int zoomLevel)
    {
        var rh = tile?.ConsoleRenderHelper;
        if (rh == null) return;

        if (rh.HasMilitaryBase && zoomLevel >= 2)
            DrawMilitaryBase(sb, px, x, y, w, h);

        if (rh.HasCommandPost && zoomLevel >= 2)
            DrawCommandPost(sb, px, x, y, w, h);
    }

    // Called after infrastructure overlay so settlement sprites render over roads
    public static void DrawUrbanSprites(ITile tile, SpriteBatch sb, Texture2D px, int x, int y, int w, int h, int zoomLevel)
    {
        var rh = tile?.ConsoleRenderHelper;
        if (rh == null) return;
        if (rh.HasCities)    DrawUrban(sb, px, x, y, w, h);
        if (rh.HasIndustrial) DrawIndustrial(sb, px, x, y, w, h);
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

    private static void DrawBuilding(SpriteBatch sb, Texture2D px, int bx, int by, int bw, int bh, Color fill, Color accent)
    {
        sb.Draw(px, new Rectangle(bx + 1,     by,          bw - 2, bh),     fill);
        sb.Draw(px, new Rectangle(bx,         by,          bw,     1),      Color.Black);
        sb.Draw(px, new Rectangle(bx,         by + bh - 1, bw,     1),      Color.Black);
        sb.Draw(px, new Rectangle(bx,         by,          1,      bh),     Color.Black);
        sb.Draw(px, new Rectangle(bx + bw - 1, by,         1,      bh),     Color.Black);
        if (bh > 8)
            sb.Draw(px, new Rectangle(bx + 2, by + bh / 3, bw - 4, 1), accent);

        // Window dots — 2×2 blue, two rows
        var win = new Color(120, 175, 230);
        int numWin     = Math.Max(1, (bw - 4) / 5);
        int winSpacing = (bw - 2) / (numWin + 1);
        foreach (int wy in new[] { by + bh / 4, by + bh / 2 })
            for (int c = 1; c <= numWin; c++)
                sb.Draw(px, new Rectangle(bx + c * winSpacing, wy, 2, 2), win);
    }

    private static void DrawUrban(SpriteBatch sb, Texture2D px, int x, int y, int w, int h)
    {
        var back   = new Color(68, 63, 57);
        var front  = new Color(88, 82, 76);
        var accent = new Color(130, 40, 40);

        int col = w / 5;   // five columns

        // Back row — 5 taller buildings, one per column
        int backBaseY = y + h * 2 / 3;
        int[] backH   = { h*2/5, h/3, h*2/5, h*3/8, h/3 };
        int[] backW   = { w/8,   w/7, w/9,   w/7,   w/8 };

        for (int i = 0; i < 5; i++)
        {
            int bx = x + col * i + (col - backW[i]) / 2;
            DrawBuilding(sb, px, bx, backBaseY - backH[i], backW[i], backH[i], back, accent);
        }

        // Front row — 4 shorter buildings, offset half a column to the right
        int frontBaseY = y + h * 5 / 6;
        int[] frontH   = { h/5,  h*3/10, h/5,  h*3/10 };
        int[] frontW   = { w/9,  w/8,    w/7,  w/8    };

        for (int i = 0; i < 4; i++)
        {
            int bx = x + col * i + col / 2 + (col - frontW[i]) / 2;
            if (bx + frontW[i] > x + w) break;
            DrawBuilding(sb, px, bx, frontBaseY - frontH[i], frontW[i], frontH[i], front, accent);
        }
    }

    private static void DrawIndustrial(SpriteBatch sb, Texture2D px, int x, int y, int w, int h)
    {
        DrawUrban(sb, px, x, y, w, h);

        int cx    = x + w / 2;
        int ch    = h / 5;
        int cw    = Math.Max(3, w / 14);
        int baseY = y + h * 2 / 3;   // align chimney with back row base
        sb.Draw(px, new Rectangle(cx - cw / 2, baseY - h * 2 / 5 - ch, cw, ch), new Color(90, 88, 84));
    }

    private static void DrawTown(SpriteBatch sb, Texture2D px, int x, int y, int w, int h, List<Direction> orientation)
    {
        _ = orientation;

        int u   = Math.Max(10, w / 6);
        int gap = 3;

        var wallA = new Color(190, 172, 130);
        var wallB = new Color(175, 155, 110);
        var win   = new Color(120, 175, 230);

        // 5 houses: widths, heights (nearly square), wall colours, roof colours
        int[]   hW    = { u,     u+3,   u-1,  u+2,   u-1 };
        int[]   hH    = { u,     u+2,   u-1,  u+1,   u   };
        Color[] walls = { wallA, wallB, wallA, wallA, wallB };
        Color[] roofs = {
            new Color(140, 60, 48),
            new Color(115, 82, 62),
            new Color(140, 60, 48),
            new Color(128, 72, 55),
            new Color(115, 82, 62),
        };

        void RoofTriangle(int rx, int baseY, int rw, int rh, Color rc)
        {
            int steps = Math.Max(2, Math.Min(6, rh));
            int stepH = Math.Max(1, rh / steps);
            for (int s = 0; s < steps; s++)
            {
                int rowW = Math.Max(2, rw * (steps - s) / steps);
                int rowY = baseY - (s + 1) * stepH;
                sb.Draw(px, new Rectangle(rx + (rw - rowW) / 2, rowY, rowW, stepH), rc);
            }
        }

        void House(int hx, int baseY, int hw, int hht, Color wc, Color rc)
        {
            int roofH = Math.Max(4, hht * 2 / 5);
            sb.Draw(px, new Rectangle(hx,          baseY - hht, hw, hht), wc);
            sb.Draw(px, new Rectangle(hx,          baseY - hht, hw, 1),   Color.Black);
            sb.Draw(px, new Rectangle(hx,          baseY - 1,   hw, 1),   Color.Black);
            sb.Draw(px, new Rectangle(hx,          baseY - hht, 1, hht),  Color.Black);
            sb.Draw(px, new Rectangle(hx + hw - 1, baseY - hht, 1, hht),  Color.Black);
            if (hht > 8 && hw > 6)
                sb.Draw(px, new Rectangle(hx + hw / 2 - 1, baseY - hht + hht / 3, 2, 2), win);
            RoofTriangle(hx, baseY - hht, hw, roofH, rc);
        }

        // Back row: 3 houses centered
        int backTotalW = hW[0] + hW[1] + hW[2] + gap * 2;
        int backX      = x + (w - backTotalW) / 2;
        int backBaseY  = y + h * 55 / 100;

        int cx = backX;
        for (int i = 0; i < 3; i++) { House(cx, backBaseY, hW[i], hH[i], walls[i], roofs[i]); cx += hW[i] + gap; }

        // Front row: 2 houses, offset half a house-width to the right
        int frontTotalW = hW[3] + hW[4] + gap;
        int frontX      = x + (w - frontTotalW) / 2 + hW[0] / 2;
        int frontBaseY  = y + h * 75 / 100;

        cx = frontX;
        for (int i = 3; i < 5; i++) { House(cx, frontBaseY, hW[i], hH[i], walls[i], roofs[i]); cx += hW[i] + gap; }
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
