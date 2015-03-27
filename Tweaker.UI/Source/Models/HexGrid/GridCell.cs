using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.UI
{
	public class HexGridCell<TCellValue>
		where TCellValue : class
	{
		private TCellValue value;
		public TCellValue Value { get { return value; } set { this.value = value; } }

		public CubeCoord CubeCoord { get; private set; }
		public AxialCoord AxialCoord { get; private set; }

		public CubeCoord[] cubeNeighbours;
		public AxialCoord[] axialNeighbours;

		public HexGridCell(AxialCoord axialCoord)
		{
			AxialCoord = axialCoord;
			CubeCoord = HexCoord.AxialToCube(axialCoord);
			CacheNeighbourCoords();
		}

		public HexGridCell(AxialCoord axialCoord, TCellValue value)
		{
			AxialCoord = axialCoord;
			CubeCoord = HexCoord.AxialToCube(axialCoord);
			Value = value;
			CacheNeighbourCoords();
		}

		private void CacheNeighbourCoords()
		{
			cubeNeighbours = new CubeCoord[6];
			axialNeighbours = new AxialCoord[6];
			for (var i = 0; i < 6; ++i)
			{
				cubeNeighbours[i] = CubeCoord + CubeCoord.Directions[i];
				axialNeighbours[i] = AxialCoord + AxialCoord.Directions[i];
			}
		}
	}	
}
