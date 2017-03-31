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
		private double _priopity;
		public Cell(int x, int y, TerrainCellType terrainCellType = TerrainCellType.None, ObjectCellType cellType = ObjectCellType.None)
		{
			X = x;
			Y = y;
			TerrainCellType = terrainCellType;
			CellType = cellType;
		}

		public int X { get; private set; }
		public int Y { get; private set; }

		public double Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public double Priority
		{
			get { return _priopity; }
			set { _priopity = value; }
		}
		public TerrainCellType TerrainCellType { get; private set; }
		public ObjectCellType CellType { get; private set; }
		public void Refresh()
		{
			Value = Single.MaxValue;
		}
		public override bool Equals(object obj)
		{
			
			var cell = obj as Cell;
			if (cell != null)
			{
				if (cell.X == this.X && cell.Y == this.Y 
					&& cell.TerrainCellType == this.TerrainCellType
					&& cell.CellType == this.CellType)
				{
					return true;
				}
			}
			return false;
		}

		public bool SameLocation(Cell cell)
		{
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
			if (newValue < Value && TerrainCellType != TerrainCellType.Block)
			{
				Value = newValue;
				return true;
			}
			return false;
		}
	}
   
	public enum TerrainCellType
	{
		Road,
		Grass,
		Snow,
		Marsh,
		Block,
		None

	}

	public enum ObjectCellType
	{
		DwellingInfantry,
		DwellingRanged,
		DwellingCavalry,
		DwellingMilitia,
		MineGold,
		MineGlass,
		MineIron,
		MineEbony,
		ResourceGold,
		ResourceGlass,
		ResourceIron,
		ResourceEbony, 
		None
	}
}
