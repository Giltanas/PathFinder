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

        public List<Cell> checkDwellingRanged(Cell dwellingCheck, HommSensorData SensorData)
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
            }
            return path;
        }

        public List<Cell> checkDwellingCavalry(Cell dwellingCheck, HommSensorData SensorData)
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
            }
            return path;
        }

        public List<Cell> checkDwellingInfantry(Cell dwellingCheck, HommSensorData SensorData)
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
            }
            return path;
        }

        public List<Cell> checkDwellingMilitia(Cell dwellingCheck, HommSensorData SensorData)
        {
            var path = new List<Cell>();
            if (dwellingCheck.CellType == ObjectCellType.DwellingMilitia)
            {
                if (SensorData.MyTreasury[Resource.Gold] >=
                    UnitsConstants.Current.UnitCost[UnitType.Militia][Resource.Gold])
                {
                    path = GetMovesStraightToCell(dwellingCheck);
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
