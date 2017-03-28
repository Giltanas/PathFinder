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
		private List<Cell> _cells;
		private List<Union> _unions;
		private bool _isWholePassBuilt = false;

		public Finder(List<Cell> cells)
		{
			_cells = cells;
			
		}

		public Stack<Cell> GetMoves(Cell startCell, Cell endCell)
		{
			startCell = _cells.Single(c => c.X == startCell.X && c.Y == startCell.Y);
			endCell = _cells.Single(c => c.X == endCell.X && c.Y == endCell.Y);
			foreach (var cell in _cells)
			{
				cell.Refresh();
			}

			if (!_isWholePassBuilt)
			{
				startCell.NeedChangeValue(0);
				sendWeave(startCell, endCell);
			}
			
			return endCell == null ? null : getMoves(startCell, endCell, new Stack<Cell>());
		}

		private Stack<Cell> getMoves(Cell startCell, Cell endCell, Stack<Cell> cells)
		{
			cells.Push(new Cell(endCell.X, endCell.Y, endCell.CellType));
			if (!endCell.Equals(startCell))
			{
				var nearCells = getNearCells(endCell);
				var sortedValueList = nearCells.FindAll(nc => nc.Value.Equals(nearCells.Min(c => c.Value)));
				if (sortedValueList.Count > 1)
				{
					var a = 1;
				}
				var newEndCell = sortedValueList.FirstOrDefault(nc=> nc.CellType.GetCellTypeWeight().Equals(sortedValueList.Min(c=> c.CellType.GetCellTypeWeight())));

				getMoves(startCell, newEndCell, cells);
			}
			return cells;
		}
		private void sendWeave(Cell startCell, Cell endCell)
		{
			if (endCell == null)
			{
				_isWholePassBuilt = true;
			}
			var nearCells = getNearCells(startCell);

			foreach (var nearCell in nearCells)
			{
				if (nearCell.NeedChangeValue(startCell.Value + startCell.CellType.GetCellTypeWeight()))
					sendWeave(nearCell, endCell);
			}
		}

		private List<Cell> getNearCells(Cell cell)
		{
			var nearCells = new List<Cell>();
			var dx = new [] { 1, -1, 0, 1 , -1 ,0 };
			var dy = new[] { 1, 1, 1, -1, -1, -1 };
			for (int i = 0; i < 6; i++)
			{
				var nearCell = getCell(cell.X + dx[i], cell.Y + dy[i]);
				if (nearCell != null && nearCell.CellType != TerrainCellType.Block )
				{
					nearCells.Add(nearCell);
				}
			}
			return nearCells;
		}

		private Cell getCell(int x, int y)
		{
			var ret =  _cells.SingleOrDefault(c => c.X == x && c.Y == y);
			if (ret == null)
			{
				return null;
			}
			return ret;
		}
	}
}
