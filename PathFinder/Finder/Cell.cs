using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finder
{
	public class Cell
	{
		public Cell(int x, int y, CellType cellType)
		{
			X = x;
			Y = y;
			CellType = cellType;
		}

		public int X { get; private set; }
		public int Y { get; private set; }
		public int Value { get; set; }
		public CellType CellType { get; private set; }

		public override bool Equals(object obj)
		{
			var cell = obj as Cell;
			return cell?.X == this.X && cell.Y == this.Y;
		}
	}
   
	public enum CellType
	{
		Road,
		Grace,
		Snow,
		Marsh
	}
}
