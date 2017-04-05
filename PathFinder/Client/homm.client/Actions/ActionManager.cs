using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoMM;
using Homm.Client.Helpers;
using HoMM.ClientClasses;
using HommFinder;

namespace Homm.Client.Actions
{
	public class ActionManager
	{
		public HommSensorData SensorData { get; set; }
		public  HommClient Client { get; private set; }
		public List<Cell> Map { get; private set; } 
		public Cell CurrentCell { get; private set; }
		private Finder _finder;
		public MapType MapType { get; private set; }
		public MapObjectData EnemyRespawn { get; private set; }
        const int ValidVerificationStepNumber = 40;

        public ActionManager(HommClient client, HommSensorData sensorData)
		{
			Client = client;
			SensorData = sensorData;
			
			var startCell  = sensorData.Location.CreateCell();

			EnemyRespawn =
				startCell.SameLocation(new Cell(0, 0)) ?
				sensorData.Map.Objects.SingleOrDefault(o => o.Location.X == 13 && o.Location.Y == 13) :
				sensorData.Map.Objects.SingleOrDefault(o => o.Location.X == 0 && o.Location.Y == 0);
			MapType = MapType.Single;

			if (sensorData.Map.Objects.Count < sensorData.Map.Height * sensorData.Map.Width)
			{
				MapType = MapType.DualHard;
			}
			else if (EnemyRespawn.Hero != null)
			{
				MapType = MapType.Dual;
			}
			
			Map = new List<Cell>();		
		}

		//TODO:Need to call this function every day if playing vs player, or you don't see whole map
		public void UpdateMap()
		{
			Map.Clear();
			Map = SensorData.Map.Objects.Select(item => item.ToCell()).ToList();
			CurrentCell = SensorData.Location.CreateCell();

			_finder = new Finder(Map,CurrentCell);
		}
	
		public List<Direction> MoveToCell(Cell cell)
		{
			UpdateMap();
			return Converter.ConvertCellPathToDirection(_finder.GetMovesStraightToCell(cell));
		}

		public List<Direction> MoveToCell(MapObjectData mapObj)
		{
			return MoveToCell(mapObj.ToCell());
		}
		//TODO:: implement 3 methods for different types of map(single, dual, dualHard)
		//TODO: change signature of this method
		public void Play()
		{
			UpdateMap();
			
			var path = new List<Cell>();

			var availableDwellings = SearchAvailableDwellings(_finder._cells);
			if (availableDwellings.Count != 0)
			{
				//TODO:: write right search of dwellings
				var dwellingCheck = availableDwellings.FirstOrDefault(i => i.Value.Equals(availableDwellings.Min(m => m.Value)));
				if (dwellingCheck != null && dwellingCheck.CellType.SubCellType == SubCellType.DwellingCavalry)
				{
					path = CheckDwelling(dwellingCheck, UnitType.Cavalry, Resource.Ebony);
					if (path.Count != 0)
					{
						move(path);
						SensorData = Client.HireUnits(getAmountOfUnitsToBuy(SubCellType.DwellingCavalry, dwellingCheck));
					}
				}

				if (dwellingCheck != null && dwellingCheck.CellType.SubCellType == SubCellType.DwellingInfantry)
				{
					path = CheckDwelling(dwellingCheck, UnitType.Infantry, Resource.Iron);
					if (path.Count != 0)
					{
						move(path);
						SensorData = Client.HireUnits(getAmountOfUnitsToBuy(SubCellType.DwellingInfantry, dwellingCheck));
					}
				}

				if (dwellingCheck != null && dwellingCheck.CellType.SubCellType == SubCellType.DwellingMilitia)
				{
					path = CheckDwelling(dwellingCheck, UnitType.Militia, Resource.Gold);
					if (path.Count != 0)
					{
						move(path);
						SensorData = Client.HireUnits(getAmountOfUnitsToBuy(SubCellType.DwellingMilitia, dwellingCheck));
					}
				}

				if (dwellingCheck != null && dwellingCheck.CellType.SubCellType == SubCellType.DwellingRanged)
				{
					path = CheckDwelling(dwellingCheck, UnitType.Ranged, Resource.Glass);
					if (path.Count != 0)
					{
						move(path);
						SensorData = Client.HireUnits(getAmountOfUnitsToBuy(SubCellType.DwellingRanged, dwellingCheck));
					}
				}

				//TODO: search Resources near path
				//TODO: search Mines near path
			}
		}

