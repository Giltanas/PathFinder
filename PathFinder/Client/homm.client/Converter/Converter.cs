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

		public static Cell ConvertMapObjectDataToCell(this MapObjectData mapObjectData)
		{
			var x = mapObjectData.Location.X;
			var y = mapObjectData.Location.Y;
			if (x % 2 == 1)
			{
				y += 1;
			}
			return new Cell(x,y, mapObjectData.GetMapObjectCellType());
		}

		public static CellType GetMapObjectCellType(this MapObjectData mapObjectData)
		{
			if (mapObjectData.Wall != null)
			{
				return CellType.Block;
			}
			if (mapObjectData.NeutralArmy != null)
			{
				//TODO: Check army strength 
				return CellType.Block;
			}
			if (mapObjectData.Mine != null)
			{
				return CellType.Block;
			}
		switch (mapObjectData.Terrain)
			{
				case Terrain.Road:
					return CellType.Road;
				case Terrain.Grass:
					return CellType.Grass;

				case Terrain.Snow:
					return CellType.Marsh;

				case Terrain.Marsh:
					return CellType.Marsh;

				case Terrain.Desert:
					return CellType.Snow;

			}
			return CellType.Block;
		}
		public static string GetMapObjectDataForPrint(this MapObjectData mapObject)
		{
			return mapObject.GetMapObjectCellType().GetCellTypeFroPrint();
		}

		public static string GetCellTypeFroPrint(this CellType cellType)
		{
			switch (cellType)
			{
				case CellType.Road:
					return "1";
				case CellType.Block:
					return "#";
				case CellType.Grass:
					return "2";
				case CellType.Marsh:
					return "3";
				case CellType.Snow:
					return "4";
			}
			return string.Empty;
		}
	}
}
