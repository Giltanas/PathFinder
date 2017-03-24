using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HommFinder
{
	public class Cell
	{
		private double _value;

		public Cell(int x, int y, CellType cellType = CellType.None)
		{
			X = x;
			Y = y;
			CellType = cellType;
			
		}

		public int X { get; private set; }
		public int Y { get; private set; }

		public double Value
		{
			get { return _value; }
			set
			{
				_value = value;
			}
		}

		public CellType CellType { get; private set; }

		public void Refresh()
		{
			Value = Single.MaxValue;
		}
		public override bool Equals(object obj)
		{
			
			var cell = obj as Cell;
			if (cell != null)
			{
				if (cell.X == this.X && cell.Y == this.Y && cell.CellType == this.CellType)
				{
					return true;
				}
			}
			return false;
		}

		public bool SameLocation(object obj)
		{
			var cell = obj as Cell;
			if (cell != null)
			{
				if (cell.X == this.X && cell.Y == this.Y)
				{
					return true;
				}
			}
			return false;
		}
		public bool NeedChangeValue(double newValue)
		{
			if (newValue < Value && CellType != CellType.Block)
			{
				Value = newValue;
				return true;
			}
			return false;
		}
	}
   
	public enum CellType
	{
		Road,
		Grass,
		Snow,
		Marsh,
		Block,
		None
	}
	
}
