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
		public HommSensorData SensorData { get; private set; }
		public List<Cell> Map { get; private set; } 
		public Cell CurrentCell { get; private set; }
		private Finder _finder;

		public ActionManager(HommSensorData sensorData)
		{
			SensorData = sensorData;
			Map = new List<Cell>();
			UpdateMap();			
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
			return Converter.ConvertCellPathToDirection(_finder.GetMoves(cell)) 
				as List<Direction>;
		}

		public List<Direction> MoveToCell(MapObjectData mapObj)
		{
			return MoveToCell(mapObj.ToCell());
		}

		//TODO: change signature of this method
		public Stack<Cell> Play()
		{
			var path = new Stack<Cell>();
			var availableMines = _finder.SearchAvailableMines();
			if (availableMines.Count != 0)
			{
				path = _finder.GetMoves(availableMines.First(i => (Math.Abs(i.Value - availableMines.Max(m=>m.Value)) < 100)));
				if (path.Count != 0)
				{
					return path;
				}
				//TODO: search Resources near path
				//TODO: search Dwellings near path			
			}

			var availableResources = _finder.SearchAvailableResources();
			if (availableResources.Count != 0)
			{
				path = _finder.GetMoves(availableResources.First(i => (Math.Abs(i.Value - availableResources.Max(m => m.Value)) < 100)));
				if (path.Count != 0)
				{
					return path;
				}
				//TODO: search Mines near path
				//TODO: search Dwellings near path
			}

			var availableDwellings = _finder.SearchAvailableDwellings();
			if (availableDwellings.Count != 0)
			{
				path = _finder.GetMoves(availableDwellings.First(i => (Math.Abs(i.Value - availableDwellings.Max(m => m.Value)) < 100)));
				if (path.Count != 0)
				{
					return path;
				}
				//TODO: search Resources near path
				//TODO: search Mines near path
			}

			return path;
		}
	}
}
