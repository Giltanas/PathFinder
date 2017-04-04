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
				var dwellingCheck = availableDwellings.First(i => i.Value.Equals(availableDwellings.Min(m => m.Value)));
				//path = _finder.CheckDwellingCavalry(dwellingCheck, SensorData);

				//if(path.Count == 0)
				//    path = _finder.CheckDwellingInfantry(dwellingCheck, SensorData);

				if (path.Count == 0)
				{
					path = _finder.CheckDwellingMilitia(dwellingCheck, SensorData);
					if (path.Count != 0)
						move(path);
					Client.HireUnits(SensorData.MyTreasury[Resource.Gold]);
				}

				//if (path.Count == 0)
				//    path = _finder.CheckDwellingRanged(dwellingCheck, SensorData);

			}
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

		private void aiLogic()
		{
			
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
