using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HommFinder.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	namespace Finder.Extensions
	{
		public static class Extensions
		{
			//1.729922279792746 SNOW 
			//1.3316062176165804 GRASS 
			//1.7292746113989637 MARSH 
			//1 - ROAD
			public static double GetCellTypeWeight(this CellType cellType)
			{
				switch (cellType)
				{
					case CellType.Road:
						return 1f;
					case CellType.Grace:
						return 1.3316062176165804f;
					case CellType.Snow:
						return 1.729922279792746f;
					case CellType.Marsh:
						return 1.7292746113989637f;
					case CellType.Block:
						return Single.MaxValue;
				}
				return -1;
			}
		}
	}
}
