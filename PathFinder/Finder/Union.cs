using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finder
{
	public class Union
	{
		public Cell CellA { get; private set; }
		public Cell CellB { get; private set; }
		public int Weight { get; private set; }

		public Union(Cell cellA, Cell cellB)
		{
			CellA = cellA;
			CellB = cellB;

			Weight = (int)cellA.CellType;
		}
	}
}
