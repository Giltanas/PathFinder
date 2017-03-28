using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HommFinder.Extensions
{

	namespace Finder.Extensions
	{
		public static class Extensions
		{
			//1.729922279792746 SNOW 
			//1.3316062176165804 GRASS 
			//1.7292746113989637 MARSH 
			//1 - ROAD
			public static double GetTerrainCellTypeWeight(this TerrainCellType cellType)
			{
				switch (cellType)
				{
					case TerrainCellType.Road:
						return 1f;
					case TerrainCellType.Grass:
						return 1.3316062176165804f;
					case TerrainCellType.Snow:
						return 1.729922279792746f;
					case TerrainCellType.Marsh:
						return 1.7292746113989637f;
					case TerrainCellType.Block:
						return Single.MaxValue;
				}
				return Single.MaxValue;
			}
		}
	}
}
