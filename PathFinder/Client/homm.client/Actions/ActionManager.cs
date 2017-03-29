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
		//TODO:: REMOVE THIS METHOD AFTER TEST
		public List<Cell> SearchAvailableDwellings()
		{
			return _finder.SearchAvailableDwellings();
		}
		//TODO:: REMOVE THIS METHOD AFTER TEST
		public List<Cell> SearchAvailableResources()
		{
			return _finder.SearchAvailableResources();
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
	}
}
