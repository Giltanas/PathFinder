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

			var availableDwellings = _finder.SearchAvailableDwellings();
			if (availableDwellings.Count != 0)
			{
				//TODO:: write right search of dwellings
				var dwellingCheck = availableDwellings.First(i => i.Value.Equals(availableDwellings.Min(m => m.Value)));
				//var dwellingCheck = availableDwellings.FirstOrDefault(i => i.CellType.SubCellType == SubCellType.DwellingInfantry);
				if (dwellingCheck != null && dwellingCheck.CellType.SubCellType == SubCellType.DwellingCavalry)
				{
					path = _finder.CheckDwelling(dwellingCheck, SensorData.MyTreasury, UnitType.Cavalry, Resource.Ebony);
					if (path.Count != 0)
					{
						move(path);
						SensorData = Client.HireUnits(getAmountOfUnitsToBuy(SubCellType.DwellingCavalry, dwellingCheck));
					}
				}

				if (dwellingCheck != null && dwellingCheck.CellType.SubCellType == SubCellType.DwellingInfantry)
				{
					path = _finder.CheckDwelling(dwellingCheck, SensorData.MyTreasury, UnitType.Infantry, Resource.Iron);
					if (path.Count != 0)
					{
						move(path);
						SensorData = Client.HireUnits(getAmountOfUnitsToBuy(SubCellType.DwellingInfantry, dwellingCheck));
					}
				}

				if (dwellingCheck != null && dwellingCheck.CellType.SubCellType == SubCellType.DwellingMilitia)
				{
					path = _finder.CheckDwellingMilitia(dwellingCheck, SensorData.MyTreasury);
					if (path.Count != 0)
					{
						move(path);
						SensorData = Client.HireUnits(getAmountOfUnitsToBuy(SubCellType.DwellingMilitia, dwellingCheck));
					}
				}

				if (dwellingCheck != null && dwellingCheck.CellType.SubCellType == SubCellType.DwellingRanged)
				{
					path = _finder.CheckDwelling(dwellingCheck, SensorData.MyTreasury, UnitType.Ranged, Resource.Glass);
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
