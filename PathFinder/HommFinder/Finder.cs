using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HommFinder.Extensions;
using HoMM;
using HoMM.ClientClasses;

namespace HommFinder
{
	public class Finder
	{
		public List<Cell> _cells;
		private Cell _startCell;
<<<<<<< HEAD
		const int ValidVerificationStepNumber = 5;
		private static readonly int[] dx0 = new[] { 1, -1, 0, 1, -1, 0 };
		private static readonly int[] dy0 = new[] { 0, 0, 1, -1, -1, -1 };
		private static readonly int[] dx1 = new[] { 1, -1, 0, 1, -1, 0 };
=======
	    private static readonly int[] dx0 = new[] { 1, -1, 0, 1, -1, 0 };
	    private static readonly int[] dy0 = new[] { 0, 0, 1, -1, -1, -1 };
        private static readonly int[] dx1 = new[] { 1, -1, 0, 1, -1, 0 };
>>>>>>> refs/remotes/origin/master
		private static readonly	int[] dy1 = new[] { 0, 0, 1, 1, 1, -1 };
		private Dictionary<Resource, int> _plusResources;
		public Finder(List<Cell> cells, Cell startCell)
		{
			_cells = cells;
			_startCell = _cells.Single(c => c.X == startCell.X && c.Y == startCell.Y);
			foreach (var cell in _cells)
			{
				cell.Refresh();
			}
			_startCell.NeedChangeValue(0);
			sendWave(_startCell);
		}
		
		public List<Cell> GetSmartPath(Cell startCell, Cell endCell, List<Cell> smartPath=null )
		{
			startCell = _cells.Find(c=> c.SameLocation(startCell));
			endCell = _cells.Find(c => c.SameLocation(endCell));

			if (startCell == null || endCell == null || endCell.TerrainCellType==TerrainCellType.Block)
			{
				return null;
			}

			var moves = new Finder(_cells,startCell).GetMovesStraightToCell(endCell);
			if (smartPath == null)
			{
				smartPath = new List<Cell>();
			}
			foreach (var move in moves)
			{
				
				smartPath.Add(move);

				if (move.Equals(endCell))
				{
					return smartPath;
				}

				var nearCells = getNearCells(move).FindAll(c => c.CellType.MainType == MainCellType.Resource);
				foreach (var resourceCell in nearCells.Where(resourceCell => resourceCell != null && !smartPath.Contains(resourceCell)))
				{
					if (resourceCell.Equals(endCell))
					{
						smartPath.Add(endCell);
						return smartPath;
					}
					return GetSmartPath(resourceCell, endCell, smartPath);
				}

			}
			return  smartPath;
		}

		public bool CanDoSomething(Dictionary<Resource,int> myTreasure)
		{
			//Implement checking can do something to reach more points
			return true;
		}
		public List<Cell> GetMovesStraightToCell(Cell endCell=null)
		{
			if (endCell == null)
			{
				return new List<Cell>();
			}

			endCell = _cells.SingleOrDefault(c=> c.SameLocation(endCell));

			return endCell.Value == Single.MaxValue ?
				new List<Cell>() : 
				getMoves(_startCell,
				_cells.SingleOrDefault(c => c.SameLocation(endCell)),
				new Stack<Cell>()).ToList();
		}

		private List<Cell> getNearCells(Cell cell)
		{
			var nearCells = new List<Cell>();
			for (int i = 0; i < 6; i++)
			{
				Cell nearCell = null;

				nearCell = cell.X % 2 == 0 ? getCell(cell.X + dx0[i], cell.Y + dy0[i]) : getCell(cell.X + dx1[i], cell.Y + dy1[i]);

				if (nearCell != null && nearCell.TerrainCellType != TerrainCellType.Block)
				{
					nearCells.Add(nearCell);
				}
			}
			return nearCells;
		}

		private Cell getCell(int x, int y)
		{
			return _cells.SingleOrDefault(c => c.X == x && c.Y == y);
		}
		private Stack<Cell> getMoves(Cell startCell, Cell endCell, Stack<Cell> cells)
		{
			cells.Push(_cells.Find(c=> c.SameLocation(endCell)));
			if (!endCell.Equals(startCell))
			{
				var nearCells = getNearCells(endCell);
				var sortedValueList = nearCells.FindAll(nc => nc.Value.Equals(nearCells.Min(c => c.Value)));
				var newEndCell = sortedValueList.FirstOrDefault(nc => nc.TerrainCellType.GetTerrainCellTypeWeight().Equals(sortedValueList.Min(c => c.TerrainCellType.GetTerrainCellTypeWeight())));

				getMoves(startCell, newEndCell, cells);
			}
			return cells;
		}
		private void sendWave(Cell startCell)
		{
			var nearCells = getNearCells(startCell);

			foreach (var nearCell in nearCells)
			{
				if (nearCell.NeedChangeValue(startCell.Value + startCell.TerrainCellType.GetTerrainCellTypeWeight()))
					sendWave(nearCell);
			}
		}
	}
}
