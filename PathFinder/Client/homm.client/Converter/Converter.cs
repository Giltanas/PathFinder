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
	public static class ConverterExtensions
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

		public static TerrainCellType GetMapObjectCellType(this MapObjectData mapObjectData)
		{
			if (mapObjectData.Wall != null)
			{
				return TerrainCellType.Block;
			}
			if (mapObjectData.NeutralArmy != null)
			{
				//TODO: Check army strength 
				return TerrainCellType.Block;
			}
			if (mapObjectData.Mine != null)
			{
				return TerrainCellType.Block;
			}
		switch (mapObjectData.Terrain)
			{
				case Terrain.Road:
					return TerrainCellType.Road;
				case Terrain.Grass:
					return TerrainCellType.Grass;

				case Terrain.Snow:
					return TerrainCellType.Snow;

				case Terrain.Marsh:
					return TerrainCellType.Marsh;

				//value of desert`s passability = value of snow`s passability
				case Terrain.Desert:
					return TerrainCellType.Snow;

			}
			return TerrainCellType.Block;
		}
		public static string GetMapObjectDataForPrint(this MapObjectData mapObject)
		{
			return mapObject.GetMapObjectCellType().GetCellTypeForPrint();
		}

		public static string GetCellTypeForPrint(this TerrainCellType cellType)
		{
			switch (cellType)
			{
				case TerrainCellType.Road:
					return "1";
				case TerrainCellType.Block:
					return "#";
				case TerrainCellType.Grass:
					return "2";
				case TerrainCellType.Marsh:
					return "3";
				case TerrainCellType.Snow:
					return "4";
			}
			return string.Empty;
		}
	}
}