        public List<Cell> SearchAvailableDwellings(List<Cell> finderCells)
        {
            return finderCells.Where(i => (i.CellType.MainType == MainCellType.Dwelling)
                           && !i.Value.Equals(Single.MaxValue) && (i.ResourcesValue > 0)).ToList();
        }

        public List<Cell> SearchAvailableResources(List<Cell> finderCells)
        {
            return finderCells.Where(i => (i.CellType.MainType == MainCellType.Resource)
                           && !i.Value.Equals(Single.MaxValue)).ToList();
        }

        public List<Cell> SearchAvailableMines(List<Cell> finderCells)
        {
            return finderCells.Where(i => (i.CellType.MainType == MainCellType.Mine)
                           && !i.Value.Equals(Single.MaxValue)).ToList();
        }

        public List<Cell> CheckDwelling(Cell dwellingCheck, UnitType unitType, Resource resource)
        {
            var path = new List<Cell>();
            var missingTreasury = existTreasuryForDwelling(dwellingCheck, unitType, resource);
            if (missingTreasury.Count == 0)
            {
                path = _finder.GetMovesStraightToCell(dwellingCheck);
            }
            else
            {
                var localPath = findResourcesForDwelling(missingTreasury, dwellingCheck, resource, _finder._cells);
                if (localPath.Count < ValidVerificationStepNumber)
                    path = localPath;
            }
            return path;
        }

        private Dictionary<Resource, int> existTreasuryForDwelling(Cell dwellingCheck, UnitType unitType, Resource resource = new Resource())
        {

            var missingResources = new Dictionary<Resource, int>();

            if (dwellingCheck.CellType.SubCellType == SubCellType.DwellingMilitia)
            {
                if (SensorData.MyTreasury[Resource.Gold] >=
                    UnitsConstants.Current.UnitCost[UnitType.Militia][Resource.Gold])
                {
                    return new Dictionary<Resource, int>();
                }
                missingResources.Add(Resource.Gold,
                    UnitsConstants.Current.UnitCost[UnitType.Militia][Resource.Gold] -
                    SensorData.MyTreasury[Resource.Gold]);
            }
            else
            {
                if (SensorData.MyTreasury[Resource.Gold] >=
                UnitsConstants.Current.UnitCost[unitType][Resource.Gold] &&
                SensorData.MyTreasury[resource] >=
                UnitsConstants.Current.UnitCost[unitType][resource])
                {
                    return new Dictionary<Resource, int>();
                }

                missingResources.Add(Resource.Gold,
                    UnitsConstants.Current.UnitCost[unitType][Resource.Gold] -
                    SensorData.MyTreasury[Resource.Gold]);

                missingResources.Add(resource,
                    UnitsConstants.Current.UnitCost[unitType][resource] -
                    SensorData.MyTreasury[resource]);
            }

            return missingResources;
        }

        private List<Cell> findResourcesForDwelling(Dictionary<Resource, int> missingTreasury, 
            Cell dwelling, Resource resource, List<Cell> finderCells)
        {
            var subCellType = new SubCellType();
            if (resource == Resource.Ebony)
                subCellType = SubCellType.ResourceEbony;
            if (resource == Resource.Iron)
                subCellType = SubCellType.ResourceIron;
            if (resource == Resource.Glass)
                subCellType = SubCellType.ResourceGlass;
            
            var cellList = new List<Cell>();
            while (missingTreasury[Resource.Gold] > 0 && finderCells.Where(o => 
            (o.CellType.SubCellType == SubCellType.ResourceGold || o.ResourcesValue > 0) 
            && !o.Value.Equals(Single.MaxValue) && !cellList.Contains(o)).ToList().Count != 0)
            {
                fillResourceCells(cellList, SubCellType.ResourceGold, finderCells);
                foreach (var item in cellList)
                {
                    missingTreasury[Resource.Gold] = missingTreasury[Resource.Gold] - item.ResourcesValue;
                }
            }

            if (!(resource == Resource.Gold))
            {
                while (missingTreasury[resource] > 0 && finderCells.Where(o => 
                (o.CellType.SubCellType == subCellType || o.ResourcesValue > 0)
                && !o.Value.Equals(Single.MaxValue) && !cellList.Contains(o)).ToList().Count != 0)
                {
                    fillResourceCells(cellList, subCellType, finderCells);
                    foreach (var item in cellList)
                    {
                        missingTreasury[resource] = missingTreasury[resource] - item.ResourcesValue;
                    }
                }
            }

            var cellPath = new List<Cell>();
            if (cellList.Count > 0)
            {
                cellPath.AddRange(_finder.GetSmartPath(SensorData.Location.CreateCell(), cellList[0]));
               
                for (int y = 1; y < cellList.Count; y++)
                {
                    var finderNew = new Finder(finderCells, cellList[y]);
                    cellPath.AddRange(finderNew.GetSmartPath(cellList[y - 1], cellList[y]));
                }
                var finderToEnd = new Finder(finderCells, cellList[cellList.Count - 1]);
                cellPath.AddRange(finderToEnd.GetSmartPath(cellList[cellList.Count - 1], dwelling));
            }
            return cellPath;
        }

