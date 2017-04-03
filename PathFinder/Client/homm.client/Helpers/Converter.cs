using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoMM;
using HoMM.ClientClasses;
using HommFinder;

namespace Homm.Client.Helpers
{
	public static class Converter
	{
		public static List<Direction> ConvertCellPathToDirection(IList<Cell> cells)
		{
			var direction = new List<Direction>();

			for (int i = 1; i < cells.Count; i++)
			{
				var x = cells[i].X - cells[i - 1].X;
				var y = cells[i].Y - cells[i - 1].Y;
				if (x == 1 && y == 1)
				{
					direction.Add(Direction.RightDown);
					continue;
				}
				if (x == 0 && y == 1)
				{
					direction.Add(Direction.Down);
					continue;
				}
				if (x == -1 && y == 1)
				{
					direction.Add(Direction.LeftDown);
					continue;
				}
				if (x == 1 && y == -1)
				{
					direction.Add(Direction.RightUp);
					continue;
				}
				if (x == 0 && y == -1)
				{
					direction.Add(Direction.Up);
					continue;
				}
				if (x == -1 && y == -1)
				{
					direction.Add(Direction.LeftUp);
					continue;
				}
				if (x == -1 && y == 0)
				{
					direction.Add(cells[i - 1].X % 2 == 0 ? Direction.LeftDown : Direction.LeftUp);
					continue;
				}
				if (x == 1 && y == 0)
				{
					direction.Add(cells[i - 1].X % 2 == 0 ? Direction.RightDown : Direction.RightUp);
				}
			}
			return direction;
		}
	}
}
