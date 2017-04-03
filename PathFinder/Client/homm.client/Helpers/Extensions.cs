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
			int resourcesValue = 0;
			if (mapObjectData.Dwelling != null)
				resourcesValue = mapObjectData.Dwelling.AvailableToBuyCount;
			
			if (mapObjectData.ResourcePile != null)
				resourcesValue = mapObjectData.ResourcePile.Amount;
			
			return new Cell(x, y, mapObjectData.GetTerrainCellType(myArmy), mapObjectData.GetObjectCellType(), resourcesValue);
		}

		public static Cell CreateCell(this LocationInfo location)
		{
			return new Cell(location.X, location.Y);
		}
		public static ObjectCellType GetObjectCellType(this MapObjectData mapObjectData)
		{
			var oct = new ObjectCellType();
			if (mapObjectData.Dwelling != null)
			{
				oct.MainType = MainCellType.Dwelling;
				switch (mapObjectData.Dwelling.UnitType)
				{
					case UnitType.Ranged:
						oct.SubCellType = SubCellType.DwellingRanged;
						break;
					case UnitType.Infantry:
						oct.SubCellType = SubCellType.DwellingInfantry;
						break;
					case UnitType.Militia:
						oct.SubCellType = SubCellType.DwellingMilitia;
						break;
					case UnitType.Cavalry:
						oct.SubCellType = SubCellType.DwellingCavalry;
						break;
				}
			}

			if (mapObjectData.Mine != null)
			{
				oct.MainType = MainCellType.Mine;
				switch (mapObjectData.Mine.Resource)
				{
					case Resource.Ebony:
						oct.SubCellType =  SubCellType.MineEbony;
						break;
				   case Resource.Glass:
						oct.SubCellType = SubCellType.MineGlass;
						break;
					case Resource.Gold:
						oct.SubCellType = SubCellType.MineGold;
						break;
					case Resource.Iron:
						oct.SubCellType = SubCellType.MineIron;
						break;
				}
			}

			if (mapObjectData.ResourcePile != null)
			{
				oct.MainType = MainCellType.Resource;
				switch (mapObjectData.ResourcePile.Resource)
				{
					case Resource.Ebony:
<<<<<<< HEAD
						oct.SubCellType = SubCellType.ResourceEbony;
						break;
					case Resource.Glass:
						oct.SubCellType = SubCellType.ResourceGlass;
						break;
					case Resource.Gold:
						oct.SubCellType = SubCellType.ResourceGold;
						break;
					case Resource.Iron:
						oct.SubCellType = SubCellType.ResourceIron;
						break;
				}
=======
                        oct.SubCellType = SubCellType.ResourceEbony;
                        break;
                    case Resource.Glass:
                        oct.SubCellType = SubCellType.ResourceGlass;
                        break;
                    case Resource.Gold:
                        oct.SubCellType = SubCellType.ResourceGold;
                        break;
                    case Resource.Iron:
                        oct.SubCellType = SubCellType.ResourceIron;
                        break;
                }
>>>>>>> refs/remotes/origin/master
			}

			return oct;
		}

		public static TerrainCellType GetTerrainCellType(this MapObjectData mapObjectData, Dictionary<UnitType, int> myArmy = null)
		{
			if (mapObjectData.Location.X == 4 && mapObjectData.Location.Y == 5)
			{
				var a = 1;
			}
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
