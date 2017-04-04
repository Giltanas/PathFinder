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
		const int ValidVerificationStepNumber = 20;
	    private static readonly int[] dx0 = new[] { 1, -1, 0, 1, -1, 0 };
	    private static readonly int[] dy0 = new[] { 0, 0, 1, -1, -1, -1 };
        private static readonly int[] dx1 = new[] { 1, -1, 0, 1, -1, 0 };
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

 
		
		public List<Cell> SearchAvailableDwellings( )
		{
			return _cells.Where(i => (i.CellType.MainType == MainCellType.Dwelling)
						   && !i.Value.Equals(Single.MaxValue) && (i.ResourcesValue>0)).ToList();

		}

		public List<Cell> SearchAvailableResources()
		{
			return _cells.Where(i => (i.CellType.MainType == MainCellType.Resource)
						   && !i.Value.Equals(Single.MaxValue)).ToList();
		}

		public List<Cell> SearchAvailableMines()
		{
			return _cells.Where(i => (i.CellType.MainType == MainCellType.Mine)
						   && !i.Value.Equals(Single.MaxValue)).ToList();
		}

		public List<Cell> CheckDwellingRanged(Cell dwellingCheck, HommSensorData SensorData)
		{
			var path = new List<Cell>();
			var missingTreasury = existTreasuryForDwelling(dwellingCheck, SensorData);
			if (missingTreasury.Count == 0)
			{
				path = GetMovesStraightToCell(dwellingCheck);
			}
			else
			{
				//TODO:: check resources near path
				var localPath = findResourcesForDwellingRanged(missingTreasury, dwellingCheck);
				if (localPath.Count < ValidVerificationStepNumber)
					path = localPath;
			}
			return path;
		}

		public List<Cell> CheckDwellingInfantry(Cell dwellingCheck, HommSensorData SensorData)
		{
			var path = new List<Cell>();
			var missingTreasury = existTreasuryForDwelling(dwellingCheck, SensorData);
			if (missingTreasury.Count == 0)
			{
				path = GetMovesStraightToCell(dwellingCheck);
			}
			else
			{
				//TODO:: check resources near path
				var localPath = findResourcesForDwellingInfantry(missingTreasury, dwellingCheck);
				if (localPath.Count < ValidVerificationStepNumber)
					path = localPath;
			}
			return path;
		}

        public List<Cell> CheckDwellingCavalry(Cell dwellingCheck, HommSensorData SensorData)
        {
            var path = new List<Cell>();
            var missingTreasury = existTreasuryForDwelling(dwellingCheck, SensorData);
            if (missingTreasury.Count == 0)
            {
                path = GetMovesStraightToCell(dwellingCheck);
            }
            else
            {
				//TODO:: check resources near path
				var localPath = findResourcesForDwellingCavalry(missingTreasury, dwellingCheck);
                if (localPath.Count < ValidVerificationStepNumber)
                    path = localPath;
            }
            return path;
        }

        public List<Cell> CheckDwellingMilitia(Cell dwellingCheck, HommSensorData SensorData)
		{
			var path = new List<Cell>();
			var missingTreasury = existTreasuryForDwelling(dwellingCheck, SensorData);
			if (missingTreasury.Count == 0)
			{
				path = GetMovesStraightToCell(dwellingCheck);
			}  
			else
			{
				//TODO:: check resources near path
				var localPath = findResourcesForDwellingMilitia(missingTreasury, dwellingCheck);
				if (localPath.Count != 0)
				   path = localPath;
			}

			return path;
		}

		private Dictionary<Resource, int> existTreasuryForDwelling(Cell dwellingCheck,
            HommSensorData localSensorData)
		{
		    var missingResources = new Dictionary<Resource, int>();
		    if (dwellingCheck.CellType.SubCellType == SubCellType.DwellingCavalry)
		    {
		        if (localSensorData.MyTreasury[Resource.Gold] >=
		            UnitsConstants.Current.UnitCost[UnitType.Cavalry][Resource.Gold] &&
		            localSensorData.MyTreasury[Resource.Ebony] >=
		            UnitsConstants.Current.UnitCost[UnitType.Cavalry][Resource.Ebony])
		        {
		            return new Dictionary<Resource, int>();
		        }

                missingResources.Add(Resource.Gold, 
                    UnitsConstants.Current.UnitCost[UnitType.Cavalry][Resource.Gold] - 
                    localSensorData.MyTreasury[Resource.Gold]);

                missingResources.Add(Resource.Ebony,
                    UnitsConstants.Current.UnitCost[UnitType.Cavalry][Resource.Ebony] - 
                    localSensorData.MyTreasury[Resource.Ebony]);
		    }

            if (dwellingCheck.CellType.SubCellType == SubCellType.DwellingInfantry)
            {
                if (localSensorData.MyTreasury[Resource.Gold] >=
                    UnitsConstants.Current.UnitCost[UnitType.Infantry][Resource.Gold] &&
                    localSensorData.MyTreasury[Resource.Iron] >=
                    UnitsConstants.Current.UnitCost[UnitType.Infantry][Resource.Iron])
                {
                    return new Dictionary<Resource, int>();
                }

                missingResources.Add(Resource.Gold,
                    UnitsConstants.Current.UnitCost[UnitType.Infantry][Resource.Gold] -
                    localSensorData.MyTreasury[Resource.Gold]);

                missingResources.Add(Resource.Iron,
                    UnitsConstants.Current.UnitCost[UnitType.Infantry][Resource.Iron] -
                    localSensorData.MyTreasury[Resource.Iron]);
            }

            if (dwellingCheck.CellType.SubCellType == SubCellType.DwellingRanged)
            {
                if (localSensorData.MyTreasury[Resource.Gold] >=
                    UnitsConstants.Current.UnitCost[UnitType.Ranged][Resource.Gold] &&
                    localSensorData.MyTreasury[Resource.Glass] >=
                    UnitsConstants.Current.UnitCost[UnitType.Ranged][Resource.Glass])
                {
                    return new Dictionary<Resource, int>();
                }

                missingResources.Add(Resource.Gold,
                    UnitsConstants.Current.UnitCost[UnitType.Ranged][Resource.Gold] -
                    localSensorData.MyTreasury[Resource.Gold]);

                missingResources.Add(Resource.Glass,
                    UnitsConstants.Current.UnitCost[UnitType.Ranged][Resource.Glass] -
                    localSensorData.MyTreasury[Resource.Glass]);
            }

            if (dwellingCheck.CellType.SubCellType == SubCellType.DwellingMilitia)
			{
				if (localSensorData.MyTreasury[Resource.Gold] >=
					UnitsConstants.Current.UnitCost[UnitType.Militia][Resource.Gold])
				{
					return new Dictionary<Resource, int>();
				}
                missingResources.Add(Resource.Gold, 
                    UnitsConstants.Current.UnitCost[UnitType.Militia][Resource.Gold] - 
                    localSensorData.MyTreasury[Resource.Gold]);
            }
	
			return missingResources;
		}

		private List<Cell> findResourcesForDwellingMilitia(Dictionary<Resource, int> missingTreasury, Cell dwelling)
		{
			var goldCellList = new List<Cell>();
			while (missingTreasury[Resource.Gold] >= 0 &&
				_cells.Where(o => (o.CellType.SubCellType == SubCellType.ResourceGold)
				&& !o.Value.Equals(Single.MaxValue) && !goldCellList.Contains(o)).ToList().Count != 0)
			{
				findResources(goldCellList, SubCellType.ResourceGold);
				foreach (var item in goldCellList)
				{
					missingTreasury[Resource.Gold] = missingTreasury[Resource.Gold] - item.ResourcesValue;
				}
			}

			var goldCellPath = new List<Cell>();
		    if (goldCellList.Count > 0)
		    {
                goldCellPath.AddRange(GetMovesStraightToCell(goldCellList[0]));
                for (int y = 1; y < goldCellList.Count; y++)
                {
                    var finderNew = new Finder(_cells, goldCellList[y]);
                    goldCellPath.AddRange(finderNew.GetMovesStraightToCell(goldCellList[y]));
                }
                var finderToEnd = new Finder(_cells, goldCellList[goldCellList.Count - 1]);
                goldCellPath.AddRange(finderToEnd.GetMovesStraightToCell(dwelling));
            }   
			return goldCellPath;
		}

	    private List<Cell> findResourcesForDwellingCavalry(Dictionary<Resource, int> missingTreasury, Cell dwelling)
	    {
			var goldEbonyCellList = new List<Cell>();
			while (missingTreasury[Resource.Gold] >= 0 &&
				_cells.Where(o => (o.CellType.SubCellType == SubCellType.ResourceGold)
				&& !o.Value.Equals(Single.MaxValue) && !goldEbonyCellList.Contains(o)).ToList().Count != 0)
			{
				findResources(goldEbonyCellList, SubCellType.ResourceGold);
				foreach (var item in goldEbonyCellList)
				{
					missingTreasury[Resource.Gold] = missingTreasury[Resource.Gold] - item.ResourcesValue;
				}
			}

			while (missingTreasury[Resource.Ebony] >= 0 &&
				_cells.Where(o => (o.CellType.SubCellType == SubCellType.ResourceEbony)
				&& !o.Value.Equals(Single.MaxValue) && !goldEbonyCellList.Contains(o)).ToList().Count != 0)
			{
				findResources(goldEbonyCellList, SubCellType.ResourceEbony);
				foreach (var item in goldEbonyCellList)
				{
					missingTreasury[Resource.Ebony] = missingTreasury[Resource.Ebony] - item.ResourcesValue;
				}
			}

			var goldEbonyCellPath = new List<Cell>();
			if (goldEbonyCellList.Count > 0)
			{
				goldEbonyCellPath.AddRange(GetMovesStraightToCell(goldEbonyCellList[0]));
				for (int y = 1; y < goldEbonyCellList.Count; y++)
				{
					var finderNew = new Finder(_cells, goldEbonyCellList[y]);
					goldEbonyCellPath.AddRange(finderNew.GetMovesStraightToCell(goldEbonyCellList[y]));
				}
				var finderToEnd = new Finder(_cells, goldEbonyCellList[goldEbonyCellList.Count - 1]);
				goldEbonyCellPath.AddRange(finderToEnd.GetMovesStraightToCell(dwelling));
			}
			return goldEbonyCellPath;
        }

		private List<Cell> findResourcesForDwellingInfantry(Dictionary<Resource, int> missingTreasury, Cell dwelling)
		{
			var goldIronCellList = new List<Cell>();
			while (missingTreasury[Resource.Gold] >= 0 &&
				_cells.Where(o => (o.CellType.SubCellType == SubCellType.ResourceGold)
				&& !o.Value.Equals(Single.MaxValue) && !goldIronCellList.Contains(o)).ToList().Count != 0)
			{
				findResources(goldIronCellList, SubCellType.ResourceGold);
				foreach (var item in goldIronCellList)
				{
					missingTreasury[Resource.Gold] = missingTreasury[Resource.Gold] - item.ResourcesValue;
				}
			}

			while (missingTreasury[Resource.Iron] >= 0 &&
				_cells.Where(o => (o.CellType.SubCellType == SubCellType.ResourceIron)
				&& !o.Value.Equals(Single.MaxValue) && !goldIronCellList.Contains(o)).ToList().Count != 0)
			{
				findResources(goldIronCellList, SubCellType.ResourceIron);
				foreach (var item in goldIronCellList)
				{
					missingTreasury[Resource.Iron] = missingTreasury[Resource.Iron] - item.ResourcesValue;
				}
			}

			var goldIRonCellPath = new List<Cell>();
			if (goldIronCellList.Count > 0)
			{
				goldIRonCellPath.AddRange(GetMovesStraightToCell(goldIronCellList[0]));
				for (int y = 1; y < goldIronCellList.Count; y++)
				{
					var finderNew = new Finder(_cells, goldIronCellList[y]);
					goldIRonCellPath.AddRange(finderNew.GetMovesStraightToCell(goldIronCellList[y]));
				}
				var finderToEnd = new Finder(_cells, goldIronCellList[goldIronCellList.Count - 1]);
				goldIRonCellPath.AddRange(finderToEnd.GetMovesStraightToCell(dwelling));
			}
			return goldIRonCellPath;
		}

		private List<Cell> findResourcesForDwellingRanged(Dictionary<Resource, int> missingTreasury, Cell dwelling)
		{
			var goldGlassCellList = new List<Cell>();
			while (missingTreasury[Resource.Gold] >= 0 &&
				_cells.Where(o => (o.CellType.SubCellType == SubCellType.ResourceGold)
				&& !o.Value.Equals(Single.MaxValue) && !goldGlassCellList.Contains(o)).ToList().Count != 0)
			{
				findResources(goldGlassCellList, SubCellType.ResourceGold);
				foreach (var item in goldGlassCellList)
				{
					missingTreasury[Resource.Gold] = missingTreasury[Resource.Gold] - item.ResourcesValue;
				}
			}

			while (missingTreasury[Resource.Glass] >= 0 &&
				_cells.Where(o => (o.CellType.SubCellType == SubCellType.ResourceGlass)
				&& !o.Value.Equals(Single.MaxValue) && !goldGlassCellList.Contains(o)).ToList().Count != 0)
			{
				findResources(goldGlassCellList, SubCellType.ResourceGlass);
				foreach (var item in goldGlassCellList)
				{
					missingTreasury[Resource.Glass] = missingTreasury[Resource.Glass] - item.ResourcesValue;
				}
			}

			var goldGlassCellPath = new List<Cell>();
			if (goldGlassCellList.Count > 0)
			{
				goldGlassCellPath.AddRange(GetMovesStraightToCell(goldGlassCellList[0]));
				for (int y = 1; y < goldGlassCellList.Count; y++)
				{
					var finderNew = new Finder(_cells, goldGlassCellList[y]);
					goldGlassCellPath.AddRange(finderNew.GetMovesStraightToCell(goldGlassCellList[y]));
				}
				var finderToEnd = new Finder(_cells, goldGlassCellList[goldGlassCellList.Count - 1]);
				goldGlassCellPath.AddRange(finderToEnd.GetMovesStraightToCell(dwelling));
			}
			return goldGlassCellPath;
		}

		private List<Cell> findResources(List<Cell> cellPath, SubCellType subCellType)
	    {
			var сellList = _cells.Where(o => (o.CellType.SubCellType == subCellType)
								&& !o.Value.Equals(Single.MaxValue) && !cellPath.Contains(o)).ToList();

			for (int i = 0; i < сellList.Count; i++)
			{
				var cell = сellList.ElementAt(i);
				var localPath = GetMovesStraightToCell(cell);
				if (localPath.Count <= ValidVerificationStepNumber)
				{
					cellPath.Add(cell);
					return cellPath;
				}
			}
			return null;
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
			var ret = _cells.SingleOrDefault(c => c.X == x && c.Y == y);
			return ret;
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
