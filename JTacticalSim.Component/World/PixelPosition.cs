#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Drawing;
using JTacticalSim.API;
using JTacticalSim.Utility;
using JTacticalSim;

namespace JTacticalSim.Component.World
{
	/// <summary>
	/// Pixel coordinate position in the world (fine).
	/// </summary>
	public struct PixelPosition
	{
		public readonly int X, Y;

		public PixelPosition(int x, int y) { X = x; Y = y; }

		public static readonly PixelPosition Zero = new PixelPosition(0, 0);

		public static explicit operator PixelPosition(int2 a) { return new PixelPosition(a.X, a.Y); }

		public static explicit operator PixelVectorInt(PixelPosition a) { return new PixelVectorInt(a.X, a.Y); }
		public static explicit operator PixelVectorFloat(PixelPosition a) { return new PixelVectorFloat(a.X, a.Y); }

		public static PixelPosition operator +(PixelPosition a, PixelVectorInt b) { return new PixelPosition(a.X + b.X, a.Y + b.Y); }
		public static PixelVectorInt operator -(PixelPosition a, PixelPosition b) { return new PixelVectorInt(a.X - b.X, a.Y - b.Y); }
		public static PixelPosition operator -(PixelPosition a, PixelVectorInt b) { return new PixelPosition(a.X - b.X, a.Y - b.Y); }

		public static bool operator ==(PixelPosition me, PixelPosition other) { return (me.X == other.X && me.Y == other.Y); }
		public static bool operator !=(PixelPosition me, PixelPosition other) { return !(me == other); }

		public static PixelPosition Max(PixelPosition a, PixelPosition b) { return new PixelPosition(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y)); }
		public static PixelPosition Min(PixelPosition a, PixelPosition b) { return new PixelPosition(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y)); }

		public static PixelPosition Lerp(PixelPosition a, PixelPosition b, int mul, int div)
		{
			return a + ((PixelVectorInt)(b - a) * mul / div);
		}

		public float2 ToFloat2() { return new float2(X, Y); }
		public int2 ToInt2() { return new int2(X, Y); }
		public Coordinate ToCoordinate() { return new Coordinate((int)(1f / JTacticalSim.Game.Instance.GameBoard.DefaultAttributes.CellSize * X), (int)(1f / JTacticalSim.Game.Instance.GameBoard.DefaultAttributes.CellSize * Y), 0); }
		//public PSubPos ToPSubPos() { return new PSubPos(X * PSubPos.PerPx, Y * PSubPos.PerPx); }

		public PixelPosition Clamp(Rectangle r)
		{
			return new PixelPosition(Math.Min(r.Right, Math.Max(X, r.Left)),
							Math.Min(r.Bottom, Math.Max(Y, r.Top)));
		}

		public override int GetHashCode() { return X.GetHashCode() ^ Y.GetHashCode(); }

		public override bool Equals(object obj)
		{
			if (obj == null || obj is DBNull) return false;

			PixelPosition o = (PixelPosition)obj;
			return o == this;
		}

		public override string ToString() { return "{0},{1}".F(X, Y); }
	}
}

