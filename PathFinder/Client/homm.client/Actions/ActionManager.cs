using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoMM;
using Homm.Client.Converter;
using HoMM.ClientClasses;
using HommFinder;

namespace Homm.Client.Actions
{
	public class ActionManager
	{
		public List<Cell> Map { get; private set; }
		public Cell CurrentCell { get; private set; }
		public ActionManager(List<Cell> map)
		{
			Map = map;
		}

		//TODO:Need to call this function every day if playing vs player, or you don't see whole map
		public void UpdateMap(IEnumerable<MapObjectData> listObjects)
		{
			Map.Clear();
			foreach (var item in listObjects)
			{
				Map.Add(item.ConvertMapObjectDataToCell());
			}
		}

		public List<Cell> SearchAvailableDwellings()
		{
			return  Map.Where(i => (i.CellType == ObjectCellType.DwellingCavalry ||
			               i.CellType == ObjectCellType.DwellingRanged ||
			               i.CellType == ObjectCellType.DwellingInfantry ||
			               i.CellType == ObjectCellType.DwellingMilitia) &&
						   i.Value != Single.MaxValue).ToList();

		}

		public List<Cell> SearchAvailableResources()
		{
			return Map.Where(i => (i.CellType == ObjectCellType.ResourceGold ||
						   i.CellType == ObjectCellType.ResourceEbony ||
						   i.CellType == ObjectCellType.ResourceGlass ||
						   i.CellType == ObjectCellType.ResourceIron) &&
						   i.Value != Single.MaxValue).ToList();
		}

		public List<Direction> MoveToCell(Cell cell)
		{
			var finder = new Finder(Map);
			return ConverterExtensions.ConvertCellPathToDirection(finder.GetMoves(CurrentCell, cell)) 
				as List<Direction>;
		}

		public List<Direction> MoveToCell(MapObjectData mapObj)
		{
			return MoveToCell(mapObj.ConvertMapObjectDataToCell());
		}
	}
}
