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
	/// Pixel coordinate vector (fine; integer)
	/// </summary>
	public struct PixelVectorInt
	{
		public readonly int X, Y;

		public PixelVectorInt(int x, int y) { X = x; Y = y; }
		public PixelVectorInt(Size p) { X = p.Width; Y = p.Height; }

		public static readonly PixelVectorInt Zero = new PixelVectorInt(0, 0);
		public static PixelVectorInt OneCell { get { return new PixelVectorInt(JTacticalSim.Game.Instance.GameBoard.DefaultAttributes.CellSize, JTacticalSim.Game.Instance.GameBoard.DefaultAttributes.CellSize); } }

		public static implicit operator PixelVectorFloat(PixelVectorInt a) { return new PixelVectorFloat((float)a.X, (float)a.Y); }
		public static explicit operator PixelVectorInt(int2 a) { return new PixelVectorInt(a.X, a.Y); }

		public static PixelVectorInt FromRadius(int r) { return new PixelVectorInt(r, r); }

		public static PixelVectorInt operator +(PixelVectorInt a, PixelVectorInt b) { return new PixelVectorInt(a.X + b.X, a.Y + b.Y); }
		public static PixelVectorInt operator -(PixelVectorInt a, PixelVectorInt b) { return new PixelVectorInt(a.X - b.X, a.Y - b.Y); }
		public static PixelVectorInt operator *(int a, PixelVectorInt b) { return new PixelVectorInt(a * b.X, a * b.Y); }
		public static PixelVectorInt operator *(PixelVectorInt b, int a) { return new PixelVectorInt(a * b.X, a * b.Y); }
		public static PixelVectorInt operator /(PixelVectorInt a, int b) { return new PixelVectorInt(a.X / b, a.Y / b); }

		public static PixelVectorInt operator -(PixelVectorInt a) { return new PixelVectorInt(-a.X, -a.Y); }

		public static bool operator ==(PixelVectorInt me, PixelVectorInt other) { return (me.X == other.X && me.Y == other.Y); }
		public static bool operator !=(PixelVectorInt me, PixelVectorInt other) { return !(me == other); }

		public static PixelVectorInt Max(PixelVectorInt a, PixelVectorInt b) { return new PixelVectorInt(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y)); }
		public static PixelVectorInt Min(PixelVectorInt a, PixelVectorInt b) { return new PixelVectorInt(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y)); }

		public static int Dot(PixelVectorInt a, PixelVectorInt b) { return a.X * b.X + a.Y * b.Y; }

		public PixelVectorInt Sign() { return new PixelVectorInt(Math.Sign(X), Math.Sign(Y)); }
		public PixelVectorInt Abs() { return new PixelVectorInt(Math.Abs(X), Math.Abs(Y)); }
		public int LengthSquared { get { return X * X + Y * Y; } }
		public int Length { get { return (int)Math.Sqrt(LengthSquared); } }

		public float2 ToFloat2() { return new float2(X, Y); }
		public int2 ToInt2() { return new int2(X, Y); }
		public CVec ToCVec() { return new CVec(X / JTacticalSim.Game.Instance.GameBoard.DefaultAttributes.CellSize, Y / JTacticalSim.Game.Instance.GameBoard.DefaultAttributes.CellSize); }

		public PixelVectorInt Clamp(Rectangle r)
		{
			return new PixelVectorInt(
				Math.Min(r.Right, Math.Max(X, r.Left)),
				Math.Min(r.Bottom, Math.Max(Y, r.Top))
			);
		}

		public override int GetHashCode() { return X.GetHashCode() ^ Y.GetHashCode(); }

		public override bool Equals(object obj)
		{
			if (obj == null || obj is DBNull) return false;

			PixelVectorInt o = (PixelVectorInt)obj;
			return o == this;
		}

		public override string ToString() { return "{0},{1}".F(X, Y); }
	}
}

