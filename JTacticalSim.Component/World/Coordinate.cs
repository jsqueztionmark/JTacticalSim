using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.Utility;

namespace JTacticalSim.Component.World
{
	public class Coordinate : ICoordinate
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }

		public Coordinate(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static readonly ICoordinate Zero = new Coordinate(0, 0, 0);

		public static explicit operator Coordinate(int2 a) { return new Coordinate(a.X, a.Y, 0); }

		public static Coordinate operator +(CVec a, Coordinate b) { return new Coordinate(a.X + b.X, a.Y + b.Y, 0); }
		public static Coordinate operator +(Coordinate a, CVec b) { return new Coordinate(a.X + b.X, a.X + b.Y, 0); }
		public static Coordinate operator -(Coordinate a, CVec b) { return new Coordinate(a.X - b.X, a.Y - b.Y, 0); }

		public static CVec operator -(Coordinate a, Coordinate b) { return new CVec(a.X - b.X, a.Y - b.Y); }

		public static bool operator ==(Coordinate me, Coordinate other) { return (me.X == other.X && me.Y == other.Y); }
		public static bool operator !=(Coordinate me, Coordinate other) { return !(me == other); }

		public static Coordinate Max(Coordinate a, Coordinate b) { return new Coordinate(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), 0); }
		public static Coordinate Min(Coordinate a, Coordinate b) { return new Coordinate(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), 0); }

		public float2 ToFloat2() { return new float2(X, Y); }
		public int2 ToInt2() { return new int2(X, Y); }
		public PixelPosition ToPixelPosition() { return new PixelPosition(JTacticalSim.Game.Instance.GameBoard.DefaultAttributes.CellSize * X, JTacticalSim.Game.Instance.GameBoard.DefaultAttributes.CellSize * Y); }

		public Coordinate Clamp(Rectangle r)
		{
			return new Coordinate(Math.Min(r.Right, Math.Max(X, r.Left)), Math.Min(r.Bottom, Math.Max(Y, r.Top)), 0);
		}

		public override int GetHashCode() {return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();}

		public override bool Equals(object obj)
		{
			if (obj == null || obj is DBNull) return false;

			ICoordinate c = (ICoordinate)obj;

			return	this.X == c.X && 
					this.Y == c.Y && 
					this.Z == c.Z;
		}

		public override string ToString()
		{
			return "{0}, {1}, {2}".F(X, Y, Z);
		}

		public string ToStringForName()
		{
			return "{0}_{1}_{2}".F(X, Y, Z);
		}
	}


}
