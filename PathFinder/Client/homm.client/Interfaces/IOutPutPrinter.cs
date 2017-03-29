using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoMM;
using HoMM.ClientClasses;
using HoMM.Engine;
using HommFinder;

namespace Homm.Client.Interfaces
{
	public interface IOutPutPrinter
	{
		void PrintMap(IEnumerable<MapObjectData> objects, Dictionary<UnitType,int> myArmy, int width, int height);
		void PrintPath(IEnumerable<MapObjectData> objects, IEnumerable<Cell> direction, Dictionary<UnitType, int> myArmy, int width, int height);
	}
}
