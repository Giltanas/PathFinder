using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoMM;
using Homm.Client.Actions;
using Homm.Client.Helpers;
using Homm.Client.Interfaces;
using HoMM.ClientClasses;
using HommFinder;

namespace Homm.Client.AILogic
{
	public abstract class AiLogic : IAiLogic
	{
		public HommSensorData SensorData { get; set; }
		public HommClient Client { get; set; }
		public List<Cell> Map { get; set; }
		public Cell CurrentCell { get; set; }
		public Finder Finder { get; set; }
		public MapType MapType { get; set; }
		public MapObjectData EnemyRespawn { get; set; }



		//TODO:Need to call this function every day if playing vs player, or you don't see whole map
		public void UpdateMap()
		{
			Map.Clear();
			Map = SensorData.Map.Objects.Select(item => item.ToCell(SensorData.MyArmy)).ToList();
			CurrentCell = SensorData.Location.CreateCell();

			Finder = new Finder(Map, CurrentCell);
		}

		public List<Direction> MoveToCell(Cell cell)
		{
			UpdateMap();
			return Converter.ConvertCellPathToDirection(Finder.GetMovesStraightToCell(cell));
		}

		public List<Direction> MoveToCell(MapObjectData mapObj)
		{
			return MoveToCell(mapObj.ToCell());
		}



		protected void workingWithMines()
		{
			var path = new List<Cell>();

			var availableMines = searchAvailableMines(Finder.Cells);
			foreach (Cell mine in availableMines)
			{
				path.AddRange(Finder.GetSmartPath(SensorData.Location.CreateCell(), mine));
			}

			if (path.Count != 0)
			{
				move(path);
			}
		}

		protected void workingWithDwellings()
		{
			var path = new List<Cell>();
			var dwelling = getAvailableDwelling(Finder.Cells);

			if (dwelling != null && dwelling.CellType.SubCellType == SubCellType.DwellingCavalry)
			{
				path = useDwelling(dwelling, UnitType.Cavalry, Resource.Ebony);
				if (path.Count != 0)
				{
					move(path);
					SensorData = Client.HireUnits(getAmountOfUnitsToBuy(SubCellType.DwellingCavalry, dwelling));
				}
			}

			if (dwelling != null && dwelling.CellType.SubCellType == SubCellType.DwellingInfantry)
			{
				path = useDwelling(dwelling, UnitType.Infantry, Resource.Iron);
				if (path.Count != 0)
				{
					move(path);
					SensorData = Client.HireUnits(getAmountOfUnitsToBuy(SubCellType.DwellingInfantry, dwelling));
				}
			}

			if (dwelling != null && dwelling.CellType.SubCellType == SubCellType.DwellingRanged)
			{
				path = useDwelling(dwelling, UnitType.Ranged, Resource.Glass);
				if (path.Count != 0)
				{
					move(path);
					SensorData = Client.HireUnits(getAmountOfUnitsToBuy(SubCellType.DwellingRanged, dwelling));
				}
			}

			if (dwelling != null && dwelling.CellType.SubCellType == SubCellType.DwellingMilitia)
			{
				path = useDwelling(dwelling, UnitType.Militia, Resource.Gold);
				if (path.Count != 0)
				{
					move(path);
					SensorData = Client.HireUnits(getAmountOfUnitsToBuy(SubCellType.DwellingMilitia, dwelling));
				}
			}
		}

		protected Cell getAvailableDwelling(List<Cell> finderCells)
		{
			var availableDwellings = finderCells.Where(i => (i.CellType.MainType == MainCellType.Dwelling)
						   && !i.Value.Equals(Single.MaxValue) && (i.ResourcesValue > 0)).ToList();

			if (availableDwellings.Count != 0)
				return availableDwellings.FirstOrDefault(i => i.Value.Equals(availableDwellings.Min(m => m.Value))
							&& (i.CellType.SubCellType == SubCellType.DwellingCavalry
								 || i.CellType.SubCellType == SubCellType.DwellingInfantry
								 || i.CellType.SubCellType == SubCellType.DwellingRanged)) ??
							availableDwellings.FirstOrDefault(i => i.Value.Equals(availableDwellings.Min(m => m.Value)));
			return null;
		}

		protected List<Cell> searchAvailableMines(List<Cell> finderCells)
		{
			return finderCells.Where(i => (i.CellType.MainType == MainCellType.Mine)
						   && !i.Value.Equals(Single.MaxValue)).ToList();
		}

		protected List<Cell> useDwelling(Cell dwellingCheck, UnitType unitType, Resource resource)
		{
			var path = new List<Cell>();
			var missingTreasury = existTreasuryForDwelling(dwellingCheck, unitType, resource);
			if (missingTreasury.Count == 0)
			{
				path = Finder.GetMovesStraightToCell(dwellingCheck);
			}
			else
			{
				path = findResourcesForDwelling(missingTreasury, dwellingCheck, resource, Finder.Cells);
			}
			return path;
		}