        private List<Cell> fillResourceCells(List<Cell> cellPath, SubCellType subCellType, 
            List<Cell> finderCells)
        {
            var сellList = finderCells.Where(o => (o.CellType.SubCellType == subCellType)
                                && !o.Value.Equals(Single.MaxValue) && !cellPath.Contains(o)).ToList();

            for (int i = 0; i < сellList.Count; i++)
            {
                var cell = сellList.ElementAt(i);
                var localPath =_finder.GetMovesStraightToCell(cell);
                if (localPath.Count <= ValidVerificationStepNumber)
                {
                    cellPath.Add(cell);
                    return cellPath;
                }
            }
            return null;
        }

        private int getAmountOfUnitsToBuy(SubCellType subCellType, Cell dwellingCheck)
		{
			//TODO:: add SubCellType check on others Dwelling
			if (subCellType == SubCellType.DwellingMilitia)
			{
			    var amountOfUnitsToBuy = SensorData.MyTreasury[Resource.Gold] / UnitsConstants.Current.UnitCost[UnitType.Militia][Resource.Gold];
			    return dwellingCheck.ResourcesValue >= amountOfUnitsToBuy ? amountOfUnitsToBuy : dwellingCheck.ResourcesValue;
			}

			if (subCellType == SubCellType.DwellingCavalry)
			{
				var maxAmountGold = SensorData.MyTreasury[Resource.Gold] / UnitsConstants.Current.UnitCost[UnitType.Cavalry][Resource.Gold];
				var maxAmountEbony = SensorData.MyTreasury[Resource.Ebony] / UnitsConstants.Current.UnitCost[UnitType.Cavalry][Resource.Ebony];
			    var amountOfUnitsToBuy = maxAmountGold < maxAmountEbony ? maxAmountGold : maxAmountEbony;

				return dwellingCheck.ResourcesValue >= amountOfUnitsToBuy ? amountOfUnitsToBuy : dwellingCheck.ResourcesValue;
			}

			if (subCellType == SubCellType.DwellingInfantry)
			{
				var maxAmountGold = SensorData.MyTreasury[Resource.Gold] / UnitsConstants.Current.UnitCost[UnitType.Infantry][Resource.Gold];
				var maxAmountIron = SensorData.MyTreasury[Resource.Iron] / UnitsConstants.Current.UnitCost[UnitType.Infantry][Resource.Iron];
			    var amountOfUnitsToBuy = maxAmountGold < maxAmountIron ? maxAmountGold : maxAmountIron;

				return dwellingCheck.ResourcesValue >= amountOfUnitsToBuy ? amountOfUnitsToBuy : dwellingCheck.ResourcesValue;

			}

			if (subCellType == SubCellType.DwellingRanged)
			{
				var maxAmountGold = SensorData.MyTreasury[Resource.Gold] / UnitsConstants.Current.UnitCost[UnitType.Ranged][Resource.Gold];
				var maxAmountGlass = SensorData.MyTreasury[Resource.Glass] / UnitsConstants.Current.UnitCost[UnitType.Ranged][Resource.Glass];
			    var amountOfUnitsToBuy = maxAmountGold < maxAmountGlass ? maxAmountGold : maxAmountGlass;

				return dwellingCheck.ResourcesValue >= amountOfUnitsToBuy ? amountOfUnitsToBuy : dwellingCheck.ResourcesValue;

			}

			return 0;
		}

		private void move(List<Cell> path)
		{
			if (path.Count != 0)
			{
				var steps = Converter.ConvertCellPathToDirection(path);
				for (var index = 0; index < steps.Count; index++)
				{
					var step = steps[index];
					//Logic moving interaption
					SensorData = Client.Move(step);
				}
			}
		}
	}

	public enum MapType
	{
		Single,
		Dual,
		//mode without open map
		DualHard
	}
}
