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

        public List<Cell> CheckDwelling(Cell dwellingCheck, HommSensorData SensorData, UnitType unitType, Resource resource)
        {
            var path = new List<Cell>();
            var missingTreasury = existTreasuryForDwelling(dwellingCheck, SensorData, unitType, resource);
            if (missingTreasury.Count == 0)
            {
                path = GetMovesStraightToCell(dwellingCheck);
            }
            else
            {
                //TODO:: check resources near path
                var localPath = findResourcesForDwelling(missingTreasury, dwellingCheck, resource);
                if (localPath.Count < ValidVerificationStepNumber)
                    path = localPath;
            }
            return path;
        }
       
        public List<Cell> CheckDwellingMilitia(Cell dwellingCheck, HommSensorData SensorData)
		{
			var path = new List<Cell>();
			var missingTreasury = existTreasuryForDwelling(dwellingCheck, SensorData, UnitType.Militia);
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
            HommSensorData localSensorData, UnitType unitType, Resource resource = new Resource())
		{

		    var missingResources = new Dictionary<Resource, int>();

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
		    else
		    {
                if (localSensorData.MyTreasury[Resource.Gold] >=
                UnitsConstants.Current.UnitCost[unitType][Resource.Gold] &&
                localSensorData.MyTreasury[resource] >=
                UnitsConstants.Current.UnitCost[unitType][resource])
                {
                    return new Dictionary<Resource, int>();
                }

                missingResources.Add(Resource.Gold,
                    UnitsConstants.Current.UnitCost[unitType][Resource.Gold] -
                    localSensorData.MyTreasury[Resource.Gold]);

                missingResources.Add(resource,
                    UnitsConstants.Current.UnitCost[unitType][resource] -
                    localSensorData.MyTreasury[resource]);
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

		private List<Cell> findResourcesForDwelling(Dictionary<Resource, int> missingTreasury, Cell dwelling, Resource resource)
		{
			var subCellType = new SubCellType();
			if (resource == Resource.Ebony)
				subCellType = SubCellType.ResourceEbony;
			if (resource == Resource.Iron)
				subCellType = SubCellType.ResourceIron;
			if (resource == Resource.Glass)
				subCellType = SubCellType.ResourceGlass;


			var cellList = new List<Cell>();
			while (missingTreasury[Resource.Gold] >= 0 &&
				_cells.Where(o => (o.CellType.SubCellType == SubCellType.ResourceGold)
				&& !o.Value.Equals(Single.MaxValue) && !cellList.Contains(o)).ToList().Count != 0)
			{
				findResources(cellList, SubCellType.ResourceGold);
				foreach (var item in cellList)
				{
					missingTreasury[Resource.Gold] = missingTreasury[Resource.Gold] - item.ResourcesValue;
				}
			}

			while (missingTreasury[resource] >= 0 &&
				_cells.Where(o => (o.CellType.SubCellType == subCellType)
				&& !o.Value.Equals(Single.MaxValue) && !cellList.Contains(o)).ToList().Count != 0)
			{
				findResources(cellList, subCellType);
				foreach (var item in cellList)
				{
					missingTreasury[resource] = missingTreasury[resource] - item.ResourcesValue;
				}
			}

			var cellPath = new List<Cell>();
			if (cellList.Count > 0)
			{
				cellPath.AddRange(GetMovesStraightToCell(cellList[0]));
				for (int y = 1; y < cellList.Count; y++)
				{
					var finderNew = new Finder(_cells, cellList[y]);
					cellPath.AddRange(finderNew.GetMovesStraightToCell(cellList[y]));
				}
				var finderToEnd = new Finder(_cells, cellList[cellList.Count - 1]);
				cellPath.AddRange(finderToEnd.GetMovesStraightToCell(dwelling));
			}
			return cellPath;
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