		protected Dictionary<Resource, int> existTreasuryForDwelling(Cell dwellingCheck, UnitType unitType, Resource resource = new Resource())
		{

			var missingResources = new Dictionary<Resource, int>();

			if (dwellingCheck.CellType.SubCellType == SubCellType.DwellingMilitia)
			{
				if (SensorData.MyTreasury[Resource.Gold] >= UnitsConstants.Current.UnitCost[UnitType.Militia][Resource.Gold])
				{
					return new Dictionary<Resource, int>();
				}
				missingResources.Add(Resource.Gold,
					UnitsConstants.Current.UnitCost[UnitType.Militia][Resource.Gold] - SensorData.MyTreasury[Resource.Gold]);
			}
			else
			{
				if (SensorData.MyTreasury[Resource.Gold] >= UnitsConstants.Current.UnitCost[unitType][Resource.Gold] &&
					SensorData.MyTreasury[resource] >= UnitsConstants.Current.UnitCost[unitType][resource])
				{
					return new Dictionary<Resource, int>();
				}

				missingResources.Add(Resource.Gold,
					UnitsConstants.Current.UnitCost[unitType][Resource.Gold] - SensorData.MyTreasury[Resource.Gold]);

				missingResources.Add(resource,
					UnitsConstants.Current.UnitCost[unitType][resource] - SensorData.MyTreasury[resource]);
			}

			return missingResources;
		}

		protected List<Cell> findResourcesForDwelling(Dictionary<Resource, int> missingTreasury,
			Cell dwelling, Resource resource, List<Cell> finderCells)
		{
			var subCellType = new SubCellType();
			if (resource == Resource.Ebony)
				subCellType = SubCellType.ResourceEbony;
			if (resource == Resource.Iron)
				subCellType = SubCellType.ResourceIron;
			if (resource == Resource.Glass)
				subCellType = SubCellType.ResourceGlass;

			var resultCellsList = new List<Cell>();
			var foundedCells = finderCells.Where(o =>
							 (o.CellType.SubCellType == SubCellType.ResourceGold
							 && o.ResourcesValue > 0) && !o.Value.Equals(Single.MaxValue) &&
							 !resultCellsList.Contains(o)).OrderBy(o => o.Value).ToList();

			for (int i = 0; i < foundedCells.Count && missingTreasury[Resource.Gold] > 0; i++)
			{
				var cell = foundedCells.ElementAt(i);
				resultCellsList.Add(cell);
				missingTreasury[Resource.Gold] = missingTreasury[Resource.Gold] - cell.ResourcesValue;
			}

			if (resource != Resource.Gold)
			{
				foundedCells = finderCells.Where(o =>
						(o.CellType.SubCellType == subCellType || o.ResourcesValue > 0)
						&& !o.Value.Equals(Single.MaxValue) && !resultCellsList.Contains(o)).
						OrderBy(o => o.Value).ToList();

				for (int i = 0; i < foundedCells.Count && missingTreasury[resource] > 0; i++)
				{
					var cell = foundedCells.ElementAt(i);
					resultCellsList.Add(cell);
					missingTreasury[resource] = missingTreasury[resource] - cell.ResourcesValue;
				}
			}

			var cellPath = new List<Cell>();
			if (resultCellsList.Count > 0)
			{
				cellPath.AddRange(Finder.GetSmartPath(SensorData.Location.CreateCell(), resultCellsList[0]));

				for (int y = 1; y < resultCellsList.Count; y++)
				{
					var finderNew = new Finder(finderCells, resultCellsList[y]);
					cellPath.AddRange(finderNew.GetSmartPath(resultCellsList[y - 1], resultCellsList[y]));
				}
				var finderToEnd = new Finder(finderCells, resultCellsList[resultCellsList.Count - 1]);
				cellPath.AddRange(finderToEnd.GetSmartPath(resultCellsList[resultCellsList.Count - 1], dwelling));
			}
			return cellPath;
		}

		protected int getAmountOfUnitsToBuy(SubCellType subCellType, Cell dwellingCheck)
		{
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

		protected void move(List<Cell> path)
		{
			if (path.Count > 0)
			{
				var steps = Converter.ConvertCellPathToDirection(path);
				for (int index = 0; index < steps.Count; index++)
				{

					Direction step = steps[index];
					SensorData = Client.Move(step);
					if (path[index + 1].ContainsEnemyArmy)
					{

						return;
					}
				}
			}
		}

		protected void aiPlayLogic()
		{
			if (canDoSomthing())
			{
				UpdateMap();
				workingWithMines();
				workingWithDwellings();
				figthForResource();
			}
		}

		protected void figthForResource()
		{
			var mines = searchAvailableMines(Finder.Cells);
			var nearestGoldMine = searchAvailableMines(Finder.Cells)
				.Find(c => c.Value.Equals(mines.Min(m => m.Value)) && c.CellType.SubCellType == SubCellType.MineGold);
			if (nearestGoldMine != null)
			{
				move(Finder.GetSmartPath(nearestGoldMine));
			}
			//implement finding other needed mines or resources 
		}

		protected bool canDoSomthing()
		{
			//TODO:: implement logic
			return true;
		}

		public abstract void MakeDecisions();
		public abstract void Act();
	}
}
