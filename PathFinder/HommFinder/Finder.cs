using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HommFinder.Extensions;

namespace HommFinder
{
	public class Finder
	{
		public List<Cell> _cells;
		private Cell _startCell;
		public Finder(List<Cell> cells, Cell startCell)
		{
			_cells = cells;
			_startCell = _cells.Single(c => c.X == startCell.X && c.Y == startCell.Y);
			foreach (var cell in _cells)
			{
				cell.Refresh();
			}
			_startCell.NeedChangeValue(0);
			sendWeave(_startCell);
		   
		}

		public Stack<Cell> GetMoves(Cell endCell=null)
		{
			if (endCell == null)
			{
				return new Stack<Cell>();
			}
			endCell = _cells.SingleOrDefault(c=> c.X == endCell.X && c.Y == endCell.Y);
			return endCell.Value == Single.MaxValue ?
				new Stack<Cell>() : 
				getMoves(_startCell,
				_cells.SingleOrDefault(c => c.X == endCell.X && c.Y == endCell.Y),new Stack<Cell>());
		}

		private Stack<Cell> getMoves(Cell startCell, Cell endCell, Stack<Cell> cells)
		{

			cells.Push(new Cell(endCell.X, endCell.Y, endCell.TerrainCellType, endCell.CellType));
			if (!endCell.Equals(startCell))
			{
				var nearCells = getNearCells(endCell);
				var sortedValueList = nearCells.FindAll(nc => nc.Value.Equals(nearCells.Min(c => c.Value)));
				var newEndCell = sortedValueList.FirstOrDefault(nc => nc.TerrainCellType.GetTerrainCellTypeWeight().Equals(sortedValueList.Min(c => c.TerrainCellType.GetTerrainCellTypeWeight())));

				getMoves(startCell, newEndCell, cells);
			}
			return cells;
		}
		private void sendWeave(Cell startCell, Cell endCell=null)
		{
			var nearCells = getNearCells(startCell);

			foreach (var nearCell in nearCells)
			{
				if (nearCell.NeedChangeValue(startCell.Value + startCell.TerrainCellType.GetTerrainCellTypeWeight()))
					sendWeave(nearCell, endCell);
			}
		}

		private List<Cell> getNearCells(Cell cell)
		{
			var nearCells = new List<Cell>();

			//X % 2 = 0
			var dx = new[] { 1, -1, 0, 1, -1, 0 };
			var dy = new[] { 0, 0, 1, -1, -1, -1 };
			//X % 2 = 1
			if (cell.X % 2 == 1)
			{
				dx = new[] {1, -1, 0, 1, -1, 0};
				dy = new[] {0, 0, 1, 1, 1, -1};
			}
			for (int i = 0; i < 6; i++)
			{
				var nearCell = getCell(cell.X + dx[i], cell.Y + dy[i]);
				if (nearCell != null && nearCell.TerrainCellType != TerrainCellType.Block)
				{
					nearCells.Add(nearCell);
				}
			}
			return nearCells;
		}

		private Cell getCell(int x, int y)
		{
			var ret = _cells.SingleOrDefault(c => c.X == x && c.Y == y);
			return ret;
		}

		public List<Cell> SearchAvailableDwellings( )
		{
			return _cells.Where(i => (i.CellType == ObjectCellType.DwellingCavalry ||
						   i.CellType == ObjectCellType.DwellingRanged ||
						   i.CellType == ObjectCellType.DwellingInfantry ||
						   i.CellType == ObjectCellType.DwellingMilitia)
						   && !i.Value.Equals(Single.MaxValue)).ToList();

		}

		public List<Cell> SearchAvailableResources()
		{
			return _cells.Where(i => (i.CellType == ObjectCellType.ResourceGold ||
						   i.CellType == ObjectCellType.ResourceEbony ||
						   i.CellType == ObjectCellType.ResourceGlass ||
						   i.CellType == ObjectCellType.ResourceIron)
						   && !i.Value.Equals(Single.MaxValue)).ToList();
		}

		public List<Cell> SearchAvailableMines()
		{
			return _cells.Where(i => (i.CellType == ObjectCellType.MineEbony ||
						   i.CellType == ObjectCellType.MineGlass ||
						   i.CellType == ObjectCellType.MineGold ||
						   i.CellType == ObjectCellType.MineIron)
						   && !i.Value.Equals(Single.MaxValue)).ToList();
		}
	}
}
