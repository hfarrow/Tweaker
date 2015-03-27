using Ghostbit.Tweaker.Core;
using System;
using System.Collections.Generic;

namespace Ghostbit.Tweaker.UI
{
	public class HexGrid<TCellValue>
		where TCellValue : class
	{
		private ITweakerLogger logger = LogManager.GetCurrentClassLogger();
		private Dictionary<string, HexGridCell<TCellValue>> cells;

		public HexGrid(uint width, uint height)
		{
			cells = HexGridFactory.MakeRectangleGrid<TCellValue>(width, height);
		}

		public void ClearCells()
		{
			foreach (var cell in cells.Values)
			{
				cell.Value = default(TCellValue);
			}
		}

		public HexGridCell<TCellValue> GetCell(AxialCoord axialCoord)
		{
			HexGridCell<TCellValue> cell = null;
			cells.TryGetValue(axialCoord.ToString(), out cell);
			return cell;
		}

		public HexGridCell<TCellValue> GetCell(CubeCoord coord)
		{
			AxialCoord axialCoord;
			HexCoord.CubeToAxial(ref coord, out axialCoord);
			return GetCell(axialCoord);
		}

		public TCellValue GetCellValue(AxialCoord coord)
		{
			return GetCell(coord).Value;
		}

		public TCellValue GetCellValue(CubeCoord coord)
		{
			return GetCell(coord).Value;
		}

		public void SetCellValue(TCellValue value, CubeCoord coord)
		{
			var cell = GetCell(coord);
			cell.Value = value;
		}

		public void SetCellValue(TCellValue value, AxialCoord coord)
		{
			var cell = GetCell(coord);
			cell.Value = value;
		}

		public IEnumerable<HexGridCell<TCellValue>> GetRingCells(CubeCoord center, uint radius)
		{
			logger.Trace("GetRingCells({0}, {1})", center, radius);
			CubeCoord direction = CubeCoord.Directions[4] * (int)radius;
			var cube = center + direction;
			for (uint i = 0; i < 6; ++i)
			{
				for (uint j = 0; j < radius; ++j)
				{
					var cell = GetCell(cube);
					//logger.Trace("yeild '{0}' @ '{1}' for {2},{3}", cell != null ? cell.AxialCoord.ToString() : "null", cube, i, j);
					if( cell != null)
					{
						yield return cell;
					}
					cube = HexCoord.GetNeighbour(cube, (i));
				}
			}
		}

		public IEnumerable<HexGridCell<TCellValue>> GetSpiralCells(CubeCoord center, uint radius)
		{
			logger.Trace("GetSpiralCells({0}, {1})", center, radius);
			var centerCell = GetCell(center);
			if (centerCell != null)
			{
				yield return centerCell;
			}

			for (uint i = 1; i <= radius; ++i)
			{
				foreach (var cell in GetRingCells(center, i))
				{
					if( cell != null)
					{
						yield return cell;
					}
				}
			}
		}

		public IEnumerable<TCellValue> GetRingValues(CubeCoord center, uint radius)
		{
			foreach (var cell in GetRingCells(center, radius))
			{
				yield return cell.Value;
			}
		}

		public IEnumerable<TCellValue> GetSpiralValues(CubeCoord center, uint radius)
		{
			foreach (var cell in GetSpiralCells(center, radius))
			{
				yield return cell.Value;
			}
		}
	}
}
