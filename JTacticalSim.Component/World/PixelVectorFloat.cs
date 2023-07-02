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

namespace JTacticalSim.Component.World
{
	/// <summary>
	/// Pixel coordinate vector (fine; float)
	/// </summary>
	public struct PixelVectorFloat
	{
		public readonly float X, Y;

		public PixelVectorFloat(float x, float y) { X = x; Y = y; }
		public PixelVectorFloat(Size p) { X = p.Width; Y = p.Height; }

		public static readonly PixelVectorFloat Zero = new PixelVectorFloat(0, 0);

		public static explicit operator PixelVectorInt(PixelVectorFloat a) { return new PixelVectorInt((int)a.X, (int)a.Y); }
		public static explicit operator PixelVectorFloat(float2 a) { return new PixelVectorFloat(a.X, a.Y); }

		public static PixelVectorFloat operator +(PixelVectorFloat a, PixelVectorFloat b) { return new PixelVectorFloat(a.X + b.X, a.Y + b.Y); }
		public static PixelVectorFloat operator -(PixelVectorFloat a, PixelVectorFloat b) { return new PixelVectorFloat(a.X - b.X, a.Y - b.Y); }
		public static PixelVectorFloat operator *(float a, PixelVectorFloat b) { return new PixelVectorFloat(a * b.X, a * b.Y); }
		public static PixelVectorFloat operator *(PixelVectorFloat b, float a) { return new PixelVectorFloat(a * b.X, a * b.Y); }
		public static PixelVectorFloat operator /(PixelVectorFloat a, float b) { return new PixelVectorFloat(a.X / b, a.Y / b); }

		public static PixelVectorFloat operator -(PixelVectorFloat a) { return new PixelVectorFloat(-a.X, -a.Y); }

		public static bool operator ==(PixelVectorFloat me, PixelVectorFloat other) { return (me.X == other.X && me.Y == other.Y); }
		public static bool operator !=(PixelVectorFloat me, PixelVectorFloat other) { return !(me == other); }

		public static PixelVectorFloat Max(PixelVectorFloat a, PixelVectorFloat b) { return new PixelVectorFloat(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y)); }
		public static PixelVectorFloat Min(PixelVectorFloat a, PixelVectorFloat b) { return new PixelVectorFloat(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y)); }

		public static float Dot(PixelVectorFloat a, PixelVectorFloat b) { return a.X * b.X + a.Y * b.Y; }

		public static PixelVectorFloat FromAngle(float a) { return new PixelVectorFloat((float)Math.Sin(a), (float)Math.Cos(a)); }

		public static PixelVectorFloat Lerp(PixelVectorFloat a, PixelVectorFloat b, float t)
		{
			return new PixelVectorFloat(
				float2.Lerp(a.X, b.X, t),
				float2.Lerp(a.Y, b.Y, t)
			);
		}

		public static PixelVectorFloat Lerp(PixelVectorFloat a, PixelVectorFloat b, PixelVectorFloat t)
		{
			return new PixelVectorFloat(
				float2.Lerp(a.X, b.X, t.X),
				float2.Lerp(a.Y, b.Y, t.Y)
			);
		}

		public PixelVectorFloat Sign() { return new PixelVectorFloat(Math.Sign(X), Math.Sign(Y)); }
		public PixelVectorFloat Abs() { return new PixelVectorFloat(Math.Abs(X), Math.Abs(Y)); }
		public PixelVectorFloat Round() { return new PixelVectorFloat((float)Math.Round(X), (float)Math.Round(Y)); }
		public float LengthSquared { get { return X * X + Y * Y; } }
		public float Length { get { return (float)Math.Sqrt(LengthSquared); } }

		public float2 ToFloat2() { return new float2(X, Y); }
		public int2 ToInt2() { return new int2((int)X, (int)Y); }

		static float Constrain(float x, float a, float b) { return x < a ? a : x > b ? b : x; }

		public PixelVectorFloat Constrain(PixelVectorFloat min, PixelVectorFloat max)
		{
			return new PixelVectorFloat(
				Constrain(X, min.X, max.X),
				Constrain(Y, min.Y, max.Y)
			);
		}

		public override int GetHashCode() { return X.GetHashCode() ^ Y.GetHashCode(); }

		public override bool Equals(object obj)
		{
			if (obj == null || obj is DBNull) return false;

			PixelVectorFloat o = (PixelVectorFloat)obj;
			return o == this;
		}

		public override string ToString() { return "({0},{1})".F(X, Y); }
	}
}

