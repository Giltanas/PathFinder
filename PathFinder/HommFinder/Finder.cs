using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HommFinder.Extensions.Finder.Extensions;

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

		public Stack<Cell> GetMoves(Cell startCell, Cell endCell = null)
		{
			foreach (var cell in _cells)
			{
				cell.Refresh();
			}

			if (!_isWholePassBuilt)
			{
				startCell.Value = 0;
				sendWeave(startCell, endCell);
			}

			return endCell == null ? null : getMoves(startCell, endCell, new Stack<Cell>());
		}

		private Stack<Cell> getMoves(Cell startCell, Cell endCell, Stack<Cell> cells)
		{
			
			if (!endCell.Equals(startCell))
			{
				cells.Push(endCell);
				var nearCells = getNearCells(endCell);
				var newEndCell = nearCells.Single(nc=> nc.Value.Equals(nearCells.Min(c => c.Value)));
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
				var scell = GetCell(cell.X + dx[i], cell.Y + dy[i]);
				if (scell != null)
				{
					nearCells.Add(cell);
				}
			}
			return nearCells;
		}

		private Cell GetCell(int X, int Y)
		{
			return _cells.Single(c => c.X == X && c.Y == Y);
		}
	}
}
