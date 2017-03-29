using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoMM;
using Homm.Client.Helpers;
using Homm.Client.Interfaces;
using HoMM.ClientClasses;
using HoMM.Engine;
using HommFinder;

namespace Homm.Client.Output
{
	public class CmdOutPutPrinter : IOutPutPrinter
	{
		public void PrintMap(IEnumerable<MapObjectData> objects, Dictionary<UnitType,int> myArmy, int width = 14, int height = 14)
		{
			
			string[,] array = new string[width, height];
			foreach (var item in objects)
			{
				var x = item.Location.X;
				var y = item.Location.Y;
				array[x, y] = item.ToDataForPrint(myArmy);
			}
			for(int i=0;i<height;i++)
			{
				for (int j = 0; j < width; j++)
				{
					if (array[j, i] != null)
					{
						Console.ForegroundColor = array[j, i].Equals("#") ? ConsoleColor.Red : ConsoleColor.Green;
						Console.Write(array[j, i]);
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.White;
						Console.Write("█");
					}
				}
				Console.WriteLine();
			}
		}

		public void PrintPath(IEnumerable<MapObjectData> objects, IEnumerable<Cell> direction, Dictionary<UnitType, int> myArmy, int width = 14, int height = 14)
		{
			Console.WriteLine();
			string[,] array = new string[width+1, height+1];
		

			foreach (var item in objects)
			{
				var x = item.Location.X;
				var y = item.Location.Y;

				array[x, y] = item.ToDataForPrint(myArmy);
			}
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					if (array[j, i] != null)
					{
						Console.ForegroundColor = array[j, i].Equals("#") ? ConsoleColor.Red : ConsoleColor.Green;
						if (direction.Any(c => c.X == j && c.Y == i))
						{
							Console.ForegroundColor = ConsoleColor.Blue;
						}
						Console.Write(array[j, i]);
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.White;
						Console.Write("█");
					}
				}
				Console.WriteLine();
			}
		}

	}
}
