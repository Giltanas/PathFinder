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
		public static IEnumerable<Direction> ConvertCellPathToDirection(Stack<Cell> cells)
		{
			var direction = new List<Direction>();
			var listCells = cells.ToList();
			for (int i = 1; i < cells.Count; i++)
			{
				var x = listCells[i].X - listCells[i - 1].X;
				var y = listCells[i].Y - listCells[i - 1].Y;
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
				}
			}
			return direction;
		}
	}
}
