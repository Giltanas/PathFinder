using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoMM;
using HoMM.ClientClasses;
using HommFinder;

namespace Homm.Client.Converter
{
	public static class Converter
	{
		public static IEnumerable<Direction> ConvertCellsToDirection(Stack<Cell> cells)
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

		public static Cell ConvertGMapObjectDataToCell(this MapObjectData mapObjectData)
		{
			var cellType = CellType.Road;

			switch (mapObjectData.Terrain)
			{
				case Terrain.Road:
					cellType = CellType.Road;
					break;
				case Terrain.Grass:
					cellType = CellType.Grass;
					break;
				case Terrain.Snow:
					cellType = CellType.Marsh;
					break;
				case Terrain.Marsh:
					cellType = CellType.Marsh;
					break;
				case Terrain.Desert:
					cellType = CellType.Snow;
					break;
			}
			if (mapObjectData.Wall != null)
			{
				cellType = CellType.Block;
			}
			if (mapObjectData.NeutralArmy != null)
			{
				//TODO: Check army strength 
				cellType = CellType.Block;
			}
			if (mapObjectData.Mine != null)
			{
				cellType = CellType.Block;
			}

			return new Cell(mapObjectData.Location.X, mapObjectData.Location.Y, cellType);
		}
	}
}
