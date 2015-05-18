using System;
using System.Collections.Generic;

namespace Ghostbit.Tweaker.UI
{
	public struct PixelCoord
	{
		public float x;
		public float y;

		public PixelCoord(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
	}

	public struct CubeCoord
	{
		public static CubeCoord[] Directions = 
		{
			new CubeCoord(1, -1, 0), new CubeCoord(1, 0, -1), new CubeCoord(0, 1, -1),
			new CubeCoord(-1, 1, 0), new CubeCoord(-1, 0, 1), new CubeCoord(0, -1, 1)
		};

		public static CubeCoord Origin = new CubeCoord(0, 0, 0);

		public int x;
		public int y;
		public int z;

		public CubeCoord(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public static CubeCoord FromFraction(double x, double y, double z)
		{
			double rx = Math.Round(x);
			double ry = Math.Round(y);
			double rz = Math.Round(z);

			double x_diff = Math.Abs(rx - x);
			double y_diff = Math.Abs(ry - y);
			double z_diff = Math.Abs(rz - z);

			if (x_diff > y_diff && x_diff > z_diff)
			{
				rx = -ry - rz;
			}
			else if(y_diff > z_diff)
			{
				ry = -rx - rz;
			}
			else
			{
				rz = -rx - ry;
			}

			return new CubeCoord((int)Math.Round((decimal)rx),
				(int)Math.Round((decimal)ry),
				(int)Math.Round((decimal)rz));
		}

		public int Distance(CubeCoord other)
		{
			return (Math.Abs(x - other.x) + Math.Abs(y - other.y) + Math.Abs(z - other.z)) / 2;
		}

		public override string ToString()
		{
			return String.Format("({0},{1},{2})", x, y, z);
		}

		public static CubeCoord operator +(CubeCoord a, CubeCoord b)
		{
			return new CubeCoord(a.x + b.x, a.y + b.y, a.z + b.z);
		}

		public static CubeCoord operator -(CubeCoord a, CubeCoord b)
		{
			return new CubeCoord(a.x - b.x, a.y - b.y, a.z - b.z);
		}

		public static CubeCoord operator *(CubeCoord a, CubeCoord b)
		{
			return new CubeCoord(a.x * b.x, a.y * b.y, a.z * b.z);
		}

		public static CubeCoord operator *(CubeCoord coord, int scalar)
		{
			return new CubeCoord(coord.x * scalar, coord.y * scalar, coord.z * scalar);
		}

		public static CubeCoord operator *(int scalar, CubeCoord coord)
		{
			return new CubeCoord(coord.x * scalar, coord.y * scalar, coord.z * scalar);
		}

		// untested but I assumed we have to round the coordinates.
		public static CubeCoord operator /(CubeCoord a, CubeCoord b)
		{
			return CubeCoord.FromFraction((double)a.x / (double)b.x, 
				(double)a.y / (double)b.y, 
				(double)a.z / (double)b.z);
		}

		public static CubeCoord operator /(CubeCoord coord, int scalar)
		{
			return CubeCoord.FromFraction((double)coord.x / (double)scalar,
				(double)coord.y / (double)scalar,
				(double)coord.z / (double)scalar);
		}

		public static CubeCoord operator /(int scalar, CubeCoord coord)
		{
			return CubeCoord.FromFraction((double)coord.x / (double)scalar,
				(double)coord.y / (double)scalar,
				(double)coord.z / (double)scalar);
		}
	}

	public struct AxialCoord
	{
		public static AxialCoord[] Directions = 
		{
			 new AxialCoord(1,0), new AxialCoord(1, -1), new AxialCoord(0, -1),
			 new AxialCoord(-1,0), new AxialCoord(-1, 1), new AxialCoord(0, 1)
		};

		public int q;
		public int r;

		public AxialCoord(int q, int r)
		{
			this.q = q;
			this.r = r;
		}

		public int Distance(AxialCoord other)
		{
			// Convert to cube coord and use CubeCoord.Distance
			CubeCoord ac;
			CubeCoord bc;
			HexCoord.AxialToCube(ref this, out ac);
			HexCoord.AxialToCube(ref other, out bc);
			return ac.Distance(bc);
		}

		public override string ToString()
		{
			return String.Format("({0},{1})", q, r);
		}

		public static AxialCoord operator +(AxialCoord a, AxialCoord b)
		{
			return new AxialCoord(a.q + b.q, a.r + b.r);
		}

		public static AxialCoord operator -(AxialCoord a, AxialCoord b)
		{
			return new AxialCoord(a.q - b.q, a.r - b.r);
		}
	}

	public static class HexCoord
	{
		public static void CubeToAxial(ref CubeCoord coord, out AxialCoord outCoord)
		{
			outCoord =  new AxialCoord(coord.x, coord.y);
			//outCoord = new AxialCoord(coord.x, coord.z);
			//outCoord = new AxialCoord(coord.y, coord.z);
		}

		public static AxialCoord CubeToAxial(CubeCoord coord)
		{
			return new AxialCoord(coord.x, coord.y);
			//return new AxialCoord(coord.x, coord.z);
			//return new AxialCoord(coord.y, coord.z);
		}

		public static void AxialToCube(ref AxialCoord coord, out CubeCoord outCoord)
		{
			outCoord = new CubeCoord(coord.q, coord.r, -coord.q - coord.r);
			//outCoord = new CubeCoord(coord.q, -coord.q - coord.r, coord.r);
			//outCoord = new CubeCoord(-coord.q - coord.r, coord.q, coord.r);
		}

		public static CubeCoord AxialToCube(AxialCoord coord)
		{
			return new CubeCoord(coord.q, coord.r, -coord.q - coord.r);
			//return new CubeCoord(coord.q, -coord.q - coord.r, coord.r);
			//return new CubeCoord(-coord.q - coord.r, coord.q, coord.r);
		}

		public static PixelCoord AxialToPixel(CubeCoord coord, float size)
		{
			AxialCoord axialCoord = CubeToAxial(coord);
			return AxialToPixel(axialCoord, size);
		}

		public static PixelCoord AxialToPixel(AxialCoord coord, float size)
		{
			// Flat Top
			float x = size * 3f / 2f * (float)coord.q;
			float y = size * (float)Math.Sqrt(3) * (coord.r + (float)coord.q / 2f);

			// Pointy Top
			//float x = size * Mathf.Sqrt(3f) * ((float)coord.q + (float)coord.r / 2f);
			//float y = size * 3f / 2f * (float)coord.r;

			//y *= -1;
			return new PixelCoord(x, y);
		}

		public static CubeCoord GetNeighbour(CubeCoord coord, uint direction)
		{
			return coord + CubeCoord.Directions[direction];
		}

		public static AxialCoord GetNeighbour(AxialCoord coord, uint direction)
		{
			return coord + AxialCoord.Directions[direction];
		}
	}
}
