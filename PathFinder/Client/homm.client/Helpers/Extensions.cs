using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoMM;
using HoMM.ClientClasses;
using HommFinder;

namespace Homm.Client.Helpers
{
	public static class Extensions
	{
		public static Cell ToCell(this MapObjectData mapObjectData, Dictionary<UnitType, int> myArmy = null)
		{
			var x = mapObjectData.Location.X;
			var y = mapObjectData.Location.Y;
			if (x % 2 == 1)
			{
				y += 1;
			}
			return new Cell(x, y, mapObjectData.GetTerrainCellType(myArmy), mapObjectData.GetObjectCellType());
		}

		public static ObjectCellType GetObjectCellType(this MapObjectData mapObjectData)
		{
			if (mapObjectData.Dwelling != null)
			{
				switch (mapObjectData.Dwelling.UnitType)
				{
					case UnitType.Ranged:
						return ObjectCellType.DwellingRanged;
					case UnitType.Infantry:
						return ObjectCellType.DwellingInfantry;
					case UnitType.Militia:
						return ObjectCellType.DwellingMilitia;
					case UnitType.Cavalry:
						return ObjectCellType.DwellingCavalry;
				}
			}

			if (mapObjectData.Mine != null)
			{
				switch (mapObjectData.Mine.Resource)
				{
					case Resource.Ebony:
						return ObjectCellType.MineEbony;
					case Resource.Glass:
						return ObjectCellType.MineGlass;
					case Resource.Gold:
						return ObjectCellType.MineGold;
					case Resource.Iron:
						return ObjectCellType.MineIron;
				}
			}

			if (mapObjectData.ResourcePile != null)
			{
				switch (mapObjectData.ResourcePile.Resource)
				{
					case Resource.Ebony:
						return ObjectCellType.ResourceEbony;
					case Resource.Glass:
						return ObjectCellType.ResourceGlass;
					case Resource.Gold:
						return ObjectCellType.ResourceGold;
					case Resource.Iron:
						return ObjectCellType.ResourceIron;
				}
			}

			return ObjectCellType.None;
		}

		public static TerrainCellType GetTerrainCellType(this MapObjectData mapObjectData, Dictionary<UnitType, int> myArmy = null)
		{
			if (mapObjectData.Wall != null)
			{
				return TerrainCellType.Block;
			}
			if (mapObjectData.NeutralArmy != null)
			{
				if (myArmy==null || Combat.Resolve(new ArmiesPair(myArmy, mapObjectData.NeutralArmy.Army)).IsDefenderWin)
				{
					return TerrainCellType.Block;
				}
			}
			if (mapObjectData.Mine != null)
			{
				return TerrainCellType.Block;
			}
			switch (mapObjectData.Terrain)
			{
				case Terrain.Road:
					return TerrainCellType.Road;
				case Terrain.Grass:
					return TerrainCellType.Grass;

				case Terrain.Snow:
					return TerrainCellType.Snow;

				case Terrain.Marsh:
					return TerrainCellType.Marsh;

				//value of desert mb equals value of snow`s 
				case Terrain.Desert:
					return TerrainCellType.Snow;

			}
			return TerrainCellType.Block;
		}
		public static string ToDataForPrint(this MapObjectData mapObject, Dictionary<UnitType, int> armyA = null)
		{
			return mapObject.GetTerrainCellType(armyA).ToDataForPrint();
		}

		public static string ToDataForPrint(this TerrainCellType cellType)
		{
			switch (cellType)
			{
				case TerrainCellType.Road:
					return "1";
				case TerrainCellType.Block:
					return "#";
				case TerrainCellType.Grass:
					return "2";
				case TerrainCellType.Marsh:
					return "3";
				case TerrainCellType.Snow:
					return "4";
			}
			return string.Empty;
		}
	}
}
