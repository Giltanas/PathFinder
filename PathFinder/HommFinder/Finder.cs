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
        
        public List<Cell> GetSmartPath(Cell startCell, Cell endCell )
        {
            var moves = GetMovesStraightToCell(endCell);
            foreach (var move in moves)
            {
               // getNearCells(move).SingleOrDefault(c=> c.CellType == )

            }
            return  new List<Cell>();
        }
        public List<Cell> GetMovesStraightToCell(Cell endCell=null)
        {
            if (endCell == null)
            {
                return new List<Cell>();
            }
            endCell = _cells.SingleOrDefault(c=> c.X == endCell.X && c.Y == endCell.Y);
            return endCell.Value == Single.MaxValue ?
                new List<Cell>() : 
                getMoves(_startCell,
                _cells.SingleOrDefault(c => c.X == endCell.X && c.Y == endCell.Y),
                new Stack<Cell>()).ToList();
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

        public List<Cell> CheckDwellingRanged(Cell dwellingCheck, HommSensorData SensorData)
        {
            var path = new List<Cell>();
            if (dwellingCheck.CellType == ObjectCellType.DwellingRanged)
            {
                if (SensorData.MyTreasury[Resource.Gold] >=
                    UnitsConstants.Current.UnitCost[UnitType.Ranged][Resource.Gold] &&
                    SensorData.MyTreasury[Resource.Glass] >=
                    UnitsConstants.Current.UnitCost[UnitType.Ranged][Resource.Glass])
                {
                    path = GetMovesStraightToCell(dwellingCheck);
                }
                else
                {
                    //TODO:: check resources near path
                    var localPath = GetMoves(_cells.First(i => (i.CellType == ObjectCellType.ResourceGold ||
                           i.CellType == ObjectCellType.ResourceGlass)
                           && !i.Value.Equals(Single.MaxValue)));
                    if (localPath.Count < ValidVerificationStepNumber)
                        path = localPath;
                }
            }
            return path;
        }

        public List<Cell> CheckDwellingCavalry(Cell dwellingCheck, HommSensorData SensorData)
        {
            var path = new List<Cell>();
            if (dwellingCheck.CellType == ObjectCellType.DwellingCavalry)
            {
                if (SensorData.MyTreasury[Resource.Gold] >=
                    UnitsConstants.Current.UnitCost[UnitType.Cavalry][Resource.Gold] &&
                    SensorData.MyTreasury[Resource.Ebony] >=
                    UnitsConstants.Current.UnitCost[UnitType.Cavalry][Resource.Ebony])
                {
                    path = GetMovesStraightToCell(dwellingCheck);
                }
                else
                {
                    //TODO:: check resources near path
                    var localPath = GetMoves(_cells.First(i => (i.CellType == ObjectCellType.ResourceGold ||
                           i.CellType == ObjectCellType.ResourceEbony)
                           && !i.Value.Equals(Single.MaxValue)));
                    if (localPath.Count < ValidVerificationStepNumber)
                        path = localPath;
                }
            }
            return path;
        }

        public List<Cell> CheckDwellingInfantry(Cell dwellingCheck, HommSensorData SensorData)
        {
            var path = new List<Cell>();
            if (dwellingCheck.CellType == ObjectCellType.DwellingInfantry)
            {
                if (SensorData.MyTreasury[Resource.Gold] >=
                    UnitsConstants.Current.UnitCost[UnitType.Infantry][Resource.Gold] &&
                    SensorData.MyTreasury[Resource.Iron] >=
                    UnitsConstants.Current.UnitCost[UnitType.Infantry][Resource.Iron])
                {
                    path = GetMovesStraightToCell(dwellingCheck);
                }
                else
                {
                    //TODO:: check resources near path
                    var localPath = GetMoves(_cells.First(i => (i.CellType == ObjectCellType.ResourceGold ||
                           i.CellType == ObjectCellType.ResourceIron)
                           && !i.Value.Equals(Single.MaxValue)));
                    if (localPath.Count < ValidVerificationStepNumber)
                        path = localPath;
                }
            }
            return path;
        }

        public List<Cell> CheckDwellingMilitia(Cell dwellingCheck, HommSensorData SensorData)
        {
            var path = new List<Cell>();
            var missingTreasury = existTreasuryForDwellingMilitia(dwellingCheck, SensorData);
            if (missingTreasury.Count == 0)
            {
                path = GetMoves(dwellingCheck);
            }  
            else
            {
                //TODO:: check resources near path
                var localPath = findResourcesForDwellingMilitia(missingTreasury);
                if (localPath.Count != 0)
                   path = localPath;
            }

            return path;
        }

	    private Dictionary<Resource, int> existTreasuryForDwellingMilitia(Cell dwellingCheck, HommSensorData SensorData)
	    {
	        if (dwellingCheck.CellType == ObjectCellType.DwellingMilitia)
	        {
	            if (SensorData.MyTreasury[Resource.Gold] >=
	                UnitsConstants.Current.UnitCost[UnitType.Militia][Resource.Gold])
	            {
                    return new Dictionary<Resource, int>();
	            }
	        }
	        var missingResources = new Dictionary<Resource, int>
	        {
	            {
	                Resource.Gold, UnitsConstants.Current.UnitCost[UnitType.Militia][Resource.Gold] -
	                               SensorData.MyTreasury[Resource.Gold]
	            }
	        };
	        return missingResources;
	    }

	    private List<Cell> findResourcesForDwellingMilitia(Dictionary<Resource, int> missingTreasury)
	    {
	        var goldCellList = new List<Cell>();
	        while (missingTreasury[Resource.Gold] >= 0 && 
                _cells.Where(o => (o.CellType == ObjectCellType.ResourceGold)
                && !o.Value.Equals(Single.MaxValue) && !goldCellList.Contains(o)).ToList().Count != 0)
            {
                var localList = findGold(goldCellList);
                foreach (var item in localList)
                {
                    missingTreasury[Resource.Gold] = missingTreasury[Resource.Gold] - item.ResourcesValue;
                }
                goldCellList.AddRange(localList);
            }

            var goldCellPath = new List<Cell>();
            foreach (var goldCell in goldCellList)
            {
                goldCellPath.AddRange(GetMoves(goldCell));
            }
            
            return goldCellPath;
        }

	    private List<Cell> findGold(List<Cell> goldCellPath)
	    {
	        var goldCellList = _cells.Where(o => (o.CellType == ObjectCellType.ResourceGold)
	                           && !o.Value.Equals(Single.MaxValue) && !goldCellPath.Contains(o)).ToList();

            for (int i = 0; i < goldCellList.Count; i++)
	        {
                var cell = goldCellList.ElementAt(i);
                var localPath = GetMoves(cell);
	            if (localPath.Count <= ValidVerificationStepNumber)
	            {
                    goldCellPath.Add(cell);
                    return goldCellPath;
                }
            }
            return path;
        }
        private List<Cell> getNearCells(Cell cell)
        {
            var nearCells = new List<Cell>();

            int[] dx;
            int[] dy;
            //X % 2 = 0
            if (cell.X % 2 == 1)
            {
                dx = new[] { 1, -1, 0, 1, -1, 0 };
                dy = new[] { 0, 0, 1, -1, -1, -1 };
            }
            //X % 2 = 1
            else
            {
                dx = new[] { 1, -1, 0, 1, -1, 0 };
                dy = new[] { 0, 0, 1, 1, 1, -1 };
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
